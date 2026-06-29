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
            migrationBuilder.AddColumn<string>(
                name: "tags",
                table: "contents",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "deployment_status",
                table: "ad_campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "deployment_step",
                table: "ad_campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tags",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "deployment_status",
                table: "ad_campaigns");

            migrationBuilder.DropColumn(
                name: "deployment_step",
                table: "ad_campaigns");
        }
    }
}
