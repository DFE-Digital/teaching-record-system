using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class TpsEstablishments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_establishment_urn",
                table: "establishments");

            migrationBuilder.CreateTable(
                name: "tps_establishment_types",
                columns: table => new
                {
                    tps_establishment_type_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    establishment_range_from = table.Column<string>(type: "character(4)", fixedLength: true, maxLength: 4, nullable: false),
                    establishment_range_to = table.Column<string>(type: "character(4)", fixedLength: true, maxLength: 4, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    short_description = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tps_establishment_types", x => x.tps_establishment_type_id);
                });

            migrationBuilder.CreateTable(
                name: "tps_establishments",
                columns: table => new
                {
                    tps_establishment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    la_code = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false),
                    establishment_code = table.Column<string>(type: "character(4)", fixedLength: true, maxLength: 4, nullable: false),
                    employers_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    school_gias_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    school_closed_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tps_establishments", x => x.tps_establishment_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_establishment_urn",
                table: "establishments",
                column: "urn");

            migrationBuilder.CreateIndex(
                name: "ix_tps_establishments_la_code_establishment_number",
                table: "tps_establishments",
                columns: new[] { "la_code", "establishment_code" });

            migrationBuilder.InsertData(
                table: "tps_establishment_types",
                columns: new[] { "tps_establishment_type_id", "establishment_range_from", "establishment_range_to", "description", "short_description" },
                values: new object[,]
                {
                    { 1, "0001", "0099", "Homes set up under the Children and Young Persons Act (e.g. Community Homes)", "Homes set up under the Children and Young Persons Act (e.g. Community Homes)" },
                    { 2, "0200", "0399", "Homes set up under the Children and Young Persons Act (e.g. Community Homes)", "Homes set up under the Children and Young Persons Act (e.g. Community Homes)" },
                    { 3, "0400", "0524", "Training and occupation centres and other DSS establishments (except day nurseries)", "Training and occupation centres and other DSS establishments (except day nurseries)" },
                    { 4, "0525", "0549", "Special Hospitals provided under Part VII of the Mental Health Act 1959", "Special Hospitals provided under Part VII of the Mental Health Act 1959" },
                    { 5, "0550", "0574", "Teachers' Superannuation (Army Civilian Lecturer) Scheme 1951 Schools or schools formerly under that Scheme", "Teachers' Superannuation (Army Civilian Lecturer) Scheme 1951 Schools or schools formerly under that Scheme" },
                    { 6, "0575", "0599", "Education Forum", "Education Forum"},
                    { 7, "0600", "0600", "CAY", "CAY" },
                    { 8, "0601", "0601", "PAY", "PAY"},
                    { 9, "0625", "0625", "DSS day nurseries", "DSS day nurseries" },
                    { 10, "0626", "0674", "Schools and institutions controlled by other government departments", "Schools and institutions controlled by other government departments" },
                    { 11, "0675", "0750", "Employment under voluntary youth organisations", "Employment under voluntary youth organisations" },
                    { 12, "0100", "0199", "Employment under voluntary youth organisations", "Employment under voluntary youth organisations" },
                    { 13, "0751", "0939", "Employment under adult and miscellaneous organisations", "Employment under adult and miscellaneous organisations" },
                    { 14, "0940", "0949", "Playing for Success Centres", "Playing for Success Centres" },
                    { 15, "1000", "1099", "LA nursery schools", "LA nursery schools" },
                    { 16, "1100", "1150", "Pupil referral units", "Pupil referral units" },
                    { 75, "1151", "1799", "LA nursery schools", "LA nursery schools" },
                    { 17, "1800", "1899", "Direct-grant nursery schools", "Direct-grant nursery schools" },
                    { 18, "1900", "1999", "Independent nursery education establishment recognised as efficient", "Independent nursery education establishment recognised as efficient" },
                    { 19, "2000", "3999", "Maintained primary schools and schools which have converted to academies", "Maintained primary schools and schools which have converted to academies" },
                    { 20, "4000", "4999", "Maintained secondary schools and schools which have converted to academies", "Maintained secondary schools and schools which have converted to academies" },
                    { 21, "5000", "5099", "Direct-grant schools (recorded up to October 1980)", "Direct-grant schools (recorded up to October 1980)" },
                    { 22, "5100", "5198", "Practical instruction centres (not all such centres have been allocated individual numbers but where a number has already been allocated its use is continued. All new centres are numbered 5199)", "Practical instruction centres" },
                    { 23, "5200", "5299", "Grant-maintained primary/middle deemed primary schools and schools which have converted to academies" , "Grant-maintained primary/middle deemed primary schools and schools which have converted to academies" },
                    { 24, "5300", "5399", "Camps, holiday classes etc.", "Camps, holiday classes etc." },
                    { 25, "5400", "5499", "Grant-maintained secondary/middle deemed secondary schools and schools which have converted to academies", "Grant-maintained secondary/middle deemed secondary schools and schools which have converted to academies" },
                    { 26, "5500", "5548", "Immigrant centres", "Immigrant centres" },
                    { 27, "5601", "5899", "Grant-maintained primary, middle and secondary schools (overflow) schools which have converted to academies", "Grant-maintained primary, middle and secondary schools (overflow) schools which have converted to academies"},
                    { 28, "5900", "5949", "Grant-maintained schools (formally Independent) schools which have converted to academies", "Grant-maintained schools (formally Independent) schools which have converted to academies" },
                    { 29, "5950", "5999", "Grant-maintained special schools and schools which have converted to academies", "Grant-maintained special schools and schools which have converted to academies" },
                    { 30, "6000", "6899", "Independent schools", "Independent schools" },
                    { 31, "6900", "6904", "City technology colleges", "City technology colleges" },
                    { 32, "6905", "6924", "City Academies", "City Academies" },
                    { 33, "7000", "7749", "Special schools (except as below)" , "Special schools" },
                    { 34, "7750", "7798", "Special schools for nursery age children" , "Special schools for nursery age children" },
                    { 35, "7800", "7899", "Boarding homes for handicapped pupils", "Boarding homes for handicapped pupils" },
                    { 36, "7900", "7999", "Establishments for further education and training of disabled persons", "Establishments for further education and training of disabled persons" },
                    { 37, "8000", "8149", "Maintained and assisted major FE establishments (not included below)", "Maintained and assisted major FE establishments" },
                    { 38, "8150", "8199", "Maintained and assisted art establishments", "Maintained and assisted art establishments"},
                    { 39, "8200", "8219", "Direct-grant major FE establishments", "Direct-grant major FE establishments" },
                    { 40, "8220", "8269", "Independent (Efficient-Rules 16) FE establishments", "Independent (Efficient-Rules 16) FE establishments" },
                    { 41, "8270", "8284", "National colleges", "National colleges"},
                    { 42, "8300", "8349", "LA farm institutes", "LA farm institutes"},
                    { 43, "8350", "8389", "LA agricultural centres", "LA agricultural centres"},
                    { 44, "8390", "8399", "Direct-grant and independent agricultural establishments", "Direct-grant and independent agricultural establishments" },
                    { 45, "8400", "8499", "LA youth welfare", "LA youth welfare" },
                    { 46, "8500", "8599", "LA adult welfare", "LA adult welfare" },
                    { 47, "8600", "8699", "Sixth form colleges", "Sixth form colleges" },
                    { 48, "8700", "8898", "Polytechnics/New Style Universities", "Polytechnics/New Style Universities" },
                    { 49, "9300", "9599", "LA colleges of education", "LA colleges of education" },
                    { 50, "9600", "9899", "Voluntary colleges of education", "Voluntary colleges of education" },
                    { 51, "0950", "0950", "Teacher /organiser (employed primarily as a teacher)", "Teacher /organiser (employed primarily as a teacher)" },
                    { 52, "0951", "0951", "Divided service between Primary and Secondary Schools" , "Divided service between Primary and Secondary Schools" },
                    { 53, "0952", "0952", "Divided service between Further Education and P & S Schools", "Divided service between Further Education and P & S Schools" },
                    { 54, "0954", "0954", "Adult Miscellaneous Organisation", "Adult Miscellaneous Organisation" },
                    { 55, "7799", "7799", "Divided service between Special Schools", "Divided service between Special Schools" },
                    { 56, "8999", "8999", "Divided service between FE establishments", "Divided service between FE establishments" },
                    { 57, "0953", "0953", "Adult Miscellaneous Organisation (not allocated an Estab No) - ie teacher paid under FE document, employed providing FE or Adult Education (eg Community College)", "Adult Miscellaneous Organisation (not allocated an Estab No)" },
                    { 58, "0955", "0955", "Teacher employed by Ministry of Defence (UK based)", "Teacher employed by Ministry of Defence (UK based)" },
                    { 59, "0960", "0960", "Unattached regular engagement in Primary Schools - ie Permanent 'supply' teacher under contract", "Unattached regular engagement in Primary Schools - ie Permanent 'supply' teacher under contract" },
                    { 60, "0961", "0961", "Unattached regular engagement in Secondary or P & S - ie Permanent 'supply' teacher under contract", "Unattached regular engagement in Secondary or P & S - ie Permanent 'supply' teacher under contract" },
                    { 61, "0962", "0962", "Visiting Teacher Primary - peripatetic teacher (eg specialist subject teacher visiting different schools)", "Visiting Teacher Primary - peripatetic teacher (eg specialist subject teacher visiting different schools)" },
                    { 62, "0963", "0963", "Visiting Teacher Secondary or P & S - peripatetic teacher (eg specialist subject teacher visiting different schools)", "Visiting Teacher Secondary or P & S - peripatetic teacher (eg specialist subject teacher visiting different schools)" },
                    { 63, "0964", "0964", "P & S teaching under Section 56 of Education Act 1944 - ie teaching other than at a school (eg at home or in a hospital, or teachers in penal establishments)", "P & S teaching under Section 56 of Education Act 1944 - ie teaching other than at a school" },
                    { 64, "0965", "0965", "Peripatetic support wholly for SEN or disabled not in a special school.", "Peripatetic support wholly for SEN or disabled not in a special school." },
                    { 65, "0966", "0966", "School supply teacher - whose contract is terminable without notice. Teacher who is employed temporarily in place of a regularly employed teacher. Teacher has made a part-time election." , "School supply teacher - whose contract is terminable without notice" },
                    { 66, "0970", "0971", "Full-time Organiser - employment involves the performance of duties in connection with the provision of education or service ancillary to education (accepted in TPS only if previously accepted under 1967 Teachers' Pension Regulations)" , "Full-time Organiser" },
                    { 67, "0972", "0972", "Full and Part-Time Youth and Community Worker" , "Full and Part-Time Youth and Community Worker" },
                    { 68, "5199", "5199", "Service as a teacher in Practical Instruction Centres - providing P & S education (previously allocated individual numbers in range 5100-5199)(not schools - unattached units). (Service other than as a teacher would have to be considered by the Department)" , "Service as a teacher in Practical Instruction Centres - providing P & S education" },
                    { 69, "5549", "5549", "Service as a teacher in Remedial Centres and Support Units - providing P & S education (not schools - unattached units). (Service other than as a teacher would have to be considered by the Department", "Service as a teacher in Remedial Centres and Support Units - providing P & S education (not schools - unattached units)" },
                    { 70, "5599", "5599", "Service as a teacher in any other P & S Centre (ie not PI or Remedial)(eg Assessment Centres outdoor pursuit centres, Teacher Centres [if paid P & S]). Service other than as a teacher would have to be considered by Teachers' Pensions)", "Service as a teacher in any other P & S Centre (ie not PI or Remedial)"},
                    { 71, "5600", "5600", "Service in Intermediate Treatment Centres - providing P & S education (not schools - unattached units)", "Service in Intermediate Treatment Centres - providing P & S education (not schools - unattached units)" },
                    { 72, "9099", "9099", "Function Provider within a LEA", "Function Provider within a LEA" },
                    { 73, "8899", "8899", "Adult Education Service (residential adult education estabs have numbers allocated in range 8290-8294). Teacher Centres if paid on FE Scales, Adult Literacy Scheme staff (LA)" , "Adult Education Service" },
                    { 74, "0999", "0999", "Service of any other kind (eg full-time educational officers in penal estabs Job Creation Schemes). Normally this code will not be used where a teacher is in receipt of a mandatory Burnham salary", "Service of any other kind (eg full-time educational officers in penal estabs Job Creation Schemes)" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tps_establishment_types");

            migrationBuilder.DropTable(
                name: "tps_establishments");

            migrationBuilder.DropIndex(
                name: "ix_establishment_urn",
                table: "establishments");

            migrationBuilder.CreateIndex(
                name: "ix_establishment_urn",
                table: "establishments",
                column: "urn",
                unique: true);
        }
    }
}
