using AISAM.Common.Dtos.Response;
using AISAM.Common;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace AISAM.IntegrationTests;

public class AdCampaignServiceTests
{
    private readonly Guid _workspaceId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _profileId = Guid.NewGuid();
    private readonly Guid _brandId = Guid.NewGuid();

    private static Brand CreateBrand(Guid workspaceId, Guid profileId)
    {
        return new Brand { Id = Guid.NewGuid(), WorkspaceId = workspaceId, ProfileId = profileId, Name = "Test brand" };
    }

    private AdCampaignService CreateService(
        FakeAdCampaignRepository campaignRepository,
        FakeWorkspaceMemberRepository workspaceMemberRepository,
        FakeWorkspaceRepository workspaceRepository,
        IBrandRepository? brandRepository = null,
        IContentRepository? contentRepository = null)
    {
        var providers = new IProviderService[] { new DummyProviderService() };
        return new AdCampaignService(
            campaignRepository,
            workspaceMemberRepository,
            workspaceRepository,
            new FakeSubscriptionRepository(),
            brandRepository ?? new FakeBrandRepository(CreateBrand(_workspaceId, _profileId)),
            contentRepository ?? new FakeContentRepository(),
            new FakeSocialService(),
            new FakePostRepository(),
            providers,
            NullLogger<AdCampaignService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSuccess()
    {
        var campaignRepo = new FakeAdCampaignRepository();
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var request = new CreateAdCampaignRequest
        {
            Name = "Test Campaign",
            BrandId = brand.Id,
            AdAccountId = "act_123456",
            Objective = "AWARENESS",
            Budget = 100000,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(8),
            Platform = "facebook"
        };

        var result = await service.CreateAsync(_workspaceId, _userId, request);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Test Campaign", result.Data.Name);
        Assert.Equal("facebook", result.Data.Platform);
        Assert.Single(campaignRepo.Added);
        Assert.Equal(CampaignStatusEnum.Draft, campaignRepo.Added[0].Status);
    }

    [Fact]
    public async Task CreateAsync_WithBudgetBelowMinimum_ReturnsError()
    {
        var campaignRepo = new FakeAdCampaignRepository();
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var request = new CreateAdCampaignRequest
        {
            Name = "Low Budget Campaign",
            BrandId = brand.Id,
            AdAccountId = "act_123456",
            Objective = "AWARENESS",
            Budget = 100,
            Platform = "facebook"
        };

        var result = await service.CreateAsync(_workspaceId, _userId, request);

        Assert.False(result.Success);
        Assert.Contains("Budget must be at least", result.Message);
        Assert.Empty(campaignRepo.Added);
    }

    [Fact]
    public async Task CreateAsync_WithVNDBudget30k_ReturnsSuccess()
    {
        var campaignRepo = new FakeAdCampaignRepository();
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var request = new CreateAdCampaignRequest
        {
            Name = "Min Budget VND",
            BrandId = brand.Id,
            AdAccountId = "act_123456",
            Objective = "AWARENESS",
            Budget = 30000,
            Platform = "facebook",
            AdAccountCurrency = "VND"
        };

        var result = await service.CreateAsync(_workspaceId, _userId, request);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task CreateAsync_WithUSDBudget100_ReturnsSuccess()
    {
        var campaignRepo = new FakeAdCampaignRepository();
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var request = new CreateAdCampaignRequest
        {
            Name = "Min Budget USD",
            BrandId = brand.Id,
            AdAccountId = "act_123456",
            Objective = "AWARENESS",
            Budget = 100,
            Platform = "facebook",
            AdAccountCurrency = "USD"
        };

        var result = await service.CreateAsync(_workspaceId, _userId, request);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task CreateAsync_WithUSDBudgetBelow100_ReturnsError()
    {
        var campaignRepo = new FakeAdCampaignRepository();
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var request = new CreateAdCampaignRequest
        {
            Name = "Under USD 100",
            BrandId = brand.Id,
            AdAccountId = "act_123456",
            Objective = "AWARENESS",
            Budget = 99,
            Platform = "facebook",
            AdAccountCurrency = "USD"
        };

        var result = await service.CreateAsync(_workspaceId, _userId, request);

        Assert.False(result.Success);
        Assert.Contains("Budget must be at least", result.Message);
    }

    [Fact]
    public async Task CreateAsync_WithEndBeforeStart_ReturnsError()
    {
        var campaignRepo = new FakeAdCampaignRepository();
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var request = new CreateAdCampaignRequest
        {
            Name = "Bad Dates Campaign",
            BrandId = brand.Id,
            AdAccountId = "act_123456",
            Objective = "AWARENESS",
            Budget = 100000,
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(5),
            Platform = "facebook"
        };

        var result = await service.CreateAsync(_workspaceId, _userId, request);

        Assert.False(result.Success);
        Assert.Contains("End date must be after start date", result.Message);
        Assert.Empty(campaignRepo.Added);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyName_ReturnsError()
    {
        var campaignRepo = new FakeAdCampaignRepository();
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var request = new CreateAdCampaignRequest
        {
            Name = "",
            BrandId = brand.Id,
            AdAccountId = "act_123456",
            Objective = "AWARENESS",
            Budget = 100000,
            Platform = "facebook"
        };

        var result = await service.CreateAsync(_workspaceId, _userId, request);

        Assert.False(result.Success);
        Assert.Contains("Campaign name is required", result.Message);
    }

    [Fact]
    public async Task CreateAsync_WithVariants_NotSummingTo100_ReturnsError()
    {
        var campaignRepo = new FakeAdCampaignRepository();
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var request = new CreateAdCampaignRequest
        {
            Name = "Bad Variants Campaign",
            BrandId = brand.Id,
            AdAccountId = "act_123456",
            Objective = "AWARENESS",
            Budget = 100000,
            Platform = "facebook",
            Variants = new List<AdSetVariantRequest>
            {
                new() { NameSuffix = "V1", BudgetShare = 40 },
                new() { NameSuffix = "V2", BudgetShare = 30 }
            }
        };

        var result = await service.CreateAsync(_workspaceId, _userId, request);

        Assert.False(result.Success);
        Assert.Contains("must sum to 100%", result.Message);
    }

    [Fact]
    public async Task CreateAsync_WithValidVariants_CreatesAdSets()
    {
        var campaignRepo = new FakeAdCampaignRepository();
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var request = new CreateAdCampaignRequest
        {
            Name = "Variant Campaign",
            BrandId = brand.Id,
            AdAccountId = "act_123456",
            Objective = "AWARENESS",
            Budget = 300000,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(8),
            Platform = "facebook",
            Variants = new List<AdSetVariantRequest>
            {
                new() { NameSuffix = "Creative A", BudgetShare = 60, Targeting = "{\"geo_locations\":{\"countries\":[\"VN\"]}}" },
                new() { NameSuffix = "Creative B", BudgetShare = 40 }
            }
        };

        var result = await service.CreateAsync(_workspaceId, _userId, request);

        Assert.True(result.Success);
        Assert.Equal(2, campaignRepo.AddedAdSets.Count);
        Assert.Equal("Variant Campaign - Creative A", campaignRepo.AddedAdSets[0].Name);
        Assert.Equal("Variant Campaign - Creative B", campaignRepo.AddedAdSets[1].Name);
        Assert.Equal("{\"geo_locations\":{\"countries\":[\"VN\"]}}", campaignRepo.AddedAdSets[0].Targeting);
        Assert.Null(campaignRepo.AddedAdSets[1].Targeting);
    }

    [Fact]
    public async Task SoftDeleteAsync_WithValidCampaign_MarksDeleted()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new AdCampaign
        {
            Id = campaignId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "Deletable Campaign",
            Status = CampaignStatusEnum.Draft,
            Platform = "facebook"
        };
        var campaignRepo = new FakeAdCampaignRepository(campaign);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var result = await service.SoftDeleteAsync(campaignId, _workspaceId, _userId);

        Assert.True(result.Success);
        Assert.True(campaignRepo.Updated.Exists(c => c.Id == campaignId && c.IsDeleted));
    }

    [Fact]
    public async Task RestoreAsync_WithDeletedCampaign_RestoresToDraft()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new AdCampaign
        {
            Id = campaignId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "Restorable Campaign",
            Status = CampaignStatusEnum.Paused,
            IsDeleted = true,
            Platform = "facebook"
        };
        var campaignRepo = new FakeAdCampaignRepository(campaign);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var result = await service.RestoreAsync(campaignId, _workspaceId, _userId);

        Assert.True(result.Success);
        Assert.True(campaignRepo.Updated.Exists(c => c.Id == campaignId && !c.IsDeleted && c.Status == CampaignStatusEnum.Draft));
    }

    [Fact]
    public async Task RestoreAsync_WithNotDeletedCampaign_ReturnsError()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new AdCampaign
        {
            Id = campaignId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "Active Campaign",
            Status = CampaignStatusEnum.Active,
            IsDeleted = false,
            Platform = "facebook"
        };
        var campaignRepo = new FakeAdCampaignRepository(campaign);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var result = await service.RestoreAsync(campaignId, _workspaceId, _userId);

        Assert.False(result.Success);
        Assert.Contains("not deleted", result.Message);
    }

    [Fact]
    public async Task DeployAsync_AlreadyCompleted_ReturnsAlreadyDeployed()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new AdCampaign
        {
            Id = campaignId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "Deployed Campaign",
            Budget = 300000,
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(20),
            Platform = "facebook",
            FacebookCampaignId = "fb-camp-123",
            DeploymentStatus = DeploymentStatusEnum.Completed,
            DeploymentStep = 4,
            Status = CampaignStatusEnum.Active,
            AdAccountId = "act_123"
        };
        var campaignRepo = new FakeAdCampaignRepository(campaign);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var result = await service.DeployAsync(campaignId, _workspaceId, _userId);

        Assert.True(result.Success);
        Assert.Contains("already deployed", result.Message);
    }

    [Fact]
    public async Task UpdateAsync_WithBudgetBelowMin_ReturnsError()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new AdCampaign
        {
            Id = campaignId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "Updateable Campaign",
            Budget = 100000,
            Status = CampaignStatusEnum.Draft,
            Platform = "facebook",
            AdAccountId = "act_123"
        };
        var campaignRepo = new FakeAdCampaignRepository(campaign);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var request = new UpdateAdCampaignRequest { Budget = 100 };
        var result = await service.UpdateAsync(campaignId, _workspaceId, _userId, request);

        Assert.False(result.Success);
        Assert.Contains("Budget must be at least", result.Message);
    }

