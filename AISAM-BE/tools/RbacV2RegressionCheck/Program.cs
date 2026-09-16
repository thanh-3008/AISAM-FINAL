using AISAM.API.Filters;
using AISAM.API.Utils;
using AISAM.Data.Model;
using AISAM.Repositories;
using AISAM.Repositories.Migrations;
using AISAM.Services.Access;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

// Only a random schema in the fixed verification database is writable.
var schema = "r11_" + Guid.NewGuid().ToString("N");
var cs = new NpgsqlConnectionStringBuilder {
    Host="127.0.0.1", Port=5432, Database="aisam_r01_verification", Username="postgres",
    Password=Environment.GetEnvironmentVariable("PGPASSWORD") ?? throw new Exception("Set PGPASSWORD locally."),
    SearchPath=schema, Pooling=false
}.ConnectionString;
await using var connection = new NpgsqlConnection(cs);
await connection.OpenAsync();
async Task Sql(string sql) { await using var cmd=new NpgsqlCommand(sql,connection); await cmd.ExecuteNonQueryAsync(); }
await Sql($"CREATE SCHEMA {schema}");
AisamContext Db()=>new(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(cs).Options);
var workspace=Guid.NewGuid();
ActionExecutingContext Action(string method,string? revision=null) {
    var http=new DefaultHttpContext(); http.Request.Method=method; http.Request.Path="/api/teams";
    http.Items[WorkspaceContextHelper.ActiveWorkspaceItemKey]=workspace;
    if(revision is not null) http.Request.Headers["If-Match"]=revision;
    return new(new ActionContext(http,new RouteData(),new ActionDescriptor()),new List<IFilterMetadata>(),new Dictionary<string,object?>(),new object());
}
WorkspaceHrV2ConcurrencyFilter Filter(AisamContext db)=>new(db,new RbacV2AccessAdapter(db,new RbacV2AccessResolver(db)));
ActionExecutedContext Done(ActionExecutingContext a)=>new(a,a.Filters,a.Controller){Result=new OkResult()};
try {
    await using(var db=Db()) {
        await Sql(db.Database.GenerateCreateScript());
        db.Workspaces.Add(new Workspace{Id=workspace,Name="R11 isolated concurrency"});
        await db.SaveChangesAsync();
        // Exercise the two migrations added after R01 in both directions.
        var generator=db.GetService<IMigrationsSqlGenerator>();
        string MigrationSql(Migration m,bool down)=>string.Join("\n",generator.Generate(down?m.DownOperations:m.UpOperations).Select(c=>c.CommandText));
        foreach(var migration in new Migration[]{new AddAutomationTeamScope(),new AddConversationTeamScope()}) {
            await Sql(MigrationSql(migration,true));
            await Sql(MigrationSql(migration,false));
            Console.WriteLine("PASS Up/Down/Up " + migration.GetType().Name);
        }
    }
    await using var first=Db(); await using var second=Db();
    var read=Action("GET");
    await Filter(first).OnActionExecutionAsync(read,()=>Task.FromResult(Done(read)));
    var revision=read.HttpContext.Response.Headers["X-HR-Revision"].ToString();
    if(string.IsNullOrEmpty(revision)) throw new Exception("Missing revision");
    var entered=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var release=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var a=Action("POST",revision); var b=Action("POST",revision);
    var writes=0;
    var taskA=Filter(first).OnActionExecutionAsync(a,async()=>{
        first.Teams.Add(new Team{WorkspaceId=workspace,Name="Winner"});
        await first.SaveChangesAsync(); Interlocked.Increment(ref writes);
        entered.SetResult(); await release.Task; return Done(a);
    });
    await entered.Task.WaitAsync(TimeSpan.FromSeconds(15));
    var taskB=Filter(second).OnActionExecutionAsync(b,async()=>{
        second.Teams.Add(new Team{WorkspaceId=workspace,Name="Must not persist"});
        await second.SaveChangesAsync(); Interlocked.Increment(ref writes); return Done(b);
    });
    await Task.Delay(150); release.SetResult();
    await Task.WhenAll(taskA,taskB).WaitAsync(TimeSpan.FromSeconds(20));
    await using var verify=Db();
    if(b.Result is not ObjectResult {StatusCode:409} || writes!=1 || await verify.Teams.CountAsync()!=1)
        throw new Exception("Concurrent stale revision was not rejected atomically");
    Console.WriteLine("PASS concurrent HR writes: one commit, one 409, no duplicate mutation");
    var stale=Action("POST",revision);
    await Filter(verify).OnActionExecutionAsync(stale,()=>throw new Exception("Stale request executed"));
    if(stale.Result is not ObjectResult {StatusCode:409}) throw new Exception("Stale revision accepted");
    Console.WriteLine("PASS stale revision rejected after commit");
} finally {
    // Identifier consists exclusively of a fixed prefix and a generated GUID.
    await Sql($"DROP SCHEMA {schema} CASCADE");
    Console.WriteLine("CLEANUP isolated R11 schema removed; application database untouched");
}
