using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TeachingRecordSystem.Core.DataStore.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PersonSearchAttributeAndNameSynonyms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "name_synonyms",
                columns: table => new
                {
                    name_synonyms_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    synonyms = table.Column<string[]>(type: "text[]", nullable: false, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_name_synonyms", x => x.name_synonyms_id);
                });

            migrationBuilder.CreateTable(
                name: "person_search_attributes",
                columns: table => new
                {
                    person_search_attribute_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    person_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attribute_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, collation: "case_insensitive"),
                    attribute_value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, collation: "case_insensitive"),
                    tags = table.Column<string[]>(type: "text[]", nullable: false),
                    attribute_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, collation: "case_insensitive")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person_search_attributes", x => x.person_search_attribute_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_name_synonyms_name",
                table: "name_synonyms",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_person_search_attributes_attribute_type_and_value",
                table: "person_search_attributes",
                columns: new[] { "attribute_type", "attribute_value" });

            migrationBuilder.CreateIndex(
                name: "ix_person_search_attributes_person_id",
                table: "person_search_attributes",
                column: "person_id");

            var refreshNameProcedureSql = @"
CREATE OR REPLACE PROCEDURE public.p_refresh_name_person_search_attributes(
    IN p_person_id uuid,
    IN p_first_name character varying,
    IN p_last_name character varying,
    IN p_attribute_key character varying)
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    DELETE FROM
        person_search_attributes
    WHERE
        person_id = p_person_id
        AND attribute_key = p_attribute_key;

    INSERT INTO
        person_search_attributes
        (
            person_id,
            attribute_type,
            attribute_value,
            tags,
            attribute_key
        )
    SELECT
        p_person_id,
        attribs.attribute_type,
        attribs.attribute_value,
        ARRAY[]::text[],
        p_attribute_key
    FROM
        (VALUES
         ('FirstName', p_first_name),
         ('LastName', p_last_name)) AS attribs (attribute_type, attribute_value)
    WHERE
        attribs.attribute_value IS NOT NULL;
    
    -- Insert synonyms of first name
    INSERT INTO
        person_search_attributes
        (
            person_id,
            attribute_type,
            attribute_value,
            tags,
            attribute_key
        )
    SELECT
        p_person_id,
        'FirstName',
        UNNEST(synonyms),
        ARRAY[CONCAT('Synonym:', p_first_name)],
        p_attribute_key
    FROM
        name_synonyms
    WHERE
        name = p_first_name;
        
    -- Insert full name as a searchable attribute
    INSERT INTO 
        person_search_attributes
        (
            person_id,
            attribute_type,
            attribute_value,
            tags,
            attribute_key
        )
    SELECT 
        first_names.person_id, 
        'FullName', 
        first_names.attribute_value || ' ' || last_names.attribute_value, 
        '{}',
        first_names.attribute_key
    FROM
            person_search_attributes first_names
        JOIN
            person_search_attributes last_names ON first_names.person_id = last_names.person_id AND first_names.attribute_key = last_names.attribute_key
    WHERE
        first_names.person_id = p_person_id
        AND first_names.attribute_type = 'FirstName'
        AND last_names.attribute_type = 'LastName';
END;
$BODY$;
";
            migrationBuilder.Sql(refreshNameProcedureSql);

            var refreshProcedureSql = @"
CREATE OR REPLACE PROCEDURE public.p_refresh_person_search_attributes(
    IN p_person_id uuid,
    IN p_first_name character varying(100),
    IN p_last_name character varying(100),
    IN p_date_of_birth date,
    IN p_national_insurance_number character(9),
    IN p_trn character(7))
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    DELETE FROM
        person_search_attributes
    WHERE
        person_id = p_person_id
        AND attribute_key IS NULL;
    
    INSERT INTO
        person_search_attributes
        (
            person_id,
            attribute_type,
            attribute_value,
            tags
        )
    SELECT
        p_person_id,
        attribs.attribute_type,
        attribs.attribute_value,
        '{}'
    FROM
        (VALUES 		 
         ('DateOfBirth', CASE WHEN p_date_of_birth IS NULL THEN NULL ELSE to_char(p_date_of_birth, 'yyyy-mm-dd') END),
         ('NationalInsuranceNumber', p_national_insurance_number),
         ('Trn', p_trn)) AS attribs (attribute_type, attribute_value)
    WHERE
        attribs.attribute_value IS NOT NULL;

    CALL p_refresh_name_person_search_attributes(
        p_person_id,
        p_first_name,
        p_last_name,
        '1');
END;
$BODY$;
";
            migrationBuilder.Sql(refreshProcedureSql);

            var triggerFunctionSql = @"
CREATE OR REPLACE FUNCTION fn_update_person_search_attributes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    IF ((TG_OP = 'DELETE')) THEN
        DELETE FROM
            person_search_attributes
        WHERE
            person_id = OLD.person_id;
    END IF;
    
    IF (((TG_OP = 'INSERT') OR (TG_OP = 'UPDATE')) AND NEW.deleted_on IS NULL) THEN
        CALL p_refresh_person_search_attributes(
            NEW.person_id,
            NEW.first_name,
            NEW.last_name,
            NEW.date_of_birth,
            NEW.national_insurance_number,
            NEW.trn);
    END IF;
    
    RETURN NULL; -- result is ignored since this is an AFTER trigger
END;
$BODY$
";
            migrationBuilder.Sql(triggerFunctionSql);

            var triggerSql = @"
CREATE TRIGGER trg_update_person_search_attributes
    AFTER INSERT OR DELETE OR UPDATE OF first_name, last_name, date_of_birth, national_insurance_number, trn, deleted_on
    ON persons
    FOR EACH ROW
    EXECUTE FUNCTION fn_update_person_search_attributes()
";
            migrationBuilder.Sql(triggerSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var dropTriggerSql = @"
DROP TRIGGER trg_update_person_search_attributes ON persons
";
            migrationBuilder.Sql(dropTriggerSql);

            var dropTriggerFunctionSql = @"
DROP FUNCTION fn_update_person_search_attributes()
";
            migrationBuilder.Sql(dropTriggerFunctionSql);

            var dropRefreshProcedureSql = @"
DROP PROCEDURE public.p_refresh_person_search_attributes(uuid, character varying, character varying, date, character, character)
";
            migrationBuilder.Sql(dropRefreshProcedureSql);

            var dropRefreshNameProcedureSql = @"
DROP PROCEDURE public.p_refresh_name_person_search_attributes(uuid, character varying, character varying, character varying)
";

            migrationBuilder.Sql(dropRefreshNameProcedureSql);

            migrationBuilder.DropTable(
                name: "name_synonyms");

            migrationBuilder.DropTable(
                name: "person_search_attributes");
        }
    }
}
