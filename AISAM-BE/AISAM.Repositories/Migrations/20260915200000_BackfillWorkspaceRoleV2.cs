using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AISAM.Repositories.Migrations;

[DbContext(typeof(AisamContext))]
[Migration("20260915200000_BackfillWorkspaceRoleV2")]
public sealed class BackfillWorkspaceRoleV2 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE workspace_members
            SET workspace_role_v2 = CASE role WHEN 1 THEN 1 WHEN 2 THEN 3 WHEN 3 THEN 3 WHEN 4 THEN 3 END
            WHERE workspace_role_v2 IS NULL;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
