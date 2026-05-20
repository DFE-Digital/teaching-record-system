CREATE OR REPLACE FUNCTION fn_insert_person_attrs()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE person_ids uuid[];
BEGIN

    person_ids := ARRAY(SELECT person_id FROM new_persons);

    CALL p_refresh_person_names(person_ids);
    CALL p_refresh_person_ninos(person_ids);

    RETURN NULL;

END;
$BODY$
