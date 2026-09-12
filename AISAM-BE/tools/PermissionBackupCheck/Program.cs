using System.Diagnostics;
using AISAM.Repositories;
using AISAM.Repositories.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

// Source is read only via pg_dump. All mutations use the isolated localhost port.
await using var source = new AisamContextFactory().CreateDbContext([]);
var settings = new NpgsqlConnectionStringBuilder(AisamContextFactory.ResolveConnectionString());
var root = Path.GetFullPath("../.artifacts/permission-backup");
Directory.CreateDirectory(root);
var dump = Path.Combine(root, args.Contains("--fresh-backup")
    ? "source-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + ".dump"
    : "source.dump");
var restoreName = "permission_restore_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
async Task Run(string exe, string[] args, bool remote = false)
{
    var psi = new ProcessStartInfo(Path.Combine(@"C:\Program Files\PostgreSQL\18\bin", exe))
    { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true, RedirectStandardOutput = true };
    foreach (var arg in args) psi.ArgumentList.Add(arg);
    psi.Environment["PGHOST"] = remote ? settings.Host! : "127.0.0.1";
    psi.Environment["PGPORT"] = remote ? settings.Port.ToString() : "55439";
    psi.Environment["PGUSER"] = remote ? settings.Username! : "permission_test";
    psi.Environment["PGPASSWORD"] = remote ? settings.Password ?? "" : "";
    psi.Environment["PGDATABASE"] = remote ? settings.Database! : restoreName;
    psi.Environment["PGSSLMODE"] = remote ? "require" : "disable";
    psi.Environment["PGCONNECT_TIMEOUT"] = "15";
    psi.ArgumentList.Add("--no-password");
    using var process = Process.Start(psi)!;
    var stderr = process.StandardError.ReadToEndAsync();
    var stdout = process.StandardOutput.ReadToEndAsync();
    await process.WaitForExitAsync(); await stdout;
    var err = await stderr;
    if (process.ExitCode != 0)
    {
        // Error file may contain source metadata; keep only in ignored artifacts.
        await File.WriteAllTextAsync(Path.Combine(root, exe + ".error.txt"), err);
        throw new Exception($"{exe} failed ({process.ExitCode}); private diagnostic saved under .artifacts.");
    }
}
if (!File.Exists(dump) || new FileInfo(dump).Length == 0)
    await Run("pg_dump.exe", ["--format=custom", "--schema=public", "--no-owner", "--no-acl", "--file="+dump], true);
Console.WriteLine("PASS source public schema/data backed up (no source writes)");
await Run("createdb.exe", [restoreName]);
var local = "Host=127.0.0.1;Port=55439;Database=" + restoreName + ";Username=permission_test;Pooling=false";
await using (var fresh = new NpgsqlConnection(local))
{
    await fresh.OpenAsync();
    await using var dropEmptySchema = new NpgsqlCommand("DROP SCHEMA public", fresh);
    await dropEmptySchema.ExecuteNonQueryAsync();
}
await Run("pg_restore.exe", ["--exit-on-error", "--no-owner", "--no-acl", "--dbname="+restoreName, dump]);
Console.WriteLine("PASS backup restored to isolated database");
await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(local).Options);
var gen = db.GetService<IMigrationsSqlGenerator>();
await db.Database.OpenConnectionAsync();
await using var tx = await db.Database.BeginTransactionAsync();
async Task<long> Count(string table) {
    await using var cmd = db.Database.GetDbConnection().CreateCommand();
    cmd.Transaction = tx.GetDbTransaction(); cmd.CommandText = "SELECT count(*) FROM " + table;
    return Convert.ToInt64(await cmd.ExecuteScalarAsync());
}
string[] tables = ["contents", "posts", "assets", "teams", "team_brands", "team_channel_access", "automation_plans"];
var before = new Dictionary<string,long>();
foreach (var t in tables) before[t] = await Count(t);
var checksums = new Dictionary<string,(string Sql,string Value)>();
foreach (var table in tables)
{
    await using var columns = db.Database.GetDbConnection().CreateCommand();
    columns.Transaction = tx.GetDbTransaction();
    columns.CommandText = $"SELECT string_agg(quote_ident(column_name),',' ORDER BY ordinal_position) FROM information_schema.columns WHERE table_schema='public' AND table_name='{table}'";
    var names = (string)(await columns.ExecuteScalarAsync())!;
    var sql = $"SELECT md5(coalesce(string_agg(row_to_json(t)::text,'' ORDER BY id),'')) FROM (SELECT {names} FROM {table}) t";
    columns.CommandText = sql;
    checksums[table] = (sql, (string)(await columns.ExecuteScalarAsync())!);
}
foreach (Migration migration in new Migration[] { new ReconcilePermissionFoundation(), new AddAutomationCreatorAttribution(), new CompletePermissionSchema() })
    foreach (var command in gen.Generate(migration.UpOperations)) await db.Database.ExecuteSqlRawAsync(command.CommandText);
