CREATE OR REPLACE FUNCTION fn_insert_tps_employments_person_search_attributes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE tps_employment_ids uuid[];
BEGIN

    tps_employment_ids := ARRAY(SELECT tps_employment_id FROM new_tps_employments);

    CALL p_refresh_tps_employments_person_search_attributes(tps_employment_ids);
    
    RETURN NULL;
END;
$BODY$
