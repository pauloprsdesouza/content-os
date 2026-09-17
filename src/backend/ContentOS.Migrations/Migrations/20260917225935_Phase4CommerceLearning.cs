using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentOS.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Phase4CommerceLearning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "learning");

            migrationBuilder.EnsureSchema(
                name: "commerce");

            migrationBuilder.CreateTable(
                name: "capstones",
                schema: "learning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_capstones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "enrollments",
                schema: "learning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    learner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    edition_id = table.Column<Guid>(type: "uuid", nullable: true),
                    purchase_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrolled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enrollments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "evaluations",
                schema: "learning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    capstone_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Passed = table.Column<bool>(type: "boolean", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "learners",
                schema: "learning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    display_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_learners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outcomes",
                schema: "learning",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    learner_id = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    capstone_id = table.Column<Guid>(type: "uuid", nullable: true),
                    Passed = table.Column<bool>(type: "boolean", nullable: false),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outcomes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "purchases",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    external_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    webhook_inbox_entry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reconciliation_run_id = table.Column<Guid>(type: "uuid", nullable: true),
                    buyer_email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    edition_id = table.Column<Guid>(type: "uuid", nullable: true),
                    learner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    enrollment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    confirmed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reconciliation_runs",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    processed_count = table.Column<int>(type: "integer", nullable: false),
                    confirmed_count = table.Column<int>(type: "integer", nullable: false),
                    ignored_count = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reconciliation_runs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "webhook_inbox",
                schema: "commerce",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    external_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    payload_json = table.Column<string>(type: "text", nullable: false),
                    signature_verified = table.Column<bool>(type: "boolean", nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_webhook_inbox", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_capstones_enrollment_id",
                schema: "learning",
                table: "capstones",
                column: "enrollment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_learner_product",
                schema: "learning",
                table: "enrollments",
                columns: new[] { "learner_id", "product_id" });

            migrationBuilder.CreateIndex(
                name: "ux_enrollments_purchase_id",
                schema: "learning",
                table: "enrollments",
                column: "purchase_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_evaluations_capstone_id",
                schema: "learning",
                table: "evaluations",
                column: "capstone_id");

            migrationBuilder.CreateIndex(
                name: "ux_learners_email",
                schema: "learning",
                table: "learners",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outcomes_enrollment_id",
                schema: "learning",
                table: "outcomes",
                column: "enrollment_id");

            migrationBuilder.CreateIndex(
                name: "ix_purchases_status_created_at",
                schema: "commerce",
                table: "purchases",
                columns: new[] { "Status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_purchases_provider_external_id",
                schema: "commerce",
                table: "purchases",
                columns: new[] { "Provider", "external_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ux_webhook_inbox_provider_external_id",
                schema: "commerce",
                table: "webhook_inbox",
                columns: new[] { "Provider", "external_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "capstones",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "enrollments",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "evaluations",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "learners",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "outcomes",
                schema: "learning");

            migrationBuilder.DropTable(
                name: "purchases",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "reconciliation_runs",
                schema: "commerce");

            migrationBuilder.DropTable(
                name: "webhook_inbox",
                schema: "commerce");
        }
    }
}
