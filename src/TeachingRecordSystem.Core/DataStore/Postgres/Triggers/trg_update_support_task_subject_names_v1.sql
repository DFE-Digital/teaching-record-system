CREATE OR REPLACE TRIGGER trg_update_support_task_subject_names
    AFTER UPDATE
    ON support_tasks
    REFERENCING OLD TABLE AS old_support_tasks NEW TABLE AS new_support_tasks
    FOR EACH STATEMENT
    EXECUTE FUNCTION fn_update_support_task_subject_names();
