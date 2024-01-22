using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TrnRequestsClientId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                update trn_requests set client_id = case
                    when client_id = 'register' then (select user_id::varchar from users where name = 'Register trainee teachers' and user_type = 2)
                    when client_id = 'apply-for-qts' then (select user_id::varchar from users where name = 'Apply for qualified teacher status (QTS) in England' and user_type = 2)
                    else client_id end
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
