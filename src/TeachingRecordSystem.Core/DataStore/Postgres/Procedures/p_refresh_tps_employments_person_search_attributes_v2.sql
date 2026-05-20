CREATE OR REPLACE PROCEDURE public.p_refresh_tps_employments_person_search_attributes(
	IN p_tps_employment_ids uuid[])
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    DELETE FROM person_search_attributes WHERE attribute_key = ANY(p_tps_employment_ids::text[]);

    INSERT INTO person_search_attributes (person_id, attribute_type, attribute_value, tags, attribute_key)
	WITH tps_employments_data AS (
		SELECT
		    person_id,
		    national_insurance_number,
		    person_postcode,
            tps_employment_id
		FROM
		    tps_employments te
		WHERE
		    te.tps_employment_id = ANY(p_tps_employment_ids)
	),
	attribs AS (
		SELECT
			te.person_id,
			unnest(ARRAY['NationalInsuranceNumber', 'Postcode']) attribute_type,
			unnest(ARRAY[te.national_insurance_number, te.person_postcode]) attribute_value,
			ARRAY['data_source:tps_employments'] tags,
            te.tps_employment_id attribute_key
		FROM
			tps_employments_data te
	)
	SELECT
	    *
	FROM
	    attribs
    WHERE
        TRIM(COALESCE(attribs.attribute_value, '')) != '';
END;
$BODY$;
