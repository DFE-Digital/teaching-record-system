CREATE OR REPLACE PROCEDURE public.p_refresh_person_search_attributes(
    IN p_person_ids uuid[])
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN

DELETE FROM person_search_attributes WHERE person_id = ANY(p_person_ids);

INSERT INTO person_search_attributes (person_id, attribute_type, attribute_value, tags, attribute_key)
WITH person_data AS (
	SELECT p.person_id, p.first_name, p.last_name, p.date_of_birth, p.national_insurance_number, p.trn
	FROM persons p
	WHERE p.person_id = ANY(p_person_ids)
),
attrs AS (
    SELECT
	    x.person_id,
	    unnest(ARRAY['DateOfBirth', 'NationalInsuranceNumber', 'Trn', 'LastName', 'FirstName', 'FullName']) attribute_type,
	    unnest(ARRAY[to_char(x.date_of_birth, 'yyyy-mm-dd'), x.national_insurance_number, x.trn, x.last_name, x.first_name, CONCAT(x.first_name, ' ', x.last_name)]) attribute_value,
	    ARRAY[]::text[] tags,
	    NULL attribute_key
    FROM person_data x
    UNION SELECT
	    n.person_id,
	    unnest(ARRAY['FirstName', 'FullName']) attribute_key,
	    unnest(ARRAY[n.synonym, CONCAT(n.synonym, ' ', n.last_name)]) attribute_value,
	    ARRAY[CONCAT('Synonym:', n.first_name)] tags,
	    NULL attribute_key
	    FROM (
		    SELECT x.person_id, x.first_name, x.last_name, unnest(synonyms) synonym
		    FROM person_data x
		    JOIN name_synonyms n ON x.first_name = n.name) n
)
SELECT * FROM attrs WHERE attribute_value IS NOT NULL;
		
END;
$BODY$;
