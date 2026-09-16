using System.Text.Json;
using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

// Read-only inspection of the configured database. Never print connection strings,
// user rows, credentials, or provider tokens. Never start API background workers.
var result = new Dictionary<string, object?> { ["checkedAtUtc"] = DateTime.UtcNow };
try
{
    var settings = new NpgsqlConnectionStringBuilder(AisamContextFactory.ResolveConnectionString())
    {
        Timeout = 15, CommandTimeout = 30, Pooling = false,
        Options = "-c default_transaction_read_only=on"
    };
    await using var connection = new NpgsqlConnection(settings.ConnectionString);
    await connection.OpenAsync();
    await using var db = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
        .UseNpgsql(connection).Options);
    result["databaseReachable"] = true;
    result["readOnly"] = true;
    result["appliedMigrations"] = (await db.Database.GetAppliedMigrationsAsync()).ToArray();
    var pending = (await db.Database.GetPendingMigrationsAsync()).ToArray();
    result["pendingMigrations"] = pending;
    if (pending.Length > 0) Environment.ExitCode = 1;
    var counts = new Dictionary<string, long>();
    foreach (var table in new[] { "contents", "posts", "assets", "automation_plans", "social_integrations" })
    {
        await using var command = new NpgsqlCommand($"SELECT count(*) FROM public.{table}", connection);
        counts[table] = Convert.ToInt64(await command.ExecuteScalarAsync());
    }
    result["counts"] = counts;
    var columns = new HashSet<string>();
    await using (var command = new NpgsqlCommand("SELECT table_name,column_name FROM information_schema.columns WHERE table_schema='public'", connection))
    await using (var reader = await command.ExecuteReaderAsync())
        while (await reader.ReadAsync()) columns.Add(reader.GetString(0) + "." + reader.GetString(1));
    var missing = new List<string>();
    foreach (var table in db.Model.GetRelationalModel().Tables)
        foreach (var column in table.Columns)
            if (!columns.Contains(table.Name + "." + column.Name))
                missing.Add($"{table.Name}.{column.Name} ({column.StoreType}, nullable={column.IsNullable})");
    result["missingModelColumns"] = missing;
    if (missing.Count > 0) Environment.ExitCode = 1;
    if (args.Contains("--rbac-v2"))
    {
        await using var viewCheck = new NpgsqlCommand("SELECT to_regclass('public.rbac_v2_preflight') IS NOT NULL", connection);
        var exists = (bool)(await viewCheck.ExecuteScalarAsync())!;
        result["rbacPreflightAvailable"] = exists;
        if (!exists) Environment.ExitCode = 1;
        else
        {
            var issues = new Dictionary<string, long>();
            await using var command = new NpgsqlCommand("SELECT issue,count(*) FROM public.rbac_v2_preflight GROUP BY issue ORDER BY issue", connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) issues[reader.GetString(0)] = reader.GetInt64(1);
            result["rbacIssues"] = issues;
            result["rbacReviewRequired"] = issues.Count > 0;
            // Closed legacy scopes need an explicit disposition, never automatic enabling.
            if (issues.Count > 0) Environment.ExitCode = 1;
        }
    }
}
catch (Exception ex)
{
    // Exception messages may include database host/user details.
    result["errorType"] = ex.GetType().Name;
    if (ex is PostgresException pg) result["sqlState"] = pg.SqlState;
    Environment.ExitCode = 1;
}
var output = Path.GetFullPath("../.artifacts/final-acceptance/database-preflight.json");
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
await File.WriteAllTextAsync(output, JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine(JsonSerializer.Serialize(result));
