using AISAM.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using MysticMind.PostgresEmbed;
using Npgsql;
using Xunit.Abstractions;

namespace AISAM.IntegrationTests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ContentOwnershipPostgresCollection : ICollectionFixture<EmbeddedPostgresFixture>
{
    public const string Name = "Content ownership PostgreSQL runtime";
}

public sealed class EmbeddedPostgresFixture : IAsyncLifetime
{
    private PgServer? server;

    public string ConnectionString => new NpgsqlConnectionStringBuilder
    {
        Host = "127.0.0.1",
        Port = server?.PgPort ?? throw new InvalidOperationException("Embedded PostgreSQL is not running."),
        Database = "postgres",
        Username = "postgres",
        Password = "test-only",
        Pooling = false,
        IncludeErrorDetail = true
    }.ConnectionString;

    public async Task InitializeAsync()
    {
        var cacheRoot = Path.Combine(Path.GetTempPath(), "aisam-embedded-postgres-cache");
        server = new PgServer(
            "15.3.0",
            dbDir: cacheRoot,
            clearInstanceDirOnStop: true,
            addLocalUserAccessPermission: false,
            startupWaitTime: 60_000);
        await server.StartAsync();
        Console.WriteLine("Embedded PostgreSQL started on an ephemeral port.");
    }

    public async Task DisposeAsync()
    {
        if (server != null) await server.DisposeAsync();
        Console.WriteLine("Embedded PostgreSQL stopped; ephemeral instance data was removed.");
    }
}

[Collection(ContentOwnershipPostgresCollection.Name)]
public sealed class ContentOwnershipPostgreSqlRuntimeTests(EmbeddedPostgresFixture postgres, ITestOutputHelper output)
{
    private const string PreviousMigration = "20260904112639_MutationPermissionRevision";
    private const string TargetMigration = "20260906041041_ContentOwnershipBoundary";

    private const string WorkspaceA = "10000000-0000-0000-0000-000000000001";
    private const string WorkspaceB = "10000000-0000-0000-0000-000000000002";
    private const string TeamA = "20000000-0000-0000-0000-000000000001";
    private const string TeamB = "20000000-0000-0000-0000-000000000002";
    private const string ForeignTeam = "20000000-0000-0000-0000-000000000003";
    private const string UniqueBrand = "30000000-0000-0000-0000-000000000001";
    private const string SharedBrand = "30000000-0000-0000-0000-000000000002";
    private const string UnavailableBrand = "30000000-0000-0000-0000-000000000003";
    private const string UniqueLegacyContent = "40000000-0000-0000-0000-000000000001";
    private const string AmbiguousLegacyContent = "40000000-0000-0000-0000-000000000002";
    private const string ValidOwnedContent = "40000000-0000-0000-0000-000000000003";