foreach (var t in tables) if (before[t] != await Count(t)) throw new Exception("Row count changed: " + t);
foreach (var (table, expected) in checksums)
{
    await using var cmd = db.Database.GetDbConnection().CreateCommand();
    cmd.Transaction = tx.GetDbTransaction(); cmd.CommandText = expected.Sql;
    if (!Equals(await cmd.ExecuteScalarAsync(), expected.Value)) throw new Exception("Legacy data changed: " + table);
}
Console.WriteLine($"PASS checksums of all pre-existing columns unchanged in {tables.Length} resource tables");
Console.WriteLine($"PASS migration operations on restored data; all {tables.Length} resource counts unchanged");
await db.Contents.AsNoTracking().Select(c => new { c.Id, c.WorkspaceId, c.PrimaryCreatorId }).Take(1).ToListAsync();
await db.AutomationPlans.AsNoTracking().Take(1).ToListAsync();
await db.AuditLogs.AsNoTracking().Take(1).ToListAsync();
Console.WriteLine("PASS EF materialization after migrations");
foreach (var table in new[] { "teams", "brands", "contents", "social_integrations", "automation_plans" })
{
    if (await Count(table) == 0) continue;
    await tx.CreateSavepointAsync("negative_case");
    var rejected = false;
    try { await db.Database.ExecuteSqlRawAsync("UPDATE " + table + " SET workspace_id='ffffffff-ffff-ffff-ffff-ffffffffffff' WHERE id=(SELECT id FROM " + table + " LIMIT 1)"); }
    catch (PostgresException e) when (e.SqlState == "23514") { rejected = true; }
    await tx.RollbackToSavepointAsync("negative_case");
    if (!rejected) throw new Exception("Tenant transfer was not rejected: " + table);
    Console.WriteLine("PASS raw SQL workspace transfer rejected: " + table);
}
await tx.CreateSavepointAsync("creator_case");
bool creatorRejected = false;
try { await db.Database.ExecuteSqlRawAsync("UPDATE contents SET primary_creator_id='ffffffff-ffff-ffff-ffff-ffffffffffff' WHERE id=(SELECT id FROM contents LIMIT 1)"); }
catch (PostgresException e) when (e.SqlState == "23514") { creatorRejected = true; }
await tx.RollbackToSavepointAsync("creator_case");
if (!creatorRejected) throw new Exception("Creator mutation was not rejected.");
Console.WriteLine("PASS raw SQL Creator mutation rejected");
await db.Database.ExecuteSqlRawAsync("""
INSERT INTO team_channel_access(id,team_brand_id,integration_id,can_view,can_publish,can_manage)
SELECT 'ffffffff-ffff-ffff-ffff-fffffffffff1',tb.id,i.id,false,false,false
FROM team_brands tb JOIN social_integrations i ON i.brand_id=tb.brand_id
WHERE NOT EXISTS(SELECT 1 FROM team_channel_access ca WHERE ca.team_brand_id=tb.id AND ca.integration_id=i.id) LIMIT 1;
""");
if (await Count("team_channel_access") > 0)
{
    await tx.CreateSavepointAsync("grant_case");
    bool rejected = false;
    try { await db.Database.ExecuteSqlRawAsync("UPDATE team_channel_access SET can_publish=true,can_view=false"); }
    catch (PostgresException e) when (e.SqlState == "23514") { rejected = true; }
    await tx.RollbackToSavepointAsync("grant_case");
    if (!rejected) throw new Exception("Publish without view was not rejected.");
    Console.WriteLine("PASS raw SQL publish grant without view rejected");
}
else throw new Exception("No channel fixture available for grant validation.");
Console.WriteLine("Reconciliation issues recorded: " + await Count("permission_migration_issues"));
await using (var issues = db.Database.GetDbConnection().CreateCommand())
{
    issues.Transaction = tx.GetDbTransaction();
    issues.CommandText = "SELECT resource_table, issue, count(*) FROM permission_migration_issues GROUP BY resource_table, issue ORDER BY resource_table, issue";
    await using var rows = await issues.ExecuteReaderAsync();
    while (await rows.ReadAsync())
        Console.WriteLine($"RECONCILE {rows.GetString(0)} / {rows.GetString(1)}: {rows.GetInt64(2)}");
}
await tx.RollbackAsync();
Console.WriteLine("PASS migrations rolled back on restore; source remains unchanged");
await db.Database.CloseConnectionAsync();
await db.Database.MigrateAsync();
Console.WriteLine("PASS standard EF migrator applied pending history on restored backup");
var dataSourceBuilder = new NpgsqlDataSourceBuilder(local);
dataSourceBuilder.EnableDynamicJson();
await using var dataSource = dataSourceBuilder.Build();
await using var accessDb = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(dataSource).Options);
var access = new AISAM.Services.Access.AccessControlService(accessDb);
var members = await accessDb.WorkspaceMembers.AsNoTracking().Where(m => m.IsActive && m.User.IsActive &&
    m.Workspace.Status != AISAM.Data.Enumeration.WorkspaceStatusEnum.Deleted).Take(20).ToListAsync();
