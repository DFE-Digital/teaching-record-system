using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class IntegrationTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "integration_transactions",
                columns: table => new
                {
                    integration_transaction_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    interface_type_id = table.Column<int>(type: "integer", nullable: false),
                    import_status = table.Column<int>(type: "integer", nullable: false),
                    total_count = table.Column<int>(type: "integer", nullable: false),
                    success_count = table.Column<int>(type: "integer", nullable: false),
                    failure_count = table.Column<int>(type: "integer", nullable: false),
                    duplicate_count = table.Column<int>(type: "integer", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integration_transactions", x => x.integration_transaction_id);
                });

            migrationBuilder.CreateTable(
                name: "integration_transaction_records",
                columns: table => new
                {
                    integration_transaction_record_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    row_data = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    failure_message = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: true),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    duplicate = table.Column<bool>(type: "boolean", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    integration_transaction_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_integration_transaction_records", x => x.integration_transaction_record_id);
                    table.ForeignKey(
                        name: "fk_integration_transaction_records_persons_person_id",
                        column: x => x.person_id,
                        principalTable: "persons",
                        principalColumn: "person_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_integrationtransactionrecord_integrationtransaction",
                        column: x => x.integration_transaction_id,
                        principalTable: "integration_transactions",
                        principalColumn: "integration_transaction_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "integration_transaction_records");

            migrationBuilder.DropTable(
                name: "integration_transactions");
        }
    }
}
