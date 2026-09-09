using AISAM.Data.Model;
using AISAM.Data.Enumeration;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AISAM.Repositories;
public partial class AisamContext
{
    public DbSet<ContentMedia> ContentMedia => Set<ContentMedia>();
    public DbSet<PublishSnapshot> PublishSnapshots => Set<PublishSnapshot>();
    public DbSet<SnapshotMedia> SnapshotMedia => Set<SnapshotMedia>();
    public DbSet<PostMedia> PostMedia => Set<PostMedia>();
    public Guid? ExecutionSnapshotId {get;set;}
    public DbSet<PublishOperation> PublishOperations=>Set<PublishOperation>();
    public DbSet<PublishRequest> PublishRequests=>Set<PublishRequest>();

    private void ConfigureMedia(ModelBuilder m)
    {
        m.Entity<PublishRequest>().HasIndex(r=>new{r.WorkspaceId,r.ActorId,r.IdempotencyKey}).IsUnique();
        m.Entity<PublishOperation>().HasIndex(o=>new{o.WorkspaceId,o.ActorId,o.IdempotencyKey,o.IntegrationId}).IsUnique();
        m.Entity<PublishOperation>().HasOne(o=>o.Snapshot).WithMany().HasForeignKey(o=>o.SnapshotId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<PublishOperation>().Property(o=>o.Status).IsConcurrencyToken();
        m.Entity<PublishOperation>().HasQueryFilter(o=>!PermissionScopeEnabled||Contents.Any(c=>c.Id==o.ContentId));
        m.Entity<Content>().Property(c=>c.MediaVersion).IsConcurrencyToken();
        m.Entity<ContentMedia>().HasIndex(c=>new{c.ContentId,c.SortOrder}).IsUnique();
        m.Entity<ContentMedia>().HasIndex(c=>new{c.ContentId,c.AssetId}).IsUnique();
        m.Entity<ContentMedia>().HasOne(c=>c.Content).WithMany().HasForeignKey(c=>c.ContentId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<ContentMedia>().HasOne(c=>c.Asset).WithMany().HasForeignKey(c=>c.AssetId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<PublishSnapshot>().HasOne(s=>s.Content).WithMany().HasForeignKey(s=>s.ContentId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<SnapshotMedia>().HasIndex(s=>new{s.SnapshotId,s.SortOrder}).IsUnique();
        m.Entity<SnapshotMedia>().HasOne(s=>s.Snapshot).WithMany(s=>s.Media).HasForeignKey(s=>s.SnapshotId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<SnapshotMedia>().HasOne(s=>s.Asset).WithMany().HasForeignKey(s=>s.AssetId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<PostMedia>().HasOne(s=>s.Post).WithMany().HasForeignKey(s=>s.PostId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<PostMedia>().HasOne(s=>s.SnapshotMedia).WithMany().HasForeignKey(s=>s.SnapshotMediaId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<PostMedia>().HasIndex(s=>new{s.PostId,s.SnapshotMediaId}).IsUnique();
        m.Entity<ContentMedia>().HasQueryFilter(s=>!PermissionScopeEnabled || Contents.Any(c=>c.Id==s.ContentId));
        m.Entity<PublishSnapshot>().HasQueryFilter(s=>!PermissionScopeEnabled || Contents.Any(c=>c.Id==s.ContentId));
        m.Entity<SnapshotMedia>().HasQueryFilter(s=>!PermissionScopeEnabled || PublishSnapshots.Any(p=>p.Id==s.SnapshotId));
        m.Entity<PostMedia>().HasQueryFilter(s=>!PermissionScopeEnabled || Posts.Any(p=>p.Id==s.PostId));
        m.Entity<Asset>().HasIndex(a=>new{a.WorkspaceId,a.BrandId,a.CreatedAt});
    }

    public async Task<PublishSnapshot> CaptureSnapshotAsync(Content content,CancellationToken ct)
    {
        var media=await ContentMedia.IgnoreQueryFilters().Include(m=>m.Asset).Where(m=>m.ContentId==content.Id).OrderBy(m=>m.SortOrder).ToListAsync(ct);
        var snapshot=new PublishSnapshot {ContentId=content.Id,WorkspaceId=content.WorkspaceId,Version=content.MediaVersion,CreatedBy=ExecutionActorId};
        snapshot.Payload=JsonSerializer.Serialize(new {content.Title,content.TextContent,content.ImageUrl,content.VideoUrl,content.AdType,content.Tags,content.BrandId,content.ProductId});
        foreach(var item in media)
        {
            if(item.Asset.ExpiredAt.HasValue || item.Asset.WorkspaceId!=content.WorkspaceId || item.Asset.BrandId!=content.BrandId) throw new ResourceMutationDeniedException();
            snapshot.Media.Add(new SnapshotMedia {SnapshotId=snapshot.Id,AssetId=item.AssetId,SortOrder=item.SortOrder,Url=item.Asset.StoragePath,
                MimeType=item.Asset.MimeType,SizeBytes=item.Asset.SizeBytes,DurationSeconds=item.Asset.DurationSeconds,Width=item.Asset.Width,Height=item.Asset.Height,Checksum=item.Asset.Sha256,IsCover=item.IsCover,AltText=item.AltText,Caption=item.Caption});
        }
        if(media.Count==0)
        {
            // Freeze legacy URLs without inventing ownership or downloading them.
            var urls=new List<(string Url,string Mime)>();
            if(!string.IsNullOrWhiteSpace(content.ImageUrl))
            {
                try {using var j=JsonDocument.Parse(content.ImageUrl);if(j.RootElement.ValueKind==JsonValueKind.Array) urls.AddRange(j.RootElement.EnumerateArray().Where(e=>e.ValueKind==JsonValueKind.String).Select(e=>(e.GetString()!,"image/legacy")));else if(j.RootElement.ValueKind==JsonValueKind.String)urls.Add((j.RootElement.GetString()!,"image/legacy"));}
                catch(JsonException){urls.Add((content.ImageUrl,"image/legacy"));}
            }
            if(!string.IsNullOrWhiteSpace(content.VideoUrl)) urls.Add((content.VideoUrl,"video/legacy"));
            foreach(var url in urls) snapshot.Media.Add(new SnapshotMedia {SnapshotId=snapshot.Id,SortOrder=snapshot.Media.Count,Url=url.Url,MimeType=url.Mime,IsCover=snapshot.Media.Count==0});
        }
        snapshot.Checksum=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(snapshot.Payload+JsonSerializer.Serialize(snapshot.Media.Select(m=>new{m.AssetId,m.SortOrder,m.Url,m.MimeType,m.IsCover,m.AltText,m.Caption,m.Checksum,m.SizeBytes,m.DurationSeconds,m.Width,m.Height})))));
        PublishSnapshots.Add(snapshot);return snapshot;
    }

    private async Task PrepareMediaAsync(CancellationToken ct)
    {
        foreach(var entry in ChangeTracker.Entries().Where(e=>e.State is EntityState.Modified or EntityState.Deleted).ToArray())
            if(entry.Entity is PublishSnapshot or AISAM.Data.Model.SnapshotMedia) throw new InvalidOperationException("Publish snapshots are immutable.");
        foreach(var entry in ChangeTracker.Entries<Asset>().Where(e=>e.State==EntityState.Modified).ToArray())
            if(new[]{nameof(Asset.StoragePath),nameof(Asset.WorkspaceId),nameof(Asset.BrandId),nameof(Asset.UploadedBy),nameof(Asset.Sha256)}.Any(p=>entry.Property(p).IsModified))
                throw new InvalidOperationException("Asset identity and storage bytes are immutable.");
        foreach(var entry in ChangeTracker.Entries<Content>().Where(e=>e.State is EntityState.Modified or EntityState.Added).ToArray())
        {
            var c=entry.Entity;
            if(await Assets.IgnoreQueryFilters().AnyAsync(a=>a.ExpiredAt!=null && (c.ImageUrl!=null && c.ImageUrl.Contains(a.StoragePath) || c.VideoUrl==a.StoragePath),ct))
                throw new ResourceMutationDeniedException();
            bool payloadChanged=entry.State==EntityState.Modified && new[]{nameof(Content.Title),nameof(Content.TextContent),nameof(Content.ImageUrl),nameof(Content.VideoUrl),nameof(Content.AdType),nameof(Content.Tags),nameof(Content.BrandId),nameof(Content.ProductId)}
                .Any(p=>entry.Property(p).IsModified && !Equals(entry.Property(p).OriginalValue,entry.Property(p).CurrentValue));
            bool transitioning=entry.State==EntityState.Added || entry.Property(nameof(Content.Status)).IsModified;
            if(payloadChanged)
            {
                var legacyMediaChanged=new[]{nameof(Content.ImageUrl),nameof(Content.VideoUrl)}.Any(p=>entry.Property(p).IsModified && !Equals(entry.Property(p).OriginalValue,entry.Property(p).CurrentValue));
                if(legacyMediaChanged && !ChangeTracker.Entries<ContentMedia>().Any(m=>m.State==EntityState.Added && m.Entity.ContentId==c.Id))
                    ContentMedia.RemoveRange(await ContentMedia.IgnoreQueryFilters().Where(m=>m.ContentId==c.Id).ToListAsync(ct));
                c.MediaVersion=Guid.NewGuid();c.SubmittedSnapshotId=null;c.ApprovedSnapshotId=null;
                if(c.Status is ContentStatusEnum.Approved or ContentStatusEnum.Published or ContentStatusEnum.PendingApproval) c.Status=ContentStatusEnum.Draft;
            }
            if(transitioning && c.Status==ContentStatusEnum.PendingApproval && !c.SubmittedSnapshotId.HasValue)
                c.SubmittedSnapshotId=(await CaptureSnapshotAsync(c,ct)).Id;
            if(transitioning && c.Status==ContentStatusEnum.Approved)
            {
                // Existing approved legacy content must pass approval again before
                // it can obtain a snapshot; never silently approve a changed draft.
                c.ApprovedSnapshotId=c.SubmittedSnapshotId??(await CaptureSnapshotAsync(c,ct)).Id;
            }
        }
        foreach(var entry in ChangeTracker.Entries<Approval>().Where(e=>e.State==EntityState.Added).ToArray())
        {
            var c=await Contents.FindAsync([entry.Entity.ContentId],ct);
            if(c is not null) entry.Entity.SnapshotId=c.SubmittedSnapshotId??c.ApprovedSnapshotId;
        }
        foreach(var entry in ChangeTracker.Entries<ContentCalendar>().Where(e=>e.State==EntityState.Added).ToArray())
        {
            var c=await Contents.FindAsync([entry.Entity.ContentId],ct);
            if(c?.ApprovedSnapshotId is {} snapshot) entry.Entity.SnapshotId=snapshot;
            else if(ExecutionActorId.HasValue) throw new InvalidOperationException("Content requires a reviewed snapshot before scheduling. Submit and approve it again.");
        }
        foreach(var entry in ChangeTracker.Entries<ContentMedia>().Where(e=>e.State==EntityState.Added).ToArray())
        {
            var c=await Contents.FindAsync([entry.Entity.ContentId],ct);var a=await Assets.FindAsync([entry.Entity.AssetId],ct);
            if(c is null||a is null||a.ExpiredAt.HasValue||a.WorkspaceId!=c.WorkspaceId||a.BrandId!=c.BrandId)throw new ResourceMutationDeniedException();
        }
        foreach(var entry in ChangeTracker.Entries<Post>().Where(e=>e.State==EntityState.Added && e.Entity.SnapshotId.HasValue).ToArray())
        {
            var snapshot=await PublishSnapshots.IgnoreQueryFilters().Include(s=>s.Media).SingleAsync(s=>s.Id==entry.Entity.SnapshotId,ct);
            if(snapshot.ContentId!=entry.Entity.ContentId)throw new ResourceMutationDeniedException();
            foreach(var media in snapshot.Media) PostMedia.Add(new PostMedia {PostId=entry.Entity.Id,SnapshotMediaId=media.Id,Status="Published"});
        }
    }
}
