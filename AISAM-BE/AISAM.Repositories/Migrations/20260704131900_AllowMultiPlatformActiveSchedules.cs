using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultiPlatformActiveSchedules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_content_calendar_content_id",
                table: "content_calendar");

            migrationBuilder.CreateIndex(
                name: "IX_content_calendar_content_id_integration_id",
                table: "content_calendar",
                columns: new[] { "content_id", "integration_id" },
                unique: true,
                filter: "\"status\" IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_content_calendar_content_id_integration_id",
                table: "content_calendar");

            migrationBuilder.CreateIndex(
                name: "IX_content_calendar_content_id",
                table: "content_calendar",
                column: "content_id",
                unique: true,
                filter: "\"status\" IN (0, 1)");
        }
    }
}
