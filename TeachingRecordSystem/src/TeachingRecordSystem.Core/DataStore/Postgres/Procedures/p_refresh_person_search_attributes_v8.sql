CREATE OR REPLACE PROCEDURE public.p_refresh_person_search_attributes(
    IN p_person_ids uuid[])
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN

DELETE FROM person_search_attributes
WHERE person_id = ANY(p_person_ids) AND 'data_source:persons' = ANY(tags);

WITH person_data AS (
    SELECT
        p.person_id,
        trim(regexp_replace((p.first_name COLLATE "default"), '\s+', ' ', 'g')) first_name,
        trim(regexp_replace((p.middle_name COLLATE "default"), '\s+', ' ', 'g')) middle_name,
        trim(regexp_replace((p.last_name COLLATE "default"), '\s+', ' ', 'g')) last_name,
        p.date_of_birth,
        p.national_insurance_number,
        p.trn,
        p.email_address
    FROM persons p
    WHERE p.person_id = ANY(p_person_ids)
),
 attrs AS (
     SELECT
         x.person_id,
         unnest(ARRAY['DateOfBirth', 'NationalInsuranceNumber', 'Trn', 'EmailAddress']) attribute_type,
         unnest(ARRAY[to_char(x.date_of_birth, 'yyyy-mm-dd'), x.national_insurance_number, x.trn, x.email_address]) attribute_value,
         ARRAY['data_source:persons'] tags,
         NULL attribute_key
     FROM person_data x
     UNION
     SELECT
         person_id,
         'FirstName',
         unnest(ARRAY[string_to_array(first_name, ' ')]),
         ARRAY['data_source:persons'] tags,
         NULL attribute_key
     FROM person_data
     UNION
     SELECT
         person_id,
         'FirstName',
         unnest(n.synonyms),
         ARRAY['data_source:persons', CONCAT('Synonym:', p.first_name)] tags,
        NULL attribute_key
    FROM person_data p, name_synonyms n, unnest(ARRAY[string_to_array(p.first_name, ' ')]) AS first_names(name)
    WHERE first_names.name = n.name
    UNION
    SELECT
        person_id,
        'MiddleName',
        unnest(ARRAY[string_to_array(middle_name, ' ')]),
        ARRAY['data_source:persons'] tags,
        NULL attribute_key
    FROM person_data
    UNION
    SELECT
        person_id,
        'LastName',
        unnest(ARRAY[string_to_array(last_name, ' ')]),
        ARRAY['data_source:persons'] tags,
        NULL attribute_key
    FROM person_data
)
INSERT INTO person_search_attributes (person_id, attribute_type, attribute_value, tags, attribute_key)
SELECT person_id, attribute_type, attribute_value, tags, attribute_key FROM attrs
WHERE TRIM(COALESCE(attribute_value, '')) != '';

END;
$BODY$;


