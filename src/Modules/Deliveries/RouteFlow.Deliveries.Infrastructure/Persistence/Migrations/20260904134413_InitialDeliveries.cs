using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RouteFlow.Deliveries.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialDeliveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "deliveries");

            migrationBuilder.CreateTable(
                name: "deliveries",
                schema: "deliveries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    current_custody = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    merchant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_driver_id = table.Column<Guid>(type: "uuid", nullable: true),
                    required_vehicle_type = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    address_street = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    address_number = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    address_complement = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    address_neighborhood = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address_state = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    address_zip_code = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    package_weight_kg = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    package_length_cm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    package_width_cm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    package_height_cm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    package_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deliveries", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "delivery_attempts",
                schema: "deliveries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    failure_category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    failure_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    delivery_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_attempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_delivery_attempts_deliveries_delivery_id",
                        column: x => x.delivery_id,
                        principalSchema: "deliveries",
                        principalTable: "deliveries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_attempts_delivery_id_attempt_number",
                schema: "deliveries",
                table: "delivery_attempts",
                columns: new[] { "delivery_id", "attempt_number" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "delivery_attempts",
                schema: "deliveries");

            migrationBuilder.DropTable(
                name: "deliveries",
                schema: "deliveries");
        }
    }
}
