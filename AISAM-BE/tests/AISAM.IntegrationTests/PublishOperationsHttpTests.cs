using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using AISAM.API.Controllers;
using AISAM.API.Middleware;
using AISAM.API.Utils;
using AISAM.Common.Models;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Services.Access;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AISAM.IntegrationTests;

// Real HTTP/MVC/auth filters with a test identity and stub permission resolver.
// This does not replace JWT or real membership integration tests.
public sealed class PublishOperationsHttpTests
{
    private sealed class Access:IAccessControlService
    {
        public bool Deny;
        public bool Race;
        private int arrivals;
        private readonly TaskCompletionSource ready=new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task<AccessDecision> CheckAsync(AccessRequest r,CancellationToken ct=default)
        {
            if(Race&&r.Permission==ResourcePermission.PostPublish)
            {
                if(Interlocked.Increment(ref arrivals)==2)ready.SetResult();
                await ready.Task.WaitAsync(TimeSpan.FromSeconds(10),ct);
            }
            return Deny?new(false,403,"ACCESS_DENIED_CHANNEL"):AccessDecision.Permit;
        }
        public Task<IReadOnlyList<Guid>> GetAccessibleBrandIdsAsync(Guid a,Guid w,CancellationToken ct=default)=>throw new NotSupportedException();
    }
    private sealed class Identity(IOptionsMonitor<AuthenticationSchemeOptions> options,ILoggerFactory logger,UrlEncoder encoder):AuthenticationHandler<AuthenticationSchemeOptions>(options,logger,encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if(!Request.Headers.ContainsKey("X-Test-Actor"))return Task.FromResult(AuthenticateResult.NoResult());
            var principal=new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier,Request.Headers["X-Test-Actor"].ToString())],Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal,Scheme.Name)));
        }
    }
    [Fact]
    public async Task ReadCancelAndCompetingCancelsRespectHttpContracts()
    {
        var workspace=Guid.NewGuid();var actor=Guid.NewGuid();var database=Guid.NewGuid().ToString();var access=new Access();
        using var server=new TestServer(new WebHostBuilder().ConfigureServices(services=>
        {
            services.AddLogging();
            services.AddDbContext<AisamContext>(o=>o.UseInMemoryDatabase(database));
            services.AddSingleton<IAccessControlService>(access);
            services.AddSingleton<IContentService>(System.Reflection.DispatchProxy.Create<IContentService,PublishingPipelineTests.ContentProxy>());
            services.AddScoped<PublishOperationService>();services.Configure<InstagramSettings>(_=>{});
            services.AddAuthentication("test").AddScheme<AuthenticationSchemeOptions,Identity>("test",_=>{});
            services.AddAuthorization();services.AddControllers().AddApplicationPart(typeof(PublishOperationsController).Assembly);
        }).Configure(app=>
        {
            app.UseMiddleware<ExceptionHandlerMiddleware>();app.UseRouting();app.UseAuthentication();app.UseAuthorization();
            app.Use(async(context,next)=>{context.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey]=workspace;await next();});
            app.UseEndpoints(e=>e.MapControllers());
        }));
        Guid queuedId,startedId,foreignId,contentId,version,integrationId;
        using(var scope=server.Services.CreateScope())
        {
            var db=scope.ServiceProvider.GetRequiredService<AisamContext>();
            var queued=new PublishOperation{WorkspaceId=workspace,ActorId=actor,ContentId=Guid.NewGuid()};
            var started=new PublishOperation{WorkspaceId=workspace,ActorId=actor,Status="Publishing"};
            var foreign=new PublishOperation{WorkspaceId=Guid.NewGuid(),ActorId=actor};
            db.AddRange(queued,started,foreign);await db.SaveChangesAsync();queuedId=queued.Id;startedId=started.Id;foreignId=foreign.Id;
            var content=new Content{WorkspaceId=workspace,Status=AISAM.Data.Enumeration.ContentStatusEnum.Approved};
            var integration=new SocialIntegration{WorkspaceId=workspace,AccessToken="private-token",SocialAccount=new(){UserAccessToken="private-token",IsActive=true}};
            db.AddRange(content,integration);await db.SaveChangesAsync();contentId=content.Id;version=content.MediaVersion;integrationId=integration.Id;
        }
        using var client=server.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized,(await client.GetAsync($"/api/publish-operations/{queuedId}")).StatusCode);
        client.DefaultRequestHeaders.Add("X-Test-Actor",actor.ToString());
        Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync($"/api/publish-operations/{foreignId}")).StatusCode);
        access.Deny=true;
        Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync($"/api/publish-operations/{queuedId}")).StatusCode);
        access.Deny=false;
        var read=await client.GetAsync($"/api/publish-operations/{queuedId}");Assert.Equal(HttpStatusCode.OK,read.StatusCode);
        var body=await read.Content.ReadAsStringAsync();Assert.DoesNotContain("idempotencyKey",body);Assert.DoesNotContain("accessToken",body);
        Assert.Equal(HttpStatusCode.Conflict,(await client.PostAsync($"/api/publish-operations/{startedId}/cancel",null)).StatusCode);
        var url=$"/api/content/{contentId}/publish-operations";
        var request=new{expectedVersion=version,integrationIds=new[]{integrationId},idempotencyKey="http-request"};
        access.Deny=true;Assert.Equal(HttpStatusCode.Forbidden,(await client.PostAsJsonAsync(url,request)).StatusCode);access.Deny=false;
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync(url,request)).StatusCode);
        Assert.Equal(HttpStatusCode.OK,(await client.PostAsJsonAsync(url,request)).StatusCode);
        var listed=await client.GetAsync(url+"?key=http-request");
        Assert.Equal(HttpStatusCode.OK,listed.StatusCode);
        Assert.Contains("Published",await listed.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Remove("X-Test-Actor");client.DefaultRequestHeaders.Add("X-Test-Actor",Guid.NewGuid().ToString());
        Assert.DoesNotContain("Published",await (await client.GetAsync(url+"?key=http-request")).Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Remove("X-Test-Actor");client.DefaultRequestHeaders.Add("X-Test-Actor",actor.ToString());
        access.Deny=true;
        Assert.Equal(HttpStatusCode.NotFound,(await client.GetAsync($"/api/content/{contentId}/publish-preview")).StatusCode);
        access.Deny=false;
        var preview=await client.GetAsync($"/api/content/{contentId}/publish-preview");
        Assert.Equal(HttpStatusCode.OK,preview.StatusCode);
        var previewBody=await preview.Content.ReadAsStringAsync();
        Assert.DoesNotContain("private-token",previewBody);Assert.Contains("caption",previewBody);
        Assert.Equal(1,((PublishingPipelineTests.ContentProxy)(object)server.Services.GetRequiredService<IContentService>()).Calls);
        Assert.Equal(HttpStatusCode.Conflict,(await client.PostAsJsonAsync(url,new{expectedVersion=Guid.NewGuid(),integrationIds=new[]{integrationId},idempotencyKey="http-request"})).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,(await client.PostAsJsonAsync(url,new{expectedVersion=version,integrationIds=Array.Empty<Guid>(),idempotencyKey="empty"})).StatusCode);
        access.Race=true;
        var attempts=await Task.WhenAll(client.PostAsync($"/api/publish-operations/{queuedId}/cancel",null),client.PostAsync($"/api/publish-operations/{queuedId}/cancel",null));
        Assert.Single(attempts.Where(r=>r.StatusCode==HttpStatusCode.OK));Assert.Single(attempts.Where(r=>r.StatusCode==HttpStatusCode.Conflict));
        using(var scope=server.Services.CreateScope())Assert.Equal("Cancelled",(await scope.ServiceProvider.GetRequiredService<AisamContext>().PublishOperations.FindAsync(queuedId))!.Status);
    }
}
