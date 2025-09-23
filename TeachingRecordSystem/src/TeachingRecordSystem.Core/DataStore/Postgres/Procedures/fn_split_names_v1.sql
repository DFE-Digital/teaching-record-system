CREATE OR REPLACE FUNCTION fn_split_names(
       names varchar[]
) RETURNS varchar[] AS $$
    SELECT array_agg(distinct names) FROM (
        SELECT unnest(string_to_array(regexp_replace(trim(n), '(\s|-)+', ' '), ' ')) names FROM unnest(names) AS data(n)
    )
$$
LANGUAGE sql;
