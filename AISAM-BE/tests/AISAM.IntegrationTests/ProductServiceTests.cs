using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace AISAM.IntegrationTests;

public class ProductServiceTests
{
    [Fact]
    public async Task GetPagedAsync_ReturnsOnlyProductsInActiveWorkspace()
    {
        var workspaceId = Guid.NewGuid();
        var ownBrand = CreateBrand(workspaceId);
        var otherBrand = CreateBrand(Guid.NewGuid());
        var repository = new FakeProductRepository(
            CreateProduct(ownBrand, "Owned"),
            CreateProduct(otherBrand, "Other"));
        var service = new ProductService(repository, new FakeBrandRepository(ownBrand, otherBrand));

        var result = await service.GetPagedAsync(workspaceId, new PaginationRequest { Page = 1, PageSize = 10 });

        Assert.True(result.Success);
        var item = Assert.Single(result.Data!.Data);
        Assert.Equal("Owned", item.Name);
        Assert.Equal(workspaceId, repository.LastWorkspaceId);
    }

    [Fact]
    public async Task CreateAsync_ReturnsNotFound_WhenBrandBelongsToAnotherWorkspace()
    {
        var service = new ProductService(new FakeProductRepository(), new FakeBrandRepository(CreateBrand(Guid.NewGuid())));

        var result = await service.CreateAsync(Guid.NewGuid(), new ProductCreateRequest
        {
            BrandId = Guid.NewGuid(),
            Name = "Product"
        });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("Brand not found", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNotFound_ForAnotherWorkspaceProduct()
    {
        var ownWorkspaceId = Guid.NewGuid();
        var product = CreateProduct(CreateBrand(Guid.NewGuid()), "Product");
        var service = new ProductService(new FakeProductRepository(product), new FakeBrandRepository(product.Brand));

        var result = await service.UpdateAsync(product.Id, ownWorkspaceId, new ProductUpdateRequestDto { Name = "Updated" });

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("Product not found", result.Message);
    }

    [Fact]
    public async Task CreateAsync_ReturnsError_WhenImageFilesAreProvided()
    {
        var workspaceId = Guid.NewGuid();
        var brand = CreateBrand(workspaceId);
        var service = new ProductService(new FakeProductRepository(), new FakeBrandRepository(brand));
        await using var createStream = new MemoryStream(new byte[] { 1 });

        var result = await service.CreateAsync(workspaceId, new ProductCreateRequest
        {
            BrandId = brand.Id,
            Name = "New product",
            ImageFiles =
            [
                new FormFile(createStream, 0, createStream.Length, "image", "product.png")
            ]
        });

        Assert.False(result.Success);
        Assert.Contains("upload is not enabled", result.Message);
    }

    private static Brand CreateBrand(Guid workspaceId)
    {
        return new Brand
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            ProfileId = Guid.NewGuid(),
            Name = "Brand"
        };
    }

    private static Product CreateProduct(Brand brand, string name)
    {
        return new Product
        {
            Id = Guid.NewGuid(),
            BrandId = brand.Id,
            Brand = brand,
            Name = name
        };
    }

    private sealed class FakeBrandRepository : IBrandRepository
    {
        private readonly Dictionary<Guid, Brand> _brands;

        public FakeBrandRepository(params Brand[] brands)
        {
            _brands = brands.ToDictionary(brand => brand.Id);
        }

        public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_brands.GetValueOrDefault(id));
        public Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task<PagedResult<Brand>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Brand>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        private readonly Dictionary<Guid, Product> _products;

        public FakeProductRepository(params Product[] products)
        {
            _products = products.ToDictionary(product => product.Id);
        }

        public Guid LastWorkspaceId { get; private set; }

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_products.GetValueOrDefault(id));
        public Task<Product?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);

        public Task<PagedResult<Product>> GetPagedAsync(PaginationRequest request, Guid? brandId = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<PagedResult<Product>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            LastWorkspaceId = workspaceId;
            var query = _products.Values.Where(product => product.Brand.WorkspaceId == workspaceId);

            if (brandId.HasValue)
            {
                query = query.Where(product => product.BrandId == brandId.Value);
            }

            if (!includeDeleted)
            {
                query = query.Where(product => !product.IsDeleted);
            }

            var data = query.ToList();
            return Task.FromResult(new PagedResult<Product>
            {
                Data = data,
                TotalCount = data.Count,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        public Task<IEnumerable<Product>> GetProductsByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default)
            => Task.FromResult(_products.Values.Where(product => product.BrandId == brandId && !product.IsDeleted).AsEnumerable());

        public Task<IEnumerable<Product>> GetProductsByBrandIdIncludingDeletedAsync(Guid brandId, CancellationToken cancellationToken = default)
            => Task.FromResult(_products.Values.Where(product => product.BrandId == brandId).AsEnumerable());

        public Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            _products[product.Id] = product;
            return Task.FromResult(product);
        }

        public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        {
            _products[product.Id] = product;
            return Task.CompletedTask;
        }
    }
}
