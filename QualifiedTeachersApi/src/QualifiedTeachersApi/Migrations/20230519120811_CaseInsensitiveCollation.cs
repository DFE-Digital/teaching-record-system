using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QualifiedTeachersApi.Migrations
{
    /// <inheritdoc />
    public partial class CaseInsensitiveCollation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("create collation case_insensitive (provider = icu, locale = 'und-u-ks-level2', deterministic = false);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("drop collation case_insensitive;");
        }
    }
}
