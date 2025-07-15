# TRS Data Sources

## Countries Reference Data

We use the reference data from the [DfE data gem](dfe-reference-data\lib\dfe\reference_data\countries_and_territories.rb).

## Subjects Reference Data

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

We use a combination of the reference data from the [DfE data gem] and [provider codes from legacy providers] which are no longer active.

<details>
  <summary>All Subject Codes</summary>

 <details>

 <summary>HECoS Subject Codes</summary>

| Field | Code   | Label                                    |
| ----- | ------ | ---------------------------------------- |
| SBJCA | 100048 | design                                   |
| SBJCA | 100061 | graphic design                           |
| SBJCA | 100068 | dance                                    |
| SBJCA | 100069 | drama                                    |
| SBJCA | 100071 | performing arts                          |
| SBJCA | 100078 | business and management                  |
| SBJCA | 100079 | business studies                         |
| SBJCA | 100150 | construction and the built environment   |
| SBJCA | 100184 | general or integrated engineering        |
| SBJCA | 100202 | manufacturing engineering                |
| SBJCA | 100225 | materials science                        |
| SBJCA | 100300 | classical studies                        |
| SBJCA | 100302 | history                                  |
| SBJCA | 100320 | English studies                          |
| SBJCA | 100321 | French language                          |
| SBJCA | 100323 | German language                          |
| SBJCA | 100326 | Italian language                         |
| SBJCA | 100329 | modern languages                         |
| SBJCA | 100337 | philosophy                               |
| SBJCA | 100346 | biology                                  |
| SBJCA | 100358 | applied computing                        |
| SBJCA | 100366 | computer science                         |
| SBJCA | 100372 | information technology                   |
| SBJCA | 100381 | environmental sciences                   |
| SBJCA | 100390 | general science                          |
| SBJCA | 100403 | mathematics                              |
| SBJCA | 100409 | geography                                |
| SBJCA | 100425 | physics                                  |
| SBJCA | 100444 | media and communication studies          |
| SBJCA | 100450 | economics                                |
| SBJCA | 100473 | health studies                           |
| SBJCA | 100476 | health and social care                   |
| SBJCA | 100485 | law                                      |
| SBJCA | 100510 | early years teaching                     |
| SBJCA | 100511 | primary teaching                         |
| SBJCA | 100642 | music education and teaching             |
| SBJCA | 100891 | hospitality                              |
| SBJCA | 101017 | food and beverage studies                |
| SBJCA | 101126 | classical Greek studies                  |
| SBJCA | 101142 | Portuguese language                      |
| SBJCA | 101165 | Chinese languages                        |
| SBJCA | 101169 | Japanese languages                       |
| SBJCA | 101192 | Arabic languages                         |
| SBJCA | 100050 | product design                           |
| SBJCA | 100091 | public services                          |
| SBJCA | 100092 | retail management                        |
| SBJCA | 100097 | sports management                        |
| SBJCA | 100101 | travel and tourism                       |
| SBJCA | 100209 | production and manufacturing engineering |
| SBJCA | 100214 | textiles technology                      |
| SBJCA | 100330 | Russian languages                        |
| SBJCA | 100332 | Spanish language                         |
| SBJCA | 100333 | Welsh language                           |
| SBJCA | 100339 | religious studies                        |
| SBJCA | 100343 | applied biology                          |
| SBJCA | 100406 | statistics                               |
| SBJCA | 100417 | chemistry                                |
| SBJCA | 100433 | sport and exercise sciences              |
| SBJCA | 100456 | childhood studies                        |
| SBJCA | 100471 | social sciences                          |
| SBJCA | 100497 | psychology                               |
| SBJCA | 100513 | teaching English as a foreign language   |
| SBJCA | 100610 | UK government/parliamentary studies      |
| SBJCA | 100893 | recreation and leisure studies           |
| SBJCA | 101038 | applied chemistry                        |
| SBJCA | 101060 | applied physics                          |
| SBJCA | 101117 | ancient Hebrew language                  |
| SBJCA | 101361 | creative arts and design                 |
| SBJCA | 101373 | hair and beauty sciences                 |
| SBJCA | 101410 | historical linguistics                   |
| SBJCA | 101420 | Latin language                           |

 </details>
 
 </details>