if (members.Count == 0) throw new Exception("No active membership for resolver smoke check.");
foreach (var member in members)
{
    var ids=await access.GetAccessibleBrandIdsAsync(member.UserId,member.WorkspaceId);
    foreach(var id in ids.Take(2))
    {
        var decision=await access.CheckAsync(new(member.UserId,member.WorkspaceId,AISAM.Services.Access.AccessResourceKind.Brand,id,AISAM.Services.Access.ResourcePermission.BrandView));
        if(!decision.Allowed) throw new Exception("Brand scope and decision differ.");
    }
}
Console.WriteLine("PASS PostgreSQL resolver scope/Brand checks for active memberships (no source writes)");
var performance=new AISAM.Services.Access.MemberPerformanceService(accessDb,access);
foreach(var member in members.Where(m=>m.Role is AISAM.Data.Enumeration.WorkspaceMemberRoleEnum.Owner or AISAM.Data.Enumeration.WorkspaceMemberRoleEnum.Manager or AISAM.Data.Enumeration.WorkspaceMemberRoleEnum.ContentCreator).Take(3))
    await performance.GetAsync(member.UserId,member.WorkspaceId,DateTime.UtcNow.AddDays(-30),DateTime.UtcNow,pageSize:2);
Console.WriteLine("PASS PostgreSQL Member Performance queries and approval submission migration on restored data");
var owner=await accessDb.WorkspaceMembers.AsNoTracking().FirstOrDefaultAsync(m=>m.IsActive && m.User.IsActive && m.Role==AISAM.Data.Enumeration.WorkspaceMemberRoleEnum.Owner &&
    m.Workspace.Status==AISAM.Data.Enumeration.WorkspaceStatusEnum.Active && (!m.Workspace.SubscriptionExpiredAt.HasValue || m.Workspace.SubscriptionExpiredAt>DateTime.UtcNow) &&
    accessDb.Teams.Any(t=>t.WorkspaceId==m.WorkspaceId && !t.IsDeleted && t.Status==AISAM.Data.Enumeration.TeamStatusEnum.Active) && accessDb.Brands.Any(b=>b.WorkspaceId==m.WorkspaceId && !b.IsDeleted));
if(owner is null) throw new Exception("Owner fixture required.");
var scopedBrands=(await access.GetAccessibleBrandIdsAsync(owner.UserId,owner.WorkspaceId)).ToArray();
accessDb.PermissionWorkspaceId=owner.WorkspaceId; accessDb.PermissionActorId=owner.UserId;
accessDb.PermissionOwner=true; accessDb.PermissionBrandIds=scopedBrands; accessDb.PermissionScopeEnabled=true;
await accessDb.Contents.CountAsync(); await accessDb.Posts.CountAsync(); await accessDb.SocialAccounts.CountAsync();
await accessDb.AutomationPlans.CountAsync(); await accessDb.Notifications.CountAsync(); await accessDb.PerformanceReports.CountAsync();
Console.WriteLine("PASS PostgreSQL query filters translate for content/post/social/automation/notification/analytics");
accessDb.PermissionScopeEnabled=false;
var fixture=await (from t in accessDb.Teams where t.WorkspaceId==owner.WorkspaceId && !t.IsDeleted && t.Status==AISAM.Data.Enumeration.TeamStatusEnum.Active
    from b in accessDb.Brands where b.WorkspaceId==owner.WorkspaceId && !b.IsDeleted select new {Team=t.Id,Brand=b.Id}).FirstOrDefaultAsync();
