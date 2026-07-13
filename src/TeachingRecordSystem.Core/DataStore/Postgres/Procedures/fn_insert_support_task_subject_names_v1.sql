CREATE OR REPLACE FUNCTION fn_insert_support_task_subject_names()
    RETURNS trigger
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE support_task_references varchar[];
BEGIN

    support_task_references := ARRAY(SELECT support_task_reference FROM new_support_tasks);

    CALL p_refresh_support_task_subject_names(support_task_references);

    RETURN NULL;

END;
$BODY$