    [Fact]
    public async Task MigrationEnforcesOwnershipBoundaryOnRealPostgreSql()
    {
        await using var connection = new NpgsqlConnection(postgres.ConnectionString);
        await connection.OpenAsync();
        await ExecuteAsync(connection, PreMigrationSchemaSql);
        await ExecuteAsync(connection, PreMigrationSeedSql);
        Assert.Equal(2L, await CountAsync(connection, "SELECT COUNT(*) FROM contents;"));
        Assert.Equal(1L, await CountAsync(connection,
            $"SELECT COUNT(DISTINCT team_id) FROM team_brands WHERE brand_id = '{UniqueBrand}'::uuid AND is_active;"));
        Assert.Equal(2L, await CountAsync(connection,
            $"SELECT COUNT(DISTINCT team_id) FROM team_brands WHERE brand_id = '{SharedBrand}'::uuid AND is_active;"));
        Assert.Equal(0L, await CountAsync(connection, """
            SELECT COUNT(*) FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = 'contents' AND column_name = 'team_id';
            """));
        output.WriteLine("SEED PASS: unique-team and shared-brand legacy contents created before team_id exists.");

        await using var context = new AisamContext(new DbContextOptionsBuilder<AisamContext>()
            .UseNpgsql(postgres.ConnectionString).Options);
        var migrator = context.GetService<IMigrator>();
        var migrationScript = migrator.GenerateScript(PreviousMigration, TargetMigration, MigrationsSqlGenerationOptions.Idempotent);
        await ExecuteAsync(connection, migrationScript);
        Assert.Equal(1L, await CountAsync(connection,
            $"SELECT COUNT(*) FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = '{TargetMigration}';"));
        Assert.Equal(1L, await CountAsync(connection, """
            SELECT COUNT(*) FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = 'contents' AND column_name = 'team_id' AND udt_name = 'uuid';
            """));
        Assert.Equal(1L, await CountAsync(connection, """
            SELECT COUNT(*) FROM pg_constraint
            WHERE conrelid = 'contents'::regclass AND contype = 'f'
              AND conname = 'FK_contents_teams_team_id_workspace_id';
            """));
        Assert.Equal(2L, await CountAsync(connection, """
            SELECT COUNT(*) FROM pg_trigger
            WHERE tgrelid = 'contents'::regclass AND NOT tgisinternal
              AND tgname IN ('content_team_boundary', 'content_team_immutable');
            """));
        var serverVersion = await ScalarAsync(connection, "SHOW server_version;");
        Assert.Equal("15.3", serverVersion);
        output.WriteLine($"MIGRATION APPLY #1 PASS: target migration executed on PostgreSQL {serverVersion} with column, FK and triggers present.");

        var uniqueOwner = await ScalarAsync(connection,
            $"SELECT COALESCE(team_id::text, 'NULL') FROM contents WHERE id = '{UniqueLegacyContent}'::uuid;");
        var ambiguousOwner = await ScalarAsync(connection,
            $"SELECT COALESCE(team_id::text, 'NULL') FROM contents WHERE id = '{AmbiguousLegacyContent}'::uuid;");
        Assert.Equal(TeamA, uniqueOwner);
        Assert.Equal("NULL", ambiguousOwner);
        output.WriteLine($"BACKFILL PASS: unique={uniqueOwner}; shared-brand ambiguous={ambiguousOwner}.");

        await ExecuteAsync(connection, $"""
            INSERT INTO contents (id, workspace_id, brand_id, team_id)
            VALUES ('{ValidOwnedContent}'::uuid, '{WorkspaceA}'::uuid, '{SharedBrand}'::uuid, '{TeamA}'::uuid);
            """);
        Assert.Equal(TeamA, await ScalarAsync(connection,
            $"SELECT team_id::text FROM contents WHERE id = '{ValidOwnedContent}'::uuid;"));
        output.WriteLine("VALID OWNERSHIP PASS: explicit owning-team content inserted.");

        await AssertPostgresRejectsAsync(connection,
            "ALTER TABLE contents DISABLE TRIGGER content_team_boundary;",
            $"""
            INSERT INTO contents (id, workspace_id, brand_id, team_id)
            VALUES ('40000000-0000-0000-0000-000000000004'::uuid, '{WorkspaceA}'::uuid, '{SharedBrand}'::uuid, '{ForeignTeam}'::uuid);
            """,
            "ALTER TABLE contents ENABLE TRIGGER content_team_boundary;",
            PostgresErrorCodes.ForeignKeyViolation,
            "FK_REJECT",
            expectedConstraint: "FK_contents_teams_team_id_workspace_id");

        await AssertPostgresRejectsAsync(connection, null,
            $"UPDATE contents SET team_id = '{TeamB}'::uuid WHERE id = '{ValidOwnedContent}'::uuid;",
            null, PostgresErrorCodes.RaiseException, "IMMUTABILITY_REJECT", "Content team ownership is immutable.");

        await AssertPostgresRejectsAsync(connection, null,
            $"""
            INSERT INTO contents (id, workspace_id, brand_id, team_id)
            VALUES ('40000000-0000-0000-0000-000000000005'::uuid, '{WorkspaceA}'::uuid, '{UnavailableBrand}'::uuid, '{TeamA}'::uuid);
            """,
            null, PostgresErrorCodes.RaiseException, "BOUNDARY_REJECT",
            "Content owning team must be active in the workspace and have access to the brand.");

        await AssertPostgresRejectsAsync(connection, null,
            $"""
            INSERT INTO contents (id, workspace_id, brand_id, team_id)
            VALUES ('40000000-0000-0000-0000-000000000006'::uuid, '{WorkspaceA}'::uuid, '{SharedBrand}'::uuid, NULL);
            """,
            null, PostgresErrorCodes.RaiseException, "NULL_OWNER_REJECT", "New content requires an owning team.");

        var beforeSecondApply = await BoundaryStateSnapshotAsync(connection);
        await ExecuteAsync(connection, migrationScript);
        var afterSecondApply = await BoundaryStateSnapshotAsync(connection);
        Assert.Equal(beforeSecondApply, afterSecondApply);
        Assert.Equal(1L, await CountAsync(connection,
            $"SELECT COUNT(*) FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = '{TargetMigration}';"));
        output.WriteLine("MIGRATION APPLY #2 PASS: idempotent script completed and ownership snapshot was unchanged.");
    }

