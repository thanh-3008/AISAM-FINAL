using AISAM.API.Controllers;
using AISAM.Common.Dtos.Request;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public sealed class WorkspaceAuditReadSecurityTests
{
    [Fact]
    public async Task Manager_AuditListContainsOnlyVisibleTeamRecords()
    {
        await using var fixture = await AuditFixture.CreateAsync();
        await fixture.Security.Resolver.ResolveAsync(
            fixture.Security.Workspace.Id,
            fixture.Security.Manager.Id,
            write: false);

        var logs = await fixture.Security.Db
            .AuditLogsForRead(fixture.Security.Workspace.Id)
            .ToListAsync();

        Assert.Equal([fixture.Visible.Id], logs.Select(log => log.Id));
    }

    [Fact]
    public async Task Owner_AuditListContainsWorkspaceRecordsButNeverOtherWorkspaceRecords()
    {
        await using var fixture = await AuditFixture.CreateAsync();
        await fixture.Security.Resolver.ResolveAsync(
            fixture.Security.Workspace.Id,
            fixture.Security.Owner.Id,
            write: false);

        var ids = await fixture.Security.Db
            .AuditLogsForRead(fixture.Security.Workspace.Id)
            .Select(log => log.Id)
            .ToListAsync();

        Assert.Contains(fixture.Visible.Id, ids);
        Assert.Contains(fixture.OtherTeam.Id, ids);
        Assert.Contains(fixture.NoTeam.Id, ids);
        Assert.DoesNotContain(fixture.OtherWorkspace.Id, ids);
    }

    [Theory]
    [InlineData(WorkspaceMemberRoleEnum.ContentCreator)]
    [InlineData(WorkspaceMemberRoleEnum.Viewer)]
    public async Task CreatorAndViewer_CannotQueryWorkspaceAuditLogs(WorkspaceMemberRoleEnum role)
    {
        await using var fixture = await AuditFixture.CreateAsync();
        await fixture.Security.Resolve(role);

        Assert.Throws<UnauthorizedAccessException>(() =>
            fixture.Security.Db.AuditLogsForRead(fixture.Security.Workspace.Id));
    }

    [Fact]
    public async Task Manager_DetailReturnsNotFoundForOtherTeamAuditRecord()
    {
        await using var fixture = await AuditFixture.CreateAsync();
        await fixture.Security.Resolver.ResolveAsync(
            fixture.Security.Workspace.Id,
            fixture.Security.Manager.Id,
            write: false);
        var controller = new WorkspaceAuditLogsController(
            fixture.Security.Db,
            fixture.Security.Db.AccessScope);

        var result = await controller.GetById(fixture.OtherTeam.Id);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    private sealed class AuditFixture : IAsyncDisposable
    {
        public PermissionSecurityTests.Fixture Security { get; private init; } = null!;
        public AuditLog Visible { get; private init; } = null!;
        public AuditLog OtherTeam { get; private init; } = null!;
        public AuditLog NoTeam { get; private init; } = null!;
        public AuditLog OtherWorkspace { get; private init; } = null!;

        public static async Task<AuditFixture> CreateAsync()
        {
            var security = await PermissionSecurityTests.Fixture.CreateAsync();
            var otherTeam = new Team
            {
                WorkspaceId = security.Workspace.Id,
                Name = "Other audit team",
                Status = TeamStatusEnum.Active
            };
            security.Db.Teams.Add(otherTeam);
            var fixture = new AuditFixture
            {
                Security = security,
                Visible = NewLog(security.Workspace.Id, security.Team.Id, security.Owner.Id, "VISIBLE"),
                OtherTeam = NewLog(security.Workspace.Id, otherTeam.Id, security.Owner.Id, "OTHER_TEAM"),
                NoTeam = NewLog(security.Workspace.Id, null, security.Owner.Id, "NO_TEAM"),
                OtherWorkspace = NewLog(security.OtherWorkspace.Id, null, security.Owner.Id, "OTHER_WORKSPACE")
            };
            security.Db.AuditLogs.AddRange(
                fixture.Visible,
                fixture.OtherTeam,
                fixture.NoTeam,
                fixture.OtherWorkspace);
            security.Db.SaveChanges();
            return fixture;
        }

        private static AuditLog NewLog(Guid workspaceId, Guid? teamId, Guid actorId, string action) => new()
        {
            WorkspaceId = workspaceId,
            TeamId = teamId,
            ActorId = actorId,
            ActionType = action,
            TargetTable = "security_test",
            TargetId = Guid.NewGuid(),
            Notes = "This field must not be projected.",
            OldValues = "{\"private\":true}",
            NewValues = "{\"private\":true}"
        };

        public ValueTask DisposeAsync() => Security.DisposeAsync();
    }
}
