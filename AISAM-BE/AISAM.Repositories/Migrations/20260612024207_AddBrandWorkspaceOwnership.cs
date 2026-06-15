using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandWorkspaceOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "brands",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_brands_workspace_id",
                table: "brands",
                column: "workspace_id");

            migrationBuilder.AddForeignKey(
                name: "FK_brands_workspaces_workspace_id",
                table: "brands",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_brands_workspaces_workspace_id",
                table: "brands");

            migrationBuilder.DropIndex(
                name: "IX_brands_workspace_id",
                table: "brands");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "brands");
        }
    }
}
