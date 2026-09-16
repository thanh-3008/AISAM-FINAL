using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AISAM.IntegrationTests;

public class BrandV2Tests
{
    [Fact]
    public async Task MemberReadsAssignedBrandButCannotDeleteIt()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var w=new Workspace();var u=new User{Email="reader@test.local"};var b=new Brand{WorkspaceId=w.Id};var hidden=new Brand{WorkspaceId=w.Id};var t=new Team{WorkspaceId=w.Id};
        var profile=new Profile{UserId=u.Id,WorkspaceId=w.Id};b.ProfileId=hidden.ProfileId=profile.Id;
        db.AddRange(profile,w,u,b,hidden,t,new WorkspaceMember{WorkspaceId=w.Id,UserId=u.Id,Role=WorkspaceMemberRoleEnum.Owner,WorkspaceRoleV2=WorkspaceRoleV2.Member},
            new TeamBrand{TeamId=t.Id,BrandId=b.Id},new TeamMember{TeamId=t.Id,UserId=u.Id,Role=TeamRoleEnum.Viewer});await db.SaveChangesAsync();
        var cfg=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{["Rbac:UseV2"]="true"}).Build();
        var service=new BrandService(new BrandRepository(db),new ProfileRepository(db),new WorkspaceMemberRepository(db,cfg),db,cfg);
        Assert.True((await service.GetByIdAsync(b.Id,w.Id,u.Id)).Success);
        Assert.False((await service.GetByIdAsync(hidden.Id,w.Id,u.Id)).Success);
        Assert.False((await service.SoftDeleteAsync(b.Id,w.Id,u.Id)).Success);
        Assert.False(b.IsDeleted);
    }
}
