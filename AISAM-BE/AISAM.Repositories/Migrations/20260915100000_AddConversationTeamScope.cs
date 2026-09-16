using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AISAM.Repositories.Migrations;

[DbContext(typeof(AisamContext))]
[Migration("20260915100000_AddConversationTeamScope")]
public sealed class AddConversationTeamScope : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
        => migrationBuilder.AddColumn<Guid>("team_id", "conversations", type: "uuid", nullable: true);
    // Old conversations have no trustworthy Team attribution; do not guess from Brand.
    protected override void Down(MigrationBuilder migrationBuilder)
        => migrationBuilder.DropColumn("team_id", "conversations");
}
