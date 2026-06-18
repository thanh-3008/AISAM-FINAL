namespace AISAM.Common.Config;

public sealed class MediaStorageSettings
{
    public string? UploadRootPath { get; set; }

    public string ResolveUploadRootPath(string? contentRootPath = null)
    {
        if (!string.IsNullOrWhiteSpace(UploadRootPath))
        {
            return Path.GetFullPath(Path.IsPathRooted(UploadRootPath)
                ? UploadRootPath
                : Path.Combine(contentRootPath ?? Directory.GetCurrentDirectory(), UploadRootPath));
        }

        var localDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localDataPath))
        {
            localDataPath = AppContext.BaseDirectory;
        }

        return Path.Combine(localDataPath, "AISAM", "wwwroot");
    }
}
