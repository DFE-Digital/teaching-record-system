using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class BackfillQualificationIdAndAlertId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                update events set qualification_id = (payload #>> Array['MandatoryQualification', 'QualificationId'])::uuid
                where (payload #>> Array['MandatoryQualification', 'QualificationId'])::uuid is not null;

                update events set alert_id = (payload #>> Array['Alert', 'AlertId'])::uuid
                where (payload #>> Array['Alert', 'AlertId'])::uuid is not null;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
