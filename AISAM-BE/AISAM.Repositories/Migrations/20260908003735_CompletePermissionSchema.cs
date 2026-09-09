using Microsoft.EntityFrameworkCore.Migrations;
namespace AISAM.Repositories.Migrations;
public partial class CompletePermissionSchema : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.Sql("""
        ALTER TABLE team_channel_access ADD COLUMN IF NOT EXISTS can_view boolean NOT NULL DEFAULT false,
          ADD COLUMN IF NOT EXISTS can_publish boolean NOT NULL DEFAULT false, ADD COLUMN IF NOT EXISTS can_manage boolean NOT NULL DEFAULT false;
        ALTER TABLE contents ADD COLUMN IF NOT EXISTS updated_by_user_id uuid;
        ALTER TABLE posts ADD COLUMN IF NOT EXISTS published_by_user_id uuid, ADD COLUMN IF NOT EXISTS executed_by_system boolean NOT NULL DEFAULT false;
        ALTER TABLE content_calendar ADD COLUMN IF NOT EXISTS scheduled_by_user_id uuid;
        ALTER TABLE audit_logs ADD COLUMN IF NOT EXISTS workspace_id uuid,
          ADD COLUMN IF NOT EXISTS affected_user_id uuid, ADD COLUMN IF NOT EXISTS approved_by uuid,
          ADD COLUMN IF NOT EXISTS requested_by uuid, ADD COLUMN IF NOT EXISTS reference_id uuid,
          ADD COLUMN IF NOT EXISTS team_id uuid, ADD COLUMN IF NOT EXISTS result varchar(30),
          ADD COLUMN IF NOT EXISTS executed_by_system boolean NOT NULL DEFAULT false;
        CREATE INDEX IF NOT EXISTS "IX_posts_published_by_user_id_published_at" ON posts(published_by_user_id,published_at);
        CREATE INDEX IF NOT EXISTS "IX_contents_workspace_id_brand_id_primary_creator_id_created_at" ON contents(workspace_id,brand_id,primary_creator_id,created_at);
        CREATE INDEX IF NOT EXISTS "IX_audit_logs_workspace_id_created_at" ON audit_logs(workspace_id,created_at);
        CREATE TABLE IF NOT EXISTS permission_migration_issues(resource_table text NOT NULL,resource_id uuid NOT NULL,issue text NOT NULL,PRIMARY KEY(resource_table,resource_id,issue));
        INSERT INTO permission_migration_issues SELECT 'posts',p.id,'channel_brand_mismatch' FROM posts p JOIN contents c ON c.id=p.content_id JOIN social_integrations i ON i.id=p.integration_id
          WHERE c.workspace_id IS DISTINCT FROM i.workspace_id OR c.brand_id IS DISTINCT FROM i.brand_id ON CONFLICT DO NOTHING;
        INSERT INTO permission_migration_issues SELECT 'contents',id,'creator_unknown' FROM contents WHERE primary_creator_id IS NULL ON CONFLICT DO NOTHING;
        INSERT INTO permission_migration_issues SELECT 'automation_plans',id,'creator_unknown' FROM automation_plans WHERE created_by_user_id IS NULL ON CONFLICT DO NOTHING;
        CREATE OR REPLACE FUNCTION aisam_permission_integrity() RETURNS trigger LANGUAGE plpgsql AS $$
        DECLARE w uuid; b uuid; tw uuid; n jsonb:=to_jsonb(NEW); o jsonb:=to_jsonb(OLD);
        BEGIN
          IF TG_OP='UPDATE' THEN
            IF TG_TABLE_NAME IN ('teams','brands','contents','social_integrations','automation_plans') AND n->'workspace_id' IS DISTINCT FROM o->'workspace_id' THEN
              RAISE EXCEPTION 'Workspace transfer requires explicit migration' USING ERRCODE='23514'; END IF;
            IF TG_TABLE_NAME IN ('contents','social_integrations','team_brands') AND n->'brand_id' IS DISTINCT FROM o->'brand_id' THEN
              RAISE EXCEPTION 'Brand transfer requires explicit migration' USING ERRCODE='23514'; END IF;
            IF TG_TABLE_NAME='team_brands' AND n->'team_id' IS DISTINCT FROM o->'team_id' THEN
              RAISE EXCEPTION 'Team transfer requires explicit migration' USING ERRCODE='23514'; END IF;
            IF TG_TABLE_NAME='contents' AND n->'primary_creator_id' IS DISTINCT FROM o->'primary_creator_id' THEN
              RAISE EXCEPTION 'Creator is immutable' USING ERRCODE='23514'; END IF;
            IF TG_TABLE_NAME='automation_plans' AND n->'created_by_user_id' IS DISTINCT FROM o->'created_by_user_id' THEN
              RAISE EXCEPTION 'Creator is immutable' USING ERRCODE='23514'; END IF;
          END IF;
          IF TG_TABLE_NAME='team_brands' THEN
            SELECT workspace_id INTO w FROM teams WHERE id=NEW.team_id FOR SHARE;
            SELECT workspace_id INTO tw FROM brands WHERE id=NEW.brand_id FOR SHARE;
            IF w IS NULL OR tw IS NULL OR w<>tw THEN RAISE EXCEPTION 'Team Brand workspace mismatch' USING ERRCODE='23514'; END IF;
          ELSIF TG_TABLE_NAME='team_channel_access' THEN
            SELECT brand_id INTO b FROM team_brands WHERE id=NEW.team_brand_id FOR SHARE;
            SELECT brand_id INTO w FROM social_integrations WHERE id=NEW.integration_id FOR SHARE;
            IF b IS NULL OR w IS NULL OR b<>w THEN RAISE EXCEPTION 'Channel Brand mismatch' USING ERRCODE='23514'; END IF;
            IF (NEW.can_publish OR NEW.can_manage) AND NOT NEW.can_view THEN RAISE EXCEPTION 'Channel mutation requires view grant' USING ERRCODE='23514'; END IF;
          ELSIF TG_TABLE_NAME IN ('contents','social_integrations') THEN
            SELECT workspace_id INTO w FROM brands WHERE id=NEW.brand_id FOR SHARE;
            IF w IS NULL OR w<>NEW.workspace_id THEN RAISE EXCEPTION 'Resource Brand workspace mismatch' USING ERRCODE='23514'; END IF;
            IF TG_TABLE_NAME='contents' AND n->>'team_id' IS NOT NULL THEN
              SELECT workspace_id INTO tw FROM teams WHERE id=(n->>'team_id')::uuid FOR SHARE;
              IF tw IS NULL OR tw<>NEW.workspace_id THEN RAISE EXCEPTION 'Content Team workspace mismatch' USING ERRCODE='23514'; END IF;
            END IF;
          ELSIF TG_TABLE_NAME='team_members' AND NEW.is_active THEN
            SELECT workspace_id INTO w FROM teams WHERE id=NEW.team_id FOR SHARE;
            PERFORM 1 FROM workspace_members WHERE workspace_id=w AND user_id=NEW.user_id AND is_active FOR SHARE;
            IF NOT FOUND THEN RAISE EXCEPTION 'Team member must be a workspace member' USING ERRCODE='23514'; END IF;
          ELSIF TG_TABLE_NAME='posts' THEN
            IF TG_OP='INSERT' OR n->'content_id' IS DISTINCT FROM o->'content_id' OR n->'integration_id' IS DISTINCT FROM o->'integration_id' THEN
              SELECT brand_id INTO b FROM contents WHERE id=NEW.content_id FOR SHARE;
              SELECT brand_id INTO w FROM social_integrations WHERE id=NEW.integration_id FOR SHARE;
              IF b IS NULL OR w IS NULL OR b<>w THEN RAISE EXCEPTION 'Post channel mismatch' USING ERRCODE='23514'; END IF;
            END IF;
          END IF;
          RETURN NEW;
        END $$;
        DO $$ DECLARE t text; BEGIN
          FOREACH t IN ARRAY ARRAY['teams','brands','contents','social_integrations','automation_plans','team_brands','team_members','team_channel_access','posts'] LOOP
            EXECUTE format('DROP TRIGGER IF EXISTS aisam_permission_integrity ON %I',t);
            EXECUTE format('CREATE TRIGGER aisam_permission_integrity BEFORE INSERT OR UPDATE ON %I FOR EACH ROW EXECUTE FUNCTION aisam_permission_integrity()',t);
          END LOOP;
        END $$;
        """);
    }
    protected override void Down(MigrationBuilder m) => throw new NotSupportedException("Restore a verified backup or roll back application preserving additive schema.");
}
