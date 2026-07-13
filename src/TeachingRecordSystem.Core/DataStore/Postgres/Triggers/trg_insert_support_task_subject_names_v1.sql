CREATE OR REPLACE TRIGGER trg_insert_support_task_subject_names
    AFTER INSERT
    ON support_tasks
    REFERENCING NEW TABLE AS new_support_tasks
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_insert_support_task_subject_names();
