using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAdSetShadowForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ad_sets_ad_campaigns_AdCampaignId",
                table: "ad_sets");

            migrationBuilder.DropIndex(
                name: "IX_content_calendar_content_id",
                table: "content_calendar");

            migrationBuilder.DropIndex(
                name: "IX_ad_sets_AdCampaignId",
                table: "ad_sets");

            migrationBuilder.DropColumn(
                name: "AdCampaignId",
                table: "ad_sets");

            migrationBuilder.CreateIndex(
                name: "IX_content_calendar_content_id",
                table: "content_calendar",
                column: "content_id",
                unique: true,
                filter: "\"status\" IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_content_calendar_content_id",
                table: "content_calendar");

            migrationBuilder.AddColumn<Guid>(
                name: "AdCampaignId",
                table: "ad_sets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_calendar_content_id",
                table: "content_calendar",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_ad_sets_AdCampaignId",
                table: "ad_sets",
                column: "AdCampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_ad_sets_ad_campaigns_AdCampaignId",
                table: "ad_sets",
                column: "AdCampaignId",
                principalTable: "ad_campaigns",
                principalColumn: "id");
        }
    }
}
