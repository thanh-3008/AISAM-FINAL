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
        var configuration = new NpgsqlConnectionStringBuilder(db.Database.GetConnectionString()) { Pooling = false };
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
