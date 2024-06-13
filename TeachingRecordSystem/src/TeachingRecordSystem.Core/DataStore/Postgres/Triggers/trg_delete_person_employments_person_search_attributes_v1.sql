CREATE OR REPLACE TRIGGER trg_delete_person_employments_person_search_attributes
    AFTER DELETE
    ON person_employments
    REFERENCING OLD TABLE AS old_person_employments
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_delete_person_employments_person_search_attributes();
