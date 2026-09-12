using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AISAM.Services.Service;

// A dedicated transaction holds only the execution lock, independently of application
// SaveChanges. Transaction scope also works through transaction-mode PostgreSQL poolers.
public sealed class AutomationExecutionLock : IAsyncDisposable
{
    private readonly NpgsqlConnection? _connection;
    private AutomationExecutionLock(NpgsqlConnection? connection) => _connection = connection;

    public static async Task<AutomationExecutionLock?> TryAcquireAsync(AisamContext db, long key, CancellationToken ct)
    {
        if (!db.Database.IsNpgsql()) return new(null);
        var rawConnectionString = db.Database.GetConnectionString();
        var configuration = new NpgsqlConnectionStringBuilder(rawConnectionString) { Pooling = false };
        if (string.IsNullOrEmpty(configuration.Password))
        {
            try
            {
                var fallbackString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
                    ?? AisamContextFactory.ResolveConnectionString();
                var fallbackBuilder = new NpgsqlConnectionStringBuilder(fallbackString);
                if (!string.IsNullOrEmpty(fallbackBuilder.Password))
                {
                    configuration.Password = fallbackBuilder.Password;
                }
            }
            catch
            {
                // Fallback resolution is best-effort
            }
        }
        var connection = new NpgsqlConnection(configuration.ConnectionString);
        try
        {
            await connection.OpenAsync(ct);
            var transaction = await connection.BeginTransactionAsync(ct);
            await using var command = new NpgsqlCommand("SELECT pg_try_advisory_xact_lock(@key)", connection, transaction);
            command.Parameters.AddWithValue("key", key);
            if (await command.ExecuteScalarAsync(ct) is true) return new(connection);
            await connection.DisposeAsync();
            return null;
        }
        catch { await connection.DisposeAsync(); throw; }
    }

    public ValueTask DisposeAsync() => _connection?.DisposeAsync() ?? ValueTask.CompletedTask;
}
