using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ProvisionMissingPersonalFreePlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO subscriptions (
                    id, profile_id, workspace_id, plan,
                    quota_posts_per_month, quota_ai_content_per_day, quota_ai_images_per_day,
                    quota_platforms, quota_accounts, analysis_level, quota_ad_budget_monthly, quota_ad_campaigns,
                    start_date, end_date, is_active, is_deleted, created_at, updated_at,
                    payos_order_code, payos_payment_link_id)
                SELECT
                    md5('aisam-personal-free-subscription:' || workspace.id::text)::uuid,
                    NULL,
                    workspace.id,
                    0,
                    20, 0, 0, 1, 1, 0, 0, 0,
                    CURRENT_DATE,
                    NULL,
                    TRUE,
                    FALSE,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP,
                    NULL,
                    NULL
                FROM workspaces workspace
                WHERE workspace.workspace_type = 1
                  AND workspace.status <> 5
                  AND NOT EXISTS (
                      SELECT 1 FROM subscriptions subscription
                      WHERE subscription.workspace_id = workspace.id
                  );

                UPDATE credit_wallets wallet
                SET balance = 50,
                    updated_at = CURRENT_TIMESTAMP
                WHERE EXISTS (
                    SELECT 1 FROM subscriptions subscription
                    WHERE subscription.workspace_id = wallet.workspace_id
                      AND subscription.plan = 0
                      AND subscription.is_active = TRUE
                      AND subscription.is_deleted = FALSE
                )
                  AND NOT EXISTS (
                    SELECT 1 FROM credit_usage_records usage
                    WHERE usage.workspace_id = wallet.workspace_id
                      AND usage.action = 1
                      AND usage.status = 2
                );

                INSERT INTO credit_usage_records (
                    id, workspace_id, user_id, ai_generation_id, action, credits, status, created_at)
                SELECT
                    md5('aisam-personal-free-credit-grant:' || workspace.id::text)::uuid,
                    workspace.id,
                    owner.user_id,
                    NULL,
                    1,
                    50,
                    2,
                    CURRENT_TIMESTAMP
                FROM workspaces workspace
                JOIN workspace_members owner
                  ON owner.workspace_id = workspace.id
                 AND owner.role = 1
                 AND owner.is_active = TRUE
                WHERE EXISTS (
                    SELECT 1 FROM subscriptions subscription
                    WHERE subscription.workspace_id = workspace.id
                      AND subscription.plan = 0
                      AND subscription.is_active = TRUE
                      AND subscription.is_deleted = FALSE
                )
                  AND NOT EXISTS (
                    SELECT 1 FROM credit_usage_records usage
                    WHERE usage.workspace_id = workspace.id
                      AND usage.action = 1
                      AND usage.status = 2
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE credit_wallets wallet
                SET balance = 0,
                    updated_at = CURRENT_TIMESTAMP
                WHERE balance = 50
                  AND EXISTS (
                      SELECT 1 FROM credit_usage_records usage
                      WHERE usage.id = md5('aisam-personal-free-credit-grant:' || wallet.workspace_id::text)::uuid
                  );

                DELETE FROM credit_usage_records usage
                WHERE usage.id = md5('aisam-personal-free-credit-grant:' || usage.workspace_id::text)::uuid;

                DELETE FROM subscriptions subscription
                WHERE subscription.id = md5('aisam-personal-free-subscription:' || subscription.workspace_id::text)::uuid;
                """);
        }
    }
}
