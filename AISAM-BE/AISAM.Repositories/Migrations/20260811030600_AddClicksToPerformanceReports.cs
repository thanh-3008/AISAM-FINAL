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
            // Step 1: Add column with default 0
            migrationBuilder.AddColumn<long>(
                name: "clicks",
                table: "performance_reports",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Step 2: Backfill existing rows from RawData.
            // Logic mirrors ExtractClicks(): GREATEST(meta clicks from FB API, trackedClicks internal).
            // - (raw_data::jsonb ->> 'clicks')::bigint  → Facebook API click count
            // - (raw_data::jsonb ->> 'trackedClicks')::bigint → Internal tracked clicks
            // - COALESCE(..., 0) handles NULL (field missing in JSON or parse fails)
            // - GREATEST(...) matches the C# Math.Max(metaClicks, trackedClicks) logic
            // Rows with NULL or empty raw_data keep clicks = 0 (defaultValue above).
            migrationBuilder.Sql(@"
                UPDATE performance_reports
                SET clicks = GREATEST(
                    COALESCE((raw_data::jsonb ->> 'clicks')::bigint, 0),
                    COALESCE((raw_data::jsonb ->> 'trackedClicks')::bigint, 0)
                )
                WHERE raw_data IS NOT NULL AND raw_data <> '' AND raw_data <> '{}';
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
