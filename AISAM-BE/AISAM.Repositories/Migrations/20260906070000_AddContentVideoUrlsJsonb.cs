using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    [DbContext(typeof(AisamContext))]
    [Migration("20260907083000_AddContentVideoUrlsJsonb")]
    public partial class AddContentVideoUrlsJsonb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE contents ADD COLUMN IF NOT EXISTS video_urls jsonb;");

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
            migrationBuilder.Sql("ALTER TABLE contents DROP COLUMN IF EXISTS video_urls;");
        }
    }
}
