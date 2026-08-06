using System;
using Npgsql;

var connectionString = "Host=aws-1-ap-northeast-1.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.dhzdvcnepphpjuwsyook;Password=5a704b07!10e0;SSL Mode=Disable;Trust Server Certificate=true;Pooling=false";

await using var dataSource = NpgsqlDataSource.Create(connectionString);

await using var cmd = dataSource.CreateCommand("UPDATE users SET is_active = true WHERE email = 'admin@aisam.com'");
var rowsAffected = await cmd.ExecuteNonQueryAsync();

Console.WriteLine($"Updated {rowsAffected} rows. Admin is now active.");
