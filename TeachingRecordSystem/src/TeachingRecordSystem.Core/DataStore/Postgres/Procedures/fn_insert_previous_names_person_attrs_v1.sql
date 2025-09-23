CREATE OR REPLACE FUNCTION fn_insert_previous_names_person_attrs()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE person_ids uuid[];
BEGIN

    person_ids := ARRAY(SELECT DISTINCT person_id FROM new_previous_names);

    CALL p_refresh_person_names(person_ids);

    RETURN NULL;

END;
$BODY$
