using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;

// Run from AISAM-BE. Uses the existing design-time configuration loader.
// No identifiers, credentials, URLs or content text are included in the report.
if (args.Length != 1) { Console.Error.WriteLine("Usage: PermissionPreflight <report.md>"); return 2; }
try
{
    await using var configurationContext = new AisamContextFactory().CreateDbContext([]);
    var conn = (NpgsqlConnection)configurationContext.Database.GetDbConnection();
    // No execution retries: the entire explicit read-only transaction must be
    // one snapshot, not independently retried EF operations.
    await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>().UseNpgsql(conn).Options);
    await conn.OpenAsync();
    await using var tx = await conn.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead);
    await using (var cmd = new NpgsqlCommand("SET TRANSACTION READ ONLY; SET LOCAL statement_timeout = '20s'", conn, tx))
        await cmd.ExecuteNonQueryAsync();
    await db.Database.UseTransactionAsync(tx);
    // Materialize bounded samples to verify the reconciled EF mappings, without
    // exposing resource names or any row-level data in the report.
    await db.Teams.AsNoTracking().Take(1).ToListAsync();
    await db.TeamBrands.AsNoTracking().Take(1).ToListAsync();
    await db.TeamChannelAccesses.AsNoTracking().Take(1).ToListAsync();
    await db.Contents.AsNoTracking().Take(1).ToListAsync();
    var report = new StringBuilder("# T00 — Database preflight\n\n");
    report.AppendLine($"Generated UTC: {DateTime.UtcNow:O}\n");
    report.AppendLine("Read-only repeatable-read transaction. Aggregate counts and schema metadata only; no database mutations.\n");
    report.AppendLine("EF mapping smoke check: Team, TeamBrand, TeamChannelAccess and Content queries passed.\n");
    var queries = new (string Name, string Sql)[]
    {
        ("Permission schema inventory", """
        SELECT table_name || '.' || column_name, data_type || ', nullable=' || is_nullable
        FROM information_schema.columns WHERE table_schema='public' AND
        (table_name IN ('teams','team_brands','team_members','posts','content_calendar','audit_logs','automation_plans') OR
        table_name LIKE 'team%channel%' OR table_name LIKE 'content%particip%' OR
        (table_name='contents' AND (column_name LIKE '%creator%' OR column_name LIKE '%user%' OR column_name LIKE '%team%')))
        ORDER BY table_name,ordinal_position
        """),
        ("Table counts", """
        SELECT 'teams' AS metric, count(*)::text AS value FROM teams
        UNION ALL SELECT 'team_brands', count(*)::text FROM team_brands
        UNION ALL SELECT 'team_members', count(*)::text FROM team_members
        UNION ALL SELECT 'contents', count(*)::text FROM contents
        UNION ALL SELECT 'posts', count(*)::text FROM posts
        UNION ALL SELECT 'assets', count(*)::text FROM assets
        UNION ALL SELECT 'schedules', count(*)::text FROM content_calendar
        UNION ALL SELECT 'integrations', count(*)::text FROM social_integrations
        """),
        ("Migration risks", """
        SELECT 'teams_without_workspace_via_legacy_profile' AS metric, count(*)::text AS value
        FROM teams t LEFT JOIN profiles p ON p.id=t.profile_id LEFT JOIN workspaces w ON w.id=p.workspace_id WHERE w.id IS NULL
        UNION ALL SELECT 'cross_workspace_team_brand', count(*)::text
        FROM team_brands tb JOIN teams t ON t.id=tb.team_id JOIN profiles p ON p.id=t.profile_id JOIN brands b ON b.id=tb.brand_id
        WHERE p.workspace_id IS DISTINCT FROM b.workspace_id
        UNION ALL SELECT 'duplicate_team_brand_pairs', count(*)::text
        FROM (SELECT team_id,brand_id FROM team_brands GROUP BY team_id,brand_id HAVING count(*)>1) d
        UNION ALL SELECT 'duplicate_team_member_pairs', count(*)::text
        FROM (SELECT team_id,user_id FROM team_members GROUP BY team_id,user_id HAVING count(*)>1) d
        UNION ALL SELECT 'content_brand_workspace_mismatch', count(*)::text
        FROM contents c JOIN brands b ON b.id=c.brand_id WHERE c.workspace_id IS DISTINCT FROM b.workspace_id
        UNION ALL SELECT 'post_channel_brand_mismatch', count(*)::text
        FROM posts p JOIN contents c ON c.id=p.content_id JOIN social_integrations i ON i.id=p.integration_id
        WHERE c.workspace_id IS DISTINCT FROM i.workspace_id OR c.brand_id IS DISTINCT FROM i.brand_id
        UNION ALL SELECT 'legacy_contents_with_video', count(*)::text FROM contents WHERE video_url IS NOT NULL AND video_url<>''
        UNION ALL SELECT 'legacy_contents_with_images', count(*)::text FROM contents WHERE image_url IS NOT NULL
        UNION ALL SELECT 'assets_without_uploader', count(*)::text FROM assets WHERE uploaded_by IS NULL
        """),
        ("Current database Team workspace (schema drift check)", """
        SELECT 'teams_invalid_direct_workspace' AS metric, count(*)::text AS value
        FROM teams t LEFT JOIN workspaces w ON w.id=t.workspace_id WHERE w.id IS NULL
        UNION ALL SELECT 'team_brand_direct_workspace_mismatch', count(*)::text
        FROM team_brands tb JOIN teams t ON t.id=tb.team_id JOIN brands b ON b.id=tb.brand_id
        WHERE t.workspace_id IS DISTINCT FROM b.workspace_id
        UNION ALL SELECT 'team_profile_missing', count(*)::text FROM teams WHERE profile_id IS NULL
        UNION ALL SELECT 'contents_without_primary_creator', count(*)::text FROM contents WHERE primary_creator_id IS NULL
        UNION ALL SELECT 'contents_with_video_urls', count(*)::text FROM contents WHERE video_urls IS NOT NULL
        """),
        ("Attribution and media schema", """
        SELECT table_name || '.' || column_name AS field, data_type || ', nullable=' || is_nullable AS definition
        FROM information_schema.columns WHERE table_schema='public' AND
        (table_name IN ('teams','posts','contents','assets') AND
        column_name IN ('workspace_id','profile_id','created_by_user_id','creator_user_id','published_by_user_id','updated_by_user_id',
        'primary_creator_id','created_by','published_by','scheduled_by','video_urls','image_url','video_url','rich_text_json','rich_text_version','uploaded_by','metadata')
        OR table_name IN ('content_media','post_media','team_social_channels')) ORDER BY table_name,ordinal_position
        """),
        ("Assignment indexes", "SELECT tablename, indexdef FROM pg_indexes WHERE schemaname='public' AND tablename IN ('team_brands','team_members') ORDER BY tablename,indexname"),
        ("Applied migrations", "SELECT \"MigrationId\", \"ProductVersion\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\"")
    };
    foreach (var (name, sql) in queries)
    {
        report.AppendLine($"## {name}\n");
        await using var cmd = new NpgsqlCommand(sql, conn, tx) { CommandTimeout = 25 };
        await using var reader = await cmd.ExecuteReaderAsync();
        report.AppendLine("| Metric / field | Value / definition |");
        report.AppendLine("|---|---|");
        while (await reader.ReadAsync())
            report.AppendLine($"| {reader.GetValue(0).ToString()!.Replace("|", "\\|")} | {reader.GetValue(1).ToString()!.Replace("|", "\\|")} |");
        report.AppendLine();
    }
    await tx.RollbackAsync();
    await File.WriteAllTextAsync(args[0], report.ToString(), new UTF8Encoding(false));
    Console.WriteLine("Preflight complete; read-only report written.");
    return 0;
}
catch (Exception ex)
{
    // Do not print exception messages: provider failures can include connection details.
    Console.Error.WriteLine($"Preflight failed: {ex.GetType().Name}" + (ex is PostgresException pg ? $" SQLSTATE={pg.SqlState}" : ""));
    return 1;
}
