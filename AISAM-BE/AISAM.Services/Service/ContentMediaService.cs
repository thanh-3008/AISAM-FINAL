using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using AISAM.Repositories;
using AISAM.Services.Access;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AISAM.Services.Service;
public sealed record MediaItemRequest(Guid AssetId,int SortOrder,bool IsCover=false,string? AltText=null,string? Caption=null);
public sealed record MediaCollection(Guid Version,IReadOnlyList<object> Items);
public sealed record UploadItemResult(int Index,Guid? AssetId,string? Url,string? Error);
public sealed class MediaConflictException:Exception;
public sealed class ContentMediaService(AisamContext db,IAccessControlService access,IMediaStorageService storage)
{
    public async Task<MediaCollection> ImportLegacyAsync(Guid actor,Guid workspace,Guid contentId,Guid expectedVersion,CancellationToken ct)
    {
        await Require(actor,workspace,contentId,ResourcePermission.ContentEdit,ct);
        var content=await db.Contents.SingleAsync(c=>c.Id==contentId,ct);
        if(content.MediaVersion!=expectedVersion || await db.ContentMedia.AnyAsync(m=>m.ContentId==contentId,ct))throw new MediaConflictException();
        var urls=new List<(string Url,AssetTypeEnum Type)>();
        if(!string.IsNullOrWhiteSpace(content.ImageUrl))
        {
            try{using var json=JsonDocument.Parse(content.ImageUrl);if(json.RootElement.ValueKind==JsonValueKind.Array)urls.AddRange(json.RootElement.EnumerateArray().Where(e=>e.ValueKind==JsonValueKind.String).Select(e=>(e.GetString()!,AssetTypeEnum.Image)));else if(json.RootElement.ValueKind==JsonValueKind.String)urls.Add((json.RootElement.GetString()!,AssetTypeEnum.Image));}
            catch(JsonException){urls.Add((content.ImageUrl,AssetTypeEnum.Image));}
        }
        if(!string.IsNullOrWhiteSpace(content.VideoUrl))urls.Add((content.VideoUrl,AssetTypeEnum.Video));
        if(urls.Count>10 || urls.Any(u=>!Uri.TryCreate(u.Url,UriKind.Absolute,out var uri) || uri.Scheme is not ("http" or "https")))throw new ArgumentException("Legacy media contains unsupported URLs; repair the draft first.");
        var items=new List<MediaItemRequest>();
        foreach(var value in urls.Distinct())
        {
            // Import only URLs already on this authorized content. No arbitrary
            // client URL import and no remote fetch. Metadata remains unknown.
            var asset=new Asset {Id=Guid.NewGuid(),WorkspaceId=workspace,BrandId=content.BrandId,UploadedBy=actor,StoragePath=value.Url,
                AssetType=value.Type,MimeType=value.Type==AssetTypeEnum.Image?"image/legacy":"video/legacy",Metadata="{\"legacy\":true}"};
            db.Assets.Add(asset);items.Add(new(asset.Id,items.Count,items.Count==0));
        }
        await db.SaveChangesAsync(ct);
        return await ReplaceAsync(actor,workspace,contentId,expectedVersion,items,ct);
    }
    private async Task Require(Guid actor,Guid workspace,Guid contentId,ResourcePermission permission,CancellationToken ct)
    {if(!(await access.CheckAsync(new(actor,workspace,AccessResourceKind.Content,contentId,permission),ct)).Allowed)throw new ResourceMutationDeniedException();}
    public async Task<MediaCollection> ReadAsync(Guid actor,Guid workspace,Guid contentId,CancellationToken ct)
    {
        await Require(actor,workspace,contentId,ResourcePermission.ContentView,ct);
        var c=await db.Contents.AsNoTracking().SingleAsync(c=>c.Id==contentId,ct);
        var rows=await db.ContentMedia.AsNoTracking().Include(m=>m.Asset).Where(m=>m.ContentId==contentId).OrderBy(m=>m.SortOrder).ToListAsync(ct);
        return new(c.MediaVersion,rows.Select(m=>(object)new {m.AssetId,m.SortOrder,m.IsCover,m.AltText,m.Caption,url=m.Asset.StoragePath,mimeType=m.Asset.MimeType,sizeBytes=m.Asset.SizeBytes,m.Asset.DurationSeconds,m.Asset.Width,m.Asset.Height}).ToList());
    }
    public async Task<IReadOnlyList<UploadItemResult>> UploadAsync(Guid actor,Guid workspace,Guid contentId,IReadOnlyList<IFormFile> files,CancellationToken ct)
    {
        await Require(actor,workspace,contentId,ResourcePermission.ContentEdit,ct);
        if(files.Count is <1 or >10 || files.Sum(f=>f.Length)>200L*1024*1024)throw new ArgumentException("Upload 1–10 files, maximum 200MB total and 50MB per file.");
        var content=await db.Contents.AsNoTracking().SingleAsync(c=>c.Id==contentId,ct);
        var results=new List<UploadItemResult>();
        for(int index=0;index<files.Count;index++)
        {
            var file=files[index];
            try
            {
                var extension=await ValidateFileAsync(file,ct);
                await Require(actor,workspace,contentId,ResourcePermission.ContentEdit,ct);
                var id=Guid.NewGuid();var folder=$"content/{workspace:N}/assets";var name=$"{id:N}{extension}";
                await using var stream=file.OpenReadStream();var hash=Convert.ToHexString(await SHA256.HashDataAsync(stream,ct));
                var stored=await storage.UploadDetailedAsync(file,folder,name,ct);var url=stored.Url;
                var asset=new Asset {Id=id,WorkspaceId=workspace,BrandId=content.BrandId,UploadedBy=actor,StoragePath=url,
                    AssetType=file.ContentType.StartsWith("video/",StringComparison.OrdinalIgnoreCase)?AssetTypeEnum.Video:AssetTypeEnum.Image,MimeType=file.ContentType.ToLowerInvariant(),SizeBytes=file.Length,Sha256=hash,
                    Width=stored.Width,Height=stored.Height,DurationSeconds=stored.DurationSeconds,
                    ProviderPublicId=stored.PublicId??$"{folder}/{id:N}"};
                db.Assets.Add(asset);await db.SaveChangesAsync(ct);
                results.Add(new(index,id,url,null));
            }
            catch(ArgumentException e){results.Add(new(index,null,null,e.Message));}
            catch(OperationCanceledException){throw;}
            catch(ResourceMutationDeniedException){throw;}
            catch {foreach(var entry in db.ChangeTracker.Entries<Asset>().Where(e=>e.State==EntityState.Added).ToList())entry.State=EntityState.Detached;results.Add(new(index,null,null,"Storage upload failed; retry this item."));}
        }
        return results;
    }
    public static async Task<string> ValidateFileAsync(IFormFile file,CancellationToken ct)
    {
        if(file.Length<=0 || file.Length>50L*1024*1024)throw new ArgumentException("File must be between 1 byte and 50MB.");
        await using var stream=file.OpenReadStream();var bytes=new byte[32];var length=await stream.ReadAtLeastAsync(bytes,12,false,ct);
        bool Match(params byte[] signature)=>length>=signature.Length && bytes.AsSpan(0,signature.Length).SequenceEqual(signature);
        var extension=file.ContentType.ToLowerInvariant() switch {
            "image/jpeg" when Match(0xff,0xd8,0xff)=>".jpg",
            "image/png" when Match(137,80,78,71,13,10,26,10)=>".png",
            "image/gif" when length>=6 && (Encoding.ASCII.GetString(bytes,0,6) is "GIF87a" or "GIF89a")=>".gif",
            "image/webp" when length>=12 && Encoding.ASCII.GetString(bytes,0,4)=="RIFF" && Encoding.ASCII.GetString(bytes,8,4)=="WEBP"=>".webp",
            "video/mp4" when length>=12 && Encoding.ASCII.GetString(bytes,4,4)=="ftyp"=>".mp4",
            "video/quicktime" when length>=12 && Encoding.ASCII.GetString(bytes,4,4)=="ftyp" && Encoding.ASCII.GetString(bytes,8,4)=="qt  "=>".mov",
            "video/webm" when Match(0x1a,0x45,0xdf,0xa3)=>".webm",
            _=>throw new ArgumentException("File signature does not match a supported image/video MIME type.")};
        return extension;
    }
    public async Task<MediaCollection> ReplaceAsync(Guid actor,Guid workspace,Guid contentId,Guid expectedVersion,IReadOnlyList<MediaItemRequest> items,CancellationToken ct)
    {
        await Require(actor,workspace,contentId,ResourcePermission.ContentEdit,ct);
        if(items.Count>10 || items.Select(i=>i.AssetId).Distinct().Count()!=items.Count || !items.Select(i=>i.SortOrder).Order().SequenceEqual(Enumerable.Range(0,items.Count)) ||
            items.Count(i=>i.IsCover)>1 || items.Any(i=>i.AltText?.Length>1000 || i.Caption?.Length>2000))throw new ArgumentException("Use at most 10 distinct assets with contiguous order 0..N-1 and one cover; alt text <=1000, caption <=2000.");
        var strategy=db.Database.CreateExecutionStrategy();bool attempted=false;
        await strategy.ExecuteAsync(async()=>{
            if(attempted)throw new MediaConflictException();attempted=true;
            await using var tx=db.Database.IsRelational()?await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct):null;
            var content=await db.Contents.SingleAsync(c=>c.Id==contentId,ct);
            if(content.MediaVersion!=expectedVersion)throw new MediaConflictException();
            var ids=items.Select(i=>i.AssetId).ToArray();
            var assets=await db.Assets.IgnoreQueryFilters().Where(a=>ids.Contains(a.Id) && a.WorkspaceId==workspace && a.BrandId==content.BrandId && a.ExpiredAt==null).ToListAsync(ct);
            if(assets.Count!=ids.Length || assets.Any(a=>a.UploadedBy!=actor && !db.PermissionOwner && !db.PermissionManager))throw new ResourceMutationDeniedException();
            db.ContentMedia.RemoveRange(await db.ContentMedia.Where(m=>m.ContentId==contentId).ToListAsync(ct));
            // Delete before insertion avoids transient unique order collisions.
            await db.SaveChangesAsync(ct);
            foreach(var item in items)db.ContentMedia.Add(new ContentMedia {ContentId=contentId,AssetId=item.AssetId,SortOrder=item.SortOrder,IsCover=item.IsCover,AltText=item.AltText,Caption=item.Caption});
            var ordered=items.OrderBy(i=>i.SortOrder).Select(i=>assets.Single(a=>a.Id==i.AssetId)).ToList();
            content.ImageUrl=JsonSerializer.Serialize(ordered.Where(a=>a.AssetType==AssetTypeEnum.Image).Select(a=>a.StoragePath));
            content.VideoUrl=ordered.FirstOrDefault(a=>a.AssetType==AssetTypeEnum.Video)?.StoragePath;
            content.MediaVersion=Guid.NewGuid();content.SubmittedSnapshotId=null;content.ApprovedSnapshotId=null;content.Status=ContentStatusEnum.Draft;
            await db.SaveChangesAsync(ct);if(tx is not null)await tx.CommitAsync(ct);
        });
        return await ReadAsync(actor,workspace,contentId,ct);
    }
}
