using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AISAM.Repositories.Migrations;

[DbContext(typeof(AisamContext))]
[Migration("20260910110000_ReconcileBrandAndCampaignSchema")]
public sealed class ReconcileBrandAndCampaignSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        foreach (var column in new[] { "accent_color", "body_font", "brand_values", "heading_font", "preferred_terms", "primary_color", "prohibited_terms", "secondary_color", "tone_of_voice" })
            migrationBuilder.Sql($"ALTER TABLE brands ADD COLUMN IF NOT EXISTS {column} text NULL;");
        foreach (var column in new[] { "benefits", "pain_points", "sku", "status", "tags" })
            migrationBuilder.Sql($"ALTER TABLE products ADD COLUMN IF NOT EXISTS {column} text NULL;");
        migrationBuilder.Sql("""
            CREATE TABLE IF NOT EXISTS campaign_insight_snapshots (
                id uuid PRIMARY KEY,
                workspace_id uuid NOT NULL REFERENCES workspaces(id) ON DELETE CASCADE,
                campaign_id uuid NOT NULL REFERENCES ad_campaigns(id) ON DELETE CASCADE,
                snapshot_date date NOT NULL,
                platform text NULL, currency text NULL, source text NULL,
                attribution_window text NULL, raw_data jsonb NULL,
                impressions bigint NULL, clicks bigint NULL, engagement bigint NULL, reach bigint NULL,
                conversions numeric NULL, spend numeric NULL, attributed_revenue numeric NULL,
                is_partial boolean NOT NULL,
                created_at timestamptz NOT NULL, updated_at timestamptz NOT NULL, synced_at timestamptz NULL
            );
            CREATE INDEX IF NOT EXISTS "IX_campaign_insight_snapshots_CampaignId" ON campaign_insight_snapshots(campaign_id);
            CREATE INDEX IF NOT EXISTS "IX_campaign_insight_snapshots_SnapshotDate" ON campaign_insight_snapshots(snapshot_date);
            CREATE INDEX IF NOT EXISTS "IX_campaign_insight_snapshots_WorkspaceId" ON campaign_insight_snapshots(workspace_id);
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_campaign_insight_snapshots_CampaignId_SnapshotDate" ON campaign_insight_snapshots(campaign_id,snapshot_date);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException("This additive repair preserves pre-existing schema; restore a verified backup to reverse it.");
}
