using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class BackfillLegacyWorkspaceDataAndLockOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TEMP TABLE legacy_profile_workspace_map
                (
                    profile_id uuid PRIMARY KEY,
                    user_id uuid NOT NULL,
                    workspace_id uuid NOT NULL
                ) ON COMMIT DROP;

                INSERT INTO legacy_profile_workspace_map (profile_id, user_id, workspace_id)
                SELECT
                    p.id,
                    p.user_id,
                    COALESCE(
                        CASE WHEN profile_stats.profile_count = 1 THEN
                            (
                                SELECT CASE WHEN COUNT(*) = 1 THEN MIN(w.id::text)::uuid END
                                FROM workspace_members wm
                                JOIN workspaces w ON w.id = wm.workspace_id
                                WHERE wm.user_id = p.user_id
                                  AND wm.role = 1
                                  AND wm.is_active = TRUE
                                  AND w.workspace_type = 1
                                  AND w.status <> 5
                            )
                        END,
                        md5('aisam-legacy-profile-workspace:' || p.id::text)::uuid
                    )
                FROM profiles p
                JOIN (
                    SELECT user_id, COUNT(*) AS profile_count
                    FROM profiles
                    GROUP BY user_id
                ) profile_stats ON profile_stats.user_id = p.user_id;

                INSERT INTO workspaces
                    (id, name, workspace_type, status, member_limit, created_at, updated_at)
                SELECT
                    map.workspace_id,
                    LEFT(COALESCE(NULLIF(TRIM(p.name), ''), 'Personal') || ' Workspace', 255),
                    1,
                    1,
                    1,
                    COALESCE(p.created_at, CURRENT_TIMESTAMP),
                    CURRENT_TIMESTAMP
                FROM legacy_profile_workspace_map map
                JOIN profiles p ON p.id = map.profile_id
                WHERE NOT EXISTS (SELECT 1 FROM workspaces w WHERE w.id = map.workspace_id);

                INSERT INTO workspace_members
                    (id, workspace_id, user_id, role, quota_mode, credit_used, joined_at, is_active)
                SELECT
                    md5('aisam-legacy-profile-owner:' || map.profile_id::text)::uuid,
                    map.workspace_id,
                    map.user_id,
                    1,
                    1,
                    0,
                    CURRENT_TIMESTAMP,
                    TRUE
                FROM legacy_profile_workspace_map map
                WHERE NOT EXISTS (
                    SELECT 1 FROM workspace_members wm
                    WHERE wm.workspace_id = map.workspace_id AND wm.user_id = map.user_id
                );

                CREATE TEMP TABLE legacy_payment_workspace_map
                (
                    user_id uuid PRIMARY KEY,
                    workspace_id uuid NOT NULL
                ) ON COMMIT DROP;

                INSERT INTO legacy_payment_workspace_map (user_id, workspace_id)
                SELECT
                    u.id,
                    COALESCE(
                        (SELECT MIN(map.workspace_id::text)::uuid FROM legacy_profile_workspace_map map WHERE map.user_id = u.id),
                        md5('aisam-legacy-user-workspace:' || u.id::text)::uuid
                    )
                FROM users u
                WHERE EXISTS (SELECT 1 FROM payments payment WHERE payment.user_id = u.id AND payment.workspace_id IS NULL);

                INSERT INTO workspaces
                    (id, name, workspace_type, status, member_limit, created_at, updated_at)
                SELECT
                    map.workspace_id,
                    LEFT(COALESCE(NULLIF(TRIM(u.full_name), ''), 'Personal') || ' Workspace', 255),
                    1,
                    1,
                    1,
                    COALESCE(u.created_at, CURRENT_TIMESTAMP),
                    CURRENT_TIMESTAMP
                FROM legacy_payment_workspace_map map
                JOIN users u ON u.id = map.user_id
                WHERE NOT EXISTS (SELECT 1 FROM workspaces w WHERE w.id = map.workspace_id);

                INSERT INTO workspace_members
                    (id, workspace_id, user_id, role, quota_mode, credit_used, joined_at, is_active)
                SELECT
                    md5('aisam-legacy-user-owner:' || map.user_id::text)::uuid,
                    map.workspace_id,
                    map.user_id,
                    1,
                    1,
                    0,
                    CURRENT_TIMESTAMP,
                    TRUE
                FROM legacy_payment_workspace_map map
                WHERE NOT EXISTS (
                    SELECT 1 FROM workspace_members wm
                    WHERE wm.workspace_id = map.workspace_id AND wm.user_id = map.user_id
                );

                UPDATE subscriptions entity
                SET workspace_id = map.workspace_id
                FROM legacy_profile_workspace_map map
                WHERE entity.workspace_id IS NULL AND entity.profile_id = map.profile_id;

                UPDATE brands entity
                SET workspace_id = map.workspace_id
                FROM legacy_profile_workspace_map map
                WHERE entity.workspace_id IS NULL AND entity.profile_id = map.profile_id;

                UPDATE contents entity
                SET workspace_id = COALESCE(brand.workspace_id, map.workspace_id)
                FROM legacy_profile_workspace_map map
                LEFT JOIN brands brand ON brand.profile_id = map.profile_id
                WHERE entity.workspace_id IS NULL
                  AND entity.profile_id = map.profile_id
                  AND (brand.id IS NULL OR brand.id = entity.brand_id);

                UPDATE social_accounts entity
                SET workspace_id = map.workspace_id
                FROM legacy_profile_workspace_map map
                WHERE entity.workspace_id IS NULL AND entity.profile_id = map.profile_id;

                UPDATE social_integrations entity
                SET workspace_id = COALESCE(account.workspace_id, brand.workspace_id, map.workspace_id)
                FROM legacy_profile_workspace_map map
                LEFT JOIN social_accounts account ON account.profile_id = map.profile_id
                LEFT JOIN brands brand ON brand.profile_id = map.profile_id
                WHERE entity.workspace_id IS NULL
                  AND entity.profile_id = map.profile_id
                  AND (account.id IS NULL OR account.id = entity.social_account_id)
                  AND (brand.id IS NULL OR brand.id = entity.brand_id);

                UPDATE content_calendar entity
                SET workspace_id = COALESCE(content.workspace_id, map.workspace_id)
                FROM legacy_profile_workspace_map map
                LEFT JOIN contents content ON content.profile_id = map.profile_id
                WHERE entity.workspace_id IS NULL
                  AND entity.profile_id = map.profile_id
                  AND (content.id IS NULL OR content.id = entity.content_id);

                UPDATE conversations entity
                SET workspace_id = COALESCE(brand.workspace_id, map.workspace_id)
                FROM legacy_profile_workspace_map map
                LEFT JOIN brands brand ON brand.profile_id = map.profile_id
                WHERE entity.workspace_id IS NULL
                  AND entity.profile_id = map.profile_id
                  AND (brand.id IS NULL OR brand.id = entity.brand_id);

                UPDATE notifications entity
                SET workspace_id = map.workspace_id
                FROM legacy_profile_workspace_map map
                WHERE entity.workspace_id IS NULL AND entity.profile_id = map.profile_id;

                UPDATE ad_campaigns entity
                SET workspace_id = COALESCE(brand.workspace_id, map.workspace_id)
                FROM legacy_profile_workspace_map map
                LEFT JOIN brands brand ON brand.profile_id = map.profile_id
                WHERE entity.workspace_id IS NULL
                  AND entity.profile_id = map.profile_id
                  AND (brand.id IS NULL OR brand.id = entity.brand_id);

                UPDATE payments entity
                SET workspace_id = subscription.workspace_id
                FROM subscriptions subscription
                WHERE entity.workspace_id IS NULL
                  AND entity.subscription_id = subscription.id;

                UPDATE payments entity
                SET workspace_id = map.workspace_id
                FROM legacy_payment_workspace_map map
                WHERE entity.workspace_id IS NULL AND entity.user_id = map.user_id;

                INSERT INTO credit_wallets (id, workspace_id, balance, created_at, updated_at)
                SELECT
                    md5('aisam-legacy-workspace-wallet:' || w.id::text)::uuid,
                    w.id,
                    0,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                FROM workspaces w
                WHERE NOT EXISTS (SELECT 1 FROM credit_wallets wallet WHERE wallet.workspace_id = w.id);

                DO $$
                DECLARE
                    missing_count bigint;
                BEGIN
                    SELECT
                        (SELECT COUNT(*) FROM subscriptions WHERE workspace_id IS NULL) +
                        (SELECT COUNT(*) FROM payments WHERE workspace_id IS NULL) +
                        (SELECT COUNT(*) FROM brands WHERE workspace_id IS NULL) +
                        (SELECT COUNT(*) FROM contents WHERE workspace_id IS NULL) +
                        (SELECT COUNT(*) FROM social_accounts WHERE workspace_id IS NULL) +
                        (SELECT COUNT(*) FROM social_integrations WHERE workspace_id IS NULL) +
                        (SELECT COUNT(*) FROM content_calendar WHERE workspace_id IS NULL) +
                        (SELECT COUNT(*) FROM conversations WHERE workspace_id IS NULL) +
                        (SELECT COUNT(*) FROM notifications WHERE workspace_id IS NULL) +
                        (SELECT COUNT(*) FROM ad_campaigns WHERE workspace_id IS NULL)
                    INTO missing_count;

                    IF missing_count > 0 THEN
                        RAISE EXCEPTION 'Workspace backfill left % ownership rows unmapped', missing_count;
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "subscriptions",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "social_integrations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "social_accounts",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "payments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "notifications",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "conversations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "contents",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "content_calendar",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "brands",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "ad_campaigns",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "subscriptions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "social_integrations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "social_accounts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "payments",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "notifications",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "conversations",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "contents",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "content_calendar",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "brands",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "workspace_id",
                table: "ad_campaigns",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
