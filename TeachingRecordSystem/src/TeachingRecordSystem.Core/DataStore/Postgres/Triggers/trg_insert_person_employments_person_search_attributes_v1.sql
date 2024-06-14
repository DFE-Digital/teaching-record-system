CREATE OR REPLACE TRIGGER trg_insert_person_employments_person_search_attributes
    AFTER INSERT
    ON person_employments
    REFERENCING NEW TABLE AS new_person_employments
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_insert_person_employments_person_search_attributes();
