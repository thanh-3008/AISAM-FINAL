using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddContentVideoUrlsJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "video_urls",
                table: "contents",
                type: "jsonb",
                nullable: true);

            // Backward-compatibility: populate video_urls JSON array for existing video_url rows
            migrationBuilder.Sql("""
                UPDATE contents
                SET video_urls = json_build_array(video_url)::jsonb
                WHERE video_url IS NOT NULL AND video_urls IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "video_urls",
                table: "contents");
        }
    }
}
