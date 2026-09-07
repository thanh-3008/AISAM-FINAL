using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    [DbContext(typeof(AisamContext))]
    [Migration("20260907080000_FixUserIsActiveDefaultAndBackfill")]
    public partial class FixUserIsActiveDefaultAndBackfill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Do not call AlterColumn because EF Core emits "ALTER COLUMN is_active TYPE boolean"
            // which PostgreSQL rejects when is_active is referenced by permission_revision_guard trigger.
            // Directly alter column default value instead.
            migrationBuilder.Sql("ALTER TABLE users ALTER COLUMN is_active SET DEFAULT TRUE;");

            // Backfill: Reactivate legacy/unintended suspended users that were affected by defaultValue: false
            // but were never explicitly suspended by an administrator.
            migrationBuilder.Sql("""
                UPDATE users
                SET is_active = TRUE
                WHERE is_active = FALSE
                  AND suspended_by IS NULL
                  AND suspended_at IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE users ALTER COLUMN is_active SET DEFAULT FALSE;");
        }
    }
}
