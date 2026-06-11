using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspacePaymentSubscriptionOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_subscriptions_profiles_profile_id",
                table: "subscriptions");

            migrationBuilder.AlterColumn<Guid>(
                name: "profile_id",
                table: "subscriptions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "subscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_workspace_id",
                table: "subscriptions",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_workspace_id",
                table: "payments",
                column: "workspace_id");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_workspaces_workspace_id",
                table: "payments",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptions_profiles_profile_id",
                table: "subscriptions",
                column: "profile_id",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptions_workspaces_workspace_id",
                table: "subscriptions",
                column: "workspace_id",
                principalTable: "workspaces",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_workspaces_workspace_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_subscriptions_profiles_profile_id",
                table: "subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_subscriptions_workspaces_workspace_id",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_subscriptions_workspace_id",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_payments_workspace_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "payments");

            migrationBuilder.AlterColumn<Guid>(
                name: "profile_id",
                table: "subscriptions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_subscriptions_profiles_profile_id",
                table: "subscriptions",
                column: "profile_id",
                principalTable: "profiles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
