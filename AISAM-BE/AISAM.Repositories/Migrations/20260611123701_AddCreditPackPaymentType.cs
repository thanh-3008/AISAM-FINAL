using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditPackPaymentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "credit_amount",
                table: "payments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "credit_pack_code",
                table: "payments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "payment_type",
                table: "payments",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_payments_payment_type",
                table: "payments",
                column: "payment_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_payments_payment_type",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "credit_amount",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "credit_pack_code",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "payment_type",
                table: "payments");
        }
    }
}
