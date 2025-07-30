CREATE OR REPLACE PROCEDURE public.p_refresh_previous_names_person_search_attributes(
    IN p_person_ids uuid[])
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN

DELETE FROM person_search_attributes
WHERE person_id = ANY(p_person_ids) AND 'data_source:previous_names' = ANY(tags);

WITH person_data AS (
	SELECT p.previous_name_id, p.person_id, p.first_name, p.middle_name, p.last_name
	FROM previous_names p
	WHERE p.person_id = ANY(p_person_ids) AND p.deleted_on IS NULL
),
attrs AS (
    SELECT
	    x.person_id,
	    unnest(ARRAY['PreviousLastName', 'PreviousMiddleName', 'PreviousFirstName', 'PreviousFullName']) attribute_type,
	    unnest(ARRAY[x.last_name, x.middle_name, x.first_name, CONCAT(x.first_name, ' ', x.last_name)]) attribute_value,
	    ARRAY['data_source:previous_names'] tags,
	    x.previous_name_id::varchar attribute_key
    FROM person_data x
    UNION SELECT
	    n.person_id,
	    unnest(ARRAY['PreviousFirstName', 'PreviousFullName']) attribute_type,
	    unnest(ARRAY[n.synonym, CONCAT(n.synonym, ' ', n.last_name)]) attribute_value,
	    ARRAY['data_source:previous_names', CONCAT('Synonym:', n.first_name)] tags,
	    previous_name_id::varchar attribute_key
	    FROM (
		    SELECT x.previous_name_id, x.person_id, x.first_name, x.last_name, unnest(synonyms) synonym
		    FROM person_data x
		    JOIN name_synonyms n ON x.first_name = n.name) n
)
INSERT INTO person_search_attributes (person_id, attribute_type, attribute_value, tags, attribute_key)
SELECT person_id, attribute_type, attribute_value, tags, attribute_key FROM attrs
WHERE attribute_value IS NOT NULL;

END;
$BODY$;
