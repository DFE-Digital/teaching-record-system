CREATE OR REPLACE FUNCTION fn_update_tps_employments_person_attrs()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE person_ids uuid[];
BEGIN

    person_ids := ARRAY(
        SELECT DISTINCT o.person_id FROM old_tps_employments o
        JOIN new_tps_employments n ON o.tps_employment_id = n.tps_employment_id
        WHERE n.national_insurance_number IS DISTINCT FROM o.national_insurance_number);

    IF (array_length(person_ids, 1) > 0) THEN
        CALL p_refresh_person_ninos(person_ids);
    END IF;

    RETURN NULL;

END;
$BODY$
