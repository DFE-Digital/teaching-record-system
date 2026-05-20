CREATE OR REPLACE TRIGGER trg_update_previous_names_person_search_attributes
    AFTER UPDATE
    ON previous_names
    REFERENCING OLD TABLE as old_previous_names NEW TABLE AS new_previous_names
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_insert_previous_names_person_search_attributes();
