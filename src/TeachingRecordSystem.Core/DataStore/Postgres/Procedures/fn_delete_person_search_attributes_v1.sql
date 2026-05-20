CREATE OR REPLACE FUNCTION fn_delete_person_search_attributes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    DELETE FROM
        person_search_attributes psa
    USING
        old_persons o
    WHERE
        psa.person_id = o.person_id;
    
    RETURN NULL;
END;
$BODY$
