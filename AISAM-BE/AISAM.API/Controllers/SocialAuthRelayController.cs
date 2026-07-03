using AISAM.Common;
using AISAM.Common.Models;
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

    public SocialAuthRelayController(IOptions<FrontendSettings> frontendSettings)
    {
        _frontendSettings = frontendSettings.Value;
    }

    [AllowAnonymous]
    [HttpGet("facebook/callback")]
    public IActionResult RelayFacebookCallback()
    {
        if (!Uri.TryCreate(_frontendSettings.BaseUrl, UriKind.Absolute, out var frontendUri) ||
            (!string.Equals(frontendUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(frontendUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
        {
            return BadRequest(GenericResponse<object>.CreateError(
                "Frontend callback URL is not configured.",
                HttpStatusCode.BadRequest));
        }

        var callbackUrl = $"{frontendUri.ToString().TrimEnd('/')}/social-callback/facebook{Request.QueryString}";
        return Redirect(callbackUrl);
    }
}
