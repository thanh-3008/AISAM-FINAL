using AISAM.Data.Model;
using Microsoft.AspNetCore.Http;

namespace AISAM.Services.IServices;

public interface IMediaStorageService
{
    async Task<StoredMedia> UploadDetailedAsync(IFormFile file,string folder,string name,CancellationToken ct=default)
        =>new(await UploadAsync(file,folder,name,ct));
    // Implementations that cannot delete must leave the tombstone for retry.
    Task<bool> DeleteAsync(string publicId,bool video,CancellationToken cancellationToken=default)=>Task.FromResult(false);
    Task<string> UploadAsync(
        IFormFile file,
        string folder,
        string fileName,
        CancellationToken cancellationToken = default);

    Task<string> UploadBytesAsync(
        byte[] data,
        string folder,
        string fileName,
        CancellationToken cancellationToken = default);

    async Task<StoredMedia> UploadBytesDetailedAsync(
        byte[] data,
        string folder,
        string fileName,
        CancellationToken cancellationToken = default)
        => new(await UploadBytesAsync(data, folder, fileName, cancellationToken));

    Task<StoredMedia?> GetMediaMetadataAsync(
        string url,
        CancellationToken cancellationToken = default) => Task.FromResult<StoredMedia?>(null);
}
