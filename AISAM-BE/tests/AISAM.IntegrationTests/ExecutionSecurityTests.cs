using AISAM.Data;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using Fixture = AISAM.IntegrationTests.PermissionSecurityTests.Fixture;

namespace AISAM.IntegrationTests;

public sealed class ExecutionSecurityTests(Xunit.Abstractions.ITestOutputHelper output)
{
    [Fact]
    public async Task BlockedDispatch_ReportsDegradedHealthWithoutClaimingSuccessfulExecution()
    {
        await using var f = await Fixture.CreateAsync();
        var health = new BackgroundJobHealthService();
        var guard = new ExecutionAuthorizationService(f.Db, new UnresolvedExecutionAuthorityPolicy(), health);
        var result = await guard.CanDispatchAsync("ScheduledPublish");
        var snapshot = System.Text.Json.JsonSerializer.Serialize(await health.GetStatusAsync());
        Assert.False(result.Allowed);
        Assert.Contains("BLOCKED_BY_BUSINESS_DECISION", snapshot); Assert.Contains("Degraded", snapshot);
        output.WriteLine(snapshot);
    }

    [Fact]
    public async Task BackgroundCredit_UsesCapturedTeamAfterTransfer_AndSeparatesSystemAudit()
    {
        await using var f = await Fixture.CreateAsync(); var operation = await Schedule(f);
        f.Db.AccessScope.Enforced = false;
        var membership = await f.Db.TeamMembers.SingleAsync(m => m.UserId == f.Owner.Id);
        membership.IsActive = false; f.Db.SaveChanges();
        f.Db.BackgroundAttribution = operation; // Only attribution is under test; no execution permission is granted.
        var record = new CreditUsageRecord { WorkspaceId = f.Workspace.Id, UserId = f.Owner.Id, Credits = 1 };
        f.Db.CreditUsageRecords.Add(record); await f.Db.SaveChangesAsync();
        Assert.Equal(operation.TeamId, record.TeamId); Assert.Equal(operation.ReferenceId, record.ReferenceId);
        var audit = await f.Db.AuditLogs.SingleAsync(a => a.TargetId == record.Id);
        Assert.True(audit.ExecutedBySystem); Assert.Equal(operation.ActorUserId, audit.RequestedBy);
        Assert.Equal(operation.TeamId, audit.TeamId);
    }

    [Fact]
    public async Task DeniedNextJob_ClearsPreviousAttributionInReusedWorkerScope()
    {
        await using var f = await Fixture.CreateAsync(); f.Db.BackgroundAttribution = await Schedule(f);
        await Guard(f.Db).CheckAsync("AiGeneration", Guid.NewGuid(), "AiGenerate");
        Assert.Null(f.Db.BackgroundAttribution);
    }

    [Fact]
    public async Task VideoStatus_RejectsOtherCreatorsFinishedJobBeforeReturningPrompt()
    {
        await using var f = await Fixture.CreateAsync();
        var job = new VideoGenerationJob { WorkspaceId = f.Workspace.Id, UserId = f.OtherCreator.Id, Status = AiStatusEnum.Completed };
        f.Db.VideoGenerationJobs.Add(job); f.Db.SaveChanges(); await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        var service = new VideoGenerationOrchestrator(null!, null!, Microsoft.Extensions.Options.Options.Create(new AISAM.Common.Models.VideoProviderSettings()),
            f.Db, null!, Microsoft.Extensions.Logging.Abstractions.NullLogger<VideoGenerationOrchestrator>.Instance, Guard(f.Db));
        var response = await service.CheckVideoStatusAsync(job.Id, f.Workspace.Id);
        Assert.Equal(403, response.StatusCode); Assert.Null(response.Data);
    }

    [Fact]
    public async Task ScheduledService_BlocksBeforeClaimOrPublishDependenciesAreTouched()
    {
        await using var f = await Fixture.CreateAsync();
        var service = new ScheduledPostingService(null!, null!, null!, null!, null!, null!, Guard(f.Db));
        var result = await service.RunDueSchedulesAsync(20);
        Assert.Equal(0, result.ScannedCount); Assert.Equal(0, result.SuccessCount);
    }

    [Fact]
    public async Task MultiChannelContent_ManagerFiltersBeforeSum_AndRevokeTakesEffect()
    {
        await using var f = await Fixture.CreateAsync();
        foreach (var (channel, count) in new[] { (f.AllowedChannel, 10L), (f.DeniedChannel, 999L) })
        {
            var post = new Post { ContentId = f.OwnContent.Id, IntegrationId = channel.Id };
            f.Db.Posts.Add(post); f.Db.PerformanceReports.Add(new PerformanceReport { PostId = post.Id, Impressions = count });
        }
        f.Db.SaveChanges(); await f.Resolve(WorkspaceMemberRoleEnum.Manager);
        Assert.Equal(10, await f.Db.ContentAnalyticsReports().SumAsync(r => r.Impressions));
        f.Db.TeamChannelAccesses.RemoveRange(f.Db.TeamChannelAccesses); f.Db.SaveChanges();
        await f.Resolve(WorkspaceMemberRoleEnum.Manager);
        Assert.Equal(0, await f.Db.ContentAnalyticsReports().SumAsync(r => r.Impressions));
    }

