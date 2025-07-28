using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class FixTrnRequestMetadataGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                update trn_request_metadata set gender = 3 where gender = 389040000;  --Other
                update trn_request_metadata set gender = null where gender = 389040001;  --Not provided
                update trn_request_metadata set gender = 4 where gender = 389040002;  --Not available
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
