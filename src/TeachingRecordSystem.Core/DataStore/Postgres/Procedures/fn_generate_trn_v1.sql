CREATE OR REPLACE FUNCTION fn_generate_trn()
    RETURNS INT
    LANGUAGE 'plpgsql'
AS $BODY$
DECLARE
    next_available_trn INT;
BEGIN
    UPDATE
        trn_ranges
    SET
        is_exhausted = CASE WHEN next_trn >= to_trn THEN TRUE ELSE FALSE END, 
        next_trn = next_trn + 1
    WHERE
        from_trn = (SELECT
                        from_trn
                    FROM
                        trn_ranges
                    WHERE
                        is_exhausted IS FALSE
                    ORDER BY
                        from_trn
                    FOR UPDATE
                    LIMIT 1)
    RETURNING next_trn - 1
    INTO next_available_trn;
    
    RETURN next_available_trn;
END;
$BODY$
