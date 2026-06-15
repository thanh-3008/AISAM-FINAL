using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceInvitationQuotaModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "credit_limit",
                table: "workspace_invitations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "quota_mode",
                table: "workspace_invitations",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "credit_limit",
                table: "workspace_invitations");

            migrationBuilder.DropColumn(
                name: "quota_mode",
                table: "workspace_invitations");
        }
    }
}
