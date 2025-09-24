CREATE OR REPLACE FUNCTION fn_split_names(
       names varchar[],
       include_synonyms boolean DEFAULT false
) RETURNS varchar[] AS $$
    WITH split_names AS (
        SELECT unnest(string_to_array(regexp_replace(trim(coalesce(n, '')), '(\s|-)+', ' '), ' ')) name_part, false is_synonym
        FROM unnest(names) AS data(n)
    ),
    synonyms AS (
        SELECT unnest(synonyms) name_part, true is_synonym FROM name_synonyms n
        JOIN split_names s ON n.name = s.name_part
    ),
    combined AS (
        SELECT name_part, is_synonym FROM split_names
        UNION ALL
        SELECT name_part, is_synonym FROM synonyms
    )
    SELECT array_agg(distinct name_part) FROM combined
    WHERE (is_synonym = false OR include_synonyms = true)
$$
LANGUAGE sql;
