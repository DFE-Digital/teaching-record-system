CREATE OR REPLACE PROCEDURE public.p_refresh_person_employments_person_search_attributes(
	IN p_person_employment_ids uuid[])
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    DELETE FROM person_search_attributes WHERE attribute_key = ANY(p_person_employment_ids::text[]);
    
    INSERT INTO person_search_attributes (person_id, attribute_type, attribute_value, tags, attribute_key)
	WITH person_employments_data AS (
		SELECT
		    person_id,
		    national_insurance_number,
		    person_postcode,
            person_employment_id
		FROM
		    person_employments pe
		WHERE
		    pe.person_employment_id = ANY(p_person_employment_ids)
	),
	attribs AS (
		SELECT
			pe.person_id,
			unnest(ARRAY['NationalInsuranceNumber', 'Postcode']) attribute_type,
			unnest(ARRAY[pe.national_insurance_number, pe.person_postcode]) attribute_value,        
			ARRAY['data_source:person_employments'] tags,
            pe.person_employment_id attribute_key
		FROM
			person_employments_data pe
	)
	SELECT
	    *
	FROM
	    attribs
    WHERE
        attribs.attribute_value IS NOT NULL;
END;
$BODY$;
