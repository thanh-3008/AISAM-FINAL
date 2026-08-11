using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class PostRepositoryTests
{
    [Fact]
    public async Task GetPagedByProfileIdAsync_ReturnsOnlyPostsForRequestedProfile()
    {
        await using var context = CreateContext();
        var fixture = SeedFixture(context);
        var repository = new PostRepository(context);

        var result = await repository.GetPagedByProfileIdAsync(fixture.Profile.Id, new PaginationRequest());

        Assert.Single(result.Data);
        Assert.Equal(fixture.Post.Id, result.Data[0].Id);
    }

    [Fact]
    public async Task GetPagedByProfileIdAsync_AppliesOptionalBrandAndStatusFilters()
    {
        await using var context = CreateContext();
        var fixture = SeedFixture(context);
        var repository = new PostRepository(context);

        var matching = await repository.GetPagedByProfileIdAsync(
            fixture.Profile.Id,
            new PaginationRequest(),
            fixture.Brand.Id,
            ContentStatusEnum.Published);
        var noMatch = await repository.GetPagedByProfileIdAsync(
            fixture.Profile.Id,
            new PaginationRequest(),
            Guid.NewGuid(),
            ContentStatusEnum.Published);

        Assert.Single(matching.Data);
        Assert.Empty(noMatch.Data);
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    private static PostRepositoryFixture SeedFixture(AisamContext context)
    {
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var otherUser = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var ownerProfile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = owner.Id,
            Name = "Owner",
            ProfileType = ProfileTypeEnum.Basic,
            Status = ProfileStatusEnum.Active
        };
        var otherProfile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = otherUser.Id,
            Name = "Other",
            ProfileType = ProfileTypeEnum.Basic,
            Status = ProfileStatusEnum.Active
        };
        var ownerBrand = new Brand
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            Name = "Owner Brand"
        };
        var otherBrand = new Brand
        {
            Id = Guid.NewGuid(),
            ProfileId = otherProfile.Id,
            Name = "Other Brand"
        };
        var ownerContent = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            BrandId = ownerBrand.Id,
            Brand = ownerBrand,
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Owner post"
        };
        var otherContent = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = otherProfile.Id,
            BrandId = otherBrand.Id,
            Brand = otherBrand,
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Other post"
        };
        var ownerAccount = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            Platform = SocialPlatformEnum.Facebook,
            AccountId = "owner-account",
            UserAccessToken = "user-token"
        };
        var otherAccount = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = otherProfile.Id,
            Platform = SocialPlatformEnum.Facebook,
            AccountId = "other-account",
            UserAccessToken = "user-token"
        };
        var ownerIntegration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = ownerProfile.Id,
            BrandId = ownerBrand.Id,
            SocialAccountId = ownerAccount.Id,
            Platform = SocialPlatformEnum.Facebook,
            ExternalId = "owner-page",
            AccessToken = "page-token"
        };
        var otherIntegration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = otherProfile.Id,
            BrandId = otherBrand.Id,
            SocialAccountId = otherAccount.Id,
            Platform = SocialPlatformEnum.Facebook,
            ExternalId = "other-page",
            AccessToken = "page-token"
        };
        var ownerPost = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = ownerContent.Id,
            Content = ownerContent,
            IntegrationId = ownerIntegration.Id,
            Integration = ownerIntegration,
            PublishedAt = DateTime.UtcNow,
            Status = ContentStatusEnum.Published
        };
        var otherPost = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = otherContent.Id,
            Content = otherContent,
            IntegrationId = otherIntegration.Id,
            Integration = otherIntegration,
            PublishedAt = DateTime.UtcNow.AddMinutes(-1),
            Status = ContentStatusEnum.Published
        };

        context.Users.AddRange(owner, otherUser);
        context.Profiles.AddRange(ownerProfile, otherProfile);
        context.Brands.AddRange(ownerBrand, otherBrand);
        context.Contents.AddRange(ownerContent, otherContent);
        context.SocialAccounts.AddRange(ownerAccount, otherAccount);
        context.SocialIntegrations.AddRange(ownerIntegration, otherIntegration);
        context.Posts.AddRange(ownerPost, otherPost);
        context.SaveChanges();

        return new PostRepositoryFixture(ownerProfile, ownerBrand, ownerPost);
    }

    private sealed record PostRepositoryFixture(Profile Profile, Brand Brand, Post Post);
}




