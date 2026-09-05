using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class MutationPermissionRevision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "permission_revision",
                table: "workspaces",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "execution_version",
                table: "execution_operations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION aisam_permission_revision_guard() RETURNS trigger LANGUAGE plpgsql AS $guard$
                DECLARE old_row jsonb; new_row jsonb; row_data jsonb; workspace_ids uuid[] := ARRAY[]::uuid[]; workspace uuid;
                BEGIN
                  IF TG_OP <> 'INSERT' THEN old_row := to_jsonb(OLD); END IF;
                  IF TG_OP <> 'DELETE' THEN new_row := to_jsonb(NEW); END IF;
                  -- Metadata updates do not invalidate permission stamps unnecessarily.
                  IF TG_TABLE_NAME = 'workspaces' AND old_row->'status' IS NOT DISTINCT FROM new_row->'status'
                     AND old_row->'subscription_expired_at' IS NOT DISTINCT FROM new_row->'subscription_expired_at' THEN RETURN NEW; END IF;
                  IF TG_TABLE_NAME = 'users' AND old_row->'is_active' IS NOT DISTINCT FROM new_row->'is_active'
                     AND old_row->'role' IS NOT DISTINCT FROM new_row->'role' THEN RETURN NEW; END IF;
                  IF TG_TABLE_NAME = 'contents' AND old_row->'brand_id' IS NOT DISTINCT FROM new_row->'brand_id'
                     AND old_row->'workspace_id' IS NOT DISTINCT FROM new_row->'workspace_id'
                     AND old_row->'primary_creator_id' IS NOT DISTINCT FROM new_row->'primary_creator_id'
                     AND old_row->'is_deleted' IS NOT DISTINCT FROM new_row->'is_deleted' THEN RETURN NEW; END IF;
                  FOREACH row_data IN ARRAY ARRAY[old_row, new_row] LOOP
                    IF row_data IS NULL THEN CONTINUE; END IF;
                    workspace := NULL;
                    IF TG_TABLE_NAME = 'workspaces' THEN workspace := (row_data->>'id')::uuid;
                    ELSIF TG_TABLE_NAME IN ('team_members', 'team_brands') THEN
                      SELECT workspace_id INTO workspace FROM teams WHERE id = (row_data->>'team_id')::uuid;
                    ELSIF TG_TABLE_NAME = 'team_channel_access' THEN
                      SELECT t.workspace_id INTO workspace FROM team_brands b JOIN teams t ON t.id = b.team_id WHERE b.id = (row_data->>'team_brand_id')::uuid;
                    ELSIF TG_TABLE_NAME = 'users' THEN
                      workspace_ids := workspace_ids || ARRAY(SELECT workspace_id FROM workspace_members WHERE user_id = (row_data->>'id')::uuid);
                    ELSE workspace := (row_data->>'workspace_id')::uuid;
                    END IF;
                    IF workspace IS NOT NULL THEN workspace_ids := array_append(workspace_ids, workspace); END IF;
                  END LOOP;
                  FOR workspace IN SELECT DISTINCT unnest(workspace_ids) ORDER BY 1 LOOP
                    UPDATE workspaces SET permission_revision = permission_revision + 1 WHERE id = workspace;
                  END LOOP;
                  IF TG_OP = 'DELETE' THEN RETURN OLD; ELSE RETURN NEW; END IF;
                END $guard$;
                
                CREATE OR REPLACE FUNCTION aisam_execution_attribution_immutable() RETURNS trigger LANGUAGE plpgsql AS $guard$
                BEGIN
                  RAISE EXCEPTION 'Execution attribution is immutable; enqueue a new operation version.';
                END $guard$;
                CREATE TRIGGER execution_attribution_immutable BEFORE UPDATE OR DELETE ON execution_operations
                FOR EACH ROW EXECUTE FUNCTION aisam_execution_attribution_immutable();
                CREATE TRIGGER permission_revision_guard BEFORE INSERT OR UPDATE OR DELETE ON workspace_members FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                CREATE TRIGGER permission_revision_guard BEFORE INSERT OR UPDATE OR DELETE ON teams FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                CREATE TRIGGER permission_revision_guard BEFORE INSERT OR UPDATE OR DELETE ON team_members FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                CREATE TRIGGER permission_revision_guard BEFORE INSERT OR UPDATE OR DELETE ON team_brands FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                CREATE TRIGGER permission_revision_guard BEFORE INSERT OR UPDATE OR DELETE ON team_channel_access FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                CREATE TRIGGER permission_revision_guard BEFORE INSERT OR UPDATE OR DELETE ON brands FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                CREATE TRIGGER permission_revision_guard BEFORE INSERT OR UPDATE OR DELETE ON social_integrations FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                CREATE TRIGGER permission_revision_guard BEFORE INSERT OR UPDATE OR DELETE ON collaboration_tasks FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                CREATE TRIGGER permission_revision_guard BEFORE INSERT OR UPDATE OR DELETE ON temporary_access_grants FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                CREATE TRIGGER permission_revision_guard BEFORE INSERT OR UPDATE OR DELETE ON content_participations FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                CREATE TRIGGER permission_revision_guard AFTER UPDATE OF status, subscription_expired_at ON workspaces FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                CREATE TRIGGER permission_revision_guard BEFORE UPDATE OF role, is_active ON users FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                CREATE TRIGGER permission_revision_guard BEFORE UPDATE OF workspace_id, brand_id, primary_creator_id, is_deleted ON contents FOR EACH ROW EXECUTE FUNCTION aisam_permission_revision_guard();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER execution_attribution_immutable ON execution_operations;
                DROP FUNCTION aisam_execution_attribution_immutable();
                DROP TRIGGER permission_revision_guard ON workspace_members;
                DROP TRIGGER permission_revision_guard ON teams;
                DROP TRIGGER permission_revision_guard ON team_members;
                DROP TRIGGER permission_revision_guard ON team_brands;
                DROP TRIGGER permission_revision_guard ON team_channel_access;
                DROP TRIGGER permission_revision_guard ON brands;
                DROP TRIGGER permission_revision_guard ON social_integrations;
                DROP TRIGGER permission_revision_guard ON collaboration_tasks;
                DROP TRIGGER permission_revision_guard ON temporary_access_grants;
                DROP TRIGGER permission_revision_guard ON content_participations;
                DROP TRIGGER permission_revision_guard ON workspaces;
                DROP TRIGGER permission_revision_guard ON users;
                DROP TRIGGER permission_revision_guard ON contents;
                DROP FUNCTION aisam_permission_revision_guard();
                """);

            migrationBuilder.DropColumn(
                name: "permission_revision",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "execution_version",
                table: "execution_operations");
        }
    }
}
