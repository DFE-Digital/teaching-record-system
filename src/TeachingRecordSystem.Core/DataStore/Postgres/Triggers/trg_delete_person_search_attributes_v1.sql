CREATE OR REPLACE TRIGGER trg_delete_person_search_attributes
    AFTER DELETE
    ON persons
    REFERENCING OLD TABLE AS old_persons
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_delete_person_search_attributes();
