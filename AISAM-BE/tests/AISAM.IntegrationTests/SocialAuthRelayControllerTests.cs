using AISAM.API.Controllers;
using AISAM.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AISAM.IntegrationTests;

public sealed class SocialAuthRelayControllerTests
{
    [Fact]
    public void RelayFacebookCallback_RedirectsToFrontendAndPreservesOAuthQuery()
    {
        var controller = new SocialAuthRelayController(Options.Create(new FrontendSettings
        {
            BaseUrl = "https://app.example.com/"
        }))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request =
                    {
                        QueryString = new QueryString("?code=oauth-code&state=oauth-state")
                    }
                }
            }
        };

        var result = Assert.IsType<RedirectResult>(controller.RelayFacebookCallback());

        Assert.Equal(
            "https://app.example.com/social-callback/facebook?code=oauth-code&state=oauth-state",
            result.Url);
    }

}