    [Fact]
    public async Task DuplicateAsync_WithValidCampaign_CreatesCopy()
    {
        var originalId = Guid.NewGuid();
        var original = new AdCampaign
        {
            Id = originalId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "Original Campaign",
            Budget = 300000,
            Status = CampaignStatusEnum.Draft,
            Platform = "facebook",
            Objective = "TRAFFIC"
        };
        var campaignRepo = new FakeAdCampaignRepository(original);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var result = await service.DuplicateAsync(originalId, _workspaceId, _userId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("Original Campaign (copy)", result.Data.Name);
        Assert.Equal(CampaignStatusEnum.Draft, result.Data.Status);
        Assert.NotEqual(originalId, result.Data.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidCampaign_ReturnsSuccess()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new AdCampaign
        {
            Id = campaignId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "Getable Campaign",
            Status = CampaignStatusEnum.Active,
            Platform = "facebook",
            Budget = 200000
        };
        var campaignRepo = new FakeAdCampaignRepository(campaign);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var result = await service.GetByIdAsync(campaignId, _workspaceId, _userId);

        Assert.True(result.Success);
        Assert.Equal("Getable Campaign", result.Data!.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WrongWorkspace_ReturnsError()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new AdCampaign
        {
            Id = campaignId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "Wrong Workspace",
            Status = CampaignStatusEnum.Draft,
            Platform = "facebook"
        };
        var campaignRepo = new FakeAdCampaignRepository(campaign);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var result = await service.GetByIdAsync(campaignId, Guid.NewGuid(), _userId);

        Assert.False(result.Success);
        Assert.Contains("not found", result.Message);
    }

    [Fact]
    public async Task CreateAsync_NonMember_ReturnsError()
    {
        var campaignRepo = new FakeAdCampaignRepository();
        var otherUserId = Guid.NewGuid();
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var request = new CreateAdCampaignRequest
        {
            Name = "Non-member attempt",
            BrandId = brand.Id,
            AdAccountId = "act_123456",
            Objective = "AWARENESS",
            Budget = 100000,
            Platform = "facebook"
        };

        var result = await service.CreateAsync(_workspaceId, otherUserId, request);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DeployAsync_CreatesObjectsInPausedAndDoesNotActivate()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new AdCampaign
        {
            Id = campaignId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "Test Deploy Campaign",
            Budget = 300000,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(8),
            Platform = "facebook",
            AdAccountId = "act_123",
            LandingUrl = "https://example.com/landing",
            Status = CampaignStatusEnum.Draft,
            DeploymentStatus = DeploymentStatusEnum.None
        };
        var campaignRepo = new FakeAdCampaignRepository(campaign);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var result = await service.DeployAsync(campaignId, _workspaceId, _userId);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(campaign.DeploymentStatus == DeploymentStatusEnum.Completed || campaignRepo.DeploymentStatusUpdates.Any(u => u.Status == DeploymentStatusEnum.Completed));
        Assert.NotEqual(CampaignStatusEnum.Active, campaign.Status);
        Assert.NotNull(campaign.FacebookCampaignId);
    }

    [Fact]
    public async Task DeployAsync_WithoutLandingUrl_ReturnsError()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new AdCampaign
        {
            Id = campaignId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "No Landing URL Campaign",
            Budget = 300000,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(8),
            Platform = "facebook",
            AdAccountId = "act_123",
            LandingUrl = null,
            Status = CampaignStatusEnum.Draft,
            DeploymentStatus = DeploymentStatusEnum.None
        };
        var campaignRepo = new FakeAdCampaignRepository(campaign);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var result = await service.DeployAsync(campaignId, _workspaceId, _userId);

        Assert.False(result.Success);
        Assert.Contains("Landing URL", result.Message);
    }

    [Fact]
    public async Task ActivateAsync_OnlyWhenDeployedCompleted()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new AdCampaign
        {
            Id = campaignId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "Activate Test Campaign",
            Budget = 300000,
            Platform = "facebook",
            AdAccountId = "act_123",
            LandingUrl = "https://example.com",
            FacebookCampaignId = "fb-test-123",
            DeploymentStatus = DeploymentStatusEnum.Completed,
            DeploymentStep = 4,
            Status = CampaignStatusEnum.Paused,
            IsActive = false
        };
        var campaignRepo = new FakeAdCampaignRepository(campaign);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var result = await service.ActivateAsync(campaignId, _workspaceId, _userId);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task ActivateAsync_NotDeployed_ReturnsError()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new AdCampaign
        {
            Id = campaignId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "Not Deployed Campaign",
            Platform = "facebook",
            AdAccountId = "act_123",
            Status = CampaignStatusEnum.Draft,
            DeploymentStatus = DeploymentStatusEnum.None
        };
        var campaignRepo = new FakeAdCampaignRepository(campaign);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);
        var service = CreateService(campaignRepo, memberRepo, workspaceRepo, new FakeBrandRepository(brand));

        var result = await service.ActivateAsync(campaignId, _workspaceId, _userId);

        Assert.False(result.Success);
        Assert.Contains("deployed before activation", result.Message);
    }