    private async Task AssertPostgresRejectsAsync(
        NpgsqlConnection connection,
        string? before,
        string violatingSql,
        string? after,
        string expectedSqlState,
        string label,
        string? expectedMessage = null,
        string? expectedConstraint = null)
    {
        if (before != null) await ExecuteAsync(connection, before);
        try
        {
            var error = await Assert.ThrowsAsync<PostgresException>(() => ExecuteAsync(connection, violatingSql));
            Assert.Equal(expectedSqlState, error.SqlState);
            if (expectedMessage != null) Assert.Equal(expectedMessage, error.MessageText);
            if (expectedConstraint != null) Assert.Equal(expectedConstraint, error.ConstraintName);
            output.WriteLine($"{label} PASS: SQLSTATE={error.SqlState}; CONSTRAINT={error.ConstraintName ?? "none"}; MESSAGE={error.MessageText}");
        }
        finally
        {
            if (after != null) await ExecuteAsync(connection, after);
        }
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql) =>
        await new NpgsqlCommand(sql, connection).ExecuteNonQueryAsync();

    private static async Task<string> ScalarAsync(NpgsqlConnection connection, string sql) =>
        (string)(await new NpgsqlCommand(sql, connection).ExecuteScalarAsync())!;

    private static async Task<long> CountAsync(NpgsqlConnection connection, string sql) =>
        (long)(await new NpgsqlCommand(sql, connection).ExecuteScalarAsync())!;

    private static Task<string> BoundaryStateSnapshotAsync(NpgsqlConnection connection) => ScalarAsync(connection, """
        SELECT
            (SELECT COALESCE(string_agg(id::text || ':' || COALESCE(team_id::text, 'NULL'), ',' ORDER BY id), '') FROM contents)
            || '|'
            || (SELECT COALESCE(string_agg(id::text || ':' || permission_revision::text, ',' ORDER BY id), '') FROM workspaces)
            || '|'
            || (SELECT COUNT(*)::text FROM "__EFMigrationsHistory");
        """);

    private const string PreMigrationSchemaSql = """
        CREATE TABLE "__EFMigrationsHistory" (
            "MigrationId" character varying(150) NOT NULL,
            "ProductVersion" character varying(32) NOT NULL,
            CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
        );
        CREATE TABLE workspaces (
            id uuid PRIMARY KEY,
            permission_revision bigint NOT NULL DEFAULT 0
        );
        CREATE TABLE teams (
            id uuid PRIMARY KEY,
            workspace_id uuid NOT NULL,
            is_deleted boolean NOT NULL DEFAULT FALSE,
            CONSTRAINT uq_teams_id_workspace UNIQUE (id, workspace_id)
        );
        CREATE TABLE team_brands (
            id uuid PRIMARY KEY,
            team_id uuid NOT NULL,
            brand_id uuid NOT NULL,
            is_active boolean NOT NULL DEFAULT TRUE
        );
        CREATE TABLE contents (
            id uuid PRIMARY KEY,
            workspace_id uuid NOT NULL,
            brand_id uuid NOT NULL
        );
        """;

    private const string PreMigrationSeedSql = $$"""
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('{{PreviousMigration}}', '9.0.9');
        INSERT INTO workspaces (id) VALUES ('{{WorkspaceA}}'::uuid), ('{{WorkspaceB}}'::uuid);
        INSERT INTO teams (id, workspace_id) VALUES
            ('{{TeamA}}'::uuid, '{{WorkspaceA}}'::uuid),
            ('{{TeamB}}'::uuid, '{{WorkspaceA}}'::uuid),
            ('{{ForeignTeam}}'::uuid, '{{WorkspaceB}}'::uuid);
        INSERT INTO team_brands (id, team_id, brand_id) VALUES
            ('50000000-0000-0000-0000-000000000001'::uuid, '{{TeamA}}'::uuid, '{{UniqueBrand}}'::uuid),
            ('50000000-0000-0000-0000-000000000002'::uuid, '{{TeamA}}'::uuid, '{{SharedBrand}}'::uuid),
            ('50000000-0000-0000-0000-000000000003'::uuid, '{{TeamB}}'::uuid, '{{SharedBrand}}'::uuid);
        INSERT INTO contents (id, workspace_id, brand_id) VALUES
            ('{{UniqueLegacyContent}}'::uuid, '{{WorkspaceA}}'::uuid, '{{UniqueBrand}}'::uuid),
            ('{{AmbiguousLegacyContent}}'::uuid, '{{WorkspaceA}}'::uuid, '{{SharedBrand}}'::uuid);
        """;
}
