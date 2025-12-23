using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DummyTable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dummy_entity1",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dummy_entity1", x => x.id);
                });

            migrationBuilder.InsertData(
                table: "dummy_entity1",
                column: "id",
                values: new object[]
                {
                    new Guid("792f2e84-a0ef-4761-b8d2-a750f81febc7"),
                    new Guid("965ce0f1-d475-4048-8449-67cdfd547ed2")
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dummy_entity1");
        }
    }
}
