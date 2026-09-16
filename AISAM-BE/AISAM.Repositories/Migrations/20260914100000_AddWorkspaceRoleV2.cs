using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AISAM.Repositories.Migrations;

[DbContext(typeof(AisamContext))]
[Migration("20260914100000_AddWorkspaceRoleV2")]
public class AddWorkspaceRoleV2 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>("workspace_role_v2", "workspace_members", nullable: true);
        // Keep legacy roles intact for the staged rollout. Unknown legacy values stay null.
        migrationBuilder.Sql("""
            UPDATE workspace_members
            SET workspace_role_v2 = CASE role WHEN 1 THEN 1 WHEN 2 THEN 3 WHEN 3 THEN 3 WHEN 4 THEN 3 END
            WHERE workspace_role_v2 IS NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("workspace_role_v2", "workspace_members");
    }
}
