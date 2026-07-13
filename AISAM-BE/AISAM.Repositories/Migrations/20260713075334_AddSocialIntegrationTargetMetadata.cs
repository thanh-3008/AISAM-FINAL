using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialIntegrationTargetMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "profile_picture_url",
                table: "social_integrations",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "target_category",
                table: "social_integrations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "target_name",
                table: "social_integrations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "target_type",
                table: "social_integrations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_social_integrations_workspace_id_platform_external_id",
                table: "social_integrations",
                columns: new[] { "workspace_id", "platform", "external_id" },
                unique: true,
                filter: "\"is_deleted\" = FALSE AND \"external_id\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_social_integrations_workspace_id_platform_external_id",
                table: "social_integrations");

            migrationBuilder.DropColumn(
                name: "profile_picture_url",
                table: "social_integrations");

            migrationBuilder.DropColumn(
                name: "target_category",
                table: "social_integrations");

            migrationBuilder.DropColumn(
                name: "target_name",
                table: "social_integrations");

            migrationBuilder.DropColumn(
                name: "target_type",
                table: "social_integrations");
        }
    }
}
