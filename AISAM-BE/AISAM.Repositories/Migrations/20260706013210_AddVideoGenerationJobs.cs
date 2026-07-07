using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoGenerationJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "video_generation_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_prompt = table.Column<string>(type: "text", nullable: false),
                    provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_fallback = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    external_job_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    segments_count = table.Column<int>(type: "integer", nullable: true),
                    current_segment = table.Column<int>(type: "integer", nullable: true),
                    video_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_video_generation_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_video_generation_jobs_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_video_generation_jobs_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_video_generation_jobs_is_fallback",
                table: "video_generation_jobs",
                column: "is_fallback");

            migrationBuilder.CreateIndex(
                name: "IX_video_generation_jobs_status",
                table: "video_generation_jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_video_generation_jobs_user_id",
                table: "video_generation_jobs",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_video_generation_jobs_workspace_id",
                table: "video_generation_jobs",
                column: "workspace_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "video_generation_jobs");
        }
    }
}
