using AISAM.Repositories;
using AISAM.Repositories.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

// Deliberately fixed to the isolated local cluster; never reads project .env.
const string connection = "Host=127.0.0.1;Port=55439;Database=postgres;Username=permission_test;Pooling=false";
await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(connection).Options);
var generator = db.GetService<IMigrationsSqlGenerator>();
string Sql(Migration migration) => string.Join("\n", generator.Generate(migration.UpOperations).Select(c => c.CommandText));
var foundation = Sql(new ReconcilePermissionFoundation());
var attribution = Sql(new AddAutomationCreatorAttribution());
const string schema = """
CREATE TABLE workspaces(id uuid PRIMARY KEY);
CREATE TABLE profiles(id uuid PRIMARY KEY, workspace_id uuid);
CREATE TABLE teams(id uuid PRIMARY KEY, profile_id uuid NOT NULL);
CREATE TABLE brands(id uuid PRIMARY KEY, workspace_id uuid);
CREATE TABLE team_brands(id uuid PRIMARY KEY, team_id uuid, brand_id uuid);
CREATE TABLE team_members(id uuid PRIMARY KEY, team_id uuid, user_id uuid);
CREATE TABLE contents(id uuid PRIMARY KEY);
CREATE TABLE social_integrations(id uuid PRIMARY KEY, workspace_id uuid, brand_id uuid);
CREATE TABLE automation_plans(id uuid PRIMARY KEY);
INSERT INTO workspaces VALUES ('00000000-0000-0000-0000-000000000001');
INSERT INTO profiles VALUES ('00000000-0000-0000-0000-000000000002','00000000-0000-0000-0000-000000000001');
INSERT INTO teams VALUES ('00000000-0000-0000-0000-000000000003','00000000-0000-0000-0000-000000000002');
INSERT INTO brands VALUES ('00000000-0000-0000-0000-000000000004','00000000-0000-0000-0000-000000000001');
INSERT INTO team_brands VALUES ('00000000-0000-0000-0000-000000000005','00000000-0000-0000-0000-000000000003','00000000-0000-0000-0000-000000000004');
INSERT INTO contents VALUES ('00000000-0000-0000-0000-000000000006');
INSERT INTO automation_plans VALUES ('00000000-0000-0000-0000-000000000007');
""";
var cases = new (string Name, string Setup, string? ExpectedError)[]
{
    ("legacy schema backfill and nullable attribution", "", null),
    ("already reconciled schema", foundation, null),
    ("channel from another Brand rejects", """
    CREATE TABLE team_channel_access(id uuid PRIMARY KEY, team_brand_id uuid, integration_id uuid);
    INSERT INTO social_integrations VALUES ('00000000-0000-0000-0000-000000000009',
      '00000000-0000-0000-0000-000000000001','00000000-0000-0000-0000-000000000099');
    INSERT INTO team_channel_access VALUES ('00000000-0000-0000-0000-000000000010',
      '00000000-0000-0000-0000-000000000005','00000000-0000-0000-0000-000000000009');
    """, "P0001"),
    ("unknown Team workspace rejects", "DELETE FROM profiles;", "P0001"),
    ("cross-workspace TeamBrand rejects", "ALTER TABLE teams ADD workspace_id uuid; UPDATE teams SET workspace_id='00000000-0000-0000-0000-000000000001'; UPDATE brands SET workspace_id='00000000-0000-0000-0000-000000000099';", "P0001"),
    ("duplicate TeamBrand rejects", "INSERT INTO team_brands SELECT '00000000-0000-0000-0000-000000000008',team_id,brand_id FROM team_brands;", "23505")
};
await using var conn = new NpgsqlConnection(connection);
await conn.OpenAsync();
foreach (var test in cases)
{
    await using var tx = await conn.BeginTransactionAsync();
    async Task Execute(string sql) { await using var cmd = new NpgsqlCommand(sql, conn, tx); await cmd.ExecuteNonQueryAsync(); }
    // Every case rolls back even its fixture schema.
    await Execute(schema);
    if (test.Setup.Length > 0) await Execute(test.Setup);
    string? error = null;
    try
    {
        await Execute(foundation);
        await Execute(attribution);
        await Execute("""
        DO $$ BEGIN
          IF (SELECT workspace_id FROM teams LIMIT 1) IS DISTINCT FROM '00000000-0000-0000-0000-000000000001'::uuid
            THEN RAISE EXCEPTION 'Backfill incorrect'; END IF;
          IF EXISTS (SELECT 1 FROM contents WHERE primary_creator_id IS NOT NULL)
            OR EXISTS (SELECT 1 FROM automation_plans WHERE created_by_user_id IS NOT NULL)
            THEN RAISE EXCEPTION 'Invented attribution'; END IF;
        END $$;
        """);
    }
    catch (PostgresException ex) { error = ex.SqlState; }
    await tx.RollbackAsync();
    if (error != test.ExpectedError) throw new Exception($"FAIL {test.Name}: expected {test.ExpectedError ?? "success"}, got {error ?? "success"}");
    Console.WriteLine($"PASS {test.Name}");
}
await using (var check = new NpgsqlCommand("SELECT to_regclass('public.teams') IS NULL AND to_regclass('public.automation_plans') IS NULL", conn))
    if (!Equals(await check.ExecuteScalarAsync(), true)) throw new Exception("Fixture rollback left tables behind.");
Console.WriteLine("PASS fixture transaction rollback leaves no tables");
Console.WriteLine("All fixtures rolled back. This checks migration operations on synthetic PostgreSQL 18 data, not a production backup or the full migration history.");
