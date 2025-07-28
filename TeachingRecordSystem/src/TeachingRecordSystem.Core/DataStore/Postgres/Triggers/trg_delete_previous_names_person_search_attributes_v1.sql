CREATE OR REPLACE TRIGGER trg_delete_previous_names_person_search_attributes
    AFTER DELETE
    ON previous_names
    REFERENCING OLD TABLE AS old_previous_names
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_delete_previous_names_person_search_attributes();
