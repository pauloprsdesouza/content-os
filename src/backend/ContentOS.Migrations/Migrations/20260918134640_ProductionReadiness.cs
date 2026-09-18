using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentOS.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ProductionReadiness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wolverine");

            migrationBuilder.AddColumn<Guid>(
                name: "origin_research_finding_id",
                schema: "knowledge",
                table: "claims",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "wolverine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    envelope_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    type = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    subject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    last_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_claims_origin_research_finding_id",
                schema: "knowledge",
                table: "claims",
                column: "origin_research_finding_id",
                unique: true,
                filter: "origin_research_finding_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_envelope_id",
                schema: "wolverine",
                table: "outbox_messages",
                column: "envelope_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_sent_at",
                schema: "wolverine",
                table: "outbox_messages",
                column: "sent_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "wolverine");

            migrationBuilder.DropIndex(
                name: "IX_claims_origin_research_finding_id",
                schema: "knowledge",
                table: "claims");

            migrationBuilder.DropColumn(
                name: "origin_research_finding_id",
                schema: "knowledge",
                table: "claims");
        }
    }
}
