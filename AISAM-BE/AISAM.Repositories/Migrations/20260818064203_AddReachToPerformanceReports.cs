using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddReachToPerformanceReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "reach",
                table: "performance_reports",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reach",
                table: "performance_reports");
        }
    }
}
