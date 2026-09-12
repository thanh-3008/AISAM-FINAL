using AISAM.Data.Model;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

// InMemory deliberately does not enforce FKs: these tests exercise application
// validation, not PostgreSQL constraint/transaction guarantees.
public class PermissionIntegrityTests
{
    [Fact]
    public async Task ChannelMutationGrantRequiresView()
    {
        await using var db = Db();
        db.TeamChannelAccesses.Add(new TeamChannelAccess { CanPublish = true });
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
        Assert.Empty(await db.TeamChannelAccesses.ToListAsync());
    }
    [Fact]
    public async Task ServerActorOverridesInputAndAttributesUpdatePublishAndSchedule()
    {
        await using var db = Db();
        var actor = new User { Email = "actor@example.test" };
        var workspace = Guid.NewGuid();
        db.AddRange(actor, new WorkspaceMember { UserId = actor.Id, WorkspaceId = workspace });
        await db.SaveChangesAsync();
        db.ExecutionActorId = actor.Id;
        db.ExecutionIsSystem = false;
        var content = new Content { WorkspaceId = workspace, PrimaryCreatorId = Guid.NewGuid(), Status = AISAM.Data.Enumeration.ContentStatusEnum.Approved };
        var post = new Post { PublishedByUserId = Guid.NewGuid() };
        var schedule = new ContentCalendar { ContentId = content.Id, WorkspaceId = workspace, ScheduledByUserId = Guid.NewGuid() };
        db.AddRange(content, post, schedule);
        await db.SaveChangesAsync();
        Assert.Equal(actor.Id, content.PrimaryCreatorId);
        Assert.Equal(actor.Id, content.UpdatedByUserId);
        Assert.Equal(actor.Id, post.PublishedByUserId);
        Assert.False(post.ExecutedBySystem);
        Assert.Equal(actor.Id, schedule.ScheduledByUserId);
        var updater = Guid.NewGuid();
        db.ExecutionActorId = updater;
        content.TextContent = "Edited";
        await db.SaveChangesAsync();
        Assert.Equal(actor.Id, content.PrimaryCreatorId);
        Assert.Equal(updater, content.UpdatedByUserId);
    }
    private static AisamContext Db() => new(new DbContextOptionsBuilder<AisamContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CrossWorkspaceTeamBrandIsRejectedBeforeAnySave(bool synchronous)
    {
        await using var db = Db();
        var team = new Team { WorkspaceId = Guid.NewGuid(), Name = "Team" };
        var brand = new Brand { WorkspaceId = Guid.NewGuid(), Name = "Brand" };
        db.AddRange(team, brand);
        db.TeamBrands.Add(new TeamBrand { TeamId = team.Id, BrandId = brand.Id });
        if (synchronous) Assert.Throws<UnauthorizedAccessException>(() => db.SaveChanges());
        else await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
        Assert.Equal(0, await db.TeamBrands.CountAsync());
        Assert.Equal(0, await db.Teams.CountAsync());
    }

    [Fact]
    public async Task ChannelFromAnotherBrandInSameWorkspaceIsRejected()
    {
        await using var db = Db();
        var workspace = Guid.NewGuid();
        var team = new Team { WorkspaceId = workspace };
        var brand = new Brand { WorkspaceId = workspace };
        var assignment = new TeamBrand { TeamId = team.Id, BrandId = brand.Id };
        var channel = new SocialIntegration { WorkspaceId = workspace, BrandId = Guid.NewGuid() };
        db.AddRange(team, brand, assignment, channel);
        db.TeamChannelAccesses.Add(new TeamChannelAccess { TeamBrandId = assignment.Id, IntegrationId = channel.Id });
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task SameWorkspaceAndBrandCanBeSavedTogether()
    {
        await using var db = Db();
        var workspace = Guid.NewGuid();
        var team = new Team { WorkspaceId = workspace };
        var brand = new Brand { WorkspaceId = workspace };
        var assignment = new TeamBrand { TeamId = team.Id, BrandId = brand.Id };
        var channel = new SocialIntegration { WorkspaceId = workspace, BrandId = brand.Id };
        db.AddRange(team, brand, assignment, channel);
        db.TeamChannelAccesses.Add(new TeamChannelAccess { TeamBrandId = assignment.Id, IntegrationId = channel.Id });
        await db.SaveChangesAsync();
        Assert.Equal(1, await db.TeamChannelAccesses.CountAsync());
        team.WorkspaceId = Guid.NewGuid();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task RevokedTrackedMembershipCannotBeUsedToAttributeNewContent()
    {
        await using var db = Db();
        var user = new User { Email = "creator@example.test" };
        var member = new WorkspaceMember { UserId = user.Id, WorkspaceId = Guid.NewGuid() };
        db.AddRange(user, member);
        await db.SaveChangesAsync();
        member.IsActive = false;
        db.Contents.Add(new Content { WorkspaceId = member.WorkspaceId, PrimaryCreatorId = user.Id });
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
        Assert.Equal(0, await db.Contents.CountAsync());
    }

    [Fact]
    public async Task CreatorIsImmutableButLegacyNullCanStillBeReadAndEdited()
    {
        await using var db = Db();
        var content = new Content { WorkspaceId = Guid.NewGuid(), TextContent = "Legacy" };
        db.Contents.Add(content);
        await db.SaveChangesAsync();
        content.TextContent = "Updated";
        await db.SaveChangesAsync();
        content.PrimaryCreatorId = Guid.NewGuid();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => db.SaveChangesAsync());
    }
}