if(fixture is null) throw new Exception("Assignment fixture required.");
var service=new AISAM.Services.Access.AssignmentService(accessDb,access);
var snapshot=await service.ReadAsync(owner.UserId,owner.WorkspaceId,fixture.Brand);
var auditBefore=await accessDb.AuditLogs.CountAsync();
var changed=await service.ChangeAsync(new(owner.UserId,owner.WorkspaceId,fixture.Brand,fixture.Team,snapshot.Revision,true));
if(changed.Revision==snapshot.Revision || await accessDb.AuditLogs.CountAsync()!=auditBefore+1) throw new Exception("Assignment/audit not saved together.");
try { await service.ChangeAsync(new(owner.UserId,owner.WorkspaceId,fixture.Brand,fixture.Team,snapshot.Revision,false)); throw new Exception("Stale revision accepted."); }
catch(AISAM.Services.Access.AssignmentConflictException) { }
if(await accessDb.AuditLogs.CountAsync()!=auditBefore+1) throw new Exception("Conflict wrote audit.");
Console.WriteLine("PASS assignment mutation/audit commit and stale revision rejection on PostgreSQL");
var foreignTeam=await accessDb.Teams.AsNoTracking().FirstOrDefaultAsync(t=>t.WorkspaceId!=owner.WorkspaceId && !t.IsDeleted);
if(foreignTeam is null) throw new Exception("Cross-workspace Team fixture required.");
try { await service.ChangeAsync(new(owner.UserId,owner.WorkspaceId,fixture.Brand,foreignTeam.Id,changed.Revision,true)); throw new Exception("Cross-workspace grant accepted."); }
catch(AISAM.Services.Access.AssignmentAccessException) { }
if(await accessDb.AuditLogs.CountAsync()!=auditBefore+1) throw new Exception("Denied assignment persisted audit as allowed.");
Console.WriteLine("PASS cross-workspace Team assignment rejected without partial write");
accessDb.PermissionScopeEnabled=true; accessDb.PermissionOwner=false; accessDb.PermissionCreator=true;
accessDb.PermissionTeamIds=[fixture.Team];
await accessDb.WorkspaceMembers.CountAsync(); await accessDb.Teams.CountAsync(); await accessDb.Conversations.CountAsync();
await accessDb.ChatMessages.CountAsync(); await accessDb.Contents.CountAsync(); await accessDb.ContentCalendars.CountAsync();
Console.WriteLine("PASS PostgreSQL non-owner member/conversation/content scope translation");
await MediaSmoke.Run(accessDb,owner.UserId,owner.WorkspaceId,fixture.Brand);
var publishSnapshot=await accessDb.PublishSnapshots.FirstAsync();
var journal=new AISAM.Data.Model.PublishOperation{WorkspaceId=owner.WorkspaceId,ActorId=owner.UserId,ContentId=publishSnapshot.ContentId,SnapshotId=publishSnapshot.Id,IntegrationId=Guid.NewGuid(),IdempotencyKey="smoke-publish",MediaResults="[]"};
accessDb.PublishOperations.Add(journal);
accessDb.PublishRequests.Add(new(){WorkspaceId=owner.WorkspaceId,ActorId=owner.UserId,IdempotencyKey="smoke-publish"});
await accessDb.SaveChangesAsync();
await using(var journalTransaction=await accessDb.Database.BeginTransactionAsync())
{
    bool rejected=false;
    try{await accessDb.Database.ExecuteSqlRawAsync("INSERT INTO publish_requests(id,workspace_id,actor_id,idempotency_key) SELECT gen_random_uuid(),workspace_id,actor_id,idempotency_key FROM publish_requests LIMIT 1");}
    catch(PostgresException e)when(e.SqlState=="23505"){rejected=true;}
    await journalTransaction.RollbackAsync();
    if(!rejected)throw new Exception("Duplicate publish request accepted.");
}
Console.WriteLine("PASS PostgreSQL publish operation JSON and request idempotency unique boundary");
await using(var claimDb=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(dataSource).Options))
await using(var cancelDb=new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(dataSource).Options))
{
    var claim=await claimDb.PublishOperations.SingleAsync(o=>o.Id==journal.Id);
    var cancel=await cancelDb.PublishOperations.SingleAsync(o=>o.Id==journal.Id);
    claim.Status="UploadingMedia";await claimDb.SaveChangesAsync();
    cancel.Status="Cancelled";bool rejected=false;
    try{await cancelDb.SaveChangesAsync();}catch(DbUpdateConcurrencyException){rejected=true;}
    if(!rejected)throw new Exception("Cancellation overwrote a claimed operation.");
    claim.Status="Queued";await claimDb.SaveChangesAsync();
    cancelDb.ChangeTracker.Clear();cancel=await cancelDb.PublishOperations.SingleAsync(o=>o.Id==journal.Id);
    cancel.Status="Cancelled";await cancelDb.SaveChangesAsync();
    claim.Status="UploadingMedia";rejected=false;
    try{await claimDb.SaveChangesAsync();}catch(DbUpdateConcurrencyException){rejected=true;}
    if(!rejected)throw new Exception("Publish claim overwrote cancellation.");
}
Console.WriteLine("PASS PostgreSQL concurrent claim/cancel reject stale writes in both orders");
accessDb.ChangeTracker.Clear();
await accessDb.Database.ExecuteSqlRawAsync("UPDATE content_calendar SET scheduled_at=now()+interval '30 days'");
var schedulerFixtures=await accessDb.ContentCalendars.IgnoreQueryFilters().AsNoTracking().Where(s=>!s.IsDeleted).Take(2).Select(s=>s.Id).ToArrayAsync();
if(schedulerFixtures.Length!=2)throw new Exception("Two schedules required for claim regression.");
await accessDb.Database.ExecuteSqlInterpolatedAsync($"UPDATE content_calendar SET status={(int)AISAM.Data.Enumeration.ScheduleStatusEnum.Failed}, is_active=true, attempt_count=0, scheduled_at=now()-interval '1 hour' WHERE id={schedulerFixtures[0]}");
await accessDb.Database.ExecuteSqlInterpolatedAsync($"UPDATE content_calendar SET status={(int)AISAM.Data.Enumeration.ScheduleStatusEnum.Pending}, is_active=true, attempt_count=1, updated_at=now(), scheduled_at=now()-interval '1 hour' WHERE id={schedulerFixtures[1]}");
var schedulerRepository=new AISAM.Repositories.Repository.ContentCalendarRepository(accessDb);
if((await schedulerRepository.ClaimDueSchedulesAtomicallyAsync(DateTime.UtcNow,10,3)).Count!=0)throw new Exception("Failed or backoff schedule was claimed.");
await accessDb.Database.ExecuteSqlInterpolatedAsync($"UPDATE content_calendar SET updated_at=now()-interval '10 minutes' WHERE id={schedulerFixtures[1]}");
var claimedSchedules=await schedulerRepository.ClaimDueSchedulesAtomicallyAsync(DateTime.UtcNow,10,3);
if(claimedSchedules.Count!=1||claimedSchedules[0].Id!=schedulerFixtures[1])throw new Exception("Retry-ready schedule not claimed exclusively.");
if((await schedulerRepository.ClaimDueSchedulesAtomicallyAsync(DateTime.UtcNow,10,3)).Count!=0)throw new Exception("Processing schedule claimed twice.");
Console.WriteLine("PASS PostgreSQL scheduler excludes Failed, enforces retry delay and claims Pending only once");
await accessDb.Database.ExecuteSqlInterpolatedAsync($"UPDATE content_calendar SET status={(int)AISAM.Data.Enumeration.ScheduleStatusEnum.Pending}, attempt_count=0 WHERE id={schedulerFixtures[1]}");
await using (var workerA = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(dataSource).Options))
await using (var workerB = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(dataSource).Options))
{
    var claims = await Task.WhenAll(
        new AISAM.Repositories.Repository.ContentCalendarRepository(workerA).ClaimDueSchedulesAtomicallyAsync(DateTime.UtcNow, 10, 3),
        new AISAM.Repositories.Repository.ContentCalendarRepository(workerB).ClaimDueSchedulesAtomicallyAsync(DateTime.UtcNow, 10, 3));
    if (claims.Sum(c => c.Count) != 1) throw new Exception("Concurrent workers did not claim exactly one schedule.");
}
await accessDb.Database.ExecuteSqlInterpolatedAsync($"UPDATE content_calendar SET updated_at=now()-interval '25 hours' WHERE id={schedulerFixtures[1]}");
accessDb.ChangeTracker.Clear();
var recoveryService = new AISAM.Services.Service.ScheduledPostingService(schedulerRepository, null!, null!, null!, null!, null!, accessDb);
await recoveryService.RunDueSchedulesAsync(10);
var recoveredSchedule = await accessDb.ContentCalendars.AsNoTracking().SingleAsync(s => s.Id == schedulerFixtures[1]);
if (recoveredSchedule.Status != AISAM.Data.Enumeration.ScheduleStatusEnum.Failed || recoveredSchedule.LastError != "PUBLISH_OUTCOME_UNKNOWN")
    throw new Exception("Abandoned claim was not quarantined for reconciliation.");
