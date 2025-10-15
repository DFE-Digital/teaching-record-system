using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessToPublication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER PUBLICATION dqt_rep_sync ADD TABLE processes, process_events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER PUBLICATION dqt_rep_sync DROP TABLE processes, process_events");
        }
    }
}
