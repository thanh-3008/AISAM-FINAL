using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace AISAM.Repositories.Migrations;
public partial class ReconcilePermissionFoundation : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            ALTER TABLE teams ALTER COLUMN profile_id DROP NOT NULL;
            ALTER TABLE teams ADD COLUMN IF NOT EXISTS workspace_id uuid NULL;
            UPDATE teams t SET workspace_id=p.workspace_id FROM profiles p
            WHERE t.workspace_id IS NULL AND t.profile_id=p.id AND p.workspace_id IS NOT NULL
              AND EXISTS (SELECT 1 FROM workspaces w WHERE w.id=p.workspace_id)
              AND NOT EXISTS (SELECT 1 FROM team_brands tb JOIN brands b ON b.id=tb.brand_id
                WHERE tb.team_id=t.id AND b.workspace_id IS DISTINCT FROM p.workspace_id);
            DO $$ BEGIN
                IF EXISTS (SELECT 1 FROM teams t LEFT JOIN workspaces w ON w.id=t.workspace_id WHERE w.id IS NULL)
                THEN RAISE EXCEPTION 'Unresolved Team workspace; reconcile data before migration'; END IF;
                IF EXISTS (SELECT 1 FROM team_brands tb JOIN teams t ON t.id=tb.team_id JOIN brands b ON b.id=tb.brand_id
                  WHERE t.workspace_id IS DISTINCT FROM b.workspace_id)
                THEN RAISE EXCEPTION 'Cross-workspace TeamBrand'; END IF;
            END $$;
            ALTER TABLE teams ALTER COLUMN workspace_id SET NOT NULL;
            ALTER TABLE team_brands ADD COLUMN IF NOT EXISTS channel_access_mode integer NOT NULL DEFAULT 0;
            ALTER TABLE contents ADD COLUMN IF NOT EXISTS primary_creator_id uuid NULL;
            ALTER TABLE contents ADD COLUMN IF NOT EXISTS team_id uuid NULL;
            CREATE TABLE IF NOT EXISTS team_channel_access (
                id uuid NOT NULL CONSTRAINT "PK_team_channel_access" PRIMARY KEY,
                team_brand_id uuid NOT NULL, integration_id uuid NOT NULL);
            DO $$ BEGIN
                IF EXISTS (SELECT 1 FROM team_channel_access ca
                    LEFT JOIN team_brands tb ON tb.id=ca.team_brand_id
                    LEFT JOIN brands b ON b.id=tb.brand_id
                    LEFT JOIN social_integrations i ON i.id=ca.integration_id
                    WHERE tb.id IS NULL OR b.id IS NULL OR i.id IS NULL
                    OR b.workspace_id IS DISTINCT FROM i.workspace_id OR b.id IS DISTINCT FROM i.brand_id)
                THEN RAISE EXCEPTION 'Invalid TeamChannelAccess'; END IF;
                IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname='FK_teams_workspaces_workspace_id' AND conrelid='teams'::regclass)
                THEN ALTER TABLE teams ADD CONSTRAINT "FK_teams_workspaces_workspace_id"
                  FOREIGN KEY (workspace_id) REFERENCES workspaces(id) ON DELETE CASCADE; END IF;
                IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname='FK_team_channel_access_team_brands_team_brand_id' AND conrelid='team_channel_access'::regclass)
                THEN ALTER TABLE team_channel_access ADD CONSTRAINT "FK_team_channel_access_team_brands_team_brand_id"
                  FOREIGN KEY (team_brand_id) REFERENCES team_brands(id) ON DELETE CASCADE; END IF;
                IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname='FK_team_channel_access_social_integrations_integration_id' AND conrelid='team_channel_access'::regclass)
                THEN ALTER TABLE team_channel_access ADD CONSTRAINT "FK_team_channel_access_social_integrations_integration_id"
                  FOREIGN KEY (integration_id) REFERENCES social_integrations(id) ON DELETE CASCADE; END IF;
            END $$;
            CREATE INDEX IF NOT EXISTS "IX_teams_workspace_id" ON teams(workspace_id);
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_team_members_team_id_user_id" ON team_members(team_id,user_id);
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_team_brands_team_id_brand_id" ON team_brands(team_id,brand_id);
            CREATE INDEX IF NOT EXISTS "IX_team_channel_access_integration_id" ON team_channel_access(integration_id);
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_team_channel_access_team_brand_id_integration_id"
              ON team_channel_access(team_brand_id,integration_id);
            """);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Columns may predate this migration; never drop legacy ownership data.
        throw new NotSupportedException("Restore a verified backup or roll back the application preserving additive schema; automatic destructive rollback is unsupported.");
    }
}