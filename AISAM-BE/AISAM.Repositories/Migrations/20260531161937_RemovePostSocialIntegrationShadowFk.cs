using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemovePostSocialIntegrationShadowFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_posts_social_integrations_SocialIntegrationId",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "IX_posts_SocialIntegrationId",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "SocialIntegrationId",
                table: "posts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SocialIntegrationId",
                table: "posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_posts_SocialIntegrationId",
                table: "posts",
                column: "SocialIntegrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_posts_social_integrations_SocialIntegrationId",
                table: "posts",
                column: "SocialIntegrationId",
                principalTable: "social_integrations",
                principalColumn: "id");
        }
    }
}
