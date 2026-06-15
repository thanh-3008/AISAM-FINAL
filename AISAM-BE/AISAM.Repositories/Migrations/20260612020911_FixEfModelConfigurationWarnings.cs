using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class FixEfModelConfigurationWarnings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_profiles_subscription_id",
                table: "profiles");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_subscription_id",
                table: "profiles",
                column: "subscription_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_profiles_subscription_id",
                table: "profiles");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_subscription_id",
                table: "profiles",
                column: "subscription_id");
        }
    }
}
