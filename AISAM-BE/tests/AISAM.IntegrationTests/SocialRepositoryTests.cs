using AISAM.Common.Dtos;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Repository;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;

public class SocialRepositoryTests
{
    [Fact]
    public async Task GetByProfileIdAsync_ExcludesSoftDeletedAccountsAndIntegrations()
    {
        await using var context = CreateContext();
        var fixture = SeedFixture(context);
        var repository = new SocialAccountRepository(context);

        var accounts = await repository.GetByProfileIdAsync(fixture.Profile.Id);

        var account = Assert.Single(accounts);
        Assert.Equal(fixture.ActiveAccount.Id, account.Id);
        Assert.Single(account.SocialIntegrations);
        Assert.Equal(fixture.ActiveIntegration.Id, account.SocialIntegrations.Single().Id);
    }

    [Fact]
    public async Task UpdateAsync_PersistsSoftDeleteFlags_ForAccountAndIntegration_WithoutRemovingPosts()
    {
        await using var context = CreateContext();
        var fixture = SeedFixture(context);
        var accountRepository = new SocialAccountRepository(context);
        var integrationRepository = new SocialIntegrationRepository(context);
        var postRepository = new PostRepository(context);

        fixture.ActiveAccount.IsDeleted = true;
        fixture.ActiveAccount.IsActive = false;
        fixture.ActiveIntegration.IsDeleted = true;
        fixture.ActiveIntegration.IsActive = false;

        await accountRepository.UpdateAsync(fixture.ActiveAccount);
        await integrationRepository.UpdateAsync(fixture.ActiveIntegration);

        var accounts = await accountRepository.GetByProfileIdAsync(fixture.Profile.Id);
        var integrations = await integrationRepository.GetByBrandIdAsync(fixture.Brand.Id);
        var posts = await postRepository.GetPagedByProfileIdAsync(fixture.Profile.Id, new PaginationRequest());

        Assert.Empty(accounts);
        Assert.Empty(integrations);
        Assert.Single(posts.Data);
        Assert.Equal(fixture.Post.Id, posts.Data[0].Id);
    }

    private static AisamContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new AisamContext(options);
    }

    private static RepositoryFixture SeedFixture(AisamContext context)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "Profile",
            ProfileType = ProfileTypeEnum.Basic,
            Status = ProfileStatusEnum.Active
        };
        var brand = new Brand
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            Name = "Brand"
        };
        var content = new Content
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            BrandId = brand.Id,
            Brand = brand,
            AdType = AdTypeEnum.TextOnly,
            TextContent = "Post body"
        };
        var activeAccount = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            Platform = SocialPlatformEnum.Facebook,
            AccountId = "active-account",
            UserAccessToken = "encrypted-user-token"
        };
        var deletedAccount = new SocialAccount
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            Platform = SocialPlatformEnum.Facebook,
            AccountId = "deleted-account",
            UserAccessToken = "encrypted-user-token",
            IsDeleted = true,
            IsActive = false
        };
        var activeIntegration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            BrandId = brand.Id,
            SocialAccountId = activeAccount.Id,
            Platform = SocialPlatformEnum.Facebook,
            ExternalId = "page-active",
            AccessToken = "page-token"
        };
        var deletedIntegration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            ProfileId = profile.Id,
            BrandId = brand.Id,
            SocialAccountId = activeAccount.Id,
            Platform = SocialPlatformEnum.Facebook,
            ExternalId = "page-deleted",
            AccessToken = "page-token",
            IsDeleted = true,
            IsActive = false
        };
        var post = new Post
        {
            Id = Guid.NewGuid(),
            ContentId = content.Id,
            Content = content,
            IntegrationId = activeIntegration.Id,
            Integration = activeIntegration,
            PublishedAt = DateTime.UtcNow,
            Status = ContentStatusEnum.Published
        };

        context.Users.Add(user);
        context.Profiles.Add(profile);
        context.Brands.Add(brand);
        context.Contents.Add(content);
        context.SocialAccounts.AddRange(activeAccount, deletedAccount);
        context.SocialIntegrations.AddRange(activeIntegration, deletedIntegration);
        context.Posts.Add(post);
        context.SaveChanges();

        return new RepositoryFixture(profile, brand, content, activeAccount, activeIntegration, post);
    }

    private sealed record RepositoryFixture(
        Profile Profile,
        Brand Brand,
        Content Content,
        SocialAccount ActiveAccount,
        SocialIntegration ActiveIntegration,
        Post Post);
}
