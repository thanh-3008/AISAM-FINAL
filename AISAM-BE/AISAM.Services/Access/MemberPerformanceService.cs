using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AISAM.Services.Access;

public sealed record MemberPerformanceRow(Guid MemberId, string Name, int ContentsCreated,
    int CreatorPublishedPosts, int PublisherPublishedPosts, int ReviewedSubmissions, decimal? ApprovalRate,
    int CompletedSchedules, int PendingSchedules, int FailedSchedules, decimal? OnTimeRate,
    decimal? FailedPublishRate, decimal? TurnaroundHours, int PostsWithInsights,
    long? Engagement, long? Impressions, long? Reach, decimal? EngagementRate, DateTime? InsightsUpdatedAt,
    IReadOnlyList<string> TeamRoles);
public sealed record PerformanceOption(Guid Id, string Name);
public sealed record MemberPerformanceResult(IReadOnlyList<MemberPerformanceRow> Items, int Total,
    DateTime From, DateTime To, DateTime UpdatedAt, int? UnattributedContents,
    IReadOnlyList<PerformanceOption> Brands, IReadOnlyList<PerformanceOption> Teams,
    IReadOnlyList<PerformanceOption> Members, IReadOnlyDictionary<string,string> MetricDefinitions,
    bool CanViewAllTeams, bool TeamSelectionRequired);

public sealed class MemberPerformanceService(AisamContext db, IAccessControlService access)
{
    public static readonly IReadOnlyDictionary<string,string> Definitions = new Dictionary<string,string> {
        ["period"]="UTC [from,to). Content: CreatedAt; publish: PublishedAt; review: decision time; schedule: ScheduledAt. Maximum 366 days.",
        ["attribution"]="Creator = PrimaryCreatorId; reviewer = ApproverUserId; scheduler = ScheduledByUserId; publisher = PublishedByUserId (PublishOperation ActorId is the legacy fallback). A successful destination counts once by IntegrationId + ExternalPostId; missing external ID uses Post.Id.",
        ["approvalRate"]="Approved / (approved + rejected) recorded submission decisions in period. Drafts excluded. Legacy decisions without submission timestamp excluded.",
        ["onTimeRate"]="Completed non-recurring schedules created by the member and executed within ±5 minutes of ScheduledAt / completed schedules with timestamps. Pending/failed shown separately. Recurring schedules excluded because occurrence history is not retained.",
        ["failedPublishRate"]="Failed non-recurring schedules / (completed + failed). Current final outcome per schedule, not provider attempt failure rate; retries are not counted repeatedly. Immediate publish failures are not durably recorded and are excluded.",
        ["turnaroundHours"]="Mean hours from submission to decision for submissions reviewed by the member in period; missing/invalid timestamps excluded.",
        ["engagement"]="Latest cumulative snapshot per destination post published in period, as of report generation; not incremental engagement earned during the selected dates. Stored provider engagement may mean engaged users or reactions + comments + shares.",
        ["engagementRate"]="100 × summed engagement / summed impressions on posts with snapshots. Reach is separate and is summed per post, not unique people across posts. No reports or zero denominator = null.",
        ["scope"]="Owner and Workspace Manager can view all active Teams. Team Manager must select a Team they actively manage and can only view members and resources in that Team. Team means content's recorded TeamId, not inferred historical membership. Workspace management alone receives unattributed content count."
    };
    public static decimal? Rate(long numerator,long denominator) => denominator==0?null:Math.Round(100m*numerator/denominator,2);
    private static bool HasInsight(PerformanceReport report,string metric)
    {
        // Legacy snapshots without raw JSON retain their stored values. A click
        // tracking-only row is not evidence of a provider insights sync.
        if(string.IsNullOrWhiteSpace(report.RawData)) return true;
        try {
            using var json=JsonDocument.Parse(report.RawData);
            var names=metric=="engagement"?new[]{"engaged_users","reactions","comments","shares"}:new[]{metric};
            return names.Any(name=>json.RootElement.TryGetProperty(name,out var value) && value.ValueKind==JsonValueKind.Number);
        } catch(JsonException) {return false;}
    }

