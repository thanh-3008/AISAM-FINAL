using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddContentCalendarSchedulingRuntimeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "attempt_count",
                table: "content_calendar",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "executed_at",
                table: "content_calendar",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "integration_id",
                table: "content_calendar",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_error",
                table: "content_calendar",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "scheduled_at",
                table: "content_calendar",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "content_calendar",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_content_calendar_integration_id",
                table: "content_calendar",
                column: "integration_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_calendar_scheduled_at",
                table: "content_calendar",
                column: "scheduled_at");

            migrationBuilder.CreateIndex(
                name: "IX_content_calendar_status",
                table: "content_calendar",
                column: "status");

            migrationBuilder.AddForeignKey(
                name: "FK_content_calendar_social_integrations_integration_id",
                table: "content_calendar",
                column: "integration_id",
                principalTable: "social_integrations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_content_calendar_social_integrations_integration_id",
                table: "content_calendar");

            migrationBuilder.DropIndex(
                name: "IX_content_calendar_integration_id",
                table: "content_calendar");

            migrationBuilder.DropIndex(
                name: "IX_content_calendar_scheduled_at",
                table: "content_calendar");

            migrationBuilder.DropIndex(
                name: "IX_content_calendar_status",
                table: "content_calendar");

            migrationBuilder.DropColumn(
                name: "attempt_count",
                table: "content_calendar");

            migrationBuilder.DropColumn(
                name: "executed_at",
                table: "content_calendar");

            migrationBuilder.DropColumn(
                name: "integration_id",
                table: "content_calendar");

            migrationBuilder.DropColumn(
                name: "last_error",
                table: "content_calendar");

            migrationBuilder.DropColumn(
                name: "scheduled_at",
                table: "content_calendar");

            migrationBuilder.DropColumn(
                name: "status",
                table: "content_calendar");
        }
    }
}
