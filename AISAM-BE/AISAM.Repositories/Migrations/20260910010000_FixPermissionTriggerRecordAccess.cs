using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AISAM.Repositories.Migrations;

[DbContext(typeof(AisamContext))]
[Migration("20260910010000_FixPermissionTriggerRecordAccess")]
public class FixPermissionTriggerRecordAccess : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("""
        DO $$
        DECLARE definition text;
        BEGIN
          SELECT pg_get_functiondef('aisam_permission_integrity()'::regprocedure) INTO definition;
          IF position('TG_TABLE_NAME=''team_members'' AND COALESCE((n->>''is_active'')::boolean, false)' IN definition) > 0 THEN
            RETURN;
          END IF;
          IF position('TG_TABLE_NAME=''team_members'' AND NEW.is_active' IN definition) = 0 THEN
            RAISE EXCEPTION 'Unexpected permission trigger definition; inspect before migrating';
          END IF;
          definition := replace(definition,
            'TG_TABLE_NAME=''team_members'' AND NEW.is_active',
            'TG_TABLE_NAME=''team_members'' AND COALESCE((n->>''is_active'')::boolean, false)');
          EXECUTE definition;
        END $$;
        """);

    // Keep the compatible bug fix on downgrade; reverting it breaks Brand/Team inserts.
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
