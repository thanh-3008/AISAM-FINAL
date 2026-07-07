using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddAutomationPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "automation_plans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    source_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    timezone = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total_items = table.Column<int>(type: "integer", nullable: false),
                    valid_items = table.Column<int>(type: "integer", nullable: false),
                    failed_items = table.Column<int>(type: "integer", nullable: false),
                    estimated_credits = table.Column<int>(type: "integer", nullable: false),
                    reserved_credits = table.Column<int>(type: "integer", nullable: false),
                    used_credits = table.Column<int>(type: "integer", nullable: false),
                    released_credits = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_automation_plans", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "automation_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    automation_plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    row_index = table.Column<int>(type: "integer", nullable: false),
                    platform = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    brand_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content_id = table.Column<Guid>(type: "uuid", nullable: true),
                    topic = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    objective = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    requested_content_type = table.Column<int>(type: "integer", nullable: false),
                    tone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cta = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    scheduled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    estimated_credits = table.Column<int>(type: "integer", nullable: false),
                    used_credits = table.Column<int>(type: "integer", nullable: false),
                    validation_errors = table.Column<string>(type: "jsonb", nullable: true),
                    source_json = table.Column<string>(type: "jsonb", nullable: false),
                    generation_attempt_count = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_automation_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_automation_items_automation_plans_automation_plan_id",
                        column: x => x.automation_plan_id,
                        principalTable: "automation_plans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_automation_items_brands_brand_id",
                        column: x => x.brand_id,
                        principalTable: "brands",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_automation_items_contents_content_id",
                        column: x => x.content_id,
                        principalTable: "contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_automation_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_automation_items_automation_plan_id_row_index_platform",
                table: "automation_items",
                columns: new[] { "automation_plan_id", "row_index", "platform" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_automation_items_brand_id",
                table: "automation_items",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_items_content_id",
                table: "automation_items",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_items_idempotency_key",
                table: "automation_items",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_automation_items_product_id",
                table: "automation_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_items_scheduled_at",
                table: "automation_items",
                column: "scheduled_at");

            migrationBuilder.CreateIndex(
                name: "IX_automation_items_status",
                table: "automation_items",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_automation_plans_workspace_id",
                table: "automation_plans",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_automation_plans_workspace_id_created_at",
                table: "automation_plans",
                columns: new[] { "workspace_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "automation_items");

            migrationBuilder.DropTable(
                name: "automation_plans");
        }
    }
}
