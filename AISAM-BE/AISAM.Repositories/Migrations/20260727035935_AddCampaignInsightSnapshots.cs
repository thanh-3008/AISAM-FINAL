using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignInsightSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "campaign_insight_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    snapshot_date = table.Column<DateTime>(type: "date", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    impressions = table.Column<long>(type: "bigint", nullable: false),
                    reach = table.Column<long>(type: "bigint", nullable: true),
                    clicks = table.Column<long>(type: "bigint", nullable: false),
                    engagement = table.Column<long>(type: "bigint", nullable: true),
                    spend = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    conversions = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    attributed_revenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    attribution_window = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "default"),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_partial = table.Column<bool>(type: "boolean", nullable: false),
                    synced_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    raw_data = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campaign_insight_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_campaign_insight_snapshots_ad_campaigns_campaign_id",
                        column: x => x.campaign_id,
                        principalTable: "ad_campaigns",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_campaign_insight_snapshots_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_campaign_insight_snapshots_campaign_id_platform_snapshot_da~",
                table: "campaign_insight_snapshots",
                columns: new[] { "campaign_id", "platform", "snapshot_date", "attribution_window" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_campaign_insight_snapshots_campaign_id_snapshot_date",
                table: "campaign_insight_snapshots",
                columns: new[] { "campaign_id", "snapshot_date" });

            migrationBuilder.CreateIndex(
                name: "IX_campaign_insight_snapshots_workspace_id_snapshot_date",
                table: "campaign_insight_snapshots",
                columns: new[] { "workspace_id", "snapshot_date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaign_insight_snapshots");
        }
    }
}
