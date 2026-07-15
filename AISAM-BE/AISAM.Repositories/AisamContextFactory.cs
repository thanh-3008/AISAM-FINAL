using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AISAM.Repositories;

public class AisamContextFactory : IDesignTimeDbContextFactory<AisamContext>
{
    public AisamContext CreateDbContext(string[] args)
    {
        var apiDirectory = FindApiDirectory();
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? LoadConnectionStringFromEnvFile(Path.Combine(apiDirectory, ".env"))
            ?? LoadConnectionStringFromAppSettings(apiDirectory);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Missing PostgreSQL connection string. Add CONNECTION_STRING to AISAM.API/.env or ConnectionStrings:DefaultConnection to AISAM.API/appsettings.Development.json.");
        }

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.EnableDynamicJson();
        var dataSource = dataSourceBuilder.Build();

        var optionsBuilder = new DbContextOptionsBuilder<AisamContext>();
        optionsBuilder.UseNpgsql(dataSource, npgsqlOptions =>
            npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));

        return new AisamContext(optionsBuilder.Options);
    }

    private static string FindApiDirectory()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current is not null)
        {
            var apiDirectory = Path.Combine(current.FullName, "AISAM.API");
            if (Directory.Exists(apiDirectory))
            {
                return apiDirectory;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not find AISAM.API directory from current working directory.");
    }

    private static string? LoadConnectionStringFromEnvFile(string envPath)
    {
        if (!File.Exists(envPath))
        {
            return null;
        }

        foreach (var line in File.ReadLines(envPath))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            var separatorIndex = trimmed.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = trimmed[..separatorIndex].Trim();
            if (!string.Equals(key, "CONNECTION_STRING", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return trimmed[(separatorIndex + 1)..].Trim().Trim('"');
        }

        return null;
    }

    private static string? LoadConnectionStringFromAppSettings(string apiDirectory)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDirectory)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        return configuration.GetConnectionString("DefaultConnection");
    }
}
