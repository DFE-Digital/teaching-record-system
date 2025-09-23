CREATE OR REPLACE TRIGGER trg_delete_tps_employments_person_attrs
    AFTER DELETE
    ON tps_employments
    REFERENCING OLD TABLE AS old_tps_employments
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_delete_tps_employments_person_attrs();
