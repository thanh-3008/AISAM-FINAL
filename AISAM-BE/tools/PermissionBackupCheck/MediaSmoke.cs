using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.Access;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;

internal static class MediaSmoke
{
    private sealed class Storage : IMediaStorageService
    {
        public Task<string> UploadAsync(IFormFile f,string folder,string name,CancellationToken ct=default)=>throw new NotSupportedException();
        public Task<string> UploadBytesAsync(byte[] b,string folder,string name,CancellationToken ct=default)=>throw new NotSupportedException();
        public Task<bool> DeleteAsync(string id,bool video,CancellationToken ct=default)=>Task.FromResult(true);
    }
    public static async Task Run(AisamContext db,Guid actor,Guid workspace,Guid brand)
    {
        db.ChangeTracker.Clear();db.PermissionScopeEnabled=false;db.PermissionOwner=true;
        var profile=await db.Brands.Where(b=>b.Id==brand).Select(b=>b.ProfileId).SingleAsync();
        var team=await db.TeamBrands.Where(t=>t.BrandId==brand).Select(t=>t.TeamId).FirstAsync();
        var content=new Content{ProfileId=profile,WorkspaceId=workspace,BrandId=brand,TeamId=team,PrimaryCreatorId=actor,TextContent="frozen original"};
        var assets=Enumerable.Range(0,5).Select(i=>new Asset{Id=Guid.NewGuid(),WorkspaceId=workspace,BrandId=brand,UploadedBy=actor,
            StoragePath="https://media.test/"+Guid.NewGuid(),AssetType=AssetTypeEnum.Image,MimeType="image/png",ProviderPublicId="smoke/"+Guid.NewGuid(),CreatedAt=DateTime.UtcNow.AddDays(-3)}).ToArray();
        db.Add(content);db.AddRange(assets);await db.SaveChangesAsync();
        var service=new ContentMediaService(db,new AccessControlService(db),new Storage());
        var order=assets.Reverse().Select((a,i)=>new MediaItemRequest(a.Id,i,i==0)).ToArray();
        await service.ReplaceAsync(actor,workspace,content.Id,content.MediaVersion,order,default);
        db.ChangeTracker.Clear();content=await db.Contents.SingleAsync(c=>c.Id==content.Id);
        var stored=await db.ContentMedia.Where(m=>m.ContentId==content.Id).OrderBy(m=>m.SortOrder).Select(m=>m.AssetId).ToArrayAsync();
        if(!stored.SequenceEqual(order.Select(i=>i.AssetId)))throw new Exception("Media order lost.");
        content.Status=ContentStatusEnum.PendingApproval;await db.SaveChangesAsync();
        content.Status=ContentStatusEnum.Approved;await db.SaveChangesAsync();
        var frozen=await db.PublishSnapshots.Include(s=>s.Media).SingleAsync(s=>s.Id==content.ApprovedSnapshotId);
        if(frozen.Media.Count!=5)throw new Exception("Incomplete snapshot.");
        content.TextContent="edited draft";await db.SaveChangesAsync();
        await service.ReplaceAsync(actor,workspace,content.Id,content.MediaVersion,[],default);
        if(!frozen.Payload.Contains("frozen original")||content.ApprovedSnapshotId!=null)throw new Exception("Snapshot changed with draft.");
        await new OrphanAssetCleanup(db,new Storage()).RunAsync(DateTime.UtcNow,default);
        if(await db.Assets.AnyAsync(a=>assets.Select(x=>x.Id).Contains(a.Id)&&a.ExpiredAt!=null))throw new Exception("Frozen asset expired.");
        await using(var tx=await db.Database.BeginTransactionAsync())
        {
            await tx.CreateSavepointAsync("immutable");bool denied=false;
            try{await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE publish_snapshots SET payload='{{}}'::jsonb WHERE id={frozen.Id}");}
            catch(PostgresException e)when(e.SqlState=="23514"){denied=true;}
            await tx.RollbackToSavepointAsync("immutable");if(!denied)throw new Exception("SQL snapshot mutation accepted.");
            denied=false;
            try{await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE assets SET expired_at=now() WHERE id={assets[0].Id}");}
            catch(PostgresException e)when(e.SqlState=="23514"){denied=true;}
            await tx.RollbackAsync();if(!denied)throw new Exception("Referenced asset expiration accepted.");
        }
        Console.WriteLine("PASS PostgreSQL five-image reorder/reload, snapshot freeze, cleanup references and SQL immutability");
    }
}
