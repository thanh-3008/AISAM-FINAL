using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class FixAdminPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE users
                SET password_hash = 'UerdQ3VtiYZ4QTm6hRz+eOmid9LnWaURY30Rxe7vVwDQT07ZVvPYFNfFc86F00bEMnxuaZ6wO9hNxLuiLWvVag==',
                    password_salt = '0YzN6SLaBxlvEmaum9P7ct2gISTgBFv+Iyc8zutGzQKn0lbvJi9D0oH39mwVloTQ0R94qhCKVaarTgAz302y0rlUGrc3A1Q//Q2VEsbQ8I1//pbbWClzhaNQ5rO9bes/uJJ/zX66xrlGfTPaAJJFZByiSXnj5x6XVBA4heUJkJY='
                WHERE email = 'admin@aisam.com';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No down migration needed — original hash was already incorrect
        }
    }
}
