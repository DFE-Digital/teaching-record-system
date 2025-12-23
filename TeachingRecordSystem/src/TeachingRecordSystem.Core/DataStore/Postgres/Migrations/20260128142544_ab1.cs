using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class ab1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "dummy_entity1",
                keyColumn: "id",
                keyValue: new Guid("01811206-405c-4f1f-9c57-5cc1284119e5"));

            migrationBuilder.DeleteData(
                table: "dummy_entity1",
                keyColumn: "id",
                keyValue: new Guid("9efba67e-9e02-4a0c-addd-09935557a6bc"));

            migrationBuilder.InsertData(
                table: "dummy_entity1",
                column: "id",
                values: new object[]
                {
                    new Guid("7cf03897-a364-46e0-b9ff-68fb61d94325"),
                    new Guid("becf1725-bffe-4933-90a1-fdfe4ec3a26c")
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "dummy_entity1",
                keyColumn: "id",
                keyValue: new Guid("7cf03897-a364-46e0-b9ff-68fb61d94325"));

            migrationBuilder.DeleteData(
                table: "dummy_entity1",
                keyColumn: "id",
                keyValue: new Guid("becf1725-bffe-4933-90a1-fdfe4ec3a26c"));

            migrationBuilder.InsertData(
                table: "dummy_entity1",
                column: "id",
                values: new object[]
                {
                    new Guid("01811206-405c-4f1f-9c57-5cc1284119e5"),
                    new Guid("9efba67e-9e02-4a0c-addd-09935557a6bc")
                });
        }
    }
}
