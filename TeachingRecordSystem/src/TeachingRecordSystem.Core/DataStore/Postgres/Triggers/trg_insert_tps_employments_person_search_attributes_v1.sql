CREATE OR REPLACE TRIGGER trg_insert_tps_employments_person_search_attributes
    AFTER INSERT
    ON tps_employments
    REFERENCING NEW TABLE AS new_tps_employments
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_insert_tps_employments_person_search_attributes();
