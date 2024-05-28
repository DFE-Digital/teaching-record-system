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
