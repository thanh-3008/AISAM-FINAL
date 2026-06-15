using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleActiveWorkspaceOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workspace_members_workspace_id",
                table: "workspace_members");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_members_workspace_id",
                table: "workspace_members",
                column: "workspace_id",
                unique: true,
                filter: "\"role\" = 1 AND \"is_active\" = TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workspace_members_workspace_id",
                table: "workspace_members");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_members_workspace_id",
                table: "workspace_members",
                column: "workspace_id");
        }
    }
}
