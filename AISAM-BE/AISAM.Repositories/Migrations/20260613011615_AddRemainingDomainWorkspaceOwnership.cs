using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingDomainWorkspaceOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "social_integrations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "social_accounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "conversations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "contents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "content_calendar",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "ad_campaigns",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_social_integrations_workspace_id",
                table: "social_integrations",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_social_accounts_workspace_id",
                table: "social_accounts",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_workspace_id",
                table: "notifications",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_conversations_workspace_id",
                table: "conversations",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_contents_workspace_id",
                table: "contents",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_calendar_workspace_id",
                table: "content_calendar",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_ad_campaigns_workspace_id",
                table: "ad_campaigns",
                column: "workspace_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ad_campaigns_workspaces_workspace_id",
                table: "ad_campaigns",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_content_calendar_workspaces_workspace_id",
                table: "content_calendar",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_contents_workspaces_workspace_id",
                table: "contents",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_conversations_workspaces_workspace_id",
                table: "conversations",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_notifications_workspaces_workspace_id",
                table: "notifications",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_social_accounts_workspaces_workspace_id",
                table: "social_accounts",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_social_integrations_workspaces_workspace_id",
                table: "social_integrations",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ad_campaigns_workspaces_workspace_id",
                table: "ad_campaigns");

            migrationBuilder.DropForeignKey(
                name: "FK_content_calendar_workspaces_workspace_id",
                table: "content_calendar");

            migrationBuilder.DropForeignKey(
                name: "FK_contents_workspaces_workspace_id",
                table: "contents");

            migrationBuilder.DropForeignKey(
                name: "FK_conversations_workspaces_workspace_id",
                table: "conversations");

            migrationBuilder.DropForeignKey(
                name: "FK_notifications_workspaces_workspace_id",
                table: "notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_social_accounts_workspaces_workspace_id",
                table: "social_accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_social_integrations_workspaces_workspace_id",
                table: "social_integrations");

            migrationBuilder.DropIndex(
                name: "IX_social_integrations_workspace_id",
                table: "social_integrations");

            migrationBuilder.DropIndex(
                name: "IX_social_accounts_workspace_id",
                table: "social_accounts");

            migrationBuilder.DropIndex(
                name: "IX_notifications_workspace_id",
                table: "notifications");

            migrationBuilder.DropIndex(
                name: "IX_conversations_workspace_id",
                table: "conversations");

            migrationBuilder.DropIndex(
                name: "IX_contents_workspace_id",
                table: "contents");

            migrationBuilder.DropIndex(
                name: "IX_content_calendar_workspace_id",
                table: "content_calendar");

            migrationBuilder.DropIndex(
                name: "IX_ad_campaigns_workspace_id",
                table: "ad_campaigns");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "social_integrations");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "social_accounts");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "content_calendar");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "ad_campaigns");
        }
    }
}
