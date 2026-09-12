using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class AccessControlServiceTests
{
    private sealed class Fixture : IAsyncDisposable
    {
        public readonly DbContextOptions<AisamContext> Options = new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        public AisamContext Db { get; }
        public AccessControlService Service { get; }
        public User User = new() { Email = "actor@example.test" };
        public Workspace W = new() { WorkspaceType = WorkspaceTypeEnum.Business };
        public Brand Brand = new(), Other = new();
        public Team Team = new();
        public TeamMember TM = new();
        public TeamBrand TB = new();
        public WorkspaceMember WM = new();
        public Content Content = new();
        public SocialIntegration Channel = new();
        public TeamChannelAccess Grant = new();
        public Fixture() { Db = new(Options); Service = new(Db); }
        public async Task Seed(WorkspaceMemberRoleEnum role)
        {
            Brand.WorkspaceId = Other.WorkspaceId = W.Id;
            Team.WorkspaceId = W.Id;
            TM.TeamId = Team.Id; TM.UserId = User.Id;
            TB.TeamId = Team.Id; TB.BrandId = Brand.Id;
            WM.WorkspaceId = W.Id; WM.UserId = User.Id; WM.Role = role;
            Content.WorkspaceId = W.Id; Content.BrandId = Brand.Id; Content.PrimaryCreatorId = User.Id;
            Channel.WorkspaceId = W.Id; Channel.BrandId = Brand.Id;
            Grant.TeamBrandId = TB.Id; Grant.IntegrationId = Channel.Id; Grant.CanView = Grant.CanPublish = Grant.CanManage = true;
            Db.AddRange(User,W,Brand,Other,Team,TM,TB,WM,Content,Channel,Grant);
            await Db.SaveChangesAsync();
        }
        public Task<AccessDecision> Check(ResourcePermission action, AccessResourceKind kind = AccessResourceKind.Content, Guid? resource = null, Guid? channel = null)
            => Service.CheckAsync(new(User.Id,W.Id,kind,resource ?? Content.Id,action,channel));
        public ValueTask DisposeAsync() => Db.DisposeAsync();
    }

    [Theory]
    [InlineData(WorkspaceMemberRoleEnum.Owner,true)]
    [InlineData(WorkspaceMemberRoleEnum.Manager,true)]
    [InlineData(WorkspaceMemberRoleEnum.ContentCreator,true)]
    [InlineData(WorkspaceMemberRoleEnum.Viewer,false)]
    public async Task RoleMatrixOnOwnedContent(WorkspaceMemberRoleEnum role, bool allowed)
    {
        await using var f = new Fixture(); await f.Seed(role);
        Assert.Equal(allowed,(await f.Check(ResourcePermission.ContentEdit)).Allowed);
        // NC-07: Owner and WorkspaceManager (Manager alias) both have workspace-wide brand scope (2 brands)
        var isWorkspaceAdmin = role is WorkspaceMemberRoleEnum.Owner or WorkspaceMemberRoleEnum.Manager;
        Assert.Equal(isWorkspaceAdmin ? 2 : 1,
            (await f.Service.GetAccessibleBrandIdsAsync(f.User.Id,f.W.Id)).Count);
    }

    [Fact]
    public async Task MissingChannelAndWrongWorkspaceDenyEvenOwner()
    {
        await using var f = new Fixture(); await f.Seed(WorkspaceMemberRoleEnum.Owner);
        Assert.Equal(404,(await f.Check(ResourcePermission.PostPublish)).StatusCode);
        var foreign = new Brand { WorkspaceId = Guid.NewGuid() }; f.Db.Add(foreign); await f.Db.SaveChangesAsync();
        Assert.Equal(404,(await f.Check(ResourcePermission.BrandView,AccessResourceKind.Brand,foreign.Id)).StatusCode);
        Assert.Equal(404,(await f.Check(ResourcePermission.BillingManage,AccessResourceKind.Content)).StatusCode);
        var wrongChannel = new SocialIntegration { WorkspaceId=f.W.Id,BrandId=f.Other.Id };
        f.Db.Add(wrongChannel); await f.Db.SaveChangesAsync();
        Assert.Equal(404,(await f.Check(ResourcePermission.PostPublish,channel:wrongChannel.Id)).StatusCode);
    }

    [Fact]
    public async Task CreatorViewAllDoesNotGrantEditingOthersAndLegacyKeysIgnored()
    {
        await using var f = new Fixture(); await f.Seed(WorkspaceMemberRoleEnum.ContentCreator);
        var other = new Content { WorkspaceId=f.W.Id,BrandId=f.Brand.Id }; f.Db.Add(other);
        f.TM.Permissions = ["content.view_all_creators", "*", "Owner"]; await f.Db.SaveChangesAsync();
        Assert.Equal(404,(await f.Check(ResourcePermission.ContentView,resource:other.Id)).StatusCode);
        f.TM.Permissions=[DelegatedPermissionKeys.ViewAllCreators]; await f.Db.SaveChangesAsync();
        Assert.True((await f.Check(ResourcePermission.ContentView,resource:other.Id)).Allowed);
        Assert.Equal(403,(await f.Check(ResourcePermission.ContentEdit,resource:other.Id)).StatusCode);
    }

    [Fact]
    public async Task RevocationIsReadFromDatabaseEvenWithStaleTrackedEntity()
    {
        await using var f = new Fixture(); await f.Seed(WorkspaceMemberRoleEnum.Manager);
        Assert.True((await f.Check(ResourcePermission.PostPublish,channel:f.Channel.Id)).Allowed);
        await using(var second = new AisamContext(f.Options))
        {
            var grant=await second.TeamChannelAccesses.SingleAsync(); grant.CanPublish=false; await second.SaveChangesAsync();
        }
        Assert.True(f.Grant.CanPublish);
        Assert.Equal(403,(await f.Check(ResourcePermission.PostPublish,channel:f.Channel.Id)).StatusCode);
        f.WM.IsActive=false; await f.Db.SaveChangesAsync();
        Assert.Equal(404,(await f.Check(ResourcePermission.ContentView)).StatusCode);
        Assert.Empty(await f.Service.GetAccessibleBrandIdsAsync(f.User.Id,f.W.Id));
    }

    [Fact]
    public async Task ExpiredWorkspaceAllowsOwnerBillingButNotContentWrites()
    {
        await using var f = new Fixture(); await f.Seed(WorkspaceMemberRoleEnum.Owner);
        f.W.SubscriptionExpiredAt=DateTime.UtcNow.AddDays(-100); await f.Db.SaveChangesAsync();
        Assert.True((await f.Check(ResourcePermission.BillingManage,AccessResourceKind.Workspace,f.W.Id)).Allowed);
        Assert.Equal(403,(await f.Check(ResourcePermission.ContentEdit)).StatusCode);
        Assert.True((await f.Check(ResourcePermission.ContentView)).Allowed);
        Assert.Equal(WorkspaceStatusEnum.Active,f.W.Status); // resolver did not persist lifecycle
    }

    [Fact]
    public async Task CreatorPublishingNeedsExplicitGrantAndOwnContent()
    {
        await using var f = new Fixture(); await f.Seed(WorkspaceMemberRoleEnum.ContentCreator);
        Assert.Equal(403,(await f.Check(ResourcePermission.PostPublish,channel:f.Channel.Id)).StatusCode);
        f.TM.Permissions=[DelegatedPermissionKeys.Publish]; await f.Db.SaveChangesAsync();
        Assert.True((await f.Check(ResourcePermission.PostPublish,channel:f.Channel.Id)).Allowed);
        f.TB.IsActive=false; await f.Db.SaveChangesAsync();
        Assert.Equal(404,(await f.Check(ResourcePermission.PostPublish,channel:f.Channel.Id)).StatusCode);
    }

    [Fact]
    public async Task HistoricalPostWithWrongBrandIsHiddenFromOwner()
    {
        await using var f = new Fixture(); await f.Seed(WorkspaceMemberRoleEnum.Owner);
        var channel=new SocialIntegration { WorkspaceId=f.W.Id,BrandId=f.Other.Id };
        var post=new Post { ContentId=f.Content.Id,IntegrationId=channel.Id };
        f.Db.AddRange(channel,post); await f.Db.SaveChangesAsync();
        Assert.Equal(404,(await f.Check(ResourcePermission.PostView,AccessResourceKind.Post,post.Id)).StatusCode);
    }

    [Fact]
    public async Task ReviewGrantDoesNotExposeHistoryOrPermitPublishing()
    {
        await using var f = new Fixture(); await f.Seed(WorkspaceMemberRoleEnum.ContentCreator);
        var other = new Content { WorkspaceId=f.W.Id,BrandId=f.Brand.Id };
        f.Db.Add(other); f.TM.Permissions=[DelegatedPermissionKeys.Review]; await f.Db.SaveChangesAsync();
        Assert.True((await f.Check(ResourcePermission.ApprovalReview,resource:other.Id)).Allowed);
        Assert.Equal(404,(await f.Check(ResourcePermission.ContentView,resource:other.Id)).StatusCode);
        Assert.Equal(404,(await f.Check(ResourcePermission.PostPublish,resource:other.Id,channel:f.Channel.Id)).StatusCode);
    }

    [Fact]
    public async Task ManagerMemberAnalyticsRequiresSharedAssignedTeam()
    {
        await using var f = new Fixture(); await f.Seed(WorkspaceMemberRoleEnum.Manager);
        var other=new User { Email="other@example.test" };
        f.Db.AddRange(other,new WorkspaceMember { WorkspaceId=f.W.Id,UserId=other.Id }); await f.Db.SaveChangesAsync();
        var request=new AccessRequest(f.User.Id,f.W.Id,AccessResourceKind.Brand,f.Brand.Id,ResourcePermission.AnalyticsMember,MemberId:other.Id);
        Assert.Equal(404,(await f.Service.CheckAsync(request)).StatusCode);
        f.Db.Add(new TeamMember { TeamId=f.Team.Id,UserId=other.Id }); await f.Db.SaveChangesAsync();
        Assert.True((await f.Service.CheckAsync(request)).Allowed);
        Assert.Equal(404,(await f.Service.CheckAsync(request with { MemberId=null })).StatusCode);
    }

    [Theory]
    [InlineData("user")]
    [InlineData("team")]
    [InlineData("teamMember")]
    [InlineData("brand")]
    public async Task RevokedOrDeletedScopeIsHidden(string scope)
    {
        await using var f = new Fixture(); await f.Seed(WorkspaceMemberRoleEnum.Manager);
        if(scope=="user") f.User.IsActive=false;
        if(scope=="team") f.Team.IsDeleted=true;
        if(scope=="teamMember") f.TM.IsActive=false;
        if(scope=="brand") f.Brand.IsDeleted=true;
        await f.Db.SaveChangesAsync();
        Assert.Equal(404,(await f.Check(ResourcePermission.ContentView)).StatusCode);
    }
}
