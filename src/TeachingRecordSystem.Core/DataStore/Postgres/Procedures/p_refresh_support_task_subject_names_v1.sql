CREATE OR REPLACE PROCEDURE public.p_refresh_support_task_subject_names(
    IN p_support_task_references varchar[])
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN

    UPDATE support_tasks
    SET subject_names = fn_split_names(ARRAY[subject_name]::varchar[] COLLATE "default")
    WHERE support_task_reference = ANY(p_support_task_references);

END;
$BODY$;
