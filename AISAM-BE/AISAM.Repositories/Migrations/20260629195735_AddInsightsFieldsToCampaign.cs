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
            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                ADD COLUMN IF NOT EXISTS clicks bigint NOT NULL DEFAULT 0;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                ADD COLUMN IF NOT EXISTS conversions bigint NOT NULL DEFAULT 0;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                ADD COLUMN IF NOT EXISTS impressions bigint NOT NULL DEFAULT 0;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                ADD COLUMN IF NOT EXISTS spend numeric(12,2) NOT NULL DEFAULT 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                DROP COLUMN IF EXISTS clicks;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                DROP COLUMN IF EXISTS conversions;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                DROP COLUMN IF EXISTS impressions;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                DROP COLUMN IF EXISTS spend;
                """);
        }
    }
}
