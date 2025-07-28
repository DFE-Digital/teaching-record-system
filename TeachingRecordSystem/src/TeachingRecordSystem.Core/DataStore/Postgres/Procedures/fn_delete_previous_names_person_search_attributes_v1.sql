CREATE OR REPLACE FUNCTION fn_delete_previous_names_person_search_attributes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    DELETE FROM
        person_search_attributes psa
    USING
        old_previous_names o
    WHERE
        psa.person_id = o.person_id AND
        psa.attribute_key = o.previous_name_id::varchar AND
        'data_source:previous_names' = ANY(psa.tags);

    RETURN NULL;
END;
$BODY$
