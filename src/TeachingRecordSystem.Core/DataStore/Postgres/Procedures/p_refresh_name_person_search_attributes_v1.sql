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
