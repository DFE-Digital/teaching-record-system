CREATE OR REPLACE FUNCTION fn_update_person_search_attributes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE person_ids uuid[];
BEGIN

    person_ids := ARRAY(
        SELECT o.person_id FROM old_persons o
        JOIN new_persons n ON o.person_id = n.person_id
        WHERE n.first_name != o.first_name
        OR n.last_name != o.last_name
        OR n.date_of_birth != o.date_of_birth
        OR n.national_insurance_number != o.national_insurance_number
        OR n.trn != o.trn
        );

    IF (array_length(person_ids, 1) > 0) THEN
        CALL p_refresh_person_search_attributes(person_ids);
    END IF;
    
    RETURN NULL;
END;
$BODY$
