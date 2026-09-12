using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddSnapshotMediaDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "duration_seconds",
                table: "snapshot_media",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "height",
                table: "snapshot_media",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "width",
                table: "snapshot_media",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "duration_seconds",
                table: "snapshot_media");

            migrationBuilder.DropColumn(
                name: "height",
                table: "snapshot_media");

            migrationBuilder.DropColumn(
                name: "width",
                table: "snapshot_media");
        }
    }
}
