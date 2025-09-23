CREATE OR REPLACE TRIGGER trg_update_tps_employments_person_attrs
    AFTER UPDATE
    ON tps_employments
    REFERENCING OLD TABLE AS old_tps_employments NEW TABLE AS new_tps_employments
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_update_tps_employments_person_attrs();
