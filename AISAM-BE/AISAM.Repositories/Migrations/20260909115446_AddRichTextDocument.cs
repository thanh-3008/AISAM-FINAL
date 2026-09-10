using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddRichTextDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "rich_text_json",
                table: "contents",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rich_text_version",
                table: "contents",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rich_text_json",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "rich_text_version",
                table: "contents");
        }
    }
}
