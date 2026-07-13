CREATE OR REPLACE FUNCTION fn_update_support_task_subject_names()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE support_task_references varchar[];
BEGIN

    support_task_references := ARRAY(
        SELECT o.support_task_reference FROM old_support_tasks o
        JOIN new_support_tasks n ON o.support_task_reference = n.support_task_reference
        WHERE n.subject_name IS DISTINCT FROM o.subject_name);

    IF (array_length(support_task_references, 1) > 0) THEN
        CALL p_refresh_support_task_subject_names(support_task_references);
    END IF;

    RETURN NULL;

END;
$BODY$
