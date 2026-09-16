using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AISAM.Repositories.Migrations;

[DbContext(typeof(AisamContext))]
[Migration("20260914110000_PrepareTeamRbacV2")]
public class PrepareTeamRbacV2 : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.AddColumn<int>("workspace_role_v2", "workspace_invitations", nullable: true);
        m.AddColumn<Guid>("default_for_brand_id", "teams", nullable: true);
        m.AddColumn<bool>("scope_enabled_v2", "team_channel_access", nullable: false, defaultValue: false);
        m.CreateIndex("IX_teams_default_for_brand_id", "teams", "default_for_brand_id", unique: true);
        m.AddForeignKey("FK_teams_brands_default_for_brand_id", "teams", "default_for_brand_id", "brands", principalColumn: "id", onDelete: ReferentialAction.Restrict);
        m.Sql("""
            CREATE TABLE rbac_v2_backfill_contents(content_id uuid PRIMARY KEY, team_id uuid NOT NULL);
            CREATE TABLE rbac_v2_created_teams(team_id uuid PRIMARY KEY);
            CREATE TABLE rbac_v2_legacy_snapshot(entity text NOT NULL, entity_id uuid NOT NULL, payload jsonb NOT NULL,
                PRIMARY KEY(entity,entity_id));
            INSERT INTO rbac_v2_legacy_snapshot
                SELECT 'team_member',id,jsonb_build_object('role',role,'permissions',permissions) FROM team_members;
            INSERT INTO rbac_v2_legacy_snapshot
                SELECT 'channel',id,jsonb_build_object('can_view',can_view,'can_publish',can_publish,'can_manage',can_manage)
                FROM team_channel_access;
            UPDATE workspace_invitations SET workspace_role_v2=3 WHERE role IN (1,2,3,4);
            """);
        m.Sql(BackfillSql);
        m.Sql("""
            ALTER TABLE workspace_members ADD CONSTRAINT rbac_v2_member_role CHECK(workspace_role_v2 IS NULL OR workspace_role_v2 IN(1,2,3));
            ALTER TABLE workspace_invitations ADD CONSTRAINT rbac_v2_invitation_role CHECK(workspace_role_v2 IS NULL OR workspace_role_v2 IN(2,3));
            CREATE FUNCTION validate_rbac_v2_channel() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN
              IF NEW.scope_enabled_v2 AND NOT EXISTS(
                SELECT 1 FROM team_brands tb JOIN teams t ON t.id=tb.team_id
                  JOIN brands b ON b.id=tb.brand_id JOIN social_integrations si ON si.id=NEW.integration_id
                WHERE tb.id=NEW.team_brand_id AND tb.is_active AND NOT t.is_deleted AND t.status=0
                  AND b.workspace_id=t.workspace_id AND si.workspace_id=t.workspace_id AND si.brand_id=b.id
                  AND si.is_active AND NOT si.is_deleted AND NOT b.is_deleted)
              THEN RAISE EXCEPTION 'Invalid v2 channel scope' USING ERRCODE='23514'; END IF;
              RETURN NEW;
            END $$;
            CREATE TRIGGER validate_rbac_v2_channel BEFORE INSERT OR UPDATE ON team_channel_access
                FOR EACH ROW EXECUTE FUNCTION validate_rbac_v2_channel();
            """);
        m.Sql("""
            CREATE VIEW rbac_v2_preflight AS
            SELECT 'unknown_workspace_role'::text issue, id entity_id FROM workspace_members WHERE workspace_role_v2 IS NULL
            UNION ALL SELECT 'unknown_invitation_role',id FROM workspace_invitations WHERE workspace_role_v2 IS NULL
            UNION ALL SELECT 'content_without_team',id FROM contents WHERE team_id IS NULL
            UNION ALL SELECT 'invalid_team_member',tm.id FROM team_members tm JOIN teams t ON t.id=tm.team_id
                WHERE tm.role NOT IN (1,2,3) OR NOT EXISTS(SELECT 1 FROM workspace_members wm
                    WHERE wm.workspace_id=t.workspace_id AND wm.user_id=tm.user_id AND wm.is_active)
            UNION ALL SELECT 'cross_workspace_team_brand',tb.id FROM team_brands tb JOIN teams t ON t.id=tb.team_id
                JOIN brands b ON b.id=tb.brand_id WHERE t.workspace_id<>b.workspace_id
            UNION ALL SELECT 'invalid_content_team',c.id FROM contents c JOIN teams t ON t.id=c.team_id
                WHERE c.workspace_id<>t.workspace_id OR NOT EXISTS(SELECT 1 FROM team_brands tb WHERE tb.team_id=t.id AND tb.brand_id=c.brand_id)
            UNION ALL SELECT 'channel_scope_needs_review',id FROM team_channel_access WHERE NOT scope_enabled_v2
            UNION ALL SELECT 'invalid_channel_brand',ca.id FROM team_channel_access ca JOIN team_brands tb ON tb.id=ca.team_brand_id
                JOIN social_integrations si ON si.id=ca.integration_id JOIN teams t ON t.id=tb.team_id
                WHERE si.brand_id<>tb.brand_id OR si.workspace_id<>t.workspace_id;
            CREATE VIEW rbac_v2_channel_diff AS
              SELECT ca.id, tb.team_id, tb.brand_id, ca.integration_id,
                     ca.can_view legacy_view,ca.can_publish legacy_publish,ca.can_manage legacy_manage,
                     (ca.can_view OR ca.can_publish) candidate_scope,ca.scope_enabled_v2,
                     'Review TeamRole-based actions before enabling scope'::text review_reason
              FROM team_channel_access ca JOIN team_brands tb ON tb.id=ca.team_brand_id;
            """);
    }

    // Re-running this SQL is safe: never changes a non-null attribution or adds any member.
    public const string BackfillSql = """
        WITH created AS (
            INSERT INTO teams(id,workspace_id,name,description,status,is_deleted,created_at,default_for_brand_id)
            SELECT gen_random_uuid(),b.workspace_id,'Default Team','Legacy content attribution',0,false,now(),b.id
            FROM brands b WHERE NOT b.is_deleted AND EXISTS(SELECT 1 FROM workspaces w WHERE w.id=b.workspace_id)
              AND EXISTS(SELECT 1 FROM contents c WHERE c.brand_id=b.id AND c.workspace_id=b.workspace_id AND c.team_id IS NULL)
            ON CONFLICT(default_for_brand_id) DO NOTHING RETURNING id
        ) INSERT INTO rbac_v2_created_teams SELECT id FROM created ON CONFLICT DO NOTHING;
        INSERT INTO team_brands(id,team_id,brand_id,assigned_at,is_active,channel_access_mode)
        SELECT gen_random_uuid(),t.id,t.default_for_brand_id,now(),true,0 FROM teams t
        WHERE t.default_for_brand_id IS NOT NULL AND NOT t.is_deleted AND t.status=0
        ON CONFLICT(team_id,brand_id) DO NOTHING;
        WITH changed AS (
            UPDATE contents c SET team_id=t.id FROM teams t JOIN team_brands tb ON tb.team_id=t.id
            WHERE c.team_id IS NULL AND t.default_for_brand_id=c.brand_id AND t.workspace_id=c.workspace_id
              AND tb.brand_id=c.brand_id AND tb.is_active AND t.status=0 AND NOT t.is_deleted
            RETURNING c.id,c.team_id
        ) INSERT INTO rbac_v2_backfill_contents SELECT id,team_id FROM changed ON CONFLICT DO NOTHING;
        """;

    protected override void Down(MigrationBuilder m)
    {
        // Only roll back untouched migration-created teams. Refuse destructive rollback after adoption.
        m.Sql("""
            DO $$ BEGIN
              IF EXISTS(SELECT 1 FROM teams t JOIN rbac_v2_created_teams r ON r.team_id=t.id
                WHERE EXISTS(SELECT 1 FROM team_members tm WHERE tm.team_id=t.id)
                   OR EXISTS(SELECT 1 FROM team_channel_access ca JOIN team_brands tb ON tb.id=ca.team_brand_id WHERE tb.team_id=t.id)
                   OR EXISTS(SELECT 1 FROM team_brands tb WHERE tb.team_id=t.id AND tb.brand_id<>t.default_for_brand_id)
                   OR EXISTS(SELECT 1 FROM contents c WHERE c.team_id=t.id AND NOT EXISTS
                      (SELECT 1 FROM rbac_v2_backfill_contents bc WHERE bc.content_id=c.id AND bc.team_id=t.id)))
                THEN RAISE EXCEPTION 'RBAC v2 teams adopted; restore verified backup instead of automatic rollback'; END IF;
            END $$;
            DROP VIEW rbac_v2_preflight;
            DROP VIEW rbac_v2_channel_diff;
            DROP TRIGGER validate_rbac_v2_channel ON team_channel_access;
            DROP FUNCTION validate_rbac_v2_channel();
            ALTER TABLE workspace_members DROP CONSTRAINT rbac_v2_member_role;
            ALTER TABLE workspace_invitations DROP CONSTRAINT rbac_v2_invitation_role;
            UPDATE contents c SET team_id=NULL FROM rbac_v2_backfill_contents b WHERE c.id=b.content_id AND c.team_id=b.team_id;
            DELETE FROM team_channel_access WHERE team_brand_id IN (SELECT id FROM team_brands WHERE team_id IN(SELECT team_id FROM rbac_v2_created_teams));
            DELETE FROM team_brands WHERE team_id IN(SELECT team_id FROM rbac_v2_created_teams);
            DELETE FROM teams WHERE id IN(SELECT team_id FROM rbac_v2_created_teams);
            DROP TABLE rbac_v2_backfill_contents,rbac_v2_created_teams,rbac_v2_legacy_snapshot;
            """);
        m.DropForeignKey("FK_teams_brands_default_for_brand_id", "teams");
        m.DropIndex("IX_teams_default_for_brand_id", "teams");
        m.DropColumn("default_for_brand_id", "teams");
        m.DropColumn("scope_enabled_v2", "team_channel_access");
        m.DropColumn("workspace_role_v2", "workspace_invitations");
    }
}
