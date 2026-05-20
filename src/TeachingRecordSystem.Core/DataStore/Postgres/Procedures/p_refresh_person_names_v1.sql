CREATE OR REPLACE PROCEDURE public.p_refresh_person_names(
    IN p_person_ids uuid[])
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN

    WITH name_data AS (
        SELECT
            person_id,
            string_agg(first_name, ' ') first_names,
            string_agg(middle_name, ' ') middle_names,
            string_agg(last_name, ' ') last_names
        FROM (
             SELECT person_id, first_name, middle_name, last_name FROM persons WHERE person_id = ANY(p_person_ids)
             UNION ALL
             SELECT person_id, first_name, middle_name, last_name FROM previous_names WHERE person_id = ANY(p_person_ids) AND deleted_on IS NULL
         )
        GROUP BY person_id
    )
    UPDATE persons
    SET
        names = fn_split_names(ARRAY[name_data.first_names, name_data.middle_names, name_data.last_names]::varchar[] COLLATE "default"),
        last_names = fn_split_names(ARRAY[name_data.last_names]::varchar[] COLLATE "default")
    FROM name_data
    WHERE persons.person_id = name_data.person_id;

END;
$BODY$;
