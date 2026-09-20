using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalReviewerSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "reviewer_name_snapshot",
                table: "approvals",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reviewer_role_snapshot",
                table: "approvals",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reviewer_name_snapshot",
                table: "approvals");

            migrationBuilder.DropColumn(
                name: "reviewer_role_snapshot",
                table: "approvals");
        }
    }
}
