using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    [DbContext(typeof(AisamContext))]
    [Migration("20260805090000_EnsureMultiAccountScheduleIndex")]
    public partial class EnsureMultiAccountScheduleIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_content_calendar_content_id";

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_content_calendar_content_id_integration_id"
                ON content_calendar (content_id, integration_id)
                WHERE "status" IN (0, 1);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_content_calendar_content_id_integration_id";

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_content_calendar_content_id"
                ON content_calendar (content_id)
                WHERE "status" IN (0, 1);
                """);
        }
    }
}
