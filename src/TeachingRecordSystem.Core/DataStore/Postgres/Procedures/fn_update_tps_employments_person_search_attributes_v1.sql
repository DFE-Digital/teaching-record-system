CREATE OR REPLACE FUNCTION fn_update_tps_employments_person_search_attributes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE tps_employment_ids uuid[];
BEGIN

    tps_employment_ids := ARRAY(
        SELECT o.tps_employment_id FROM old_tps_employments o
        JOIN new_tps_employments n ON o.tps_employment_id = n.tps_employment_id
        WHERE n.national_insurance_number IS DISTINCT FROM o.national_insurance_number
        OR n.person_postcode IS DISTINCT FROM o.person_postcode
        );

    IF (array_length(tps_employment_ids, 1) > 0) THEN
        CALL p_refresh_tps_employments_person_search_attributes(tps_employment_ids);
    END IF;
    
    RETURN NULL;
END;
$BODY$
