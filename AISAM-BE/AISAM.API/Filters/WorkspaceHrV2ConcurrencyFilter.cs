using AISAM.API.Utils;
using AISAM.Repositories;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AISAM.API.Filters;

// Serializes HR writes at workspace scope. Applied only to HR controllers, never arbitrary resources.
public sealed class WorkspaceHrV2ConcurrencyFilter(AisamContext db,IAccessControlService access) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context,ActionExecutionDelegate next)
    {
        var http=context.HttpContext;
        if(access is not RbacV2AccessAdapter || http.Request.Path.Value?.Contains("/validate/")==true || http.Request.Path.Value?.EndsWith("/accept")==true)
        { await next();return; }
        var workspace=WorkspaceContextHelper.GetActiveWorkspaceIdOrThrow(http);
        var ct=http.RequestAborted;
        if(HttpMethods.IsGet(http.Request.Method))
        { http.Response.Headers["X-HR-Revision"]=await Revision(workspace,ct);await next();return; }
        var expected=http.Request.Headers["If-Match"].ToString().Trim('"');
        if(string.IsNullOrWhiteSpace(expected))
        {context.Result=Error(428,"ACCESS_REVISION_REQUIRED");return;}
        try
        {
            // Npgsql's retrying execution strategy requires user-created transactions
            // to execute as one retriable unit. Without this wrapper every HR write
            // fails before the controller action is reached when retries are enabled.
            var strategy=db.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx=db.Database.IsRelational()
                    ?await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct)
                    :null;
                if(db.Database.IsRelational())
                    await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM workspaces WHERE id={workspace} FOR UPDATE",ct);
                // Middleware may have tracked membership before the lock was acquired.
                // HR service authorization must reload it inside this transaction.
                db.ChangeTracker.Clear();
                if(!string.Equals(expected,await Revision(workspace,ct),StringComparison.Ordinal))
                {context.Result=Error(409,"ACCESS_REVISION_CONFLICT");return;}
                var executed=await next();
                if(executed.Exception is { } actionError && IsSerializationFailure(actionError))
                {
                    executed.ExceptionHandled=true;
                    executed.Result=context.Result=Error(409,"ACCESS_REVISION_CONFLICT");
                    return;
                }
                if(executed.Exception is not null || executed.Result is ObjectResult {StatusCode: >=400})return;
                http.Response.Headers["X-HR-Revision"]=await Revision(workspace,ct);
                if(tx is not null)await tx.CommitAsync(ct);
            });
        }
        catch(Exception ex) when(IsSerializationFailure(ex))
        {context.Result=Error(409,"ACCESS_REVISION_CONFLICT");}
    }

    private static bool IsSerializationFailure(Exception error)
    {
        // Npgsql's execution strategy can wrap DbUpdateException in InvalidOperationException.
        for(Exception? current=error;current is not null;current=current.InnerException)
            if(current is Npgsql.PostgresException {SqlState:"40001"})return true;
        return false;
    }

    private async Task<string> Revision(Guid workspace,CancellationToken ct)
    {
        var members=await db.WorkspaceMembers.IgnoreQueryFilters().AsNoTracking().Where(m=>m.WorkspaceId==workspace).OrderBy(m=>m.Id)
            .Select(m=>new {m.Id,m.WorkspaceRoleV2,m.IsActive,m.QuotaMode,m.CreditLimit}).ToListAsync(ct);
        var invitations=await db.WorkspaceInvitations.AsNoTracking().Where(i=>i.WorkspaceId==workspace).OrderBy(i=>i.Id)
            .Select(i=>new {i.Id,i.WorkspaceRoleV2,i.AcceptedAt,i.RevokedAt,i.ExpiresAt}).ToListAsync(ct);
        var teams=await db.Teams.IgnoreQueryFilters().AsNoTracking().Where(t=>t.WorkspaceId==workspace).OrderBy(t=>t.Id).Select(t=>new{t.Id,t.Name,t.Description,t.Status,t.IsDeleted}).ToListAsync(ct);
        var teamIds=teams.Select(t=>t.Id).ToArray();
        var teamMembers=await db.TeamMembers.IgnoreQueryFilters().AsNoTracking().Where(m=>teamIds.Contains(m.TeamId)).OrderBy(m=>m.Id).Select(m=>new{m.Id,m.Role,m.IsActive}).ToListAsync(ct);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new {workspace,members,invitations,teams,teamMembers}))));
    }
    private static ObjectResult Error(int status,string code)=>new(new{success=false,statusCode=status,errorCode=code}){StatusCode=status};
}
