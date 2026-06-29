using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddInsightsFieldsToCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "clicks",
                table: "ad_campaigns",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "conversions",
                table: "ad_campaigns",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "impressions",
                table: "ad_campaigns",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<decimal>(
                name: "spend",
                table: "ad_campaigns",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "clicks",
                table: "ad_campaigns");

            migrationBuilder.DropColumn(
                name: "conversions",
                table: "ad_campaigns");

            migrationBuilder.DropColumn(
                name: "impressions",
                table: "ad_campaigns");

            migrationBuilder.DropColumn(
                name: "spend",
                table: "ad_campaigns");
        }
    }
}
