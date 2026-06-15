using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceMemberLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "member_limit",
                table: "workspaces",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.Sql(
                "UPDATE workspaces SET member_limit = 10 WHERE workspace_type = 2;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "member_limit",
                table: "workspaces");
        }
    }
}
