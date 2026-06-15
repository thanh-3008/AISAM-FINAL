using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workspaces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    workspace_type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    subscription_expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workspaces", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workspace_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    quota_mode = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    credit_limit = table.Column<long>(type: "bigint", nullable: true),
                    credit_used = table.Column<long>(type: "bigint", nullable: false),
                    credit_period_start = table.Column<DateTime>(type: "date", nullable: true),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workspace_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_workspace_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workspace_members_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_workspace_members_is_active",
                table: "workspace_members",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_members_user_id",
                table: "workspace_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_members_workspace_id",
                table: "workspace_members",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_workspace_members_workspace_id_role",
                table: "workspace_members",
                columns: new[] { "workspace_id", "role" });

            migrationBuilder.CreateIndex(
                name: "IX_workspace_members_workspace_id_user_id",
                table: "workspace_members",
                columns: new[] { "workspace_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_archived_at",
                table: "workspaces",
                column: "archived_at");

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_deleted_at",
                table: "workspaces",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_status",
                table: "workspaces",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_subscription_expired_at",
                table: "workspaces",
                column: "subscription_expired_at");

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_workspace_type",
                table: "workspaces",
                column: "workspace_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workspace_members");

            migrationBuilder.DropTable(
                name: "workspaces");
        }
    }
}
