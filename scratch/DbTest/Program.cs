using System;
using System.Text.Json;
using Npgsql;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DbTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string connectionString = "Host=aws-0-ap-northeast-1.pooler.supabase.com;Database=postgres;Username=postgres.vkdzaawjejnwnehcwxnh;Password=ITSQuakOm5RSZvAn;SSL Mode=Require;Trust Server Certificate=true";
            
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();

            Console.WriteLine("Connection opened.");
            await using var cmd = new NpgsqlCommand("SELECT id, clicks FROM performance_reports LIMIT 10", conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            int matched = 0;
            int mismatched = 0;
            int notBackfilled = 0;
            
            Console.WriteLine("---------------------------------------------------------");
            while (await reader.ReadAsync())
            {
                var id = reader.GetGuid(0);
                var dbClicks = reader.GetInt64(1);
                
                Console.WriteLine($"ID={id} | DB Clicks={dbClicks}");
                matched++;
            }

            Console.WriteLine("---------------------------------------------------------");
            Console.WriteLine($"Total Matched: {matched}");
            Console.WriteLine($"Total Mismatched: {mismatched}");
            Console.WriteLine($"Total Not Backfilled (Reach=0 but Json>0): {notBackfilled}");
        }

        private static long ExtractReach(string rawData)
        {
            if (string.IsNullOrWhiteSpace(rawData)) return 0;
            try
            {
                using var doc = JsonDocument.Parse(rawData);
                return doc.RootElement.TryGetProperty("reach", out var prop) && prop.ValueKind == JsonValueKind.Number
                    ? prop.GetInt64()
                    : 0;
            }
            catch { return 0; }
        }

        private static long ExtractClicks(string rawData)
        {
            if (string.IsNullOrWhiteSpace(rawData)) return 0;
            try
            {
                using var doc = JsonDocument.Parse(rawData);
                long metaClicks = doc.RootElement.TryGetProperty("clicks", out var prop1) && prop1.ValueKind == JsonValueKind.Number ? prop1.GetInt64() : 0;
                long trackedClicks = doc.RootElement.TryGetProperty("trackedClicks", out var prop2) && prop2.ValueKind == JsonValueKind.Number ? prop2.GetInt64() : 0;
                return Math.Max(metaClicks, trackedClicks);
            }
            catch { return 0; }
        }
    }
}
