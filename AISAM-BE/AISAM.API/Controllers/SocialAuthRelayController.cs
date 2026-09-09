using AISAM.Common;
using AISAM.Common.Models;
using AISAM.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;

namespace AISAM.API.Controllers;

[ApiController]
[Route("api/social-auth")]
public sealed class SocialAuthRelayController : ControllerBase
{
    private readonly FrontendSettings _frontendSettings;
    private readonly FacebookSettings? _facebookSettings;
    private readonly IOAuthStateStore? _oauthStateStore;
    private readonly IOriginResolver? _originResolver;

    public SocialAuthRelayController(
        IOptions<FrontendSettings> frontendSettings,
        IOAuthStateStore? oauthStateStore = null,
        IOriginResolver? originResolver = null,
        IOptions<FacebookSettings>? facebookSettings = null)
    {
        _frontendSettings = frontendSettings.Value;
        _oauthStateStore = oauthStateStore;
        _originResolver = originResolver;
        _facebookSettings = facebookSettings?.Value;
    }

    [AllowAnonymous]
    [HttpGet("facebook/callback")]
    public IActionResult RelayFacebookCallback()
    {
        string? targetOrigin = null;

        var state = Request.Query["state"].ToString();
        if (!string.IsNullOrWhiteSpace(state) && _oauthStateStore != null)
        {
            var peeked = _oauthStateStore.TryPeekOrigin(state);
            if (!string.IsNullOrWhiteSpace(peeked) &&
                (_originResolver == null || _originResolver.IsAllowedOrigin(peeked)))
            {
                targetOrigin = peeked;
            }
        }

        if (string.IsNullOrWhiteSpace(targetOrigin))
        {
            targetOrigin = _frontendSettings.BaseUrl;
        }

        if (!Uri.TryCreate(targetOrigin, UriKind.Absolute, out var targetUri) ||
            (!string.Equals(targetUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(targetUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(GenericResponse<object>.CreateError(
                "Frontend callback URL is not configured.",
                HttpStatusCode.BadRequest));
        }

        var path = _facebookSettings?.RedirectPath ?? "/social-callback/facebook";
        var normalizedPath = path.StartsWith('/') ? path : $"/{path}";
        var callbackUrl = $"{targetUri.ToString().TrimEnd('/')}{normalizedPath}{Request.QueryString}";
        return Redirect(callbackUrl);
    }
}

