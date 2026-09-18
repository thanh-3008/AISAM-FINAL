using System.Reflection;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AISAM.IntegrationTests;

public class VideoMetadataSnapshotTests
{
    public class DummyProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var returnType = targetMethod?.ReturnType;
            if (returnType == null || returnType == typeof(void)) return null;
            if (returnType == typeof(Task)) return Task.CompletedTask;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var innerType = returnType.GetGenericArguments()[0];
                var defaultVal = innerType.IsValueType ? Activator.CreateInstance(innerType) : null;
                return typeof(Task).GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(innerType)
                    .Invoke(null, [defaultVal]);
            }
            return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
        }

        public static T Create<T>() where T : class => DispatchProxy.Create<T, DummyProxy>();
    }

    private sealed class FakeMediaStorage : IMediaStorageService
    {
        public Task<string> UploadAsync(IFormFile file, string folder, string fileName, CancellationToken cancellationToken = default)
            => Task.FromResult($"https://res.cloudinary.com/demo/video/upload/v1/{folder}/{fileName}");

        public Task<string> UploadBytesAsync(byte[] data, string folder, string fileName, CancellationToken cancellationToken = default)
            => Task.FromResult($"https://res.cloudinary.com/demo/video/upload/v1/{folder}/{fileName}");

        public Task<StoredMedia?> GetMediaMetadataAsync(string url, CancellationToken cancellationToken = default)
        {
            if (url.Contains("ai-video-facebook-test"))
            {
                return Task.FromResult<StoredMedia?>(new StoredMedia(
                    Url: url,
                    Width: 576,
                    Height: 1024,
                    DurationSeconds: 8.732m,
                    PublicId: "ai-videos/ai-video-facebook-test",
                    SizeBytes: 886797L));
            }
            return Task.FromResult<StoredMedia?>(null);
        }
    }

    [Fact]
    public void StoredMedia_CarriesAllMetadataFields()
    {
        var media = new StoredMedia(
            Url: "https://res.cloudinary.com/demo/video/upload/v1/test.mp4",
            Width: 1080,
            Height: 1920,
            DurationSeconds: 15.5m,
            PublicId: "test",
            SizeBytes: 2048576L);

        Assert.Equal("https://res.cloudinary.com/demo/video/upload/v1/test.mp4", media.Url);
        Assert.Equal(1080, media.Width);
        Assert.Equal(1920, media.Height);
        Assert.Equal(15.5m, media.DurationSeconds);
        Assert.Equal("test", media.PublicId);
        Assert.Equal(2048576L, media.SizeBytes);
    }

    [Fact]
    public async Task CaptureSnapshotAsync_WithRegisteredMetadata_PopulatesVideoSnapshotMedia()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        var videoUrl = "https://res.cloudinary.com/demo/video/upload/v1/ai-videos/ai-video-facebook-test.mp4";
        var content = new Content
        {
            WorkspaceId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Title = "Test Facebook Video",
            TextContent = "Exciting product launch!",
            VideoUrl = videoUrl,
            Status = ContentStatusEnum.Draft
        };
        db.Contents.Add(content);
        await db.SaveChangesAsync();

        // Register resolved metadata into the context
        db.RegisterResolvedMediaMetadata(videoUrl, new StoredMedia(
            Url: videoUrl,
            Width: 576,
            Height: 1024,
            DurationSeconds: 8.732m,
            PublicId: "ai-videos/ai-video-facebook-test",
            SizeBytes: 886797L));

        var snapshot = await db.CaptureSnapshotAsync(content, default);
        await db.SaveChangesAsync();

        Assert.NotNull(snapshot);
        var media = Assert.Single(snapshot.Media);
        Assert.Equal(videoUrl, media.Url);
        Assert.Equal("video/mp4", media.MimeType);
        Assert.Equal(8.732m, media.DurationSeconds);
        Assert.Equal(886797L, media.SizeBytes);
        Assert.Equal(576, media.Width);
        Assert.Equal(1024, media.Height);
    }

    [Fact]
    public async Task CaptureSnapshotAsync_WithoutRegisteredMetadata_PreservesLegacyFallback()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        var videoUrl = "https://res.cloudinary.com/demo/video/upload/v1/legacy-video.mp4";
        var content = new Content
        {
            WorkspaceId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Title = "Legacy Video",
            TextContent = "Old post",
            VideoUrl = videoUrl,
            Status = ContentStatusEnum.Draft
        };
        db.Contents.Add(content);
        await db.SaveChangesAsync();

        // No metadata registered
        var snapshot = await db.CaptureSnapshotAsync(content, default);
        await db.SaveChangesAsync();

        Assert.NotNull(snapshot);
        var media = Assert.Single(snapshot.Media);
        Assert.Equal(videoUrl, media.Url);
        Assert.Equal("video/legacy", media.MimeType);
        Assert.Null(media.DurationSeconds);
        Assert.Null(media.SizeBytes);
    }

    [Fact]
    public async Task ImageTextContent_CapturesSnapshot_WithoutRegression()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        var imageUrl = "https://res.cloudinary.com/demo/image/upload/v1/sample.jpg";
        var content = new Content
        {
            WorkspaceId = Guid.NewGuid(),
            BrandId = Guid.NewGuid(),
            Title = "Image Post",
            TextContent = "Photo update",
            ImageUrl = imageUrl,
            Status = ContentStatusEnum.Draft
        };
        db.Contents.Add(content);
        await db.SaveChangesAsync();

        var snapshot = await db.CaptureSnapshotAsync(content, default);
        await db.SaveChangesAsync();

        Assert.NotNull(snapshot);
        var media = Assert.Single(snapshot.Media);
        Assert.Equal(imageUrl, media.Url);
        Assert.Equal("image/legacy", media.MimeType);
        Assert.Null(media.DurationSeconds);
    }

    [Fact]
    public async Task AdminReFreezeSnapshotAsync_CreatesNewSnapshotAndFixesPublishingValidation()
    {
        await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        var videoUrl = "https://res.cloudinary.com/demo/video/upload/v1/ai-videos/ai-video-facebook-test.mp4";
        var workspaceId = Guid.NewGuid();
        var brandId = Guid.NewGuid();

        var content = new Content
        {
            WorkspaceId = workspaceId,
            BrandId = brandId,
            Title = "Video Stuck In Review",
            TextContent = "Publishing blocked by MEDIA_METADATA_REQUIRED",
            VideoUrl = videoUrl,
            Status = ContentStatusEnum.Draft
        };
        db.Contents.Add(content);
        await db.SaveChangesAsync();

        // 1. Initial snapshot captured without metadata (simulates legacy bug state)
        var oldSnapshot = await db.CaptureSnapshotAsync(content, default);
        content.SubmittedSnapshotId = oldSnapshot.Id;
        content.ApprovedSnapshotId = oldSnapshot.Id;
        content.Status = ContentStatusEnum.Approved;
        await db.SaveChangesAsync();

        // Verify the bug reproduces on the old snapshot
        var fbIntegration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            BrandId = brandId,
            Platform = SocialPlatformEnum.Facebook,
            AccessToken = "valid-token",
            IsActive = true,
            SocialAccount = new SocialAccount { UserAccessToken = "valid-token", IsActive = true }
        };
        var igIntegration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            BrandId = brandId,
            Platform = SocialPlatformEnum.Instagram,
            AccessToken = "valid-token",
            IsActive = true,
            SocialAccount = new SocialAccount { UserAccessToken = "valid-token", IsActive = true }
        };
        var ttIntegration = new SocialIntegration
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            BrandId = brandId,
            Platform = SocialPlatformEnum.TikTok,
            AccessToken = "valid-token",
            IsActive = true,
            SocialAccount = new SocialAccount { UserAccessToken = "valid-token", IsActive = true }
        };

        var fbCapability = PublishingCapabilities.For(fbIntegration, fbIntegration.SocialAccount, DateTime.UtcNow);
        var igCapability = PublishingCapabilities.For(igIntegration, igIntegration.SocialAccount, DateTime.UtcNow);
        var ttCapability = PublishingCapabilities.For(ttIntegration, ttIntegration.SocialAccount, DateTime.UtcNow);

        // Before fix: validation fails on old snapshot
        var oldMedia = await db.SnapshotMedia.Where(m => m.SnapshotId == oldSnapshot.Id).ToListAsync();
        Assert.Equal("MEDIA_METADATA_REQUIRED", PublishingCapabilities.Validate(fbCapability, oldMedia));
        Assert.Equal("MEDIA_METADATA_REQUIRED", PublishingCapabilities.Validate(igCapability, oldMedia));
        Assert.Equal("MEDIA_METADATA_REQUIRED", PublishingCapabilities.Validate(ttCapability, oldMedia));

        // 2. Perform AdminReFreezeSnapshotAsync
        var storage = new FakeMediaStorage();
        var contentService = new ContentService(
            DummyProxy.Create<IContentRepository>(),
            DummyProxy.Create<IBrandRepository>(),
            DummyProxy.Create<IProductRepository>(),
            DummyProxy.Create<ISocialIntegrationRepository>(),
            DummyProxy.Create<ISocialAccountRepository>(),
            DummyProxy.Create<IPostRepository>(),
            [],
            DummyProxy.Create<ISocialTokenProtector>(),
            DummyProxy.Create<IQuotaService>(),
            DummyProxy.Create<IContentCalendarRepository>(),
            DummyProxy.Create<IWorkspaceRepository>(),
            context: db,
            mediaStorageService: storage);

        var result = await contentService.AdminReFreezeSnapshotAsync(content.Id, default);
        Assert.True(result.Success);

        // 3. Verify immutability: both snapshots exist, old snapshot is NOT mutated
        var allSnapshots = await db.PublishSnapshots.Include(s => s.Media).OrderBy(s => s.CreatedAt).ToListAsync();
        Assert.Equal(2, allSnapshots.Count);

        var initialSnap = allSnapshots[0];
        Assert.Equal(oldSnapshot.Id, initialSnap.Id);
        Assert.Equal("video/legacy", initialSnap.Media.Single().MimeType);
        Assert.Null(initialSnap.Media.Single().DurationSeconds);

        // 4. Verify new snapshot has rich metadata and content points to it
        var newSnap = allSnapshots[1];
        Assert.NotEqual(oldSnapshot.Id, newSnap.Id);
        Assert.Equal(content.ApprovedSnapshotId, newSnap.Id);

        var newMedia = newSnap.Media.Single();
        Assert.Equal("video/mp4", newMedia.MimeType);
        Assert.Equal(8.732m, newMedia.DurationSeconds);
        Assert.Equal(886797L, newMedia.SizeBytes);
        Assert.Equal(576, newMedia.Width);
        Assert.Equal(1024, newMedia.Height);

        // 5. Verify PublishingCapabilities.Validate now SUCCEEDS across all platforms!
        var newMediaList = await db.SnapshotMedia.Where(m => m.SnapshotId == newSnap.Id).ToListAsync();
        Assert.Null(PublishingCapabilities.Validate(fbCapability, newMediaList));
        Assert.Null(PublishingCapabilities.Validate(igCapability, newMediaList));
        Assert.Null(PublishingCapabilities.Validate(ttCapability, newMediaList));
    }
}
