using AISAM.Repositories;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AISAM.Repositories.Migrations;

[DbContext(typeof(AisamContext))]
[Migration("20260910100000_RepairLegacyUserActivation")]
public sealed class RepairLegacyUserActivation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The original suspension migration defaulted every existing user to false.
        // Never reactivate a user with evidence of an explicit suspension.
        migrationBuilder.Sql("""
            ALTER TABLE users ALTER COLUMN is_active SET DEFAULT true;
            UPDATE users SET is_active = true
            WHERE is_active = false AND suspended_at IS NULL
              AND suspended_by IS NULL AND suspension_reason IS NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("Activation repair cannot infer previous intentional account states. Restore a verified backup instead.");
}
