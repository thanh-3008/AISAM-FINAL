using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AISAM.Repositories.Migrations;

[DbContext(typeof(AisamContext))]
[Migration("20260919160000_AddDeviceTokens")]
public sealed class AddDeviceTokens : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "device_tokens",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                platform = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                device_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                last_active_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_device_tokens", x => x.id);
                table.ForeignKey(
                    name: "FK_device_tokens_profiles_profile_id",
                    column: x => x.profile_id,
                    principalTable: "profiles",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_device_tokens_profile_id_is_active",
            table: "device_tokens",
            columns: new[] { "profile_id", "is_active" });

        migrationBuilder.CreateIndex(
            name: "IX_device_tokens_token",
            table: "device_tokens",
            column: "token",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "device_tokens");
    }
}
