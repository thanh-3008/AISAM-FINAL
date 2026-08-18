using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddClicksToPerformanceReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "clicks",
                table: "performance_reports",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.Sql(@"
                UPDATE performance_reports
                SET clicks = GREATEST(
                    COALESCE((raw_data::jsonb ->> 'clicks')::bigint, 0),
                    COALESCE((raw_data::jsonb ->> 'trackedClicks')::bigint, 0)
                )
                WHERE raw_data IS NOT NULL AND raw_data::text <> '' AND raw_data::text <> '{}';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "clicks",
                table: "performance_reports");
        }
    }
}
