using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentOS.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AiResultInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbox_messages",
                schema: "wolverine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_id = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    event_type = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_message_id",
                schema: "wolverine",
                table: "inbox_messages",
                column: "message_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox_messages",
                schema: "wolverine");
        }
    }
}
