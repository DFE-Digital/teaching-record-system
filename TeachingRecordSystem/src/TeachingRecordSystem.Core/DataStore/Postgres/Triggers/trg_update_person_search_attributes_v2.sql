CREATE OR REPLACE TRIGGER trg_update_person_search_attributes
    AFTER UPDATE
    ON persons
    REFERENCING OLD TABLE AS old_persons NEW TABLE AS new_persons
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_update_person_search_attributes();
