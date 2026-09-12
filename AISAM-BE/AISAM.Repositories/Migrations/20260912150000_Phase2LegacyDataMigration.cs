using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AISAM.Repositories.Migrations;

[DbContext(typeof(AisamContext))]
[Migration("20260912150000_Phase2LegacyDataMigration")]
public partial class Phase2LegacyDataMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Ensure Default Team for workspaces lacking an active team
        migrationBuilder.Sql(@"
            INSERT INTO teams (id, workspace_id, name, description, status, is_deleted, created_at)
            SELECT 
                gen_random_uuid(),
                w.id,
                'Default Team',
                'System generated team for legacy content and brand scoping',
                1, -- TeamStatusEnum.Active
                false,
                NOW()
            FROM workspaces w
            WHERE NOT EXISTS (
                SELECT 1 FROM teams t 
                WHERE t.workspace_id = w.id AND t.is_deleted = false AND t.status = 1
            )
            AND (
                EXISTS (SELECT 1 FROM contents c WHERE c.workspace_id = w.id AND c.team_id IS NULL)
                OR EXISTS (SELECT 1 FROM brands b WHERE b.workspace_id = w.id AND b.is_deleted = false)
            );
        ");

        // 2. Ensure each Brand with contents has at least one TeamBrand link
        migrationBuilder.Sql(@"
            INSERT INTO team_brands (id, team_id, brand_id, is_active)
            SELECT 
                gen_random_uuid(),
                (
                    SELECT t.id FROM teams t 
                    WHERE t.workspace_id = b.workspace_id AND t.is_deleted = false AND t.status = 1 
                    ORDER BY t.created_at ASC LIMIT 1
                ),
                b.id,
                true
            FROM brands b
            WHERE NOT EXISTS (
                SELECT 1 FROM team_brands tb 
                JOIN teams t ON t.id = tb.team_id 
                WHERE tb.brand_id = b.id AND tb.is_active = true AND t.workspace_id = b.workspace_id AND t.is_deleted = false
            )
            AND EXISTS (
                SELECT 1 FROM contents c WHERE c.brand_id = b.id AND c.team_id IS NULL
            )
            ON CONFLICT (team_id, brand_id) DO UPDATE SET is_active = true;
        ");

        // 3. Backfill contents.team_id (D-02)
        migrationBuilder.Sql(@"
            UPDATE contents c
            SET team_id = COALESCE(
                (
                    SELECT tb.team_id 
                    FROM team_brands tb
                    JOIN teams t ON t.id = tb.team_id
                    WHERE tb.brand_id = c.brand_id 
                      AND tb.is_active = true 
                      AND t.workspace_id = c.workspace_id 
                      AND t.is_deleted = false
                      AND t.status = 1
                    ORDER BY tb.id ASC
                    LIMIT 1
                ),
                (
                    SELECT t.id 
                    FROM teams t 
                    WHERE t.workspace_id = c.workspace_id 
                      AND t.is_deleted = false 
                      AND t.status = 1
                    ORDER BY t.created_at ASC 
                    LIMIT 1
                )
            )
            WHERE c.team_id IS NULL;
        ");

        // 4. Auto-provision TeamMember.Role = Manager (1) for legacy Managers (D-01)
        migrationBuilder.Sql(@"
            INSERT INTO team_members (id, team_id, user_id, role, permissions, joined_at, is_active)
            SELECT 
                gen_random_uuid(),
                t.id,
                wm.user_id,
                1, -- TeamRoleEnum.Manager
                '[]'::jsonb,
                NOW(),
                true
            FROM workspace_members wm
            JOIN teams t ON t.workspace_id = wm.workspace_id AND t.is_deleted = false AND t.status = 1
            WHERE wm.role = 2 AND wm.is_active = true
            ON CONFLICT (team_id, user_id) 
            DO UPDATE SET role = 1, is_active = true;
        ");

        // 5. Backup and remap workspace_members.role (D-01 & R-06)
        migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS _rbac_migration_wm_backup (
                workspace_member_id uuid PRIMARY KEY,
                workspace_id uuid NOT NULL,
                user_id uuid NOT NULL,
                old_role integer NOT NULL,
                new_role integer NOT NULL,
                migrated_at timestamp with time zone DEFAULT NOW()
            );

            INSERT INTO _rbac_migration_wm_backup (workspace_member_id, workspace_id, user_id, old_role, new_role)
            SELECT 
                wm.id, 
                wm.workspace_id, 
                wm.user_id, 
                wm.role,
                CASE 
                    WHEN wm.role = 1 THEN 1
                    ELSE 3 -- Member
                END
            FROM workspace_members wm
            ON CONFLICT (workspace_member_id) DO NOTHING;

            UPDATE workspace_members wm
            SET role = b.new_role
            FROM _rbac_migration_wm_backup b
            WHERE wm.id = b.workspace_member_id AND wm.role <> b.new_role;

            UPDATE workspace_invitations
            SET role = CASE 
                WHEN role = 1 THEN 1
                ELSE 3 -- Member
            END
            WHERE role NOT IN (1, 3);
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Restore original roles from backup table
        migrationBuilder.Sql(@"
            UPDATE workspace_members wm
            SET role = b.old_role
            FROM _rbac_migration_wm_backup b
            WHERE wm.id = b.workspace_member_id;

            DROP TABLE IF EXISTS _rbac_migration_wm_backup;
        ");
    }
}