Console.WriteLine("PASS PostgreSQL two simultaneous scheduler claims and abandoned-claim quarantine");
await using (var lockDb = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(dataSource).Options))
{
    const long lockKey = 0x414953414D47454E;
    var firstLock = await AISAM.Services.Service.AutomationExecutionLock.TryAcquireAsync(accessDb, lockKey, default);
    if (firstLock is null) throw new Exception("Automation lock unavailable in isolated database.");
    await using (var contender = await AISAM.Services.Service.AutomationExecutionLock.TryAcquireAsync(lockDb, lockKey, default))
        if (contender is not null) throw new Exception("Two automation workers acquired the same execution lock.");
    await firstLock.DisposeAsync();
    await using var recovered = await AISAM.Services.Service.AutomationExecutionLock.TryAcquireAsync(lockDb, lockKey, default);
    if (recovered is null) throw new Exception("Disconnected automation worker did not release lock.");
}
Console.WriteLine("PASS PostgreSQL automation lock excludes second worker and releases on disconnect");
accessDb.PermissionScopeEnabled = false;
accessDb.ChangeTracker.Clear();
var richContent = new AISAM.Data.Model.Content { WorkspaceId=owner.WorkspaceId, ProfileId=await accessDb.Brands.Where(b=>b.Id==fixture.Brand).Select(b=>b.ProfileId).FirstAsync(),
    BrandId=fixture.Brand, TeamId=fixture.Team, PrimaryCreatorId=owner.UserId, TextContent="client text must be replaced",
    RichTextVersion=1, RichTextJson="""{"type":"doc","content":[{"type":"paragraph","content":[{"type":"text","text":"Unicode 👩‍💻 #AISAM","marks":[{"type":"bold"}]}]}]}""" };
