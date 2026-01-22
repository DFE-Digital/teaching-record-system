CREATE OR REPLACE FUNCTION public.fn_resolve_record_by_trn(
	IN p_trn CHAR(7)
)
RETURNS TABLE(person_id UUID, trn CHAR(7), status INT)
LANGUAGE SQL
AS $body$

	WITH RECURSIVE active_persons(person_id) AS (
		SELECT person_id FROM persons WHERE trn = p_trn
		UNION ALL
		SELECT persons.merged_with_person_id FROM persons, active_persons
		WHERE persons.person_id = active_persons.person_id
	)
    SELECT p.person_id, p.trn, p.status FROM persons p
    JOIN active_persons a ON p.person_id = a.person_id
    WHERE p.merged_with_person_id IS NULL;

$body$;
