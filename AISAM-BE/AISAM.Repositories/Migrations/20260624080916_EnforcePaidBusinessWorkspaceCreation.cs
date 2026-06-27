using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class EnforcePaidBusinessWorkspaceCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "payments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "pending_workspace_name",
                table: "payments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "requested_plan",
                table: "payments",
                type: "integer",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM payments WHERE workspace_id IS NULL;");

            migrationBuilder.DropColumn(
                name: "pending_workspace_name",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "requested_plan",
                table: "payments");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
