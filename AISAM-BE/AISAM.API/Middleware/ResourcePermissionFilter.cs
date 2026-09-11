using AISAM.API.Utils;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AISAM.API.Middleware;

public sealed class ResourcePermissionFilter(IAccessControlService access,AisamContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
    {
        if(!db.PermissionScopeEnabled) { await next(); return; }
        var ct=context.HttpContext.RequestAborted;
        var actor=UserClaimsHelper.GetUserIdOrThrow(context.HttpContext.User);
        var workspace=db.PermissionWorkspaceId;
        var method=context.HttpContext.Request.Method;
        bool read=HttpMethods.IsGet(method) || HttpMethods.IsHead(method);
        var action=context.ActionDescriptor.RouteValues.TryGetValue("action",out var a)?a??"":"";
        var controller=context.ActionDescriptor.RouteValues.TryGetValue("controller",out var c)?c??"":"";
        db.PermissionReviewQueue=controller=="Content" && action=="ReviewQueue" && read;
        db.PermissionOnlyMyContent=controller=="Content" && read && context.HttpContext.Request.Query["mine"]=="true";
        if(!db.PermissionOwner && !db.PermissionManager && (controller=="SocialAuth" || controller=="SocialAccounts" && (!read || action.Contains("Target",StringComparison.OrdinalIgnoreCase))))
        {
            // OAuth credential discovery is account-wide; it has no trusted Brand scope.
            context.Result=new ObjectResult(new {success=false,errorCode="ACCESS_DENIED_CHANNEL"}){StatusCode=403}; return;
        }
        var ids=new Dictionary<string,Guid>(StringComparer.OrdinalIgnoreCase);
        bool duplicateMismatch=false;
        void AddId(string key,Guid value)
        {
            if(ids.TryGetValue(key,out var existing) && existing!=value) duplicateMismatch=true;
            else ids[key]=value;
        }
        foreach(var pair in context.ActionArguments)
        {
            if(pair.Value is Guid id) AddId(pair.Key,id);
            else if(pair.Value is not null && pair.Value is not string)
                foreach(var property in pair.Value.GetType().GetProperties().Where(p=>p.GetIndexParameters().Length==0 && (p.PropertyType==typeof(Guid) || p.PropertyType==typeof(Guid?))))
                    if(property.GetValue(pair.Value) is Guid nested) AddId(property.Name,nested);
        }
        if(duplicateMismatch) { context.Result=new BadRequestObjectResult(new {success=false,errorCode="RESOURCE_ID_MISMATCH"}); return; }
        async Task<bool> Check(AccessResourceKind kind,Guid id,ResourcePermission permission,Guid? channel=null)
        {
            var result=await access.CheckAsync(new(actor,workspace,kind,id,permission,channel,IncludeDeleted:action=="Restore"),ct);
            if(result.Allowed) return true;
            context.Result=new ObjectResult(new {success=false,statusCode=result.StatusCode,errorCode=result.ErrorCode}){StatusCode=result.StatusCode};
            return false;
        }
        if(ids.TryGetValue("postId",out var postId))
        {
            var post=await db.Posts.AsNoTracking().Include(p=>p.Content).FirstOrDefaultAsync(p=>p.Id==postId,ct);
            if(post is null) { context.Result=new NotFoundResult(); return; }
            bool isAuthor=post.Content!=null && post.Content.PrimaryCreatorId==actor;
            if(!isAuthor && !await Check(AccessResourceKind.Post,postId,ResourcePermission.PostView)) return;
            if(!read)
            {
                if(!await Check(AccessResourceKind.Content,post.ContentId,ResourcePermission.ContentDelete)) return;
            }
        }
        if(ids.TryGetValue("brandId",out var brand))
        {
            var permission=read?ResourcePermission.BrandView:
                controller is "Content" or "Gemini" ? ResourcePermission.ContentCreate:ResourcePermission.BrandManage;
            if(!await Check(AccessResourceKind.Brand,brand,permission)) return;
        }
        if(ids.TryGetValue("productId",out var product) || (controller=="Product" && ids.TryGetValue("id",out product)))
        {
            var value=await db.Products.AsNoTracking().FirstOrDefaultAsync(p=>p.Id==product,ct);
            if(value is null) { context.Result=new NotFoundResult(); return; }
            if(!await Check(AccessResourceKind.Brand,value.BrandId,read?ResourcePermission.BrandView:controller=="Product"?ResourcePermission.BrandManage:ResourcePermission.ContentCreate)) return;
        }
        if(ids.TryGetValue("scheduleId",out var scheduleId))
        {
            var sched=await db.ContentCalendars.AsNoTracking().Include(s=>s.Content).FirstOrDefaultAsync(s=>s.Id==scheduleId,ct);
            if(sched is null) { context.Result=new NotFoundResult(); return; }
            var requiredPerm=read?ResourcePermission.ContentView:ResourcePermission.PostPublish;
            if(sched.Content!=null && !await Check(AccessResourceKind.Brand,sched.Content.BrandId,requiredPerm)) return;
        }
        if(ids.TryGetValue("campaignId",out var campaignId) || (controller=="AdCampaign" && ids.TryGetValue("id",out campaignId)))
        {
            var campaign=await db.AdCampaigns.AsNoTracking().FirstOrDefaultAsync(c=>c.Id==campaignId,ct);
            if(campaign is null) { context.Result=new NotFoundResult(); return; }
            if(!await Check(AccessResourceKind.Brand,campaign.BrandId,read?ResourcePermission.BrandView:ResourcePermission.BrandManage)) return;
        }
        if(ids.TryGetValue("contentId",out var content))
        {
            var review=action is "Approve" or "Reject";
            var publish=action.Contains("Publish",StringComparison.OrdinalIgnoreCase) || controller=="ContentSchedules" && !read;
            ids.TryGetValue("integrationId",out var integration);
            var permission=review?ResourcePermission.ApprovalReview:publish?ResourcePermission.PostPublish:
                read || action=="Clone"?ResourcePermission.ContentView:HttpMethods.IsDelete(method)?ResourcePermission.ContentDelete:ResourcePermission.ContentEdit;
            if(!await Check(AccessResourceKind.Content,content,permission,publish?integration:null)) return;
            if(review) db.PermissionReviewContentId=content;
        }
        foreach(var key in new[]{"socialIntegrationId","integrationId"})
            if(ids.TryGetValue(key,out var integration) && !ids.ContainsKey("contentId") &&
                !await Check(AccessResourceKind.Channel,integration,read?ResourcePermission.SocialView:ResourcePermission.SocialManage)) return;
        // List/aggregate scope lives in EF. Legacy operations without explicit resource
        // IDs still retain role restrictions in ActiveWorkspaceMiddleware.
        await next();
    }
}
