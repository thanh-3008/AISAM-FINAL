using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddDeploymentStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE contents
                ADD COLUMN IF NOT EXISTS tags jsonb;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                ADD COLUMN IF NOT EXISTS deployment_status integer NOT NULL DEFAULT 0;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                ADD COLUMN IF NOT EXISTS deployment_step integer NOT NULL DEFAULT 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE contents
                DROP COLUMN IF EXISTS tags;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                DROP COLUMN IF EXISTS deployment_status;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                DROP COLUMN IF EXISTS deployment_step;
                """);
        }
    }
}
