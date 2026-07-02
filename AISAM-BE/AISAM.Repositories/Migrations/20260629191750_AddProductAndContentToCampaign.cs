using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAndContentToCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                ADD COLUMN IF NOT EXISTS content_id uuid;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                ADD COLUMN IF NOT EXISTS product_id uuid;
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_ad_campaigns_content_id"
                ON ad_campaigns (content_id);
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_ad_campaigns_product_id"
                ON ad_campaigns (product_id);
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint
                        WHERE conname = 'FK_ad_campaigns_contents_content_id'
                    ) THEN
                        ALTER TABLE ad_campaigns
                        ADD CONSTRAINT "FK_ad_campaigns_contents_content_id"
                        FOREIGN KEY (content_id) REFERENCES contents(id);
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint
                        WHERE conname = 'FK_ad_campaigns_products_product_id'
                    ) THEN
                        ALTER TABLE ad_campaigns
                        ADD CONSTRAINT "FK_ad_campaigns_products_product_id"
                        FOREIGN KEY (product_id) REFERENCES products(id);
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                DROP CONSTRAINT IF EXISTS "FK_ad_campaigns_contents_content_id";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                DROP CONSTRAINT IF EXISTS "FK_ad_campaigns_products_product_id";
                """);

            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_ad_campaigns_content_id";
                """);

            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_ad_campaigns_product_id";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                DROP COLUMN IF EXISTS content_id;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE ad_campaigns
                DROP COLUMN IF EXISTS product_id;
                """);
        }
    }
}
