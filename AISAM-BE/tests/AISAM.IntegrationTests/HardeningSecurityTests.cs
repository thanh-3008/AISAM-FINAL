using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AISAM.IntegrationTests;

public class HardeningSecurityTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Automation_confirm_and_retry_deny_before_credit_reservation(bool retry)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        await f.Resolve(WorkspaceMemberRoleEnum.Owner);
        var service = new AutomationService(null!, null!, null!, null!, f.Authorization,
            new ExecutionAuthorizationService(f.Db, new UnresolvedExecutionAuthorityPolicy()));
        var response = retry ? await service.RetryAsync(f.Workspace.Id, Guid.NewGuid()) :
            await service.ConfirmAsync(f.Workspace.Id, Guid.NewGuid());
        Assert.Equal(403, response.StatusCode);
        Assert.Equal("BLOCKED_BY_BUSINESS_DECISION", response.Message);
    }

    [Fact]
    public void Video_job_serialization_excludes_tracked_navigation_and_provider_diagnostics()
    {
        var job = new VideoGenerationJob { ErrorMessage = "TEST_PROVIDER_PRIVATE_DETAIL",
            User = new User { PasswordHash = "TEST_PASSWORD_HASH", PasswordSalt = "TEST_SALT" },
            Workspace = new Workspace { Name = "TEST_PRIVATE_NAVIGATION" } };
        var json = System.Text.Json.JsonSerializer.Serialize(job);
        foreach (var hidden in new[] { "TEST_PROVIDER_PRIVATE_DETAIL", "TEST_PASSWORD_HASH", "TEST_SALT", "TEST_PRIVATE_NAVIGATION" })
            Assert.DoesNotContain(hidden, json);
        Assert.Contains("Generation failed.", json);
    }

    [Fact]
    public async Task Authorized_schedule_captures_enqueue_evidence_without_inventing_approval()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        await f.Resolve(WorkspaceMemberRoleEnum.Manager);
        await f.Authorization.EnsureAsync(f.Workspace.Id, f.OwnContent.Id, ContentAction.Schedule, f.AllowedChannel.Id, default);
        var schedule = new ContentCalendar { WorkspaceId = f.Workspace.Id, ContentId = f.OwnContent.Id,
            ProfileId = f.Profile.Id, IntegrationId = f.AllowedChannel.Id };
        f.Db.ContentCalendars.Add(schedule); await f.Db.SaveChangesAsync();
        var snapshot = await f.Db.Set<ExecutionOperation>().SingleAsync();
        Assert.Equal(f.Manager.Id, snapshot.ActorUserId); Assert.Equal(f.Team.Id, snapshot.TeamId);
        Assert.NotNull(snapshot.EnqueueAuthorizedAt); Assert.Equal(1, snapshot.ExecutionVersion);
        Assert.Null(snapshot.ApprovedBy); Assert.Null(snapshot.ApprovedAt);
        Assert.Equal(0, snapshot.PolicyVersion);
        Assert.False((await new ExecutionAuthorizationService(f.Db, new UnresolvedExecutionAuthorityPolicy())
            .CheckAsync("ContentCalendar", schedule.Id, "Publish")).Allowed);
        await using var writer = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseSqlite(f.Connection).Options);
        (await writer.TeamMembers.SingleAsync(m => m.UserId == f.Manager.Id)).IsActive = false;
        await writer.SaveChangesAsync();
        Assert.Equal(f.Team.Id, (await writer.Set<ExecutionOperation>().AsNoTracking().SingleAsync()).TeamId);
    }

    [Fact]
    public async Task Request_cannot_forge_execution_actor_or_approval()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        await f.Resolve(WorkspaceMemberRoleEnum.Owner);
        f.Db.Set<ExecutionOperation>().Add(new ExecutionOperation { WorkspaceId = f.Workspace.Id, ActorUserId = f.OtherCreator.Id,
            ResourceId = f.OwnContent.Id, ReferenceId = Guid.NewGuid(), ApprovedBy = f.Owner.Id, ApprovedAt = DateTime.UtcNow });
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => f.Db.SaveChangesAsync());
    }

    [Fact]
    public async Task Unstamped_non_owner_enqueue_is_denied_before_persistence()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        f.Db.ContentCalendars.Add(new ContentCalendar { WorkspaceId = f.Workspace.Id, ContentId = f.OwnContent.Id,
            ProfileId = f.Profile.Id, IntegrationId = f.AllowedChannel.Id });
        Assert.Throws<UnauthorizedAccessException>(() => f.Db.SaveChanges());
        Assert.Empty(await f.Db.ContentCalendars.AsNoTracking().ToListAsync());
    }

    [Theory]
    [InlineData("/api/credit-usage/wallet", WorkspaceMemberRoleEnum.Manager)]
    [InlineData("/api/quota/workspace/current", WorkspaceMemberRoleEnum.Manager)]
    [InlineData("/api/payment/subscription/current", WorkspaceMemberRoleEnum.Manager)]
    [InlineData("/api/payment/history", WorkspaceMemberRoleEnum.Manager)]
    [InlineData("/api/credit-usage/wallet", WorkspaceMemberRoleEnum.ContentCreator)]
    [InlineData("/api/quota/workspace/current", WorkspaceMemberRoleEnum.Viewer)]
    public async Task Billing_endpoints_fail_closed_before_sensitive_services(string path, WorkspaceMemberRoleEnum role)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        var scope = await f.Resolve(role);
        var http = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        http.Request.Method = "GET"; http.Request.Path = path;
        http.Response.Body = new MemoryStream();
        http.Items[AISAM.API.Utils.WorkspaceContextHelper.ActiveWorkspaceMembershipItemKey] =
            await f.Db.WorkspaceMembers.SingleAsync(m => m.UserId == scope.UserId);
        var reached = false;
        var middleware = new AISAM.API.Middleware.ResourceAccessMiddleware(_ => { reached = true; return Task.CompletedTask; });
        await middleware.InvokeAsync(http, f.Resolver, f.Db);
        Assert.Equal(403, http.Response.StatusCode);
        Assert.False(reached);
    }

    [Fact]
    public async Task Manager_dashboard_omits_billing_and_never_calls_billing_dependencies()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        await f.Resolve(WorkspaceMemberRoleEnum.Manager);
        var service = new WorkspaceDashboardService(
            new AISAM.Repositories.Repository.CreditUsageRecordRepository(f.Db),
            new AISAM.Repositories.Repository.PostRepository(f.Db),
            new AISAM.Repositories.Repository.WorkspaceMemberRepository(f.Db), null!, null!, f.Db);
        var result = await service.GetSummaryAsync(f.Workspace.Id);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data.CreditBalance); Assert.Null(result.Data.MaxBalanceCap);
        Assert.Null(result.Data.PostQuotaLimit); Assert.Null(result.Data.PostsRemaining);
        var json = System.Text.Json.JsonSerializer.Serialize(result.Data);
        foreach (var field in new[] { "CreditBalance", "MaxBalanceCap", "PostQuotaLimit", "PostsRemaining" })
            Assert.DoesNotContain($"\"{field}\"", json);
    }

    [Theory]
    [InlineData(ContentAction.AiChat)]
    [InlineData(ContentAction.AiGenerateImage)]
    [InlineData(ContentAction.AiGenerateVideo)]
    [InlineData(ContentAction.AiAdopt)]
    public async Task Temporary_edit_never_elevates_AI_actions(ContentAction action)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        await f.AddGrant(DateTime.UtcNow.AddHours(1));
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, action));
    }

    [Theory]
    [InlineData(WorkspaceMemberRoleEnum.ContentCreator)]
    [InlineData(WorkspaceMemberRoleEnum.Manager)]
    [InlineData(WorkspaceMemberRoleEnum.Viewer)]
    public async Task Shared_content_does_not_expose_private_AI_payloads(WorkspaceMemberRoleEnum role)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        var privateJob = new AiGeneration { ContentId = f.OwnContent.Id, AiPrompt = "Private other actor instruction" };
        var legacyJob = new AiGeneration { ContentId = f.OwnContent.Id, AiPrompt = "Unknown original actor" };
        f.Db.AiGenerations.AddRange(privateJob, legacyJob);
        f.Db.Set<ExecutionOperation>().Add(new ExecutionOperation { WorkspaceId = f.Workspace.Id, ActorUserId = f.OtherCreator.Id,
            TeamId = f.Team.Id, ResourceId = f.OwnContent.Id, ResourceType = "AiGeneration", ReferenceId = privateJob.Id, RequestedAction = "AiGenerate" });
        f.Db.SaveChanges();
        await f.Resolve(role);
        Assert.Empty(await f.Db.AiGenerations.Select(g => g.AiPrompt).ToListAsync());
        await f.Resolve(WorkspaceMemberRoleEnum.Owner);
        Assert.Equal(2, await f.Db.AiGenerations.CountAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Proven_original_AI_actor_can_read_own_job_unless_downgraded_to_viewer(bool downgraded)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        var job = new AiGeneration { ContentId = f.OwnContent.Id };
        f.Db.AiGenerations.Add(job);
        f.Db.Set<ExecutionOperation>().Add(new ExecutionOperation { WorkspaceId = f.Workspace.Id, ActorUserId = f.Creator.Id,
            TeamId = f.Team.Id, ResourceId = f.OwnContent.Id, ResourceType = "AiGeneration", ReferenceId = job.Id, RequestedAction = "AiGenerate" });
        f.Db.SaveChanges();
        if (downgraded)
        {
            (await f.Db.WorkspaceMembers.SingleAsync(m => m.UserId == f.Creator.Id)).Role = WorkspaceMemberRoleEnum.Viewer;
            await f.Db.SaveChangesAsync();
        }
        await f.Resolver.ResolveAsync(f.Workspace.Id, f.Creator.Id, false);
        if (downgraded) Assert.Empty(await f.Db.AiGenerations.ToListAsync());
        else Assert.Equal(job.Id, (await f.Db.AiGenerations.SingleAsync()).Id);
    }

    [Fact]
    public async Task Synchronous_scoped_write_rejects_cross_workspace_team_brand()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        await f.Resolve(WorkspaceMemberRoleEnum.Owner);
        f.Db.TeamBrands.Add(new TeamBrand { TeamId = f.Team.Id, BrandId = f.ForeignContent.BrandId });
        Assert.Throws<UnauthorizedAccessException>(() => f.Db.SaveChanges());
    }

    [Fact]
    public async Task Synchronous_scoped_write_preserves_creator_immutability()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        await f.Resolve(WorkspaceMemberRoleEnum.Owner);
        f.OwnContent.PrimaryCreatorId = f.Owner.Id;
        Assert.Throws<UnauthorizedAccessException>(() => f.Db.SaveChanges());
    }

    [Theory]
    [InlineData("role", false)]
    [InlineData("team", false)]
    [InlineData("brand", false)]
    [InlineData("channel", false)]
    [InlineData("membership", false)]
    [InlineData("role", true)]
    [InlineData("brand", true)]
    public async Task Authorized_mutation_does_not_commit_after_independent_revoke(string change, bool synchronous)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        await f.Authorization.EnsureAsync(f.Workspace.Id, f.OwnContent.Id, ContentAction.Edit, f.AllowedChannel.Id, default);
        await using var writer = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseSqlite(f.Connection).Options);
        switch (change)
        {
            case "role": (await writer.WorkspaceMembers.SingleAsync(m => m.UserId == f.Creator.Id)).Role = WorkspaceMemberRoleEnum.Viewer; break;
            case "membership": (await writer.WorkspaceMembers.SingleAsync(m => m.UserId == f.Creator.Id)).IsActive = false; break;
            case "team": (await writer.TeamMembers.SingleAsync(m => m.UserId == f.Creator.Id)).IsActive = false; break;
            case "brand": (await writer.TeamBrands.SingleAsync()).IsActive = false; break;
            case "channel": writer.TeamChannelAccesses.Remove(await writer.TeamChannelAccesses.SingleAsync()); break;
        }
        if (synchronous) writer.SaveChanges(); else await writer.SaveChangesAsync();
        f.OwnContent.Title = "Must not persist";
        if (synchronous) Assert.Throws<MutationAuthorizationException>(() => f.Db.SaveChanges());
        else await Assert.ThrowsAsync<MutationAuthorizationException>(() => f.Db.SaveChangesAsync());
        Assert.NotEqual("Must not persist", await writer.Contents.AsNoTracking().Where(c => c.Id == f.OwnContent.Id).Select(c => c.Title).SingleAsync());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Temporary_revoke_or_reassignment_after_authorize_blocks_commit(bool reassign)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        var grant = await f.AddGrant(DateTime.UtcNow.AddHours(1));
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        await f.Authorization.EnsureAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Edit, null, default);
        await using var writer = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseSqlite(f.Connection).Options);
        if (reassign) (await writer.CollaborationTasks.SingleAsync()).AssigneeId = f.OtherCreator.Id;
        else (await writer.TemporaryAccessGrants.SingleAsync()).RevokedAt = DateTime.UtcNow;
        await writer.SaveChangesAsync();
        f.OtherContent.Title = "Must not persist";
        await Assert.ThrowsAsync<MutationAuthorizationException>(() => f.Db.SaveChangesAsync());
    }

    private sealed class MutableClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private sealed class AdvanceClockAfterWrite(MutableClock clock, DateTimeOffset deadline) : Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor
    {
        public override ValueTask<int> SavedChangesAsync(Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesCompletedEventData eventData,
            int result, CancellationToken cancellationToken = default)
        {
            clock.Now = deadline;
            return ValueTask.FromResult(result);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Grant_expiry_during_database_write_rolls_back_before_commit(bool outerTransaction)
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        var clock = new MutableClock(DateTimeOffset.UtcNow);
        var deadline = clock.Now.AddMinutes(1);
        await f.AddGrant(deadline.UtcDateTime);
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseSqlite(f.Connection)
            .AddInterceptors(new AdvanceClockAfterWrite(clock, deadline)).Options);
        var resolver = new ResourceAccessService(db);
        await resolver.ResolveAsync(f.Workspace.Id, f.Creator.Id, true);
        var authorizer = new ContentAuthorizationService(db, resolver, new CollaborationAccessService(db), clock);
        await authorizer.EnsureAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Edit, null, default);
        await using var transaction = outerTransaction ? await db.Database.BeginTransactionAsync() : null;
        var content = await db.Contents.SingleAsync(c => c.Id == f.OtherContent.Id);
        content.Title = "Expired while writing";
        await Assert.ThrowsAsync<MutationAuthorizationException>(() => db.SaveChangesAsync());
        if (transaction != null) await transaction.CommitAsync();
        Assert.NotEqual("Expired while writing", await f.Db.Contents.AsNoTracking().Where(c => c.Id == content.Id).Select(c => c.Title).SingleAsync());
    }

    [Fact]
    public async Task Grant_expiring_at_commit_is_denied_without_revision_change()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        var clock = new MutableClock(DateTimeOffset.UtcNow);
        var grant = await f.AddGrant(clock.Now.AddMinutes(1).UtcDateTime);
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        var authorization = new ContentAuthorizationService(f.Db, f.Resolver, new CollaborationAccessService(f.Db), clock);
        await authorization.EnsureAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Edit, null, default);
        clock.Now = new DateTimeOffset(grant.ExpiresAt, TimeSpan.Zero);
        f.OtherContent.Title = "Expired mutation";
        await Assert.ThrowsAsync<MutationAuthorizationException>(() => f.Db.SaveChangesAsync());
    }

    [Fact]
    public async Task Valid_grant_is_independent_of_blocked_task_status()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        await f.AddGrant(DateTime.UtcNow.AddHours(1));
        (await f.Db.CollaborationTasks.SingleAsync()).Status = CollaborationTaskStatus.Blocked;
        await f.Db.SaveChangesAsync();
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.True(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Edit));
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Publish));
    }

    [Fact]
    public async Task Native_permission_survives_expired_temporary_source()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        await f.AddGrant(DateTime.UtcNow.AddMinutes(-1));
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.True(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OwnContent.Id, ContentAction.Edit));
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Edit));
    }

    [Fact]
    public async Task Multi_channel_aggregates_pagination_and_projection_refresh_after_revoke()
    {
        await using var f = await PermissionSecurityTests.Fixture.CreateAsync();
        await f.Resolve(WorkspaceMemberRoleEnum.Manager);
        var query = f.Db.CampaignsForAnalytics();
        Assert.Equal(10, await query.SumAsync(c => c.Impressions));
        Assert.Equal(1, await query.CountAsync());
        Assert.Equal(10, await query.AverageAsync(c => c.Impressions));
        Assert.Equal(f.AllowedCampaign.Id, await query.OrderByDescending(c => c.Impressions).Select(c => c.Id).FirstAsync());
        Assert.Empty(await query.OrderBy(c => c.Id).Skip(1).Take(10).ToListAsync());
        Assert.Equal(10, (await query.GroupBy(c => c.IntegrationId).Select(g => g.Sum(c => c.Impressions)).ToListAsync()).Single());
        Assert.Equal(10, (await query.GroupBy(c => c.CreatedAt.Date).Select(g => g.Sum(c => c.Impressions)).ToListAsync()).Single());
        var oldVersion = f.Db.AccessScope.Version;
        await using var writer = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseSqlite(f.Connection).Options);
        writer.TeamChannelAccesses.Remove(await writer.TeamChannelAccesses.SingleAsync());
        await writer.SaveChangesAsync();
        await f.Resolve(WorkspaceMemberRoleEnum.Manager);
        Assert.NotEqual(oldVersion, f.Db.AccessScope.Version);
        Assert.Empty(await query.ToListAsync());
        Assert.All(await f.Db.CampaignMetadata(f.Workspace.Id).ToListAsync(), c => Assert.Null(c.Impressions));
    }
}
