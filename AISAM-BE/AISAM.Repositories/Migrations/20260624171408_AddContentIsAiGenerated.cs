using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddContentIsAiGenerated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_ai_generated",
                table: "contents",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_ai_generated",
                table: "contents");
        }
    }
}
