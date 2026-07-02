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

            migrationBuilder.Sql("""
                WITH ranked AS (
                    SELECT
                        id,
                        ROW_NUMBER() OVER (
                            PARTITION BY content_id
                            ORDER BY updated_at DESC, created_at DESC, id DESC
                        ) AS rn
                    FROM content_calendar
                    WHERE status IN (0, 1)
                )
                UPDATE content_calendar AS cc
                SET
                    status = 3,
                    last_error = COALESCE(cc.last_error, 'Superseded by a newer active schedule during migration.'),
                    updated_at = NOW()
                FROM ranked
                WHERE cc.id = ranked.id
                  AND ranked.rn > 1;
                """);

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
