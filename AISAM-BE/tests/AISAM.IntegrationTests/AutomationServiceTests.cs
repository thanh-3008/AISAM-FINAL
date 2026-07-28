using AISAM.Common.Dtos;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.Service;

namespace AISAM.IntegrationTests;

public class AutomationServiceTests
{
    [Fact]
    public async Task ImportCsvAsync_CombinesSimpleDateAndTimeColumns()
    {
        var workspaceId = Guid.NewGuid();
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspaceId, ProfileId = Guid.NewGuid(), Name = "Demo Brand" };
        var repository = new FakeAutomationRepository();
        var service = CreateService(repository, brand);
        var future = DateTime.UtcNow.AddDays(5);
        var csv = $"Brand,Topic,Platforms,ContentType,Date,Time\nDemo Brand,Launch,Facebook,Text,{future:yyyy-MM-dd},09:30";
        await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(csv));

        var result = await service.ImportCsvAsync(workspaceId, brand.ProfileId, "Simple schedule", "UTC", "schedule.csv", stream);

        Assert.True(result.Success);
        Assert.Equal(1, result.Data!.ValidItems);
        Assert.Equal(9, repository.Plan!.Items.Single().ScheduledAt.Hour);
        Assert.Equal(30, repository.Plan.Items.Single().ScheduledAt.Minute);
    }

    [Fact]
    public async Task CreateAsync_SplitsOneRowIntoPlatformItemsWithStableUniqueKeys()
    {
        var workspaceId = Guid.NewGuid();
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspaceId, ProfileId = Guid.NewGuid(), Name = "Demo Brand" };
        var repository = new FakeAutomationRepository();
        var service = CreateService(repository, brand);

        var result = await service.CreateAsync(workspaceId, brand.ProfileId, new CreateAutomationPlanRequest
        {
            Name = "August plan",
            Rows =
            {
                new AutomationImportRowRequest
                {
                    BrandId = brand.Id,
                    Topic = "Product launch",
                    Platforms = new() { "Facebook", "Instagram", "TikTok" },
                    ContentType = "Video",
                    ScheduledAt = DateTime.UtcNow.AddDays(2)
                }
            }
        });

        Assert.True(result.Success);
        Assert.Equal(3, result.Data!.TotalItems);
        Assert.Equal(3, result.Data.ValidItems);
        Assert.Equal(3, repository.Plan!.Items.Select(item => item.IdempotencyKey).Distinct().Count());
        Assert.Equal(new[] { "facebook", "instagram", "tiktok" }, repository.Plan.Items.Select(item => item.Platform).OrderBy(value => value));
    }

    [Fact]
    public async Task CreateAndConfirm_RejectsInvalidTikTokTextButQueuesValidItems()
    {
        var workspaceId = Guid.NewGuid();
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspaceId, ProfileId = Guid.NewGuid(), Name = "Demo Brand" };
        var repository = new FakeAutomationRepository();
        var service = CreateService(repository, brand);

        var created = await service.CreateAsync(workspaceId, brand.ProfileId, new CreateAutomationPlanRequest
        {
            Name = "Mixed plan",
            Rows =
            {
                new AutomationImportRowRequest { BrandId = brand.Id, Topic = "Text post", Platforms = new() { "Facebook", "TikTok" }, ContentType = "Text", ScheduledAt = DateTime.UtcNow.AddDays(1) }
            }
        });

        Assert.True(created.Success);
        Assert.Equal(1, created.Data!.ValidItems);
        Assert.Equal(1, created.Data.FailedItems);
        var confirmed = await service.ConfirmAsync(workspaceId, created.Data.Id);
        Assert.True(confirmed.Success);
        Assert.Equal(AutomationPlanStatusEnum.Generating.ToString(), confirmed.Data!.Status);
    }

    [Fact]
    public async Task UpdateItemAsync_RevalidatesInvalidItemBeforeConfirmation()
    {
        var workspaceId = Guid.NewGuid();
        var brand = new Brand { Id = Guid.NewGuid(), WorkspaceId = workspaceId, ProfileId = Guid.NewGuid(), Name = "Demo Brand" };
        var repository = new FakeAutomationRepository();
        var service = CreateService(repository, brand);
        var created = await service.CreateAsync(workspaceId, brand.ProfileId, new CreateAutomationPlanRequest
        {
            Name = "Editable plan",
            Rows = { new AutomationImportRowRequest { BrandId = brand.Id, Topic = "TikTok post", Platforms = ["TikTok"], ContentType = "Text", ScheduledAt = DateTime.UtcNow.AddDays(2) } }
        });

        var updated = await service.UpdateItemAsync(workspaceId, created.Data!.Id, created.Data.Items.Single().Id, new UpdateAutomationItemRequest
        {
            BrandId = brand.Id, Topic = "TikTok video", Platform = "TikTok", ContentType = "Video", ScheduledAt = DateTime.UtcNow.AddDays(3)
        });

        Assert.True(updated.Success);
        Assert.Equal(1, updated.Data!.ValidItems);
        Assert.Equal(0, updated.Data.FailedItems);
        Assert.Equal("Pending", updated.Data.Items.Single().Status);
        Assert.Empty(updated.Data.Items.Single().ValidationErrors);
    }

    private static AutomationService CreateService(FakeAutomationRepository repository, Brand brand)
        => new(repository, new FakeBrandRepository(brand), new FakeProductRepository(), new FakeAutomationCreditService());

    private sealed class FakeAutomationCreditService : AISAM.Services.IServices.IAutomationCreditService
    {
        public Task<AISAM.Common.GenericResponse<bool>> ReserveAsync(Guid planId, CancellationToken cancellationToken = default) => Task.FromResult(AISAM.Common.GenericResponse<bool>.CreateSuccess(true));
        public Task<AISAM.Common.GenericResponse<bool>> SettleAsync(Guid itemId, Guid userId, CreditActionEnum action, int amount, int expectedItemUsedCredits, CancellationToken cancellationToken = default) => Task.FromResult(AISAM.Common.GenericResponse<bool>.CreateSuccess(true));
        public Task ReleaseAsync(Guid planId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeAutomationRepository : IAutomationRepository
    {
        public AutomationPlan? Plan { get; private set; }
        public Task AddAsync(AutomationPlan plan, CancellationToken cancellationToken = default) { Plan = plan; return Task.CompletedTask; }
        public Task<AutomationPlan?> GetByIdAsync(Guid workspaceId, Guid planId, CancellationToken cancellationToken = default) => Task.FromResult(Plan?.WorkspaceId == workspaceId && Plan.Id == planId ? Plan : null);
        public Task<IReadOnlyList<AutomationPlan>> GetByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AutomationPlan>>(Plan is { } plan && plan.WorkspaceId == workspaceId ? new[] { plan } : Array.Empty<AutomationPlan>());
        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeBrandRepository : IBrandRepository
    {
        private readonly Brand _brand;
        public FakeBrandRepository(Brand brand) => _brand = brand;
        public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Brand?>(id == _brand.Id ? _brand : null);
        public Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => GetByIdAsync(id, cancellationToken);
        public Task<PagedResult<Brand>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default) => Task.FromResult(new PagedResult<Brand> { Data = workspaceId == _brand.WorkspaceId ? new List<Brand> { _brand } : new List<Brand>(), TotalCount = 1, Page = 1, PageSize = 100 });
        public Task<PagedResult<Brand>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsByNameInWorkspaceAsync(Guid workspaceId, string name, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Product?>(null);
        public Task<Product?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Product?>(null);
        public Task<PagedResult<Product>> GetPagedAsync(PaginationRequest request, Guid? brandId = null, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Product>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IEnumerable<Product>> GetProductsByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Product>>(Array.Empty<Product>());
        public Task<IEnumerable<Product>> GetProductsByBrandIdIncludingDeletedAsync(Guid brandId, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<Product>>(Array.Empty<Product>());
        public Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Product product, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
