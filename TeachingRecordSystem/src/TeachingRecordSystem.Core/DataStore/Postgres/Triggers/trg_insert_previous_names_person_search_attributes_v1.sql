CREATE OR REPLACE TRIGGER trg_insert_previous_names_person_search_attributes
    AFTER INSERT
    ON previous_names
    REFERENCING NEW TABLE AS new_previous_names
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_insert_previous_names_person_search_attributes();
