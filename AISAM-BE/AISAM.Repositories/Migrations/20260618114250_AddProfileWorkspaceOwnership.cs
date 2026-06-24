using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileWorkspaceOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "profiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_profiles_workspace_id",
                table: "profiles",
                column: "workspace_id",
                unique: true,
                filter: "\"workspace_id\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_profiles_workspaces_workspace_id",
                table: "profiles",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_profiles_workspaces_workspace_id",
                table: "profiles");

            migrationBuilder.DropIndex(
                name: "IX_profiles_workspace_id",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "profiles");
        }
    }
}
