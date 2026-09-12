using AISAM.Repositories;
using AISAM.Data.Enumeration;
using AISAM.Services.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AISAM.Services.Service;
public sealed class OrphanAssetCleanup(AisamContext db,IMediaStorageService storage)
{
    public async Task<int> RunAsync(DateTime now,CancellationToken ct)
    {
        var cutoff=now.AddHours(-24);
        // Serializable attachment transactions conflict with expiration. Expire
        // first, commit, then delete remotely: tombstones forbid future reuse.
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{
            await using var tx=db.Database.IsRelational()?await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct):null;
            var candidates=db.Database.IsNpgsql() ? db.Assets.FromSqlInterpolated($"""
                SELECT a.* FROM assets a WHERE a.workspace_id IS NOT NULL AND a.brand_id IS NOT NULL
                AND a.provider_public_id IS NOT NULL AND a.expired_at IS NULL AND a.created_at < {cutoff}
                AND NOT EXISTS(SELECT 1 FROM content_media m WHERE m.asset_id=a.id)
                AND NOT EXISTS(SELECT 1 FROM snapshot_media m WHERE m.asset_id=a.id OR m.url=a.storage_path)
                AND NOT EXISTS(SELECT 1 FROM contents c WHERE strpos(COALESCE(c.image_url::text,''),a.storage_path)>0 OR c.video_url=a.storage_path)
                """).IgnoreQueryFilters() : db.Assets.IgnoreQueryFilters().Where(a=>a.WorkspaceId!=null && a.BrandId!=null && a.ProviderPublicId!=null && a.ExpiredAt==null && a.CreatedAt<cutoff &&
                !db.ContentMedia.IgnoreQueryFilters().Any(m=>m.AssetId==a.Id) && !db.SnapshotMedia.IgnoreQueryFilters().Any(m=>m.AssetId==a.Id || m.Url==a.StoragePath) &&
                !db.Contents.IgnoreQueryFilters().Any(c=>c.ImageUrl!=null && c.ImageUrl.Contains(a.StoragePath) || c.VideoUrl==a.StoragePath));
            var assets=await candidates.OrderBy(a=>a.CreatedAt).Take(50).ToListAsync(ct);
            foreach(var asset in assets)asset.ExpiredAt=now;
            await db.SaveChangesAsync(ct);if(tx is not null)await tx.CommitAsync(ct);
        });
        var expired=await db.Assets.IgnoreQueryFilters().Where(a=>a.ExpiredAt!=null && a.StorageDeletedAt==null && a.ProviderPublicId!=null).Take(50).ToListAsync(ct);
        var count=0;
        foreach(var asset in expired)
        {
            if(await storage.DeleteAsync(asset.ProviderPublicId!,asset.AssetType==AssetTypeEnum.Video,ct)) {asset.StorageDeletedAt=now;count++;}
        }
        await db.SaveChangesAsync(ct);return count;
    }
}
public sealed class OrphanAssetCleanupWorker(IServiceScopeFactory factory,ILogger<OrphanAssetCleanupWorker> logger):BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var timer=new PeriodicTimer(TimeSpan.FromHours(1));
        while(await timer.WaitForNextTickAsync(ct))
        {
            try{using var scope=factory.CreateScope();await scope.ServiceProvider.GetRequiredService<OrphanAssetCleanup>().RunAsync(DateTime.UtcNow,ct);}
            catch(OperationCanceledException)when(ct.IsCancellationRequested){return;}
            catch(Exception){logger.LogWarning("Orphan asset cleanup did not complete; will retry next hour.");}
        }
    }
}
