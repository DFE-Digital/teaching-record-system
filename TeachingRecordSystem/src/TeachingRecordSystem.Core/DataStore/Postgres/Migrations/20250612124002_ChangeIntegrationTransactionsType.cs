using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIntegrationTransactionsType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_integrationtransactionrecord_integrationtransaction",
                table: "integration_transaction_records");

            migrationBuilder.RenameColumn(
                name: "interface_type_id",
                table: "integration_transactions",
                newName: "interface_type");

            migrationBuilder.AlterColumn<long>(
                name: "integration_transaction_id",
                table: "integration_transaction_records",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "fk_integrationtransactionrecord_integrationtransaction",
                table: "integration_transaction_records",
                column: "integration_transaction_id",
                principalTable: "integration_transactions",
                principalColumn: "integration_transaction_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_integrationtransactionrecord_integrationtransaction",
                table: "integration_transaction_records");

            migrationBuilder.RenameColumn(
                name: "interface_type",
                table: "integration_transactions",
                newName: "interface_type_id");

            migrationBuilder.AlterColumn<long>(
                name: "integration_transaction_id",
                table: "integration_transaction_records",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_integrationtransactionrecord_integrationtransaction",
                table: "integration_transaction_records",
                column: "integration_transaction_id",
                principalTable: "integration_transactions",
                principalColumn: "integration_transaction_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