    [Fact]
    public void PostgreSqlModel_ContainsCompositeBoundariesAndTranslatableExecutionLookup()
    {
        using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql("Host=localhost;Database=translation_only").Options);
        var entity = db.Model.FindEntityType(typeof(ExecutionOperation))!;
        Assert.Contains(entity.GetForeignKeys(), fk => fk.Properties.Select(p => p.Name).SequenceEqual(new[] { "TeamId", "WorkspaceId" }));
        Assert.Contains(entity.GetIndexes(), i => i.IsUnique && i.Properties.Count == 3);
        Assert.Contains("execution_operations", db.Set<ExecutionOperation>().Where(o => o.WorkspaceId == Guid.Empty).ToQueryString());
        Assert.True(db.Model.FindEntityType(typeof(CollaborationTask))!.FindProperty("UpdatedAt")!.IsConcurrencyToken);
    }
    private static ExecutionAuthorizationService Guard(AisamContext db) => new(db, new UnresolvedExecutionAuthorityPolicy());
    private static async Task<ExecutionOperation> Schedule(Fixture f)
    {
        await f.Resolve(WorkspaceMemberRoleEnum.Owner);
        var schedule = new ContentCalendar { WorkspaceId = f.Workspace.Id, ContentId = f.OwnContent.Id,
            ProfileId = f.Profile.Id, IntegrationId = f.AllowedChannel.Id };
        f.Db.ContentCalendars.Add(schedule);
        await f.Db.SaveChangesAsync();
        return await f.Db.Set<ExecutionOperation>().SingleAsync();
    }

    [Theory]
    [InlineData("ScheduledPublish")]
    [InlineData("PostInsights")]
    [InlineData("CampaignInsights")]
    [InlineData("AutomationOperations")]
    public async Task UnapprovedDispatch_DeniesWithoutSelectingAuthority(string operation)
    {
        var result = await new UnresolvedExecutionAuthorityPolicy().CanDispatchAsync(operation, default);
        Assert.False(result.Allowed); Assert.Equal("BLOCKED_BY_BUSINESS_DECISION", result.Code);
    }

    [Fact]
    public async Task MissingLegacyContext_CannotFallBackToCurrentOwner()
    {
        await using var f = await Fixture.CreateAsync();
        var result = await Guard(f.Db).CheckAsync("AiGeneration", Guid.NewGuid(), "AiGenerate");
        Assert.Equal("EXECUTION_CONTEXT_REQUIRED", result.Code);
    }

    [Fact]
    public async Task ScheduleSnapshot_PreservesRequesterTeamAndTarget_WithoutInventingApproval()
    {
        await using var f = await Fixture.CreateAsync(); var o = await Schedule(f);
        Assert.Equal(f.Owner.Id, o.ActorUserId); Assert.Equal(f.Team.Id, o.TeamId);
        Assert.Equal(f.Workspace.Id, o.WorkspaceId); Assert.Equal(f.Brand.Id, o.BrandId);
        Assert.Equal(f.AllowedChannel.Id, o.IntegrationId); Assert.Null(o.ApprovedBy); Assert.Null(o.EnqueueAuthorizedAt);
        Assert.True((await Guard(f.Db).ValidateAsync(o)).Allowed);
        Assert.Equal("BLOCKED_BY_BUSINESS_DECISION", (await Guard(f.Db).CheckAsync(o.ResourceType, o.ReferenceId, o.RequestedAction)).Code);
    }

    [Theory]
    [InlineData("workspace")]
    [InlineData("team")]
    [InlineData("brand")]
    [InlineData("channel")]
    [InlineData("resource")]
    [InlineData("deleted")]
    [InlineData("reference")]
    [InlineData("missing-team")]
    [InlineData("unknown-type")]
    public async Task ExecutionIntegrity_DeniesTamperedRelationships(string field)
    {
        await using var f = await Fixture.CreateAsync(); var o = await Schedule(f);
        f.Db.Entry(o).State = EntityState.Detached;
        switch (field)
        {
            case "workspace": o.WorkspaceId = f.OtherWorkspace.Id; break;
            case "team": o.TeamId = Guid.NewGuid(); break;
            case "brand": o.BrandId = f.ForeignContent.BrandId; break;
            case "channel": o.IntegrationId = Guid.NewGuid(); break;
            case "resource": o.ResourceId = f.ForeignContent.Id; break;
            case "reference": o.ReferenceId = Guid.NewGuid(); break;
            case "missing-team": o.TeamId = null; break;
            case "unknown-type": o.ResourceType = "Arbitrary"; break;
            case "deleted": f.OwnContent.IsDeleted = true; f.Db.SaveChanges(); break;
        }
        Assert.False((await Guard(f.Db).ValidateAsync(o)).Allowed);
    }

    [Fact]
    public async Task Attribution_CannotBeRewrittenAfterEnqueue()
    {
        await using var f = await Fixture.CreateAsync(); var o = await Schedule(f);
        o.ActorUserId = f.Creator.Id;
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Db.SaveChangesAsync());
    }

    [Fact]
    public async Task CreditOfAnotherUser_DoesNotInheritAdministratorsActiveTeam()
    {
        await using var f = await Fixture.CreateAsync(); await f.Resolve(WorkspaceMemberRoleEnum.Owner);
        var record = new CreditUsageRecord { WorkspaceId = f.Workspace.Id, UserId = f.OtherCreator.Id };
        f.Db.CreditUsageRecords.Add(record); await f.Db.SaveChangesAsync();
        Assert.Null(record.TeamId);
    }

    [Fact]
    public async Task DatabaseRejectsCrossWorkspaceTask_EvenWithUnscopedSynchronousWrite()
    {
        await using var f = await Fixture.CreateAsync();
        f.Db.CollaborationTasks.Add(new CollaborationTask { WorkspaceId = f.OtherWorkspace.Id, TeamId = f.Team.Id,
            ContentId = f.OwnContent.Id, AssignedBy = f.Owner.Id, AssigneeId = f.Creator.Id });
        Assert.Throws<DbUpdateException>(() => f.Db.SaveChanges());
    }

    [Fact]
    public async Task ExpiryRepeat_IsIdempotent_AndAuditDistinguishesSystemFromRequester()
    {
        await using var f = await Fixture.CreateAsync(); await f.AddGrant(DateTime.UtcNow.AddMinutes(-1));
        var service = new CollaborationAccessService(f.Db);
        await service.ExpireAsync(DateTime.UtcNow, null, default);
        var count = await f.Db.Notifications.CountAsync();
        Assert.True(count > 0);
        await service.ExpireAsync(DateTime.UtcNow, null, default);
        Assert.Equal(count, await f.Db.Notifications.CountAsync());
        var audit = await f.Db.AuditLogs.SingleAsync();
        Assert.True(audit.ExecutedBySystem); Assert.Equal(f.Owner.Id, audit.RequestedBy);
        Assert.Equal(f.Creator.Id, audit.AffectedUserId);
    }

    [Theory]
    [InlineData(ContentAction.Reschedule)]
    [InlineData(ContentAction.Unschedule)]
    [InlineData(ContentAction.Unpublish)]
    [InlineData(ContentAction.MediaUpload)]
    [InlineData(ContentAction.AiGenerate)]
    [InlineData(ContentAction.AiImprove)]
    public async Task TemporaryCanEdit_DoesNotElevateNewActions(ContentAction action)
    {
        await using var f = await Fixture.CreateAsync(); await f.AddGrant(DateTime.UtcNow.AddHours(1));
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, action));
    }

    [Fact]
    public async Task ConcurrentTaskTransition_LoserCannotCommitDuplicateAudit()
    {
        await using var f = await Fixture.CreateAsync(); var task = await f.AddTask();
        await using var second = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseSqlite(f.Connection).Options);
        var stale = await second.CollaborationTasks.SingleAsync();
        task.UpdatedAt = task.UpdatedAt.AddSeconds(1); task.Status = CollaborationTaskStatus.Blocked;
        await f.Db.SaveChangesAsync();
        stale.UpdatedAt = stale.UpdatedAt.AddSeconds(2); stale.Status = CollaborationTaskStatus.Blocked;
        second.AuditLogs.Add(new AuditLog { ActorId = f.Owner.Id, WorkspaceId = f.Workspace.Id, TargetId = task.Id, TargetTable = "collaboration_tasks", ActionType = "DUPLICATE" });
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        Assert.False(await f.Db.AuditLogs.AnyAsync(a => a.ActionType == "DUPLICATE"));
    }

    [Fact]
    public async Task TeamTransfer_DoesNotTriggerTaskRevocation_ByExpiryWorker()
    {
        await using var f = await Fixture.CreateAsync();
        var task = new CollaborationTask
        {
            WorkspaceId = f.Workspace.Id,
            TeamId = f.Team.Id,
            ContentId = f.OtherContent.Id,
            AssigneeId = f.Creator.Id,
            AssignedBy = f.Owner.Id,
            IntegrationId = f.AllowedChannel.Id,
            Title = "Active Team Task",
            Status = CollaborationTaskStatus.Pending
        };
        f.Db.CollaborationTasks.Add(task);
        await f.Db.SaveChangesAsync();

        // Creator transfers from f.Team to another team in the same workspace
        var membership = await f.Db.TeamMembers.SingleAsync(m => m.TeamId == f.Team.Id && m.UserId == f.Creator.Id);
        membership.IsActive = false;
        var newTeam = new Team { WorkspaceId = f.Workspace.Id, Name = "Transferred Team", Status = TeamStatusEnum.Active };
        f.Db.Teams.Add(newTeam);
        f.Db.TeamMembers.Add(new TeamMember { TeamId = newTeam.Id, UserId = f.Creator.Id, Role = nameof(WorkspaceMemberRoleEnum.ContentCreator) });
        await f.Db.SaveChangesAsync();

        // Run worker expiry
        var service = new CollaborationAccessService(f.Db);
        await service.ExpireAsync(DateTime.UtcNow, null, default);

        // Task must NOT be transitioned to Blocked or ACCESS_REVOKED
        var persistedTask = await f.Db.CollaborationTasks.SingleAsync(t => t.Id == task.Id);
        Assert.Equal(CollaborationTaskStatus.Pending, persistedTask.Status);
        Assert.Null(persistedTask.BlockedReason);
        Assert.False(await f.Db.Notifications.AnyAsync(n => n.TargetId == task.Id));
        Assert.False(await f.Db.AuditLogs.AnyAsync(a => a.TargetId == task.Id && a.ActionType == "ACCESS_REVOKED"));

        // Boundary verification: user still cannot Edit without Phase Team Transfer implementation
        await f.Resolve(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.False(await f.Authorization.AllowsAsync(f.Workspace.Id, f.OtherContent.Id, ContentAction.Edit));
    }

    [Fact]
    public async Task ResourceUnlink_StillRevokesTask_WhenBrandOrChannelRemoved()
    {
        await using var f = await Fixture.CreateAsync();
        var task = new CollaborationTask
        {
            WorkspaceId = f.Workspace.Id,
            TeamId = f.Team.Id,
            ContentId = f.OtherContent.Id,
            AssigneeId = f.Creator.Id,
            AssignedBy = f.Owner.Id,
            IntegrationId = f.AllowedChannel.Id,
            Title = "Channel Bound Task",
            Status = CollaborationTaskStatus.Pending
        };
        f.Db.CollaborationTasks.Add(task);
        await f.Db.SaveChangesAsync();

        // Unlink the channel from the team
        var channel = await f.Db.TeamChannelAccesses.SingleAsync(c => c.TeamBrandId == f.TeamBrand.Id && c.IntegrationId == f.AllowedChannel.Id);
        f.Db.TeamChannelAccesses.Remove(channel);
        await f.Db.SaveChangesAsync();

        // Run worker expiry
        var service = new CollaborationAccessService(f.Db);
        await service.ExpireAsync(DateTime.UtcNow, null, default);

        // Task must be revoked because channel access was unlinked from the team
        var persistedTask = await f.Db.CollaborationTasks.SingleAsync(t => t.Id == task.Id);
        Assert.Equal(CollaborationTaskStatus.Blocked, persistedTask.Status);
        Assert.Equal("ACCESS_REVOKED", persistedTask.BlockedReason);
        Assert.True(await f.Db.AuditLogs.AnyAsync(a => a.TargetId == task.Id && a.ActionType == "ACCESS_REVOKED"));
    }

    [Fact]
    public async Task ExplicitRevoke_StillRevokesTask_EvenIfTeamResourcesIntact()
    {
        await using var f = await Fixture.CreateAsync();
        var grant = await f.AddGrant(DateTime.UtcNow.AddHours(1));

        // Explicit revoke action by manager/admin
        grant.RevokedAt = DateTime.UtcNow;
        await f.Db.SaveChangesAsync();

        // Run worker expiry
        var service = new CollaborationAccessService(f.Db);
        await service.ExpireAsync(DateTime.UtcNow, null, default);

        var task = await f.Db.CollaborationTasks.SingleAsync();
        Assert.Equal(CollaborationTaskStatus.Blocked, task.Status);
        Assert.Equal("ACCESS_REVOKED", task.BlockedReason);
        Assert.True(await f.Db.AuditLogs.AnyAsync(a => a.TargetId == task.Id && a.ActionType == "ACCESS_REVOKED"));
    }
}
