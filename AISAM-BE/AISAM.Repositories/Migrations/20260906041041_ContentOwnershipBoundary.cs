using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ContentOwnershipBoundary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "team_id",
                table: "contents",
                type: "uuid",
                nullable: true);

            // Reconcile only deterministic legacy ownership. Shared-brand and
            // otherwise ambiguous rows deliberately remain NULL (fail closed).
            migrationBuilder.Sql("""
                WITH candidates AS (
                    SELECT c.id AS content_id, (ARRAY_AGG(DISTINCT tb.team_id))[1] AS team_id
                    FROM contents c
                    JOIN team_brands tb ON tb.brand_id = c.brand_id AND tb.is_active = TRUE
                    JOIN teams t ON t.id = tb.team_id AND t.workspace_id = c.workspace_id
                        AND t.is_deleted = FALSE
                    WHERE c.team_id IS NULL
                    GROUP BY c.id
                    HAVING COUNT(DISTINCT tb.team_id) = 1
                ), reconciled AS (
                    UPDATE contents c SET team_id = candidates.team_id
                    FROM candidates WHERE c.id = candidates.content_id
                    RETURNING c.workspace_id
                )
                UPDATE workspaces w SET permission_revision = permission_revision + 1
                WHERE w.id IN (SELECT DISTINCT workspace_id FROM reconciled);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_contents_team_id_workspace_id",
                table: "contents",
                columns: new[] { "team_id", "workspace_id" });

            migrationBuilder.AddForeignKey(
                name: "FK_contents_teams_team_id_workspace_id",
                table: "contents",
                columns: new[] { "team_id", "workspace_id" },
                principalTable: "teams",
                principalColumns: new[] { "id", "workspace_id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("""
                CREATE FUNCTION aisam_content_team_boundary() RETURNS trigger LANGUAGE plpgsql AS $guard$
                BEGIN
                  IF NEW.team_id IS NULL THEN
                    IF TG_OP = 'INSERT' THEN RAISE EXCEPTION 'New content requires an owning team.'; END IF;
                    RETURN NEW;
                  END IF;
                  IF NOT EXISTS (
                    SELECT 1 FROM teams t
                    JOIN team_brands tb ON tb.team_id = t.id AND tb.brand_id = NEW.brand_id AND tb.is_active = TRUE
                    WHERE t.id = NEW.team_id AND t.workspace_id = NEW.workspace_id AND t.is_deleted = FALSE
                  ) THEN
                    RAISE EXCEPTION 'Content owning team must be active in the workspace and have access to the brand.';
                  END IF;
                  RETURN NEW;
                END $guard$;
                CREATE FUNCTION aisam_content_team_immutable() RETURNS trigger LANGUAGE plpgsql AS $guard$
                BEGIN
                  IF OLD.team_id IS NOT NULL AND NEW.team_id IS DISTINCT FROM OLD.team_id THEN
                    RAISE EXCEPTION 'Content team ownership is immutable.';
                  END IF;
                  IF NEW.team_id IS DISTINCT FROM OLD.team_id THEN
                    UPDATE workspaces SET permission_revision = permission_revision + 1 WHERE id = NEW.workspace_id;
                  END IF;
                  RETURN NEW;
                END $guard$;
                CREATE TRIGGER content_team_boundary BEFORE INSERT OR UPDATE OF team_id, brand_id, workspace_id ON contents
                FOR EACH ROW EXECUTE FUNCTION aisam_content_team_boundary();
                CREATE TRIGGER content_team_immutable BEFORE UPDATE OF team_id ON contents
                FOR EACH ROW EXECUTE FUNCTION aisam_content_team_immutable();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER IF EXISTS content_team_boundary ON contents;
                DROP TRIGGER IF EXISTS content_team_immutable ON contents;
                DROP FUNCTION IF EXISTS aisam_content_team_boundary();
                DROP FUNCTION IF EXISTS aisam_content_team_immutable();
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_contents_teams_team_id_workspace_id",
                table: "contents");

            migrationBuilder.DropIndex(
                name: "IX_contents_team_id_workspace_id",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "team_id",
                table: "contents");
        }
    }
}
