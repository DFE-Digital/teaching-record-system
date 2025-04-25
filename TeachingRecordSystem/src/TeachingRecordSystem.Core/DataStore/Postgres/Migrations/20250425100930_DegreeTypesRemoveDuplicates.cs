using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class DegreeTypesRemoveDuplicates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "degree_types",
                keyColumn: "degree_type_id",
                keyValue: new Guid("78a8d033-06c8-4beb-b415-5907f5f39207"));

            migrationBuilder.DeleteData(
                table: "degree_types",
                keyColumn: "degree_type_id",
                keyValue: new Guid("ae28704f-cfa3-4c6e-a47d-c4a048383018"));

            migrationBuilder.DeleteData(
                table: "degree_types",
                keyColumn: "degree_type_id",
                keyValue: new Guid("e0b22ab0-fa25-4c31-aa61-cab56a4e6a2b"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "degree_types",
                columns: new[] { "degree_type_id", "is_active", "name" },
                values: new object[,]
                {
                    { new Guid("78a8d033-06c8-4beb-b415-5907f5f39207"), true, "Postgraduate Certificate in Education" },
                    { new Guid("ae28704f-cfa3-4c6e-a47d-c4a048383018"), true, "Professional PGCE" },
                    { new Guid("e0b22ab0-fa25-4c31-aa61-cab56a4e6a2b"), true, "PGCE" }
                });
        }
    }
}
