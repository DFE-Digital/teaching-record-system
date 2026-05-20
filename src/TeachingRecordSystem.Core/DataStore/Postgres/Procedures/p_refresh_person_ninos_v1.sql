CREATE OR REPLACE PROCEDURE public.p_refresh_person_ninos(
    IN p_person_ids uuid[])
LANGUAGE 'plpgsql'
AS $BODY$
BEGIN

    WITH nino_data AS (
        SELECT person_id, array_agg(DISTINCT national_insurance_number) ninos
        FROM (
             SELECT person_id, national_insurance_number FROM persons WHERE person_id = ANY(p_person_ids)
             UNION ALL
             SELECT
                 person_id,
                 national_insurance_number
             FROM tps_employments WHERE person_id = ANY(p_person_ids)
         )
        GROUP BY person_id
    )
    UPDATE persons
    SET
        national_insurance_numbers = array_remove(nino_data.ninos, NULL)
    FROM nino_data
    WHERE persons.person_id = nino_data.person_id;

END;
$BODY$;
