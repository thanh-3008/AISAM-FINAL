using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AISAM.Repositories.Migrations;

[DbContext(typeof(AisamContext))]
[Migration("20260912140000_MigrateRbacSchemaAndEnums")]
public partial class MigrateRbacSchemaAndEnums : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Ensure foreign key constraint and index for contents.team_id -> teams.id
        migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS ""IX_contents_team_id"" ON contents(team_id);

            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'FK_contents_teams_team_id'
                ) THEN
                    ALTER TABLE contents 
                    ADD CONSTRAINT ""FK_contents_teams_team_id"" 
                    FOREIGN KEY (team_id) REFERENCES teams(id) ON DELETE SET NULL;
                END IF;
            END $$;
        ");

        // 2. Convert team_members.role from character varying to integer (TeamRoleEnum: Manager=1, ContentCreator=2, Viewer=3)
        migrationBuilder.Sql(@"
            DO $$
            BEGIN
                -- Validate distinct existing string values before altering column
                RAISE NOTICE 'Current distinct team_members roles:';
                PERFORM role FROM team_members GROUP BY role;

                -- Alter column type using explicit conversion mapping
                ALTER TABLE team_members 
                ALTER COLUMN role TYPE integer 
                USING (
                    CASE 
                        WHEN role::text = '1' THEN 1
                        WHEN role::text = '2' THEN 2
                        WHEN role::text = '3' THEN 3
                        WHEN LOWER(TRIM(role::text)) IN ('manager', 'owner') THEN 1
                        WHEN LOWER(TRIM(role::text)) = 'contentcreator' THEN 2
                        WHEN LOWER(TRIM(role::text)) = 'viewer' THEN 3
                        ELSE 3
                    END
                );

                -- Set default value to 3 (TeamRoleEnum.Viewer)
                ALTER TABLE team_members ALTER COLUMN role SET DEFAULT 3;
            END $$;
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Revert team_members.role back to varchar(100)
        migrationBuilder.Sql(@"
            ALTER TABLE team_members ALTER COLUMN role DROP DEFAULT;

            ALTER TABLE team_members 
            ALTER COLUMN role TYPE character varying(100) 
            USING (
                CASE 
                    WHEN role = 1 THEN 'Manager'
                    WHEN role = 2 THEN 'ContentCreator'
                    WHEN role = 3 THEN 'Viewer'
                    ELSE 'Viewer'
                END
            );
        ");

        // Drop foreign key and index on contents.team_id
        migrationBuilder.Sql(@"
            ALTER TABLE contents DROP CONSTRAINT IF EXISTS ""FK_contents_teams_team_id"";
            DROP INDEX IF EXISTS ""IX_contents_team_id"";
        ");
    }
}
