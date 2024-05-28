CREATE OR REPLACE TRIGGER trg_update_person_search_attributes
    AFTER INSERT OR DELETE OR UPDATE OF first_name, last_name, date_of_birth, national_insurance_number, trn, deleted_on
    ON persons
    FOR EACH ROW
    EXECUTE FUNCTION fn_update_person_search_attributes();
