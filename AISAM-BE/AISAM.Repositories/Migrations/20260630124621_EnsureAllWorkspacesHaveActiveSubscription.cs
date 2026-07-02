using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EnsureAllWorkspacesHaveActiveSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                -- 1. Backfill workspace_id for subscriptions still missing it
                --    Join through profile → user → owned workspace_member
                UPDATE subscriptions sub
                SET workspace_id = map.workspace_id,
                    updated_at = NOW()
                FROM (
                    SELECT DISTINCT ON (p.id)
                        p.id AS profile_id,
                        wm.workspace_id
                    FROM profiles p
                    JOIN workspace_members wm ON wm.user_id = p.user_id
                    WHERE wm.is_active = TRUE
                      AND wm.role = 1
                      AND wm.workspace_id IS NOT NULL
                    ORDER BY p.id, wm.joined_at ASC
                ) map
                WHERE sub.profile_id = map.profile_id
                  AND sub.workspace_id IS NULL;

                -- 2. Create Free subscriptions for any workspace that lacks one
                INSERT INTO subscriptions (
                    id, profile_id, workspace_id, plan,
                    quota_posts_per_month, quota_ai_content_per_day, quota_ai_images_per_day,
                    quota_platforms, quota_accounts, analysis_level,
                    quota_ad_budget_monthly, quota_ad_campaigns,
                    start_date, end_date, is_active, is_deleted, created_at, updated_at)
                SELECT
                    md5('aisam-ensure-subscription:' || w.id::text)::uuid,
                    NULL,
                    w.id,
                    0,
                    20, 0, 0, 1, 1, 0, 0, 0,
                    CURRENT_DATE,
                    NULL,
                    TRUE,
                    FALSE,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                FROM workspaces w
                WHERE w.status = 1
                  AND NOT EXISTS (
                      SELECT 1 FROM subscriptions s
                      WHERE s.workspace_id = w.id
                        AND s.is_active = TRUE
                        AND s.is_deleted = FALSE
                  );

                -- 3. Create credit wallets for workspaces still missing one
                INSERT INTO credit_wallets (id, workspace_id, balance, created_at, updated_at)
                SELECT
                    md5('aisam-ensure-wallet:' || w.id::text)::uuid,
                    w.id,
                    0,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                FROM workspaces w
                WHERE NOT EXISTS (
                    SELECT 1 FROM credit_wallets cw WHERE cw.workspace_id = w.id
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM credit_wallets
                WHERE id = md5('aisam-ensure-wallet:' || workspace_id::text)::uuid;

                DELETE FROM subscriptions
                WHERE id = md5('aisam-ensure-subscription:' || workspace_id::text)::uuid;
                """);
        }
    }
}
