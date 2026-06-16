using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using AISAM.Services.Service;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class ProductWorkspaceOwnershipTests
{
    [Fact]
    public async Task GetPagedAsync_ReturnsOnlyProductsFromActiveWorkspaceBrands()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var profile = AddProfile(context, user);
        var firstWorkspace = AddWorkspace(context);
        var secondWorkspace = AddWorkspace(context);
        var firstBrand = AddBrand(context, profile, firstWorkspace);
        var secondBrand = AddBrand(context, profile, secondWorkspace);
        context.Products.AddRange(
            new Product { BrandId = firstBrand.Id, Name = "Visible", CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
            new Product { BrandId = secondBrand.Id, Name = "Hidden", CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        var result = await service.GetPagedAsync(new PaginationRequest { PageSize = 1 }, firstWorkspace.Id, user.Id);

        Assert.True(result.Success);
        var product = Assert.Single(result.Data!.Data);
        Assert.Equal("Visible", product.Name);
        Assert.Equal(1, result.Data.TotalCount);
    }

    [Fact]
    public async Task CreateAsync_RejectsBrandFromDifferentWorkspace()
    {
        await using var context = CreateContext();
        var user = AddUser(context);
        var profile = AddProfile(context, user);
        var firstWorkspace = AddWorkspace(context);
        var secondWorkspace = AddWorkspace(context);
        var secondBrand = AddBrand(context, profile, secondWorkspace);
        var service = CreateService(context);

        var result = await service.CreateAsync(firstWorkspace.Id, user.Id, new ProductCreateRequest
        {
            BrandId = secondBrand.Id,
            Name = "Cross-workspace product"
        });

        Assert.False(result.Success);
        Assert.Empty(context.Products);
    }

    private static ProductService CreateService(AisamContext context)
        => new(new ProductRepository(context), new BrandRepository(context), new FakeProductImageStorageService());

    private static AisamContext CreateContext()
        => new(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString("N")).Options);

    private static User AddUser(AisamContext context)
    {
        var user = new User { Email = $"{Guid.NewGuid():N}@example.com", PasswordHash = "hash", PasswordSalt = "salt" };
        context.Users.Add(user);
        context.SaveChanges();
        return user;
    }

    private static Profile AddProfile(AisamContext context, User user)
    {
        var profile = new Profile { UserId = user.Id, User = user, Name = "Profile", ProfileType = ProfileTypeEnum.Basic };
        context.Profiles.Add(profile);
        context.SaveChanges();
        return profile;
    }

    private static Workspace AddWorkspace(AisamContext context)
    {
        var workspace = new Workspace { Name = "Workspace", WorkspaceType = WorkspaceTypeEnum.Business };
        context.Workspaces.Add(workspace);
        context.SaveChanges();
        return workspace;
    }

    private static Brand AddBrand(AisamContext context, Profile profile, Workspace workspace)
    {
        var brand = new Brand
        {
            ProfileId = profile.Id,
            Profile = profile,
            WorkspaceId = workspace.Id,
            Workspace = workspace,
            Name = "Brand"
        };
        context.Brands.Add(brand);
        context.SaveChanges();
        return brand;
    }
}
