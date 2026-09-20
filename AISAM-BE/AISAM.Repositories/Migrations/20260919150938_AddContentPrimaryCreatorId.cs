using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddContentPrimaryCreatorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_contents_primary_creator_id",
                table: "contents",
                column: "primary_creator_id");

            migrationBuilder.AddForeignKey(
                name: "FK_contents_users_primary_creator_id",
                table: "contents",
                column: "primary_creator_id",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_contents_users_primary_creator_id",
                table: "contents");

            migrationBuilder.DropIndex(
                name: "IX_contents_primary_creator_id",
                table: "contents");
        }
    }
}
