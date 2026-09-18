using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentOS.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EditorialPipeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "citation_content_hashes",
                schema: "content",
                table: "content_units",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "format",
                schema: "content",
                table: "content_units",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "artigo");

            migrationBuilder.AddColumn<Guid>(
                name: "topic_discovery_id",
                schema: "content",
                table: "content_units",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "discovered_works",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    discovery_id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    publication_year = table.Column<int>(type: "integer", nullable: true),
                    topic_id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    topic_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    abstract_text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discovered_works", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "editorial_series",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Format = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    area_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    area_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    area_is_subfield = table.Column<bool>(type: "boolean", nullable: false),
                    window_days = table.Column<int>(type: "integer", nullable: false),
                    Cadence = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    next_collection_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_editorial_series", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "topic_discoveries",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Format = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    area_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    area_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    area_is_subfield = table.Column<bool>(type: "boolean", nullable: false),
                    window_days = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    owner_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    series_id = table.Column<Guid>(type: "uuid", nullable: true),
                    scheduled_for = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    operation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content_unit_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topic_discoveries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "topic_proposals",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    discovery_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Rationale = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    work_ids = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    is_selected = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topic_proposals", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_discovered_works_discovery_id",
                schema: "content",
                table: "discovered_works",
                column: "discovery_id");

            migrationBuilder.CreateIndex(
                name: "IX_editorial_series_owner_user_id",
                schema: "content",
                table: "editorial_series",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_topic_discoveries_owner_user_id_updated_at",
                schema: "content",
                table: "topic_discoveries",
                columns: new[] { "owner_user_id", "updated_at" });

            migrationBuilder.CreateIndex(
                name: "IX_topic_discoveries_series_id_scheduled_for",
                schema: "content",
                table: "topic_discoveries",
                columns: new[] { "series_id", "scheduled_for" },
                unique: true,
                filter: "series_id IS NOT NULL AND scheduled_for IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_topic_proposals_discovery_id",
                schema: "content",
                table: "topic_proposals",
                column: "discovery_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discovered_works",
                schema: "content");

            migrationBuilder.DropTable(
                name: "editorial_series",
                schema: "content");

            migrationBuilder.DropTable(
                name: "topic_discoveries",
                schema: "content");

            migrationBuilder.DropTable(
                name: "topic_proposals",
                schema: "content");

            migrationBuilder.DropColumn(
                name: "citation_content_hashes",
                schema: "content",
                table: "content_units");

            migrationBuilder.DropColumn(
                name: "format",
                schema: "content",
                table: "content_units");

            migrationBuilder.DropColumn(
                name: "topic_discovery_id",
                schema: "content",
                table: "content_units");
        }
    }
}
