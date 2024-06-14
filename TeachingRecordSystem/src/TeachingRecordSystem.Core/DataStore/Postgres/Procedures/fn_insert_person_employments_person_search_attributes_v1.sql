CREATE OR REPLACE FUNCTION fn_insert_person_employments_person_search_attributes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE person_employment_ids uuid[];
BEGIN

    person_employment_ids := ARRAY(SELECT person_employment_id FROM new_person_employments);

    CALL p_refresh_person_employments_person_search_attributes(person_employment_ids);
    
    RETURN NULL;
END;
$BODY$
