using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AISAM.Repositories.Migrations;

[DbContext(typeof(AisamContext))]
[Migration("20260914120000_AddAutomationTeamScope")]
public sealed class AddAutomationTeamScope : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>("team_id", "automation_items", type: "uuid", nullable: true);
        // Existing content is the only trustworthy source; never infer membership from Brand.
        migrationBuilder.Sql("""
            UPDATE automation_items i SET team_id=c.team_id
            FROM contents c, automation_plans p
            WHERE i.content_id=c.id AND i.automation_plan_id=p.id
              AND p.workspace_id=c.workspace_id AND i.brand_id=c.brand_id;
            """);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropColumn("team_id", "automation_items");
}