    [Fact]
    public async Task DeployAsync_WithCreativeError_SavesFailedState()
    {
        var campaignId = Guid.NewGuid();
        var campaign = new AdCampaign
        {
            Id = campaignId,
            WorkspaceId = _workspaceId,
            ProfileId = _profileId,
            BrandId = _brandId,
            Name = "Creative Error Campaign",
            Budget = 300000,
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(8),
            Platform = "facebook",
            AdAccountId = "act_123",
            LandingUrl = "https://example.com/landing",
            Status = CampaignStatusEnum.Draft,
            DeploymentStatus = DeploymentStatusEnum.None
        };
        var campaignRepo = new FakeAdCampaignRepository(campaign);
        var memberRepo = new FakeWorkspaceMemberRepository(_workspaceId, _userId);
        var workspaceRepo = new FakeWorkspaceRepository(_workspaceId);
        var brand = CreateBrand(_workspaceId, _profileId);

        var errorProvider = new ErrorProviderService("facebook", throwOnCreative: true, errorMessage: "Creative khong hop le: app/content co the duoc tao khi Meta app con Development mode");
        var providers = new IProviderService[] { errorProvider };
        var service = new AdCampaignService(
            campaignRepo, memberRepo, workspaceRepo,
            new FakeSubscriptionRepository(),
            new FakeBrandRepository(brand),
            new FakeContentRepository(),
            new FakeSocialService(),
            new FakePostRepository(),
            providers,
            NullLogger<AdCampaignService>.Instance);

        var result = await service.DeployAsync(campaignId, _workspaceId, _userId);

        Assert.False(result.Success);
        Assert.Contains("Development mode", result.Message);
    }

