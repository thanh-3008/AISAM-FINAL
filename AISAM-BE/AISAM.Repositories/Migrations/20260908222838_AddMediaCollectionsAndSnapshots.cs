using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AISAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaCollectionsAndSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "snapshot_id",
                table: "posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "approved_snapshot_id",
                table: "contents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "media_version",
                table: "contents",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "submitted_snapshot_id",
                table: "contents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "snapshot_id",
                table: "content_calendar",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "brand_id",
                table: "assets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "expired_at",
                table: "assets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "provider_public_id",
                table: "assets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sha256",
                table: "assets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "storage_deleted_at",
                table: "assets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "workspace_id",
                table: "assets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "snapshot_id",
                table: "approvals",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "content_media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_cover = table.Column<bool>(type: "boolean", nullable: false),
                    alt_text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    caption = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_media", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_media_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_content_media_contents_content_id",
                        column: x => x.content_id,
                        principalTable: "contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "publish_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workspace_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<Guid>(type: "uuid", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    checksum = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_publish_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_publish_snapshots_contents_content_id",
                        column: x => x.content_id,
                        principalTable: "contents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "snapshot_media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    mime_type = table.Column<string>(type: "text", nullable: true),
                    is_cover = table.Column<bool>(type: "boolean", nullable: false),
                    alt_text = table.Column<string>(type: "text", nullable: true),
                    caption = table.Column<string>(type: "text", nullable: true),
                    size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    checksum = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_snapshot_media", x => x.id);
                    table.ForeignKey(
                        name: "FK_snapshot_media_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_snapshot_media_publish_snapshots_snapshot_id",
                        column: x => x.snapshot_id,
                        principalTable: "publish_snapshots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "post_media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    snapshot_media_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_media_id = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    error_code = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_post_media", x => x.id);
                    table.ForeignKey(
                        name: "FK_post_media_posts_post_id",
                        column: x => x.post_id,
                        principalTable: "posts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_post_media_snapshot_media_snapshot_media_id",
                        column: x => x.snapshot_media_id,
                        principalTable: "snapshot_media",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assets_workspace_id_brand_id_created_at",
                table: "assets",
                columns: new[] { "workspace_id", "brand_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_content_media_asset_id",
                table: "content_media",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_content_media_content_id_asset_id",
                table: "content_media",
                columns: new[] { "content_id", "asset_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_media_content_id_sort_order",
                table: "content_media",
                columns: new[] { "content_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_post_media_post_id_snapshot_media_id",
                table: "post_media",
                columns: new[] { "post_id", "snapshot_media_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_post_media_snapshot_media_id",
                table: "post_media",
                column: "snapshot_media_id");

            migrationBuilder.CreateIndex(
                name: "IX_publish_snapshots_content_id",
                table: "publish_snapshots",
                column: "content_id");

            migrationBuilder.CreateIndex(
                name: "IX_snapshot_media_asset_id",
                table: "snapshot_media",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_snapshot_media_snapshot_id_sort_order",
                table: "snapshot_media",
                columns: new[] { "snapshot_id", "sort_order" },
                unique: true);
            migrationBuilder.Sql("""
                CREATE FUNCTION aisam_snapshot_immutable() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN RAISE EXCEPTION 'Publish snapshot is immutable' USING ERRCODE='23514'; END $$;
                CREATE TRIGGER snapshot_immutable BEFORE UPDATE OR DELETE ON publish_snapshots FOR EACH ROW EXECUTE FUNCTION aisam_snapshot_immutable();
                CREATE TRIGGER snapshot_media_immutable BEFORE UPDATE OR DELETE ON snapshot_media FOR EACH ROW EXECUTE FUNCTION aisam_snapshot_immutable();
                CREATE FUNCTION aisam_media_asset_guard() RETURNS trigger LANGUAGE plpgsql AS $$
                DECLARE a assets%ROWTYPE; c contents%ROWTYPE;
                BEGIN
                  IF NEW.asset_id IS NOT NULL THEN
                    SELECT * INTO a FROM assets WHERE id=NEW.asset_id FOR UPDATE;
                    IF NOT FOUND OR a.expired_at IS NOT NULL THEN RAISE EXCEPTION 'Asset unavailable' USING ERRCODE='23514'; END IF;
                    IF TG_TABLE_NAME='content_media' THEN
                      SELECT * INTO c FROM contents WHERE id=NEW.content_id;
                      IF a.workspace_id IS DISTINCT FROM c.workspace_id OR a.brand_id IS DISTINCT FROM c.brand_id THEN RAISE EXCEPTION 'Asset outside content scope' USING ERRCODE='23514'; END IF;
                    END IF;
                  END IF;
                  RETURN NEW;
                END $$;
                CREATE TRIGGER media_asset_guard BEFORE INSERT OR UPDATE ON content_media FOR EACH ROW EXECUTE FUNCTION aisam_media_asset_guard();
                CREATE TRIGGER frozen_asset_guard BEFORE INSERT ON snapshot_media FOR EACH ROW EXECUTE FUNCTION aisam_media_asset_guard();
                CREATE FUNCTION aisam_asset_expiry_guard() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN
                  IF NEW.storage_path IS DISTINCT FROM OLD.storage_path OR NEW.workspace_id IS DISTINCT FROM OLD.workspace_id OR NEW.brand_id IS DISTINCT FROM OLD.brand_id OR NEW.uploaded_by IS DISTINCT FROM OLD.uploaded_by THEN
                    RAISE EXCEPTION 'Asset identity is immutable' USING ERRCODE='23514';
                  END IF;
                  IF OLD.expired_at IS NULL AND NEW.expired_at IS NOT NULL AND (
                    EXISTS(SELECT 1 FROM content_media WHERE asset_id=NEW.id) OR
                    EXISTS(SELECT 1 FROM snapshot_media WHERE asset_id=NEW.id OR url=NEW.storage_path) OR
                    EXISTS(SELECT 1 FROM contents WHERE strpos(COALESCE(image_url::text,''),NEW.storage_path)>0 OR video_url=NEW.storage_path)
                  ) THEN RAISE EXCEPTION 'Asset is referenced' USING ERRCODE='23514'; END IF;
                  RETURN NEW;
                END $$;
                CREATE TRIGGER asset_expiry_guard BEFORE UPDATE ON assets FOR EACH ROW EXECUTE FUNCTION aisam_asset_expiry_guard();
                CREATE FUNCTION aisam_legacy_asset_guard() RETURNS trigger LANGUAGE plpgsql AS $$
                DECLARE a assets%ROWTYPE;
                BEGIN
                  FOR a IN SELECT * FROM assets WHERE strpos(COALESCE(NEW.image_url::text,''),storage_path)>0 OR NEW.video_url=storage_path FOR UPDATE LOOP
                    IF a.expired_at IS NOT NULL THEN RAISE EXCEPTION 'Asset expired' USING ERRCODE='23514'; END IF;
                  END LOOP;
                  RETURN NEW;
                END $$;
                CREATE TRIGGER legacy_asset_guard BEFORE INSERT OR UPDATE OF image_url,video_url ON contents FOR EACH ROW EXECUTE FUNCTION aisam_legacy_asset_guard();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TRIGGER legacy_asset_guard ON contents;
                DROP TRIGGER asset_expiry_guard ON assets;
                DROP TRIGGER media_asset_guard ON content_media;
                DROP TRIGGER frozen_asset_guard ON snapshot_media;
                DROP TRIGGER snapshot_immutable ON publish_snapshots;
                DROP TRIGGER snapshot_media_immutable ON snapshot_media;
                DROP FUNCTION aisam_legacy_asset_guard();
                DROP FUNCTION aisam_asset_expiry_guard();
                DROP FUNCTION aisam_media_asset_guard();
                DROP FUNCTION aisam_snapshot_immutable();
                """);

            migrationBuilder.DropTable(
                name: "content_media");

            migrationBuilder.DropTable(
                name: "post_media");

            migrationBuilder.DropTable(
                name: "snapshot_media");

            migrationBuilder.DropTable(
                name: "publish_snapshots");

            migrationBuilder.DropIndex(
                name: "IX_assets_workspace_id_brand_id_created_at",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "snapshot_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "approved_snapshot_id",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "media_version",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "submitted_snapshot_id",
                table: "contents");

            migrationBuilder.DropColumn(
                name: "snapshot_id",
                table: "content_calendar");

            migrationBuilder.DropColumn(
                name: "brand_id",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "expired_at",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "provider_public_id",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "sha256",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "storage_deleted_at",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "workspace_id",
                table: "assets");

            migrationBuilder.DropColumn(
                name: "snapshot_id",
                table: "approvals");
        }
    }
}
