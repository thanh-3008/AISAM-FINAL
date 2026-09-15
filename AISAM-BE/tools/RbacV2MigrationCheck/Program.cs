using AISAM.Repositories;
using AISAM.Repositories.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

// Fixed local verification database, never the application's database.
var password = Environment.GetEnvironmentVariable("PGPASSWORD") ?? throw new Exception("Set PGPASSWORD locally.");
var cs = new NpgsqlConnectionStringBuilder { Host="127.0.0.1", Port=5432, Database="aisam_r01_verification", Username="postgres", Password=password }.ConnectionString;
await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(cs).Options);
var generator=db.GetService<IMigrationsSqlGenerator>();
string Sql(Migration m,bool down=false)=>string.Join("\n",generator.Generate(down?m.DownOperations:m.UpOperations).Select(c=>c.CommandText));
await using var conn=new NpgsqlConnection(cs);
await conn.OpenAsync();
foreach (var mode in new[] { "backup", "fresh" })
{
Console.WriteLine(mode);
await using var tx=await conn.BeginTransactionAsync();
async Task Run(string sql) { await using var cmd=new NpgsqlCommand(sql,conn,tx); await cmd.ExecuteNonQueryAsync(); }
async Task<long> Count(string sql) { await using var cmd=new NpgsqlCommand(sql,conn,tx); return Convert.ToInt64(await cmd.ExecuteScalarAsync()); }
async Task Assert(string name,string sql) { if(await Count(sql)!=0) throw new Exception("FAIL "+name); Console.WriteLine("PASS "+name); }
if(mode=="fresh")
{
    await Run("CREATE SCHEMA r01_fresh; SET LOCAL search_path TO r01_fresh;");
    await Run(db.Database.GenerateCreateScript());
    await Run("ALTER TABLE workspace_members DROP COLUMN workspace_role_v2; ALTER TABLE workspace_invitations DROP COLUMN workspace_role_v2; ALTER TABLE teams DROP COLUMN default_for_brand_id CASCADE; ALTER TABLE team_channel_access DROP COLUMN scope_enabled_v2;");
}
else await Run(Sql(new MigrateRbacSchemaAndEnums()));
var memberCount=await Count("SELECT count(*) FROM team_members");
var contentCount=await Count("SELECT count(*) FROM contents");
var nullCount=await Count("SELECT count(*) FROM contents WHERE team_id IS NULL");
await Run("CREATE TEMP TABLE r01_original AS SELECT id,team_id,primary_creator_id FROM contents;");
await Run(Sql(new AddWorkspaceRoleV2()));
await Run(Sql(new PrepareTeamRbacV2()));
await Assert("legacy Manager never promoted", "SELECT count(*) FROM workspace_members WHERE role<>1 AND workspace_role_v2=2");
await Assert("invitations never promoted", "SELECT count(*) FROM workspace_invitations WHERE workspace_role_v2 IN(1,2)");
await Assert("channel scope starts denied", "SELECT count(*) FROM team_channel_access WHERE scope_enabled_v2");
await Assert("valid content backfilled", "SELECT count(*) FROM contents c JOIN brands b ON b.id=c.brand_id WHERE c.team_id IS NULL AND c.workspace_id=b.workspace_id AND NOT b.is_deleted");
await Assert("existing attribution preserved", "SELECT count(*) FROM contents c JOIN r01_original o ON o.id=c.id WHERE (o.team_id IS NOT NULL AND o.team_id IS DISTINCT FROM c.team_id) OR o.primary_creator_id IS DISTINCT FROM c.primary_creator_id");
if(await Count("SELECT count(*) FROM workspace_members")>0)
{
    await Run("SAVEPOINT invalid_role");
    try { await Run("UPDATE workspace_members SET workspace_role_v2=99"); throw new Exception("Invalid role accepted"); }
    catch(PostgresException e) when(e.SqlState=="23514") { await Run("ROLLBACK TO SAVEPOINT invalid_role"); Console.WriteLine("PASS invalid v2 role rejected"); }
}
var teams=await Count("SELECT count(*) FROM teams");
await Run(PrepareTeamRbacV2.BackfillSql);
if(teams!=await Count("SELECT count(*) FROM teams") || memberCount!=await Count("SELECT count(*) FROM team_members") || contentCount!=await Count("SELECT count(*) FROM contents")) throw new Exception("Backfill changed members/content count or duplicated teams");
Console.WriteLine("PASS repeated backfill preserves memberships and content");
await using(var report=new NpgsqlCommand("SELECT issue,count(*) FROM rbac_v2_preflight GROUP BY issue ORDER BY issue",conn,tx))
await using(var reader=await report.ExecuteReaderAsync())
while(await reader.ReadAsync()) Console.WriteLine($"REPORT {reader.GetString(0)}: {reader.GetInt64(1)}");
await Run(Sql(new PrepareTeamRbacV2(),true));
await Run(Sql(new AddWorkspaceRoleV2(),true));
if(nullCount!=await Count("SELECT count(*) FROM contents WHERE team_id IS NULL")) throw new Exception("Rollback attribution mismatch");
Console.WriteLine("PASS rollback restores original attribution");
await tx.RollbackAsync();
Console.WriteLine("PASS transaction rolled back; source database untouched");

}
