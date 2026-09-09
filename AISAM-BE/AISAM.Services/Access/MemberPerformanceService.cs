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
    long? Engagement, long? Impressions, long? Reach, decimal? EngagementRate, DateTime? InsightsUpdatedAt);
public sealed record PerformanceOption(Guid Id, string Name);
public sealed record MemberPerformanceResult(IReadOnlyList<MemberPerformanceRow> Items, int Total,
    DateTime From, DateTime To, DateTime UpdatedAt, int? UnattributedContents,
    IReadOnlyList<PerformanceOption> Brands, IReadOnlyList<PerformanceOption> Teams,
    IReadOnlyList<PerformanceOption> Members, IReadOnlyDictionary<string,string> MetricDefinitions);

public sealed class MemberPerformanceService(AisamContext db, IAccessControlService access)
{
    public static readonly IReadOnlyDictionary<string,string> Definitions = new Dictionary<string,string> {
        ["period"]="UTC [from,to). Content: CreatedAt; publish: PublishedAt; review: decision time; schedule: ScheduledAt. Maximum 366 days.",
        ["attribution"]="Creator = PrimaryCreatorId; Publisher = PublishedByUserId. A successful destination counts once by IntegrationId + ExternalPostId; missing external ID uses Post.Id.",
        ["approvalRate"]="Approved / (approved + rejected) recorded submission decisions in period. Drafts excluded. Legacy decisions without submission timestamp excluded.",
        ["onTimeRate"]="Completed non-recurring schedules executed within ±5 minutes of ScheduledAt / completed schedules with timestamps. Pending/failed shown separately. Recurring schedules excluded because occurrence history is not retained.",
        ["failedPublishRate"]="Failed non-recurring schedules / (completed + failed). Current final outcome per schedule, not provider attempt failure rate; retries are not counted repeatedly. Immediate publish failures are not durably recorded and are excluded.",
        ["turnaroundHours"]="Mean hours from submission to decision for recorded submissions decided in period; missing/invalid timestamps excluded.",
        ["engagement"]="Latest cumulative snapshot per destination post published in period, as of report generation; not incremental engagement earned during the selected dates. Stored provider engagement may mean engaged users or reactions + comments + shares.",
        ["engagementRate"]="100 × summed engagement / summed impressions on posts with snapshots. Reach is separate and is summed per post, not unique people across posts. No reports or zero denominator = null.",
        ["scope"]="Current active membership and Brand/Team permissions apply before aggregation. Team filter means content's recorded TeamId, not inferred historical team. Creator sees only own attribution in allowed Brands. Owner alone receives unattributed content count."
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
        var owner=membership.Role==WorkspaceMemberRoleEnum.Owner;
        var creator=membership.Role==WorkspaceMemberRoleEnum.ContentCreator;
        if(!owner && !creator && membership.Role!=WorkspaceMemberRoleEnum.Manager) throw new PerformanceAccessException(403);
        if(creator && memberId.HasValue && memberId!=actor) throw new PerformanceAccessException(404);
        var brandIds=(await access.GetAccessibleBrandIdsAsync(actor,workspace,ct)).ToArray();
        if(brandId.HasValue && !brandIds.Contains(brandId.Value)) throw new PerformanceAccessException(404);
        var brands=await db.Brands.IgnoreQueryFilters().AsNoTracking().Where(b=>brandIds.Contains(b.Id)).OrderBy(b=>b.Name).Select(b=>new PerformanceOption(b.Id,b.Name)).ToListAsync(ct);
        if(brandId.HasValue) brandIds=[brandId.Value];
        var teamsQuery=db.Teams.IgnoreQueryFilters().AsNoTracking().Where(t=>t.WorkspaceId==workspace && !t.IsDeleted && t.Status==TeamStatusEnum.Active);
        if(!owner) teamsQuery=teamsQuery.Where(t=>db.TeamMembers.IgnoreQueryFilters().Any(m=>m.TeamId==t.Id && m.UserId==actor && m.IsActive));
        var teams=await teamsQuery.OrderBy(t=>t.Name).Select(t=>new PerformanceOption(t.Id,t.Name)).ToListAsync(ct);
        if(teamId.HasValue && !teams.Any(t=>t.Id==teamId)) throw new PerformanceAccessException(404);
        var membersQuery=db.WorkspaceMembers.IgnoreQueryFilters().AsNoTracking().Where(m=>m.WorkspaceId==workspace && m.IsActive);
        var permittedMembers=new List<PerformanceOption>();
        foreach(var member in await membersQuery.Select(m=>new {m.UserId,m.User.FullName}).ToListAsync(ct))
        {
            if(creator && member.UserId!=actor) continue;
            if(teamId.HasValue && !await db.TeamMembers.IgnoreQueryFilters().AnyAsync(m=>m.TeamId==teamId && m.UserId==member.UserId && m.IsActive,ct)) continue;
            bool allowed=owner;
            foreach(var brand in brandIds)
                if((await access.CheckAsync(new(actor,workspace,AccessResourceKind.Brand,brand,ResourcePermission.AnalyticsMember,MemberId:member.UserId),ct)).Allowed) {allowed=true;break;}
            if(allowed) permittedMembers.Add(new(member.UserId,member.FullName??"Member"));
        }
        if(memberId.HasValue && !permittedMembers.Any(m=>m.Id==memberId)) throw new PerformanceAccessException(404);
        var selected=permittedMembers.Where(m=>!memberId.HasValue || m.Id==memberId).OrderBy(m=>m.Name).ThenBy(m=>m.Id).ToList();
        var rows=new List<MemberPerformanceRow>();
        foreach(var member in selected.Skip((page-1)*pageSize).Take(pageSize))
        {
            // Resolve member/Brand pairs independently: shared team in Alpha must
            // never grant a Manager this same member's data in Beta.
            var memberBrands=new List<Guid>();
            foreach(var brand in brandIds)
                if((await access.CheckAsync(new(actor,workspace,AccessResourceKind.Brand,brand,ResourcePermission.AnalyticsMember,MemberId:member.Id),ct)).Allowed) memberBrands.Add(brand);
            var contents=db.Contents.IgnoreQueryFilters().AsNoTracking().Where(c=>c.WorkspaceId==workspace && !c.IsDeleted && memberBrands.Contains(c.BrandId) && (!teamId.HasValue || c.TeamId==teamId));
            var own=contents.Where(c=>c.PrimaryCreatorId==member.Id);
            var created=await own.CountAsync(c=>c.CreatedAt>=from && c.CreatedAt<to,ct);
            var postsQuery=from p in db.Posts.IgnoreQueryFilters().AsNoTracking()
                join c in contents on p.ContentId equals c.Id
                join i in db.SocialIntegrations.IgnoreQueryFilters().AsNoTracking() on p.IntegrationId equals i.Id
                where !p.IsDeleted && !i.IsDeleted && i.WorkspaceId==workspace && i.BrandId==c.BrandId && p.Status==ContentStatusEnum.Published && p.PublishedAt>=@from && p.PublishedAt<to
                    && (c.PrimaryCreatorId==member.Id || p.PublishedByUserId==member.Id)
                select new {Post=p,Creator=c.PrimaryCreatorId};
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
            var reviews=await db.Approvals.IgnoreQueryFilters().AsNoTracking().Where(a=>!a.IsDeleted && own.Any(c=>c.Id==a.ContentId) && a.SubmittedAt.HasValue &&
                (a.Status==ContentStatusEnum.Approved || a.Status==ContentStatusEnum.Rejected) && (a.ApprovedAt??a.CreatedAt)>=from && (a.ApprovedAt??a.CreatedAt)<to).ToListAsync(ct);
            var decisions=reviews.GroupBy(a=>(a.ContentId,a.SubmittedAt)).Select(g=>g.OrderByDescending(a=>a.ApprovedAt??a.CreatedAt).ThenBy(a=>a.Id).First()).ToList();
            var turnaround=decisions.Where(a=>(a.ApprovedAt??a.CreatedAt)>=a.SubmittedAt).Select(a=>((a.ApprovedAt??a.CreatedAt)-a.SubmittedAt!.Value).TotalHours).ToList();
            var schedules=await db.ContentCalendars.IgnoreQueryFilters().AsNoTracking().Where(s=>s.WorkspaceId==workspace && !s.IsDeleted && s.RepeatType==RepeatTypeEnum.None &&
                s.ScheduledAt>=from && s.ScheduledAt<to && own.Any(c=>c.Id==s.ContentId) && s.IntegrationId.HasValue &&
                db.SocialIntegrations.IgnoreQueryFilters().Any(i=>i.Id==s.IntegrationId && !i.IsDeleted && i.WorkspaceId==workspace && own.Any(c=>c.Id==s.ContentId && c.BrandId==i.BrandId))).ToListAsync(ct);
            var completed=schedules.Where(s=>s.Status==ScheduleStatusEnum.Completed).ToList();
            var timed=completed.Where(s=>s.ExecutedAt.HasValue).ToList();
            var failed=schedules.Count(s=>s.Status==ScheduleStatusEnum.Failed);
            rows.Add(new(member.Id,member.Name,created,creatorPosts.Length,posts.Count(p=>p.Post.PublishedByUserId==member.Id),decisions.Count,
                Rate(decisions.Count(a=>a.Status==ContentStatusEnum.Approved),decisions.Count),completed.Count,
                schedules.Count(s=>s.Status is ScheduleStatusEnum.Pending or ScheduleStatusEnum.Processing),failed,
                Rate(timed.Count(s=>Math.Abs((s.ExecutedAt!.Value-s.ScheduledAt!.Value).TotalMinutes)<=5),timed.Count),Rate(failed,failed+completed.Count),
                turnaround.Count==0?null:Math.Round((decimal)turnaround.Average(),2),latest.Count,
                engagementKnown?latest.Sum(r=>r.Engagement):null,impressionsKnown?latest.Sum(r=>r.Impressions):null,reachKnown?latest.Sum(r=>r.Reach):null,
                engagementKnown&&impressionsKnown?Rate(latest.Sum(r=>r.Engagement),latest.Sum(r=>r.Impressions)):null,latest.Count==0?null:latest.Max(r=>r.CreatedAt)));
        }
        int? unattributed=owner?await db.Contents.IgnoreQueryFilters().CountAsync(c=>c.WorkspaceId==workspace && !c.IsDeleted && brandIds.Contains(c.BrandId) &&
            (!teamId.HasValue || c.TeamId==teamId) && c.PrimaryCreatorId==null && c.CreatedAt>=from && c.CreatedAt<to,ct):null;
        return new(rows,selected.Count,from,to,DateTime.UtcNow,unattributed,brands,teams,permittedMembers,Definitions);
    }
}
public sealed class PerformanceAccessException(int statusCode):Exception("Member performance is outside your access scope.") { public int StatusCode {get;}=statusCode; }
