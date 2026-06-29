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
            migrationBuilder.AddColumn<Guid>(
                name: "content_id",
                table: "ad_campaigns",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "product_id",
                table: "ad_campaigns",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ad_campaigns_content_id",
                table: "ad_campaigns",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_ad_campaigns_product_id",
                table: "ad_campaigns",
                column: "product_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ad_campaigns_contents_content_id",
                table: "ad_campaigns",
                column: "content_id",
                principalTable: "contents",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_ad_campaigns_products_product_id",
                table: "ad_campaigns",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ad_campaigns_contents_content_id",
                table: "ad_campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_ad_campaigns_products_product_id",
                table: "ad_campaigns");

            migrationBuilder.DropIndex(
                name: "IX_ad_campaigns_content_id",
                table: "ad_campaigns");

            migrationBuilder.DropIndex(
                name: "IX_ad_campaigns_product_id",
                table: "ad_campaigns");

            migrationBuilder.DropColumn(
                name: "content_id",
                table: "ad_campaigns");

            migrationBuilder.DropColumn(
                name: "product_id",
                table: "ad_campaigns");
        }
    }
}
