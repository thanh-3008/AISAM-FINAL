using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class BrandWorkspaceOwnershipTests
{
    [Fact]
    public async Task CreateAndListAsync_IsolatesBrandsByActiveWorkspace()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var profile = AddProfile(context, user);
        var firstWorkspace = AddWorkspace(context, user);
        var secondWorkspace = AddWorkspace(context, user);
        var service = CreateService(context);

        var created = await service.CreateAsync(firstWorkspace.Id, user.Id, new CreateBrandRequest
        {
            ProfileId = profile.Id,
            Name = "First workspace brand"
        });
        var firstList = await service.GetPagedByWorkspaceIdAsync(firstWorkspace.Id, user.Id, new PaginationRequest());
        var secondList = await service.GetPagedByWorkspaceIdAsync(secondWorkspace.Id, user.Id, new PaginationRequest());

        Assert.True(created.Success);
        Assert.Equal(firstWorkspace.Id, created.Data!.WorkspaceId);
        Assert.Single(firstList.Data!.Data);
        Assert.Empty(secondList.Data!.Data);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNotFoundAcrossWorkspaceBoundary()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var profile = AddProfile(context, user);
        var firstWorkspace = AddWorkspace(context, user);
        var secondWorkspace = AddWorkspace(context, user);
        var service = CreateService(context);
        var created = await service.CreateAsync(firstWorkspace.Id, user.Id, new CreateBrandRequest
        {
            ProfileId = profile.Id,
            Name = "Private brand"
        });

        var result = await service.GetByIdAsync(created.Data!.Id, secondWorkspace.Id, user.Id);

        Assert.False(result.Success);
        Assert.Equal("Brand not found", result.Message);
    }

    [Fact]
    public async Task CreateAsync_RejectsProfileOwnedByDifferentUser()
    {
        await using var context = CreateContext();
        var member = AddUser(context);
        var otherUser = AddUser(context);
        var otherProfile = AddProfile(context, otherUser);
        var workspace = AddWorkspace(context, member);
        var service = CreateService(context);

        var result = await service.CreateAsync(workspace.Id, member.Id, new CreateBrandRequest
        {
            ProfileId = otherProfile.Id,
            Name = "Invalid brand"
        });

        Assert.False(result.Success);
        Assert.Empty(context.Brands);
    }

    private static BrandService CreateService(AisamContext context)
    {
        return new BrandService(
            new BrandRepository(context),
            new ProfileRepository(context),
            new WorkspaceMemberRepository(context));
    }

    private static AisamContext CreateContext()
    {
        return new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options);
    }

    private static User AddUser(AisamContext context)
    {
        var user = new User
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    private static Profile AddProfile(AisamContext context, User user)
    {
        var profile = new Profile
        {
            UserId = user.Id,
            User = user,
            Name = "Profile",
            ProfileType = ProfileTypeEnum.Basic
        };
        context.Profiles.Add(profile);
        context.SaveChanges();
        return profile;
    }

    private static Workspace AddWorkspace(AisamContext context, User user)
    {
        var workspace = new Workspace
        {
            Name = "Workspace",
            WorkspaceType = WorkspaceTypeEnum.Business,
            Members =
            [
                new WorkspaceMember
                {
                    UserId = user.Id,
                    User = user,
                    Role = WorkspaceMemberRoleEnum.Owner
                }
            ]
        };
        context.Workspaces.Add(workspace);
        context.SaveChanges();
        return workspace;
    }
}
