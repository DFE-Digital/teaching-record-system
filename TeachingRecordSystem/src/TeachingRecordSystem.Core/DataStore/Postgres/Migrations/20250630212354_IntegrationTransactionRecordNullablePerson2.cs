using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class IntegrationTransactionRecordNullablePerson2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_integration_transaction_records_persons_person_id",
                table: "integration_transaction_records");

            migrationBuilder.AlterColumn<Guid>(
                name: "person_id",
                table: "integration_transaction_records",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "fk_integration_transaction_records_persons_person_id",
                table: "integration_transaction_records",
                column: "person_id",
                principalTable: "persons",
                principalColumn: "person_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_integration_transaction_records_persons_person_id",
                table: "integration_transaction_records");

            migrationBuilder.AlterColumn<Guid>(
                name: "person_id",
                table: "integration_transaction_records",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_integration_transaction_records_persons_person_id",
                table: "integration_transaction_records",
                column: "person_id",
                principalTable: "persons",
                principalColumn: "person_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
