using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ExecutionAttributionAndIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_temporary_access_grants_task_id",
                table: "temporary_access_grants");

            migrationBuilder.DropIndex(
                name: "IX_collaboration_tasks_team_id",
                table: "collaboration_tasks");

            migrationBuilder.AddColumn<Guid>(
                name: "affected_user_id",
                table: "audit_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "approved_by",
                table: "audit_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "executed_by_system",
                table: "audit_logs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "reference_id",
                table: "audit_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "requested_by",
                table: "audit_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "team_id",
                table: "audit_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_teams_id_workspace_id",
                table: "teams",
                columns: new[] { "id", "workspace_id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_collaboration_tasks_id_workspace_id",
                table: "collaboration_tasks",
                columns: new[] { "id", "workspace_id" });

            migrationBuilder.CreateTable(
                name: "execution_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    team_id = table.Column<Guid>(type: "uuid", nullable: true),
                    resource_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    brand_id = table.Column<Guid>(type: "uuid", nullable: true),
                    integration_id = table.Column<Guid>(type: "uuid", nullable: true),
                    requested_action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    approval_authority = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    execution_policy = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    policy_version = table.Column<int>(type: "integer", nullable: false),
                    enqueue_authorized_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_execution_operations", x => x.id);
                    table.ForeignKey(
                        name: "FK_execution_operations_teams_team_id_workspace_id",
                        columns: x => new { x.team_id, x.workspace_id },
                        principalTable: "teams",
                        principalColumns: new[] { "id", "workspace_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_execution_operations_users_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_execution_operations_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_temporary_access_grants_task_id_workspace_id",
                table: "temporary_access_grants",
                columns: new[] { "task_id", "workspace_id" });

            migrationBuilder.CreateIndex(
                name: "IX_contents_id_workspace_id",
                table: "contents",
                columns: new[] { "id", "workspace_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_collaboration_tasks_team_id_workspace_id",
                table: "collaboration_tasks",
                columns: new[] { "team_id", "workspace_id" });

            migrationBuilder.CreateIndex(
                name: "IX_execution_operations_actor_user_id",
                table: "execution_operations",
                column: "actor_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_execution_operations_resource_type_reference_id_requested_a~",
                table: "execution_operations",
                columns: new[] { "resource_type", "reference_id", "requested_action" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_execution_operations_team_id_workspace_id",
                table: "execution_operations",
                columns: new[] { "team_id", "workspace_id" });

            migrationBuilder.CreateIndex(
                name: "IX_execution_operations_workspace_id_team_id_created_at",
                table: "execution_operations",
                columns: new[] { "workspace_id", "team_id", "created_at" });

            migrationBuilder.AddForeignKey(
                name: "FK_collaboration_tasks_teams_team_id_workspace_id",
                table: "collaboration_tasks",
                columns: new[] { "team_id", "workspace_id" },
                principalTable: "teams",
                principalColumns: new[] { "id", "workspace_id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_temporary_access_grants_collaboration_tasks_task_id_workspa~",
                table: "temporary_access_grants",
                columns: new[] { "task_id", "workspace_id" },
                principalTable: "collaboration_tasks",
                principalColumns: new[] { "id", "workspace_id" },
                onDelete: ReferentialAction.Restrict);
            // PostgreSQL can reference a unique index without making WorkspaceId an EF
            // identifying key (legacy unassigned content must remain readable).
            migrationBuilder.Sql("""
                ALTER TABLE collaboration_tasks ADD CONSTRAINT fk_collaboration_content_workspace
                  FOREIGN KEY (content_id, workspace_id) REFERENCES contents (id, workspace_id) ON DELETE RESTRICT;
                ALTER TABLE content_participations ADD CONSTRAINT fk_participation_content_workspace
                  FOREIGN KEY (content_id, workspace_id) REFERENCES contents (id, workspace_id) ON DELETE RESTRICT;
                ALTER TABLE temporary_access_grants ADD CONSTRAINT ck_temporary_grant_dates
                  CHECK (expires_at > granted_at);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Never silently discard event attribution when rolling the application back.
            migrationBuilder.Sql("""
                DO $$ BEGIN
                  IF EXISTS (SELECT 1 FROM execution_operations) OR EXISTS (
                    SELECT 1 FROM audit_logs WHERE requested_by IS NOT NULL OR executed_by_system) THEN
                    RAISE EXCEPTION 'Rollback requires a reviewed attribution archive; no changes applied.';
                  END IF;
                END $$;
                ALTER TABLE collaboration_tasks DROP CONSTRAINT fk_collaboration_content_workspace;
                ALTER TABLE content_participations DROP CONSTRAINT fk_participation_content_workspace;
                ALTER TABLE temporary_access_grants DROP CONSTRAINT ck_temporary_grant_dates;
                """);
            migrationBuilder.DropForeignKey(
                name: "FK_collaboration_tasks_teams_team_id_workspace_id",
                table: "collaboration_tasks");

            migrationBuilder.DropForeignKey(
                name: "FK_temporary_access_grants_collaboration_tasks_task_id_workspa~",
                table: "temporary_access_grants");

            migrationBuilder.DropTable(
                name: "execution_operations");

            migrationBuilder.DropIndex(
                name: "IX_temporary_access_grants_task_id_workspace_id",
                table: "temporary_access_grants");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_teams_id_workspace_id",
                table: "teams");

            migrationBuilder.DropIndex(
                name: "IX_contents_id_workspace_id",
                table: "contents");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_collaboration_tasks_id_workspace_id",
                table: "collaboration_tasks");

            migrationBuilder.DropIndex(
                name: "IX_collaboration_tasks_team_id_workspace_id",
                table: "collaboration_tasks");

            migrationBuilder.DropColumn(
                name: "affected_user_id",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "approved_by",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "executed_by_system",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "reference_id",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "requested_by",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "team_id",
                table: "audit_logs");

            migrationBuilder.CreateIndex(
                name: "IX_temporary_access_grants_task_id",
                table: "temporary_access_grants",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "IX_collaboration_tasks_team_id",
                table: "collaboration_tasks",
                column: "team_id");
        }
    }
}
