CREATE OR REPLACE FUNCTION fn_update_person_search_attributes()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
BEGIN
    IF ((TG_OP = 'DELETE')) THEN
        DELETE FROM
            person_search_attributes
        WHERE
            person_id = OLD.person_id;
    END IF;
    
    IF (((TG_OP = 'INSERT') OR (TG_OP = 'UPDATE')) AND NEW.deleted_on IS NULL) THEN
        CALL p_refresh_person_search_attributes(
            NEW.person_id,
            NEW.first_name,
            NEW.last_name,
            NEW.date_of_birth,
            NEW.national_insurance_number,
            NEW.trn);
    END IF;
    
    RETURN NULL; -- result is ignored since this is an AFTER trigger
END;
$BODY$
