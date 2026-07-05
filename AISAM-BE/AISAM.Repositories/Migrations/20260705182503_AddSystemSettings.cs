using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    value = table.Column<string>(type: "jsonb", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_system_settings_users_updated_by",
                        column: x => x.updated_by,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_system_settings_key",
                table: "system_settings",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_system_settings_updated_by",
                table: "system_settings",
                column: "updated_by");

            migrationBuilder.Sql(@"
                INSERT INTO users (id, email, full_name, role, is_email_verified, password_hash, password_salt, created_at)
                SELECT gen_random_uuid(), 'admin@aisam.com', 'Super Admin', 2, true, 'ezbsYCnaHQFB3i2hTxLMyriAWmWFpfljIiYjz6bTjInYp/tbJd+5yX6UYEpHBDIoDPl6PZQSKFd+0iN5LCmipA==', 'ogj9QceE0qO+BFbltp3UHXSIDc56ZyL+YGuDXWIrMISPmhjiqrkE6SKdqgGXTGQLl2jVfLAmILxIlhGbesgl1F1Og7dVJ1RjjIVrmdWSey8/c39agLKPJ/UGIEYliPs+fSCD3NS3OyATO/rB6EVNwOzkUyWnTzgmKhUxR/CnN2E=', NOW()
                WHERE NOT EXISTS (SELECT 1 FROM users WHERE email = 'admin@aisam.com');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "system_settings");
        }
    }
}
