using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentOS.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Phase3CatalogPublication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.EnsureSchema(
                name: "publication");

            migrationBuilder.CreateTable(
                name: "editions",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_editions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "products",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "publication_packages",
                schema: "publication",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    edition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    renderer_version = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    manifest_json = table.Column<string>(type: "text", nullable: true),
                    export_blob_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    export_blob_length = table.Column<long>(type: "bigint", nullable: true),
                    confirmed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_publication_packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "curriculum_items",
                schema: "catalog",
                columns: table => new
                {
                    edition_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    content_version_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curriculum_items", x => new { x.edition_id, x.Position });
                    table.ForeignKey(
                        name: "FK_curriculum_items_editions_edition_id",
                        column: x => x.edition_id,
                        principalSchema: "catalog",
                        principalTable: "editions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_curriculum_items_edition_content_version",
                schema: "catalog",
                table: "curriculum_items",
                columns: new[] { "edition_id", "content_version_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_editions_product_name",
                schema: "catalog",
                table: "editions",
                columns: new[] { "product_id", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_name",
                schema: "catalog",
                table: "products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "ix_publication_packages_edition_created_at",
                schema: "publication",
                table: "publication_packages",
                columns: new[] { "edition_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_publication_packages_status_updated_at",
                schema: "publication",
                table: "publication_packages",
                columns: new[] { "Status", "updated_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "curriculum_items",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "products",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "publication_packages",
                schema: "publication");

            migrationBuilder.DropTable(
                name: "editions",
                schema: "catalog");
        }
    }
}
