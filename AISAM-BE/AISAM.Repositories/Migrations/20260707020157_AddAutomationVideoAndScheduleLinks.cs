using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomationVideoAndScheduleLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "content_calendar_id",
                table: "automation_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "video_job_id",
                table: "automation_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "video_provider",
                table: "automation_items",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_automation_items_content_calendar_id",
                table: "automation_items",
                column: "content_calendar_id",
                unique: true,
                filter: "content_calendar_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_automation_items_content_calendar_content_calendar_id",
                table: "automation_items",
                column: "content_calendar_id",
                principalTable: "content_calendar",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_automation_items_content_calendar_content_calendar_id",
                table: "automation_items");

            migrationBuilder.DropIndex(
                name: "IX_automation_items_content_calendar_id",
                table: "automation_items");

            migrationBuilder.DropColumn(
                name: "content_calendar_id",
                table: "automation_items");

            migrationBuilder.DropColumn(
                name: "video_job_id",
                table: "automation_items");

            migrationBuilder.DropColumn(
                name: "video_provider",
                table: "automation_items");
        }
    }
}
