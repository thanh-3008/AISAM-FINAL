using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.Access;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AISAM.IntegrationTests;
public class ContentMediaTests
{
    private sealed class Access:IAccessControlService
    {
        public Task<AccessDecision> CheckAsync(AccessRequest r,CancellationToken ct=default)=>Task.FromResult(AccessDecision.Permit);
        public Task<IReadOnlyList<Guid>> GetAccessibleBrandIdsAsync(Guid a,Guid w,CancellationToken ct=default)=>throw new NotSupportedException();
    }
    private sealed class Storage:IMediaStorageService
    {
        public int Uploads,Deletes;
        public Task<string> UploadAsync(IFormFile file,string folder,string name,CancellationToken ct=default){Uploads++;return Task.FromResult("https://storage.test/"+name);}
        public async Task<StoredMedia> UploadDetailedAsync(IFormFile file,string folder,string name,CancellationToken ct=default)=>new(await UploadAsync(file,folder,name,ct),640,480,12,"verified-public-id");
        public Task<string> UploadBytesAsync(byte[] data,string folder,string name,CancellationToken ct=default)=>throw new NotSupportedException();
        public Task<bool> DeleteAsync(string id,bool video,CancellationToken ct=default){Deletes++;return Task.FromResult(true);}
    }
    private static IFormFile File(byte[] bytes,string mime)=>new FormFile(new MemoryStream(bytes),0,bytes.Length,"files","untrusted.exe"){Headers=new HeaderDictionary(),ContentType=mime};
    [Fact]
    public async Task UploadHasPerItemValidationAndDoesNotTrustExtension()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var c=new Content{WorkspaceId=Guid.NewGuid(),BrandId=Guid.NewGuid()};db.Add(c);await db.SaveChangesAsync();
        var storage=new Storage();var service=new ContentMediaService(db,new Access(),storage);
        var result=await service.UploadAsync(Guid.NewGuid(),c.WorkspaceId,c.Id,[File([137,80,78,71,13,10,26,10,0,0,0,0],"image/png"),File([1,2,3,4,5,6,7,8,9,0,0,0],"image/png")],default);
        Assert.NotNull(result[0].AssetId);Assert.EndsWith(".png",result[0].Url);Assert.Null(result[1].AssetId);Assert.NotNull(result[1].Error);Assert.Equal(1,storage.Uploads);
        var asset=await db.Assets.SingleAsync();Assert.Equal(c.WorkspaceId,asset.WorkspaceId);Assert.Equal(c.BrandId,asset.BrandId);Assert.Equal(64,asset.Sha256!.Length);
        Assert.Equal(640,asset.Width);Assert.Equal(480,asset.Height);Assert.Equal(12,asset.DurationSeconds);Assert.Equal("verified-public-id",asset.ProviderPublicId);
    }
    [Fact]
    public async Task LegacyImportPreservesOrderWithoutDownloadingAndRejectsRepeatedImport()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var actor=Guid.NewGuid();var c=new Content{WorkspaceId=Guid.NewGuid(),BrandId=Guid.NewGuid(),ImageUrl="[\"https://old.test/2\",\"https://old.test/1\"]"};
        db.Add(c);await db.SaveChangesAsync();var storage=new Storage();var service=new ContentMediaService(db,new Access(),storage);
        await service.ImportLegacyAsync(actor,c.WorkspaceId,c.Id,c.MediaVersion,default);
        var rows=await db.ContentMedia.Include(m=>m.Asset).OrderBy(m=>m.SortOrder).ToListAsync();
        Assert.Equal(new[]{"https://old.test/2","https://old.test/1"},rows.Select(m=>m.Asset.StoragePath));Assert.Equal(0,storage.Uploads);
        Assert.All(rows,m=>{Assert.Equal(actor,m.Asset.UploadedBy);Assert.Null(m.Asset.ProviderPublicId);});
        await Assert.ThrowsAsync<MediaConflictException>(()=>service.ImportLegacyAsync(actor,c.WorkspaceId,c.Id,c.MediaVersion,default));
    }
    [Fact]
    public async Task OrderSnapshotAndCleanupRemainSafeAcrossDraftEdits()
    {
        await using var db=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var actor=Guid.NewGuid();var w=Guid.NewGuid();var b=Guid.NewGuid();var c=new Content{WorkspaceId=w,BrandId=b,TextContent="approved words"};
        var a=new Asset{Id=Guid.NewGuid(),WorkspaceId=w,BrandId=b,UploadedBy=actor,StoragePath="https://a",MimeType="image/png",AssetType=AssetTypeEnum.Image,CreatedAt=DateTime.UtcNow.AddDays(-3),ProviderPublicId="a",Width=640,Height=480};
        var second=new Asset{Id=Guid.NewGuid(),WorkspaceId=w,BrandId=b,UploadedBy=actor,StoragePath="https://b",MimeType="image/png",AssetType=AssetTypeEnum.Image};
        var other=new Asset{Id=Guid.NewGuid(),WorkspaceId=Guid.NewGuid(),BrandId=b,UploadedBy=actor,StoragePath="https://other"};
        var orphan=new Asset{Id=Guid.NewGuid(),WorkspaceId=w,BrandId=b,UploadedBy=actor,StoragePath="https://orphan",CreatedAt=DateTime.UtcNow.AddDays(-3),ProviderPublicId="orphan"};
        db.AddRange(c,a,second,other,orphan);await db.SaveChangesAsync();
        var storage=new Storage();var service=new ContentMediaService(db,new Access(),storage);
        await service.ReplaceAsync(actor,w,c.Id,c.MediaVersion,[new(second.Id,1),new(a.Id,0,true,"cover")],default);
        var original=c.MediaVersion;
        db.ChangeTracker.Clear();c=await db.Contents.SingleAsync();
        Assert.Equal(a.Id,(await db.ContentMedia.OrderBy(m=>m.SortOrder).FirstAsync()).AssetId);
        await Assert.ThrowsAsync<MediaConflictException>(()=>service.ReplaceAsync(actor,w,c.Id,Guid.NewGuid(),[],default));
        await Assert.ThrowsAsync<ResourceMutationDeniedException>(()=>service.ReplaceAsync(actor,w,c.Id,original,[new(other.Id,0)],default));
        c.Status=ContentStatusEnum.PendingApproval;await db.SaveChangesAsync();
        var snapshotId=c.SubmittedSnapshotId;Assert.NotNull(snapshotId);
        c.Status=ContentStatusEnum.Approved;await db.SaveChangesAsync();Assert.Equal(snapshotId,c.ApprovedSnapshotId);
        var schedule=new ContentCalendar{ContentId=c.Id,WorkspaceId=w};db.Add(schedule);await db.SaveChangesAsync();Assert.Equal(snapshotId,schedule.SnapshotId);
        c.TextContent="changed draft";await db.SaveChangesAsync();Assert.Equal(ContentStatusEnum.Draft,c.Status);Assert.Null(c.ApprovedSnapshotId);
        await service.ReplaceAsync(actor,w,c.Id,c.MediaVersion,[],default);
        var frozen=await db.PublishSnapshots.Include(s=>s.Media).SingleAsync();Assert.Contains("approved words",frozen.Payload);Assert.Equal(2,frozen.Media.Count);Assert.Equal(snapshotId,schedule.SnapshotId);
        Assert.Equal(640,frozen.Media.Single(m=>m.AssetId==a.Id).Width);
        Assert.Equal(1,await new OrphanAssetCleanup(db,storage).RunAsync(DateTime.UtcNow,default));Assert.Equal(1,storage.Deletes);Assert.Null(a.ExpiredAt);
        frozen.Payload="{}";await Assert.ThrowsAsync<InvalidOperationException>(()=>db.SaveChangesAsync());
    }
}
