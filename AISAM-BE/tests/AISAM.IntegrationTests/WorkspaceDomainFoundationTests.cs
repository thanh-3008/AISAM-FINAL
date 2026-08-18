using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class WorkspaceDomainFoundationTests
{
    [Fact]
    public void WorkspaceTypeEnum_UsesApprovedValues()
    {
        Assert.Equal(1, (int)WorkspaceTypeEnum.Personal);
        Assert.Equal(2, (int)WorkspaceTypeEnum.Business);
    }

    [Fact]
    public void Workspace_DefaultsToActiveWithEmptyMembers()
    {
        var workspace = new Workspace();

        Assert.Equal(WorkspaceStatusEnum.Active, workspace.Status);
        Assert.Empty(workspace.Members);
    }

    [Fact]
    public void WorkspaceMember_DefaultsToSharedPoolAndActive()
    {
        var member = new WorkspaceMember();

        Assert.Equal(MemberQuotaModeEnum.SharedPool, member.QuotaMode);
        Assert.True(member.IsActive);
        Assert.Equal(0, member.CreditUsed);
    }

    [Fact]
    public void UserCanBeRepresentedAsMemberOfMultipleWorkspaces()
    {
        var userId = Guid.NewGuid();
        var firstMembership = new WorkspaceMember
        {
            UserId = userId,
            WorkspaceId = Guid.NewGuid(),
            Role = WorkspaceMemberRoleEnum.Owner
        };
        var secondMembership = new WorkspaceMember
        {
            UserId = userId,
            WorkspaceId = Guid.NewGuid(),
            Role = WorkspaceMemberRoleEnum.Manager
        };

        Assert.Equal(firstMembership.UserId, secondMembership.UserId);
        Assert.NotEqual(firstMembership.WorkspaceId, secondMembership.WorkspaceId);
    }

    [Fact]
    public void DbContext_ConfiguresUniqueWorkspaceMembership()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(WorkspaceMember));

        var uniqueMembershipIndex = entityType!.GetIndexes().Single(index =>
            index.IsUnique &&
            index.Properties.Select(property => property.Name)
                .SequenceEqual([nameof(WorkspaceMember.WorkspaceId), nameof(WorkspaceMember.UserId)]));

        Assert.NotNull(uniqueMembershipIndex);
    }

    [Fact]
    public void DbContext_ConfiguresWorkspaceMemberRelationships()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(WorkspaceMember));
        var foreignKeys = entityType!.GetForeignKeys().ToList();

        Assert.Contains(foreignKeys, foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Workspace) &&
            foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
        Assert.Contains(foreignKeys, foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(User) &&
            foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AisamContext(options);
    }
}




