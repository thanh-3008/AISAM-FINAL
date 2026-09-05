using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class PermissionAccessControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing profile-owned Teams do not have a proven Workspace mapping.
            // Stop before changing schema/data rather than fabricating Guid.Empty ownership.
            // Legacy migration remains blocked pending a reviewed mapping and PostgreSQL evidence.
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM teams) THEN
                        RAISE EXCEPTION 'PermissionAccessControl requires a reviewed legacy Team-to-Workspace mapping; no changes applied.';
                    END IF;
                END $$;
                """);
            migrationBuilder.AlterColumn<Guid>(
                name: "profile_id",
                table: "teams",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "teams",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "channel_access_mode",
                table: "team_brands",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "balance_after",
                table: "credit_usage_records",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "balance_before",
                table: "credit_usage_records",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "brand_id",
                table: "credit_usage_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "integration_id",
                table: "credit_usage_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "reference_id",
                table: "credit_usage_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "team_id",
                table: "credit_usage_records",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "primary_creator_id",
                table: "contents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "audit_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "collaboration_tasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    integration_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assignee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    blocked_reason = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_collaboration_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_collaboration_tasks_contents_content_id",
                        column: x => x.content_id,
                        principalTable: "contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_collaboration_tasks_teams_team_id",
                        column: x => x.team_id,
                        principalTable: "teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "content_participations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recorded_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_participations", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_participations_contents_content_id",
                        column: x => x.content_id,
                        principalTable: "contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "team_channel_access",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_brand_id = table.Column<Guid>(type: "uuid", nullable: false),
                    integration_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_team_channel_access", x => x.id);
                    table.ForeignKey(
                        name: "FK_team_channel_access_social_integrations_integration_id",
                        column: x => x.integration_id,
                        principalTable: "social_integrations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_team_channel_access_team_brands_team_brand_id",
                        column: x => x.team_brand_id,
                        principalTable: "team_brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "temporary_access_grants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_by = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    can_edit = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_temporary_access_grants", x => x.id);
                    table.ForeignKey(
                        name: "FK_temporary_access_grants_collaboration_tasks_task_id",
                        column: x => x.task_id,
                        principalTable: "collaboration_tasks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_teams_workspace_id",
                table: "teams",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_members_team_id_user_id",
                table: "team_members",
                columns: new[] { "team_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_team_brands_team_id_brand_id",
                table: "team_brands",
                columns: new[] { "team_id", "brand_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_credit_usage_records_workspace_id_team_id_created_at",
                table: "credit_usage_records",
                columns: new[] { "workspace_id", "team_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_contents_workspace_id_primary_creator_id",
                table: "contents",
                columns: new[] { "workspace_id", "primary_creator_id" });

            migrationBuilder.CreateIndex(
                name: "IX_collaboration_tasks_content_id",
                table: "collaboration_tasks",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_collaboration_tasks_team_id",
                table: "collaboration_tasks",
                column: "team_id");

            migrationBuilder.CreateIndex(
                name: "IX_collaboration_tasks_workspace_id_assignee_id_status",
                table: "collaboration_tasks",
                columns: new[] { "workspace_id", "assignee_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_content_participations_content_id_user_id",
                table: "content_participations",
                columns: new[] { "content_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_team_channel_access_integration_id",
                table: "team_channel_access",
                column: "integration_id");

            migrationBuilder.CreateIndex(
                name: "IX_team_channel_access_team_brand_id_integration_id",
                table: "team_channel_access",
                columns: new[] { "team_brand_id", "integration_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_temporary_access_grants_task_id",
                table: "temporary_access_grants",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_temporary_access_grants_user_id_expires_at",
                table: "temporary_access_grants",
                columns: new[] { "user_id", "expires_at" });

            migrationBuilder.AddForeignKey(
                name: "FK_teams_workspaces_workspace_id",
                table: "teams",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
            // Approved rule: a Workspace with no legacy Teams receives one Team named
            // after the Workspace. This creates new identities, not legacy ownership mappings.
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM workspace_members GROUP BY workspace_id, user_id HAVING COUNT(*) > 1) THEN
                        RAISE EXCEPTION 'Duplicate workspace membership requires review before default Team creation.';
                    END IF;
                END $$;
                INSERT INTO teams (id, workspace_id, profile_id, name, is_deleted, status, created_at)
                SELECT gen_random_uuid(), w.id, NULL, w.name, false, 0, CURRENT_TIMESTAMP
                FROM workspaces w WHERE NOT EXISTS (SELECT 1 FROM teams t WHERE t.workspace_id = w.id);
                INSERT INTO team_members (id, team_id, user_id, role, permissions, joined_at, is_active)
                SELECT gen_random_uuid(), t.id, m.user_id,
                    CASE m.role WHEN 1 THEN 'Owner' WHEN 2 THEN 'Manager' WHEN 3 THEN 'ContentCreator' ELSE 'Viewer' END,
                    '[]'::jsonb, CURRENT_TIMESTAMP, m.is_active
                FROM teams t JOIN workspace_members m ON m.workspace_id = t.workspace_id
                WHERE NOT EXISTS (SELECT 1 FROM team_members tm WHERE tm.team_id = t.id AND tm.user_id = m.user_id);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                    IF EXISTS (SELECT 1 FROM teams WHERE profile_id IS NULL) THEN
                        RAISE EXCEPTION 'Rollback requires a reviewed Profile mapping for workspace Teams; no changes applied.';
                    END IF;
                END $$;
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_teams_workspaces_workspace_id",
                table: "teams");

            migrationBuilder.DropTable(
                name: "content_participations");

            migrationBuilder.DropTable(
                name: "team_channel_access");

            migrationBuilder.DropTable(
                name: "temporary_access_grants");

            migrationBuilder.DropTable(
                name: "collaboration_tasks");

            migrationBuilder.DropIndex(
                name: "IX_teams_workspace_id",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_team_members_team_id_user_id",
                table: "team_members");

            migrationBuilder.DropIndex(
                name: "IX_team_brands_team_id_brand_id",
                table: "team_brands");

            migrationBuilder.DropIndex(
                name: "IX_credit_usage_records_workspace_id_team_id_created_at",
                table: "credit_usage_records");

            migrationBuilder.DropIndex(
                name: "IX_contents_workspace_id_primary_creator_id",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "teams");

            migrationBuilder.DropColumn(
                name: "channel_access_mode",
                table: "team_brands");

            migrationBuilder.DropColumn(
                name: "balance_after",
                table: "credit_usage_records");

            migrationBuilder.DropColumn(
                name: "balance_before",
                table: "credit_usage_records");

            migrationBuilder.DropColumn(
                name: "brand_id",
                table: "credit_usage_records");

            migrationBuilder.DropColumn(
                name: "integration_id",
                table: "credit_usage_records");

            migrationBuilder.DropColumn(
                name: "reference_id",
                table: "credit_usage_records");

            migrationBuilder.DropColumn(
                name: "team_id",
                table: "credit_usage_records");

            migrationBuilder.DropColumn(
                name: "primary_creator_id",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "audit_logs");

            migrationBuilder.AlterColumn<Guid>(
                name: "profile_id",
                table: "teams",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
