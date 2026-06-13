using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;
using System.Net;

namespace AISAM.IntegrationTests;

public class BrandServiceTests
{
    [Fact]
    public async Task CreateAsync_StampsWorkspaceOwnershipAndProfileMetadata()
    {
        var workspaceId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var repository = new FakeBrandRepository();
        var service = new BrandService(repository);

        var result = await service.CreateAsync(workspaceId, profileId, userId, new CreateBrandRequest
        {
            Name = "Workspace brand"
        });

        Assert.True(result.Success);
        var created = Assert.Single(repository.Brands.Values);
        Assert.Equal(workspaceId, created.WorkspaceId);
        Assert.Equal(profileId, created.ProfileId);
        Assert.Equal(workspaceId, result.Data!.WorkspaceId);
        Assert.Equal(userId, result.Data.UserId);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsOnlyBrandsInActiveWorkspace()
    {
        var workspaceId = Guid.NewGuid();
        var repository = new FakeBrandRepository(
            CreateBrand(workspaceId, Guid.NewGuid(), "Owned brand"),
            CreateBrand(Guid.NewGuid(), Guid.NewGuid(), "Other brand"));
        var service = new BrandService(repository);

        var result = await service.GetPagedAsync(workspaceId, new PaginationRequest());

        Assert.True(result.Success);
        var item = Assert.Single(result.Data!.Data);
        Assert.Equal("Owned brand", item.Name);
        Assert.Equal(workspaceId, repository.LastPagedWorkspaceId);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFound_ForAnotherWorkspace()
    {
        var brand = CreateBrand(Guid.NewGuid(), Guid.NewGuid(), "Brand");
        var repository = new FakeBrandRepository(brand);
        var service = new BrandService(repository);

        var result = await service.UpdateAsync(brand.Id, Guid.NewGuid(), new UpdateBrandRequest
        {
            Name = "Updated"
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("Brand not found", result.Message);
    }

    [Fact]
    public async Task RestoreAsync_ReturnsNotFound_ForAnotherWorkspace()
    {
        var brand = CreateBrand(Guid.NewGuid(), Guid.NewGuid(), "Deleted");
        brand.IsDeleted = true;
        var repository = new FakeBrandRepository(brand);
        var service = new BrandService(repository);

        var result = await service.RestoreAsync(brand.Id, Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
    }

    private static Brand CreateBrand(Guid workspaceId, Guid profileId, string name)
    {
        return new Brand
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            ProfileId = profileId,
            Name = name,
            Profile = new Profile
            {
                Id = profileId,
                UserId = Guid.NewGuid(),
                Name = "Profile"
            }
        };
    }

    private sealed class FakeBrandRepository : IBrandRepository
    {
        public Dictionary<Guid, Brand> Brands { get; } = new();

        public FakeBrandRepository(params Brand[] brands)
        {
            foreach (var brand in brands)
            {
                Brands[brand.Id] = brand;
            }
        }

        public Guid LastPagedWorkspaceId { get; private set; }

        public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Brands.GetValueOrDefault(id));

        public Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Brands.GetValueOrDefault(id));

        public Task<PagedResult<Brand>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<PagedResult<Brand>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            LastPagedWorkspaceId = workspaceId;
            var data = Brands.Values
                .Where(brand => brand.WorkspaceId == workspaceId && (includeDeleted || !brand.IsDeleted))
                .ToList();

            return Task.FromResult(new PagedResult<Brand>
            {
                Data = data,
                TotalCount = data.Count,
                Page = 1,
                PageSize = 10
            });
        }

        public Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default)
        {
            brand.Profile ??= new Profile
            {
                Id = brand.ProfileId,
                UserId = Guid.NewGuid(),
                Name = "Profile"
            };

            Brands[brand.Id] = brand;
            return Task.FromResult(brand);
        }

        public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default)
        {
            Brands[brand.Id] = brand;
            return Task.CompletedTask;
        }
    }
}