accessDb.Contents.Add(richContent); await accessDb.SaveChangesAsync();
richContent.Status=AISAM.Data.Enumeration.ContentStatusEnum.Approved; await accessDb.SaveChangesAsync();
var richId=richContent.Id; var richSnapshot=richContent.ApprovedSnapshotId;
accessDb.ChangeTracker.Clear();
richContent=await accessDb.Contents.SingleAsync(c=>c.Id==richId);
if(richContent.RichTextVersion!=1 || richContent.TextContent!="Unicode 👩‍💻 #AISAM")throw new Exception("Rich text did not round trip through PostgreSQL JSONB.");
var frozenRich=await accessDb.PublishSnapshots.AsNoTracking().SingleAsync(s=>s.Id==richSnapshot);
var frozenPayload=System.Text.Json.JsonSerializer.Deserialize<AISAM.Data.Model.Content>(frozenRich.Payload)!;
if(frozenPayload.FormattedCaptions?["facebook"]!=richContent.TextContent)throw new Exception("Formatted caption was not frozen.");
richContent.RichTextJson=richContent.RichTextJson!.Replace("bold","italic");await accessDb.SaveChangesAsync();
if(richContent.Status!=AISAM.Data.Enumeration.ContentStatusEnum.Draft || richContent.ApprovedSnapshotId!=null)throw new Exception("Formatting edit reused approval.");
Console.WriteLine("PASS PostgreSQL rich JSON/version, derived text, frozen captions and formatting-only invalidation");
await accessDb.GetService<IMigrator>().MigrateAsync("20260908142531_AddApprovalSubmissionTimestamp");
await accessDb.Database.MigrateAsync();
Console.WriteLine("PASS T06 downgrade/reapply on isolated restore (new media data is discarded by downgrade)");
await QueryLoadCheck.RunAsync(dataSource, owner.UserId, owner.WorkspaceId, Path.Combine(root, "query-load.json"));
