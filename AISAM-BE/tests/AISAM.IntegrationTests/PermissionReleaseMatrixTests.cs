using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class PermissionReleaseMatrixTests
{
    [Fact]
    public async Task FiveActorsTwoTeamsAndBrandsEnforceOwnershipBillingAndRevocation()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var workspace=new Workspace { WorkspaceType=WorkspaceTypeEnum.Business };
        var roles=new[]{WorkspaceMemberRoleEnum.Owner,WorkspaceMemberRoleEnum.Manager,WorkspaceMemberRoleEnum.Viewer,WorkspaceMemberRoleEnum.ContentCreator,WorkspaceMemberRoleEnum.ContentCreator};
        var users=Enumerable.Range(0,5).Select(i=>new User { Email=$"release-{i}@example.test", IsActive=true }).ToArray();
        var teams=Enumerable.Range(0,2).Select(i=>new Team { WorkspaceId=workspace.Id, Name=$"Release team {i}" }).ToArray();
        var brands=Enumerable.Range(0,2).Select(i=>new Brand { WorkspaceId=workspace.Id, Name=$"Release brand {i}" }).ToArray();
        var links=Enumerable.Range(0,2).Select(i=>new TeamBrand { TeamId=teams[i].Id,BrandId=brands[i].Id }).ToArray();
        var channels=Enumerable.Range(0,2).Select(i=>new SocialIntegration { WorkspaceId=workspace.Id,BrandId=brands[i].Id }).ToArray();
        var grants=Enumerable.Range(0,2).Select(i=>new TeamChannelAccess { TeamBrandId=links[i].Id,IntegrationId=channels[i].Id,CanView=true,CanPublish=true }).ToArray();
        var contents=Enumerable.Range(0,2).Select(i=>new Content { WorkspaceId=workspace.Id,BrandId=brands[i].Id,TeamId=teams[i].Id,PrimaryCreatorId=users[i+3].Id }).ToArray();
        db.Add(workspace);db.AddRange(users);db.AddRange(teams);db.AddRange(brands);db.AddRange(links);db.AddRange(channels);db.AddRange(grants);
        for(int i=0;i<5;i++)db.Add(new WorkspaceMember { WorkspaceId=workspace.Id,UserId=users[i].Id,Role=roles[i],IsActive=true });
        for(int i=1;i<5;i++)db.Add(new TeamMember { TeamId=teams[i==4?1:0].Id,UserId=users[i].Id,IsActive=true });
        db.AddRange(contents);await db.SaveChangesAsync();
        var access=new AccessControlService(db);
        for(int i=0;i<5;i++)
        {
            var billing=await access.CheckAsync(new(users[i].Id,workspace.Id,AccessResourceKind.Workspace,workspace.Id,ResourcePermission.BillingManage));
            Assert.Equal(i==0,billing.Allowed);
        }
        // A channel grant alone does not delegate publishing to a Creator.
        Assert.False((await access.CheckAsync(new(users[3].Id,workspace.Id,AccessResourceKind.Content,contents[0].Id,ResourcePermission.PostPublish,channels[0].Id))).Allowed);
        var creatorMember = await db.TeamMembers.SingleAsync(m => m.UserId == users[3].Id);
        creatorMember.Permissions = [DelegatedPermissionKeys.Publish];
        channels[0].IsActive = true;
        await db.SaveChangesAsync();
        Assert.True((await access.CheckAsync(new(users[3].Id,workspace.Id,AccessResourceKind.Content,contents[0].Id,ResourcePermission.PostPublish,channels[0].Id))).Allowed);
        Assert.False((await access.CheckAsync(new(users[3].Id,workspace.Id,AccessResourceKind.Content,contents[1].Id,ResourcePermission.ContentView))).Allowed);
        Assert.False((await access.CheckAsync(new(users[1].Id,workspace.Id,AccessResourceKind.Brand,brands[1].Id,ResourcePermission.AnalyticsView))).Allowed);
        Assert.False((await access.CheckAsync(new(users[2].Id,workspace.Id,AccessResourceKind.Content,contents[0].Id,ResourcePermission.PostPublish,channels[0].Id))).Allowed);
        grants[0].CanPublish=false;await db.SaveChangesAsync();
        Assert.False((await access.CheckAsync(new(users[3].Id,workspace.Id,AccessResourceKind.Content,contents[0].Id,ResourcePermission.PostPublish,channels[0].Id))).Allowed);
    }
}
