CREATE OR REPLACE TRIGGER trg_insert_person_search_attributes
    AFTER INSERT
    ON persons
    REFERENCING NEW TABLE AS new_persons
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_insert_person_search_attributes();
