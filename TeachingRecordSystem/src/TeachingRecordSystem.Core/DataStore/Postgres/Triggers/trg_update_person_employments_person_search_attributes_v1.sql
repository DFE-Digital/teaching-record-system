CREATE OR REPLACE TRIGGER trg_update_person_employments_person_search_attributes
    AFTER UPDATE
    ON person_employments
    REFERENCING OLD TABLE AS old_person_employments NEW TABLE AS new_person_employments
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_update_person_employments_person_search_attributes();