    public async Task<MemberPerformanceResult> GetAsync(Guid actor,Guid workspace,DateTime from,DateTime to,
        Guid? brandId=null,Guid? teamId=null,Guid? memberId=null,int page=1,int pageSize=20,CancellationToken ct=default)
    {
        if(from.Kind!=DateTimeKind.Utc || to.Kind!=DateTimeKind.Utc || from>=to || to-from>TimeSpan.FromDays(366) || page<1 || page>100000 || pageSize<1 || pageSize>100)
            throw new ArgumentException("Use a UTC interval from < to, at most 366 days; pageSize 1–100.");
        var membership=await db.WorkspaceMembers.IgnoreQueryFilters().AsNoTracking().Include(m=>m.Workspace)
            .SingleOrDefaultAsync(m=>m.WorkspaceId==workspace && m.UserId==actor && m.IsActive,ct);
        if(membership is null || membership.Workspace.Status==WorkspaceStatusEnum.Deleted ||
           !await db.Users.AsNoTracking().AnyAsync(u=>u.Id==actor && u.IsActive,ct)) throw new PerformanceAccessException(403);
        var v2=access is RbacV2AccessAdapter;
        var context=v2?await new RbacV2AccessResolver(db).ContextAsync(actor,workspace,ct):null;
        var owner=v2 ? context?.WorkspaceRole is "Owner" or "WorkspaceManager" : membership.Role==WorkspaceMemberRoleEnum.Owner;
        if(v2 ? context is null || !owner && !context.Scopes.Any(s=>s.Role=="Manager") : !owner && membership.Role!=WorkspaceMemberRoleEnum.Manager) throw new PerformanceAccessException(403);
        var brandIds=(await access.GetAccessibleBrandIdsAsync(actor,workspace,ct)).ToArray();
        if(v2 && owner) brandIds=await db.Brands.IgnoreQueryFilters().Where(b=>b.WorkspaceId==workspace && !b.IsDeleted).Select(b=>b.Id).ToArrayAsync(ct);
        if(v2 && !owner) brandIds=context!.Scopes.Where(s=>s.Role=="Manager").Select(s=>s.BrandId).Distinct().ToArray();
        var teamsQuery=db.Teams.IgnoreQueryFilters().AsNoTracking().Where(t=>t.WorkspaceId==workspace && !t.IsDeleted && t.Status==TeamStatusEnum.Active);
        if(!owner) teamsQuery=teamsQuery.Where(t=>db.TeamMembers.IgnoreQueryFilters().Any(m=>m.TeamId==t.Id && m.UserId==actor && m.IsActive && m.Role==TeamRoleEnum.Manager));
        var teams=await teamsQuery.OrderBy(t=>t.Name).Select(t=>new PerformanceOption(t.Id,t.Name)).ToListAsync(ct);
        if(!owner && teams.Count==0) throw new PerformanceAccessException(403);
        if(teamId.HasValue && !teams.Any(t=>t.Id==teamId)) throw new PerformanceAccessException(404);

        if(teamId.HasValue)
        {
            var teamBrandIds=await db.TeamBrands.IgnoreQueryFilters().AsNoTracking()
                .Where(tb=>tb.TeamId==teamId && tb.IsActive).Select(tb=>tb.BrandId).ToArrayAsync(ct);
            brandIds=brandIds.Intersect(teamBrandIds).ToArray();
        }
        if(brandId.HasValue && !brandIds.Contains(brandId.Value)) throw new PerformanceAccessException(404);
        var brands=await db.Brands.IgnoreQueryFilters().AsNoTracking().Where(b=>brandIds.Contains(b.Id) && !b.IsDeleted)
            .OrderBy(b=>b.Name).Select(b=>new PerformanceOption(b.Id,b.Name)).ToListAsync(ct);
        if(brandId.HasValue) brandIds=[brandId.Value];

        if(!owner && !teamId.HasValue)
            return new([],0,from,to,DateTime.UtcNow,null,brands,teams,[],Definitions,false,true);

        var membersQuery=db.WorkspaceMembers.IgnoreQueryFilters().AsNoTracking().Where(m=>m.WorkspaceId==workspace && m.IsActive);
        if(teamId.HasValue) membersQuery=membersQuery.Where(m=>db.TeamMembers.IgnoreQueryFilters().Any(tm=>tm.TeamId==teamId && tm.UserId==m.UserId && tm.IsActive));
        var permittedMembers=await membersQuery.OrderBy(m=>m.User.FullName).ThenBy(m=>m.UserId)
            .Select(m=>new PerformanceOption(m.UserId,m.User.FullName??m.User.Email)).ToListAsync(ct);
        if(memberId.HasValue && !permittedMembers.Any(m=>m.Id==memberId)) throw new PerformanceAccessException(404);
        var selected=permittedMembers.Where(m=>!memberId.HasValue || m.Id==memberId).OrderBy(m=>m.Name).ThenBy(m=>m.Id).ToList();
        var roleTeamIds=teamId.HasValue?[teamId.Value]:teams.Select(t=>t.Id).ToArray();
        var roleRows=await db.TeamMembers.IgnoreQueryFilters().AsNoTracking()
            .Where(tm=>roleTeamIds.Contains(tm.TeamId) && tm.IsActive)
            .Select(tm=>new {tm.UserId,tm.Role}).ToListAsync(ct);
        var rolesByMember=roleRows.GroupBy(row=>row.UserId).ToDictionary(group=>group.Key,
            group=>(IReadOnlyList<string>)group.Select(row=>row.Role.ToString()).Distinct().Order().ToArray());
        var rows=new List<MemberPerformanceRow>();
        foreach(var member in selected.Skip((page-1)*pageSize).Take(pageSize))
        {
            // Resolve member/Brand pairs independently: shared team in Alpha must
            // never grant a Manager this same member's data in Beta.
            var memberBrands=new List<Guid>();
            if(v2) memberBrands.AddRange(brandIds);
            foreach(var brand in brandIds)
                if(!v2 && (await access.CheckAsync(new(actor,workspace,AccessResourceKind.Brand,brand,ResourcePermission.AnalyticsMember,MemberId:member.Id,TeamId:teamId),ct)).Allowed) memberBrands.Add(brand);
            var contents=db.Contents.IgnoreQueryFilters().AsNoTracking().Where(c=>c.WorkspaceId==workspace && !c.IsDeleted && memberBrands.Contains(c.BrandId) && (!teamId.HasValue || c.TeamId==teamId));
            if(v2 && !owner) contents=contents.Where(c=>c.TeamId.HasValue &&
                db.Teams.IgnoreQueryFilters().Any(t=>t.Id==c.TeamId && t.WorkspaceId==workspace && !t.IsDeleted && t.Status==TeamStatusEnum.Active) &&
                db.TeamBrands.IgnoreQueryFilters().Any(tb=>tb.TeamId==c.TeamId && tb.BrandId==c.BrandId && tb.IsActive) &&
                db.TeamMembers.IgnoreQueryFilters().Any(tm=>tm.TeamId==c.TeamId && tm.UserId==member.Id && tm.IsActive) &&
                db.TeamMembers.IgnoreQueryFilters().Any(tm=>tm.TeamId==c.TeamId && tm.UserId==actor && tm.IsActive && tm.Role==TeamRoleEnum.Manager));
            var own=contents.Where(c=>c.PrimaryCreatorId==member.Id);
            var created=await own.CountAsync(c=>c.CreatedAt>=from && c.CreatedAt<to,ct);
            var postsQuery=from p in db.Posts.IgnoreQueryFilters().AsNoTracking()
                join c in contents on p.ContentId equals c.Id
                join i in db.SocialIntegrations.IgnoreQueryFilters().AsNoTracking() on p.IntegrationId equals i.Id
                let operationActor=db.PublishOperations.IgnoreQueryFilters().AsNoTracking()
                    .Where(o=>o.WorkspaceId==workspace && o.ContentId==p.ContentId && o.IntegrationId==p.IntegrationId && o.Status=="Published" &&
                        ((p.ExternalPostId!=null && o.ProviderId==p.ExternalPostId) || (p.SnapshotId!=null && o.SnapshotId==p.SnapshotId)))
                    .OrderByDescending(o=>o.UpdatedAt).Select(o=>(Guid?)o.ActorId).FirstOrDefault()
                where !p.IsDeleted && !i.IsDeleted && i.WorkspaceId==workspace && i.BrandId==c.BrandId && p.Status==ContentStatusEnum.Published && p.PublishedAt>=@from && p.PublishedAt<to
                    && (c.PrimaryCreatorId==member.Id || p.PublishedByUserId==member.Id || operationActor==member.Id)
                    && (!v2 || owner || db.TeamChannelAccesses.IgnoreQueryFilters().Any(g=>g.IntegrationId==p.IntegrationId && g.ScopeEnabledV2 &&
                        db.TeamBrands.IgnoreQueryFilters().Any(tb=>tb.Id==g.TeamBrandId && tb.TeamId==c.TeamId && tb.BrandId==c.BrandId && tb.IsActive)))
                select new {Post=p,Creator=c.PrimaryCreatorId,OperationActor=operationActor};
            var allPosts=await postsQuery.ToListAsync(ct);
            var posts=allPosts.GroupBy(p=>(p.Post.IntegrationId,Key:string.IsNullOrEmpty(p.Post.ExternalPostId)?p.Post.Id.ToString():p.Post.ExternalPostId))
                .Select(g=>g.OrderBy(p=>p.Post.PublishedAt).ThenBy(p=>p.Post.Id).First()).ToList();
            var creatorPosts=posts.Where(p=>p.Creator==member.Id).Select(p=>p.Post.Id).ToArray();
            var creatorPostIds=allPosts.Where(p=>p.Creator==member.Id).Select(p=>p.Post.Id).ToArray();
            var destinationByPost=allPosts.ToDictionary(p=>p.Post.Id,p=>(p.Post.IntegrationId,Key:string.IsNullOrEmpty(p.Post.ExternalPostId)?p.Post.Id.ToString():p.Post.ExternalPostId));
            var reports=await db.PerformanceReports.IgnoreQueryFilters().AsNoTracking().Where(r=>!r.IsDeleted && r.PostId.HasValue && creatorPostIds.Contains(r.PostId.Value) && r.AdId==null)
                .ToListAsync(ct);
            var latest=reports.Where(r=>HasInsight(r,"engagement") || HasInsight(r,"impressions") || HasInsight(r,"reach")).GroupBy(r=>destinationByPost[r.PostId!.Value]).Select(g=>g.OrderByDescending(r=>r.ReportDate).ThenByDescending(r=>r.CreatedAt).ThenBy(r=>r.Id).First()).ToList();
            var engagementKnown=latest.Count>0 && latest.All(r=>HasInsight(r,"engagement"));
            var impressionsKnown=latest.Count>0 && latest.All(r=>HasInsight(r,"impressions"));
            var reachKnown=latest.Count>0 && latest.All(r=>HasInsight(r,"reach"));
            var creatorReviews=await db.Approvals.IgnoreQueryFilters().AsNoTracking().Where(a=>!a.IsDeleted && own.Any(c=>c.Id==a.ContentId) && a.SubmittedAt.HasValue &&
                (a.Status==ContentStatusEnum.Approved || a.Status==ContentStatusEnum.Rejected) && (a.ApprovedAt??a.CreatedAt)>=from && (a.ApprovedAt??a.CreatedAt)<to).ToListAsync(ct);
            var creatorDecisions=creatorReviews.GroupBy(a=>(a.ContentId,a.SubmittedAt))
                .Select(g=>g.OrderByDescending(a=>a.ApprovedAt??a.CreatedAt).ThenBy(a=>a.Id).First()).ToList();
            var reviewerReviews=await db.Approvals.IgnoreQueryFilters().AsNoTracking().Where(a=>!a.IsDeleted && a.ApproverUserId==member.Id &&
                contents.Any(c=>c.Id==a.ContentId) && a.SubmittedAt.HasValue &&
                (a.Status==ContentStatusEnum.Approved || a.Status==ContentStatusEnum.Rejected) && (a.ApprovedAt??a.CreatedAt)>=from && (a.ApprovedAt??a.CreatedAt)<to).ToListAsync(ct);
            var reviewerDecisions=reviewerReviews.GroupBy(a=>(a.ContentId,a.SubmittedAt))
                .Select(g=>g.OrderByDescending(a=>a.ApprovedAt??a.CreatedAt).ThenBy(a=>a.Id).First()).ToList();
            var turnaround=reviewerDecisions.Where(a=>(a.ApprovedAt??a.CreatedAt)>=a.SubmittedAt)
                .Select(a=>((a.ApprovedAt??a.CreatedAt)-a.SubmittedAt!.Value).TotalHours).ToList();
            var schedules=await db.ContentCalendars.IgnoreQueryFilters().AsNoTracking().Where(s=>s.WorkspaceId==workspace && !s.IsDeleted && s.RepeatType==RepeatTypeEnum.None &&
                s.ScheduledByUserId==member.Id && s.ScheduledAt>=from && s.ScheduledAt<to && contents.Any(c=>c.Id==s.ContentId) && s.IntegrationId.HasValue &&
                db.SocialIntegrations.IgnoreQueryFilters().Any(i=>i.Id==s.IntegrationId && !i.IsDeleted && i.WorkspaceId==workspace &&
                    contents.Any(c=>c.Id==s.ContentId && c.BrandId==i.BrandId))).ToListAsync(ct);
            var completed=schedules.Where(s=>s.Status==ScheduleStatusEnum.Completed).ToList();
            var timed=completed.Where(s=>s.ExecutedAt.HasValue).ToList();
            var failed=schedules.Count(s=>s.Status==ScheduleStatusEnum.Failed);
            rows.Add(new(member.Id,member.Name,created,creatorPosts.Length,posts.Count(p=>(p.Post.PublishedByUserId??p.OperationActor)==member.Id),reviewerDecisions.Count,
                Rate(creatorDecisions.Count(a=>a.Status==ContentStatusEnum.Approved),creatorDecisions.Count),completed.Count,
                schedules.Count(s=>s.Status is ScheduleStatusEnum.Pending or ScheduleStatusEnum.Processing),failed,
                Rate(timed.Count(s=>Math.Abs((s.ExecutedAt!.Value-s.ScheduledAt!.Value).TotalMinutes)<=5),timed.Count),Rate(failed,failed+completed.Count),
                turnaround.Count==0?null:Math.Round((decimal)turnaround.Average(),2),latest.Count,
                engagementKnown?latest.Sum(r=>r.Engagement):null,impressionsKnown?latest.Sum(r=>r.Impressions):null,reachKnown?latest.Sum(r=>r.Reach):null,
                engagementKnown&&impressionsKnown?Rate(latest.Sum(r=>r.Engagement),latest.Sum(r=>r.Impressions)):null,latest.Count==0?null:latest.Max(r=>r.CreatedAt),
                rolesByMember.GetValueOrDefault(member.Id,[])));
        }
        int? unattributed=owner?await db.Contents.IgnoreQueryFilters().CountAsync(c=>c.WorkspaceId==workspace && !c.IsDeleted && brandIds.Contains(c.BrandId) &&
            (!teamId.HasValue || c.TeamId==teamId) && c.PrimaryCreatorId==null && c.CreatedAt>=from && c.CreatedAt<to,ct):null;
        return new(rows,selected.Count,from,to,DateTime.UtcNow,unattributed,brands,teams,permittedMembers,Definitions,owner,false);
    }
}
public sealed class PerformanceAccessException(int statusCode):Exception("Member performance is outside your access scope.") { public int StatusCode {get;}=statusCode; }