    private sealed class FakeAdCampaignRepository : IAdCampaignRepository
    {
        private readonly Dictionary<Guid, AdCampaign> _campaigns;
        public List<AdCampaign> Added { get; } = new();
        public List<AdCampaign> Updated { get; } = new();
        public List<AdSet> AddedAdSets { get; } = new();
        public List<AdCreative> AddedCreatives { get; } = new();
        public List<Ad> AddedAds { get; } = new();
        public List<(Guid CampaignId, DeploymentStatusEnum Status, int Step)> DeploymentStatusUpdates { get; } = new();
        public List<(Guid CampaignId, CampaignStatusEnum Status)> StatusUpdates { get; } = new();

        public FakeAdCampaignRepository(params AdCampaign[] campaigns)
        {
            _campaigns = campaigns.ToDictionary(c => c.Id);
        }

        public Task<AdCampaign?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _campaigns.TryGetValue(id, out var c);
            return Task.FromResult(c is { IsDeleted: false } ? c : null);
        }

        public Task<IReadOnlyList<Ad>> GetAdsByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Ad>>(AddedAds);
        }

        public Task<AdCampaign?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _campaigns.TryGetValue(id, out var c);
            return Task.FromResult(c);
        }

        public Task<AdCampaign> AddAsync(AdCampaign campaign, CancellationToken cancellationToken = default)
        {
            Added.Add(campaign);
            _campaigns[campaign.Id] = campaign;
            return Task.FromResult(campaign);
        }

        public Task UpdateAsync(AdCampaign campaign, CancellationToken cancellationToken = default)
        {
            Updated.Add(campaign);
            _campaigns[campaign.Id] = campaign;
            return Task.CompletedTask;
        }

        public Task AddAdSetAsync(AdSet adSet, CancellationToken cancellationToken = default)
        {
            AddedAdSets.Add(adSet);
            return Task.CompletedTask;
        }

        public Task AddAdCreativeAsync(AdCreative creative, CancellationToken cancellationToken = default)
        {
            AddedCreatives.Add(creative);
            return Task.CompletedTask;
        }

        public Task AddAdAsync(Ad ad, CancellationToken cancellationToken = default)
        {
            AddedAds.Add(ad);
            return Task.CompletedTask;
        }

        public Task<PagedResult<AdCampaign>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var items = _campaigns.Values.Where(c => c.WorkspaceId == workspaceId && (includeDeleted || !c.IsDeleted)).ToList();
            return Task.FromResult(new PagedResult<AdCampaign> { Data = items, TotalCount = items.Count, Page = request.Page, PageSize = request.PageSize });
        }

        public Task<int> CountByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
            => Task.FromResult(_campaigns.Values.Count(c => c.WorkspaceId == workspaceId && !c.IsDeleted));

        public Task SetFacebookCampaignIdAsync(Guid campaignId, string facebookCampaignId, CancellationToken cancellationToken = default)
        {
            if (_campaigns.TryGetValue(campaignId, out var c)) c.FacebookCampaignId = facebookCampaignId;
            return Task.CompletedTask;
        }

        public Task UpdateCampaignInsightsAsync(Guid campaignId, long impressions, long clicks, decimal spend, long conversions, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateDeploymentStatusAsync(Guid campaignId, DeploymentStatusEnum status, int step, CancellationToken cancellationToken = default)
        {
            DeploymentStatusUpdates.Add((campaignId, status, step));
            return Task.CompletedTask;
        }
        public Task UpdateDeploymentFailureAsync(Guid campaignId, int step, string message, CancellationToken cancellationToken = default)
        {
            DeploymentStatusUpdates.Add((campaignId, DeploymentStatusEnum.Failed, step));
            return Task.CompletedTask;
        }
        public Task UpdateCampaignStatusAsync(Guid campaignId, CampaignStatusEnum status, CancellationToken cancellationToken = default)
        {
            StatusUpdates.Add((campaignId, status));
            return Task.CompletedTask;
        }
        public Task<AdSet?> GetAdSetByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default) => Task.FromResult<AdSet?>(null);
        public Task<Ad?> GetAdByAdSetIdAsync(Guid adSetId, CancellationToken cancellationToken = default) => Task.FromResult<Ad?>(null);
        public Task<AdCreative?> GetCreativeByIdAsync(Guid creativeId, CancellationToken cancellationToken = default) => Task.FromResult<AdCreative?>(null);
        public Task<IReadOnlyList<AdSet>> GetAdSetsByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AdSet>>(new List<AdSet>());
        public Task<IReadOnlyList<Ad>> GetAdsByAdSetIdAsync(Guid adSetId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<Ad>>(new List<Ad>());
        public Task HardDeleteAdAsync(Guid adId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task HardDeleteAdCreativeAsync(Guid creativeId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task HardDeleteAdSetAsync(Guid adSetId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task ClearFacebookIdsAsync(Guid campaignId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<decimal> SumSpendByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => Task.FromResult(0m);
        public Task<Dictionary<Guid, int>> UpdateExpiredCampaignsAsync(CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<Guid, int>());
        public Task<IReadOnlyList<AdCampaign>> GetDeployedCampaignsForSyncAsync(int batchSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AdCampaign>>(new List<AdCampaign>());
        public Task<IReadOnlyList<AdCampaign>> GetDeployedPendingActivationAsync(int batchSize, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AdCampaign>>(new List<AdCampaign>());
        public Task<IReadOnlyList<AdCampaign>> GetActiveCampaignsPastEndDateAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AdCampaign>>(new List<AdCampaign>());
        public Task<int> AutoPauseCampaignsExceedSpendAsync(Guid workspaceId, decimal quotaLimit, CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class FakeWorkspaceMemberRepository : IWorkspaceMemberRepository
    {
        private readonly Dictionary<(Guid, Guid), WorkspaceMember> _members;

        public FakeWorkspaceMemberRepository(Guid workspaceId, Guid userId)
        {
            _members = new Dictionary<(Guid, Guid), WorkspaceMember>
            {
                [(workspaceId, userId)] = new WorkspaceMember { WorkspaceId = workspaceId, UserId = userId, IsActive = true }
            };
        }

        public Task<WorkspaceMember?> GetByWorkspaceAndUserAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
        {
            _members.TryGetValue((workspaceId, userId), out var member);
            return Task.FromResult(member);
        }

        public Task<WorkspaceMember?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<WorkspaceMember>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<WorkspaceMember>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<WorkspaceMember> AddAsync(WorkspaceMember member, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(WorkspaceMember member, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<WorkspaceMember> TransferOwnershipAsync(Guid workspaceId, Guid currentOwnerUserId, Guid targetMemberId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> RemoveAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default) => Task.FromResult(_members.ContainsKey((workspaceId, userId)));
    }

    private sealed class FakeWorkspaceRepository : IWorkspaceRepository
    {
        private readonly Dictionary<Guid, Workspace> _workspaces;

        public FakeWorkspaceRepository(Guid workspaceId)
        {
            _workspaces = new Dictionary<Guid, Workspace>
            {
                [workspaceId] = new Workspace { Id = workspaceId, Name = "Test", Status = WorkspaceStatusEnum.Active, WorkspaceType = WorkspaceTypeEnum.Personal }
            };
        }

        public Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_workspaces.GetValueOrDefault(id));

        public Task<Workspace?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
            => GetByIdAsync(id, cancellationToken);

        public Task<IReadOnlyList<Workspace>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Workspace> AddAsync(Workspace workspace, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(_workspaces.ContainsKey(id));
        public Task<PagedResult<Workspace>> GetPagedAllAsync(PaginationRequest request, int? workspaceType = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<IReadOnlyList<Workspace>> GetAllActiveAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    private sealed class FakeBrandRepository : IBrandRepository
    {
        private readonly Dictionary<Guid, Brand> _brands;

        public FakeBrandRepository(params Brand[] brands)
        {
            _brands = brands.ToDictionary(b => b.Id);
        }

        public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_brands.GetValueOrDefault(id));

        public Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
            => GetByIdAsync(id, cancellationToken);

        public Task<PagedResult<Brand>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<bool> ExistsByNameInWorkspaceAsync(Guid workspaceId, string name, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<List<Brand>> GetByNamesAndIdsAsync(Guid workspaceId, IEnumerable<string> names, IEnumerable<Guid> ids, CancellationToken cancellationToken = default) => Task.FromResult(new List<Brand>());
    }

    private sealed class FakeContentRepository : IContentRepository
    {
        public Task HardDeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<Content?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Content?>(null);
        public Task<Content?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Content?>(null);
        public Task<PagedResult<ContentListDto>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, AdTypeEnum? adType = null, bool includeDeleted = false, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => Task.FromResult(new PagedResult<ContentListDto> { Data = new List<ContentListDto>(), TotalCount = 0, Page = 1, PageSize = 20 });
        public Task<Content> AddAsync(Content content, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Content content, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountByWorkspaceAndAdTypeAsync(Guid workspaceId, AdTypeEnum adType, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<List<string>> GetDistinctTagsByWorkspaceAsync(Guid workspaceId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<string>> GetDistinctTagsByProfileAsync(Guid profileId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<ContentListDto>> GetPagedAllAsync(PaginationRequest request, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> GetCountAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Dictionary<DateTime, int>> GetDailyCreatedAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<DateTime, int>());
    }

    private sealed class FakeSubscriptionRepository : ISubscriptionRepository
    {
        public Task<Subscription?> GetCurrentActiveByProfileIdAsync(Guid profileId, CancellationToken cancellationToken = default) => Task.FromResult<Subscription?>(null);
        public Task<Subscription?> GetCurrentActiveByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default) => Task.FromResult<Subscription?>(null);
        public Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult<Subscription?>(null);
        public Task<Subscription> AddAsync(Subscription subscription, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task UpdateAsync(Subscription subscription, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Subscription>> GetPagedAllAsync(PaginationRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<int> CountSuccessfulPromptUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<int> CountSuccessfulPostUsageAsync(Guid profileId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<int> CountSuccessfulPromptUsageByWorkspaceIdAsync(Guid workspaceId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task<int> CountSuccessfulPostUsageByWorkspaceIdAsync(Guid workspaceId, DateTime windowStart, DateTime? windowEnd, CancellationToken cancellationToken = default) => Task.FromResult(0);
    }

    private sealed class FakeSocialService : ISocialService
    {
        public Task<AuthUrlResponse> GetAuthUrlAsync(string provider, Guid profileId, CancellationToken cancellationToken = default) => Task.FromResult(new AuthUrlResponse());
        public Task<SocialAccountDto> LinkAccountAsync(string provider, Guid profileId, SocialCallbackRequest request, CancellationToken cancellationToken = default) => Task.FromResult(new SocialAccountDto());
        public Task<IReadOnlyList<SocialAccountDto>> GetProfileAccountsAsync(Guid profileId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SocialAccountDto>>(new List<SocialAccountDto> { new SocialAccountDto { Provider = "facebook", AccessToken = "test-token" } });
        public Task<IReadOnlyList<AvailableTargetDto>> ListAvailableTargetsForAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AvailableTargetDto>>(new List<AvailableTargetDto>());
        public Task<SocialAccountDto> LinkSelectedTargetsForAccountAsync(Guid profileId, Guid socialAccountId, LinkSelectedTargetsRequest request, CancellationToken cancellationToken = default) => Task.FromResult(new SocialAccountDto());
        public Task<IReadOnlyList<SocialTargetDto>> GetLinkedTargetsAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<SocialTargetDto>>(new List<SocialTargetDto>());
        public Task<bool> UnlinkAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UnlinkTargetAsync(Guid profileId, Guid socialIntegrationId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<IReadOnlyList<SocialIntegrationDto>> GetIntegrationsByBrandAsync(Guid profileId, Guid brandId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SocialIntegrationDto>>(new List<SocialIntegrationDto>
            {
                new SocialIntegrationDto { Platform = "facebook", ExternalId = "page-123", Name = "Test Page" }
            });
        public Task<SocialAccountDto?> GetSocialAccountByIdAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default) => Task.FromResult<SocialAccountDto?>(null);
        public Task<IReadOnlyList<FacebookAdAccountData>> GetAdAccountsForSocialAccountAsync(Guid profileId, Guid socialAccountId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<FacebookAdAccountData>>(new List<FacebookAdAccountData>());
        public Task<string?> GetFacebookUserAccessTokenAsync(Guid profileId, CancellationToken cancellationToken = default) => Task.FromResult<string?>("test-token");
    }

    private sealed class FakePostRepository : IPostRepository
    {
        public Task<Post?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<Post> AddAsync(Post post, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Post>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<PagedResult<Post>> GetPagedByWorkspaceIdAsync(Guid workspaceId, PaginationRequest request, Guid? brandId = null, ContentStatusEnum? status = null, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        public Task<List<Post>> GetPublishedByContentIdAsync(Guid contentId, CancellationToken cancellationToken = default) => Task.FromResult(new List<Post>());
        public Task DeleteAsync(Post post, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class DummyProviderService : IProviderService
    {
        public string ProviderName => "facebook";
        public Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
        public Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default) => Task.FromResult(new SocialAccountDto());
        public Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<AvailableTargetDto>());
        public Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, string>());
        public Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default) => Task.FromResult(new PublishResultDto());
        public Task<IEnumerable<FacebookAdAccountData>> GetAdAccountsAsync(string userAccessToken, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<FacebookAdAccountData>());
        public Task<string> CreateCampaignAsync(string adAccountId, string userAccessToken, string name, string objective, decimal? budget, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default) => Task.FromResult("fb-camp-test-123");
        public Task<string> CreateAdSetAsync(string adAccountId, string userAccessToken, string campaignId, string name, string objective, decimal? dailyBudget, DateTime? startDate, DateTime? endDate, string targetingJson, CancellationToken cancellationToken = default) => Task.FromResult("fb-adset-test-123");
        public Task<string> CreateAdCreativeAsync(string adAccountId, string userAccessToken, string pageId, string message, string linkUrl, string? imageUrl, string? callToAction, string? instagramMediaId = null, string? instagramActorId = null, string? objectStoryId = null, CancellationToken cancellationToken = default) => Task.FromResult("fb-creative-test-123");
        public Task<string> CreateAdAsync(string adAccountId, string userAccessToken, string adSetId, string creativeId, string name, string status, CancellationToken cancellationToken = default) => Task.FromResult("fb-ad-test-123");
        public Task<FacebookInsightData?> GetCampaignInsightsAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default) => Task.FromResult<FacebookInsightData?>(null);
        public Task<bool> UpdateCampaignStatusAsync(string adAccountId, string userAccessToken, string campaignId, string status, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateCampaignNameAsync(string adAccountId, string userAccessToken, string campaignId, string name, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateAdSetStatusAsync(string adAccountId, string userAccessToken, string adSetId, string status, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateAdSetBudgetAsync(string adAccountId, string userAccessToken, string adSetId, decimal dailyBudget, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateAdStatusAsync(string adAccountId, string userAccessToken, string adId, string status, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<string?> GetAdEffectiveStatusAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default) => Task.FromResult<string?>("ACTIVE");
        public Task<string?> GetAdSetEffectiveStatusAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default) => Task.FromResult<string?>("ACTIVE");
        public Task<bool> DeleteCampaignAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> DeleteAdSetAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> DeleteAdCreativeAsync(string adAccountId, string userAccessToken, string creativeId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> DeleteAdAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class ErrorProviderService : IProviderService
    {
        private readonly bool _throwOnCreative;
        private readonly string _errorMessage;

        public string ProviderName { get; }

        public ErrorProviderService(string providerName, bool throwOnCreative = false, string errorMessage = "Creative error")
        {
            ProviderName = providerName;
            _throwOnCreative = throwOnCreative;
            _errorMessage = errorMessage;
        }

        public Task<string> GetAuthUrlAsync(string state, string redirectUri, CancellationToken cancellationToken = default) => Task.FromResult(string.Empty);
        public Task<SocialAccountDto> ExchangeCodeAsync(string code, string redirectUri, CancellationToken cancellationToken = default) => Task.FromResult(new SocialAccountDto());
        public Task<IEnumerable<AvailableTargetDto>> GetTargetsAsync(string accessToken, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<AvailableTargetDto>());
        public Task<Dictionary<string, string>> GetTargetAccessTokensAsync(string userAccessToken, IEnumerable<string> providerTargetIds, CancellationToken cancellationToken = default) => Task.FromResult(new Dictionary<string, string>());
        public Task<PublishResultDto> PublishAsync(SocialAccount account, SocialIntegration integration, PostDto post, CancellationToken cancellationToken = default) => Task.FromResult(new PublishResultDto());
        public Task<IEnumerable<FacebookAdAccountData>> GetAdAccountsAsync(string userAccessToken, CancellationToken cancellationToken = default) => Task.FromResult(Enumerable.Empty<FacebookAdAccountData>());
        public Task<string> CreateCampaignAsync(string adAccountId, string userAccessToken, string name, string objective, decimal? budget, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default) => Task.FromResult("fb-camp-test-123");
        public Task<string> CreateAdSetAsync(string adAccountId, string userAccessToken, string campaignId, string name, string objective, decimal? dailyBudget, DateTime? startDate, DateTime? endDate, string targetingJson, CancellationToken cancellationToken = default) => Task.FromResult("fb-adset-test-123");
        public Task<string> CreateAdCreativeAsync(string adAccountId, string userAccessToken, string pageId, string message, string linkUrl, string? imageUrl, string? callToAction, string? instagramMediaId = null, string? instagramActorId = null, string? objectStoryId = null, CancellationToken cancellationToken = default)
        {
            if (_throwOnCreative) throw new InvalidOperationException(_errorMessage);
            return Task.FromResult("fb-creative-test-123");
        }
        public Task<string> CreateAdAsync(string adAccountId, string userAccessToken, string adSetId, string creativeId, string name, string status, CancellationToken cancellationToken = default) => Task.FromResult("fb-ad-test-123");
        public Task<FacebookInsightData?> GetCampaignInsightsAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default) => Task.FromResult<FacebookInsightData?>(null);
        public Task<bool> UpdateCampaignStatusAsync(string adAccountId, string userAccessToken, string campaignId, string status, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateCampaignNameAsync(string adAccountId, string userAccessToken, string campaignId, string name, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateAdSetStatusAsync(string adAccountId, string userAccessToken, string adSetId, string status, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateAdSetBudgetAsync(string adAccountId, string userAccessToken, string adSetId, decimal dailyBudget, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> UpdateAdStatusAsync(string adAccountId, string userAccessToken, string adId, string status, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<string?> GetAdEffectiveStatusAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default) => Task.FromResult<string?>("ACTIVE");
        public Task<string?> GetAdSetEffectiveStatusAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default) => Task.FromResult<string?>("ACTIVE");
        public Task<bool> DeleteCampaignAsync(string adAccountId, string userAccessToken, string campaignId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> DeleteAdSetAsync(string adAccountId, string userAccessToken, string adSetId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> DeleteAdCreativeAsync(string adAccountId, string userAccessToken, string creativeId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> DeleteAdAsync(string adAccountId, string userAccessToken, string adId, CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}






