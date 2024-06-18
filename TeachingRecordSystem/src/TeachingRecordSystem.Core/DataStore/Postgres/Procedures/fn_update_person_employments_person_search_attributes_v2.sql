CREATE OR REPLACE FUNCTION fn_update_person_employments_person_search_attributes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE person_employment_ids uuid[];
BEGIN

    person_employment_ids := ARRAY(
        SELECT o.person_employment_id FROM old_person_employments o
        JOIN new_person_employments n ON o.person_employment_id = n.person_employment_id
        WHERE n.national_insurance_number IS DISTINCT FROM o.national_insurance_number
        OR n.person_postcode IS DISTINCT FROM o.person_postcode
        );

    IF (array_length(person_employment_ids, 1) > 0) THEN
        CALL p_refresh_person_employments_person_search_attributes(person_employment_ids);
    END IF;
    
    RETURN NULL;
END;
$BODY$
