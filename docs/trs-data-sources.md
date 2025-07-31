# TRS Data Sources

## Countries Reference Data

We use the reference data from the [DfE data gem](https://github.com/DFE-Digital/dfe-reference-data/blob/main/docs/lists_countries_and_territories.md).

## Subjects Reference Data

We use the reference data from the [DfE data gem](https://github.com/DFE-Digital/dfe-reference-data/blob/main/docs/lists_subject_specialisms.md).

We use a combination of the current [HECoS subject codes](https://www.hesa.ac.uk/collection/c24053/) which are [subject specialisms](https://www.hesa.ac.uk/collection/c24053/e/SBJCA) and legacy DQT subject codes which we could not map to HECoS codes.
We attemped mapping of legacy codes in the following way:

1. Is it a current HECoS code
2. Is there an exact match between the DQT name field and a HECoS name if you subsitute "Ancient" and "Classical"
3. Is there an exact match between the DQT name field and a JACS3 name if you subsitute "Ancient" and "Classical"
4. Is there an exact match between the DQT name field and a HECoS name
5. Is there an exact match between the DQT code and a JACS code, which in turn matches to a HECoS code
6. Is there an exact match between the DQT name field and a JACS name, which in turn matches to a HECoS name/code
7. Is there an exact match between the DQT name field and the non-preffered HECoS subject name

## Providers Reference Data

We use a combination of the reference data from the list of currently active ITT providers, provider codes from legacy providers which are no longer active, and providers/establishments that we receive from TPS data.
