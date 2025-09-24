using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SplitNamesSynonyms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop function fn_split_names(varchar[])");

            migrationBuilder.Procedure("fn_split_names_v2.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop function fn_split_names(varchar[], bool)");

            migrationBuilder.Procedure("fn_split_names_v1.sql");
        }
    }
}
