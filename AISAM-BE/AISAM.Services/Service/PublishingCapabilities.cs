using AISAM.Data.Model;
using AISAM.Data.Enumeration;

namespace AISAM.Services.Service;

// Limits describe the current AISAM adapter, not every capability of the platform.
public sealed record PublishingCapability(string Platform,string AdapterVersion,bool Available,string? UnavailableReason,
    bool TextOnly,bool MultiImage,bool MultiVideo,bool MixedMedia,int MaxItems,long MaxItemBytes,string RichText="PlainText")
{
    // Conservative AISAM limits; provider may impose stricter account restrictions.
    public string LimitSource=>"AISAM conservative upload policy";
    public int MaxVideoDurationSeconds=>60;
    public bool RequiresVideoMetadata=>true;
    public long MaxImageBytes=>8L*1024*1024;
    public string[] ImageMimeTypes=>Platform==nameof(SocialPlatformEnum.Instagram)?["image/jpeg"]:["image/jpeg","image/png"];
    public string[] VideoMimeTypes=>["video/mp4"];
    public string AccountValidation=>"Stored credentials checked; provider verifies live permissions";
}

public static class PublishingCapabilities
{
    public static PublishingCapability For(SocialIntegration integration,SocialAccount? account,DateTime now,bool verifiedCarousel=false)
    {
        var platform=integration.Platform;
        var supported=platform is SocialPlatformEnum.Facebook or SocialPlatformEnum.Instagram or SocialPlatformEnum.TikTok;
        var reason=!supported?"PLATFORM_UNSUPPORTED":integration.IsDeleted||!integration.IsActive||account is null||account.IsDeleted||!account.IsActive
            ?"CHANNEL_INACTIVE":string.IsNullOrWhiteSpace(integration.AccessToken)||string.IsNullOrWhiteSpace(account.UserAccessToken)||
                integration.ExpiresAt<=now||account.ExpiresAt<=now?"SOCIAL_REAUTH_REQUIRED":null;
        return new(platform.ToString(),"aisam-v1",reason is null,reason,platform==SocialPlatformEnum.Facebook,
            platform is SocialPlatformEnum.Facebook or SocialPlatformEnum.Instagram,platform==SocialPlatformEnum.Instagram&&verifiedCarousel,platform==SocialPlatformEnum.Instagram&&verifiedCarousel,
            platform==SocialPlatformEnum.TikTok?1:10,50L*1024*1024);
    }

    public static string? Validate(PublishingCapability capability,IReadOnlyCollection<SnapshotMedia> media)
    {
        if(!capability.Available)return capability.UnavailableReason;
        if(media.Count>capability.MaxItems||media.Any(m=>m.SizeBytes>capability.MaxItemBytes))return "MEDIA_LIMIT_EXCEEDED";
        int images=media.Count(m=>m.MimeType?.StartsWith("image/",StringComparison.OrdinalIgnoreCase)==true);
        int videos=media.Count(m=>m.MimeType?.StartsWith("video/",StringComparison.OrdinalIgnoreCase)==true);
        if(images+videos!=media.Count)return "MEDIA_TYPE_UNSUPPORTED";
        if(media.Any(m=>m.MimeType?.StartsWith("image/")==true&&m.MimeType!="image/legacy"&&!capability.ImageMimeTypes.Contains(m.MimeType)))return "MEDIA_TYPE_UNSUPPORTED";
        if(media.Any(m=>m.MimeType?.StartsWith("image/")==true&&m.SizeBytes>capability.MaxImageBytes))return "MEDIA_LIMIT_EXCEEDED";
        foreach(var video in media.Where(m=>m.MimeType?.StartsWith("video/")==true))
        {
            if(video.MimeType!="video/legacy"&&!capability.VideoMimeTypes.Contains(video.MimeType!))return "MEDIA_TYPE_UNSUPPORTED";
            if(video.DurationSeconds is null or <=0)return "MEDIA_METADATA_REQUIRED";
            if(video.DurationSeconds>capability.MaxVideoDurationSeconds)return "MEDIA_LIMIT_EXCEEDED";
        }
        if(media.Count==0&&!capability.TextOnly||images>1&&!capability.MultiImage||videos>1&&!capability.MultiVideo||images>0&&videos>0&&!capability.MixedMedia)
            return "MEDIA_TYPE_UNSUPPORTED";
        if(capability.Platform==nameof(SocialPlatformEnum.TikTok)&&images>0)return "MEDIA_TYPE_UNSUPPORTED";
        if(media.Any(m=>!Uri.TryCreate(m.Url,UriKind.Absolute,out var uri)||uri.Scheme is not ("https" or "http")))return "MEDIA_URL_INVALID";
        return null;
    }
}
