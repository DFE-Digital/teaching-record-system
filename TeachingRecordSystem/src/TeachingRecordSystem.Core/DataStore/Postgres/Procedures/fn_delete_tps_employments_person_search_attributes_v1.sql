CREATE OR REPLACE FUNCTION fn_delete_tps_employments_person_search_attributes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    DELETE FROM
        person_search_attributes psa
    USING
        old_tps_employments o
    WHERE
        psa.attribute_key = o.tps_employment_id::text;
    
    RETURN NULL;
END;
$BODY$
