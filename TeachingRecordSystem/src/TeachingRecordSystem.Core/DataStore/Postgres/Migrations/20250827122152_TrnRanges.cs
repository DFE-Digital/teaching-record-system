using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TrnRanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trn_ranges",
                columns: table => new
                {
                    from_trn = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    to_trn = table.Column<int>(type: "integer", nullable: false),
                    next_trn = table.Column<int>(type: "integer", nullable: false),
                    is_exhausted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trn_ranges", x => x.from_trn);
                });

            migrationBuilder.CreateIndex(
                name: "ix_trn_ranges_unexhausted_trn_ranges",
                table: "trn_ranges",
                column: "from_trn",
                filter: "is_exhausted IS FALSE");

            migrationBuilder.Procedure("fn_generate_trn_v1.sql");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var dropFunctionSql = @"
DROP FUNCTION fn_generate_trn()
";
            migrationBuilder.Sql(dropFunctionSql);

            migrationBuilder.DropTable(
                name: "trn_ranges");
        }
    }
}
