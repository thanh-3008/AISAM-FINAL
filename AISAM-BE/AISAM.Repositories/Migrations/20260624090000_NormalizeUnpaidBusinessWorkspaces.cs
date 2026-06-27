using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    [DbContext(typeof(AisamContext))]
    [Migration("20260624090000_NormalizeUnpaidBusinessWorkspaces")]
    public partial class NormalizeUnpaidBusinessWorkspaces : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE subscriptions AS subscription
                SET is_active = FALSE,
                    end_date = COALESCE(subscription.end_date, CURRENT_DATE),
                    updated_at = NOW()
                FROM workspaces AS workspace
                WHERE subscription.workspace_id = workspace.id
                  AND workspace.workspace_type = 2
                  AND subscription.is_active = TRUE
                  AND NOT EXISTS (
                      SELECT 1
                      FROM payments AS payment
                      WHERE payment.status = 1
                        AND payment.payment_type = 1
                        AND (payment.subscription_id = subscription.id OR
                             (payment.subscription_id IS NULL AND payment.workspace_id = workspace.id))
                  );

                UPDATE workspaces AS workspace
                SET status = 2,
                    subscription_expired_at = NOW() - INTERVAL '1 second',
                    archived_at = NULL,
                    member_limit = 1,
                    updated_at = NOW()
                WHERE workspace.workspace_type = 2
                  AND workspace.status <> 5
                  AND NOT EXISTS (
                      SELECT 1
                      FROM payments AS payment
                      WHERE payment.workspace_id = workspace.id
                        AND payment.status = 1
                        AND payment.payment_type = 1
                  );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This data correction cannot restore unpaid subscriptions safely.
        }
    }
}
