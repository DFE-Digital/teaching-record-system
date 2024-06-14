CREATE OR REPLACE FUNCTION fn_delete_person_employments_person_search_attributes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    DELETE FROM
        person_search_attributes psa
    USING
        old_person_employments o
    WHERE
        psa.attribute_key = o.person_employment_id::text;
    
    RETURN NULL;
END;
$BODY$
