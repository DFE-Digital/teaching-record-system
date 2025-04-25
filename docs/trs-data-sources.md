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
 
 <details>

  <summary>Legacy Subject Codes</summary>

| dfeta_name                                                 | TRS Code Type | TRS_Legacy_Code |
| ---------------------------------------------------------- | ------------- | --------------- |
| Afrikaans                                                  | Legacy        | T7007           |
| Agricultural Business Mngement                             | Legacy        | N1000           |
| Analysis Of Science and Tech.                              | Legacy        | F9604           |
| Analytical Sciences                                        | Legacy        | F9607           |
| Ancient Greek                                              | Legacy        | Q7001           |
| Ancient Greek Lang & Lit                                   | Legacy        | Q8870           |
| Ancient Oriental Studies                                   | Legacy        | Q9700           |
| Applied Educationapplied Education                         | Legacy        | X9004           |
| Applied ICT                                                | Legacy        | G510            |
| Applied Mechanics                                          | Legacy        | H3001           |
| Arabic                                                     | Legacy        | T6200           |
| Architectural Studies                                      | Legacy        | K1003           |
| Art & Crafts                                               | Legacy        | W9000           |
| Art & Media                                                | Legacy        | W2501           |
| Art and Design                                             | Legacy        | W2001           |
| Art and Design Studies                                     | Legacy        | W9923           |
| Art and Music                                              | Legacy        | W2800           |
| Art Education                                              | Legacy        | X9000           |
| Art, Craft & Design                                        | Legacy        | W2401           |
| Art,design and Technology                                  | Legacy        | W2409           |
| Arts Administration                                        | Legacy        | W9900           |
| Arts and Physical Education                                | Legacy        | W1009           |
| Arts-General (Where Subject Not Spec)                      | Legacy        | Z0079           |
| Asian Languages                                            | Legacy        | T5016           |
| B/Tec Nat Cert In Business & Finance                       | Legacy        | 11200           |
| Bed (UK)                                                   | Legacy        | Z0080           |
| Behavioural Sciences                                       | Legacy        | L7300           |
| Behavioural Studies                                        | Legacy        | L7301           |
| Bengali                                                    | Legacy        | ZZ9001          |
| Bilingual Education                                        | Legacy        | Q1411           |
| Biological Science                                         | Legacy        | C1200           |
| Biological Studies                                         | Legacy        | C1005           |
| Biology (With Science)                                     | Legacy        | C9702           |
| Biology and Science                                        | Legacy        | C9705           |
| Biology Botany                                             | Legacy        | Z0023           |
| Biology With Core Science                                  | Legacy        | C9701           |
| Broad Balanced Science                                     | Legacy        | F9608           |
| Building Construction                                      | Legacy        | K2500           |
| Building Studies                                           | Legacy        | K2001           |
| Business and Management Studies                            | Legacy        | N8810           |
| Business Education                                         | Legacy        | N1211           |
| Business Education (Tech)                                  | Legacy        | N9701           |
| Business Management Studies                                | Legacy        | N1001           |
| Business Policy                                            | Legacy        | N1205           |
| Business Studies & Info Tech                               | Legacy        | N9703           |
| C D T Incorp Tech & Info Tech                              | Legacy        | W9926           |
| C D T With Computer Science                                | Legacy        | W9909           |
| Catering & Institutional Management                        | Legacy        | N8870           |
| Ceramics Technology                                        | Legacy        | J3200           |
| Chemical Sciences                                          | Legacy        | F1001           |
| Chemical Technology                                        | Legacy        | F1600           |
| Chemistry and Science                                      | Legacy        | F9631           |
| Chemistry With Core Science                                | Legacy        | F9616           |
| Chemistry With Science                                     | Legacy        | F9615           |
| Child Developement                                         | Legacy        | X9002           |
| Child Sexual Abuse child Sexu                              | Legacy        | X9006           |
| Cinema & Film Studio Work                                  | Legacy        | W5300           |
| City and Guilds Engineering Planning, Es                   | Legacy        | 10216           |
| City and Guilds Farm Machinery                             | Legacy        | 10218           |
| Civilisation                                               | Legacy        | Q8201           |
| Classical Civilisation                                     | Legacy        | Q8200           |
| Classical Languages                                        | Legacy        | Q8101           |
| Classical Studies (In Translation)                         | Legacy        | Q8100           |
| Combd Science With Intens Phys                             | Legacy        | Y1003           |
| Combined Arts                                              | Legacy        | W9914           |
| Combined Languages                                         | Legacy        | ZZ9003          |
| Combined Science                                           | Legacy        | Y1001           |
| Combustion Science                                         | Legacy        | H8601           |
| Commerce                                                   | Legacy        | N1206           |
| Commercial Management                                      | Legacy        | N1207           |
| Communication Studies                                      | Legacy        | P8830           |
| Communications                                             | Legacy        | P3002           |
| Community Studies                                          | Legacy        | L5202           |
| Computer and Information Tech                              | Legacy        | G9009           |
| Computer Educ With Science                                 | Legacy        | G5401           |
| Computer Education                                         | Legacy        | G5400           |
| Computer Education With Maths                              | Legacy        | G5007           |
| Computer Studies                                           | Legacy        | G5000           |
| Computing and Science                                      | Legacy        | G5402           |
| Computing and Technology                                   | Legacy        | G9007           |
| Contemporary Studies                                       | Legacy        | V9000           |
| Co-Ordinated Sciences                                      | Legacy        | F9618           |
| Craft                                                      | Legacy        | W2402           |
| Craft & Design                                             | Legacy        | W2404           |
| Craft & Technology                                         | Legacy        | W2405           |
| Craft, Design & Technology                                 | Legacy        | W2403           |
| Creat Stud (Art, Move & Music)                             | Legacy        | W9916           |
| Creative Arts                                              | Legacy        | W9001           |
| Creative Arts (Art & Design)                               | Legacy        | W2002           |
| Creative Arts (Art)                                        | Legacy        | W1004           |
| Creative Arts (Dance)                                      | Legacy        | W4501           |
| Creative Arts (Drama)                                      | Legacy        | W4001           |
| Creative Arts (Music)                                      | Legacy        | W3002           |
| Creative Design                                            | Legacy        | W2003           |
| Creative Studies (Art)                                     | Legacy        | W1008           |
| Creative Studies (Music)                                   | Legacy        | W3004           |
| Creative,express.Arts(Gen)                                 | Legacy        | W3005           |
| Critical & Contextual Studies                              | Legacy        | W9921           |
| Curriculm Development In Schs                              | Legacy        | X9008           |
| Czech                                                      | Legacy        | T1400           |
| Danish                                                     | Legacy        | R7300           |
| Design & Tech (Cdt/Home Econ)                              | Legacy        | W9904           |
| Design & Tech : Home Economics                             | Legacy        | W9927           |
| Design & Tech-Food & Textiles                              | Legacy        | W9908           |
| Design (C.D.T.)                                            | Legacy        | W2406           |
| Design (General)                                           | Legacy        | W2004           |
| Design and Craft                                           | Legacy        | W9902           |
| Design and Technology Ed                                   | Legacy        | H8703           |
| Design For Technology                                      | Legacy        | W9925           |
| Design Related Activities                                  | Legacy        | W2011           |
| Design Technology                                          | Legacy        | W2505           |
| Domestic Science                                           | Legacy        | N7501           |
| Drama & Movement                                           | Legacy        | W4002           |
| Drama & Theatre Studies                                    | Legacy        | W4403           |
| Drama and Contextual Studies                               | Legacy        | W4009           |
| Drama and Education                                        | Legacy        | W9915           |
| Drama and Media Studies                                    | Legacy        | W9924           |
| Drama and Spoken English                                   | Legacy        | W4010           |
| Drama and Spoken Language                                  | Legacy        | W4011           |
| Drama With English                                         | Legacy        | W9918           |
| Drama, Music, Movement, Lit                                | Legacy        | W4012           |
| Dress & Textiles                                           | Legacy        | W2201           |
| Dyeing                                                     | Legacy        | J4603           |
| Earth Studies                                              | Legacy        | F9201           |
| Ecological Studies                                         | Legacy        | C9003           |
| Economics and Business Ed                                  | Legacy        | N9702           |
| Economics With Business Studs                              | Legacy        | L1005           |
| Economics With Social Studs                                | Legacy        | L1004           |
| Ed For Those With Sn                                       | Legacy        | X8860           |
| Ed Of The Deaf & Part. Hearing                             | Legacy        | X6003           |
| Edn.Of Childn.With Sp.Needs                                | Legacy        | X6401           |
| Education (Other Than Bed UK)                              | Legacy        | Z0093           |
| Education Of Children With Learning Difficulties           | Legacy        | X6001           |
| Education of special education needs children              | Legacy        | X6005           |
| Education Of The Deaf                                      | Legacy        | X6002           |
| Education Of The Disadvantaged                             | Legacy        | X6004           |
| Education Of The Part. Hearing                             | Legacy        | X6006           |
| Educational Computing                                      | Legacy        | X9011           |
| Educational Drama                                          | Legacy        | W4008           |
| Educational Studies                                        | Legacy        | X9003           |
| Electronics                                                | Legacy        | H6000           |
| Eng. As A 2nd Or Foreign Lang.                             | Legacy        | Z0034           |
| Engineering (General)                                      | Legacy        | H1000           |
| Engineering (Tech: Science)                                | Legacy        | H8701           |
| Engineering Science                                        | Legacy        | H1001           |
| Engineering Technology                                     | Legacy        | H1002           |
| English and Cummunications                                 | Legacy        | Q3009           |
| English and Drama                                          | Legacy        | Q3011           |
| English As A Foreign Language                              | Legacy        | Q3700           |
| English For Non-Native Speakrs                             | Legacy        | Q3004           |
| English Lang and Literature                                | Legacy        | Q9702           |
| English Linguistic Studies                                 | Legacy        | Q1002           |
| English Politics                                           | Legacy        | M1002           |
| English With Drama                                         | Legacy        | W4003           |
| English, Drama, Media Studies                              | Legacy        | Q9715           |
| Env Studies (History & Geog)                               | Legacy        | F9025           |
| Env. Stud (Geog Hist Science)                              | Legacy        | L8205           |
| Environ Science & Outdoor Stud                             | Legacy        | F9628           |
| Environmental & Social Studies                             | Legacy        | L8201           |
| Environmental Issues                                       | Legacy        | F9612           |
| Environmental Technologies                                 | Legacy        | K3400           |
| Express. Arts(Music and Drama)                             | Legacy        | W3006           |
| Expressive Arts                                            | Legacy        | W9003           |
| Expressive Arts (Art & Design)                             | Legacy        | W2006           |
| Expressive Arts (Art)                                      | Legacy        | W1005           |
| Expressive Arts (Dance)                                    | Legacy        | W4502           |
| Expressive Arts (Drama)                                    | Legacy        | W4004           |
| Expressive Arts (Music)                                    | Legacy        | W3003           |
| Expressive Arts (Physical Ed)                              | Legacy        | W9917           |
| Expressive Arts(Visual Arts)                               | Legacy        | W1505           |
| Fellow Royal Photographic Society                          | Legacy        | 10519           |
| Fine Art & Textile Design                                  | Legacy        | W2010           |
| Fine Arts                                                  | Legacy        | W1000           |
| Finnish                                                    | Legacy        | T1300           |
| Folklife Studies                                           | Legacy        | V3201           |
| Food & Nutrition                                           | Legacy        | B4003           |
| Food Science and Nutrition                                 | Legacy        | D4201           |
| Foreign & Community Languages                              | Legacy        | Q1301           |
| Foreign Languages                                          | Legacy        | Q1303           |
| French and German                                          | Legacy        | R1001           |
| French and Spanish                                         | Legacy        | Q9704           |
| French Lang and Literature                                 | Legacy        | R1103           |
| French Lang, Lit & Cult                                    | Legacy        | R8810           |
| French Language & Studies                                  | Legacy        | R1101           |
| French Politics                                            | Legacy        | M1003           |
| French Studies (In Translation)                            | Legacy        | R1100           |
| French With German                                         | Legacy        | R8203           |
| French With Italian                                        | Legacy        | R8202           |
| French With Russian                                        | Legacy        | R8205           |
| French With Spanish                                        | Legacy        | R8204           |
| Frenchlang and Contemp Studs                               | Legacy        | R1104           |
| Further Ed. Teacher Training                               | Legacy        | X5001           |
| Gaelic                                                     | Legacy        | Q5001           |
| Games and Sports                                           | Legacy        | X9020           |
| Gen Asian Lang, Lit & Cult                                 | Legacy        | T8850           |
| Gen Euro Lang, Lit & Cult                                  | Legacy        | T8820           |
| Gen Modern Languages                                       | Legacy        | T8890           |
| Gen Studies In Social Sciences                             | Legacy        | Y2000           |
| General Art and Design                                     | Legacy        | W8890           |
| General Biological Sciences                                | Legacy        | C8890           |
| General Humanities                                         | Legacy        | V8890           |
| General Language Studies                                   | Legacy        | T8880           |
| General Studies In Arts                                    | Legacy        | Y3000           |
| General Studies In Humanities                              | Legacy        | Y3200           |
| General Studies In Science                                 | Legacy        | Y1000           |
| General Technologies                                       | Legacy        | J8890           |
| General Topics In Education                                | Legacy        | X8890           |
| Geography & Environmental Stds                             | Legacy        | L8002           |
| Geography (As A Science)                                   | Legacy        | F8000           |
| Geography (Not As Physical Science)                        | Legacy        | L8000           |
| Geography (Unspecified)                                    | Legacy        | L8001           |
| Geography and The Environment                              | Legacy        | L8202           |
| Geography Studies As A Science                             | Legacy        | F8880           |
| Geography With Info Tech                                   | Legacy        | H8704           |
| Geography: As Physical Sce                                 | Legacy        | L8880           |
| German Lang, Lit & Cult                                    | Legacy        | R8820           |
| German Language & Studies                                  | Legacy        | R2103           |
| German With French                                         | Legacy        | Q9708           |
| Greek (Classical)                                          | Legacy        | Q7000           |
| Greek and Roman Civilisation                               | Legacy        | V1016           |
| Greek Civilisation                                         | Legacy        | V1006           |
| Greek History                                              | Legacy        | V1007           |
| Greek Studies                                              | Legacy        | V1008           |
| Handicraft Teachers Diploma                                | Legacy        | 10584           |
| Handicraft: City and Guilds Of London In                   | Legacy        | 211             |
| Handicraft: Other UK Quals. In Handicraf                   | Legacy        | 299             |
| Handicraft: Overseas Quals. In Handicraf                   | Legacy        | 289             |
| Health                                                     | Legacy        | B9904           |
| Health and Movement                                        | Legacy        | B9902           |
| Health Education                                           | Legacy        | B9900           |
| Hindi                                                      | Legacy        | ZZ9004          |
| Hispanic                                                   | Legacy        | R4001           |
| Hispanic Studies                                           | Legacy        | R4003           |
| Historical & Geog Studies                                  | Legacy        | V9002           |
| History and Cultural Studies                               | Legacy        | V9004           |
| History and Geography                                      | Legacy        | Z0101           |
| History and Social Studies                                 | Legacy        | V5003           |
| History and The Environment                                | Legacy        | V9003           |
| History Of Education                                       | Legacy        | X9007           |
| History, Geog & Relig Studies                              | Legacy        | Q9711           |
| Home & Community Studies                                   | Legacy        | L5204           |
| Home Economics (Design & Tech)                             | Legacy        | W9912           |
| Home Management                                            | Legacy        | N7502           |
| Home Science                                               | Legacy        | N7503           |
| Horticultural Science                                      | Legacy        | D2501           |
| Housing Studies                                            | Legacy        | N8004           |
| Human Development                                          | Legacy        | L7202           |
| Human Ecology                                              | Legacy        | C9005           |
| Human Movement and Health Stds                             | Legacy        | B9901           |
| Human Movement Studies                                     | Legacy        | W4503           |
| Human Physiology                                           | Legacy        | B1001           |
| Human Sciences                                             | Legacy        | L3405           |
| Human Studies                                              | Legacy        | L3401           |
| Icelandic                                                  | Legacy        | ZZ9005          |
| Industrial Studies                                         | Legacy        | N6100           |
| Info Technlgy/Computing                                    | Legacy        | G5602           |
| Information Science                                        | Legacy        | P2000           |
| Information Studies                                        | Legacy        | P2001           |
| Information Tech'ogy:computing                             | Legacy        | G5604           |
| Integrated Physical Sciences                               | Legacy        | F9606           |
| Integrated Science                                         | Legacy        | F9601           |
| Intergrated Studies                                        | Legacy        | F9629           |
| Irish                                                      | Legacy        | Q5300           |
| Italian Lang, Lit & Cult                                   | Legacy        | R8830           |
| Italian Language & Studies                                 | Legacy        | R3101           |
| Japanese                                                   | Legacy        | T4000           |
| Japanese Lang, Lit & Cult                                  | Legacy        | T8840           |
| Jewish Studies                                             | Legacy        | V1409           |
| Land and Property Management                               | Legacy        | N8000           |
| Language                                                   | Legacy        | Q1403           |
| Language & Literacy                                        | Legacy        | Q1404           |
| Language and Communications                                | Legacy        | Q1409           |
| Language Arts                                              | Legacy        | Q1410           |
| Language Literacy & Literature                             | Legacy        | Q1407           |
| Language Studies                                           | Legacy        | Q1400           |
| Language Studies & Philology                               | Legacy        | Q8814           |
| Language/Literature                                        | Legacy        | Q3010           |
| Latin                                                      | Legacy        | Q6000           |
| Latin Amcn Lang, Lit & Cult                                | Legacy        | R8860           |
| Latin American Languages                                   | Legacy        | R6001           |
| Leisure and Tourism                                        | Legacy        | N222            |
| Liberal Studies                                            | Legacy        | V9001           |
| Ling,lit & Cult Herit-Welsh                                | Legacy        | Q5207           |
| Literacy                                                   | Legacy        | Q1405           |
| Literary Studies                                           | Legacy        | Q2002           |
| Literature & Communic Studies                              | Legacy        | P4603           |
| Literature & Media Studies                                 | Legacy        | P4600           |
| Literature (Anglo-Irish)                                   | Legacy        | P4602           |
| Literature and Communications                              | Legacy        | P4601           |
| Literature and Drama                                       | Legacy        | Q2005           |
| Management Home Hotel & Institutio                         | Legacy        | Z0066           |
| Management In Education                                    | Legacy        | X8000           |
| Mathematical Education                                     | Legacy        | G1401           |
| Mathematical Engineering                                   | Legacy        | J9201           |
| Mathematical Physics                                       | Legacy        | F3200           |
| Mathematical Science                                       | Legacy        | G1500           |
| Mathematical Studies                                       | Legacy        | G1400           |
| Mathematics & Computer Studies                             | Legacy        | G5004           |
| Mathematics and Science                                    | Legacy        | G9005           |
| Maths and Info. Technology                                 | Legacy        | G9006           |
| Maths With Computer Science                                | Legacy        | G9003           |
| Maths.Science and Technology                               | Legacy        | G1502           |
| Maths.Stats. and Computing                                 | Legacy        | G5009           |
| Medieval Studies                                           | Legacy        | V1002           |
| Metals Technology                                          | Legacy        | J2001           |
| Metalwork                                                  | Legacy        | W6100           |
| Metalwork Engineering                                      | Legacy        | J2002           |
| Mfl(French, Spanish, German)                               | Legacy        | Q9707           |
| Micro-Computing                                            | Legacy        | G5005           |
| Minerals Estate Management                                 | Legacy        | N1002           |
| Modern and Community Langs                                 | Legacy        | T2005           |
| Modern English Studies                                     | Legacy        | Q3100           |
| Modern Foreign Languages                                   | Legacy        | R8201           |
| Modern Greek                                               | Legacy        | T2400           |
| Modern Hebrew                                              | Legacy        | ZZ9006          |
| Modern Literature                                          | Legacy        | Q2004           |
| Modern Studies                                             | Legacy        | Y3201           |
| Moral Education                                            | Legacy        | V7608           |
| Movement Studies                                           | Legacy        | W4504           |
| Movement Studies/Science                                   | Legacy        | F9630           |
| Multi-Cultural Education                                   | Legacy        | X6007           |
| Music & Instrumental Teaching                              | Legacy        | W9928           |
| Music and Drama                                            | Legacy        | W3300           |
| Music In Education                                         | Legacy        | W9919           |
| Music Studies                                              | Legacy        | W9920           |
| Natural Environmental Science                              | Legacy        | F9003           |
| Natural Philosophy                                         | Legacy        | F3001           |
| Norwegian                                                  | Legacy        | R7500           |
| Numeracy                                                   | Legacy        | G9002           |
| Office Studies                                             | Legacy        | N7601           |
| Operational Rsearch Techniques                             | Legacy        | G4500           |
| Organisation & Methods                                     | Legacy        | N2001           |
| Organisation Studies                                       | Legacy        | N1202           |
| Other                                                      | Legacy        | Z000            |
| Other Sciences                                             | Legacy        | ZZ9007          |
| Outdoor & Envir Studies                                    | Legacy        | X9018           |
| Outdoor Activities                                         | Legacy        | X2018           |
| Outdoor and Science Education                              | Legacy        | X9012           |
| Outdoor Education                                          | Legacy        | X2004           |
| Outdoor Education Studies                                  | Legacy        | X9013           |
| P E & Recreation Studies                                   | Legacy        | X9016           |
| Pedology                                                   | Legacy        | D9002           |
| Performance Arts                                           | Legacy        | W4005           |
| Personal and Social Education                              | Legacy        | ZZ9008          |
| Personal, Social and Moral Ed                              | Legacy        | L8206           |
| Personal,social and Careers Ed                             | Legacy        | X9010           |
| Personnel and Social Education                             | Legacy        | L8203           |
| Physical Education and Dance                               | Legacy        | X2017           |
| Physical Education With Dance                              | Legacy        | X2005           |
| Physical Education/Games                                   | Legacy        | X2016           |
| Physical Science                                           | Legacy        | F9602           |
| Physics (With Science)                                     | Legacy        | F9623           |
| Physics and Science                                        | Legacy        | F9632           |
| Physics With Core Science                                  | Legacy        | F9613           |
| Physics With Technology                                    | Legacy        | F6007           |
| Physics/Engineering Science                                | Legacy        | F6006           |
| Place and Society                                          | Legacy        | F9019           |
| Plant Science                                              | Legacy        | C2002           |
| Policy Making                                              | Legacy        | N1208           |
| Political Economy                                          | Legacy        | L1101           |
| Political Education                                        | Legacy        | M1004           |
| Political Science                                          | Legacy        | M1005           |
| Portuguese Lang, Lit & Cult                                | Legacy        | R8850           |
| Post-Graduate Certificate In Education()                   | Legacy        | 10886           |
| Practical Theology                                         | Legacy        | V8006           |
| Primary Curriculum                                         | Legacy        | X9005           |
| Printed Textile Design                                     | Legacy        | W2205           |
| Product Design and Technology                              | Legacy        | W9910           |
| Professional Studies                                       | Legacy        | N1209           |
| Property Surveying                                         | Legacy        | N8003           |
| Provisions 16 - 19                                         | Legacy        | X9017           |
| Psychology (Not Solely As Social S)                        | Legacy        | C8000           |
| Psychology (Solely As Social Study)                        | Legacy        | L7001           |
| Punjabi                                                    | Legacy        | ZZ9009          |
| Pure Science                                               | Legacy        | F9005           |
| Recreational Management                                    | Legacy        | X2001           |
| Recreational Studies                                       | Legacy        | X2006           |
| Religion                                                   | Legacy        | V8010           |
| Religious & Moral Education                                | Legacy        | V8008           |
| Religious & Moral Studies                                  | Legacy        | V8004           |
| Remedial Education                                         | Legacy        | X6400           |
| Resource ManagementF9020Rural & Environmental ScienceF9007 | Legacy        | N1103           |
| Rural & Env Sc (With Integ Sc)                             | Legacy        | F9622           |
| Rural & Environmental Science                              | Legacy        | F9020           |
| Rural Environmental Science                                | Legacy        | F9007           |
| Rural Science                                              | Legacy        | F9008           |
| Rural Studies                                              | Legacy        | F9010           |
| Russian Lang, Lit & Cult                                   | Legacy        | R8880           |
| Russian Language & Studies                                 | Legacy        | R8101           |
| Russian With German                                        | Legacy        | R8206           |
| Scandinavian Lang, Lit & Cult                              | Legacy        | R8870           |
| Science & Environ Studies                                  | Legacy        | F9614           |
| Science & The Environment                                  | Legacy        | F9012           |
| Science (Unspecified)                                      | Legacy        | F9603           |
| Science (With Biology)                                     | Legacy        | C9704           |
| Science and Technology                                     | Legacy        | F9605           |
| Science Education                                          | Legacy        | F9023           |
| Science In The Enviroment                                  | Legacy        | F9022           |
| Science In The Human Environment                           | Legacy        | F9013           |
| Science With Chemistry                                     | Legacy        | F9626           |
| Science With Mathematics                                   | Legacy        | G1501           |
| Science With Physics                                       | Legacy        | F9625           |
| Science With Technology                                    | Legacy        | F9619           |
| Science: Environmental Science                             | Legacy        | F9620           |
| Science:earth Science                                      | Legacy        | F9026           |
| Science-Bal Sc With Environ Sc                             | Legacy        | F9610           |
| Science-Biology-Bath Ude                                   | Legacy        | C1003           |
| Science-Chemistry-Bath Ude                                 | Legacy        | F1004           |
| Science-Geology-Bath Ude                                   | Legacy        | F6005           |
| Science-Physics-Bath Ude                                   | Legacy        | F3012           |
| Secretarial Studies                                        | Legacy        | N9700           |
| Social Administration                                      | Legacy        | L4000           |
| Social and Enviro Studies                                  | Legacy        | L3406           |
| Social and Life Skills                                     | Legacy        | G3400           |
| Social Biology                                             | Legacy        | C1900           |
| Social Education                                           | Legacy        | L3403           |
| Social Science                                             | Legacy        | L3200           |
| Social Science/Studies                                     | Legacy        | ZZ9010          |
| Spanish (And Studies)                                      | Legacy        | Z0043           |
| Spanish Lang, Lit & Cult                                   | Legacy        | R8840           |
| Spanish Language & Studies                                 | Legacy        | R4101           |
| Spanish Studies (In Translation)                           | Legacy        | R4100           |
| Spanish With French                                        | Legacy        | Q9709           |
| Special Education Studies                                  | Legacy        | X9014           |
| Speech & Drama                                             | Legacy        | W4600           |
| Speech Therapy                                             | Legacy        | B9503           |
| Speech Training                                            | Legacy        | B9504           |
| Sport                                                      | Legacy        | X2007           |
| Sport and Physical Activity                                | Legacy        | X9015           |
| Studies In Art                                             | Legacy        | W9922           |
| Studies In Geog and Society                                | Legacy        | L8004           |
| Studies In History and Soc                                 | Legacy        | V5004           |
| Studies In Humanities                                      | Legacy        | V9005           |
| Studies In Technology                                      | Legacy        | W9907           |
| Swedish                                                    | Legacy        | R7200           |
| Teach Eng -Speakers Other Lang                             | Legacy        | Q9706           |
| Teacher Training                                           | Legacy        | X8810           |
| Teaching Diploma In Speech and Drama                       | Legacy        | 612             |
| Tech (Design & Info. Tech)                                 | Legacy        | W9901           |
| Tech (Home Economics/Textiles)                             | Legacy        | W9906           |
| Tech: Business Studies                                     | Legacy        | W2504           |
| Tech:design and Technology                                 | Legacy        | W2506           |
| Technical Graphics                                         | Legacy        | W9913           |
| Technological Mathematics                                  | Legacy        | G5008           |
| Technology                                                 | Legacy        | J9001           |
| Technology (C.D.T.)                                        | Legacy        | W2407           |
| Technology (Home Economics)                                | Legacy        | H8702           |
| Technology and Mathematics                                 | Legacy        | G9004           |
| Technology Of Education                                    | Legacy        | X7000           |
| Technology With Science                                    | Legacy        | F9609           |
| Tefl/Tesl                                                  | Legacy        | Q3008           |
| Textiles & Dress                                           | Legacy        | W2207           |
| Theatre Arts                                               | Legacy        | W4402           |
| Theological Studies                                        | Legacy        | V8005           |
| Three Dimensional Design                                   | Legacy        | W2408           |
| Time,place and Society                                     | Legacy        | F9024           |
| Turkish                                                    | Legacy        | T6800           |
| Urdu                                                       | Legacy        | T5002           |
| Victorian Studies                                          | Legacy        | V1201           |
| Visual Art                                                 | Legacy        | W1500           |
| Visual Studies                                             | Legacy        | W1504           |
| Vocational English                                         | Legacy        | Q1406           |
| Voice Production                                           | Legacy        | W4006           |
| Welsh and Drama                                            | Legacy        | Q5206           |
| Welsh and Other Celtic Lang                                | Legacy        | Z0046           |
| Welsh and Welsh Studies                                    | Legacy        | Q5205           |
| Welsh As A Modern Language                                 | Legacy        | Q5201           |
| Wood Metal and Management                                  | Legacy        | W6101           |
| Writing                                                    | Legacy        | W4007           |
| Youth & Community Studies                                  | Legacy        | L5206           |

</details>

</details>

<details>
  <summary>All Provider Codes</summary>
  
| dfeta_establishmentid                | dfeta_ukprn | name                                                                                   |
|--------------------------------------|-------------|----------------------------------------------------------------------------------------|
| C2F135C7-C7AE-E311-B8ED-005056822391 | 10007823    | Edge Hill University                                                                   |
| 579A3DC1-C7AE-E311-B8ED-005056822391 | 10004180    | Manchester Metropolitan University                                                     |
| 0DF235C7-C7AE-E311-B8ED-005056822391 | 10001143    | Canterbury Christ Church University                                                    |
| 27F135C7-C7AE-E311-B8ED-005056822391 | 10007784    | University College London                                                              |
| 8B9A3DC1-C7AE-E311-B8ED-005056822391 | 10005790    | Sheffield Hallam University                                                            |
| 06F235C7-C7AE-E311-B8ED-005056822391 | 10003956    | Liverpool Hope University                                                              |
| 2F9A3DC1-C7AE-E311-B8ED-005056822391 | 10000886    | University of Brighton                                                                 |
| 656F35CD-C7AE-E311-B8ED-005056822391 | 10007776    | Roehampton University                                                                  |
| 15F135C7-C7AE-E311-B8ED-005056822391 | 10007792    | University of Exeter                                                                   |
| A3F135C7-C7AE-E311-B8ED-005056822391 | 10007842    | University of Cumbria                                                                  |
| 46F135C7-C7AE-E311-B8ED-005056822391 | 10007802    | University of Reading                                                                  |
| FEF135C7-C7AE-E311-B8ED-005056822391 | 10003863    | Leeds Trinity University                                                               |
| 24F235C7-C7AE-E311-B8ED-005056822391 | 10007843    | St Mary's University                                                                   |
| 7DF135C7-C7AE-E311-B8ED-005056822391 | 10007163    | University of Warwick                                                                  |
| 349A3DC1-C7AE-E311-B8ED-005056822391 | 10007164    | University of the West of England, Bristol                                             |
| ABF135C7-C7AE-E311-B8ED-005056822391 | 10000571    | Bath Spa University                                                                    |
| D3F035C7-C7AE-E311-B8ED-005056822391 | 10007166    | University of Wolverhampton                                                            |
| 616F35CD-C7AE-E311-B8ED-005056822391 | 10007137    | University of Chichester                                                               |
| 4E9A3DC1-C7AE-E311-B8ED-005056822391 | 10003957    | Liverpool John Moores University                                                       |
| CCF035C7-C7AE-E311-B8ED-005056822391 | 10007159    | University of Sunderland                                                               |
| 689A3DC1-C7AE-E311-B8ED-005056822391 | 10004797    | Nottingham Trent University                                                            |
| EBF035C7-C7AE-E311-B8ED-005056822391 | 10007146    | University of Greenwich                                                                |
| F0F135C7-C7AE-E311-B8ED-005056822391 | 10007145    | University of Gloucestershire                                                          |
| 289A3DC1-C7AE-E311-B8ED-005056822391 | 10007140    | Birmingham City University                                                             |
| 31F135C7-C7AE-E311-B8ED-005056822391 | 10007798    | University of Manchester                                                               |
| 686F35CD-C7AE-E311-B8ED-005056822391 | 10003614    | University of Winchester                                                               |
| 1DF135C7-C7AE-E311-B8ED-005056822391 | 10007795    | University of Leeds                                                                    |
| 09F135C7-C7AE-E311-B8ED-005056822391 | 10007788    | University of Cambridge                                                                |
| 0CF135C7-C7AE-E311-B8ED-005056822391 | 10007143    | University of Durham                                                                   |
| FFF035C7-C7AE-E311-B8ED-005056822391 | 10004351    | Middlesex University                                                                   |
| 566F35CD-C7AE-E311-B8ED-005056822391 | 10007811    | Bishop Grosseteste University                                                          |
| FAF135C7-C7AE-E311-B8ED-005056822391 | 10007713    | York St John University                                                                |
| 01F235C7-C7AE-E311-B8ED-005056822391 | 10007848    | University of Chester                                                                  |
| 6E9A3DC1-C7AE-E311-B8ED-005056822391 | 10004930    | Oxford Brookes University                                                              |
| 03F135C7-C7AE-E311-B8ED-005056822391 | 10006840    | University of Birmingham                                                               |
| 3B9A3DC1-C7AE-E311-B8ED-005056822391 | 10007147    | University of Hertfordshire                                                            |
| E9F135C7-C7AE-E311-B8ED-005056822391 | 10007832    | Newman University, Birmingham                                                          |
| 629A3DC1-C7AE-E311-B8ED-005056822391 | 10001282    | University of Northumbria At Newcastle                                                 |
| 4E6F35CD-C7AE-E311-B8ED-005056822391 | 10007851    | University of Derby                                                                    |
| 429A3DC1-C7AE-E311-B8ED-005056822391 | 10003861    | Leeds Beckett University                                                               |
| F1F035C7-C7AE-E311-B8ED-005056822391 | 10007144    | University of East London                                                              |
| 1EF135C7-C7AE-E311-B8ED-005056822391 | 10007796    | University of Leicester                                                                |
| 1CF235C7-C7AE-E311-B8ED-005056822391 | 10002718    | Goldsmiths, University of London                                                       |
| F8F035C7-C7AE-E311-B8ED-005056822391 | 10003678    | Kingston University                                                                    |
| 3AF135C7-C7AE-E311-B8ED-005056822391 | 10007154    | University of Nottingham                                                               |
| 8EF135C7-C7AE-E311-B8ED-005056822391 | 10007789    | University of East Anglia                                                              |
| 1D9A3DC1-C7AE-E311-B8ED-005056822391 | NULL        | University of Wales Institute, Cardiff                                                 |
| 4DF135C7-C7AE-E311-B8ED-005056822391 | 10007158    | University of Southampton                                                              |
| 769A3DC1-C7AE-E311-B8ED-005056822391 | 10007801    | University of Plymouth                                                                 |
| BE016A3A-CAAE-E311-B8ED-005056822391 | NULL        | St.Martins College, Lancaster ITT-923/9676                                             |
| D0F135C7-C7AE-E311-B8ED-005056822391 | 10007139    | University of Worcester                                                                |
| 1AF135C7-C7AE-E311-B8ED-005056822391 | 10007149    | University of Hull                                                                     |
| 0063D872-7939-ED11-9DB1-0022489FDDF4 | NULL        | Non-UK establishment                                                                   |
| 219A3DC1-C7AE-E311-B8ED-005056822391 | 10007138    | University of Northampton                                                              |
| 12F135C7-C7AE-E311-B8ED-005056822391 | 10007799    | University of Newcastle Upon Tyne                                                      |
| 86F135C7-C7AE-E311-B8ED-005056822391 | 10000961    | Brunel University London                                                               |
| 78F135C7-C7AE-E311-B8ED-005056822391 | 10007806    | University of Sussex                                                                   |
| 489A3DC1-C7AE-E311-B8ED-005056822391 | 10001883    | De Montfort University                                                                 |
| 07F135C7-C7AE-E311-B8ED-005056822391 | 10007786    | University of Bristol                                                                  |
| 169A3DC1-C7AE-E311-B8ED-005056822391 | 10000291    | Anglia Ruskin University                                                               |
| DEF035C7-C7AE-E311-B8ED-005056822391 | 10004048    | London Metropolitan University                                                         |
| BFF135C7-C7AE-E311-B8ED-005056822391 | NULL        | Manchester Metropolitan University, Crewe & Alsage ITT                                 |
| DDF135C7-C7AE-E311-B8ED-005056822391 | 10007148    | University of Huddersfield                                                             |
| 30D001B7-C9AE-E311-B8ED-005056822391 | NULL        | UNIVERSITY COLLEGE WORCESTER                                                           |
| 1BBA41BA-C4AE-E311-B8ED-005056822391 | NULL        | College Of St Mark & St John                                                           |
| 3AC818A5-C9AE-E311-B8ED-005056822391 | NULL        | Other EU Establishment                                                                 |
| 3847A70A-CAAE-E311-B8ED-005056822391 | 10035578    | Tes Institute                                                                          |
| 0D9A3DC1-C7AE-E311-B8ED-005056822391 | 10007152    | University of Bedfordshire                                                             |
| 40F135C7-C7AE-E311-B8ED-005056822391 | 10007774    | University of Oxford                                                                   |
| 5EF135C7-C7AE-E311-B8ED-005056822391 | 10007857    | Bangor University                                                                      |
| 866EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | University Of Central England                                                          |
| F7F135C7-C7AE-E311-B8ED-005056822391 | 10003138    | Homerton College, Cambridge                                                            |
| 34F135C7-C7AE-E311-B8ED-005056822391 | 10007767    | Keele University ITT                                                                   |
| E4F035C7-C7AE-E311-B8ED-005056822391 | 10004078    | London South Bank University                                                           |
| 1A9A3DC1-C7AE-E311-B8ED-005056822391 | 10000840    | Bradford College ITT                                                                   |
| CD560814-636E-E511-B631-005056822391 | 10024318    | Teach First                                                                            |
| 9DF135C7-C7AE-E311-B8ED-005056822391 | 10004113    | Loughborough University                                                                |
| 1187FBC8-C9AE-E311-B8ED-005056822391 | 10046623    | e-Qualitas                                                                             |
| 49F135C7-C7AE-E311-B8ED-005056822391 | 10007157    | University of Sheffield                                                                |
| 2AF135C7-C7AE-E311-B8ED-005056822391 | 10003645    | King's College London                                                                  |
| 15F235C7-C7AE-E311-B8ED-005056822391 | 10037449    | University of St Mark & St John                                                        |
| 72F135C7-C7AE-E311-B8ED-005056822391 | 10007167    | University of York                                                                     |
| 7CC718A5-C9AE-E311-B8ED-005056822391 | 10007787    | University of Buckingham                                                               |
| 95F135C7-C7AE-E311-B8ED-005056822391 | 10007850    | University of Bath                                                                     |
| B9016A3A-CAAE-E311-B8ED-005056822391 | NULL        | St.Martins College, Ambleside ITT- 909/9381                                            |
| 52F135C7-C7AE-E311-B8ED-005056822391 | NULL        | University of Wales, Aberystwyth ITT                                                   |
| 839A3DC1-C7AE-E311-B8ED-005056822391 | 10007155    | University of Portsmouth                                                               |
| B4F135C7-C7AE-E311-B8ED-005056822391 | 8           | Bretton Hall College                                                                   |
| 6AF135C7-C7AE-E311-B8ED-005056822391 | NULL        | University of Wales, Swansea ITT                                                       |
| FB993DC1-C7AE-E311-B8ED-005056822391 | 10007773    | Open University                                                                        |
| 5D6F35CD-C7AE-E311-B8ED-005056822391 | 42          | Westminster College, Oxford                                                            |
| 2C7135CD-C7AE-E311-B8ED-005056822391 | 10007846    | Swansea Institute of Higher Education                                                  |
| B9F135C7-C7AE-E311-B8ED-005056822391 | NULL        | University of Hull, Scarborough Campus ITT                                             |
| 0F7135CD-C7AE-E311-B8ED-005056822391 | NULL        | University of Wales College, Newport                                                   |
| B9E32AFB-C3AE-E311-B8ED-005056822391 | 10000840    | Bradford College                                                                       |
| 8BA748AF-C7AE-E311-B8ED-005056822391 | 10052832    | Kent and Medway Training - KMT                                                         |
| C187FBC8-C9AE-E311-B8ED-005056822391 | 10020524    | West London Partnership                                                                |
| 6D6F35CD-C7AE-E311-B8ED-005056822391 | NULL        | University of Southampton New College ITT                                              |
| 4687FBC8-C9AE-E311-B8ED-005056822391 | 10053421    | The Cambridge Partnership                                                              |
| 22F135C7-C7AE-E311-B8ED-005056822391 | 10006842    | University of Liverpool                                                                |
| C4F035C7-C7AE-E311-B8ED-005056822391 | 10006299    | Staffordshire University                                                               |
| 9C993DC1-C7AE-E311-B8ED-005056822391 | NULL        | University Of Wales ITT                                                                |
| 099A3DC1-C7AE-E311-B8ED-005056822391 | NULL        | Swansea Metropolitan University ITT                                                    |
| 1587FBC8-C9AE-E311-B8ED-005056822391 | 10035411    | Educate Group - Initial Teacher Training                                               |
| 284D4E9C-C69C-E411-A03E-005056822390 | 10006399    | Suffolk and Norfolk ITT                                                                |
| E3F135C7-C7AE-E311-B8ED-005056822391 | NULL        | University of Birmingham, Westhill ITT                                                 |
| E686FBC8-C9AE-E311-B8ED-005056822391 | 10020551    | London North Consortium                                                                |
| B9993DC1-C7AE-E311-B8ED-005056822391 | 10058590    | Bromley Schools’ Collegiate                                                            |
| 717035CD-C7AE-E311-B8ED-005056822391 | 10001726    | Coventry University                                                                    |
| F05050A9-C7AE-E311-B8ED-005056822391 | 10034789    | Gloucestershire Initial Teacher Education Partnership                                  |
| 87A748AF-C7AE-E311-B8ED-005056822391 | 10006894    | West Midlands Consortium                                                               |
| D0206F71-6E6B-E511-B631-005056822391 | 10055363    | GORSE SCITT                                                                            |
| B7016A3A-CAAE-E311-B8ED-005056822391 | NULL        | St.Martins College, Carlisle Campus ITT                                                |
| 0D87FBC8-C9AE-E311-B8ED-005056822391 | 10020548    | Essex Schools' ITT Partnership                                                         |
| DC80C283-D289-E411-A72C-005056822391 | 10037395    | United Teaching National SCITT                                                         |
| 075050A9-C7AE-E311-B8ED-005056822391 | 10007063    | Cornwall School Centred Initial Teacher Training (Cornwall SCITT)                      |
| 02D350A3-C7AE-E311-B8ED-005056822391 | 10046787    | Teaching London: LDBS SCITT                                                            |
| 55026A3A-CAAE-E311-B8ED-005056822391 | 10044534    | ARK Teacher Training                                                                   |
| 27F1421C-A53A-EE11-BDF4-000D3A675225 | 10019464    | Best Practice Network                                                                  |
| FB86FBC8-C9AE-E311-B8ED-005056822391 | 10020503    | Hertfordshire Regional Partnership                                                     |
| 57DBF3CE-C9AE-E311-B8ED-005056822391 | NULL        | Institute of Education                                                                 |
| 679BE903-520F-EE11-8F6D-0022489CE242 | 10090839    | NIoT: National Institute of Teaching                                                   |
| 2A5050A9-C7AE-E311-B8ED-005056822391 | 10032988    | SAF Initial Teacher Training                                                           |
| F453792E-CAAE-E311-B8ED-005056822391 | 10055126    | Harris Initial Teacher Education                                                       |
| 8E635531-C96C-E511-AE63-005056822390 | 10054050    | South Farnham SCITT                                                                    |
| 105050A9-C7AE-E311-B8ED-005056822391 | 10006399    | Suffolk and Norfolk Primary SCITT                                                      |
| 4E5B044D-7F79-E511-B631-005056822391 | 10054307    | Red Kite Teacher Training                                                              |
| 157135CD-C7AE-E311-B8ED-005056822391 | 10007833    | Glyndwr University                                                                     |
| E2DCF3CE-C9AE-E311-B8ED-005056822391 | 10005959    | Somerset SCITT                                                                         |
| 12F69716-CAAE-E311-B8ED-005056822391 | 10055116    | Carmel Teacher Training Partnership (CTTP)                                             |
| 675050A9-C7AE-E311-B8ED-005056822391 | 10046003    | SCITT in East London Schools (SCITTELS)                                                |
| FBD250A3-C7AE-E311-B8ED-005056822391 | 10046090    | Forest Independent Primary Collegiate SCITT                                            |
| FCCFB150-F589-E411-A72C-005056822391 | 10002327    | Essex Teacher Training                                                                 |
| DB5050A9-C7AE-E311-B8ED-005056822391 | 10060466    | Ted Wragg Teacher Training Partnership                                                 |
| BFD250A3-C7AE-E311-B8ED-005056822391 | 10059888    | Essex Primary SCITT                                                                    |
| 6ADBF3CE-C9AE-E311-B8ED-005056822391 | 10020558    | North West and Lancashire Consortium                                                   |
| 52DBF3CE-C9AE-E311-B8ED-005056822391 | 10035498    | Colchester Teacher Training Consortium                                                 |
| 348A5A7E-96E1-E311-8A4F-005056822390 | 10055370    | Chiltern Training Group                                                                |
| CF5050A9-C7AE-E311-B8ED-005056822391 | 10058882    | BEC Teacher Training                                                                   |
| D686FBC8-C9AE-E311-B8ED-005056822391 | 10013505    | The Robert Owen Consortium                                                             |
| 249A3DC1-C7AE-E311-B8ED-005056822391 | 10007816    | Central School of Speech and Drama                                                     |
| 9C9728A9-FE89-E411-A72C-005056822391 | 10059630    | OTT SCITT                                                                              |
| DA87FBC8-C9AE-E311-B8ED-005056822391 | 10055359    | Hull SCITT                                                                             |
| 59A748AF-C7AE-E311-B8ED-005056822391 | 10055198    | The Bedfordshire Schools’ Training Partnership                                         |
| 9B5050A9-C7AE-E311-B8ED-005056822391 | 10000712    | University College Birmingham                                                          |
| 80756ACF-247F-E611-9FCA-00505682090B | 10057399    | Xavier Teach SouthEast                                                                 |
| 7CF02630-008A-E411-A72C-005056822391 | 10055220    | Cambridge Training Schools Network, CTSN SCITT                                         |
| 17DEF3CE-C9AE-E311-B8ED-005056822391 | NULL        | Newman College of Higher Education Employment-based Initial Teacher Training           |
| 525050A9-C7AE-E311-B8ED-005056822391 | 10020552    | Suffolk and Norfolk Secondary SCITT                                                    |
| 7AA748AF-C7AE-E311-B8ED-005056822391 | 10059545    | North Essex Teacher Training (NETT)                                                    |
| BDDDF3CE-C9AE-E311-B8ED-005056822391 | 10059102    | Two Mile Ash ITT Partnership                                                           |
| 5C49B421-DD89-E411-A72C-005056822391 | 10055541    | The OAKS (Ormiston and Keele SCITT)                                                    |
| 74A748AF-C7AE-E311-B8ED-005056822391 | 10058864    | Mid Essex Initial Teacher Training                                                     |
| 280C8931-EC89-E411-A03E-005056822390 | 10058228    | Lincolnshire SCITT                                                                     |
| 47AB4BC3-BF9C-E411-A03E-005056822390 | 10067657    | Durham SCITT                                                                           |
| C85050A9-C7AE-E311-B8ED-005056822391 | 10046788    | Wandsworth Primary Schools’ Consortium                                                 |
| EA5050A9-C7AE-E311-B8ED-005056822391 | NULL        | Challney High School for Boys                                                          |
| E1D250A3-C7AE-E311-B8ED-005056822391 | NULL        | The Marches Consortium SCITT                                                           |
| 17F69716-CAAE-E311-B8ED-005056822391 | 10055368    | George Abbot SCITT                                                                     |
| FADDF3CE-C9AE-E311-B8ED-005056822391 | 10004926    | Oxon-Bucks Partnership                                                                 |
| 5487FBC8-C9AE-E311-B8ED-005056822391 | 10020496    | Bradford and Northern Employment-based Teacher Training                                |
| 81C718A5-C9AE-E311-B8ED-005056822391 | 10055369    | Bournemouth Bay Teacher Training Partnership                                           |
| 5C4F2C92-C589-E411-A72C-005056822391 | 10055366    | Ashton on Mersey School SCITT                                                          |
| 835050A9-C7AE-E311-B8ED-005056822391 | 10046132    | Leicester & Leicestershire SCITT                                                       |
| D5DDF3CE-C9AE-E311-B8ED-005056822391 | 10006337    | Stockton Teacher Training Partnership                                                  |
| EA4D901C-CAAE-E311-B8ED-005056822391 | 10058343    | TKAT SCITT                                                                             |
| DA86FBC8-C9AE-E311-B8ED-005056822391 | 10020554    | STORM                                                                                  |
| D3DCF3CE-C9AE-E311-B8ED-005056822391 | NULL        | University of Gloucestershire - Urban Learning Foundation                              |
| BCA8DD4C-DE89-E411-A72C-005056822391 | 10060817    | Keele and North Staffordshire Teacher Education                                        |
| C6D250A3-C7AE-E311-B8ED-005056822391 | 10058284    | Titan Partnership Ltd                                                                  |
| E25050A9-C7AE-E311-B8ED-005056822391 | 10058513    | The Grand Union Training Partnership                                                   |
| FC4F50A9-C7AE-E311-B8ED-005056822391 | NULL        | The Thames Primary Consortium                                                          |
| E6DDF3CE-C9AE-E311-B8ED-005056822391 | 10020452    | Southend Teacher Training Partnership                                                  |
| 476F35CD-C7AE-E311-B8ED-005056822391 | NULL        | University of Cumbria, Lancaster ITT                                                   |
| 0A5050A9-C7AE-E311-B8ED-005056822391 | 10045987    | Primary Catholic Partnership SCITT                                                     |
| 10887C22-8F8A-E411-A03E-005056822390 | 10047807    | Bradford Birth to 19 SCITT                                                             |
| C8386A42-E289-E411-A03E-005056822390 | 10055365    | Inspiring Leaders - Teacher Training                                                   |
| 7CE68E7E-DC89-E411-A72C-005056822391 | 10037587    | The Sheffield SCITT                                                                    |
| 163EA7F7-4546-E511-BDA8-005056822390 | 10053276    | Partnership London SCITT (PLS)                                                         |
| 8F36D784-CA6C-E511-AE63-005056822390 | 10032610    | Astra SCITT                                                                            |
| EDDCF3CE-C9AE-E311-B8ED-005056822391 | 10045988    | 2Schools Consortium                                                                    |
| AE5050A9-C7AE-E311-B8ED-005056822391 | 10048041    | Cumbria Primary Teacher Training                                                       |
| D6F135C7-C7AE-E311-B8ED-005056822391 | NULL        | University College of North Wales                                                      |
| BCB6B034-018A-E411-A72C-005056822391 | 10052835    | Shotton Hall SCITT                                                                     |
| 71AFA0C4-DE77-E511-AE63-005056822390 | 10046414    | Bright Futures SCITT                                                                   |
| 0087FBC8-C9AE-E311-B8ED-005056822391 | 10059123    | The Havering Teacher Training Partnership                                              |
| 5C8B244F-DC89-E411-A72C-005056822391 | 10046141    | The Shire Foundation                                                                   |
| 0DCC00C3-C9AE-E311-B8ED-005056822391 | 10058447    | The Deepings SCITT                                                                     |
| CD32ECD4-C9AE-E311-B8ED-005056822391 | 10020474    | Outstanding Primary Schools SCITT                                                      |
| 119A3DC1-C7AE-E311-B8ED-005056822391 | 99999999    | University of Wales Newport                                                            |
| EE86FBC8-C9AE-E311-B8ED-005056822391 | 10054033    | Kingsbridge EIP SCITT                                                                  |
| 4BDBF3CE-C9AE-E311-B8ED-005056822391 | 10034178    | Mid Somerset Consortium for Teacher Training                                           |
| 9C4DEC94-C689-E411-A72C-005056822391 | 10055360    | Associated Merseyside Partnership SCITT                                                |
| D3D250A3-C7AE-E311-B8ED-005056822391 | 10057353    | Poole SCITT                                                                            |
| 2F5050A9-C7AE-E311-B8ED-005056822391 | 10059112    | Devon Primary SCITT                                                                    |
| 7E5050A9-C7AE-E311-B8ED-005056822391 | 10004714    | North Tyneside SCITT                                                                   |
| D8FF208D-4646-E511-BDA8-005056822390 | 10034167    | Haybridge SCITT                                                                        |
| 79EB2827-4446-E511-BDA8-005056822390 | 10058211    | Five Counties SCITT                                                                    |
| 015050A9-C7AE-E311-B8ED-005056822391 | 10042780    | Portsmouth Primary SCITT                                                               |
| 1B5050A9-C7AE-E311-B8ED-005056822391 | 10029147    | London School of Jewish Studies (LSJS)                                                 |
| A48B22AC-E689-E411-A03E-005056822390 | 10055113    | Leicester and Leicestershire SCITT                                                     |
| EED250A3-C7AE-E311-B8ED-005056822391 | NULL        | The National SCITT                                                                     |
| 795050A9-C7AE-E311-B8ED-005056822391 | 10046101    | Gateshead Primary SCITT                                                                |
| 99FE12E3-6084-E511-8C28-005056822391 | 10034267    | Essex and Thames SCITT                                                                 |
| B187FBC8-C9AE-E311-B8ED-005056822391 | 10058506    | Alban Federation SCITT                                                                 |
| 9552792E-CAAE-E311-B8ED-005056822391 | 10035071    | Buckingham Partnership                                                                 |
| 44DBF3CE-C9AE-E311-B8ED-005056822391 | 10003570    | Kent County Council                                                                    |
| D4AC7134-CAAE-E311-B8ED-005056822391 | 10003692    | Kirklees and Calderdale SCITT                                                          |
| CE86FBC8-C9AE-E311-B8ED-005056822391 | 10043119    | The Merseyside, Cheshire & Greater Manchester Teacher Training Consortium              |
| 3C673D74-E489-E411-A72C-005056822391 | 10058739    | Sutton SCITT                                                                           |
| 55DBF3CE-C9AE-E311-B8ED-005056822391 | 10052834    | George Spencer Academy SCITT                                                           |
| F2DDF3CE-C9AE-E311-B8ED-005056822391 | 10005413    | Redcar and Cleveland Teacher Training Partnership                                      |
| 1C905D48-E189-E411-A72C-005056822391 | 10034759    | Arthur Terry SCITT                                                                     |
| 3F5050A9-C7AE-E311-B8ED-005056822391 | 10046049    | High Force Education SCITT                                                             |
| 955050A9-C7AE-E311-B8ED-005056822391 | 10057945    | The Learning Institute South West                                                      |
| DBAB7134-CAAE-E311-B8ED-005056822391 | 10004772    | Norfolk Teacher Training Centre                                                        |
| CF32ECD4-C9AE-E311-B8ED-005056822391 | 10003503    | North East Partnership SCITT (Physical Education)                                      |
| D1DDF3CE-C9AE-E311-B8ED-005056822391 | 10032965    | St. Joseph's College Stoke Secondary Partnership                                       |
| 7C0C0DEF-F489-E411-A72C-005056822391 | 10002008    | Doncaster ITT Partnership                                                              |
| 7C086542-EC89-E411-A72C-005056822391 | 10055362    | Nottinghamshire Torch SCITT                                                            |
| 03056A3A-CAAE-E311-B8ED-005056822391 | 10043978    | Future Teacher Training                                                                |
| 1C620117-DE89-E411-A72C-005056822391 | 10065300    | King Edward’s Consortium                                                               |
| 155050A9-C7AE-E311-B8ED-005056822391 | 10031382    | Northampton Teacher Training Partnership                                               |
| 10DEF3CE-C9AE-E311-B8ED-005056822391 | 10058342    | South Birmingham SCITT                                                                 |
| FC6A8F89-D589-E411-A72C-005056822391 | 10058582    | i2i Teaching Partnership SCITT                                                         |
| A82942C6-EC89-E411-A03E-005056822390 | 10064599    | London East Teacher Training Alliance (LETTA)                                          |
| E7D250A3-C7AE-E311-B8ED-005056822391 | 10001951    | Devon Secondary Teacher Training Group (DSTTG)                                         |
| BBDDF3CE-C9AE-E311-B8ED-005056822391 | 10020458    | Wakefield Partnership for Initial Teacher Training                                     |
| 8F5050A9-C7AE-E311-B8ED-005056822391 | 5568        | Leeds SCITT                                                                            |
| 142275E6-4346-E511-BDA8-005056822390 | 10059208    | Hillingdon SCITT                                                                       |
| D24577B7-9581-EF11-AC21-6045BDE15784 | 10095073    | Ambition Teacher Training                                                              |
| 745D2F2E-4646-E511-BDA8-005056822390 | 10058237    | Yorkshire and Humber Teacher Training                                                  |
| 2587FBC8-C9AE-E311-B8ED-005056822391 | NULL        | East Lincolnshire GTP                                                                  |
| C6DDF3CE-C9AE-E311-B8ED-005056822391 | 10006426    | Surrey LA ITT Provider                                                                 |
| FC5C2B05-D789-E411-A72C-005056822391 | 10058516    | The Tommy Flowers SCITT                                                                |
| 876BBAF4-7E79-E511-B631-005056822391 | 10059424    | Sacred Heart Newcastle SCITT                                                           |
| C5F135C7-C7AE-E311-B8ED-005056822391 | NULL        | University of Cumbria, Ambleside ITT                                                   |
| 02DEF3CE-C9AE-E311-B8ED-005056822391 | NULL        | Northumbria University                                                                 |
| EF3B5945-7939-ED11-9DB1-0022489FDDF4 | NULL        | UK establishment (Scotland/Northern Ireland)                                           |
| C2A58822-CAAE-E311-B8ED-005056822391 | 10040125    | North West SHARES SCITT                                                                |
| 07DEF3CE-C9AE-E311-B8ED-005056822391 | 10004694    | North Lincolnshire SCITT Partnership                                                   |
| BB5050A9-C7AE-E311-B8ED-005056822391 | 10026558    | Wigmore Primary                                                                        |
| 7C471BDB-237F-E611-9FCA-00505682090B | 10058966    | Manchester Nexus SCITT                                                                 |
| 865F9FFF-F38D-E511-9194-005056822391 | 10055218    | Prestolee SCITT                                                                        |
| 64DBF3CE-C9AE-E311-B8ED-005056822391 | NULL        | The University of Nottingham Partnership                                               |
| BD87FBC8-C9AE-E311-B8ED-005056822391 | 10033896    | West Berkshire Training Partnership                                                    |
| 1E6577D6-EC8A-E611-AE4B-005056822390 | 10064216    | Exceed SCITT                                                                           |
| 5C9A3DC1-C7AE-E311-B8ED-005056822391 | 10007161    | Teeside University                                                                     |
| A8766136-F58D-E511-9194-005056822391 | 10057362    | The John Taylor SCITT                                                                  |
| 9C856EEF-EC89-E411-A72C-005056822391 | 10058663    | North Wiltshire SCITT                                                                  |
| 5887FBC8-C9AE-E311-B8ED-005056822391 | 10055135    | Bourton Meadow Initial Teacher Training Centre                                         |
| E2642E87-C9AE-E311-B8ED-005056822391 | NULL        | RB trainees managed by TTA                                                             |
| 7C721C8C-E889-E411-A72C-005056822391 | 10058677    | Ripley ITT                                                                             |
| 3A5050A9-C7AE-E311-B8ED-005056822391 | 10001695    | Cornwall SCITT Partnership                                                             |
| 4B8D58AD-DBB1-E811-9B67-000D3A269589 | 10048788    | Star Teachers SCITT                                                                    |
| 009A3DC1-C7AE-E311-B8ED-005056822391 | 10005544    | Royal Academy of Dance                                                                 |
| 5F87FBC8-C9AE-E311-B8ED-005056822391 | 10020471    | Birmingham Advisory Schools Service                                                    |
| DEAB7134-CAAE-E311-B8ED-005056822391 | 10058314    | Teach East                                                                             |
| 42DAF49C-4546-E511-BDA8-005056822390 | 10053217    | GLF Schools' Teacher Training                                                          |
| A0EE93CF-D989-E411-A03E-005056822390 | 10055367    | East Midlands Teacher Training Partnership                                             |
| 88E042BB-C7AE-E311-B8ED-005056822391 | 10007768    | University Of Lancaster                                                                |
| 5FDBF3CE-C9AE-E311-B8ED-005056822391 | 10020529    | Northamptonshire, Leicester and Milton Keynes Consortium                               |
| 2D87FBC8-C9AE-E311-B8ED-005056822391 | NULL        | Dorset Teacher Education Partnership                                                   |
| A1642E87-C9AE-E311-B8ED-005056822391 | NULL        | Colchester SCITT                                                                       |
| 3287FBC8-C9AE-E311-B8ED-005056822391 | 10020506    | Doncaster GTP Partnership                                                              |
| 9717DE4D-4746-E511-BDA8-005056822390 | 10053216    | Barr Beacon SCITT                                                                      |
| 84C21271-B099-E511-926A-005056822391 | 10067433    | West Essex SCITT                                                                       |
| 5C72549C-247F-E611-9FCA-00505682090B | 10058551    | Yorkshire Wolds Teacher Training                                                       |
| 1C14E14C-D089-E411-A72C-005056822391 | 10052837    | Bluecoat SCITT Alliance Nottingham                                                     |
| EA86FBC8-C9AE-E311-B8ED-005056822391 | NULL        | The Kirklees Partnership for Employment-based Teacher Training                         |
| A0A748AF-C7AE-E311-B8ED-005056822391 | 10034818    | The Dorset Teacher Training Partnership SCITT                                          |
| D03AD5E6-C9AE-E311-B8ED-005056822391 | NULL        | Robert Owen Society SCITT                                                              |
| 1C02D75D-EF89-E411-A72C-005056822391 | 10045107    | Mersey Boroughs ITT Partnership                                                        |
| C5512FAC-0D31-E511-9AF5-005056822391 | 10058414    | Compton SCITT                                                                          |
| 9CE57488-DF89-E411-A72C-005056822391 | 10033571    | The Hampshire LEARN SCITT Partnership                                                  |
| E6AC7134-CAAE-E311-B8ED-005056822391 | 10029212    | St. George's Academy Partnership                                                       |
| DC16F33A-058A-E411-A72C-005056822391 | 10005550    | Royal Borough of Windsor and Maidenhead SCITT                                          |
| E9DCF3CE-C9AE-E311-B8ED-005056822391 | NULL        | University of Manchester Teach First                                                   |
| 5C7FAFEE-E489-E411-A72C-005056822391 | 10035849    | Sutton Park SCITT                                                                      |
| 5E4B867A-EA8A-E611-AE4B-005056822390 | 10058549    | Teach Kent & Sussex                                                                    |
| 3A87FBC8-C9AE-E311-B8ED-005056822391 | 10001437    | CILT: The National Centre for Languages                                                |
| 505B0E1B-888A-E411-A03E-005056822390 | 10046861    | Fylde Coast Teaching School SCITT                                                      |
| 5C87FBC8-C9AE-E311-B8ED-005056822391 | NULL        | Bishop Grosseteste College GTP                                                         |
| 82AEEC99-958F-E611-AA43-00505682090B | 10060171    | The National Modern Languages SCITT                                                    |
| 68CE6D98-D4A9-E711-AB15-000D3A269589 | 10058686    | National Mathematics & Physics SCITT                                                   |
| C1DDF3CE-C9AE-E311-B8ED-005056822391 | 10034809    | Thamesmead SCITT                                                                       |
| 325050A9-C7AE-E311-B8ED-005056822391 | 10020500    | Birmingham Primary SCITT                                                               |
| 86C718A5-C9AE-E311-B8ED-005056822391 | 10020543    | Durham Secondary Applied SCITT                                                         |
| 4C5050A9-C7AE-E311-B8ED-005056822391 | 10020447    | Nottingham City Primary SCITT                                                          |
| CCFB8028-CAAE-E311-B8ED-005056822391 | 10089898    | Pennine Lancashire SCITT                                                               |
| 4FDBF3CE-C9AE-E311-B8ED-005056822391 | 10005145    | Wessex Schools Training Partnership                                                    |
| E2AB7134-CAAE-E311-B8ED-005056822391 | 10058594    | Wildern Partnership                                                                    |
| 1CD7F8C0-058A-E411-A72C-005056822391 | 10046736    | NELTA                                                                                  |
| 98632E87-C9AE-E311-B8ED-005056822391 | NULL        | The East Northamptonshire Collge                                                       |
| C9CB00C3-C9AE-E311-B8ED-005056822391 | 10045996    | Tendring Hundred Primary SCITT                                                         |
| 08A969D3-768C-E611-AA43-00505682090B | 10058243    | The Coventry SCITT                                                                     |
| 0CDEF3CE-C9AE-E311-B8ED-005056822391 | 10020540    | North Bedfordshire Training Partnership                                                |
| 08D350A3-C7AE-E311-B8ED-005056822391 | 10035668    | South Coast SCITT                                                                      |
| 465050A9-C7AE-E311-B8ED-005056822391 | 10020499    | Northumbria DT Partnership SCITT                                                       |
| 6D5050A9-C7AE-E311-B8ED-005056822391 | 10020455    | Swindon SCITT                                                                          |
| 1E036A3A-CAAE-E311-B8ED-005056822391 | 10058732    | Stourport SCITT                                                                        |
| 5C82558C-C989-E411-A72C-005056822391 | 10054410    | Altius Teacher Training                                                                |
| A5056786-C8AE-E311-B8ED-005056822391 | NULL        | EM Direct                                                                              |
| 1D87FBC8-C9AE-E311-B8ED-005056822391 | 10020531    | East Sussex LEA and the University of Sussex Consortium                                |
| CC8C3F8C-D989-E411-A03E-005056822390 | 10064183    | London District East Teaching School Hub                                               |
| 310C6096-4746-E511-BDA8-005056822390 | 10058335    | East of England Teacher Training                                                       |
| FAF53239-A8A5-E611-8E8C-005056822390 | 10058690    | Huddersfield Horizon SCITT                                                             |
| 7D056786-C8AE-E311-B8ED-005056822391 | NULL        | Newman College                                                                         |
| DC191147-F689-E411-A72C-005056822391 | 10059665    | Chepping View Primary Academy SCITT                                                    |
| 64EE4655-878A-E411-A03E-005056822390 | 10048033    | Fareham and Gosport Primary SCITT                                                      |
| 6DDBF3CE-C9AE-E311-B8ED-005056822391 | NULL        | University of Bath Partnership GTP                                                     |
| F886FBC8-C9AE-E311-B8ED-005056822391 | 10034918    | Isle of Wight SCITT                                                                    |
| 039A3DC1-C7AE-E311-B8ED-005056822391 | NULL        | Glyndwr University Wrexham ITT                                                         |
| CC086CBA-4146-E511-BDA8-005056822390 | 10032342    | AA Teamworks West Yorkshire SCITT                                                      |
| B586FBC8-C9AE-E311-B8ED-005056822391 | NULL        | Trainees managed by TDA                                                                |
| A25050A9-C7AE-E311-B8ED-005056822391 | NULL        | The Urban Learning Foundation SCITT                                                    |
| 6187FBC8-C9AE-E311-B8ED-005056822391 | 10055191    | The Beauchamp ITT Partnership                                                          |
| E186FBC8-C9AE-E311-B8ED-005056822391 | 10020465    | Loughborough Encompass                                                                 |
| 7C35FEAC-EA89-E411-A72C-005056822391 | 10058215    | Pioneers Partnership SCITT                                                             |
| DEDDF3CE-C9AE-E311-B8ED-005056822391 | 10038655    | Southfields Academy teaching school SCITT                                              |
| D7AB7134-CAAE-E311-B8ED-005056822391 | 10034865    | The Basingstoke Alliance SCITT                                                         |
| EF53792E-CAAE-E311-B8ED-005056822391 | 10034549    | Three Counties Alliance SCITT                                                          |
| CDD250A3-C7AE-E311-B8ED-005056822391 | NULL        | Solihull Secondary SCITT                                                               |
| 498F33E0-D489-E411-A03E-005056822390 | 10035146    | Cheshire East SCITT                                                                    |
| 1987FBC8-C9AE-E311-B8ED-005056822391 | 10020462    | Eastwood and Leigh GTP Partnership                                                     |
| 2EDD47EE-A43A-EE11-BDF4-000D3A675225 | 10058640    | Inspiring Future Teachers                                                              |
| A7D7A842-4346-E511-BDA8-005056822390 | 10060731    | The South Downs SCITT                                                                  |
| EADDF3CE-C9AE-E311-B8ED-005056822391 | 10020553    | The Slough Partnership                                                                 |
| D55050A9-C7AE-E311-B8ED-005056822391 | 10004230    | Maryvale Institute SCITT                                                               |
| CB87FBC8-C9AE-E311-B8ED-005056822391 | 10020511    | Royal Borough of Windsor and Maidenhead Graduate Teacher Training Partnership          |
| C0056786-C8AE-E311-B8ED-005056822391 | NULL        | South West GTP Consortium                                                              |
| B9056786-C8AE-E311-B8ED-005056822391 | NULL        | North East London Partnership                                                          |
| DFB1B6D7-2D3D-EA11-A961-000D3A2AAD25 | 10006841    | University of Bolton                                                                   |
| 3CA96F6C-EC89-E411-A72C-005056822391 | 10057355    | Northern Lights SCITT                                                                  |
| 040E51AE-C4AE-E311-B8ED-005056822391 | 10031583    | Tudor Grange SCITT                                                                     |
| 29E63745-D4A9-E711-AB15-000D3A269589 | 10060032    | Inspiration Teacher Training                                                           |
| 3175EE04-4746-E511-BDA8-005056822390 | 10031352    | Lampton LWA SCITT                                                                      |
| 7CA3DB26-F589-E411-A72C-005056822391 | 10002131    | East Sussex Teacher Training Partnership                                               |
| 8D96E245-D474-EF11-A670-000D3A6884A1 | 10006399    | Norfolk, Essex and Suffolk Teacher Training (NESTT)                                    |
| 1C1A319E-ED89-E411-A72C-005056822391 | 10058252    | North Manchester ITT Partnership                                                       |
| 416AA1BA-DE1A-EC11-B6E6-000D3AB3E315 | 10058884    | EAST SCITT                                                                             |
| B4A58822-CAAE-E311-B8ED-005056822391 | 10059445    | Endeavour Learning SCITT                                                               |
| 40DBF3CE-C9AE-E311-B8ED-005056822391 | NULL        | LearnED                                                                                |
| F153792E-CAAE-E311-B8ED-005056822391 | 10032853    | Southend SCITT                                                                         |
| 735050A9-C7AE-E311-B8ED-005056822391 | NULL        | West Mercia Consortium SCITT                                                           |
| BD993DC1-C7AE-E311-B8ED-005056822391 | 9065        | Council For National Academic Awards                                                   |
| CB86FBC8-C9AE-E311-B8ED-005056822391 | NULL        | Mid-Essex ITT Consortium                                                               |
| B4993DC1-C7AE-E311-B8ED-005056822391 | 10004360    | North London Consortium                                                                |
| F54F50A9-C7AE-E311-B8ED-005056822391 | NULL        | South London Consortium SCITT                                                          |
| EC53792E-CAAE-E311-B8ED-005056822391 | 10061209    | Consilium SCITT                                                                        |
| 73642E87-C9AE-E311-B8ED-005056822391 | NULL        | Hull Citywide                                                                          |
| B7C48DC7-4646-E511-BDA8-005056822390 | 10053211    | Henry Maynard Training E17                                                             |
| B8A58822-CAAE-E311-B8ED-005056822391 | 10016462    | The Solent SCITT                                                                       |
| 6CA748AF-C7AE-E311-B8ED-005056822391 | 10006653    | The Douay Martyrs Consortium SCITT                                                     |
| D4632E87-C9AE-E311-B8ED-005056822391 | NULL        | NEECC (North East Essex Coastal Confederation)                                         |
| F420EED7-8B8A-E411-A03E-005056822390 | 10055364    | Landau Forte College Derby SCITT                                                       |
| 75E6B28E-1253-E611-89BC-00505682090B | 10059068    | Bishop’s Stortford SCITT                                                               |
| 0E86FBC8-C9AE-E311-B8ED-005056822391 | NULL        | No Establishment - Restricted by other GTC                                             |
| D259DEAA-4346-E511-BDA8-005056822390 | 10052469    | Anton Andover Alliance                                                                 |
| C1993DC1-C7AE-E311-B8ED-005056822391 | NULL        | University Of Hull ITT                                                                 |
| D186FBC8-C9AE-E311-B8ED-005056822391 | 10020525    | Matthew Moss Initial Teacher Training Partnership                                      |
| DE642E87-C9AE-E311-B8ED-005056822391 | NULL        | Gloucester Initial Teacher Education Partnership - GTP                                 |
| 98DF7E97-B08A-E411-A03E-005056822390 | 10055139    | Peninsula SCITT                                                                        |
| EEDDF3CE-C9AE-E311-B8ED-005056822391 | 10020449    | Saffron Walden and Comberton Training Schools                                          |
| 21932C7A-520F-EE11-8F6D-0022489CE242 | 10000722    | Bishop Challoner Training School                                                       |
| C25050A9-C7AE-E311-B8ED-005056822391 | 10069264    | The Oxfordshire Consortium SCITT                                                       |
| 07F12F2B-530F-EE11-8F6D-0022489CE242 | 10035500    | Teach West London Teaching School Hub                                                  |
| 77E042BB-C7AE-E311-B8ED-005056822391 | NULL        | University Of Gloucestershire ITT                                                      |
| 706F35CD-C7AE-E311-B8ED-005056822391 | 10007858    | University of Wales Trinity Saint David                                                |
| 378BE345-247F-E611-9FCA-00505682090B | 10058491    | Prince Henry's High School & South Worcestershire SCITT                                |
| C4632E87-C9AE-E311-B8ED-005056822391 | NULL        | Luton Teacher Training Partnership                                                     |
| 37DBF3CE-C9AE-E311-B8ED-005056822391 | NULL        | The Pilgrim Partnership                                                                |
| DE87FBC8-C9AE-E311-B8ED-005056822391 | 10020463    | London East Consortium, University of Cumbria                                          |
| CE632E87-C9AE-E311-B8ED-005056822391 | NULL        | Merseyside and Cheshire GTP Partnership                                                |
| 6E642E87-C9AE-E311-B8ED-005056822391 | NULL        | Yorkshire & Derbyshire Training Partnership                                            |
| 51A748AF-C7AE-E311-B8ED-005056822391 | NULL        | Newman Catholic Partnership SCITT                                                      |
| 61632E87-C9AE-E311-B8ED-005056822391 | NULL        | Agency for Jewish Education                                                            |
| 25D350A3-C7AE-E311-B8ED-005056822391 | NULL        | North London Consortium SCITT                                                          |
| 3CACE4F8-068A-E411-A72C-005056822391 | 10048061    | Swindon Secondary Schools Teaching Alliance Initial Teacher Education (SSSTA ITE)      |
| B7D250A3-C7AE-E311-B8ED-005056822391 | NULL        | The Centre for British Teachers SCITT                                                  |
| 29C818A5-C9AE-E311-B8ED-005056822391 | NULL        | Lancashire Consortium (London Satellite)                                               |
| 595050A9-C7AE-E311-B8ED-005056822391 | NULL        | London Arts Consortium SCITT                                                           |
| 7F632E87-C9AE-E311-B8ED-005056822391 | NULL        | University College Chichester                                                          |
| 7B3F8F49-C7AE-E311-B8ED-005056822391 | 10006034    | Southfields Community College                                                          |
| 64C7C048-ED8D-E511-9194-005056822391 | 10048034    | CREC Early Years Partnership                                                           |
| 56F2A694-520F-EE11-8F6D-0022489CE242 | 10055739    | Exchange Teacher Training                                                              |
| D1642E87-C9AE-E311-B8ED-005056822391 | NULL        | New Enterprise Education Consortium (NEEC)                                             |
| 8AC718A5-C9AE-E311-B8ED-005056822391 | 10020555    | Hastings and Rother SCITT                                                              |
| 21642E87-C9AE-E311-B8ED-005056822391 | NULL        | Stoke on Trent                                                                         |
| B55050A9-C7AE-E311-B8ED-005056822391 | NULL        | Woodrow Consortium SCITT                                                               |
| 895050A9-C7AE-E311-B8ED-005056822391 | 10020448    | Middlesborough SCITT                                                                   |
| A7F288FA-F418-E611-8528-00505682090B | 10064620    | Hamwic SCITT                                                                           |
| 8CE5AA49-A53A-EE11-BDF4-000D3A675225 | 10058417    | SWIFT Teacher Training                                                                 |
| 9B642E87-C9AE-E311-B8ED-005056822391 | NULL        | Poole Secondary School                                                                 |
| 75056786-C8AE-E311-B8ED-005056822391 | NULL        | Essex Advisory and Inspection Service                                                  |
| E014B4C5-DF89-E411-A03E-005056822390 | 10061002    | HART of Yorkshire                                                                      |
| 5CED7F55-C7AE-E311-B8ED-005056822391 | 10001322    | Charles Darwin School                                                                  |
| 9C9D389C-E789-E411-A72C-005056822391 | 10046628    | Services for Education SCITT                                                           |
| EA056786-C8AE-E311-B8ED-005056822391 | NULL        | Portsmouth                                                                             |
| 2B642E87-C9AE-E311-B8ED-005056822391 | NULL        | Surrey LEA Primary                                                                     |
| 99A748AF-C7AE-E311-B8ED-005056822391 | NULL        | South East Essex Consortium SCITT                                                      |
| 62A748AF-C7AE-E311-B8ED-005056822391 | 10001853    | Kent Training Group SCITT                                                              |
| 6387FBC8-C9AE-E311-B8ED-005056822391 | NULL        | Jewish Teacher Training Partnership                                                    |
| 0FD350A3-C7AE-E311-B8ED-005056822391 | NULL        | Bexley Primary Consortium Teacher Training SCITT                                       |
| BC016A3A-CAAE-E311-B8ED-005056822391 | NULL        | St.Martins College, East London                                                        |
| 2E642E87-C9AE-E311-B8ED-005056822391 | NULL        | University of Sussex Institute of Education                                            |
| 78642E87-C9AE-E311-B8ED-005056822391 | NULL        | St Martin's College East London                                                        |
| 86859443-C7AE-E311-B8ED-005056822391 | 10004360    | Mill Hill County High School                                                           |
| C5993DC1-C7AE-E311-B8ED-005056822391 | NULL        | University Of Wales, Cardiff ITT                                                       |
| 19F80BBF-520F-EE11-8F6D-0022489CE242 | 10063694    | Vantage North Humber Teacher Training                                                  |
| 8EC718A5-C9AE-E311-B8ED-005056822391 | 10020479    | South Essex, Southend and Thurrock SCITT                                               |
| BCBD6AC9-E289-E411-A72C-005056822391 | 10006197    | Teach@SJB                                                                              |
| 71993DC1-C7AE-E311-B8ED-005056822391 | 10007805    | University Of Strathclyde                                                              |
| 11642E87-C9AE-E311-B8ED-005056822391 | NULL        | Staffordshire LEA                                                                      |
| 43642E87-C9AE-E311-B8ED-005056822391 | NULL        | Wakefield                                                                              |
| 2E00A12C-908A-E411-A03E-005056822390 | 10048011    | BLT SCITT                                                                              |
| C814781F-8E8A-E411-A03E-005056822390 | 10047333    | Central England Teacher Training                                                       |
| 49BDBAA6-520F-EE11-8F6D-0022489CE242 | 10060390    | REAch Teach Primary Partnership                                                        |
| 38642E87-C9AE-E311-B8ED-005056822391 | NULL        | Titan Partnership Secondary SCITT                                                      |
| BD896BFD-C6AE-E311-B8ED-005056822391 | 10016063    | Top Valley School                                                                      |
| C73273F7-C6AE-E311-B8ED-005056822391 | 10000596    | The Beauchamp College                                                                  |
| E3632E87-C9AE-E311-B8ED-005056822391 | NULL        | North Lincolnshire Initial Teacher Training                                            |
| FB632E87-C9AE-E311-B8ED-005056822391 | NULL        | Redcar & Cleveland LEA                                                                 |
| FCAB9F58-EB89-E411-A72C-005056822391 | 10052838    | Perry Beeches SCITT                                                                    |
| 63632E87-C9AE-E311-B8ED-005056822391 | NULL        | Anglia Polytechnic University                                                          |
| B4632E87-C9AE-E311-B8ED-005056822391 | NULL        | Isle of Wight Partnership                                                              |
| FA5E6380-A43A-EE11-BDF4-000D3A675225 | 10060518    | Embrace SCITT                                                                          |
| AB632E87-C9AE-E311-B8ED-005056822391 | NULL        | Hazelwick School, Crawley (West Sussex)                                                |
| D2632E87-C9AE-E311-B8ED-005056822391 | NULL        | Mid Essex GTP Consortium                                                               |
| 4D71C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Bourton Meadow School                                                                  |
| 36E8A26C-C4AE-E311-B8ED-005056822391 | 10037252    | The Arthur Terry School                                                                |
| 62993DC1-C7AE-E311-B8ED-005056822391 | 10007790    | University Of Edinburgh                                                                |
| BA632E87-C9AE-E311-B8ED-005056822391 | NULL        | Kingsbridge NLC (Wigan)                                                                |
| 14016A3A-CAAE-E311-B8ED-005056822391 | 10020466    | LearnED                                                                                |
| BA0C4E47-C2AE-E311-B8ED-005056822391 | 10031350    | Kemnal Technology College                                                              |
| 05642E87-C9AE-E311-B8ED-005056822391 | NULL        | South East Midlands GTP Partnership                                                    |
| D2993DC1-C7AE-E311-B8ED-005056822391 | NULL        | University Of London                                                                   |
| 1B642E87-C9AE-E311-B8ED-005056822391 | NULL        | Stockton-on-Tess LEA                                                                   |
| 5D642E87-C9AE-E311-B8ED-005056822391 | NULL        | Windsor & Maidenhead                                                                   |
| 34642E87-C9AE-E311-B8ED-005056822391 | NULL        | Thamesmead School                                                                      |
| 7B849443-C7AE-E311-B8ED-005056822391 | 10001306    | Chalvedon School and Sixth Form College                                                |
| 8184D3A6-C2AE-E311-B8ED-005056822391 | 10053880    | Rushey Mead Academy                                                                    |
| 3CA0FAF6-E289-E411-A72C-005056822391 | 10005647    | Teach@salesian                                                                         |
| 68993DC1-C7AE-E311-B8ED-005056822391 | 10007794    | University Of Glasgow                                                                  |
| DB632E87-C9AE-E311-B8ED-005056822391 | NULL        | Ninestiles School                                                                      |
| 90993DC1-C7AE-E311-B8ED-005056822391 | 10005343    | Queen's University Belfast                                                             |
| 9097A035-A984-E611-9901-005056822390 | 10061212    | Teach Northants                                                                        |
| 3D107C05-530F-EE11-8F6D-0022489CE242 | 10064251    | Scarborough Teaching Alliance                                                          |
| F7FE7261-C7AE-E311-B8ED-005056822391 | 10015528    | William Edwards School                                                                 |
| CA632E87-C9AE-E311-B8ED-005056822391 | NULL        | Matthew Moss High School                                                               |
| 3C6A053C-E789-E411-A72C-005056822391 | 10055361    | South Cumbria SCITT                                                                    |
| E0CC9BEE-4373-E511-AE63-005056822390 | NULL        | Tauheedul Future Teachers                                                              |
| A2993DC1-C7AE-E311-B8ED-005056822391 | NULL        | Stranmillis University College ITT                                                     |
| D9DDF3CE-C9AE-E311-B8ED-005056822391 | 10020486    | Stockport Teacher Training Partnership                                                 |
| 3BEE7F55-C7AE-E311-B8ED-005056822391 | 10004641    | Ninestiles School                                                                      |
| 2E3073F7-C6AE-E311-B8ED-005056822391 | 10005813    | Shoeburyness High School                                                               |
| A7642E87-C9AE-E311-B8ED-005056822391 | NULL        | George Spencer Training School                                                         |
| 1291540F-C7AE-E311-B8ED-005056822391 | 10006468    | Sydenham School                                                                        |
| 30F63F21-C7AE-E311-B8ED-005056822391 | 10006224    | St Mary's College                                                                      |
| A7993DC1-C7AE-E311-B8ED-005056822391 | NULL        | St Mary's University College ITT                                                       |
| 17642E87-C9AE-E311-B8ED-005056822391 | NULL        | Stockport Metropolitan Borough Council                                                 |
| C240BEF8-C9AE-E311-B8ED-005056822391 | NULL        | Leeds Metropolitan University Teach First                                              |
| 24E06303-C7AE-E311-B8ED-005056822391 | 10015556    | Greendown School                                                                       |
| 1CE64C15-C7AE-E311-B8ED-005056822391 | 10016893    | Mayfield School                                                                        |
| 7A993DC1-C7AE-E311-B8ED-005056822391 | NULL        | University Of Aberdeen ITT                                                             |
| 981A7067-C7AE-E311-B8ED-005056822391 | 10001242    | Cecil Jones High School                                                                |
| 1D91540F-C7AE-E311-B8ED-005056822391 | 10007617    | Woolston High School                                                                   |
| D63F8F49-C7AE-E311-B8ED-005056822391 | 10006778    | The Philip Morant School and College                                                   |
| E73D451B-C7AE-E311-B8ED-005056822391 | 10005333    | Queen Elizabeth's School                                                               |
| 66FF7261-C7AE-E311-B8ED-005056822391 | 10006640    | Colne Community School                                                                 |
| 5652EEB0-A43A-EE11-BDF4-000D3A675225 | 10058240    | One Cumbria                                                                            |
| C8993DC1-C7AE-E311-B8ED-005056822391 | NULL        | University College Cardiff                                                             |
| 53E44C15-C7AE-E311-B8ED-005056822391 | 10007919    | Crown Hills Community School                                                           |
| 3BBBED97-C6AE-E311-B8ED-005056822391 | 10046003    | Colegrave Primary School                                                               |
| 1E385C09-C7AE-E311-B8ED-005056822391 | 10015190    | David Lister School                                                                    |
| F83E8F49-C7AE-E311-B8ED-005056822391 | 10005388    | Ravens Wood School                                                                     |
| 0E11E69D-C6AE-E311-B8ED-005056822391 | 10045988    | Oakthorpe Primary School                                                               |
| C43B451B-C7AE-E311-B8ED-005056822391 | 10005489    | Rivington & Blackrod High School                                                       |
| ADF73F21-C7AE-E311-B8ED-005056822391 | 10006289    | St Pauls Catholic                                                                      |
| 30A87A5B-C7AE-E311-B8ED-005056822391 | 10006785    | The Lincoln School of Science and Technology                                           |
| 4A385C09-C7AE-E311-B8ED-005056822391 | 10002981    | Heath Park School                                                                      |
| 833E451B-C7AE-E311-B8ED-005056822391 | NULL        | Carmel Technology College                                                              |
| 1D375C09-C7AE-E311-B8ED-005056822391 | 10004417    | Monks Walk School                                                                      |
| 5C993DC1-C7AE-E311-B8ED-005056822391 | 10007160    | University Of Surrey                                                                   |
| E08082EB-C6AE-E311-B8ED-005056822391 | 10014900    | Winifred Holtby School & Technollogy College                                           |
| D796874F-C7AE-E311-B8ED-005056822391 | 10001885    | Deacon's School                                                                        |
| E41A5791-C7AE-E311-B8ED-005056822391 | 10013345    | Emmanuel College                                                                       |
| 88993DC1-C7AE-E311-B8ED-005056822391 | 10007804    | University Of Stirling                                                                 |
| 58E54C15-C7AE-E311-B8ED-005056822391 | 10016512    | North Manchester High School                                                           |
| B939AFF9-C1AE-E311-B8ED-005056822391 | 10007608    | Woodham Academy                                                                        |
| 3ADF6303-C7AE-E311-B8ED-005056822391 | 10006931    | Toll Bar School                                                                        |
| 71DB7AF1-C6AE-E311-B8ED-005056822391 | 10015342    | Ellis Guilford                                                                         |
| A78F540F-C7AE-E311-B8ED-005056822391 | 10005666    | Sandringham School                                                                     |
| C364F591-C6AE-E311-B8ED-005056822391 | 10053211    | Henry Maynard Infants School                                                           |
| 353F8F49-C7AE-E311-B8ED-005056822391 | NULL        | Sharnbrook Upper School                                                                |
| 1F5050A9-C7AE-E311-B8ED-005056822391 | NULL        | Gatsby SCITT                                                                           |
| 7A1A7067-C7AE-E311-B8ED-005056822391 | 10017370    | Sir Charles Lucas School                                                               |
| 58876BFD-C6AE-E311-B8ED-005056822391 | 10007560    | Withernsea High School                                                                 |
| D196874F-C7AE-E311-B8ED-005056822391 | NULL        | Poole High School                                                                      |
| 0DE64C15-C7AE-E311-B8ED-005056822391 | 10016165    | King Richard School                                                                    |
| 93A748AF-C7AE-E311-B8ED-005056822391 | NULL        | Brooke Weston CTC SCITT                                                                |
| 983273F7-C6AE-E311-B8ED-005056822391 | 10014990    | Bexhill High                                                                           |
| 398282EB-C6AE-E311-B8ED-005056822391 | 10015982    | The Sweyne Park School                                                                 |
| 8F2D8AE5-C6AE-E311-B8ED-005056822391 | 10015654    | Hall Mead School                                                                       |
| F52C8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Bjps Cassel Fox                                                                        |
| 3D35ACE7-A33A-EE11-BDF4-000D3A675225 | 10064234    | Mulberry College of Teaching                                                           |
| 19B4069E-99AE-E311-8A4F-005056822390 | 99999999    | Other UK                                                                               |
| EB5F5D3B-C2AE-E311-B8ED-005056822391 | 10016462    | Park Community School                                                                  |
| 403D451B-C7AE-E311-B8ED-005056822391 | 10006290    | St Peters High                                                                         |
| 327135CD-C7AE-E311-B8ED-005056822391 | NULL        | Trinity College, Carmarthen                                                            |
| 8C0A6485-C7AE-E311-B8ED-005056822391 | 10015538    | Hampton School                                                                         |
| F6D250A3-C7AE-E311-B8ED-005056822391 | NULL        | National Secondary Music Group Access To Music SCITT                                   |
| AFDA7AF1-C6AE-E311-B8ED-005056822391 | 10016313    | Langdon School                                                                         |
| 95993DC1-C7AE-E311-B8ED-005056822391 | 10007807    | University Of Ulster                                                                   |
| 6AC7BFBB-C6AE-E311-B8ED-005056822391 | 10077177    | Millfields Community School                                                            |
| 24ED7F55-C7AE-E311-B8ED-005056822391 | 10003139    | Homewood School and Sixth Form Centre                                                  |
| 5A447FE8-C2AE-E311-B8ED-005056822391 | NULL        | Cramlington Learning Village                                                           |
| D93F8F49-C7AE-E311-B8ED-005056822391 | 10001911    | Denefield School                                                                       |
| CA3D451B-C7AE-E311-B8ED-005056822391 | 10002663    | George Green's School                                                                  |
| F0DB7AF1-C6AE-E311-B8ED-005056822391 | 10018010    | Brampton Manor School                                                                  |
| 7DA77A5B-C7AE-E311-B8ED-005056822391 | 10000722    | Bishop Challoner Catholic School                                                       |
| E01A5791-C7AE-E311-B8ED-005056822391 | NULL        | Dixons City Technology College                                                         |
| F38082EB-C6AE-E311-B8ED-005056822391 | 10001068    | Buttershaw Business and Enterprise College                                             |
| DA3173F7-C6AE-E311-B8ED-005056822391 | 10007994    | Ringmer Community School                                                               |
| 708282EB-C6AE-E311-B8ED-005056822391 | 10006024    | Southborough School                                                                    |
| 33375C09-C7AE-E311-B8ED-005056822391 | 10015586    | The Wildern School                                                                     |
| 76365C09-C7AE-E311-B8ED-005056822391 | 10000442    | Astor School                                                                           |
| C8849443-C7AE-E311-B8ED-005056822391 | 10001678    | Coopers School                                                                         |
| 1BFF7261-C7AE-E311-B8ED-005056822391 | 10006153    | St Edmunds Catholic School                                                             |
| A9365C09-C7AE-E311-B8ED-005056822391 | 10001896    | Deansfield High School                                                                 |
| 5690540F-C7AE-E311-B8ED-005056822391 | 10006709    | Byng Kenrick Central Schooland Sixth Form Centre                                       |
| C08182EB-C6AE-E311-B8ED-005056822391 | 10015547    | Endeavor High School                                                                   |
| 86FE7261-C7AE-E311-B8ED-005056822391 | 10006621    | The Canterbury High                                                                    |
| DA993DC1-C7AE-E311-B8ED-005056822391 | NULL        | South Glamorgan Institute Of Higher Education                                          |
| 4BE64C15-C7AE-E311-B8ED-005056822391 | 10016269    | Kings' School, Winchester                                                              |
| 3BD97AF1-C6AE-E311-B8ED-005056822391 | 10016032    | Thomas Lord Andley School                                                              |
| 903E8F49-C7AE-E311-B8ED-005056822391 | 10006781    | Plume School                                                                           |
| 16A87A5B-C7AE-E311-B8ED-005056822391 | 10006658    | The Eastwood School (11-18)                                                            |
| 2196874F-C7AE-E311-B8ED-005056822391 | 10003977    | Lodge Park Technology College                                                          |
| 56A383C1-A33A-EE11-BDF4-000D3A675225 | 10002412    | University Centre Farnborough                                                          |
| AF536A73-C7AE-E311-B8ED-005056822391 | NULL        | Oundle School                                                                          |
| E4D97AF1-C6AE-E311-B8ED-005056822391 | 10007565    | Wodenborough Technology College                                                        |
| E8FE7261-C7AE-E311-B8ED-005056822391 | NULL        | Greensward College                                                                     |
| 83E44C15-C7AE-E311-B8ED-005056822391 | 10016065    | Islington Green School                                                                 |
| FE3273F7-C6AE-E311-B8ED-005056822391 | 10014845    | Aylsham High School                                                                    |
| DD96874F-C7AE-E311-B8ED-005056822391 | 10006614    | Bromfords School                                                                       |
| 1DF69716-CAAE-E311-B8ED-005056822391 | 10033631    | Surrey South Farneham SCITT                                                            |
| 79ED7F55-C7AE-E311-B8ED-005056822391 | 10006203    | St Joseph's Catholic Comprehensive School, Swindon                                     |
| 0E8E540F-C7AE-E311-B8ED-005056822391 | 10015354    | Glenmoor School                                                                        |
| 21365C09-C7AE-E311-B8ED-005056822391 | 10004757    | The Northicote School                                                                  |
| 03365C09-C7AE-E311-B8ED-005056822391 | 10003262    | Imberhorne School                                                                      |
| 658282EB-C6AE-E311-B8ED-005056822391 | 10015750    | Hereford Technology School                                                             |
| E1FE7261-C7AE-E311-B8ED-005056822391 | 10002776    | Greensward College                                                                     |
| EF2F73F7-C6AE-E311-B8ED-005056822391 | 10007482    | White Hart Lane School                                                                 |
| 8C96874F-C7AE-E311-B8ED-005056822391 | NULL        | Dartford Grammar School                                                                |
| 4CA77A5B-C7AE-E311-B8ED-005056822391 | NULL        | Fullbrook School                                                                       |
| 93632E87-C9AE-E311-B8ED-005056822391 | NULL        | Hexham & Newcastle Diocese Catholic Partnership (South)                                |
| 82E042BB-C7AE-E311-B8ED-005056822391 | 10007800    | The University of the West of Scotland                                                 |
| 6C859443-C7AE-E311-B8ED-005056822391 | 10010342    | Watford Grammar School                                                                 |
| 74DA7AF1-C6AE-E311-B8ED-005056822391 | 10005664    | Sandown High School                                                                    |
| 17ED7F55-C7AE-E311-B8ED-005056822391 | 10005918    | Slough Grammar School                                                                  |
| 983E8F49-C7AE-E311-B8ED-005056822391 | 10006644    | Cornwallis School                                                                      |
| E58082EB-C6AE-E311-B8ED-005056822391 | 10014850    | Eston Park School                                                                      |
| 90E06303-C7AE-E311-B8ED-005056822391 | 10006665    | The Ferrers School                                                                     |
| B03B451B-C7AE-E311-B8ED-005056822391 | 10007515    | William Parker School                                                                  |
| 29E16303-C7AE-E311-B8ED-005056822391 | 10001687    | Corby Community College                                                                |
| 2D385C09-C7AE-E311-B8ED-005056822391 | 10004986    | Parkfield High School                                                                  |
| 968182EB-C6AE-E311-B8ED-005056822391 | 10003272    | Impington Village College                                                              |
| 4F1A7067-C7AE-E311-B8ED-005056822391 | 10005799    | Shenfield High School                                                                  |
| 8E3F8F49-C7AE-E311-B8ED-005056822391 | 10004625    | Newtead Wood School for Girls                                                          |
| EBA67A5B-C7AE-E311-B8ED-005056822391 | 10004391    | Minster College                                                                        |
| 073E451B-C7AE-E311-B8ED-005056822391 | 10004127    | Lutterworth Grammar School                                                             |
| EFD87AF1-C6AE-E311-B8ED-005056822391 | 10002751    | Great Cornard Upper School                                                             |
| 9F95874F-C7AE-E311-B8ED-005056822391 | 10006217    | St Martins In The Field Girls School                                                   |
| 6B3F8F49-C7AE-E311-B8ED-005056822391 | 10002705    | Glyn Adt Technology School                                                             |
| 852F73F7-C6AE-E311-B8ED-005056822391 | NULL        | Whitesmore School                                                                      |
| E5ED7F55-C7AE-E311-B8ED-005056822391 | 10005258    | Prospect College                                                                       |
| 0D96874F-C7AE-E311-B8ED-005056822391 | 10006037    | Southlands School                                                                      |
| 1F3C451B-C7AE-E311-B8ED-005056822391 | 10004244    | Matthew Humberstone C of E School                                                      |
| 8C8E540F-C7AE-E311-B8ED-005056822391 | 10004839    | Oakmead College                                                                        |
| 8CC9BFBB-C6AE-E311-B8ED-005056822391 | 10018900    | South Farnham Community Junior School                                                  |
| 4EDF6303-C7AE-E311-B8ED-005056822391 | NULL        | Samuel Whitbread                                                                       |
| 053373F7-C6AE-E311-B8ED-005056822391 | 10001917    | Deptford Green School                                                                  |
| EB993DC1-C7AE-E311-B8ED-005056822391 | NULL        | Cardiff Institute Of Higher Education ITT                                              |
| A1DB7AF1-C6AE-E311-B8ED-005056822391 | 10001711    | Coundon Court Secondary School                                                         |
| C196874F-C7AE-E311-B8ED-005056822391 | 10004291    | The Douay Martyrs Consortium.                                                          |
| 803F8F49-C7AE-E311-B8ED-005056822391 | 10002458    | Finchley Catholic High School                                                          |
| DDFE7261-C7AE-E311-B8ED-005056822391 | 10000619    | Belfairs High School                                                                   |
| 280EFD8B-C6AE-E311-B8ED-005056822391 | 10072977    | Westfield Junior School                                                                |
| FA849443-C7AE-E311-B8ED-005056822391 | 10005920    | Small Heath School                                                                     |
| 66E54C15-C7AE-E311-B8ED-005056822391 | 10006636    | City of Leicester School                                                               |
| 7F615C8B-C7AE-E311-B8ED-005056822391 | 10013989    | Immanuel College                                                                       |
| 9996874F-C7AE-E311-B8ED-005056822391 | 10004191    | Manor School                                                                           |
| D73273F7-C6AE-E311-B8ED-005056822391 | 10015544    | Harper Green School                                                                    |
| ABFE7261-C7AE-E311-B8ED-005056822391 | 10007442    | Westcliffe High School                                                                 |
| D9365C09-C7AE-E311-B8ED-005056822391 | 10005812    | Shireland Language College                                                             |
| 96642E87-C9AE-E311-B8ED-005056822391 | NULL        | North Hampshire Primary Partnership                                                    |
| C63F8F49-C7AE-E311-B8ED-005056822391 | 10001325    | Charlton School                                                                        |
| 4EB03A27-C7AE-E311-B8ED-005056822391 | NULL        | Two Mile Ash Middle School                                                             |
| EBDB7AF1-C6AE-E311-B8ED-005056822391 | 10004763    | Northumberland Park Community School                                                   |
| 0A3F451B-C7AE-E311-B8ED-005056822391 | 10004782    | Notre Dame High School                                                                 |
| 82E64C15-C7AE-E311-B8ED-005056822391 | 10000875    | Brigemary Community                                                                    |
| 6BE06303-C7AE-E311-B8ED-005056822391 | 10001314    | Chantry High School and Sixth Form Centre                                              |
| 6DE54C15-C7AE-E311-B8ED-005056822391 | 10016917    | Millbrook Community School                                                             |
| D2D291DF-C6AE-E311-B8ED-005056822391 | 10073738    | Bury & Whitefield Jewish Primary School                                                |
| 11FF7261-C7AE-E311-B8ED-005056822391 | 10016633    | St Clere's School                                                                      |
| 57E06303-C7AE-E311-B8ED-005056822391 | 10004756    | Northgate High                                                                         |
| C6E74C15-C7AE-E311-B8ED-005056822391 | 10015046    | Woodlands School                                                                       |
| A1DA7AF1-C6AE-E311-B8ED-005056822391 | 10015339    | Dormers Wells School                                                                   |
| 8BDB7AF1-C6AE-E311-B8ED-005056822391 | 10014930    | Airedale Academy                                                                       |
| 248A6BFD-C6AE-E311-B8ED-005056822391 | NULL        | The Mcentee School                                                                     |
| 1FED7F55-C7AE-E311-B8ED-005056822391 | 10005625    | Saffron Walden High School                                                             |
| 648482EB-C6AE-E311-B8ED-005056822391 | 10017833    | Norden High School                                                                     |
| DDE44C15-C7AE-E311-B8ED-005056822391 | 10017340    | Sir Henry Cooper                                                                       |
| F1ED7F55-C7AE-E311-B8ED-005056822391 | 10006307    | Stanground College                                                                     |
| 873073F7-C6AE-E311-B8ED-005056822391 | 10006801    | The Ridings School                                                                     |
| 99FE7261-C7AE-E311-B8ED-005056822391 | 10006248    | St Peter & St Paul Catholic High School                                                |
| 998F540F-C7AE-E311-B8ED-005056822391 | 10017225    | Hollins High School                                                                    |
| E1D291DF-C6AE-E311-B8ED-005056822391 | NULL        | Christchurch CE                                                                        |
| 92365C09-C7AE-E311-B8ED-005056822391 | 10000346    | Archers Court School                                                                   |
| 2BDF6303-C7AE-E311-B8ED-005056822391 | 10018163    | Temple School                                                                          |
| 19F83F21-C7AE-E311-B8ED-005056822391 | NULL        | Yesoiday Hatorah School                                                                |
| EC95874F-C7AE-E311-B8ED-005056822391 | 10004102    | Lord Grey School                                                                       |
| C0DE6303-C7AE-E311-B8ED-005056822391 | 10015197    | Brumby Comprehensive School                                                            |
| 15D350A3-C7AE-E311-B8ED-005056822391 | NULL        | Association of Muslim Schools SCITT                                                    |
| 8ADA7AF1-C6AE-E311-B8ED-005056822391 | 10000139    | Adeyfield                                                                              |
| 4AAF3A27-C7AE-E311-B8ED-005056822391 | 10003617    | King David High School                                                                 |
| 1A1B7067-C7AE-E311-B8ED-005056822391 | 10016256    | The Saint Christopher School                                                           |
| 6D375C09-C7AE-E311-B8ED-005056822391 | 10004899    | Ormesby Comprehensive School                                                           |
| 96896BFD-C6AE-E311-B8ED-005056822391 | NULL        | The Earl of Scarbrough High School                                                     |
| B63E451B-C7AE-E311-B8ED-005056822391 | 10003644    | King Solomon High School                                                               |
| AD886BFD-C6AE-E311-B8ED-005056822391 | 10006579    | Pilton Community College                                                               |
| F90C6579-C7AE-E311-B8ED-005056822391 | 10010048    | Marlborough College                                                                    |
| 44B03A27-C7AE-E311-B8ED-005056822391 | 10076285    | Holy Trinity CE Junior School                                                          |
| 82859443-C7AE-E311-B8ED-005056822391 | 10016208    | The St Thomas the Apostle College                                                      |
| 921A7067-C7AE-E311-B8ED-005056822391 | 10006828    | The Thomas Aveling School                                                              |
| 1EA87A5B-C7AE-E311-B8ED-005056822391 | 10015100    | Castle View School                                                                     |
| 34859443-C7AE-E311-B8ED-005056822391 | NULL        | Manshead School                                                                        |
| 13A87A5B-C7AE-E311-B8ED-005056822391 | 10002598    | Fulston Manor School                                                                   |
| C5A77A5B-C7AE-E311-B8ED-005056822391 | 10000215    | All Hallows Catholic School                                                            |
| B6BAED97-C6AE-E311-B8ED-005056822391 | NULL        | Grange Park Primary School                                                             |
| CD8182EB-C6AE-E311-B8ED-005056822391 | NULL        | Stratton Upper School                                                                  |
| FAF73F21-C7AE-E311-B8ED-005056822391 | 10005863    | Sir John Cass Foundation and Redcoat Church of England Secondary School                |
| DFE64C15-C7AE-E311-B8ED-005056822391 | 10001380    | Chestnut Grove School                                                                  |
| DA9F9F10-CAAE-E311-B8ED-005056822391 | NULL        | Tudor Grange SCITT                                                                     |
| E8E16303-C7AE-E311-B8ED-005056822391 | NULL        | Westfield Community School                                                             |
| FAD97AF1-C6AE-E311-B8ED-005056822391 | 10002927    | Hatfield Visual Arts College                                                           |
| 9CD97AF1-C6AE-E311-B8ED-005056822391 | 10006619    | The Calder Learning Trust                                                              |
| 165F0C80-C6AE-E311-B8ED-005056822391 | 10072859    | Brampton Primary School                                                                |
| 76DE6303-C7AE-E311-B8ED-005056822391 | 10013766    | Haven Hill School                                                                      |
| AAE44C15-C7AE-E311-B8ED-005056822391 | 10001344    | Chatham South School                                                                   |
| BE3F8F49-C7AE-E311-B8ED-005056822391 | 10004993    | Parmiter's School                                                                      |
| 3DE54C15-C7AE-E311-B8ED-005056822391 | 10007892    | Babington Community Technology College                                                 |
| 85385C09-C7AE-E311-B8ED-005056822391 | 10004428    | Moreton Community School                                                               |
| EDC5BFBB-C6AE-E311-B8ED-005056822391 | 10046132    | Dovelands Primary School                                                               |
| EBF53F21-C7AE-E311-B8ED-005056822391 | 10015284    | De La Salle                                                                            |
| CF375C09-C7AE-E311-B8ED-005056822391 | 10001683    | Coppice High School                                                                    |
| 8E8282EB-C6AE-E311-B8ED-005056822391 | NULL        | Northfields Upper School                                                               |
| F5DF6303-C7AE-E311-B8ED-005056822391 | 10005857    | Sir Frank Markham Community School                                                     |
| 82C8BFBB-C6AE-E311-B8ED-005056822391 | 10074668    | Oxford Gardens School                                                                  |
| D2F53F21-C7AE-E311-B8ED-005056822391 | 10000733    | Bishop Vesey's Grammar School                                                          |
| 3AF63F21-C7AE-E311-B8ED-005056822391 | 10014939    | Bishop Rawstone C of E Langauge College                                                |
| 5D859443-C7AE-E311-B8ED-005056822391 | NULL        | George Spencer School                                                                  |
| 38E06303-C7AE-E311-B8ED-005056822391 | 10016892    | Matthew Moss High School                                                               |
| 05E84C15-C7AE-E311-B8ED-005056822391 | 10014925    | Andrew Marvell School                                                                  |
| 7B8182EB-C6AE-E311-B8ED-005056822391 | 10016182    | Kingswoood High School                                                                 |
| DB2F73F7-C6AE-E311-B8ED-005056822391 | 10006913    | Tile Hill Wood                                                                         |
| 61DF6303-C7AE-E311-B8ED-005056822391 | 10004092    | Longdean School                                                                        |
| 973C451B-C7AE-E311-B8ED-005056822391 | 10003652    | Kings College                                                                          |
| 283C451B-C7AE-E311-B8ED-005056822391 | 10007667    | Wycombe High School                                                                    |
| DE3E8F49-C7AE-E311-B8ED-005056822391 | 10038068    | St Michael's Catholic College                                                          |
| 1896874F-C7AE-E311-B8ED-005056822391 | 10000737    | The Bishop's Stortford High School                                                     |
| 4070C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Meir Heath School                                                                      |
| C7F53F21-C7AE-E311-B8ED-005056822391 | 10004917    | Our Lady's Convent High School                                                         |
| 73EE7F55-C7AE-E311-B8ED-005056822391 | 10015712    | Great Yarmouth High                                                                    |
| 812A8AE5-C6AE-E311-B8ED-005056822391 | 10077598    | St Francis Roman Catholic Primary School                                               |
| C5DF6303-C7AE-E311-B8ED-005056822391 | 10015808    | Healing Comprehensive School                                                           |
| 150DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Belleville Primary School                                                              |
| D2DF6303-C7AE-E311-B8ED-005056822391 | 10004228    | Maryhill High School                                                                   |
| B995874F-C7AE-E311-B8ED-005056822391 | 10007357    | Watford Grammar School for Girls                                                       |
| 1A408F49-C7AE-E311-B8ED-005056822391 | 10007194    | Uxbridge High School                                                                   |
| A23273F7-C6AE-E311-B8ED-005056822391 | 10006866    | Windsor Boys' School                                                                   |
| 9A8E540F-C7AE-E311-B8ED-005056822391 | NULL        | The Ramsgate School                                                                    |
| EC8E540F-C7AE-E311-B8ED-005056822391 | 10007094    | Twynham School                                                                         |
| 0696874F-C7AE-E311-B8ED-005056822391 | 10003447    | Jack Hunt School (Homerton)                                                            |
| 618F540F-C7AE-E311-B8ED-005056822391 | 10015229    | Brighton Hill Community School                                                         |
| 0D1B5791-C7AE-E311-B8ED-005056822391 | NULL        | Brooke City Technology School                                                          |
| 1065F591-C6AE-E311-B8ED-005056822391 | 10079990    | Star Primary School                                                                    |
| F0E54C15-C7AE-E311-B8ED-005056822391 | 10016281    | The Ockendon School                                                                    |
| EFDF6303-C7AE-E311-B8ED-005056822391 | NULL        | Mark Rutherford Upper School & Community College                                       |
| F412E69D-C6AE-E311-B8ED-005056822391 | NULL        | Thorpe Greenways Junior School                                                         |
| C6FE7261-C7AE-E311-B8ED-005056822391 | 10018183    | The Deanes School                                                                      |
| D9DA7AF1-C6AE-E311-B8ED-005056822391 | 10001741    | Cranford Community                                                                     |
| B896874F-C7AE-E311-B8ED-005056822391 | NULL        | Cardinal Hinsley High School                                                           |
| E195874F-C7AE-E311-B8ED-005056822391 | 10006724    | The King John School                                                                   |
| B962F591-C6AE-E311-B8ED-005056822391 | 10071525    | Parklands Junior School                                                                |
| 0D365C09-C7AE-E311-B8ED-005056822391 | 10000763    | Blakeston Community School                                                             |
| EE4F50A9-C7AE-E311-B8ED-005056822391 | NULL        | Bristol and Bath Catholic Partnership SCITT                                            |
| 80FE7261-C7AE-E311-B8ED-005056822391 | 10006722    | The King Edmund School                                                                 |
| 381B5791-C7AE-E311-B8ED-005056822391 | 10007135    | Unity City Academy                                                                     |
| 4A2D8AE5-C6AE-E311-B8ED-005056822391 | 10000995    | Burleigh Community College                                                             |
| 5D8182EB-C6AE-E311-B8ED-005056822391 | NULL        | Redborne Upper School                                                                  |
| E4E44C15-C7AE-E311-B8ED-005056822391 | 10017796    | Swan Valley Community School                                                           |
| BFCB372D-C7AE-E311-B8ED-005056822391 | 10006745    | The London Oratory School                                                              |
| 3796874F-C7AE-E311-B8ED-005056822391 | 10003554    | Kemnal Technical College                                                               |
| 15D87AF1-C6AE-E311-B8ED-005056822391 | 10002569    | Frank F.Harrison Community School                                                      |
| 2764F591-C6AE-E311-B8ED-005056822391 | 10077807    | Marine Park First School                                                               |
| 48A77A5B-C7AE-E311-B8ED-005056822391 | 10003657    | Kings Norton Girls School                                                              |
| 37F83F21-C7AE-E311-B8ED-005056822391 | 10069591    | Malorees Junior School                                                                 |
| 7EB40486-C6AE-E311-B8ED-005056822391 | NULL        | Halsnead Community Primary School                                                      |
| 702F73F7-C6AE-E311-B8ED-005056822391 | 10006453    | Swanley School                                                                         |
| 8DE64C15-C7AE-E311-B8ED-005056822391 | 10007902    | Brune Park Community School                                                            |
| 071B5791-C7AE-E311-B8ED-005056822391 | NULL        | Djanogly Civil Technical College                                                       |
| A2876BFD-C6AE-E311-B8ED-005056822391 | 10014793    | Bispham High School                                                                    |
| FA96874F-C7AE-E311-B8ED-005056822391 | 10004422    | Montagu School                                                                         |
| 1BA87A5B-C7AE-E311-B8ED-005056822391 | 10006787    | The Priory School                                                                      |
| CDFE7261-C7AE-E311-B8ED-005056822391 | 10005683    | The Hedley Walter High School                                                          |
| C996874F-C7AE-E311-B8ED-005056822391 | 10016074    | Icknield High School                                                                   |
| FA3073F7-C6AE-E311-B8ED-005056822391 | 10003800    | Lea Valley High School                                                                 |
| 26E54C15-C7AE-E311-B8ED-005056822391 | 10017061    | Sydney Smith School                                                                    |
| D6E64C15-C7AE-E311-B8ED-005056822391 | 10015261    | Elizabeth Garrat Anderson                                                              |
| 09EE7F55-C7AE-E311-B8ED-005056822391 | 10005345    | Queen's School                                                                         |
| AA3E451B-C7AE-E311-B8ED-005056822391 | 10006102    | St Albans Catholic High                                                                |
| 148382EB-C6AE-E311-B8ED-005056822391 | 10015539    | Emerson Park School                                                                    |
| F0896BFD-C6AE-E311-B8ED-005056822391 | 10001341    | Chatham Grammer School for Boys                                                        |
| 03A77A5B-C7AE-E311-B8ED-005056822391 | 10007466    | Westwood St Thomas School                                                              |
| A2E54C15-C7AE-E311-B8ED-005056822391 | NULL        | Crispin School                                                                         |
| F23273F7-C6AE-E311-B8ED-005056822391 | 10014865    | Birches Head High School                                                               |
| BA3173F7-C6AE-E311-B8ED-005056822391 | 10006890    | Thomas Mills School                                                                    |
| 4B8E540F-C7AE-E311-B8ED-005056822391 | 10016662    | Mitchell High                                                                          |
| D4FE7261-C7AE-E311-B8ED-005056822391 | 10006558    | Tendring Technology & Sixth Form College                                               |
| 51996F6D-C7AE-E311-B8ED-005056822391 | 10029222    | Oakham School                                                                          |
| 0D8D540F-C7AE-E311-B8ED-005056822391 | 10007886    | Richard Aldworth Community School                                                      |
| 98DE6303-C7AE-E311-B8ED-005056822391 | 10017466    | Rushcroft School                                                                       |
| 38B03A27-C7AE-E311-B8ED-005056822391 | NULL        | The Westborough Primary School                                                         |
| 448382EB-C6AE-E311-B8ED-005056822391 | 10007488    | Whitefield School                                                                      |
| 34056786-C8AE-E311-B8ED-005056822391 | NULL        | University Of Reading                                                                  |
| 188182EB-C6AE-E311-B8ED-005056822391 | 10005501    | The Robert Manning School                                                              |
| 5A12E69D-C6AE-E311-B8ED-005056822391 | 10073066    | Charles Dickens Primary School                                                         |
| 2EFF7261-C7AE-E311-B8ED-005056822391 | 10001481    | Clacton County High School                                                             |
| 3D8F540F-C7AE-E311-B8ED-005056822391 | 10007957    | Lees Brook Community Sports College                                                    |
| D0E64C15-C7AE-E311-B8ED-005056822391 | 10016522    | Passmores School                                                                       |
| 8AD97AF1-C6AE-E311-B8ED-005056822391 | 10001826    | Dagenham Priory Comprehensive School, Arts College                                     |
| 9F1A7067-C7AE-E311-B8ED-005056822391 | 10006682    | The Harwich School                                                                     |
| 4E96874F-C7AE-E311-B8ED-005056822391 | 10006791    | The Radcliffe School                                                                   |
| 48EE7F55-C7AE-E311-B8ED-005056822391 | 10004834    | Oaklands Catholic School                                                               |
| 080D6579-C7AE-E311-B8ED-005056822391 | 10008596    | Whitgift School                                                                        |
| 51DB7AF1-C6AE-E311-B8ED-005056822391 | 10006868    | The Woodlands School                                                                   |
| 6E8182EB-C6AE-E311-B8ED-005056822391 | 10001729    | Cowes High School                                                                      |
| 7BD97AF1-C6AE-E311-B8ED-005056822391 | 10006817    | Smithdon High School                                                                   |
| 6B8182EB-C6AE-E311-B8ED-005056822391 | 10005326    | Queen Elizabeth's Community College                                                    |
| D73B451B-C7AE-E311-B8ED-005056822391 | 10005554    | Royal Latin School                                                                     |
| D73E8F49-C7AE-E311-B8ED-005056822391 | NULL        | St John the Baptist School -                                                           |
| B5E44C15-C7AE-E311-B8ED-005056822391 | 10005880    | Sittingbourne Community College                                                        |
| 8E1A5791-C7AE-E311-B8ED-005056822391 | 10016607    | More House School                                                                      |
| 4A886BFD-C6AE-E311-B8ED-005056822391 | 10003875    | Leiston High School                                                                    |
| FEEC7F55-C7AE-E311-B8ED-005056822391 | 10000603    | Beaverwood School for Girls                                                            |
| 183D451B-C7AE-E311-B8ED-005056822391 | 10006674    | The Green School                                                                       |
| 71E06303-C7AE-E311-B8ED-005056822391 | NULL        | Copleston High School                                                                  |
| BF385C09-C7AE-E311-B8ED-005056822391 | 10017398    | West Redcar School                                                                     |
| 9DBBED97-C6AE-E311-B8ED-005056822391 | NULL        | Bowes Primary School                                                                   |
| F01A5791-C7AE-E311-B8ED-005056822391 | NULL        | Macmillian College                                                                     |
| 508382EB-C6AE-E311-B8ED-005056822391 | 10003087    | Hillcrest School and Sixth Form Centre                                                 |
| 873F8F49-C7AE-E311-B8ED-005056822391 | 10000243    | Alperton Community School                                                              |
| 8C876BFD-C6AE-E311-B8ED-005056822391 | NULL        | Latimer Community Art College                                                          |
| CC2F73F7-C6AE-E311-B8ED-005056822391 | 10002675    | Gladesmore Community School                                                            |
| 7D365C09-C7AE-E311-B8ED-005056822391 | 10015490    | Cradley High School                                                                    |
| E83F8F49-C7AE-E311-B8ED-005056822391 | 10017683    | City of Portsmouth Boy's School                                                        |
| 36F73F21-C7AE-E311-B8ED-005056822391 | 10006720    | The Judd School                                                                        |
| 5C5F0C80-C6AE-E311-B8ED-005056822391 | NULL        | Broadfields Infants School                                                             |
| 5BDA7AF1-C6AE-E311-B8ED-005056822391 | 10000208    | Alexandra High School                                                                  |
| 153173F7-C6AE-E311-B8ED-005056822391 | 10015276    | Withywood Community School                                                             |
| 34BCED97-C6AE-E311-B8ED-005056822391 | NULL        | Bexhill Academy                                                                        |
| DADB7AF1-C6AE-E311-B8ED-005056822391 | 10000682    | Bicester Community College                                                             |
| D98182EB-C6AE-E311-B8ED-005056822391 | 10004574    | New College                                                                            |
| ECD97AF1-C6AE-E311-B8ED-005056822391 | 10015537    | Failsworth School                                                                      |
| E216CFAF-C6AE-E311-B8ED-005056822391 | 10069870    | Sketchley Hill Primary                                                                 |
| A5E54C15-C7AE-E311-B8ED-005056822391 | 10017041    | Bethnal Green Academy                                                                  |
| 2E13E69D-C6AE-E311-B8ED-005056822391 | NULL        | Thorpe Greenways Infant School                                                         |
| 950C6579-C7AE-E311-B8ED-005056822391 | 10013257    | Cranleigh School                                                                       |
| 78DB7AF1-C6AE-E311-B8ED-005056822391 | 10001855    | Dartford West Technology College                                                       |
| 9CC6BFBB-C6AE-E311-B8ED-005056822391 | 10077261    | Becket Primary School                                                                  |
| FC375C09-C7AE-E311-B8ED-005056822391 | NULL        | St Birinus School                                                                      |
| 3FDE6303-C7AE-E311-B8ED-005056822391 | 10003673    | Kingsthorpe Community College                                                          |
| 681A7067-C7AE-E311-B8ED-005056822391 | 10017512    | St Helena School                                                                       |
| 3F408F49-C7AE-E311-B8ED-005056822391 | 10001668    | Convent of Jesus and Mary Language College                                             |
| 2FD97AF1-C6AE-E311-B8ED-005056822391 | 10016830    | Malet Lambert School and Language College                                              |
| 00DA7AF1-C6AE-E311-B8ED-005056822391 | 10006466    | Swinton Community School                                                               |
| 4CDB7AF1-C6AE-E311-B8ED-005056822391 | 10018232    | The Coseley School                                                                     |
| DB849443-C7AE-E311-B8ED-005056822391 | 10002947    | Haydon School                                                                          |
| F03073F7-C6AE-E311-B8ED-005056822391 | 10001899    | Deben High School                                                                      |
| 86B50486-C6AE-E311-B8ED-005056822391 | 10075644    | Parkinson Lane Community Primary School                                                |
| 9AD250A3-C7AE-E311-B8ED-005056822391 | NULL        | Brooke Weston City Technology College                                                  |
| 5290540F-C7AE-E311-B8ED-005056822391 | 10003600    | Kidbrooke School                                                                       |
| BB3273F7-C6AE-E311-B8ED-005056822391 | 10003502    | John Smeaton Community College                                                         |
| 87FF7261-C7AE-E311-B8ED-005056822391 | 10005674    | Sandwich Technology School                                                             |
| 3290540F-C7AE-E311-B8ED-005056822391 | NULL        | Ashlawn School                                                                         |
| 26CC372D-C7AE-E311-B8ED-005056822391 | 10003066    | Highams Park School                                                                    |
| 74375C09-C7AE-E311-B8ED-005056822391 | 10006693    | The Highfield School                                                                   |
| D0E06303-C7AE-E311-B8ED-005056822391 | 10006804    | Rushden School                                                                         |
| 811A7067-C7AE-E311-B8ED-005056822391 | 10003096    | Hillview School for Girls                                                              |
| 82993DC1-C7AE-E311-B8ED-005056822391 | 10007852    | The University Of Dundee                                                               |
| F33D451B-C7AE-E311-B8ED-005056822391 | 10009472    | Archbishop Thurston CE School                                                          |
| 91D97AF1-C6AE-E311-B8ED-005056822391 | 10004094    | Longford Community School                                                              |
| 5BDB7AF1-C6AE-E311-B8ED-005056822391 | 10003759    | Lampton School                                                                         |
| C3365C09-C7AE-E311-B8ED-005056822391 | 10004221    | Marriots School                                                                        |
| 9B3F8F49-C7AE-E311-B8ED-005056822391 | 10007308    | Wallington High School for Girls                                                       |
| 31886BFD-C6AE-E311-B8ED-005056822391 | 10006448    | Swadelands School                                                                      |
| 1F8282EB-C6AE-E311-B8ED-005056822391 | 10002688    | Glenthorne High School                                                                 |
| FDA67A5B-C7AE-E311-B8ED-005056822391 | 10001851    | Darrick School                                                                         |
| 4ED291DF-C6AE-E311-B8ED-005056822391 | NULL        | St Edmund Campion Catholic School                                                      |
| 74536A73-C7AE-E311-B8ED-005056822391 | 10008507    | St Benedicts School                                                                    |
| A826A1D3-C6AE-E311-B8ED-005056822391 | 10079370    | Church Cowley St James First School                                                    |
| 75859443-C7AE-E311-B8ED-005056822391 | NULL        | Collingwood College                                                                    |
| A495874F-C7AE-E311-B8ED-005056822391 | NULL        | Myton School                                                                           |
| B01A7067-C7AE-E311-B8ED-005056822391 | 10006684    | The Hayesbrook School                                                                  |
| FA3273F7-C6AE-E311-B8ED-005056822391 | 10007366    | Weald of Kent Grammar School                                                           |
| E9F53F21-C7AE-E311-B8ED-005056822391 | 10006228    | St Marylebone Church of England School                                                 |
| F5FE7261-C7AE-E311-B8ED-005056822391 | 10017286    | The Grays School                                                                       |
| D71A5791-C7AE-E311-B8ED-005056822391 | 10008122    | The Brit School                                                                        |
| 5FE44C15-C7AE-E311-B8ED-005056822391 | 10016125    | The Wavell School                                                                      |
| 49C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Headlands Lower School                                                                 |
| 903C451B-C7AE-E311-B8ED-005056822391 | 10001069    | Buxton Community School                                                                |
| 55859443-C7AE-E311-B8ED-005056822391 | 10006749    | Maplesden Noakes School                                                                |
| 3BA87A5B-C7AE-E311-B8ED-005056822391 | 10006719    | John Warner School                                                                     |
| 0EBCED97-C6AE-E311-B8ED-005056822391 | 10070022    | Raynham Primary School                                                                 |
| 62E06303-C7AE-E311-B8ED-005056822391 | 10006641    | Community College Whistable                                                            |
| BCC1D6A9-C6AE-E311-B8ED-005056822391 | 10078291    | Bedgrove County First School                                                           |
| A9FE7261-C7AE-E311-B8ED-005056822391 | 10005635    | St Mary's Catholic School                                                              |
| 1ADA7AF1-C6AE-E311-B8ED-005056822391 | 10000675    | Bexleyheath School                                                                     |
| 278382EB-C6AE-E311-B8ED-005056822391 | NULL        | Hellesdon High School                                                                  |
| FEF53F21-C7AE-E311-B8ED-005056822391 | 10017241    | St Ursula's School                                                                     |
| C88382EB-C6AE-E311-B8ED-005056822391 | NULL        | Thamesbridge College                                                                   |
| C9A67A5B-C7AE-E311-B8ED-005056822391 | 10002662    | George Dixon School and Sixth Form Centre                                              |
| 5E3173F7-C6AE-E311-B8ED-005056822391 | 10002310    | Ernesford Grange School and Community College                                          |
| 9D375C09-C7AE-E311-B8ED-005056822391 | NULL        | Biddenham Upper School                                                                 |
| B8355C09-C7AE-E311-B8ED-005056822391 | 10016255    | Leasowes Community College                                                             |
| A0F63F21-C7AE-E311-B8ED-005056822391 | 10016470    | St Lukes CE School                                                                     |
| 72E54C15-C7AE-E311-B8ED-005056822391 | 10002876    | Hampstead School                                                                       |
| 9383149F-C9AE-E311-B8ED-005056822391 | NULL        | The Kirlees Partnership                                                                |
| 7AF73F21-C7AE-E311-B8ED-005056822391 | 10006620    | The Campion                                                                            |
| 8B298AE5-C6AE-E311-B8ED-005056822391 | 10075987    | St Joseph's RC Primary School                                                          |
| 86886BFD-C6AE-E311-B8ED-005056822391 | 10017112    | Pilton Community College.                                                              |
| C3876BFD-C6AE-E311-B8ED-005056822391 | 10002036    | Friffield School                                                                       |
| E48382EB-C6AE-E311-B8ED-005056822391 | 10008568    | Tudor Grange School                                                                    |
| FF1A5791-C7AE-E311-B8ED-005056822391 | 10008649    | Leigh CTC                                                                              |
| 6F96874F-C7AE-E311-B8ED-005056822391 | 10013284    | Comberton Village                                                                      |
| C38C52FC-EF10-E511-A3DA-005056822390 | 99999995    | Other UK (Scotland)                                                                    |
| 28A77A5B-C7AE-E311-B8ED-005056822391 | 10006739    | Herts GM School 1                                                                      |
| 573173F7-C6AE-E311-B8ED-005056822391 | 10016185    | King's Wood School                                                                     |
| 7D876BFD-C6AE-E311-B8ED-005056822391 | 10002604    | Furze Platt Senior School                                                              |
| AD876BFD-C6AE-E311-B8ED-005056822391 | 10014792    | Alderman Peel High School                                                              |
| 01DF6303-C7AE-E311-B8ED-005056822391 | NULL        | Copleston High School                                                                  |
| F7DE6303-C7AE-E311-B8ED-005056822391 | 10001317    | Chapter .School                                                                        |
| 953073F7-C6AE-E311-B8ED-005056822391 | 10005214    | Princes Risborough School                                                              |
| 2491540F-C7AE-E311-B8ED-005056822391 | 10007520    | Wilmington Hall School                                                                 |
| 7B632E87-C9AE-E311-B8ED-005056822391 | NULL        | Chester College of HE                                                                  |
| E71A5791-C7AE-E311-B8ED-005056822391 | 10015940    | John Cabot City Technology College                                                     |
| 5217CFAF-C6AE-E311-B8ED-005056822391 | 10075299    | Carlton Junior School                                                                  |
| A83273F7-C6AE-E311-B8ED-005056822391 | NULL        | Walshaw High School                                                                    |
| DBF53F21-C7AE-E311-B8ED-005056822391 | 10000341    | The Archbishop Grimshaw Catholic School                                                |
| 880D6579-C7AE-E311-B8ED-005056822391 | 10008422    | The Perse School                                                                       |
| B43B451B-C7AE-E311-B8ED-005056822391 | 10000044    | Abbot Beyne School                                                                     |
| B7D31C2A-0FD0-EB11-BACC-000D3ADB5ECD | 10024962    | Luminate Partnership for ITT                                                           |
| 6D90540F-C7AE-E311-B8ED-005056822391 | 10015066    | Yardleys School                                                                        |
| 9A8282EB-C6AE-E311-B8ED-005056822391 | 10017991    | The Selhurst High School                                                               |
| 898F540F-C7AE-E311-B8ED-005056822391 | 10016340    | Lostock Hall High School                                                               |
| 05996F6D-C7AE-E311-B8ED-005056822391 | 10015447    | Dulwich College                                                                        |
| CA76B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Old Clee Junior School                                                                 |
| C5CB372D-C7AE-E311-B8ED-005056822391 | 10003732    | La Retraite RC School                                                                  |
| 5AFF7261-C7AE-E311-B8ED-005056822391 | 10005039    | Pent Valley School                                                                     |
| E65E0C80-C6AE-E311-B8ED-005056822391 | NULL        | The Duchy School                                                                       |
| 5B8E540F-C7AE-E311-B8ED-005056822391 | 10002329    | Estover Community School                                                               |
| D13F8F49-C7AE-E311-B8ED-005056822391 | 10005633    | St Georges CE School (RB)                                                              |
| 2E3F451B-C7AE-E311-B8ED-005056822391 | 10006261    | St Thomas More Catholic School                                                         |
| F52F73F7-C6AE-E311-B8ED-005056822391 | 10005197    | The President Kennedy Secondary                                                        |
| 8E886BFD-C6AE-E311-B8ED-005056822391 | 10015126    | Collegiate High School                                                                 |
| B42B8AE5-C6AE-E311-B8ED-005056822391 | 10075178    | Avigdor Hirsch Torah Temimah Primary School                                            |
| DBCB372D-C7AE-E311-B8ED-005056822391 | 10000989    | Bullers Wood School for Girls                                                          |
| A38282EB-C6AE-E311-B8ED-005056822391 | 10007337    | The Warriner School                                                                    |
| 6420B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Bexton Primary School                                                                  |
| 33E06303-C7AE-E311-B8ED-005056822391 | 10005858    | Sir Frederic Osborn School                                                             |
| 3725A1D3-C6AE-E311-B8ED-005056822391 | 10078472    | Bridge & Patrixbourne CEP School                                                       |
| DFA67A5B-C7AE-E311-B8ED-005056822391 | 10006800    | Ridgeway Pre School                                                                    |
| B30A6485-C7AE-E311-B8ED-005056822391 | 10008567    | Trinity School of John Whitgift                                                        |
| 19859443-C7AE-E311-B8ED-005056822391 | 10015041    | Ashton-on-Mersey School                                                                |
| 43A77A5B-C7AE-E311-B8ED-005056822391 | 10017269    | Purbrook Park School                                                                   |
| 498182EB-C6AE-E311-B8ED-005056822391 | 10010000    | Freebrough Community College                                                           |
| 47996F6D-C7AE-E311-B8ED-005056822391 | 10078232    | King's House School                                                                    |
| 17C9BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Rose Hill First School                                                                 |
| F2DA7AF1-C6AE-E311-B8ED-005056822391 | 10004281    | Medina High School                                                                     |
| 8090540F-C7AE-E311-B8ED-005056822391 | 10017794    | The Channel School                                                                     |
| 230CFD8B-C6AE-E311-B8ED-005056822391 | 10076421    | Keir Hardie Primary School                                                             |
| BBB60486-C6AE-E311-B8ED-005056822391 | NULL        | Whitley Park Junior School                                                             |
| AC849443-C7AE-E311-B8ED-005056822391 | 10017704    | The Avon Valley School                                                                 |
| 9763F591-C6AE-E311-B8ED-005056822391 | NULL        | Bolton Brow Primary Academy                                                            |
| 6EFE7261-C7AE-E311-B8ED-005056822391 | 10018203    | Cornelius Vermuyden                                                                    |
| 1D876BFD-C6AE-E311-B8ED-005056822391 | 10006747    | The Long Eaton School                                                                  |
| 06F83F21-C7AE-E311-B8ED-005056822391 | 10010917    | Bishop Challoner Catholic School                                                       |
| 4E3E451B-C7AE-E311-B8ED-005056822391 | 10006269    | St Wilfrid's Catholic Comprehensive School, Crawle                                     |
| 763F451B-C7AE-E311-B8ED-005056822391 | 10006255    | St Robert of Newminster Roman Catholic School                                          |
| A7DB7AF1-C6AE-E311-B8ED-005056822391 | 10003048    | Heston Community School                                                                |
| D4896BFD-C6AE-E311-B8ED-005056822391 | 10007460    | Weston Favell Upper School                                                             |
| 6A2F73F7-C6AE-E311-B8ED-005056822391 | 10006865    | The Willink School                                                                     |
| A32B8AE5-C6AE-E311-B8ED-005056822391 | 10073750    | Michael Sobell Sinai School                                                            |
| F3E44C15-C7AE-E311-B8ED-005056822391 | 10015164    | Burnage High School                                                                    |
| 6B26A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Overleigh St Mary's CE Primary School                                                  |
| 7BE54C15-C7AE-E311-B8ED-005056822391 | 10015160    | Wright Robinson Sports College                                                         |
| 5EE74C15-C7AE-E311-B8ED-005056822391 | NULL        | Orleton Park School                                                                    |
| DB1A5791-C7AE-E311-B8ED-005056822391 | NULL        | The City Technology College                                                            |
| 2D11E69D-C6AE-E311-B8ED-005056822391 | 10077108    | Willingdon Primary School                                                              |
| 8C3E8F49-C7AE-E311-B8ED-005056822391 | 10014062    | The Queens School                                                                      |
| 4A96874F-C7AE-E311-B8ED-005056822391 | NULL        | Wootton Upper School                                                                   |
| ACD97AF1-C6AE-E311-B8ED-005056822391 | 10015094    | Bydales School                                                                         |
| 78FF7261-C7AE-E311-B8ED-005056822391 | 10016227    | The Stanway School                                                                     |
| B6E06303-C7AE-E311-B8ED-005056822391 | 10003655    | Kings Langley School                                                                   |
| AAE64C15-C7AE-E311-B8ED-005056822391 | 10017588    | Stauton Park Community School                                                          |
| 07375C09-C7AE-E311-B8ED-005056822391 | 10006810    | The Sele School                                                                        |
| 8DE54C15-C7AE-E311-B8ED-005056822391 | 10017886    | Fir Vale School                                                                        |
| 2B8F540F-C7AE-E311-B8ED-005056822391 | 10008581    | The Vyne Community School                                                              |
| 222D8AE5-C6AE-E311-B8ED-005056822391 | 10003045    | Hertswood School                                                                       |
| DC385C09-C7AE-E311-B8ED-005056822391 | 10003615    | King Alfreds                                                                           |
| 57642E87-C9AE-E311-B8ED-005056822391 | NULL        | Wimborne Area Training Partnership                                                     |
| 66E44C15-C7AE-E311-B8ED-005056822391 | 10002882    | Handsworth Wood Girls School                                                           |
| DAE54C15-C7AE-E311-B8ED-005056822391 | 10006893    | Thomas Tallis School                                                                   |
| 333F451B-C7AE-E311-B8ED-005056822391 | 10001164    | Cardinal Newman Catholic School A Specialist Science College                           |
| 6C76B0C7-C6AE-E311-B8ED-005056822391 | 10071332    | William Davis Primary School                                                           |
| C4E44C15-C7AE-E311-B8ED-005056822391 | 10004284    | Medway Community College                                                               |
| 30CC372D-C7AE-E311-B8ED-005056822391 | 10004190    | Manor High School (Foundation)                                                         |
| 8D849443-C7AE-E311-B8ED-005056822391 | 10000130    | Adams Grammar School                                                                   |
| 7B96874F-C7AE-E311-B8ED-005056822391 | 10006271    | St Wilfrid's CE Technology College                                                     |
| 80ED7F55-C7AE-E311-B8ED-005056822391 | 10006856    | The Westgate School                                                                    |
| D6E44C15-C7AE-E311-B8ED-005056822391 | 10007207    | Valley Park Community School                                                           |
| 55DE6303-C7AE-E311-B8ED-005056822391 | 10001754    | The Cressex Community School                                                           |
| 838D540F-C7AE-E311-B8ED-005056822391 | 10000685    | Biddulph High School                                                                   |
| E58E540F-C7AE-E311-B8ED-005056822391 | 10015007    | Bemrose Community School                                                               |
| 47859443-C7AE-E311-B8ED-005056822391 | NULL        | Highbury Secondary School                                                              |
| D38182EB-C6AE-E311-B8ED-005056822391 | 10003638    | King Edward VII School                                                                 |
| 4B876BFD-C6AE-E311-B8ED-005056822391 | 10004155    | Maiden Erlegh School                                                                   |
| 39365C09-C7AE-E311-B8ED-005056822391 | 10007079    | Turnford School                                                                        |
| 293073F7-C6AE-E311-B8ED-005056822391 | 10003496    | The John O'Gaunt Community School                                                      |
| 176ADEA3-C6AE-E311-B8ED-005056822391 | 10081010    | Lightwoods Primary School                                                              |
| 9CE6D865-0982-E411-A72C-005056822391 | 10007854    | Cardiff Metropolitan University                                                        |
| 7490540F-C7AE-E311-B8ED-005056822391 | 10017693    | The Lancaster School                                                                   |
| 78876BFD-C6AE-E311-B8ED-005056822391 | 10002991    | Helenswood School                                                                      |
| 1E996F6D-C7AE-E311-B8ED-005056822391 | 10016322    | King's College School                                                                  |
| 39536A73-C7AE-E311-B8ED-005056822391 | 10016249    | Leicester Islamic Academy                                                              |
| F63C451B-C7AE-E311-B8ED-005056822391 | 10004104    | Lord William's School                                                                  |
| 35EE7F55-C7AE-E311-B8ED-005056822391 | 10002897    | Gm School 3                                                                            |
| 193373F7-C6AE-E311-B8ED-005056822391 | 10006695    | The Holt School                                                                        |
| CDDA7AF1-C6AE-E311-B8ED-005056822391 | 10001335    | Charters School                                                                        |
| 518182EB-C6AE-E311-B8ED-005056822391 | 10002856    | Hall Garth                                                                             |
| A0FF7261-C7AE-E311-B8ED-005056822391 | 10002745    | Gravesend Grammar School for Boys                                                      |
| A40CFD8B-C6AE-E311-B8ED-005056822391 | 10074561    | Thameside Primary School                                                               |
| 98BAED97-C6AE-E311-B8ED-005056822391 | 10081481    | Grafton Infants School                                                                 |
| 08A77A5B-C7AE-E311-B8ED-005056822391 | 10016963    | The Appleton School                                                                    |
| 29E74C15-C7AE-E311-B8ED-005056822391 | 10016667    | The Mount Fitchet High School                                                          |
| 0A8F540F-C7AE-E311-B8ED-005056822391 | NULL        | High View Community Education Centre                                                   |
| F0E2CE37-FC89-E411-A03E-005056822390 | 10035104    | Cramlington Teaching School Alliance SCITT                                             |
| 613E451B-C7AE-E311-B8ED-005056822391 | 10007047    | Trinity Catholic High School                                                           |
| 81536A73-C7AE-E311-B8ED-005056822391 | 10014957    | Ampleforth College                                                                     |
| 1D8D540F-C7AE-E311-B8ED-005056822391 | 10018149    | Penrice School                                                                         |
| 8B355C09-C7AE-E311-B8ED-005056822391 | 10016204    | Lealands High School                                                                   |
| 1DD97AF1-C6AE-E311-B8ED-005056822391 | 10016041    | Thrybergh Academy and Sports College                                                   |
| E13B451B-C7AE-E311-B8ED-005056822391 | 10006761    | Neale-Wade Community College                                                           |
| 54859443-C7AE-E311-B8ED-005056822391 | 10007443    | Westcliff High School for Boys                                                         |
| 3EDB7AF1-C6AE-E311-B8ED-005056822391 | 10016514    | Parkside Community College                                                             |
| 0B0D6579-C7AE-E311-B8ED-005056822391 | 10017185    | Bolton School (Boy's Division)                                                         |
| 033173F7-C6AE-E311-B8ED-005056822391 | 10015150    | Bramhall High School                                                                   |
| 74FE7261-C7AE-E311-B8ED-005056822391 | 10005373    | Rainham Mark Grammar School                                                            |
| 4B3073F7-C6AE-E311-B8ED-005056822391 | 10015714    | The Halifax Academy                                                                    |
| 6E365C09-C7AE-E311-B8ED-005056822391 | 10016214    | Kelvin Hall School                                                                     |
| 51F83F21-C7AE-E311-B8ED-005056822391 | 10070226    | Holy Family Catholic School                                                            |
| 73CB372D-C7AE-E311-B8ED-005056822391 | NULL        | St Helens RC Primary School                                                            |
| 6A8482EB-C6AE-E311-B8ED-005056822391 | 10005592    | Ryde High School                                                                       |
| 3F64F591-C6AE-E311-B8ED-005056822391 | 10079723    | Old Palace Primary School                                                              |
| CE0C6579-C7AE-E311-B8ED-005056822391 | 10008365    | Loughborough Grammar School                                                            |
| 2D8282EB-C6AE-E311-B8ED-005056822391 | NULL        | The CEdars Upper School and Community College                                          |
| 693C451B-C7AE-E311-B8ED-005056822391 | 10006679    | The Gryphon School                                                                     |
| 318482EB-C6AE-E311-B8ED-005056822391 | NULL        | T.P. Rily Commuinty School                                                             |
| 817D99D9-C6AE-E311-B8ED-005056822391 | 10075796    | Christchurch CE Primary School                                                         |
| 053273F7-C6AE-E311-B8ED-005056822391 | 10015234    | Bower Park School                                                                      |
| 67876BFD-C6AE-E311-B8ED-005056822391 | 10006874    | Theale Green Community School                                                          |
| DF3273F7-C6AE-E311-B8ED-005056822391 | 10015927    | Holmfirth High School                                                                  |
| 63365C09-C7AE-E311-B8ED-005056822391 | 10006765    | The Noble School                                                                       |
| B73D451B-C7AE-E311-B8ED-005056822391 | 10005390    | Rawlins Community College                                                              |
| 3C1B5791-C7AE-E311-B8ED-005056822391 | 10015340    | Djanogly City Academy Nottingham                                                       |
| 243F451B-C7AE-E311-B8ED-005056822391 | 10004915    | Our Lady and St Chad RC Comprehensive School                                           |
| 20886BFD-C6AE-E311-B8ED-005056822391 | 10007508    | William Bradford                                                                       |
| 1B19CFAF-C6AE-E311-B8ED-005056822391 | 10076180    | Mayflower Primary School                                                               |
| 913273F7-C6AE-E311-B8ED-005056822391 | 10003484    | John Colet School                                                                      |
| B2A77A5B-C7AE-E311-B8ED-005056822391 | 10006799    | Ricksstones School                                                                     |
| 13365C09-C7AE-E311-B8ED-005056822391 | 10000649    | Benton Park School                                                                     |
| 848282EB-C6AE-E311-B8ED-005056822391 | 10016566    | North Walsham High School                                                              |
| D8DF6303-C7AE-E311-B8ED-005056822391 | 10016058    | Hreod Parkway School                                                                   |
| 9A10E69D-C6AE-E311-B8ED-005056822391 | NULL        | Mount Pellon Junior and Infant School                                                  |
| 808282EB-C6AE-E311-B8ED-005056822391 | NULL        | Aylwin Girls' School                                                                   |
| 3A1B7067-C7AE-E311-B8ED-005056822391 | 10006598    | The Billericay School                                                                  |
| D4BBED97-C6AE-E311-B8ED-005056822391 | NULL        | Wood End Park Primary School                                                           |
| 99536A73-C7AE-E311-B8ED-005056822391 | 10008506    | St Bedes School                                                                        |
| 2496874F-C7AE-E311-B8ED-005056822391 | 10002299    | Epsom & Ewell High School                                                              |
| 3F8D540F-C7AE-E311-B8ED-005056822391 | 10015288    | Fearns Community High School                                                           |
| E118CFAF-C6AE-E311-B8ED-005056822391 | 10076182    | Granby Primary School                                                                  |
| F6A67A5B-C7AE-E311-B8ED-005056822391 | 10004418    | Monk's Dyke Technology College                                                         |
| F6355C09-C7AE-E311-B8ED-005056822391 | 10003068    | Highbury Grove                                                                         |
| 1891540F-C7AE-E311-B8ED-005056822391 | 10017964    | Fernhill Secondary School                                                              |
| A73E8F49-C7AE-E311-B8ED-005056822391 | 10002785    | Greenwood Dale School                                                                  |
| AEAF3A27-C7AE-E311-B8ED-005056822391 | NULL        | Greenway Junior School                                                                 |
| 3A8482EB-C6AE-E311-B8ED-005056822391 | 10002853    | Halewood Community Comprehensive School                                                |
| F41EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | St Stephen's Junior School                                                             |
| 22A87A5B-C7AE-E311-B8ED-005056822391 | 10000289    | Angley Secondary School                                                                |
| 16E64C15-C7AE-E311-B8ED-005056822391 | NULL        | Waverley School                                                                        |
| 4211E69D-C6AE-E311-B8ED-005056822391 | NULL        | Cleves Primary School                                                                  |
| 565F0C80-C6AE-E311-B8ED-005056822391 | 10074206    | Brooksward Combined School                                                             |
| 3F365C09-C7AE-E311-B8ED-005056822391 | 10015653    | Halyard High School                                                                    |
| CF8D540F-C7AE-E311-B8ED-005056822391 | 10017047    | Portchester School                                                                     |
| C6E16303-C7AE-E311-B8ED-005056822391 | NULL        | Collenswood School                                                                     |
| 80AF3A27-C7AE-E311-B8ED-005056822391 | 10079486    | St Patricks RC Primary                                                                 |
| 50FE7261-C7AE-E311-B8ED-005056822391 | 10016131    | The Thorpe Bay School                                                                  |
| 2D375C09-C7AE-E311-B8ED-005056822391 | NULL        | Harrowden Middle School                                                                |
| B5BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Oakwood School                                                                         |
| 3BE84C15-C7AE-E311-B8ED-005056822391 | 10004251    | Mayflower County High School                                                           |
| AEDF6303-C7AE-E311-B8ED-005056822391 | 10006899    | Aylsham High School                                                                    |
| F5DA7AF1-C6AE-E311-B8ED-005056822391 | 10005865    | Sir John Lawes                                                                         |
| 4C395C09-C7AE-E311-B8ED-005056822391 | NULL        | Callington Community College                                                           |
| 86E54C15-C7AE-E311-B8ED-005056822391 | 10014998    | Bitterne Park                                                                          |
| 51DA7AF1-C6AE-E311-B8ED-005056822391 | 10016181    | Royal Liberty School                                                                   |
| D31A5791-C7AE-E311-B8ED-005056822391 | 10017069    | Bacon's College                                                                        |
| 7FDF6303-C7AE-E311-B8ED-005056822391 | 10006594    | The Beaconsfield School                                                                |
| 768D540F-C7AE-E311-B8ED-005056822391 | NULL        | Sutton High School                                                                     |
| DA74B0C7-C6AE-E311-B8ED-005056822391 | 10071339    | Osmani Primary School                                                                  |
| EF993DC1-C7AE-E311-B8ED-005056822391 | NULL        | Leeds Polytechnic                                                                      |
| 628382EB-C6AE-E311-B8ED-005056822391 | 10015965    | Varndean School                                                                        |
| AB8182EB-C6AE-E311-B8ED-005056822391 | 10015640    | Etone Community School                                                                 |
| 1B8A6BFD-C6AE-E311-B8ED-005056822391 | 10004305    | Mereway Upper School                                                                   |
| DADE6303-C7AE-E311-B8ED-005056822391 | NULL        | Bungay High School                                                                     |
| FD2F73F7-C6AE-E311-B8ED-005056822391 | 10017749    | Stockport School                                                                       |
| 5FF73F21-C7AE-E311-B8ED-005056822391 | 10017762    | St Michael's CE High School                                                            |
| A0876BFD-C6AE-E311-B8ED-005056822391 | 10001731    | Cox Green School                                                                       |
| 720EFD8B-C6AE-E311-B8ED-005056822391 | 10079700    | Merryhills Primary School                                                              |
| 68BAED97-C6AE-E311-B8ED-005056822391 | 10072884    | Fryent Primary School                                                                  |
| 7A876BFD-C6AE-E311-B8ED-005056822391 | 10004961    | Paget County High School                                                               |
| 713073F7-C6AE-E311-B8ED-005056822391 | 10002848    | Hainault Forest High School                                                            |
| C0A67A5B-C7AE-E311-B8ED-005056822391 | 10006247    | St Pauls Catholic School                                                               |
| C4DA7AF1-C6AE-E311-B8ED-005056822391 | 10034549    | Minsthorpe Community College                                                           |
| C1C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Hillside Primary School                                                                |
| B0385C09-C7AE-E311-B8ED-005056822391 | 10001371    | Cheslyn Hay High School                                                                |
| 75996F6D-C7AE-E311-B8ED-005056822391 | 10017658    | Westholme Upper School                                                                 |
| 3B3F8F49-C7AE-E311-B8ED-005056822391 | 10014869    | Queen Elizabeth School                                                                 |
| 90EE667F-C7AE-E311-B8ED-005056822391 | 10008293    | Ipswich School                                                                         |
| 0F395C09-C7AE-E311-B8ED-005056822391 | 10015302    | Fowey Community College                                                                |
| 86D45197-C7AE-E311-B8ED-005056822391 | 10000634    | Belstead School                                                                        |
| 5C3C451B-C7AE-E311-B8ED-005056822391 | NULL        | De Aston School                                                                        |
| 470EFD8B-C6AE-E311-B8ED-005056822391 | 10077809    | Southridge First School                                                                |
| BFF63F21-C7AE-E311-B8ED-005056822391 | 10017331    | Sacred Heart High School                                                               |
| 753273F7-C6AE-E311-B8ED-005056822391 | 10007075    | Tunbridge Wells Girls' Grammar School                                                  |
| EC8082EB-C6AE-E311-B8ED-005056822391 | 10016465    | Park Lane Learning Trust                                                               |
| 7D896BFD-C6AE-E311-B8ED-005056822391 | 10001472    | City of Norwich School                                                                 |
| 62D87AF1-C6AE-E311-B8ED-005056822391 | 10007358    | Wath Comprehensive School : A Language College                                         |
| C83E8F49-C7AE-E311-B8ED-005056822391 | 10003648    | Kings School                                                                           |
| 27F83F21-C7AE-E311-B8ED-005056822391 | 10073704    | Priory School                                                                          |
| C0FE7261-C7AE-E311-B8ED-005056822391 | 10002669    | The Giles School                                                                       |
| 71D87AF1-C6AE-E311-B8ED-005056822391 | NULL        | Mansfield High School                                                                  |
| 138182EB-C6AE-E311-B8ED-005056822391 | 10002584    | Frome Community College                                                                |
| 613F451B-C7AE-E311-B8ED-005056822391 | 10000220    | All Saints RC High School                                                              |
| 3B395C09-C7AE-E311-B8ED-005056822391 | 10002445    | Ferryhill Comprehensive School                                                         |
| BFF73F21-C7AE-E311-B8ED-005056822391 | 10006265    | St Thomas Moore School                                                                 |
| DB3173F7-C6AE-E311-B8ED-005056822391 | 10014916    | Berry Hill High School                                                                 |
| C4849443-C7AE-E311-B8ED-005056822391 | 10001681    | Copland Community School GMS                                                           |
| 19886BFD-C6AE-E311-B8ED-005056822391 | 10005983    | South Holderness School                                                                |
| C00DFD8B-C6AE-E311-B8ED-005056822391 | 10071276    | Roe Green Infant School                                                                |
| 4BE16303-C7AE-E311-B8ED-005056822391 | 10005657    | Samuel Ward Upper School                                                               |
| 1FE06303-C7AE-E311-B8ED-005056822391 | 10007465    | Westwood High School                                                                   |
| 32FF7261-C7AE-E311-B8ED-005056822391 | 10001532    | Colbayns High School                                                                   |
| 3F8E540F-C7AE-E311-B8ED-005056822391 | 10015621    | Whitworth Community High                                                               |
| CFD97AF1-C6AE-E311-B8ED-005056822391 | 10002138    | Eastbrook Comprehensive                                                                |
| DA8E540F-C7AE-E311-B8ED-005056822391 | 10016113    | Thistley Hough High School                                                             |
| 210A6485-C7AE-E311-B8ED-005056822391 | 10015581    | Egerton Rothesay School                                                                |
| 197D99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Mary's Church of England Voluntary Aided Primar                                     |
| 76DF6303-C7AE-E311-B8ED-005056822391 | NULL        | Springwood High School                                                                 |
| A818CFAF-C6AE-E311-B8ED-005056822391 | 10072544    | Winchelsea Primary School Ruskington                                                   |
| CB996F6D-C7AE-E311-B8ED-005056822391 | 10008102    | Birkdale School                                                                        |
| 1ED55197-C7AE-E311-B8ED-005056822391 | 10015947    | The Vally School                                                                       |
| F93B451B-C7AE-E311-B8ED-005056822391 | 10006328    | Steyning Grammar School                                                                |
| 4990540F-C7AE-E311-B8ED-005056822391 | 10008549    | The Abbey School                                                                       |
| 9DD491DF-C6AE-E311-B8ED-005056822391 | 10071625    | St Bernadette's RC Primary                                                             |
| DC375C09-C7AE-E311-B8ED-005056822391 | 10004242    | Matthew Arnold School                                                                  |
| 7ADA7AF1-C6AE-E311-B8ED-005056822391 | 10008910    | Chesterton Community College                                                           |
| 55ED7F55-C7AE-E311-B8ED-005056822391 | 10002922    | Hasmonean High School                                                                  |
| 7DFE7261-C7AE-E311-B8ED-005056822391 | 10013307    | Sir William Robertson High School                                                      |
| 48375C09-C7AE-E311-B8ED-005056822391 | 10017913    | Gillbrook Technology College                                                           |
| 6C8482EB-C6AE-E311-B8ED-005056822391 | 10013310    | The Aveland School                                                                     |
| 6CE74C15-C7AE-E311-B8ED-005056822391 | NULL        | Edge End High                                                                          |
| 77F73F21-C7AE-E311-B8ED-005056822391 | NULL        | St Paul's RC Secondary School                                                          |
| 44615C8B-C7AE-E311-B8ED-005056822391 | NULL        | Sedgemoor College                                                                      |
| 5EE64C15-C7AE-E311-B8ED-005056822391 | 10015930    | Homerton College of Technology                                                         |
| CCCEA8CD-C6AE-E311-B8ED-005056822391 | 10068553    | St Andrew's C of E Primary School                                                      |
| 72F73F21-C7AE-E311-B8ED-005056822391 | 10006119    | St Bedes School                                                                        |
| F0FE7261-C7AE-E311-B8ED-005056822391 | 10006189    | St John Fisher School                                                                  |
| 2675B0C7-C6AE-E311-B8ED-005056822391 | 10075294    | Middle Park Primary School                                                             |
| A43B451B-C7AE-E311-B8ED-005056822391 | NULL        | Canon Williamson C of E High School                                                    |
| 743E8F49-C7AE-E311-B8ED-005056822391 | NULL        | Budmouth School                                                                        |
| A5C1D6A9-C6AE-E311-B8ED-005056822391 | 10079520    | Berkeley Junior School                                                                 |
| 3FE74C15-C7AE-E311-B8ED-005056822391 | 10002748    | Great Baddow High School                                                               |
| 05C92DD3-C7AE-E311-B8ED-005056822391 | NULL        | Cheltenham and Gloucester College of Higher Education                                  |
| F9F53F21-C7AE-E311-B8ED-005056822391 | 10006201    | St John's Roman Catholic Voluntary Aided Comprehen                                     |
| 6C8282EB-C6AE-E311-B8ED-005056822391 | 10015746    | Waverley School                                                                        |
| B5E54C15-C7AE-E311-B8ED-005056822391 | 10007948    | Hummersknott School                                                                    |
| E795874F-C7AE-E311-B8ED-005056822391 | 10005682    | Sawtry Community Technology College                                                    |
| BC74B0C7-C6AE-E311-B8ED-005056822391 | 10078058    | Gospel Oak School                                                                      |
| 833D451B-C7AE-E311-B8ED-005056822391 | 10016437    | St Andrew's C of E High School for Boys                                                |
| 592C8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Mary's Catholic Primary School                                                      |
| 17859443-C7AE-E311-B8ED-005056822391 | 10001142    | Canon Slade C of E School                                                              |
| 76E16303-C7AE-E311-B8ED-005056822391 | NULL        | Warden Park School                                                                     |
| 883C451B-C7AE-E311-B8ED-005056822391 | 10005915    | Slough and Eron Church of England Shcool                                               |
| 15D97AF1-C6AE-E311-B8ED-005056822391 | 10000162    | North Doncaster Technology College                                                     |
| F5A67A5B-C7AE-E311-B8ED-005056822391 | 10005636    | St Michaels Catholic High School                                                       |
| D7866BFD-C6AE-E311-B8ED-005056822391 | 10016045    | The John Spendluffe School, Alford                                                     |
| A8E74C15-C7AE-E311-B8ED-005056822391 | 10016425    | Madeley Court School                                                                   |
| AE8F540F-C7AE-E311-B8ED-005056822391 | 10005374    | Rainham School for Girls                                                               |
| BB3073F7-C6AE-E311-B8ED-005056822391 | 10001302    | Chace School                                                                           |
| 5D8D540F-C7AE-E311-B8ED-005056822391 | 10015184    | Crookhorn Community School                                                             |
| CF90540F-C7AE-E311-B8ED-005056822391 | 10000699    | Birchwood Community High School                                                        |
| 2B8A6BFD-C6AE-E311-B8ED-005056822391 | 10007933    | Graham School                                                                          |
| B13073F7-C6AE-E311-B8ED-005056822391 | 10001470    | City of Lincoln Community College                                                      |
| 0D408F49-C7AE-E311-B8ED-005056822391 | 10006445    | Sutton Grammer School for Boys                                                         |
| F5D87AF1-C6AE-E311-B8ED-005056822391 | 10016067    | Villiers High School                                                                   |
| 523F451B-C7AE-E311-B8ED-005056822391 | 10005631    | St Benedict School                                                                     |
| C33E451B-C7AE-E311-B8ED-005056822391 | 10006270    | St Wilfrid's Catholic High School & Sixth Form College: A Voluntary Academy            |
| 03DA7AF1-C6AE-E311-B8ED-005056822391 | 10004906    | Ossett Academy and Sixth Form College                                                  |
| AAE06303-C7AE-E311-B8ED-005056822391 | 10001364    | Chesham Park Community College                                                         |
| 723E451B-C7AE-E311-B8ED-005056822391 | 10006206    | St Joseph's Catholic Academy                                                           |
| D42F73F7-C6AE-E311-B8ED-005056822391 | 10015456    | Cumberland School                                                                      |
| 858E540F-C7AE-E311-B8ED-005056822391 | 10006715    | John Kitto Community College                                                           |
| 9F996F6D-C7AE-E311-B8ED-005056822391 | 10029220    | The Lady Eleanor Holles School                                                         |
| FFA77A5B-C7AE-E311-B8ED-005056822391 | 10003515    | Ancaster High School                                                                   |
| EFCB372D-C7AE-E311-B8ED-005056822391 | 10000866    | Brentside High School                                                                  |
| EC2F73F7-C6AE-E311-B8ED-005056822391 | 10014861    | Ashburton Community School                                                             |
| 24D291DF-C6AE-E311-B8ED-005056822391 | NULL        | King David Junior School                                                               |
| D3DB7AF1-C6AE-E311-B8ED-005056822391 | 10014023    | The Peele School                                                                       |
| 3D3E8F49-C7AE-E311-B8ED-005056822391 | 10000194    | Albany School                                                                          |
| 21E74C15-C7AE-E311-B8ED-005056822391 | 10016212    | Kingsmead Community School                                                             |
| 89D45197-C7AE-E311-B8ED-005056822391 | 10029323    | St Piers School                                                                        |
| 4F3D451B-C7AE-E311-B8ED-005056822391 | 10006280    | St Bonaventure's RC School                                                             |
| 3E63F591-C6AE-E311-B8ED-005056822391 | 10071935    | Raglan Infant School                                                                   |
| 778E540F-C7AE-E311-B8ED-005056822391 | 10003946    | Lipson Community College                                                               |
| 6EDA7AF1-C6AE-E311-B8ED-005056822391 | 10001492    | Claverham Community College                                                            |
| 8F6ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Poplars CP School                                                                      |
| CFE16303-C7AE-E311-B8ED-005056822391 | 10000232    | Alleyne's High School                                                                  |
| D45E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Chantry Primary School                                                                 |
| D590540F-C7AE-E311-B8ED-005056822391 | 10003607    | Kimberley Secondary School                                                             |
| 708E540F-C7AE-E311-B8ED-005056822391 | 10015436    | Fort Hill Community School                                                             |
| B9DE6303-C7AE-E311-B8ED-005056822391 | 10016188    | Kelmscott School                                                                       |
| A690540F-C7AE-E311-B8ED-005056822391 | 10015923    | Holte School                                                                           |
| 781DB8C1-C6AE-E311-B8ED-005056822391 | 10075092    | Reigate Priory School                                                                  |
| 778282EB-C6AE-E311-B8ED-005056822391 | 10001402    | Chislehurst and Sidcup Grammer School                                                  |
| 06876BFD-C6AE-E311-B8ED-005056822391 | 10017511    | Raynes High School                                                                     |
| 109A6F6D-C7AE-E311-B8ED-005056822391 | 10040195    | Uppingham School                                                                       |
| 9ECCA8CD-C6AE-E311-B8ED-005056822391 | 10069384    | Hotspur Primary School                                                                 |
| 3263F591-C6AE-E311-B8ED-005056822391 | 10070038    | Bonner Primary School                                                                  |
| 68FE7261-C7AE-E311-B8ED-005056822391 | 10017152    | Grantham CE School                                                                     |
| 3D96874F-C7AE-E311-B8ED-005056822391 | 10006683    | Hayes Manor School                                                                     |
| 5EDA7AF1-C6AE-E311-B8ED-005056822391 | 10015941    | Hyde Technology College                                                                |
| 87D87AF1-C6AE-E311-B8ED-005056822391 | 10015968    | Thorns School and Community                                                            |
| 8AE44C15-C7AE-E311-B8ED-005056822391 | 10001417    | Christ's College                                                                       |
| EDE06303-C7AE-E311-B8ED-005056822391 | 10000736    | Bishop's Hatfield Girls' School                                                        |
| F8996F6D-C7AE-E311-B8ED-005056822391 | 10003658    | The Kings School                                                                       |
| 583073F7-C6AE-E311-B8ED-005056822391 | NULL        | Lytchett Minster                                                                       |
| A8298AE5-C6AE-E311-B8ED-005056822391 | 10070745    | St John's RC Primary School                                                            |
| 2FE06303-C7AE-E311-B8ED-005056822391 | 10015873    | Frederick Gough School                                                                 |
| 3C3273F7-C6AE-E311-B8ED-005056822391 | 10003563    | Kennet School                                                                          |
| 4C671D28-66D4-E911-A956-000D3A2AADAC | NULL        | OT                                                                                     |
| 4812E69D-C6AE-E311-B8ED-005056822391 | 10069643    | Accrington Peel Park County Primary School                                             |
| 938F540F-C7AE-E311-B8ED-005056822391 | 10005593    | Rydens School                                                                          |
| A1896BFD-C6AE-E311-B8ED-005056822391 | 10006650    | The Denes High School                                                                  |
| 792A8AE5-C6AE-E311-B8ED-005056822391 | 10073747    | Jewish Primary School                                                                  |
| 6FC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Hexthorpe Primary School                                                               |
| 44CC372D-C7AE-E311-B8ED-005056822391 | 10001089    | Calday Grange Grammar School                                                           |
| E33C451B-C7AE-E311-B8ED-005056822391 | NULL        | Magdalen College School                                                                |
| 258182EB-C6AE-E311-B8ED-005056822391 | 10016984    | The James Hornsby High School                                                          |
| 1C3373F7-C6AE-E311-B8ED-005056822391 | 10006592    | The Barclay School                                                                     |
| 91DB7AF1-C6AE-E311-B8ED-005056822391 | 10006929    | Todmorden High School                                                                  |
| 5F2F73F7-C6AE-E311-B8ED-005056822391 | 10016210    | De Lacy Academy                                                                        |
| F6BBED97-C6AE-E311-B8ED-005056822391 | NULL        | Westgarth Primary School                                                               |
| 59395C09-C7AE-E311-B8ED-005056822391 | 10015645    | Weydon School                                                                          |
| 501ACFAF-C6AE-E311-B8ED-005056822391 | NULL        | Gloucester School                                                                      |
| 08BCED97-C6AE-E311-B8ED-005056822391 | 10079783    | The Vale First and Middle School                                                       |
| 64E64C15-C7AE-E311-B8ED-005056822391 | 10017621    | The Connaugh School                                                                    |
| 4F0DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Upcroft Primary School                                                                 |
| 15859443-C7AE-E311-B8ED-005056822391 | NULL        | Our Lady's Catholic High                                                               |
| 09FF7261-C7AE-E311-B8ED-005056822391 | 10004231    | Mascalls School                                                                        |
| 0D8182EB-C6AE-E311-B8ED-005056822391 | 10001383    | Chichester High School for Boys                                                        |
| C020B8C1-C6AE-E311-B8ED-005056822391 | 10076954    | Fernhill Primary School                                                                |
| 2890540F-C7AE-E311-B8ED-005056822391 | 10002879    | Hamstead Hall School                                                                   |
| 11ED7F55-C7AE-E311-B8ED-005056822391 | 10008008    | St Peter's Catholic Comprehensive School                                               |
| 60AF3A27-C7AE-E311-B8ED-005056822391 | 10075984    | Our Lady of Victories RC School                                                        |
| 9E3D451B-C7AE-E311-B8ED-005056822391 | 10001258    | Central Foundation Girls' School                                                       |
| EB375C09-C7AE-E311-B8ED-005056822391 | NULL        | Freebrough Community College                                                           |
| D3DE6303-C7AE-E311-B8ED-005056822391 | 10006702    | The Hundred of Hoo School                                                              |
| 5B68DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Woodhouse Junior School                                                                |
| 45FE7261-C7AE-E311-B8ED-005056822391 | 10001352    | Chelmer Valley High School                                                             |
| A2615C8B-C7AE-E311-B8ED-005056822391 | 10015609    | Danes Hill Preparatory School                                                          |
| CC385C09-C7AE-E311-B8ED-005056822391 | 10002575    | Freman College                                                                         |
| 01FF7261-C7AE-E311-B8ED-005056822391 | 10015470    | Gable Hall School                                                                      |
| 4E8282EB-C6AE-E311-B8ED-005056822391 | 10003790    | Launceston College                                                                     |
| 7596874F-C7AE-E311-B8ED-005056822391 | 10000597    | Beauchamps High School                                                                 |
| E5FE7261-C7AE-E311-B8ED-005056822391 | 10006170    | St Gregory's Catholic Comprehensive School                                             |
| 3D896BFD-C6AE-E311-B8ED-005056822391 | 10000606    | Beckfoot School                                                                        |
| B4896BFD-C6AE-E311-B8ED-005056822391 | 10000783    | Blythe Bridge High School                                                              |
| 07E74C15-C7AE-E311-B8ED-005056822391 | 10015398    | Burnt Mill School                                                                      |
| 9A3173F7-C6AE-E311-B8ED-005056822391 | 10007964    | Melbourn Village College                                                               |
| 158E540F-C7AE-E311-B8ED-005056822391 | 10002920    | Hartsdown Technology College                                                           |
| 1D3F451B-C7AE-E311-B8ED-005056822391 | 10006160    | St Francis of Assisi                                                                   |
| F5A77A5B-C7AE-E311-B8ED-005056822391 | 10015945    | The Winston Churchill School                                                           |
| 1D3073F7-C6AE-E311-B8ED-005056822391 | 10006672    | The Grange School                                                                      |
| CFAC7134-CAAE-E311-B8ED-005056822391 | NULL        | The University of Bristol Teach First                                                  |
| DFB30486-C6AE-E311-B8ED-005056822391 | 10077340    | Savile Park Primary School                                                             |
| D26EC7B5-C6AE-E311-B8ED-005056822391 | 10079832    | Elangeni School                                                                        |
| E3CB372D-C7AE-E311-B8ED-005056822391 | 10008672    | Riddlesdown High School                                                                |
| 688D540F-C7AE-E311-B8ED-005056822391 | 10018690    | Churnet View Middle                                                                    |
| AD3D451B-C7AE-E311-B8ED-005056822391 | 10015251    | Cathedral Academy                                                                      |
| CCDE6303-C7AE-E311-B8ED-005056822391 | 10007325    | Walton Community School                                                                |
| 138282EB-C6AE-E311-B8ED-005056822391 | 10017599    | The Chafford School                                                                    |
| 362A8AE5-C6AE-E311-B8ED-005056822391 | 10073748    | Wolfson Hillel Primary School                                                          |
| 571ACFAF-C6AE-E311-B8ED-005056822391 | 10075307    | The William Penn School                                                                |
| 1523A1D3-C6AE-E311-B8ED-005056822391 | 10070500    | Newborough C of E Primary School                                                       |
| FBA67A5B-C7AE-E311-B8ED-005056822391 | NULL        | Blenheim High School                                                                   |
| D5849443-C7AE-E311-B8ED-005056822391 | 10014798    | Abbs Cross School                                                                      |
| 23F63F21-C7AE-E311-B8ED-005056822391 | NULL        | St Theodore's RC High School & Sixth Form Centre                                       |
| 35CC372D-C7AE-E311-B8ED-005056822391 | NULL        | The Streetly School                                                                    |
| 62CC372D-C7AE-E311-B8ED-005056822391 | 10000618    | Beechen Cliff School                                                                   |
| 6CA77A5B-C7AE-E311-B8ED-005056822391 | 10006909    | Thurstable School                                                                      |
| 613E8F49-C7AE-E311-B8ED-005056822391 | 10005386    | Rastrick High School                                                                   |
| D0C6BFBB-C6AE-E311-B8ED-005056822391 | 10073031    | Blenheim Primary School                                                                |
| 5E1DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Mansfield Green Community School                                                       |
| 9BE64C15-C7AE-E311-B8ED-005056822391 | 10016462    | Park Community School                                                                  |
| 47F73F21-C7AE-E311-B8ED-005056822391 | 10006120    | St Bede's School                                                                       |
| 193F8F49-C7AE-E311-B8ED-005056822391 | 10005341    | Queen Marys High                                                                       |
| 103F451B-C7AE-E311-B8ED-005056822391 | NULL        | St Augustine of Canterbury School                                                      |
| F47A589D-C7AE-E311-B8ED-005056822391 | 10015127    | Bower Grove School                                                                     |
| 1DDB7AF1-C6AE-E311-B8ED-005056822391 | 10001101    | Caludon Castle School                                                                  |
| 6F395C09-C7AE-E311-B8ED-005056822391 | 10007128    | Ulverston Victoria High School                                                         |
| B8A77A5B-C7AE-E311-B8ED-005056822391 | 10015399    | Camshill School                                                                        |
| 9B1A5791-C7AE-E311-B8ED-005056822391 | 10015589    | Haslemere Prep School                                                                  |
| C78D540F-C7AE-E311-B8ED-005056822391 | 10003949    | Liskeard School and Community College                                                  |
| 37615C8B-C7AE-E311-B8ED-005056822391 | 10017042    | Riverston Independent School                                                           |
| 713173F7-C6AE-E311-B8ED-005056822391 | 10007478    | Whitby Community College                                                               |
| 29DB7AF1-C6AE-E311-B8ED-005056822391 | 10006824    | Sydney Russell School                                                                  |
| 59C7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Greyfriars Primary School                                                              |
| 1AE06303-C7AE-E311-B8ED-005056822391 | 10006741    | The Lindsey School & Community Arts College                                            |
| A912E69D-C6AE-E311-B8ED-005056822391 | NULL        | Colley Lane County Primary School                                                      |
| 030D6579-C7AE-E311-B8ED-005056822391 | 10003772    | Lancing College                                                                        |
| 6E7A589D-C7AE-E311-B8ED-005056822391 | 10010767    | Royal School for Deaf Derby                                                            |
| 6DE64C15-C7AE-E311-B8ED-005056822391 | 10015305    | Geoffrey Chaucer Technology College                                                    |
| AD96874F-C7AE-E311-B8ED-005056822391 | 10006183    | St James' Catholic High School                                                         |
| 47DB7AF1-C6AE-E311-B8ED-005056822391 | 10015626    | Willingsworth High School                                                              |
| 7E3073F7-C6AE-E311-B8ED-005056822391 | 10015847    | Hazel Grove High                                                                       |
| D20BFD8B-C6AE-E311-B8ED-005056822391 | 10069862    | Langtoft Primary School                                                                |
| 43DA7AF1-C6AE-E311-B8ED-005056822391 | NULL        | The Purbeck School                                                                     |
| FC7B99D9-C6AE-E311-B8ED-005056822391 | 10074704    | St Pauls CE Primary School                                                             |
| 63ED7F55-C7AE-E311-B8ED-005056822391 | 10002752    | Great Marlow School                                                                    |
| 49896BFD-C6AE-E311-B8ED-005056822391 | 10002964    | Headlands School                                                                       |
| CF3B451B-C7AE-E311-B8ED-005056822391 | 10004911    | Otley Prince Henry's Grammar School Specialist Language College                        |
| FB365C09-C7AE-E311-B8ED-005056822391 | 10015454    | Court Moor School                                                                      |
| 7ED391DF-C6AE-E311-B8ED-005056822391 | 10076876    | St Thomas A Becket                                                                     |
| FD8D540F-C7AE-E311-B8ED-005056822391 | 10015239    | Calthorpe Park School                                                                  |
| 558282EB-C6AE-E311-B8ED-005056822391 | 10000400    | Ashfield School                                                                        |
| BBB03A27-C7AE-E311-B8ED-005056822391 | NULL        | Eastwood School                                                                        |
| 2871C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Dormanstown Primary School                                                             |
| 68DB7AF1-C6AE-E311-B8ED-005056822391 | 10000479    | Aylward School                                                                         |
| BB70C7B5-C6AE-E311-B8ED-005056822391 | 10079830    | Bradwell Village Middle School                                                         |
| 97385C09-C7AE-E311-B8ED-005056822391 | 10007575    | Wolsingham School                                                                      |
| 92355C09-C7AE-E311-B8ED-005056822391 | 10017312    | Perryfields High School                                                                |
| CD7C99D9-C6AE-E311-B8ED-005056822391 | 10077423    | Dartington C of E Primary School                                                       |
| A63F8F49-C7AE-E311-B8ED-005056822391 | 10015909    | The Holly Hall Foundation School                                                       |
| 3A90540F-C7AE-E311-B8ED-005056822391 | 10004465    | Mulberry School for Girls                                                              |
| F71A5791-C7AE-E311-B8ED-005056822391 | NULL        | Landau Forte City Technology College                                                   |
| D3056786-C8AE-E311-B8ED-005056822391 | NULL        | Manchester Metropolitan University                                                     |
| 15E54C15-C7AE-E311-B8ED-005056822391 | 10016431    | Mark Hall School                                                                       |
| AD1A7067-C7AE-E311-B8ED-005056822391 | 10003225    | Hylands School                                                                         |
| 05A87A5B-C7AE-E311-B8ED-005056822391 | 10004217    | Marlborough School                                                                     |
| 232A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Helens RC Primary School                                                            |
| F3E64C15-C7AE-E311-B8ED-005056822391 | 10016156    | Kingsbury School                                                                       |
| E3F53F21-C7AE-E311-B8ED-005056822391 | 10015863    | Holy Trinity Catholic School                                                           |
| E4096485-C7AE-E311-B8ED-005056822391 | 10008469    | Royal Hospital School                                                                  |
| B1E54C15-C7AE-E311-B8ED-005056822391 | 10018703    | Maiden Beech Middle School                                                             |
| 728182EB-C6AE-E311-B8ED-005056822391 | NULL        | Coombe Girls' School                                                                   |
| CEF53F21-C7AE-E311-B8ED-005056822391 | 10002674    | Guru Nanak Sikh College                                                                |
| F2D97AF1-C6AE-E311-B8ED-005056822391 | 10016258    | Longdendale High School                                                                |
| 27625C8B-C7AE-E311-B8ED-005056822391 | 10014868    | Balham Preparatory School                                                              |
| 21BAED97-C6AE-E311-B8ED-005056822391 | NULL        | Upton Cross Primary School                                                             |
| D6DA7AF1-C6AE-E311-B8ED-005056822391 | 10007601    | Woodbridge High School                                                                 |
| 3EF73F21-C7AE-E311-B8ED-005056822391 | 10006124    | St Benedicts RC High School                                                            |
| A20A6485-C7AE-E311-B8ED-005056822391 | 10014948    | Blossom House School                                                                   |
| 42E84C15-C7AE-E311-B8ED-005056822391 | 10004441    | Moulsham High School                                                                   |
| CA365C09-C7AE-E311-B8ED-005056822391 | 10006632    | Cherwell School                                                                        |
| 2CE06303-C7AE-E311-B8ED-005056822391 | 10017714    | Springhill High School                                                                 |
| 1CEE667F-C7AE-E311-B8ED-005056822391 | 10015620    | Epsom College                                                                          |
| BB6FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Holywell Primary School                                                                |
| 9FE16303-C7AE-E311-B8ED-005056822391 | 10015422    | Denbigh High School                                                                    |
| 57DF6303-C7AE-E311-B8ED-005056822391 | 10002754    | Great Wryley High School                                                               |
| F5DB7AF1-C6AE-E311-B8ED-005056822391 | 10004979    | Park Hall School                                                                       |
| 9E2F73F7-C6AE-E311-B8ED-005056822391 | 10048204    | Oasis Academy Brislington                                                              |
| 611A7067-C7AE-E311-B8ED-005056822391 | 10006263    | St Thomas More Catholic Primary School                                                 |
| D02F73F7-C6AE-E311-B8ED-005056822391 | 10015551    | Harrow High School                                                                     |
| 9CED7F55-C7AE-E311-B8ED-005056822391 | 10001009    | Bushey Hall School                                                                     |
| 57E74C15-C7AE-E311-B8ED-005056822391 | 10017308    | Belvidere Comprehensive                                                                |
| 17E84C15-C7AE-E311-B8ED-005056822391 | 10006850    | The Warwick School                                                                     |
| 94FE7261-C7AE-E311-B8ED-005056822391 | 10006139    | St Clements Danes School                                                               |
| A9E54C15-C7AE-E311-B8ED-005056822391 | 10000101    | Acland Burghley School                                                                 |
| 69E54C15-C7AE-E311-B8ED-005056822391 | NULL        | Malory School                                                                          |
| F696874F-C7AE-E311-B8ED-005056822391 | 10016707    | Lynn Grove V A High School                                                             |
| F2E06303-C7AE-E311-B8ED-005056822391 | 10003579    | Kesgrave High                                                                          |
| DAE44C15-C7AE-E311-B8ED-005056822391 | 10013305    | Woolwich Polytechnic School                                                            |
| F7F63F21-C7AE-E311-B8ED-005056822391 | 10017499    | St Matthews RC High School                                                             |
| 57375C09-C7AE-E311-B8ED-005056822391 | 10017559    | Ridgewood High School                                                                  |
| 88B9ED97-C6AE-E311-B8ED-005056822391 | 10078055    | Morden Mount Primary School                                                            |
| D38282EB-C6AE-E311-B8ED-005056822391 | 10005464    | Richard Hale School                                                                    |
| 48B50486-C6AE-E311-B8ED-005056822391 | 10069022    | Knop Law Primary School                                                                |
| 837A589D-C7AE-E311-B8ED-005056822391 | 10015616    | Wishmore Cross School                                                                  |
| 15CC372D-C7AE-E311-B8ED-005056822391 | 10017701    | Stratford School                                                                       |
| 1868DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Ash Green Primay School                                                                |
| D440BEF8-C9AE-E311-B8ED-005056822391 | NULL        | AOR Education Management Direct                                                        |
| C53D451B-C7AE-E311-B8ED-005056822391 | 10005873    | Sir Wlliam Borlase's Grammar School                                                    |
| D496874F-C7AE-E311-B8ED-005056822391 | 10016350    | Kingsdown School                                                                       |
| FF866BFD-C6AE-E311-B8ED-005056822391 | NULL        | Campion School                                                                         |
| 272A8AE5-C6AE-E311-B8ED-005056822391 | 10073732    | North Cheshire Jewish Primary School                                                   |
| C4C7BFBB-C6AE-E311-B8ED-005056822391 | 10073849    | Morningside Primary School                                                             |
| 153E451B-C7AE-E311-B8ED-005056822391 | 10000773    | Blue Coat Cof E Comprehensive School                                                   |
| 778F540F-C7AE-E311-B8ED-005056822391 | NULL        | Campian School                                                                         |
| 03E06303-C7AE-E311-B8ED-005056822391 | 10017115    | Beechwood School                                                                       |
| 560BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Hartley Primary School                                                                 |
| 408182EB-C6AE-E311-B8ED-005056822391 | 10008020    | Walton High School                                                                     |
| C062F591-C6AE-E311-B8ED-005056822391 | 10076576    | Cotteridge Junior & Infant School                                                      |
| 89859443-C7AE-E311-B8ED-005056822391 | 10003777    | Langley Park School for Boys                                                           |
| E8A67A5B-C7AE-E311-B8ED-005056822391 | 10002603    | Furherwick Park Foundation School                                                      |
| D53D451B-C7AE-E311-B8ED-005056822391 | 10003637    | King Edward VII                                                                        |
| 9CF83F21-C7AE-E311-B8ED-005056822391 | NULL        | North West London Jewish Primary School                                                |
| FE0A6485-C7AE-E311-B8ED-005056822391 | 10008312    | King Edward's School Witley                                                            |
| E70DFD8B-C6AE-E311-B8ED-005056822391 | 10079724    | Ben Jonson Primary School                                                              |
| F91CB8C1-C6AE-E311-B8ED-005056822391 | 10073851    | Primrose Hill Primary School                                                           |
| E4A67A5B-C7AE-E311-B8ED-005056822391 | 10000374    | Arthur Mellows Village College                                                         |
| 9BDA7AF1-C6AE-E311-B8ED-005056822391 | 10001499    | Cleeve Park School                                                                     |
| 12E84C15-C7AE-E311-B8ED-005056822391 | 10000724    | The Bishop David Brown School                                                          |
| F2AF3A27-C7AE-E311-B8ED-005056822391 | NULL        | St Joesephs RC GM Primary School                                                       |
| 3A3F451B-C7AE-E311-B8ED-005056822391 | 10016881    | Our Lady's RC High School                                                              |
| E910E69D-C6AE-E311-B8ED-005056822391 | 10072857    | Calverton Primary School                                                               |
| 3B96874F-C7AE-E311-B8ED-005056822391 | 10010903    | Norbury Manor High School for Girls                                                    |
| B0876BFD-C6AE-E311-B8ED-005056822391 | NULL        | Sir John Leman High School                                                             |
| CD3E8F49-C7AE-E311-B8ED-005056822391 | 10002023    | Downham Market High School                                                             |
| 6496874F-C7AE-E311-B8ED-005056822391 | 10006711    | The John Bentley School                                                                |
| F03F8F49-C7AE-E311-B8ED-005056822391 | 10005552    | The Royal Grammar School                                                               |
| 716EC7B5-C6AE-E311-B8ED-005056822391 | 10069752    | Hillborough Infant and Nursery School                                                  |
| D4A77A5B-C7AE-E311-B8ED-005056822391 | 10005747    | Senacre Technology College                                                             |
| 432C8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Holy Family Catholic School                                                            |
| B395874F-C7AE-E311-B8ED-005056822391 | 10001701    | Costessey High School                                                                  |
| DFDB7AF1-C6AE-E311-B8ED-005056822391 | NULL        | Westley Middle School                                                                  |
| C696874F-C7AE-E311-B8ED-005056822391 | 10000261    | Altrincham Girl's Grammar School                                                       |
| 963F8F49-C7AE-E311-B8ED-005056822391 | 10001558    | Coloma Convent Girls' School                                                           |
| 013C451B-C7AE-E311-B8ED-005056822391 | 10004131    | Lymm High School                                                                       |
| AF8082EB-C6AE-E311-B8ED-005056822391 | NULL        | St John Fisher RC First School                                                         |
| F1B9ED97-C6AE-E311-B8ED-005056822391 | 10073648    | Christchurch Primary School                                                            |
| 87DF6303-C7AE-E311-B8ED-005056822391 | 10001510    | Clough Hall Technology School                                                          |
| D2365C09-C7AE-E311-B8ED-005056822391 | 10011781    | Hillcrest School & Community College                                                   |
| 2FD55197-C7AE-E311-B8ED-005056822391 | 10015758    | Elms Bank High                                                                         |
| E1385C09-C7AE-E311-B8ED-005056822391 | 10017568    | Priory Community School                                                                |
| C612E69D-C6AE-E311-B8ED-005056822391 | 10072629    | Pownall Green Primary School                                                           |
| 87F63F21-C7AE-E311-B8ED-005056822391 | 10006288    | St Mary's Catholic High School                                                         |
| B9B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Eastwood                                                                               |
| E3849443-C7AE-E311-B8ED-005056822391 | 10002823    | Gunnersbury Catholic School                                                            |
| 76D591DF-C6AE-E311-B8ED-005056822391 | 10079443    | St Thomas More Catholic School                                                         |
| D40C6579-C7AE-E311-B8ED-005056822391 | 10003213    | Hurst Lodge School                                                                     |
| F3876BFD-C6AE-E311-B8ED-005056822391 | 10005663    | Sandhurst School                                                                       |
| 5B17CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Claremont Primary                                                                      |
| B062F591-C6AE-E311-B8ED-005056822391 | 10075052    | Raglan Junior School                                                                   |
| 0663F591-C6AE-E311-B8ED-005056822391 | NULL        | Southlake Primary                                                                      |
| 18DA7AF1-C6AE-E311-B8ED-005056822391 | 10002139    | Eastbury Comprehensive School                                                          |
| 82C7BFBB-C6AE-E311-B8ED-005056822391 | 10073072    | Moorhill Primary School                                                                |
| AF3273F7-C6AE-E311-B8ED-005056822391 | 10015278    | Framlingham Earl High School                                                           |
| F7AF3A27-C7AE-E311-B8ED-005056822391 | NULL        | Castleview School                                                                      |
| 4B8482EB-C6AE-E311-B8ED-005056822391 | 10015585    | Egerton Parks Art College                                                              |
| 31E16303-C7AE-E311-B8ED-005056822391 | 10006595    | Benjamin Britten High                                                                  |
| 25FE7261-C7AE-E311-B8ED-005056822391 | 10007077    | Tunbridge Wells High School                                                            |
| BEC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Mountfields Lodge School                                                               |
| C2DB7AF1-C6AE-E311-B8ED-005056822391 | 10016618    | Newland School for Girls                                                               |
| 716FC7B5-C6AE-E311-B8ED-005056822391 | 10077855    | Braybrook County Primary Schoolq                                                       |
| 72E44C15-C7AE-E311-B8ED-005056822391 | 10001218    | Castle Community School                                                                |
| 3A20B8C1-C6AE-E311-B8ED-005056822391 | 10073214    | Rickstones School                                                                      |
| D1A77A5B-C7AE-E311-B8ED-005056822391 | 10002713    | Goffs School                                                                           |
| C5886BFD-C6AE-E311-B8ED-005056822391 | 10003152    | Hornsea School                                                                         |
| 29395C09-C7AE-E311-B8ED-005056822391 | 10006671    | The Grange School and Sports College                                                   |
| 3562F591-C6AE-E311-B8ED-005056822391 | 10072634    | Lark Hill Primary School                                                               |
| 8419CFAF-C6AE-E311-B8ED-005056822391 | 10074567    | Norton Road Primary School                                                             |
| E48182EB-C6AE-E311-B8ED-005056822391 | 10002045    | Droitwich Spa High School                                                              |
| EE8D540F-C7AE-E311-B8ED-005056822391 | 10007311    | Walmer School                                                                          |
| 1ADB7AF1-C6AE-E311-B8ED-005056822391 | NULL        | Thorne Grammar School                                                                  |
| 7EE16303-C7AE-E311-B8ED-005056822391 | 10002938    | Haverstock School                                                                      |
| 83B30486-C6AE-E311-B8ED-005056822391 | NULL        | Floweryfield County Primary School                                                     |
| DD8282EB-C6AE-E311-B8ED-005056822391 | 10018148    | Pickering High School                                                                  |
| 8790540F-C7AE-E311-B8ED-005056822391 | 10015819    | Wellfield Community School                                                             |
| 51F63F21-C7AE-E311-B8ED-005056822391 | 10015008    | St Christopher's CE High School                                                        |
| 30DE6303-C7AE-E311-B8ED-005056822391 | 10015773    | Elliott Durham School                                                                  |
| F9866BFD-C6AE-E311-B8ED-005056822391 | 10006617    | The Bulmershe School                                                                   |
| 2D8382EB-C6AE-E311-B8ED-005056822391 | 10003581    | Kesteven & Sleaford High                                                               |
| 7A3E451B-C7AE-E311-B8ED-005056822391 | 10006662    | The English Martyrs School                                                             |
| 4226A1D3-C6AE-E311-B8ED-005056822391 | 10075817    | Caistor Church                                                                         |
| AAF63F21-C7AE-E311-B8ED-005056822391 | 10003263    | Immanuel CE Community College                                                          |
| 663173F7-C6AE-E311-B8ED-005056822391 | 10015368    | Cheadle Hulme College                                                                  |
| 1FD391DF-C6AE-E311-B8ED-005056822391 | 10076877    | Sacred Heart Catholic School                                                           |
| B0A58822-CAAE-E311-B8ED-005056822391 | NULL        | AOR Hull Citywide Partnership                                                          |
| 9FA77A5B-C7AE-E311-B8ED-005056822391 | 10004639    | Nicholas Breakspear Roman Catholic School                                              |
| 258D540F-C7AE-E311-B8ED-005056822391 | 10017405    | Sinfin Community School                                                                |
| C10A6485-C7AE-E311-B8ED-005056822391 | 10008518    | St Mary's School                                                                       |
| 21A77A5B-C7AE-E311-B8ED-005056822391 | 10016396    | The Gleed Girls' Technology College                                                    |
| 3F3C451B-C7AE-E311-B8ED-005056822391 | 10000800    | Borden Grammar School                                                                  |
| 6DFF7261-C7AE-E311-B8ED-005056822391 | 10000869    | Brentwood Ursuline Convent High School                                                 |
| B33C451B-C7AE-E311-B8ED-005056822391 | 10007285    | Wadham Community School                                                                |
| E9E44C15-C7AE-E311-B8ED-005056822391 | 10008304    | Judgemeadow Community School                                                           |
| 57D97AF1-C6AE-E311-B8ED-005056822391 | 10003167    | Hounslow Manor School                                                                  |
| 02896BFD-C6AE-E311-B8ED-005056822391 | 10016129    | Usworth School                                                                         |
| 2C365C09-C7AE-E311-B8ED-005056822391 | 10015764    | Henry Compton School                                                                   |
| A31B5791-C7AE-E311-B8ED-005056822391 | 10017495    | Shaftesbury High School                                                                |
| 71536A73-C7AE-E311-B8ED-005056822391 | 10047306    | Southwark Independent School                                                           |
| 90996F6D-C7AE-E311-B8ED-005056822391 | 10008271    | Highgate School                                                                        |
| 972F73F7-C6AE-E311-B8ED-005056822391 | 10004330    | Mexborough School                                                                      |
| 628282EB-C6AE-E311-B8ED-005056822391 | 10003487    | John Hampden Grammar School                                                            |
| 30FE7261-C7AE-E311-B8ED-005056822391 | 10003113    | Hockerill Anglo European School                                                        |
| 44FF7261-C7AE-E311-B8ED-005056822391 | 10014808    | Barstable School                                                                       |
| C590540F-C7AE-E311-B8ED-005056822391 | 10015403    | The Four Dwelling's High School                                                        |
| F2866BFD-C6AE-E311-B8ED-005056822391 | 10004091    | Longcroft School                                                                       |
| 26FF7261-C7AE-E311-B8ED-005056822391 | 10000296    | Anglo European School                                                                  |
| 90E44C15-C7AE-E311-B8ED-005056822391 | 10018518    | Danley Middle School                                                                   |
| CDED667F-C7AE-E311-B8ED-005056822391 | 10016478    | Farnborough Hill                                                                       |
| 4D3E8F49-C7AE-E311-B8ED-005056822391 | 10000204    | Aldersley High School                                                                  |
| 8476B0C7-C6AE-E311-B8ED-005056822391 | NULL        | South Parade Infants School                                                            |
| 458A6BFD-C6AE-E311-B8ED-005056822391 | 10005539    | Roundwood Park                                                                         |
| 420DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Outlane Infant School                                                                  |
| D68F540F-C7AE-E311-B8ED-005056822391 | 10015838    | Hodge Hill School                                                                      |
| 1CC0D6A9-C6AE-E311-B8ED-005056822391 | 10072919    | Deptford Park Primary School                                                           |
| 338382EB-C6AE-E311-B8ED-005056822391 | 10003000    | The Hemel Hempstead School                                                             |
| 223F8F49-C7AE-E311-B8ED-005056822391 | 10015044    | Blessed Thomas Holford School                                                          |
| 1496874F-C7AE-E311-B8ED-005056822391 | 10001830    | Dallam School                                                                          |
| A5886BFD-C6AE-E311-B8ED-005056822391 | NULL        | Gosford Hill School                                                                    |
| 03F73F21-C7AE-E311-B8ED-005056822391 | 10016015    | Trinity C of E High School                                                             |
| 9BBCED97-C6AE-E311-B8ED-005056822391 | NULL        | Greenholm Primary School                                                               |
| 76F83F21-C7AE-E311-B8ED-005056822391 | 10015453    | Cardinal Wiseman Catholic School                                                       |
| C36FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Dowson First School                                                                    |
| 211DB8C1-C6AE-E311-B8ED-005056822391 | 10071322    | Penwortham School                                                                      |
| 310CFD8B-C6AE-E311-B8ED-005056822391 | 10080068    | Garfield Primary School                                                                |
| 3E77B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Barwell CE Junior School                                                               |
| E61A7067-C7AE-E311-B8ED-005056822391 | 10006204    | St Josephs College                                                                     |
| DDC1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Hazel Slade County Primary School                                                      |
| 4BA87A5B-C7AE-E311-B8ED-005056822391 | 10007510    | William De Ferrers                                                                     |
| F1CDA8CD-C6AE-E311-B8ED-005056822391 | 10074237    | Hook Norton Primary School                                                             |
| 7AC9BFBB-C6AE-E311-B8ED-005056822391 | 10072253    | Castleton Primary School                                                               |
| A0E74C15-C7AE-E311-B8ED-005056822391 | 10001776    | Croxteth Community College                                                             |
| B4C7BFBB-C6AE-E311-B8ED-005056822391 | 10068765    | Gledhow Primary School                                                                 |
| 58E84C15-C7AE-E311-B8ED-005056822391 | 10006631    | The Chauncy School                                                                     |
| A83C451B-C7AE-E311-B8ED-005056822391 | 10001323    | Charles Edward Brooke                                                                  |
| 5164F591-C6AE-E311-B8ED-005056822391 | 10077210    | The Close Junior School                                                                |
| F9D291DF-C6AE-E311-B8ED-005056822391 | 10068564    | St Anne's C of E Junior and Infant School                                              |
| 6BEE667F-C7AE-E311-B8ED-005056822391 | 10008467    | Royal Grammar School                                                                   |
| 46886BFD-C6AE-E311-B8ED-005056822391 | 10017033    | The Tennyson High School                                                               |
| 3471C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Eyrescroft Primary School                                                              |
| 8C6EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Lambwath Primary School                                                                |
| EAC72DD3-C7AE-E311-B8ED-005056822391 | 10007856    | Aberystwyth University                                                                 |
| DE8E540F-C7AE-E311-B8ED-005056822391 | 10001880    | De Ferrers Specialist Technology College                                               |
| 4DDA7AF1-C6AE-E311-B8ED-005056822391 | 10006910    | Thurston Community College                                                             |
| 153D451B-C7AE-E311-B8ED-005056822391 | 10000729    | Bishop Ramsey C of E School                                                            |
| 023E451B-C7AE-E311-B8ED-005056822391 | 10003098    | Hinchingbrooke School                                                                  |
| 89E54C15-C7AE-E311-B8ED-005056822391 | 10005102    | Pimlico School                                                                         |
| 08408F49-C7AE-E311-B8ED-005056822391 | 10014860    | Windsor High School                                                                    |
| F0385C09-C7AE-E311-B8ED-005056822391 | 10002911    | Harrow Way Community School                                                            |
| F53E8F49-C7AE-E311-B8ED-005056822391 | 10005344    | Queen's Park Community School                                                          |
| 32B03A27-C7AE-E311-B8ED-005056822391 | 10070225    | St Joseph's Convent Catholic Primary                                                   |
| 9CED667F-C7AE-E311-B8ED-005056822391 | 10008382    | Millfields                                                                             |
| B98D540F-C7AE-E311-B8ED-005056822391 | 10007294    | Walderslade Girls' School                                                              |
| 0B8482EB-C6AE-E311-B8ED-005056822391 | 10006304    | Stanborough School                                                                     |
| 618D540F-C7AE-E311-B8ED-005056822391 | 10005421    | Redruth School: A Technology College                                                   |
| 1019CFAF-C6AE-E311-B8ED-005056822391 | 10074033    | South Wootton First School                                                             |
| BD866BFD-C6AE-E311-B8ED-005056822391 | NULL        | Matthew Murray High School                                                             |
| 1DCDA8CD-C6AE-E311-B8ED-005056822391 | 10079347    | Edensor Church of England Primary School                                               |
| 80B03A27-C7AE-E311-B8ED-005056822391 | 10080467    | Hertfordshire GM School                                                                |
| 12536A73-C7AE-E311-B8ED-005056822391 | 10005159    | Portsmouth Grammar School                                                              |
| D3E54C15-C7AE-E311-B8ED-005056822391 | 10015208    | Cedar Mount High School                                                                |
| 748282EB-C6AE-E311-B8ED-005056822391 | 10016299    | Sanders Draper School                                                                  |
| C810E69D-C6AE-E311-B8ED-005056822391 | NULL        | Tedburn St Mary School                                                                 |
| 038F540F-C7AE-E311-B8ED-005056822391 | 10001339    | Chase Terrace High School                                                              |
| 92A77A5B-C7AE-E311-B8ED-005056822391 | 10015134    | Caister High School                                                                    |
| D11A7067-C7AE-E311-B8ED-005056822391 | 10006157    | St Edwards College                                                                     |
| 11EE7F55-C7AE-E311-B8ED-005056822391 | 10013303    | The Castle Hill GM Community School                                                    |
| E5385C09-C7AE-E311-B8ED-005056822391 | 10016261    | The Romsey School                                                                      |
| F01A7067-C7AE-E311-B8ED-005056822391 | 10078093    | Menorah Grammar School                                                                 |
| 79FE7261-C7AE-E311-B8ED-005056822391 | 10015376    | Cirencester Deer Park School                                                           |
| 22FF7261-C7AE-E311-B8ED-005056822391 | 10017186    | The Gilberd School                                                                     |
| 9F77B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Raynsford VC Lower School                                                              |
| 48B9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Glebe Primary School                                                                   |
| 96E06303-C7AE-E311-B8ED-005056822391 | 10006903    | Thurleston High School                                                                 |
| 740C6579-C7AE-E311-B8ED-005056822391 | 10016439    | St Andrew's School                                                                     |
| C5F53F21-C7AE-E311-B8ED-005056822391 | 10017335    | Sacred Heart Roman Catholic Comprehensive School                                       |
| BD385C09-C7AE-E311-B8ED-005056822391 | 10015205    | Wyvern Community School                                                                |
| 1196874F-C7AE-E311-B8ED-005056822391 | 10006603    | Blessed Robert                                                                         |
| 083C451B-C7AE-E311-B8ED-005056822391 | NULL        | Habergham High School                                                                  |
| 1FEEAE04-CAAE-E311-B8ED-005056822391 | 10034267    | Runwell Community Primary School                                                       |
| 893273F7-C6AE-E311-B8ED-005056822391 | 10014951    | Batley Business and Enterprise College                                                 |
| 5CD87AF1-C6AE-E311-B8ED-005056822391 | 10017436    | Rose Bridge High School                                                                |
| B9E64C15-C7AE-E311-B8ED-005056822391 | 10006864    | William Morris Academy                                                                 |
| 1CC2D6A9-C6AE-E311-B8ED-005056822391 | 10068925    | Barmston Village Primary School                                                        |
| DE3F8F49-C7AE-E311-B8ED-005056822391 | 10001340    | Chasetown High School                                                                  |
| D4D391DF-C6AE-E311-B8ED-005056822391 | 10076004    | Our Lady and St Joseph RC Primary School                                               |
| 371FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Newlands Primary School                                                                |
| 4B859443-C7AE-E311-B8ED-005056822391 | 10005401    | Reading School                                                                         |
| A48182EB-C6AE-E311-B8ED-005056822391 | 10001220    | Castle Manor Community Upper School                                                    |
| 60FF7261-C7AE-E311-B8ED-005056822391 | 10000867    | Brentwood County High                                                                  |
| 25B30486-C6AE-E311-B8ED-005056822391 | 10071943    | Carlton Vale Infant School                                                             |
| 5025A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Weston All Saints CofE Primary School                                                  |
| D83D451B-C7AE-E311-B8ED-005056822391 | 10007046    | Tring School                                                                           |
| B6DF6303-C7AE-E311-B8ED-005056822391 | 10007351    | Washwood Heath Technology School                                                       |
| B33F8F49-C7AE-E311-B8ED-005056822391 | 10016779    | Marshland High School                                                                  |
| 72FF7261-C7AE-E311-B8ED-005056822391 | 10006200    | St John's RC Comprehensive School                                                      |
| D63C451B-C7AE-E311-B8ED-005056822391 | 10007093    | Twyford CE High School                                                                 |
| 8412E69D-C6AE-E311-B8ED-005056822391 | NULL        | Highfield Junior & Infant School                                                       |
| B9DA7AF1-C6AE-E311-B8ED-005056822391 | 10016055    | The Wordsley School                                                                    |
| 82AF3A27-C7AE-E311-B8ED-005056822391 | 10070741    | Manchester GM School                                                                   |
| 5E96874F-C7AE-E311-B8ED-005056822391 | 10007199    | Vale of Catmose College                                                                |
| 01859443-C7AE-E311-B8ED-005056822391 | 10006686    | The Heathfield Foundation Technology College                                           |
| D664F591-C6AE-E311-B8ED-005056822391 | 10073852    | Brecknock Primary School                                                               |
| E4D291DF-C6AE-E311-B8ED-005056822391 | 10073477    | Cartwright and Kelsey Church of England Primary Sc                                     |
| A16ADEA3-C6AE-E311-B8ED-005056822391 | 10079727    | Crampton Primary                                                                       |
| CA1DB8C1-C6AE-E311-B8ED-005056822391 | 10078045    | Sherington Primary School                                                              |
| 0096874F-C7AE-E311-B8ED-005056822391 | 10006284    | St John's School & Community College                                                   |
| 64385C09-C7AE-E311-B8ED-005056822391 | 10000100    | Acklam Grange                                                                          |
| 9F96874F-C7AE-E311-B8ED-005056822391 | 10006593    | The Beacon Shcool                                                                      |
| 0E375C09-C7AE-E311-B8ED-005056822391 | 10004942    | Oxford School                                                                          |
| 558182EB-C6AE-E311-B8ED-005056822391 | NULL        | William Gee School                                                                     |
| EC7C99D9-C6AE-E311-B8ED-005056822391 | 10080458    | Hertsmere Jewish Primary School                                                        |
| 3A886BFD-C6AE-E311-B8ED-005056822391 | 10015022    | Warbeck High School                                                                    |
| B9E16303-C7AE-E311-B8ED-005056822391 | 10003075    | Highfield's School                                                                     |
| 0FE06303-C7AE-E311-B8ED-005056822391 | 10014788    | Falinge Park High School                                                               |
| 285E0C80-C6AE-E311-B8ED-005056822391 | 10081457    | Victoria Dock Primary School                                                           |
| CE68DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Carlton Primary Academy                                                                |
| 88F53F21-C7AE-E311-B8ED-005056822391 | 10006244    | St Paul's Catholic College                                                             |
| D03173F7-C6AE-E311-B8ED-005056822391 | 10016174    | Kingsmeadow Community Comprehensive School                                             |
| 81DE6303-C7AE-E311-B8ED-005056822391 | 10006704    | The Immingham School                                                                   |
| 1EE74C15-C7AE-E311-B8ED-005056822391 | 10015782    | Heathfield School                                                                      |
| E50BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Corsham Primary School                                                                 |
| 241EB8C1-C6AE-E311-B8ED-005056822391 | 10075083    | St Giles Junior School                                                                 |
| 30ED7F55-C7AE-E311-B8ED-005056822391 | 10001010    | Bushey Meads School                                                                    |
| AADE6303-C7AE-E311-B8ED-005056822391 | 10017686    | The Causeway School                                                                    |
| C9E54C15-C7AE-E311-B8ED-005056822391 | 10017959    | Parklands High School                                                                  |
| 361ACFAF-C6AE-E311-B8ED-005056822391 | 10079855    | Warden Hill Junior School                                                              |
| 5EE84C15-C7AE-E311-B8ED-005056822391 | 10017787    | St Michael's High School                                                               |
| 55DB7AF1-C6AE-E311-B8ED-005056822391 | 10004117    | Loxford School of Science and Technology                                               |
| EC96874F-C7AE-E311-B8ED-005056822391 | 10001833    | Dame Alice Owen's School                                                               |
| A324A1D3-C6AE-E311-B8ED-005056822391 | 10076303    | St Margaret's CE Junior School                                                         |
| 8B19CFAF-C6AE-E311-B8ED-005056822391 | 10069737    | Calcot Infant School and Nursery                                                       |
| 37DE6303-C7AE-E311-B8ED-005056822391 | 10014049    | The King Edward VI School                                                              |
| 651A7067-C7AE-E311-B8ED-005056822391 | 10006163    | St Georges Church of England Foundation School                                         |
| B1B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Chestnut Street CE Primary School                                                      |
| D6ED7F55-C7AE-E311-B8ED-005056822391 | 10001689    | Corfe Hills School                                                                     |
| A8DA7AF1-C6AE-E311-B8ED-005056822391 | 10002256    | Enfield County School                                                                  |
| B6385C09-C7AE-E311-B8ED-005056822391 | 10004307    | Meriden School                                                                         |
| 068282EB-C6AE-E311-B8ED-005056822391 | 10002428    | Fearnhill School                                                                       |
| 0C3073F7-C6AE-E311-B8ED-005056822391 | 10005460    | Rhodesway School                                                                       |
| 6274B0C7-C6AE-E311-B8ED-005056822391 | 10078024    | Kingsmead Primary School                                                               |
| 40D87AF1-C6AE-E311-B8ED-005056822391 | 10006577    | The Alumwell School                                                                    |
| F36FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Parnwell Primary School                                                                |
| E7876BFD-C6AE-E311-B8ED-005056822391 | 10015641    | Earlsheaton Technology College                                                         |
| ED3B451B-C7AE-E311-B8ED-005056822391 | NULL        | Bourne Grammar School                                                                  |
| 0B8A6BFD-C6AE-E311-B8ED-005056822391 | 10018013    | Leytonstone School                                                                     |
| BB849443-C7AE-E311-B8ED-005056822391 | 10001002    | Burntwood School                                                                       |
| EC996F6D-C7AE-E311-B8ED-005056822391 | 10004971    | Pangbourne College                                                                     |
| 0D90540F-C7AE-E311-B8ED-005056822391 | 10006639    | Coleshill School                                                                       |
| E4C5BFBB-C6AE-E311-B8ED-005056822391 | 10069219    | Gorsemoor Primary School                                                               |
| FEE54C15-C7AE-E311-B8ED-005056822391 | 10017670    | Springfield School                                                                     |
| 71996F6D-C7AE-E311-B8ED-005056822391 | 10003660    | King's School, Rochester                                                               |
| DB1B5791-C7AE-E311-B8ED-005056822391 | 10016044    | John F Kennedy School                                                                  |
| 2EA77A5B-C7AE-E311-B8ED-005056822391 | 10017385    | St Benedicts Catholic Sports College                                                   |
| 1DD250A3-C7AE-E311-B8ED-005056822391 | 10016297    | Linden Lodge School                                                                    |
| 9E64F591-C6AE-E311-B8ED-005056822391 | 10076347    | Cherry Tree County Primary School                                                      |
| 600D6579-C7AE-E311-B8ED-005056822391 | 10013359    | Warminster Prep School                                                                 |
| 84876BFD-C6AE-E311-B8ED-005056822391 | 10015543    | Great Torrington Community School                                                      |
| AA8D540F-C7AE-E311-B8ED-005056822391 | 10017091    | Rodborough School                                                                      |
| 9074B0C7-C6AE-E311-B8ED-005056822391 | 10080706    | Beehive Lane Community Primary School                                                  |
| 343C451B-C7AE-E311-B8ED-005056822391 | 10005869    | Sir Joseph Williamson's Mathematical School                                            |
| 4B5E0C80-C6AE-E311-B8ED-005056822391 | 10070648    | St George's New Town Junior School                                                     |
| 97A77A5B-C7AE-E311-B8ED-005056822391 | NULL        | North Kesteven School                                                                  |
| D8A77A5B-C7AE-E311-B8ED-005056822391 | 10003641    | King Harold                                                                            |
| 8FF83F21-C7AE-E311-B8ED-005056822391 | 10000342    | Archbishop Ilsley RC School                                                            |
| 98E74C15-C7AE-E311-B8ED-005056822391 | 10016722    | Notley High School                                                                     |
| B80C6579-C7AE-E311-B8ED-005056822391 | 10000779    | Blundell's School                                                                      |
| A2ED7F55-C7AE-E311-B8ED-005056822391 | 10013283    | The Gartree Community School                                                           |
| CD69DEA3-C6AE-E311-B8ED-005056822391 | 10072103    | Heycroft Primary School                                                                |
| 04876BFD-C6AE-E311-B8ED-005056822391 | 10000791    | Bognor Regis Community College                                                         |
| F764F591-C6AE-E311-B8ED-005056822391 | NULL        | Friars Grove Junior                                                                    |
| 2F65F591-C6AE-E311-B8ED-005056822391 | NULL        | Langford Lower School                                                                  |
| D7375C09-C7AE-E311-B8ED-005056822391 | 10004770    | Norton Hill Academy                                                                    |
| 6B3E451B-C7AE-E311-B8ED-005056822391 | 10017667    | St Anne's RC School                                                                    |
| A58F540F-C7AE-E311-B8ED-005056822391 | 10006832    | The Towers School                                                                      |
| 553F451B-C7AE-E311-B8ED-005056822391 | 10001895    | Deanery CE High School                                                                 |
| FF90540F-C7AE-E311-B8ED-005056822391 | 10008493    | Sir Jonathan North Community College                                                   |
| 6FE54C15-C7AE-E311-B8ED-005056822391 | 10010607    | Fullhurst Community College                                                            |
| 02EE7F55-C7AE-E311-B8ED-005056822391 | 10015289    | Cartmel Priory C of E School                                                           |
| 9D65F591-C6AE-E311-B8ED-005056822391 | NULL        | Skelton Junior School                                                                  |
| A1BCED97-C6AE-E311-B8ED-005056822391 | 10075627    | Holmfirth Junior Infant and Nursery School                                             |
| 177B589D-C7AE-E311-B8ED-005056822391 | 10014884    | Falconer School                                                                        |
| 5BE16303-C7AE-E311-B8ED-005056822391 | 10005988    | Rodillian Academy                                                                      |
| D969DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Hoyland Common Primary School                                                          |
| 5A1A7067-C7AE-E311-B8ED-005056822391 | 10006027    | Southend High School for Boys                                                          |
| 353273F7-C6AE-E311-B8ED-005056822391 | 10016099    | Trentham High School                                                                   |
| F26ADEA3-C6AE-E311-B8ED-005056822391 | 10074556    | Falkland Primary School                                                                |
| C171C7B5-C6AE-E311-B8ED-005056822391 | 10076165    | Whitehall Primary                                                                      |
| C217CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Shackleton Lower School                                                                |
| 047E99D9-C6AE-E311-B8ED-005056822391 | 10072428    | St Mary's Church of England School, Prittlewell                                        |
| 3CD491DF-C6AE-E311-B8ED-005056822391 | 10071687    | Our Lady of Dolours RC                                                                 |
| B88282EB-C6AE-E311-B8ED-005056822391 | 10003512    | Joseph Leckie Community Tech College                                                   |
| 47365C09-C7AE-E311-B8ED-005056822391 | 10014976    | Bishopsgarth School                                                                    |
| CCB03A27-C7AE-E311-B8ED-005056822391 | NULL        | Thorpedene Junior School                                                               |
| 72886BFD-C6AE-E311-B8ED-005056822391 | 10007568    | Woldgate School                                                                        |
| D712E69D-C6AE-E311-B8ED-005056822391 | 10077088    | Trosnant Junior School                                                                 |
| C1F73F21-C7AE-E311-B8ED-005056822391 | 10017979    | Christ Church CE High School                                                           |
| F68F540F-C7AE-E311-B8ED-005056822391 | NULL        | The Matthew Arnold School DR                                                           |
| 93D87AF1-C6AE-E311-B8ED-005056822391 | 10045188    | Wickersley School and Sports College                                                   |
| CE8082EB-C6AE-E311-B8ED-005056822391 | 10016038    | John Hanson School                                                                     |
| C9F63F21-C7AE-E311-B8ED-005056822391 | 10006725    | The Kings School                                                                       |
| 355C0C80-C6AE-E311-B8ED-005056822391 | 10054143    | St Anne's Nursey School                                                                |
| 5DCEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Great Kingshill CE Combined School                                                     |
| 2B3173F7-C6AE-E311-B8ED-005056822391 | 10015792    | Holden Lane High School                                                                |
| 1068DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Southey Green Junior School                                                            |
| 91896BFD-C6AE-E311-B8ED-005056822391 | 10000540    | Barnwell School                                                                        |
| 2E2B8AE5-C6AE-E311-B8ED-005056822391 | 10076254    | St. Matthias CE Primary School                                                         |
| C196C435-C7AE-E311-B8ED-005056822391 | NULL        | Queensbury School                                                                      |
| 3C1ACFAF-C6AE-E311-B8ED-005056822391 | 10069662    | Warren Park Primary School                                                             |
| 0114E69D-C6AE-E311-B8ED-005056822391 | 10073238    | Grays Infant School                                                                    |
| 5371C7B5-C6AE-E311-B8ED-005056822391 | 10074594    | John Scurr Primary School                                                              |
| 27375C09-C7AE-E311-B8ED-005056822391 | 10003494    | John Mason School                                                                      |
| B1C5BFBB-C6AE-E311-B8ED-005056822391 | 10076159    | Coleman Primary School                                                                 |
| 4BCC372D-C7AE-E311-B8ED-005056822391 | 10007333    | Wardle High School                                                                     |
| 7D18CFAF-C6AE-E311-B8ED-005056822391 | 10074361    | Penpol School                                                                          |
| AB71C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Cliff Park Community Middle School                                                     |
| 09536A73-C7AE-E311-B8ED-005056822391 | 10008204    | Durham High School for Girls                                                           |
| 3A3E451B-C7AE-E311-B8ED-005056822391 | 10018883    | Trinity CE Middle School                                                               |
| 6918CFAF-C6AE-E311-B8ED-005056822391 | 10078583    | Watlington Community Primary School                                                    |
| 901EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Waltham Holy Cross Junior                                                              |
| 1D615C8B-C7AE-E311-B8ED-005056822391 | 10008232    | Farlington School                                                                      |
| 9DC1D6A9-C6AE-E311-B8ED-005056822391 | 10080077    | Little Ealing Primary School                                                           |
| 41298AE5-C6AE-E311-B8ED-005056822391 | 10079296    | St Clement & St James School                                                           |
| 56B50486-C6AE-E311-B8ED-005056822391 | NULL        | Northwood Primary                                                                      |
| 833273F7-C6AE-E311-B8ED-005056822391 | 10016348    | Little Lever School                                                                    |
| 84DE6303-C7AE-E311-B8ED-005056822391 | 10002453    | Filsham Valley School                                                                  |
| B1375C09-C7AE-E311-B8ED-005056822391 | 10017373    | Rye Hills Secondary School                                                             |
| AC8E540F-C7AE-E311-B8ED-005056822391 | 10003151    | Horndean Community School                                                              |
| 4B1A7067-C7AE-E311-B8ED-005056822391 | 10000945    | Brockhill Park                                                                         |
| 59859443-C7AE-E311-B8ED-005056822391 | 10003770    | Lancaster Royal Grammar                                                                |
| 0413E69D-C6AE-E311-B8ED-005056822391 | 10073188    | Bearwood Primary School                                                                |
| 480D6579-C7AE-E311-B8ED-005056822391 | NULL        | Bedford High School                                                                    |
| 23BBED97-C6AE-E311-B8ED-005056822391 | 10072138    | Wallands Community                                                                     |
| 71F83F21-C7AE-E311-B8ED-005056822391 | 10017353    | Sion Manning Roman Catholic School                                                     |
| F90BFD8B-C6AE-E311-B8ED-005056822391 | 10075643    | Warley Road Primary School                                                             |
| FDD191DF-C6AE-E311-B8ED-005056822391 | 10070781    | St William of York RC School                                                           |
| 8968DEA3-C6AE-E311-B8ED-005056822391 | 10078034    | Colvestone Primary School                                                              |
| 6F17CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Meadow Primary School                                                                  |
| EA8182EB-C6AE-E311-B8ED-005056822391 | 10004299    | Menzies High School                                                                    |
| 6A385C09-C7AE-E311-B8ED-005056822391 | NULL        | Brackenhoe School                                                                      |
| 86E74C15-C7AE-E311-B8ED-005056822391 | 10001003    | The Burton Borough                                                                     |
| 355F0C80-C6AE-E311-B8ED-005056822391 | 10074205    | Bushmead Primary School                                                                |
| 4E8D540F-C7AE-E311-B8ED-005056822391 | 10001105    | Camborne School and Community College                                                  |
| D265F591-C6AE-E311-B8ED-005056822391 | 10070013    | St Stephen's Primary School                                                            |
| 4E896BFD-C6AE-E311-B8ED-005056822391 | NULL        | Hastingbury School and Community College                                               |
| 2125A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Richard Wakefield Primary School                                                       |
| 99D87AF1-C6AE-E311-B8ED-005056822391 | 10004919    | Ousedale School                                                                        |
| B8F63F21-C7AE-E311-B8ED-005056822391 | 10003486    | John F Kennedy                                                                         |
| 3D298AE5-C6AE-E311-B8ED-005056822391 | NULL        | Rough Close CE Primary School                                                          |
| BD75B0C7-C6AE-E311-B8ED-005056822391 | 10069898    | Broad Oak County Primary - A Beacon School - 888/2815                                  |
| ED0A6485-C7AE-E311-B8ED-005056822391 | NULL        | Bredon School                                                                          |
| E2993DC1-C7AE-E311-B8ED-005056822391 | NULL        | Barry College                                                                          |
| 6590540F-C7AE-E311-B8ED-005056822391 | 10004438    | Moseley School                                                                         |
| 6D2D8AE5-C6AE-E311-B8ED-005056822391 | 10015083    | Brookfield High School                                                                 |
| 55B30486-C6AE-E311-B8ED-005056822391 | NULL        | Barclay Junior                                                                         |
| 793E8F49-C7AE-E311-B8ED-005056822391 | 10000366    | The Arnewood School                                                                    |
| 3E3F8F49-C7AE-E311-B8ED-005056822391 | 10006629    | Gm School 2                                                                            |
| 11A77A5B-C7AE-E311-B8ED-005056822391 | 10006816    | The Skinners Company School                                                            |
| 772F73F7-C6AE-E311-B8ED-005056822391 | 10018429    | Chalkstone Middle School                                                               |
| A2D391DF-C6AE-E311-B8ED-005056822391 | 10076340    | Ince CofE Primary School                                                               |
| EF8282EB-C6AE-E311-B8ED-005056822391 | 10016254    | Kettlethorpe High School                                                               |
| 5B3E451B-C7AE-E311-B8ED-005056822391 | 10006563    | Cardinal Wiseman RC High School                                                        |
| 6A8382EB-C6AE-E311-B8ED-005056822391 | 10016066    | The Westgate School                                                                    |
| 3CED7F55-C7AE-E311-B8ED-005056822391 | 10007369    | Weavers School                                                                         |
| 662F73F7-C6AE-E311-B8ED-005056822391 | 10005598    | Charles Thorp Comprehensive School                                                     |
| B63273F7-C6AE-E311-B8ED-005056822391 | NULL        | Thomas Bennett Community College                                                       |
| 6AF53F21-C7AE-E311-B8ED-005056822391 | 10015269    | Corpus Christi High School                                                             |
| DB896BFD-C6AE-E311-B8ED-005056822391 | 10003696    | Kirkley High School                                                                    |
| E0E54C15-C7AE-E311-B8ED-005056822391 | 10005354    | Quintin Kynaston School                                                                |
| 5E385C09-C7AE-E311-B8ED-005056822391 | 10001501    | Clevedon Community School                                                              |
| 0E385C09-C7AE-E311-B8ED-005056822391 | 10007376    | Wednesfield High School                                                                |
| 5FE06303-C7AE-E311-B8ED-005056822391 | 10007197    | Vale of Ancholme Comprehensive School                                                  |
| A6F53F21-C7AE-E311-B8ED-005056822391 | 10006144    | St Cuthbert's RC High School                                                           |
| FE986F6D-C7AE-E311-B8ED-005056822391 | NULL        | Westfield Technology College                                                           |
| 3DD87AF1-C6AE-E311-B8ED-005056822391 | 10006730    | The Knights Templar School                                                             |
| A0CB372D-C7AE-E311-B8ED-005056822391 | 10069677    | The Thomas Willingale                                                                  |
| AEB50486-C6AE-E311-B8ED-005056822391 | NULL        | Elmhurst Primary School                                                                |
| A9B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Thorpenede Infant School                                                               |
| BEED7F55-C7AE-E311-B8ED-005056822391 | 10002394    | Fairfax School                                                                         |
| 7AD491DF-C6AE-E311-B8ED-005056822391 | 10076019    | St Aloysius RC Junior School                                                           |
| 04B50486-C6AE-E311-B8ED-005056822391 | 10070280    | Downshall Primary School                                                               |
| 6FB40486-C6AE-E311-B8ED-005056822391 | 10076616    | Cleveland Junior School                                                                |
| 4BFF7261-C7AE-E311-B8ED-005056822391 | 10004755    | Northfleet School for Boys                                                             |
| E6DB7AF1-C6AE-E311-B8ED-005056822391 | 10015308    | Woodcote High School                                                                   |
| 458F540F-C7AE-E311-B8ED-005056822391 | 10003632    | King Edward VI High School                                                             |
| 0D896BFD-C6AE-E311-B8ED-005056822391 | 10015701    | Eastbourne Technology College                                                          |
| 843173F7-C6AE-E311-B8ED-005056822391 | 10004393    | The Mirfield Free Grammar and Sixth Form                                               |
| C60A6485-C7AE-E311-B8ED-005056822391 | NULL        | Al Hijrah Secondary School                                                             |
| 48DA7AF1-C6AE-E311-B8ED-005056822391 | 10001194    | Carisbrooke High                                                                       |
| 828F540F-C7AE-E311-B8ED-005056822391 | 10015693    | Hasland Hall Community School                                                          |
| 10886BFD-C6AE-E311-B8ED-005056822391 | 10016091    | Tom Hood School                                                                        |
| A3355C09-C7AE-E311-B8ED-005056822391 | 10003647    | King's Manor School                                                                    |
| EA90540F-C7AE-E311-B8ED-005056822391 | 10015826    | William Beaumont Community School                                                      |
| 793C451B-C7AE-E311-B8ED-005056822391 | 10017490    | Robert May's School                                                                    |
| C911E69D-C6AE-E311-B8ED-005056822391 | 10072855    | Essex Primary School                                                                   |
| B325A1D3-C6AE-E311-B8ED-005056822391 | 10071550    | Rowledge C of E Primary School                                                         |
| 1C6FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Earith Primary School                                                                  |
| B80AFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Red Hill Primary School                                                                |
| 1D8E540F-C7AE-E311-B8ED-005056822391 | 10017524    | Queensbridge School                                                                    |
| 0CD87AF1-C6AE-E311-B8ED-005056822391 | 10018031    | Rokeby School                                                                          |
| 11C7BFBB-C6AE-E311-B8ED-005056822391 | 10073028    | Little London Community Primary School and Nursery                                     |
| 22E16303-C7AE-E311-B8ED-005056822391 | 10002395    | Fairfield High School                                                                  |
| EAF63F21-C7AE-E311-B8ED-005056822391 | NULL        | St Michael's Catholic College                                                          |
| C82F73F7-C6AE-E311-B8ED-005056822391 | 10017345    | Seladon High School                                                                    |
| C1BFD6A9-C6AE-E311-B8ED-005056822391 | 10071257    | Hobbayne Primary School                                                                |
| FD96874F-C7AE-E311-B8ED-005056822391 | 10000196    | Alcester Grammar School                                                                |
| D5F73F21-C7AE-E311-B8ED-005056822391 | 10014985    | All Saints Roman Catholic High School, Rossendale                                      |
| D6876BFD-C6AE-E311-B8ED-005056822391 | 10006359    | Stowmarket High School                                                                 |
| 74BCED97-C6AE-E311-B8ED-005056822391 | NULL        | The Links Primary School                                                               |
| 113D451B-C7AE-E311-B8ED-005056822391 | 10006156    | St Edwards C of E Comprehensive School                                                 |
| FC1CB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Grestone Primary School                                                                |
| 06DB7AF1-C6AE-E311-B8ED-005056822391 | 10002980    | Heathfield Community Centre                                                            |
| E92A8AE5-C6AE-E311-B8ED-005056822391 | 10078149    | St Andrews C of E Primary School                                                       |
| D1F73F21-C7AE-E311-B8ED-005056822391 | 10006110    | St Anne's Catholic High School for Girls                                               |
| FC866BFD-C6AE-E311-B8ED-005056822391 | 10008005    | Sir Harry Smith Community College                                                      |
| D1D491DF-C6AE-E311-B8ED-005056822391 | 10070150    | St Mary's RC Primary*                                                                  |
| ACD87AF1-C6AE-E311-B8ED-005056822391 | 10006393    | Sudbury Upper School                                                                   |
| C211E69D-C6AE-E311-B8ED-005056822391 | NULL        | Welldon Park Middle School                                                             |
| B8DB7AF1-C6AE-E311-B8ED-005056822391 | NULL        | The Compton High School & Sports College                                               |
| CE5D0C80-C6AE-E311-B8ED-005056822391 | 10071462    | Alexandra Park Junior School                                                           |
| 678F540F-C7AE-E311-B8ED-005056822391 | 10015424    | Crestwood Community School                                                             |
| 31DF6303-C7AE-E311-B8ED-005056822391 | 10017828    | Oriel Hs                                                                               |
| 498282EB-C6AE-E311-B8ED-005056822391 | 10007223    | Verulam School                                                                         |
| DDE06303-C7AE-E311-B8ED-005056822391 | 10004950    | Oxted School                                                                           |
| 490A6485-C7AE-E311-B8ED-005056822391 | 10008265    | Headington School Oxford                                                               |
| 7B0DFD8B-C6AE-E311-B8ED-005056822391 | 10080410    | Norwood Green Infant and Nursery School                                                |
| DCE64C15-C7AE-E311-B8ED-005056822391 | 10013526    | Islington Arts and Media School                                                        |
| 590C6579-C7AE-E311-B8ED-005056822391 | 10015264    | Cottesmore School                                                                      |
| 6AC1D6A9-C6AE-E311-B8ED-005056822391 | 10069518    | Brunswick House Primary School                                                         |
| 99FF7261-C7AE-E311-B8ED-005056822391 | 10000644    | Bennett Memorial Dioscesan School                                                      |
| B6CEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Weavers Close CE Primary School                                                        |
| 0AE06303-C7AE-E311-B8ED-005056822391 | 10004544    | Neatherd                                                                               |
| 06F73F21-C7AE-E311-B8ED-005056822391 | 10006784    | The Priory School                                                                      |
| EBDE6303-C7AE-E311-B8ED-005056822391 | 10015649    | Foxhills School Technology College                                                     |
| 6FB50486-C6AE-E311-B8ED-005056822391 | 10071897    | Perry Beeches School                                                                   |
| D53E8F49-C7AE-E311-B8ED-005056822391 | 10006729    | The Kingswood School                                                                   |
| EB3C451B-C7AE-E311-B8ED-005056822391 | 10006751    | Marlborough School                                                                     |
| 98DB7AF1-C6AE-E311-B8ED-005056822391 | 10014971    | Astley High School                                                                     |
| CE3273F7-C6AE-E311-B8ED-005056822391 | 10015296    | Cromwell Community College                                                             |
| 0FD291DF-C6AE-E311-B8ED-005056822391 | 10070231    | St Joseph's Catholic Primary                                                           |
| 2C1B7067-C7AE-E311-B8ED-005056822391 | 10002746    | Gravesend Grammar for Girls                                                            |
| 5E90540F-C7AE-E311-B8ED-005056822391 | 10014881    | Abbey Wood School                                                                      |
| 13F73F21-C7AE-E311-B8ED-005056822391 | 10000340    | Archbishop Blanch School                                                               |
| 68C2D6A9-C6AE-E311-B8ED-005056822391 | 10077213    | Brookside Primaty School                                                               |
| 6CCCA8CD-C6AE-E311-B8ED-005056822391 | 10069444    | Forsbrook C of E Controlled Primary School                                             |
| AA17CFAF-C6AE-E311-B8ED-005056822391 | 10079021    | Bridge Junior School                                                                   |
| 013173F7-C6AE-E311-B8ED-005056822391 | 10016961    | The Albany School                                                                      |
| 090A6485-C7AE-E311-B8ED-005056822391 | 10008633    | City of London Freeman's School                                                        |
| 8764F591-C6AE-E311-B8ED-005056822391 | NULL        | Roxbourne Middle School                                                                |
| 4ACCA8CD-C6AE-E311-B8ED-005056822391 | 10073960    | West Ham Church Primary School                                                         |
| 20859443-C7AE-E311-B8ED-005056822391 | 10002887    | Hanson School                                                                          |
| ACDB7AF1-C6AE-E311-B8ED-005056822391 | 10017942    | Gaynes School                                                                          |
| 0D2D8AE5-C6AE-E311-B8ED-005056822391 | 10014807    | Bearwood School                                                                        |
| 62859443-C7AE-E311-B8ED-005056822391 | 10005322    | Queen Elizabeth Grammar School                                                         |
| C768DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Parkside Junior School                                                                 |
| 678E540F-C7AE-E311-B8ED-005056822391 | 10006860    | The Wey Valley School                                                                  |
| F4C5BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Blenheim Primary                                                                       |
| 432D8AE5-C6AE-E311-B8ED-005056822391 | 10015440    | Harriet Costello Comprehensive School                                                  |
| 2CE54C15-C7AE-E311-B8ED-005056822391 | 10016490    | Moat Community College                                                                 |
| A23073F7-C6AE-E311-B8ED-005056822391 | 10004611    | Newlands Girls School                                                                  |
| 8C536A73-C7AE-E311-B8ED-005056822391 | NULL        | Sherborne School                                                                       |
| B4FE7261-C7AE-E311-B8ED-005056822391 | 10006802    | The Robert Napier School                                                               |
| 9075B0C7-C6AE-E311-B8ED-005056822391 | 10073661    | Betty Layward Primary                                                                  |
| 37ED7F55-C7AE-E311-B8ED-005056822391 | 10003926    | Lincoln Christ's Hospital                                                              |
| 87C6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | John Wheeldon Primary                                                                  |
| 7EF73F21-C7AE-E311-B8ED-005056822391 | 10017677    | St Bede's Catholic School                                                              |
| C4A67A5B-C7AE-E311-B8ED-005056822391 | 10003778    | Langley Park School for Girls                                                          |
| B4D97AF1-C6AE-E311-B8ED-005056822391 | NULL        | Ferndown Upper School                                                                  |
| ECD191DF-C6AE-E311-B8ED-005056822391 | 10078529    | All Saints C of E Junior School                                                        |
| 609A6F6D-C7AE-E311-B8ED-005056822391 | 10018749    | Sandroyd School                                                                        |
| 728382EB-C6AE-E311-B8ED-005056822391 | 10003235    | Ibstock Community College                                                              |
| FB605C8B-C7AE-E311-B8ED-005056822391 | 10004774    | Norwich School                                                                         |
| 141B5791-C7AE-E311-B8ED-005056822391 | 10016970    | The Business Academy Bexley                                                            |
| DDF63F21-C7AE-E311-B8ED-005056822391 | 10006689    | Henrietta Barnett School                                                               |
| 53876BFD-C6AE-E311-B8ED-005056822391 | 10003346    | Intake High School Arts College                                                        |
| 3EB03A27-C7AE-E311-B8ED-005056822391 | 10069638    | Lacey Gardens Junior School                                                            |
| 73C1D6A9-C6AE-E311-B8ED-005056822391 | 10080078    | Willow Tree Primary School                                                             |
| E73273F7-C6AE-E311-B8ED-005056822391 | 10006061    | Speedwell Technology College                                                           |
| 9EF83F21-C7AE-E311-B8ED-005056822391 | 10004780    | Notre Damehigh School                                                                  |
| 45DA7AF1-C6AE-E311-B8ED-005056822391 | 10001500    | Cleeve School                                                                          |
| 980EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Ranelagh Primary School                                                                |
| 9FFE7261-C7AE-E311-B8ED-005056822391 | 10017221    | The Fitz Wimarc School                                                                 |
| 3B896BFD-C6AE-E311-B8ED-005056822391 | 10017873    | Norlington School for Boys                                                             |
| 2CE84C15-C7AE-E311-B8ED-005056822391 | 10017117    | Ashford High School                                                                    |
| 2C0DFD8B-C6AE-E311-B8ED-005056822391 | 10046090    | Hazelwood Infant School                                                                |
| E6993DC1-C7AE-E311-B8ED-005056822391 | NULL        | West Glamorgan Institute Of Higher Education                                           |
| EB19CFAF-C6AE-E311-B8ED-005056822391 | 10072908    | Furzedown Primary School                                                               |
| 321B7067-C7AE-E311-B8ED-005056822391 | NULL        | Shenfield High School                                                                  |
| 6AE74C15-C7AE-E311-B8ED-005056822391 | NULL        | Edge End High School                                                                   |
| 98896BFD-C6AE-E311-B8ED-005056822391 | 10006692    | Hewett School                                                                          |
| CD8F540F-C7AE-E311-B8ED-005056822391 | 10015496    | Glenburn High School                                                                   |
| E05D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Hart Hill Primary School                                                               |
| 2423A1D3-C6AE-E311-B8ED-005056822391 | 10080546    | Newport Junior                                                                         |
| E98F540F-C7AE-E311-B8ED-005056822391 | 10015534    | Westfield Community College                                                            |
| A2096485-C7AE-E311-B8ED-005056822391 | 10017369    | Shiplake College                                                                       |
| DD886BFD-C6AE-E311-B8ED-005056822391 | 10006763    | Netherhall School                                                                      |
| 4C3F8F49-C7AE-E311-B8ED-005056822391 | 10005477    | Ringwood School                                                                        |
| D90A6485-C7AE-E311-B8ED-005056822391 | 10008415    | Oxford High School                                                                     |
| E6CDA8CD-C6AE-E311-B8ED-005056822391 | 10068642    | All Saints' CofE Primary School                                                        |
| 6D0DFD8B-C6AE-E311-B8ED-005056822391 | 10075053    | Hazlewood Junior School                                                                |
| AED250A3-C7AE-E311-B8ED-005056822391 | NULL        | Brooke Weston CTC SCITT                                                                |
| E73B451B-C7AE-E311-B8ED-005056822391 | 10018753    | Mayfield CE Middle School                                                              |
| AE1DB8C1-C6AE-E311-B8ED-005056822391 | 10075109    | West Bridgford Junior School                                                           |
| 49E84C15-C7AE-E311-B8ED-005056822391 | NULL        | Prittlewell School                                                                     |
| 03BBED97-C6AE-E311-B8ED-005056822391 | NULL        | Cranford Park Primary School                                                           |
| 94AF3A27-C7AE-E311-B8ED-005056822391 | 10077491    | St Helen's Primary School                                                              |
| A365F591-C6AE-E311-B8ED-005056822391 | 10068892    | Ley Hill School                                                                        |
| 5AED7F55-C7AE-E311-B8ED-005056822391 | 10003489    | Brent GM School                                                                        |
| AD2A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Benedict's RC Primary                                                               |
| 7D2F73F7-C6AE-E311-B8ED-005056822391 | 10017121    | Plashet School                                                                         |
| 832D8AE5-C6AE-E311-B8ED-005056822391 | 10000543    | Barrs Hill School                                                                      |
| 910CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Kensington Primary School                                                              |
| A570C7B5-C6AE-E311-B8ED-005056822391 | 10072840    | Welford Primary School                                                                 |
| 7CAF3A27-C7AE-E311-B8ED-005056822391 | 10079883    | Selsdon Primary School                                                                 |
| AB25A1D3-C6AE-E311-B8ED-005056822391 | NULL        | St Margaret's CE (C) Junior School                                                     |
| BB3C451B-C7AE-E311-B8ED-005056822391 | 10000398    | Ashby Grammar School                                                                   |
| 503173F7-C6AE-E311-B8ED-005056822391 | 10004904    | Orwell High School                                                                     |
| 6A3E8F49-C7AE-E311-B8ED-005056822391 | 10000590    | Beaconsfield High School                                                               |
| CE1A5791-C7AE-E311-B8ED-005056822391 | NULL        | Haberdashers' Aske's Hatcham College                                                   |
| 3DB40486-C6AE-E311-B8ED-005056822391 | 10071936    | Carterhatch Infant School                                                              |
| CB90540F-C7AE-E311-B8ED-005056822391 | 10016782    | Oaklands School                                                                        |
| B18182EB-C6AE-E311-B8ED-005056822391 | 10006348    | Stoke Park School and Community Technology College                                     |
| CB2A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Pardes House Primary School                                                            |
| 333173F7-C6AE-E311-B8ED-005056822391 | 10004980    | Park House School                                                                      |
| 6EE06303-C7AE-E311-B8ED-005056822391 | 10016127    | The Humberston School                                                                  |
| 3FDA7AF1-C6AE-E311-B8ED-005056822391 | 10004924    | Outwood Grange College of Technology                                                   |
| AD5E0C80-C6AE-E311-B8ED-005056822391 | 10080026    | Belmont Primary School                                                                 |
| 2F3F8F49-C7AE-E311-B8ED-005056822391 | 10003130    | Holy Trinity CofE Senior School                                                        |
| D38E540F-C7AE-E311-B8ED-005056822391 | 10002766    | Greenacre School                                                                       |
| 61C7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Whitfield Valley Primary School                                                        |
| DCC8BFBB-C6AE-E311-B8ED-005056822391 | 10069265    | Valley Road Primary School                                                             |
| 7BF83F21-C7AE-E311-B8ED-005056822391 | 10017483    | St Bernadette Catholic Secondary School                                                |
| FCDF6303-C7AE-E311-B8ED-005056822391 | NULL        | The Headlands School                                                                   |
| 2D96874F-C7AE-E311-B8ED-005056822391 | 10000406    | Ashmole School                                                                         |
| 343E451B-C7AE-E311-B8ED-005056822391 | 10017726    | St Bedes Inter-Church Comprehensive School                                             |
| 22E84C15-C7AE-E311-B8ED-005056822391 | NULL        | Woking High School                                                                     |
| 9D385C09-C7AE-E311-B8ED-005056822391 | 10016485    | Mill Chase Community School                                                            |
| 5AD391DF-C6AE-E311-B8ED-005056822391 | NULL        | St Philip's CE Primary School                                                          |
| B775B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Wold Primary School                                                                    |
| 481A7067-C7AE-E311-B8ED-005056822391 | 10010902    | St Benedict's RC College                                                               |
| 00DB7AF1-C6AE-E311-B8ED-005056822391 | 10015181    | Brown Hills High School                                                                |
| EA2F73F7-C6AE-E311-B8ED-005056822391 | 10015577    | Westergate Community School                                                            |
| 431DB8C1-C6AE-E311-B8ED-005056822391 | 10070867    | Asterdale Primary School                                                               |
| 05F83F21-C7AE-E311-B8ED-005056822391 | 10000343    | Archbishop Michael Ramsey Technology College                                           |
| E211E69D-C6AE-E311-B8ED-005056822391 | 10072553    | Long Sutton Primary School                                                             |
| 1613E69D-C6AE-E311-B8ED-005056822391 | 10079040    | Bottesford Junior School                                                               |
| CC849443-C7AE-E311-B8ED-005056822391 | 10002769    | Greenford High School                                                                  |
| DFE44C15-C7AE-E311-B8ED-005056822391 | 10017591    | Soar Valley College                                                                    |
| BF18CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Dallow Primary School                                                                  |
| 2AD250A3-C7AE-E311-B8ED-005056822391 | 10015618    | Harlow Fields School                                                                   |
| FFC8BFBB-C6AE-E311-B8ED-005056822391 | 10071321    | Ravenstone Primary                                                                     |
| 309A6F6D-C7AE-E311-B8ED-005056822391 | 10027314    | St Faith's School                                                                      |
| D7E06303-C7AE-E311-B8ED-005056822391 | 10003117    | Holbrook High School                                                                   |
| 9C17CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Eynsham Community Primary School                                                       |
| BE5D0C80-C6AE-E311-B8ED-005056822391 | 10071285    | Anson Primary School                                                                   |
| 3D8182EB-C6AE-E311-B8ED-005056822391 | 10006457    | Swanwick Hall School                                                                   |
| 673F451B-C7AE-E311-B8ED-005056822391 | 10017734    | St Joseph's RChs & Sports College                                                      |
| B1BAED97-C6AE-E311-B8ED-005056822391 | 10063213    | Mayfield Primary School                                                                |
| 4D3C451B-C7AE-E311-B8ED-005056822391 | 10004156    | Maidstone Grammar School                                                               |
| 208E540F-C7AE-E311-B8ED-005056822391 | 10006508    | Tavistock College                                                                      |
| BF2F73F7-C6AE-E311-B8ED-005056822391 | 10006172    | The St Guthlac School                                                                  |
| 360A6485-C7AE-E311-B8ED-005056822391 | 10000516    | Bancroft's School                                                                      |
| A4DF6303-C7AE-E311-B8ED-005056822391 | 10001468    | City of Ely Community College                                                          |
| D2996F6D-C7AE-E311-B8ED-005056822391 | NULL        | Bryanston School                                                                       |
| A4FE7261-C7AE-E311-B8ED-005056822391 | 10004842    | Oakwood Park Grammar School                                                            |
| 0E0EFD8B-C6AE-E311-B8ED-005056822391 | 10076587    | Handsworth Primary School                                                              |
| CBD87AF1-C6AE-E311-B8ED-005056822391 | 10000576    | Baxter College                                                                         |
| BA3E8F49-C7AE-E311-B8ED-005056822391 | 10002566    | Francis Bacon School                                                                   |
| 936FC7B5-C6AE-E311-B8ED-005056822391 | 10077499    | Southwood Middle School                                                                |
| B03F8F49-C7AE-E311-B8ED-005056822391 | 10016334    | Lea Manor High School Performing Arts College                                          |
| B68F540F-C7AE-E311-B8ED-005056822391 | 10002904    | Harrogate Grammer School                                                               |
| 24E54C15-C7AE-E311-B8ED-005056822391 | 10005733    | Sedgehill School                                                                       |
| 9B849443-C7AE-E311-B8ED-005056822391 | 10007675    | Wyndmondham College                                                                    |
| 2713E69D-C6AE-E311-B8ED-005056822391 | 10081272    | Whiteknights Primary School                                                            |
| 52E16303-C7AE-E311-B8ED-005056822391 | 10016812    | Oathall Community College                                                              |
| 82886BFD-C6AE-E311-B8ED-005056822391 | 10007287    | Waingel's Copse School                                                                 |
| 2965F591-C6AE-E311-B8ED-005056822391 | 10068767    | Park Road Junior Infant and Nursery School                                             |
| D2096485-C7AE-E311-B8ED-005056822391 | 10007950    | Ipswich High School                                                                    |
| 147E99D9-C6AE-E311-B8ED-005056822391 | 10079258    | St George's CE Aided Primary School                                                    |
| 8EBBED97-C6AE-E311-B8ED-005056822391 | NULL        | Lyons Hall Primary School                                                              |
| A47A99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Margarets CE Primary                                                                |
| 8C77B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Blackfordby St Margaret's CE Primary                                                   |
| 2AC0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Aylesford Primary School                                                               |
| 0CCC372D-C7AE-E311-B8ED-005056822391 | 10006912    | Tiffin School                                                                          |
| EC18CFAF-C6AE-E311-B8ED-005056822391 | 10079859    | Farley Junior School                                                                   |
| 22E64C15-C7AE-E311-B8ED-005056822391 | 10017609    | Brays Grove School                                                                     |
| 910D6579-C7AE-E311-B8ED-005056822391 | 10007056    | Trinity. School                                                                        |
| D863F591-C6AE-E311-B8ED-005056822391 | NULL        | Brackenbury Primary School                                                             |
| C9F73F21-C7AE-E311-B8ED-005056822391 | 10006887    | Thomas Becket Upper School                                                             |
| 9E09E2AD-EF10-E511-A3DA-005056822390 | 99999993    | Other UK (Northern Ireland)                                                            |
| D1C7BFBB-C6AE-E311-B8ED-005056822391 | 10072267    | Bracken Edge Primary School                                                            |
| 9D526A73-C7AE-E311-B8ED-005056822391 | NULL        | Ludgrove Preparatory School                                                            |
| 418282EB-C6AE-E311-B8ED-005056822391 | 10015841    | Haywood High School                                                                    |
| BB3F8F49-C7AE-E311-B8ED-005056822391 | NULL        | St Georges CE School                                                                   |
| 1C3173F7-C6AE-E311-B8ED-005056822391 | 10015690    | Wintringham School                                                                     |
| 5965F591-C6AE-E311-B8ED-005056822391 | 10071302    | Roding Primary School                                                                  |
| 0FCCA8CD-C6AE-E311-B8ED-005056822391 | 10073787    | Barcombe Church of England Primary School                                              |
| 1BED7F55-C7AE-E311-B8ED-005056822391 | 10009087    | Sawston Village                                                                        |
| 212B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Mary's Roman Catholic Primary School                                                |
| 441ACFAF-C6AE-E311-B8ED-005056822391 | 10075289    | Harmans Water Primary School                                                           |
| E6D87AF1-C6AE-E311-B8ED-005056822391 | 10003400    | Ise Community College                                                                  |
| B6D391DF-C6AE-E311-B8ED-005056822391 | NULL        | St Anne's Catholic Primary School                                                      |
| 9150BF3B-C7AE-E311-B8ED-005056822391 | 10000822    | Bournemoouth School                                                                    |
| 29615C8B-C7AE-E311-B8ED-005056822391 | 10073500    | Milbourne Lodge Junior School                                                          |
| E720B8C1-C6AE-E311-B8ED-005056822391 | 10076186    | Clayton Brook Primary School                                                           |
| 6DB9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Queen's Primary School                                                                 |
| 7B0C6579-C7AE-E311-B8ED-005056822391 | 10006232    | St Marys School                                                                        |
| 43A87A5B-C7AE-E311-B8ED-005056822391 | 10013321    | Central School                                                                         |
| 6A0EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Rushy Meadow Primary School                                                            |
| 563D451B-C7AE-E311-B8ED-005056822391 | 10006253    | S. Peters Collegiate School                                                            |
| 548482EB-C6AE-E311-B8ED-005056822391 | 10018121    | Cansfield High Specialist Language College                                             |
| E91EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Windale First School                                                                   |
| B5E74C15-C7AE-E311-B8ED-005056822391 | 10005141    | Ponteland High School                                                                  |
| 3364F591-C6AE-E311-B8ED-005056822391 | NULL        | Barhill Primary School                                                                 |
| 120CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Slip End Lower School                                                                  |
| CA8282EB-C6AE-E311-B8ED-005056822391 | NULL        | Hobart High School                                                                     |
| 81BBED97-C6AE-E311-B8ED-005056822391 | 10081402    | Cavendish Primary School                                                               |
| 8774B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Northlands Junior School                                                               |
| 843073F7-C6AE-E311-B8ED-005056822391 | 10010362    | Tong High School                                                                       |
| 386FC7B5-C6AE-E311-B8ED-005056822391 | 10072802    | Albany Infants and Nursery                                                             |
| DE2C8AE5-C6AE-E311-B8ED-005056822391 | 10080347    | St Bede's RC Primary School                                                            |
| AC6FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Oatlands Community Infant School                                                       |
| 473D451B-C7AE-E311-B8ED-005056822391 | 10000767    | Blessed George Napier RC School                                                        |
| 9011E69D-C6AE-E311-B8ED-005056822391 | NULL        | Eckington Junior School                                                                |
| 60D250A3-C7AE-E311-B8ED-005056822391 | NULL        | Heathermount School                                                                    |
| 3EC818A5-C9AE-E311-B8ED-005056822391 | NULL        | Unknown                                                                                |
| 95C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Hazeldene School                                                                       |
| DE8D540F-C7AE-E311-B8ED-005056822391 | 10016280    | Kirk Hallam Community Technology College                                               |
| CF866BFD-C6AE-E311-B8ED-005056822391 | 10015657    | Witton Park High School                                                                |
| 1964F591-C6AE-E311-B8ED-005056822391 | NULL        | Church Mead Infants and Nursery School                                                 |
| 742F73F7-C6AE-E311-B8ED-005056822391 | 10015734    | Waylands Community High School                                                         |
| 64615C8B-C7AE-E311-B8ED-005056822391 | 10077630    | Garden House Boy's School                                                              |
| C6C5BFBB-C6AE-E311-B8ED-005056822391 | 10076158    | Herrick Primary School                                                                 |
| 07E54C15-C7AE-E311-B8ED-005056822391 | 10003636    | King Edward VII School                                                                 |
| 38859443-C7AE-E311-B8ED-005056822391 | 10002153    | Ecclesbourne School                                                                    |
| 359A6F6D-C7AE-E311-B8ED-005056822391 | NULL        | Tauheedul-Islam Girls' High School                                                     |
| 0E3C451B-C7AE-E311-B8ED-005056822391 | 10005868    | Sir John Talbot's School                                                               |
| 98E54C15-C7AE-E311-B8ED-005056822391 | 10015478    | Oakwood High School                                                                    |
| EB526A73-C7AE-E311-B8ED-005056822391 | 10006177    | St Helens School                                                                       |
| 55C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Brickhill Lower School                                                                 |
| 45E44C15-C7AE-E311-B8ED-005056822391 | 10004369    | Millom School                                                                          |
| E27A99D9-C6AE-E311-B8ED-005056822391 | 10073973    | St Edwards C of E Primary School                                                       |
| 92298AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Anne's Catholic Primary School                                                      |
| 7A1EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Millbrook Primary School                                                               |
| E43E451B-C7AE-E311-B8ED-005056822391 | 10001766    | Crompton House School                                                                  |
| 79BCED97-C6AE-E311-B8ED-005056822391 | NULL        | Franche First School                                                                   |
| DE0A6485-C7AE-E311-B8ED-005056822391 | 10004150    | The Magdalen College School                                                            |
| BD3E451B-C7AE-E311-B8ED-005056822391 | NULL        | Our Lady's RC High                                                                     |
| 293F8F49-C7AE-E311-B8ED-005056822391 | 10006898    | Thornton Grammar School                                                                |
| CA6EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Ramsey Manor Lower School                                                              |
| EECBA8CD-C6AE-E311-B8ED-005056822391 | 10078488    | Carisbrook Primary School                                                              |
| 6E71C7B5-C6AE-E311-B8ED-005056822391 | 10075096    | Banstead Community Junior School                                                       |
| BB5D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Brampton Primary                                                                       |
| 7A7C99D9-C6AE-E311-B8ED-005056822391 | NULL        | National CE Junior                                                                     |
| E48F540F-C7AE-E311-B8ED-005056822391 | 10006827    | The Thomas Alleyne School                                                              |
| F76ADEA3-C6AE-E311-B8ED-005056822391 | 10072777    | Horley County Infant School                                                            |
| E3E54C15-C7AE-E311-B8ED-005056822391 | 10017775    | Swanlea School                                                                         |
| 22D87AF1-C6AE-E311-B8ED-005056822391 | 10015614    | Hartshead High School                                                                  |
| AC5D0C80-C6AE-E311-B8ED-005056822391 | NULL        | The Sundridge School                                                                   |
| A8D150A3-C7AE-E311-B8ED-005056822391 | 10015068    | College Park                                                                           |
| 3B71C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Hammond Junior Mixed and Infant School                                                 |
| C10AFD8B-C6AE-E311-B8ED-005056822391 | 10073859    | Chiseldon Primary                                                                      |
| 95F73F21-C7AE-E311-B8ED-005056822391 | 10006126    | St Bernards Catholic School                                                            |
| 763273F7-C6AE-E311-B8ED-005056822391 | 10006092    | Sprowston High School                                                                  |
| 7471C7B5-C6AE-E311-B8ED-005056822391 | 10071206    | Springcroft Primary School                                                             |
| 002A8AE5-C6AE-E311-B8ED-005056822391 | 10071664    | St Thomas More RC Primary                                                              |
| 1DB60486-C6AE-E311-B8ED-005056822391 | NULL        | Jackfield Infants' School                                                              |
| F68D540F-C7AE-E311-B8ED-005056822391 | 10015473    | Cape Cornwall School                                                                   |
| 21BCED97-C6AE-E311-B8ED-005056822391 | 10075275    | Barncroft Primary School                                                               |
| 6811E69D-C6AE-E311-B8ED-005056822391 | 10072133    | Wivelsfield Primary                                                                    |
| C025A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Brenzett Church of England Primary School                                              |
| 20DA7AF1-C6AE-E311-B8ED-005056822391 | 10016731    | Nower Hill High School                                                                 |
| 67F73F21-C7AE-E311-B8ED-005056822391 | NULL        | Ripley St Thomas CE                                                                    |
| 5CEE7F55-C7AE-E311-B8ED-005056822391 | 10001854    | Dartford Grammar School for Girls                                                      |
| BAF73F21-C7AE-E311-B8ED-005056822391 | 10000222    | All Saints Catholic College                                                            |
| 72896BFD-C6AE-E311-B8ED-005056822391 | 10015857    | Warwick School for Boys                                                                |
| FE3173F7-C6AE-E311-B8ED-005056822391 | 10017869    | Carterton Community College                                                            |
| E4896BFD-C6AE-E311-B8ED-005056822391 | NULL        | Woolmer Hill School                                                                    |
| 64F83F21-C7AE-E311-B8ED-005056822391 | 10017026    | St Joseph's Catholic High School                                                       |
| CD0CFD8B-C6AE-E311-B8ED-005056822391 | 10079994    | Manor Primary School                                                                   |
| 065E0C80-C6AE-E311-B8ED-005056822391 | 10071174    | Aldingbourne Primary School                                                            |
| 710CFD8B-C6AE-E311-B8ED-005056822391 | 10071280    | Lyon Park Infant School                                                                |
| BA18CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Lent Rise Primary School                                                               |
| B18D540F-C7AE-E311-B8ED-005056822391 | 10007692    | Yateley Community School                                                               |
| 1070C7B5-C6AE-E311-B8ED-005056822391 | 10068882    | Hardwick Community Primary School                                                      |
| C01EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Spinnens Acre CJ School                                                                |
| 86B40486-C6AE-E311-B8ED-005056822391 | 10073306    | Lammack County Primary                                                                 |
| 87BBED97-C6AE-E311-B8ED-005056822391 | NULL        | Grange Primary School                                                                  |
| 70375C09-C7AE-E311-B8ED-005056822391 | 10004913    | Ounsdale High School                                                                   |
| 7363F591-C6AE-E311-B8ED-005056822391 | NULL        | Jubile School                                                                          |
| CB0A6485-C7AE-E311-B8ED-005056822391 | 10017473    | Princethorpe College                                                                   |
| 0DF63F21-C7AE-E311-B8ED-005056822391 | 10017967    | Royal Alexandra & Albert School                                                        |
| 431B7067-C7AE-E311-B8ED-005056822391 | 10018038    | King's House School                                                                    |
| A7B30486-C6AE-E311-B8ED-005056822391 | 10075064    | Manor Junior School                                                                    |
| 48C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | John Wycliffe Primary School                                                           |
| E5E74C15-C7AE-E311-B8ED-005056822391 | 10003670    | Tudor Grange Academy Redditch                                                          |
| 87ED667F-C7AE-E311-B8ED-005056822391 | 10018600    | St Martin's Ampleforth, St Lawrence Educational Tr                                     |
| 656FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Great Wood Primary School                                                              |
| CEE44C15-C7AE-E311-B8ED-005056822391 | 10013297    | Hamilton Community College                                                             |
| 9B896BFD-C6AE-E311-B8ED-005056822391 | 10002071    | Duston Upper School                                                                    |
| B0E06303-C7AE-E311-B8ED-005056822391 | 10018215    | Newcastle High School                                                                  |
| C55E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Harpur Mount CP School                                                                 |
| A1F83F21-C7AE-E311-B8ED-005056822391 | 10006123    | St John Almond RC School                                                               |
| 2F0EFD8B-C6AE-E311-B8ED-005056822391 | 10072969    | Cromer H S                                                                             |
| 1576B0C7-C6AE-E311-B8ED-005056822391 | 10077172    | Jubilee Primary School                                                                 |
| CFCB372D-C7AE-E311-B8ED-005056822391 | 10003008    | Hendon School                                                                          |
| 2724A1D3-C6AE-E311-B8ED-005056822391 | 10070345    | Barton St Peter's C of E Primary School                                                |
| 79E06303-C7AE-E311-B8ED-005056822391 | 10015294    | Chesterton Community                                                                   |
| 8D2F73F7-C6AE-E311-B8ED-005056822391 | 10017167    | Priestnall School                                                                      |
| B43D451B-C7AE-E311-B8ED-005056822391 | 10006779    | The Piggott School                                                                     |
| 5A8282EB-C6AE-E311-B8ED-005056822391 | NULL        | Ivy Bank High School                                                                   |
| 51E54C15-C7AE-E311-B8ED-005056822391 | 10015216    | Chamberlayne Park School                                                               |
| ECC6BFBB-C6AE-E311-B8ED-005056822391 | 10069924    | Balladen Community Primary School                                                      |
| C9CCA8CD-C6AE-E311-B8ED-005056822391 | 10074257    | Codicote C of E Primary School                                                         |
| ABD491DF-C6AE-E311-B8ED-005056822391 | 10070765    | Christ The King School                                                                 |
| B92A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Joseph's RC Junior                                                                  |
| C68182EB-C6AE-E311-B8ED-005056822391 | 10002100    | Ampleforth College                                                                     |
| 81E74C15-C7AE-E311-B8ED-005056822391 | 10018071    | Palatine High School                                                                   |
| CFBBED97-C6AE-E311-B8ED-005056822391 | 10073226    | Stanway Fiveways Primary School                                                        |
| DF3073F7-C6AE-E311-B8ED-005056822391 | 10007111    | Uckfield Community College                                                             |
| 51CEA8CD-C6AE-E311-B8ED-005056822391 | 10076322    | Curry Rivel CE VC Primary School                                                       |
| 0B96874F-C7AE-E311-B8ED-005056822391 | 10007416    | West Hatch High School                                                                 |
| A312E69D-C6AE-E311-B8ED-005056822391 | 10069944    | Spring Hill County Primary                                                             |
| B7CB372D-C7AE-E311-B8ED-005056822391 | NULL        | Holly Cross RC Primary School                                                          |
| 6C62F591-C6AE-E311-B8ED-005056822391 | 10078939    | Worsbrough Common Primary School                                                       |
| 022C8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Our Lady of Lourdes Catholic Primary School                                            |
| 84B60486-C6AE-E311-B8ED-005056822391 | 10070028    | Southborough Primary                                                                   |
| 5BCB372D-C7AE-E311-B8ED-005056822391 | 10072414    | St Andrew's Primary School                                                             |
| 731EB8C1-C6AE-E311-B8ED-005056822391 | 10076077    | Ermington Primary School                                                               |
| D4615C8B-C7AE-E311-B8ED-005056822391 | 10018618    | Chinthurst School                                                                      |
| 7F385C09-C7AE-E311-B8ED-005056822391 | 10017880    | Grangefield School                                                                     |
| BF3B451B-C7AE-E311-B8ED-005056822391 | 10004352    | Midhurst Grammar Schol                                                                 |
| 70385C09-C7AE-E311-B8ED-005056822391 | 10004136    | Lytham St Annes High School                                                            |
| 2A8182EB-C6AE-E311-B8ED-005056822391 | 10016489    | Milton Cross School                                                                    |
| 7E64F591-C6AE-E311-B8ED-005056822391 | 10077805    | Langley First School                                                                   |
| 6EBBED97-C6AE-E311-B8ED-005056822391 | 10068788    | Queensgate Primary School                                                              |
| 007B589D-C7AE-E311-B8ED-005056822391 | 10018082    | Selly Oak School                                                                       |
| 7D859443-C7AE-E311-B8ED-005056822391 | 10000002    | Cardinal Vaughan                                                                       |
| 2EE64C15-C7AE-E311-B8ED-005056822391 | NULL        | North Westminster Community School                                                     |
| 8AF83F21-C7AE-E311-B8ED-005056822391 | 10015517    | Harris C of E High School                                                              |
| 64D97AF1-C6AE-E311-B8ED-005056822391 | 10006582    | Armthorpe Academy                                                                      |
| 88E64C15-C7AE-E311-B8ED-005056822391 | 10015738    | Fulham Cross School                                                                    |
| BF615C8B-C7AE-E311-B8ED-005056822391 | 10008443    | Putney High                                                                            |
| 4290540F-C7AE-E311-B8ED-005056822391 | 10014945    | Beaumont Leys School                                                                   |
| CC536A73-C7AE-E311-B8ED-005056822391 | 10015633    | Windsmoor House School                                                                 |
| 398E540F-C7AE-E311-B8ED-005056822391 | 10003439    | Ivybridge Community College                                                            |
| 5769DEA3-C6AE-E311-B8ED-005056822391 | NULL        | The Usher Junior School                                                                |
| F4C1D6A9-C6AE-E311-B8ED-005056822391 | 10078191    | Uplands Manor Primary School                                                           |
| 5ED97AF1-C6AE-E311-B8ED-005056822391 | 10007332    | Wanstead High School                                                                   |
| 69D97AF1-C6AE-E311-B8ED-005056822391 | 10000439    | Aston Academy                                                                          |
| 9D1EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Oak Tree Primary School                                                                |
| 713F8F49-C7AE-E311-B8ED-005056822391 | 10005324    | Queen Elizabeth School                                                                 |
| 025F0C80-C6AE-E311-B8ED-005056822391 | 10071331    | Alderbrook Primary School                                                              |
| 51AF3A27-C7AE-E311-B8ED-005056822391 | 10017629    | St Joseph's Catholic High School                                                       |
| B43073F7-C6AE-E311-B8ED-005056822391 | 10002914    | Hartismere High School                                                                 |
| A3F63F21-C7AE-E311-B8ED-005056822391 | 10000776    | The Nottingham Bluecoat School                                                         |
| 10C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Dulwich Hamlet Junior School                                                           |
| 7CEA3A37-ED89-E411-A72C-005056822391 | 10034569    | North West Teaching School Alliance                                                    |
| 7A1ACFAF-C6AE-E311-B8ED-005056822391 | NULL        | Downside Junior School                                                                 |
| F70A6485-C7AE-E311-B8ED-005056822391 | 10018621    | Kingshott School                                                                       |
| CD19CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Stanley Junior School                                                                  |
| CD96874F-C7AE-E311-B8ED-005056822391 | 10006647    | The Cottesloes School                                                                  |
| EF1EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Southwold Primary                                                                      |
| E63E8F49-C7AE-E311-B8ED-005056822391 | 10017347    | Salesian College                                                                       |
| 5462F591-C6AE-E311-B8ED-005056822391 | NULL        | Alfred Mizen First School                                                              |
| 176FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Welland Primary School                                                                 |
| 05C7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Wakely Junior School                                                                   |
| F32F73F7-C6AE-E311-B8ED-005056822391 | 10015904    | Eastlea School                                                                         |
| E6E64C15-C7AE-E311-B8ED-005056822391 | 10000572    | Battersea Technology College                                                           |
| 4E8F540F-C7AE-E311-B8ED-005056822391 | 10001675    | Coombe Dean School                                                                     |
| BE1A7067-C7AE-E311-B8ED-005056822391 | 10007231    | Virgo Fidelis Convent Senior School                                                    |
| D9BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Moorlands Infant School                                                                |
| 9213E69D-C6AE-E311-B8ED-005056822391 | 10076384    | Greenfield Primary School                                                              |
| 03CEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Nicholas C of E (C) First School                                                    |
| 8F298AE5-C6AE-E311-B8ED-005056822391 | 10075975    | St Joseph's Catholic Primary School                                                    |
| 62E54C15-C7AE-E311-B8ED-005056822391 | 10015513    | Weston Park Boys School                                                                |
| E6605C8B-C7AE-E311-B8ED-005056822391 | 10008582    | Wakefield Girls' High School                                                           |
| CFD87AF1-C6AE-E311-B8ED-005056822391 | 10015243    | Colne Primet High School                                                               |
| B8876BFD-C6AE-E311-B8ED-005056822391 | 10000957    | Brownhills Community School                                                            |
| F3D191DF-C6AE-E311-B8ED-005056822391 | NULL        | St Lawrence CE Primary School                                                          |
| 6A1ACFAF-C6AE-E311-B8ED-005056822391 | 10074616    | Goodrich Community Primary School                                                      |
| 9390540F-C7AE-E311-B8ED-005056822391 | NULL        | Brine Leas High School                                                                 |
| 5619CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Trewirgie Junior School                                                                |
| 083173F7-C6AE-E311-B8ED-005056822391 | 10015776    | Honley High School                                                                     |
| 0AD391DF-C6AE-E311-B8ED-005056822391 | 10070230    | Our Lady of Peace Infant School                                                        |
| CE6ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Nansen Primary School                                                                  |
| CF3073F7-C6AE-E311-B8ED-005056822391 | 10016294    | Kingsford Community School                                                             |
| 97D391DF-C6AE-E311-B8ED-005056822391 | 10077371    | Puller Memorial C of E of (VA) Primary                                                 |
| D718CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Waterfield First School                                                                |
| BBE54C15-C7AE-E311-B8ED-005056822391 | 10002536    | Forest Hill School                                                                     |
| 6DDB7AF1-C6AE-E311-B8ED-005056822391 | 10005499    | Robert Clack School                                                                    |
| D31FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Burton Joyce Primary School                                                            |
| C095874F-C7AE-E311-B8ED-005056822391 | 10005003    | Pates Grammar School                                                                   |
| 78C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Berkeley Infant School                                                                 |
| 6CB03A27-C7AE-E311-B8ED-005056822391 | NULL        | Ryvers School                                                                          |
| 512D8AE5-C6AE-E311-B8ED-005056822391 | 10015133    | Bottisham Village College                                                              |
| A277B0C7-C6AE-E311-B8ED-005056822391 | 10078343    | Thornhill Lees Church of England Voluntary Controlled Infant and Nursery School        |
| E28D540F-C7AE-E311-B8ED-005056822391 | 10007952    | John Hunt of Everest Community School                                                  |
| 865E0C80-C6AE-E311-B8ED-005056822391 | 10073646    | Leagrave Primary School                                                                |
| FF24A1D3-C6AE-E311-B8ED-005056822391 | 10078755    | Blackshaw Moor CE First School                                                         |
| BCC6BFBB-C6AE-E311-B8ED-005056822391 | 10078086    | Castlechurch Primary                                                                   |
| 9EDB7AF1-C6AE-E311-B8ED-005056822391 | 10015298    | George Salter High School                                                              |
| 8462F591-C6AE-E311-B8ED-005056822391 | NULL        | Harrold Lower School                                                                   |
| FC298AE5-C6AE-E311-B8ED-005056822391 | 10070742    | Mount Carmel RC Primary                                                                |
| CB0AFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Alton Park Junior School                                                               |
| 54C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Charlemont Primary School                                                              |
| 6765F591-C6AE-E311-B8ED-005056822391 | 10076416    | Dawlish Primary School                                                                 |
| 5870C7B5-C6AE-E311-B8ED-005056822391 | 10074093    | Goldfield Infant's & Nursery School                                                    |
| BFED667F-C7AE-E311-B8ED-005056822391 | NULL        | St Antony's Lewston School                                                             |
| 5B6EC7B5-C6AE-E311-B8ED-005056822391 | 10077257    | The Grove Primary School                                                               |
| 773F8F49-C7AE-E311-B8ED-005056822391 | 10038069    | Sacred Heart Catholic School                                                           |
| C165F591-C6AE-E311-B8ED-005056822391 | 10072897    | John Perry Primary School                                                              |
| DBBBED97-C6AE-E311-B8ED-005056822391 | 10080001    | Cranmer Primary School                                                                 |
| 0F7B589D-C7AE-E311-B8ED-005056822391 | 10017837    | Southfield School                                                                      |
| BA1A5791-C7AE-E311-B8ED-005056822391 | 10017351    | Royal Grammer School                                                                   |
| 84D491DF-C6AE-E311-B8ED-005056822391 | NULL        | St Thomas More's RC Primary School                                                     |
| 4E6FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Stokes Wood Primary                                                                    |
| 5611E69D-C6AE-E311-B8ED-005056822391 | 10042388    | Leiston Primary School                                                                 |
| 2A8E540F-C7AE-E311-B8ED-005056822391 | 10015499    | Winton School                                                                          |
| BE26A1D3-C6AE-E311-B8ED-005056822391 | NULL        | The Holme CE Controlled                                                                |
| 0A65F591-C6AE-E311-B8ED-005056822391 | 10071271    | Mitchell Brook Primary School                                                          |
| 96876BFD-C6AE-E311-B8ED-005056822391 | 10005984    | East Riding Secondary School                                                           |
| C3F73F21-C7AE-E311-B8ED-005056822391 | 10017763    | St Bede's RC High School                                                               |
| 600A6485-C7AE-E311-B8ED-005056822391 | 10017638    | St Edwards School                                                                      |
| 550CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Heathfield Lower School                                                                |
| A03C451B-C7AE-E311-B8ED-005056822391 | 10002046    | The Dronfield School                                                                   |
| 477A589D-C7AE-E311-B8ED-005056822391 | 10015938    | John Grant School                                                                      |
| 5820B8C1-C6AE-E311-B8ED-005056822391 | 10076896    | Burstwick Community Primary School                                                     |
| 53298AE5-C6AE-E311-B8ED-005056822391 | 10079308    | St John the Baptist C of E School                                                      |
| 946EC7B5-C6AE-E311-B8ED-005056822391 | 10079048    | Whitehill Junior School                                                                |
| 4C62F591-C6AE-E311-B8ED-005056822391 | 10080067    | Prince of Wales Primary School                                                         |
| 34A87A5B-C7AE-E311-B8ED-005056822391 | NULL        | Davenant Foundation School                                                             |
| 248E540F-C7AE-E311-B8ED-005056822391 | 10002583    | Frogmore Community School                                                              |
| 8111E69D-C6AE-E311-B8ED-005056822391 | NULL        | Vicarage Primary School                                                                |
| 4A26A1D3-C6AE-E311-B8ED-005056822391 | 10076331    | St Peters CE                                                                           |
| A42A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Joseph's RC Junior                                                                  |
| B476B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Bardfield Primary School                                                               |
| C93073F7-C6AE-E311-B8ED-005056822391 | 10016887    | Marshall's Park School                                                                 |
| C93F8F49-C7AE-E311-B8ED-005056822391 | 10006837    | The Trinity RC School                                                                  |
| 8F63F591-C6AE-E311-B8ED-005056822391 | 10070935    | Nevill Road Infant School                                                              |
| BF536A73-C7AE-E311-B8ED-005056822391 | 10004420    | Monkton Combe School                                                                   |
| CDAF3A27-C7AE-E311-B8ED-005056822391 | NULL        | St John's (CofE) Primary Academy, Clifton                                              |
| DD866BFD-C6AE-E311-B8ED-005056822391 | 10001321    | Charles Burrell High School                                                            |
| 5FC72DD3-C7AE-E311-B8ED-005056822391 | NULL        | King's College London                                                                  |
| 43C0D6A9-C6AE-E311-B8ED-005056822391 | 10074040    | Holton Le Clay Infants School                                                          |
| 7A2F73F7-C6AE-E311-B8ED-005056822391 | 10002545    | Fortismere Secondary School                                                            |
| 5D69DEA3-C6AE-E311-B8ED-005056822391 | 10077718    | Waterfoot CP School                                                                    |
| 0C8282EB-C6AE-E311-B8ED-005056822391 | NULL        | The Deepings School                                                                    |
| 473E8F49-C7AE-E311-B8ED-005056822391 | 10002881    | Handsworth Grammar School for Boys                                                     |
| ACB40486-C6AE-E311-B8ED-005056822391 | NULL        | Summerbank Primary School                                                              |
| 41385C09-C7AE-E311-B8ED-005056822391 | 10003064    | High Tunstall School                                                                   |
| 68D391DF-C6AE-E311-B8ED-005056822391 | 10074951    | Sacred Heart RC Primary School                                                         |
| 58896BFD-C6AE-E311-B8ED-005056822391 | 10017002    | The Cavendish School                                                                   |
| A3C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Robsack Wood Community Primary School                                                  |
| B53173F7-C6AE-E311-B8ED-005056822391 | 10000991    | Burford School                                                                         |
| 66E16303-C7AE-E311-B8ED-005056822391 | 10006360    | Stowupland High School                                                                 |
| 23D250A3-C7AE-E311-B8ED-005056822391 | 10004829    | Oak Lodge School                                                                       |
| 713E8F49-C7AE-E311-B8ED-005056822391 | 10001349    | Chellaston School                                                                      |
| 50EE7F55-C7AE-E311-B8ED-005056822391 | 10000768    | Hugh Faringdon Catholic School                                                         |
| 427035CD-C7AE-E311-B8ED-005056822391 | NULL        | Anglia Ruskin University                                                               |
| 4CB30486-C6AE-E311-B8ED-005056822391 | 10069536    | Gurnard Primary School                                                                 |
| 06B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Pewley Down School                                                                     |
| 8FD291DF-C6AE-E311-B8ED-005056822391 | NULL        | St. Teresa's RC Primary School                                                         |
| E0BAED97-C6AE-E311-B8ED-005056822391 | NULL        | Burrowmoor County Primary School                                                       |
| B75D0C80-C6AE-E311-B8ED-005056822391 | NULL        | The Calder High School                                                                 |
| 8A1FB8C1-C6AE-E311-B8ED-005056822391 | 10069504    | Kings Farm Primary School                                                              |
| CFDB7AF1-C6AE-E311-B8ED-005056822391 | NULL        | The Brackenhale School                                                                 |
| 2DD291DF-C6AE-E311-B8ED-005056822391 | 10074462    | St Augustine's Roman Catholic Voluntary Aided Primary School                           |
| 53615C8B-C7AE-E311-B8ED-005056822391 | 10014955    | Ardingly College                                                                       |
| 2E17CFAF-C6AE-E311-B8ED-005056822391 | 10075265    | Grange Primary School                                                                  |
| B776A6C4-CF99-E511-B8CB-005056822390 | NULL        | King Alfred's College                                                                  |
| 7BF53F21-C7AE-E311-B8ED-005056822391 | 10017684    | St Joseph's School                                                                     |
| 217A589D-C7AE-E311-B8ED-005056822391 | NULL        | Glenwood School                                                                        |
| 86B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Takeley Primary School                                                                 |
| 503F451B-C7AE-E311-B8ED-005056822391 | 10006273    | St Aidan's Catholic Academy                                                            |
| 9ACB372D-C7AE-E311-B8ED-005056822391 | NULL        | Prince Avenue Primary School                                                           |
| CEBAED97-C6AE-E311-B8ED-005056822391 | 10068769    | Cliffe Hill Community Primary School                                                   |
| 9D1ACFAF-C6AE-E311-B8ED-005056822391 | 10069753    | Foxdell Infant School                                                                  |
| 5FB30486-C6AE-E311-B8ED-005056822391 | 10076579    | Shaw Hill Primary                                                                      |
| C917CFAF-C6AE-E311-B8ED-005056822391 | 10074532    | Hemingford Grey Primary School                                                         |
| C463F591-C6AE-E311-B8ED-005056822391 | 10071304    | Parsloes Primary School                                                                |
| CFC1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | The Hills Lower School                                                                 |
| 23DB7AF1-C6AE-E311-B8ED-005056822391 | 10006687    | The Heathland School                                                                   |
| AAC5BFBB-C6AE-E311-B8ED-005056822391 | 10076160    | Sparkenhoe Community Primary School                                                    |
| 3213E69D-C6AE-E311-B8ED-005056822391 | 10069210    | Ringshall Primary School                                                               |
| B8896BFD-C6AE-E311-B8ED-005056822391 | 10007618    | Wootton Bassett School                                                                 |
| C48282EB-C6AE-E311-B8ED-005056822391 | 10002181    | Edmonton County School                                                                 |
| B5CCA8CD-C6AE-E311-B8ED-005056822391 | 10076904    | Commonswood School                                                                     |
| 6EE16303-C7AE-E311-B8ED-005056822391 | 10004635    | Nicholas Chamberlaine Comprehensive School                                             |
| 0D26A1D3-C6AE-E311-B8ED-005056822391 | 10078132    | Hippings Methodist Primary School                                                      |
| 207B99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Saviour's Church of England Infant School                                           |
| 1A8382EB-C6AE-E311-B8ED-005056822391 | 10007388    | Wembley High School                                                                    |
| 0814E69D-C6AE-E311-B8ED-005056822391 | NULL        | Weston Coyney Infants' School                                                          |
| 395E0C80-C6AE-E311-B8ED-005056822391 | 10074552    | Oak Green School                                                                       |
| 346BDEA3-C6AE-E311-B8ED-005056822391 | 10079840    | Woodside Junior                                                                        |
| 74F63F21-C7AE-E311-B8ED-005056822391 | 10001257    | Central Foundation School                                                              |
| D418CFAF-C6AE-E311-B8ED-005056822391 | 10074549    | Francis Edmonds Combined School                                                        |
| 093D451B-C7AE-E311-B8ED-005056822391 | 10000136    | Addey and Stanhope School                                                              |
| 8CBFD6A9-C6AE-E311-B8ED-005056822391 | 10079839    | Broughton Junior School                                                                |
| AABFD6A9-C6AE-E311-B8ED-005056822391 | 10076564    | Park Hill Primary School                                                               |
| 6C896BFD-C6AE-E311-B8ED-005056822391 | 10018815    | Saxmundham Middle School                                                               |
| 29D97AF1-C6AE-E311-B8ED-005056822391 | 10017729    | The King's School Specialising in Mathematics and Computing                            |
| 53BAED97-C6AE-E311-B8ED-005056822391 | NULL        | Florence Primary School                                                                |
| A80CFD8B-C6AE-E311-B8ED-005056822391 | 10071508    | Lathom Primary School                                                                  |
| 470A6485-C7AE-E311-B8ED-005056822391 | 10016207    | Lord Wandsworth College                                                                |
| 8265F591-C6AE-E311-B8ED-005056822391 | NULL        | Nine Mile Ride Primary School                                                          |
| 8A8282EB-C6AE-E311-B8ED-005056822391 | NULL        | Oxstalls Community                                                                     |
| 44B40486-C6AE-E311-B8ED-005056822391 | 10040307    | Coteford Junior School                                                                 |
| BFC5BFBB-C6AE-E311-B8ED-005056822391 | 10078031    | Lauriston Primary School                                                               |
| AFF53F21-C7AE-E311-B8ED-005056822391 | 10017797    | St Richards Catholic College                                                           |
| F12C8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Catherine's C of E (VA) School                                                      |
| C86EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | North Wootton Community School                                                         |
| ADE74C15-C7AE-E311-B8ED-005056822391 | 10000940    | Broadgreen High School                                                                 |
| 6CEE7F55-C7AE-E311-B8ED-005056822391 | 10005330    | Queen Elizabeth's Grammar School                                                       |
| 71C6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Abbey Farm Junior School                                                               |
| CD12E69D-C6AE-E311-B8ED-005056822391 | NULL        | Plains Farm Primary School                                                             |
| 7BD391DF-C6AE-E311-B8ED-005056822391 | NULL        | St Bernadettes RC Primary School                                                       |
| 6CF73F21-C7AE-E311-B8ED-005056822391 | 10006286    | St Leonard's Roman Catholic Voluntary Aided Compre                                     |
| 500A6485-C7AE-E311-B8ED-005056822391 | 10016006    | ISP Senior School                                                                      |
| E3F63F21-C7AE-E311-B8ED-005056822391 | 10006838    | The Trinity Catholic Technology College                                                |
| AB0CFD8B-C6AE-E311-B8ED-005056822391 | 10070937    | Great Moor Infants School                                                              |
| 14DA7AF1-C6AE-E311-B8ED-005056822391 | 10015151    | Bow School                                                                             |
| D20A6485-C7AE-E311-B8ED-005056822391 | 10008892    | Abbey College                                                                          |
| 6D096485-C7AE-E311-B8ED-005056822391 | 10015990    | Underley Garden School                                                                 |
| 42F63F21-C7AE-E311-B8ED-005056822391 | 10018185    | St Aidans CE Technology College                                                        |
| BEE54C15-C7AE-E311-B8ED-005056822391 | 10015165    | City of Portsmouth Girls' School                                                       |
| 9EDF6303-C7AE-E311-B8ED-005056822391 | 10018804    | Trevelyan Middle School                                                                |
| 35B60486-C6AE-E311-B8ED-005056822391 | 10076614    | Gilbert Colvin Primary                                                                 |
| A868DEA3-C6AE-E311-B8ED-005056822391 | 10073364    | Bean Primary School                                                                    |
| 29DE6303-C7AE-E311-B8ED-005056822391 | 10007176    | Upper Avon School                                                                      |
| 5E2A8AE5-C6AE-E311-B8ED-005056822391 | 10070722    | St Vincents RC Primary School                                                          |
| 2E71C7B5-C6AE-E311-B8ED-005056822391 | 10076172    | Abbey Primary Community School                                                         |
| 35C7BFBB-C6AE-E311-B8ED-005056822391 | 10069547    | Mary Exton JMI School                                                                  |
| E5DE6303-C7AE-E311-B8ED-005056822391 | 10017029    | The Lammas School                                                                      |
| 736ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Gorsey Bank Primary School                                                             |
| 9CDE6303-C7AE-E311-B8ED-005056822391 | 10005654    | Titus Salt School                                                                      |
| CBFE7261-C7AE-E311-B8ED-005056822391 | 10004300    | Meopham School                                                                         |
| FF8082EB-C6AE-E311-B8ED-005056822391 | 10015335    | Counthill High                                                                         |
| 0BED7F55-C7AE-E311-B8ED-005056822391 | 10002973    | Heanor Gate School                                                                     |
| F7E06303-C7AE-E311-B8ED-005056822391 | 10001847    | Darlaston Community School                                                             |
| DF5E0C80-C6AE-E311-B8ED-005056822391 | 10074565    | Caversham Primary School                                                               |
| 416EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | College House Junior School                                                            |
| 463C451B-C7AE-E311-B8ED-005056822391 | 10004157    | Maidstone Grammar School for Girls                                                     |
| A5375C09-C7AE-E311-B8ED-005056822391 | 10007611    | Woodhouse High School                                                                  |
| A0E54C15-C7AE-E311-B8ED-005056822391 | 10015790    | Eastbourne School                                                                      |
| 0175B0C7-C6AE-E311-B8ED-005056822391 | 10069171    | Mulgrave Primary School                                                                |
| E06FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Cromwell Rd Community School                                                           |
| A4F73F21-C7AE-E311-B8ED-005056822391 | 10000731    | Bishops Stopfords School                                                               |
| 14BAED97-C6AE-E311-B8ED-005056822391 | 10070023    | Wilbury Primary School                                                                 |
| 06CC372D-C7AE-E311-B8ED-005056822391 | 10002822    | Gumley House Convent School                                                            |
| 0D20B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Walter D'Ayncourt Community School                                                     |
| 2BCC372D-C7AE-E311-B8ED-005056822391 | 10000574    | Baverstock School                                                                      |
| 390A6485-C7AE-E311-B8ED-005056822391 | NULL        | John Loughborough School                                                               |
| AA096485-C7AE-E311-B8ED-005056822391 | 10016349    | The Nativity School                                                                    |
| F00AFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Wainstalls School                                                                      |
| 7369DEA3-C6AE-E311-B8ED-005056822391 | 10079877    | Hyrstmount Junior School                                                               |
| C2BAED97-C6AE-E311-B8ED-005056822391 | 10076581    | Stoneydown Park School                                                                 |
| AE6ADEA3-C6AE-E311-B8ED-005056822391 | 10069542    | South Ferriby Primary School                                                           |
| 7A19CFAF-C6AE-E311-B8ED-005056822391 | 10077898    | Wellsmead First School                                                                 |
| D2605C8B-C7AE-E311-B8ED-005056822391 | 10033893    | St Paul's School                                                                       |
| BC298AE5-C6AE-E311-B8ED-005056822391 | NULL        | The Rosary RC Junior School                                                            |
| 9B25A1D3-C6AE-E311-B8ED-005056822391 | NULL        | St Andrew's CE Primary School                                                          |
| 69395C09-C7AE-E311-B8ED-005056822391 | 10002403    | Falmouth Community School                                                              |
| 99876BFD-C6AE-E311-B8ED-005056822391 | 10003101    | Iveshead School                                                                        |
| 28CDA8CD-C6AE-E311-B8ED-005056822391 | 10068555    | St Luke's CE Primary                                                                   |
| CB355C09-C7AE-E311-B8ED-005056822391 | NULL        | Manor Farm Community School                                                            |
| 60EE7F55-C7AE-E311-B8ED-005056822391 | 10004604    | Newent Community School                                                                |
| 7AE74C15-C7AE-E311-B8ED-005056822391 | 10016911    | Meole Brace School                                                                     |
| A48D540F-C7AE-E311-B8ED-005056822391 | 10004624    | Newquay Treterras School Community School                                              |
| 925E0C80-C6AE-E311-B8ED-005056822391 | 10070626    | Balksbury Junior School                                                                |
| B2E44C15-C7AE-E311-B8ED-005056822391 | 10017237    | The North School                                                                       |
| 4570C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Angel Road First School                                                                |
| 5F6ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Wessex Infant School                                                                   |
| 893E451B-C7AE-E311-B8ED-005056822391 | 10017613    | St Peters High School                                                                  |
| C93E451B-C7AE-E311-B8ED-005056822391 | NULL        | Alban C of E Middle School                                                             |
| DC8382EB-C6AE-E311-B8ED-005056822391 | 10005195    | Presdales School                                                                       |
| C62A8AE5-C6AE-E311-B8ED-005056822391 | 10080483    | Hasmonean Preparatory                                                                  |
| 7EE44C15-C7AE-E311-B8ED-005056822391 | 10005974    | South Craven School                                                                    |
| A2E06303-C7AE-E311-B8ED-005056822391 | NULL        | Vandyke Upper School                                                                   |
| 35B30486-C6AE-E311-B8ED-005056822391 | 10072636    | Bolshaw Primary School                                                                 |
| CBDF6303-C7AE-E311-B8ED-005056822391 | 10005874    | Sir William Ramsay School                                                              |
| A93F8F49-C7AE-E311-B8ED-005056822391 | 10000206    | Aldridge School                                                                        |
| 188482EB-C6AE-E311-B8ED-005056822391 | 10016309    | Little Ilford School                                                                   |
| C63C451B-C7AE-E311-B8ED-005056822391 | NULL        | Sandford CE VC Middle School                                                           |
| 8F096485-C7AE-E311-B8ED-005056822391 | NULL        | Guildford High School for Girls                                                        |
| D2CCA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Mary's Primary School                                                               |
| 85C7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Highwoods Community Primary School                                                     |
| DB615C8B-C7AE-E311-B8ED-005056822391 | 10018702    | Abercorn School                                                                        |
| C7536A73-C7AE-E311-B8ED-005056822391 | 10043890    | Chase Academy                                                                          |
| 9B5D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Maxilla Nursery School                                                                 |
| 0D3F8F49-C7AE-E311-B8ED-005056822391 | 10001348    | Cheam High School                                                                      |
| 243273F7-C6AE-E311-B8ED-005056822391 | 10006755    | The Misbourne School                                                                   |
| 060EFD8B-C6AE-E311-B8ED-005056822391 | 10078035    | Berger Primary School                                                                  |
| B51A7067-C7AE-E311-B8ED-005056822391 | 10016860    | Manningtree High School                                                                |
| AF3C451B-C7AE-E311-B8ED-005056822391 | 10014844    | Esher CE High School                                                                   |
| 58D45197-C7AE-E311-B8ED-005056822391 | 10015974    | Trinity School                                                                         |
| 938E540F-C7AE-E311-B8ED-005056822391 | 10006492    | Tamarside Community College                                                            |
| 9E0A6485-C7AE-E311-B8ED-005056822391 | 10015174    | Brondesbury College for Boys                                                           |
| 51385C09-C7AE-E311-B8ED-005056822391 | 10016862    | Manor College of Technology                                                            |
| BF10E69D-C6AE-E311-B8ED-005056822391 | 10073240    | Telscombe Cliffs Community Primary School                                              |
| 2612E69D-C6AE-E311-B8ED-005056822391 | NULL        | Gallions Primary School                                                                |
| C28F540F-C7AE-E311-B8ED-005056822391 | 10002066    | Durham Johnston Comprehensive School                                                   |
| E43173F7-C6AE-E311-B8ED-005056822391 | 10015170    | Bowland High School                                                                    |
| AC75B0C7-C6AE-E311-B8ED-005056822391 | 10069900    | Coates Lane Primary School                                                             |
| 26859443-C7AE-E311-B8ED-005056822391 | 10006648    | The Crossley Heath School                                                              |
| 6165F591-C6AE-E311-B8ED-005056822391 | 10079718    | Chalgrove Primary School                                                               |
| 627C99D9-C6AE-E311-B8ED-005056822391 | 10069046    | Newchurch CE School                                                                    |
| 0991540F-C7AE-E311-B8ED-005056822391 | 10001529    | Cockshut Hill Technology Centre                                                        |
| 8E1B7067-C7AE-E311-B8ED-005056822391 | 10015874    | Wellington College                                                                     |
| 7F74B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Pear Tree Mead Primary and Nursery School                                              |
| 25CDA8CD-C6AE-E311-B8ED-005056822391 | 10076741    | St Martin's C of E Primary School                                                      |
| CF13E69D-C6AE-E311-B8ED-005056822391 | NULL        | Fairlands Primary School                                                               |
| 7EDA7AF1-C6AE-E311-B8ED-005056822391 | NULL        | The Gateway Community College                                                          |
| 382C8AE5-C6AE-E311-B8ED-005056822391 | 10076006    | Notre Dame Catholic Primary School                                                     |
| 4ED97AF1-C6AE-E311-B8ED-005056822391 | 10007382    | Welling School                                                                         |
| 1A8282EB-C6AE-E311-B8ED-005056822391 | 10015696    | Hamptom Community College                                                              |
| 4E6BDEA3-C6AE-E311-B8ED-005056822391 | 10075067    | Cubitt Town Junior School                                                              |
| 0770C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Rosebery Primary School                                                                |
| 352D8AE5-C6AE-E311-B8ED-005056822391 | 10001209    | Carshalton High School                                                                 |
| DF25A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Manor Park First                                                                       |
| A218CFAF-C6AE-E311-B8ED-005056822391 | 10069577    | Wildern School                                                                         |
| 0EB60486-C6AE-E311-B8ED-005056822391 | 10080064    | Earlsmead County Primary School                                                        |
| 625D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Fortune Park Early Years Centre                                                        |
| 9065F591-C6AE-E311-B8ED-005056822391 | NULL        | Storey Primary School                                                                  |
| C2B50486-C6AE-E311-B8ED-005056822391 | 10072150    | Middle Street Primary School                                                           |
| 029A6F6D-C7AE-E311-B8ED-005056822391 | NULL        | Bedford School                                                                         |
| 4811E69D-C6AE-E311-B8ED-005056822391 | 10076368    | Valewood Primary School                                                                |
| E496874F-C7AE-E311-B8ED-005056822391 | 10005419    | Rednock School                                                                         |
| 6219CFAF-C6AE-E311-B8ED-005056822391 | 10068949    | Hawkedon Primary School                                                                |
| 20C0D6A9-C6AE-E311-B8ED-005056822391 | 10081359    | Roundhill Primary School                                                               |
| 478D540F-C7AE-E311-B8ED-005056822391 | NULL        | Bishop Heber High School                                                               |
| 383C451B-C7AE-E311-B8ED-005056822391 | 10006767    | The Norton Knatchbull School                                                           |
| C8DB7AF1-C6AE-E311-B8ED-005056822391 | 10017305    | The Garendon High School                                                               |
| 1BE16303-C7AE-E311-B8ED-005056822391 | 10002741    | Grange Technology College                                                              |
| 543E451B-C7AE-E311-B8ED-005056822391 | 10007057    | Trinity C of E Voluntary Aided Comprehensive Schoo                                     |
| 64EE7F55-C7AE-E311-B8ED-005056822391 | 10004445    | Mount Grace School                                                                     |
| 9C12E69D-C6AE-E311-B8ED-005056822391 | 10069240    | Holden Lane Primary                                                                    |
| 9B8082EB-C6AE-E311-B8ED-005056822391 | NULL        | St Francis Church of England VA Primary School                                         |
| C3D97AF1-C6AE-E311-B8ED-005056822391 | 10018853    | Solent Middle School                                                                   |
| 942C8AE5-C6AE-E311-B8ED-005056822391 | 10072736    | St Augustine of Canterbury Catholic Primary School                                     |
| BF1DB8C1-C6AE-E311-B8ED-005056822391 | 10073848    | Shacklewell Primary                                                                    |
| FC8E540F-C7AE-E311-B8ED-005056822391 | 10006673    | The Grange School                                                                      |
| 4BC1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Manor Hall First School                                                                |
| 9D71C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Alderman William Jones Primary School                                                  |
| 178282EB-C6AE-E311-B8ED-005056822391 | 10006933    | Tolworth Girls School                                                                  |
| 81CD00C3-C9AE-E311-B8ED-005056822391 | NULL        | University of Manchester                                                               |
| 6763F591-C6AE-E311-B8ED-005056822391 | NULL        | Rendell Primary School                                                                 |
| 62ED667F-C7AE-E311-B8ED-005056822391 | 10018764    | All Hallows Preparatory                                                                |
| 76385C09-C7AE-E311-B8ED-005056822391 | 10005805    | Sheredes School                                                                        |
| D9B50486-C6AE-E311-B8ED-005056822391 | 10071942    | Mount Stewart Infant School                                                            |
| 32CCA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Andrews Lower School                                                                |
| B9D45197-C7AE-E311-B8ED-005056822391 | 10015318    | Eleanor Smith School & Primary Support Service                                         |
| 4EBBED97-C6AE-E311-B8ED-005056822391 | NULL        | Speenhamland Primary School                                                            |
| 901ACFAF-C6AE-E311-B8ED-005056822391 | NULL        | Prince of Wales School                                                                 |
| 1FC8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Maybury Primary School                                                                 |
| 812F73F7-C6AE-E311-B8ED-005056822391 | 10005764    | Seven King's High                                                                      |
| B617CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Holmesdale Infant School                                                               |
| 7619CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Maidenhall Infant School                                                               |
| A1B30486-C6AE-E311-B8ED-005056822391 | 10073630    | Lodge Farm Primary School                                                              |
| CD5E0C80-C6AE-E311-B8ED-005056822391 | NULL        | The Parks                                                                              |
| 6875B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Windrush Primary School                                                                |
| F6D291DF-C6AE-E311-B8ED-005056822391 | 10075457    | St Wilfrid's CE Primary School                                                         |
| CACBA8CD-C6AE-E311-B8ED-005056822391 | 10078508    | St Marys Junior School                                                                 |
| 89D291DF-C6AE-E311-B8ED-005056822391 | 10076882    | Cottesmore St Mary's                                                                   |
| C523A1D3-C6AE-E311-B8ED-005056822391 | 10070343    | Haxey CE Primary                                                                       |
| 0D3273F7-C6AE-E311-B8ED-005056822391 | 10015700    | Withins School                                                                         |
| F8D77AF1-C6AE-E311-B8ED-005056822391 | 10004423    | Montsaye School                                                                        |
| 8E2C8AE5-C6AE-E311-B8ED-005056822391 | 10072670    | St John the Baptist Roman Catholic Primary School,                                     |
| 3C3E8F49-C7AE-E311-B8ED-005056822391 | 10006659    | Ellen Wilkinson Girls High School                                                      |
| D017CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Anchorsholme Primary School                                                            |
| 8A90540F-C7AE-E311-B8ED-005056822391 | 10003054    | Hextable School                                                                        |
| 99B60486-C6AE-E311-B8ED-005056822391 | NULL        | Beddington Park Primary                                                                |
| C6526A73-C7AE-E311-B8ED-005056822391 | 10008348    | The Leys School                                                                        |
| A93173F7-C6AE-E311-B8ED-005056822391 | 10015832    | West Craven High Technology College                                                    |
| A80C6579-C7AE-E311-B8ED-005056822391 | 10041538    | Anderida Learning Centre                                                               |
| 148A6BFD-C6AE-E311-B8ED-005056822391 | 10002543    | Fort Pitt Grammar School                                                               |
| F92F73F7-C6AE-E311-B8ED-005056822391 | 10017739    | Smith's Wood School                                                                    |
| 64CEA8CD-C6AE-E311-B8ED-005056822391 | 10069844    | St Paul's C of E Primary School                                                        |
| F112E69D-C6AE-E311-B8ED-005056822391 | 10072628    | Moss Hey Primary School                                                                |
| 935C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Evan Davies Nursery School                                                             |
| 317B99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Vincents Catholic Junior School                                                     |
| 0C7D99D9-C6AE-E311-B8ED-005056822391 | 10072069    | Padiham St Leonard's Voluntary Aided C of England                                      |
| B71DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Lee Chapel Primary                                                                     |
| F73F8F49-C7AE-E311-B8ED-005056822391 | 10003104    | Hipperholme and Lightcliffe High School                                                |
| 1BD87AF1-C6AE-E311-B8ED-005056822391 | 10015877    | Hope High School                                                                       |
| F218CFAF-C6AE-E311-B8ED-005056822391 | 10068979    | St Matthew's Primary School                                                            |
| D411E69D-C6AE-E311-B8ED-005056822391 | 10072131    | Chantry Community School                                                               |
| 0E76B0C7-C6AE-E311-B8ED-005056822391 | 10078023    | Grasmere School                                                                        |
| 5D2B8AE5-C6AE-E311-B8ED-005056822391 | 10072054    | Broughton In Amounderness C of E School                                                |
| 8E8182EB-C6AE-E311-B8ED-005056822391 | 10000974    | Buckingham School                                                                      |
| 47BAED97-C6AE-E311-B8ED-005056822391 | 10070894    | Marsden Infant and Nursery School                                                      |
| 85D591DF-C6AE-E311-B8ED-005056822391 | 10071707    | St Francis RC Primary School                                                           |
| F719CFAF-C6AE-E311-B8ED-005056822391 | 10076498    | Tibshelf Infant                                                                        |
| A0D291DF-C6AE-E311-B8ED-005056822391 | 10075548    | Trinity & St Michael's V/A C of E/Methodist Primar                                     |
| B68382EB-C6AE-E311-B8ED-005056822391 | 10016130    | Western Technology College                                                             |
| 0EC0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | The Marlborough Primary School                                                         |
| EB385C09-C7AE-E311-B8ED-005056822391 | 10015855    | Higham Lane School                                                                     |
| 7D0A6485-C7AE-E311-B8ED-005056822391 | 10008536    | Sutton High Sch.                                                                       |
| 155E0C80-C6AE-E311-B8ED-005056822391 | 10070279    | Chase Lane Infants                                                                     |
| FD18CFAF-C6AE-E311-B8ED-005056822391 | 10068978    | Stopsley Community Primary School                                                      |
| A3E44C15-C7AE-E311-B8ED-005056822391 | 10007329    | Walworth School                                                                        |
| C33273F7-C6AE-E311-B8ED-005056822391 | 10007487    | Bristol Metropolitan College                                                           |
| 5CDE6303-C7AE-E311-B8ED-005056822391 | 10016524    | Patcham High School                                                                    |
| 28D87AF1-C6AE-E311-B8ED-005056822391 | 10003004    | Hemsworth Arts and Community College                                                   |
| EBE64C15-C7AE-E311-B8ED-005056822391 | 10005582    | Rushcliffe School                                                                      |
| 75DB7AF1-C6AE-E311-B8ED-005056822391 | 10017536    | Rhyddings High School                                                                  |
| F6E74C15-C7AE-E311-B8ED-005056822391 | 10007303    | Walker Technology College                                                              |
| 662D8AE5-C6AE-E311-B8ED-005056822391 | 10017297    | Tanbridge House School                                                                 |
| 6CD87AF1-C6AE-E311-B8ED-005056822391 | 10003122    | The Woodrush High School                                                               |
| 0D1ACFAF-C6AE-E311-B8ED-005056822391 | 10074640    | Gordonbrock Primary School                                                             |
| B83073F7-C6AE-E311-B8ED-005056822391 | NULL        | Howard of Effingham School                                                             |
| 83BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Harraton Primary School                                                                |
| DF0BFD8B-C6AE-E311-B8ED-005056822391 | 10072116    | Frinton-on-Sea Primary School                                                          |
| 2F26A1D3-C6AE-E311-B8ED-005056822391 | NULL        | St Andrew's CE Primary                                                                 |
| 79886BFD-C6AE-E311-B8ED-005056822391 | 10015658    | Deincourt Community School                                                             |
| 11CEA8CD-C6AE-E311-B8ED-005056822391 | 10078507    | St Mary's Junior School                                                                |
| 500EFD8B-C6AE-E311-B8ED-005056822391 | 10080429    | Westfield County Infant School                                                         |
| C77C99D9-C6AE-E311-B8ED-005056822391 | 10079325    | St Paul's CE Primary                                                                   |
| FDB50486-C6AE-E311-B8ED-005056822391 | NULL        | Beech Hill Junior and Infant School                                                    |
| 07385C09-C7AE-E311-B8ED-005056822391 | NULL        | Langley High School                                                                    |
| 5CB40486-C6AE-E311-B8ED-005056822391 | 10071171    | Camelsdale First School                                                                |
| 7E0EFD8B-C6AE-E311-B8ED-005056822391 | 10073057    | Park Hill Primary                                                                      |
| 42D291DF-C6AE-E311-B8ED-005056822391 | NULL        | David King Infant School                                                               |
| 4C70C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Wybourn Community Primary and Nursery School                                           |
| 24DA7AF1-C6AE-E311-B8ED-005056822391 | 10000865    | Brentford School for Girls                                                             |
| 7264F591-C6AE-E311-B8ED-005056822391 | 10071914    | Yeading Infant & Nursery School                                                        |
| 19D291DF-C6AE-E311-B8ED-005056822391 | 10072460    | Holy Cross CE Primary                                                                  |
| 38C2D6A9-C6AE-E311-B8ED-005056822391 | 10069235    | Bridgtown Primary School                                                               |
| CC74B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Old Ford Junior Mixed School                                                           |
| 3DD391DF-C6AE-E311-B8ED-005056822391 | 10079298    | Holy Trinity CE School                                                                 |
| 8E385C09-C7AE-E311-B8ED-005056822391 | 10001430    | Churchill Academy                                                                      |
| EED45197-C7AE-E311-B8ED-005056822391 | 10031038    | Heathside School                                                                       |
| 74CCA8CD-C6AE-E311-B8ED-005056822391 | 10073975    | St Aidans CE Primary School                                                            |
| E2AF3A27-C7AE-E311-B8ED-005056822391 | 10069590    | Kilburn Park Foundation School                                                         |
| 35D97AF1-C6AE-E311-B8ED-005056822391 | 10003071    | Highdown School                                                                        |
| 64DE6303-C7AE-E311-B8ED-005056822391 | 10008951    | Moorside High School                                                                   |
| D5B60486-C6AE-E311-B8ED-005056822391 | 10071521    | Harefield Junior School                                                                |
| 45B30486-C6AE-E311-B8ED-005056822391 | NULL        | Penzance Infant School                                                                 |
| 5C0DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Burton End Community Primary                                                           |
| A3B50486-C6AE-E311-B8ED-005056822391 | NULL        | Deansbrook Junior School                                                               |
| 036BDEA3-C6AE-E311-B8ED-005056822391 | 10070692    | Granby Junior School                                                                   |
| A03E8F49-C7AE-E311-B8ED-005056822391 | 10003769    | Lancaster Girls' Grammar School                                                        |
| 7EB50486-C6AE-E311-B8ED-005056822391 | NULL        | Beaver Road Primary School                                                             |
| 2062F591-C6AE-E311-B8ED-005056822391 | 10076420    | Park Primary School                                                                    |
| F016CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Hathaway Primary School                                                                |
| 9C3273F7-C6AE-E311-B8ED-005056822391 | 10017553    | Sandon High School                                                                     |
| A6D87AF1-C6AE-E311-B8ED-005056822391 | 10018254    | South Charnwood High School                                                            |
| 746EC7B5-C6AE-E311-B8ED-005056822391 | 10077082    | Hiltingbury Junior School                                                              |
| BE355C09-C7AE-E311-B8ED-005056822391 | 10002018    | Dover Grammar School for Girls                                                         |
| 667B99D9-C6AE-E311-B8ED-005056822391 | 10070173    | Our Lady of the Most Holy Rosary RC Primary School                                     |
| 700A6485-C7AE-E311-B8ED-005056822391 | 10077620    | Kerem Primary School                                                                   |
| D63173F7-C6AE-E311-B8ED-005056822391 | 10016663    | Monkwearmouth Academy                                                                  |
| 2A1EB8C1-C6AE-E311-B8ED-005056822391 | 10080721    | Fairhouse County Junior School                                                         |
| 55C2D6A9-C6AE-E311-B8ED-005056822391 | 10069579    | Titchfield Primary School                                                              |
| F2E74C15-C7AE-E311-B8ED-005056822391 | 10001873    | Dayncourt School                                                                       |
| DE8F540F-C7AE-E311-B8ED-005056822391 | 10005035    | Penketh High                                                                           |
| EEF73F21-C7AE-E311-B8ED-005056822391 | 10004916    | Our Lady's High School                                                                 |
| 8B375C09-C7AE-E311-B8ED-005056822391 | 10017153    | The Hereson School                                                                     |
| FC8F540F-C7AE-E311-B8ED-005056822391 | 10015332    | Cove School                                                                            |
| 31365C09-C7AE-E311-B8ED-005056822391 | NULL        | Milham Ford School                                                                     |
| 06B60486-C6AE-E311-B8ED-005056822391 | 10080152    | Meadows First School                                                                   |
| 66375C09-C7AE-E311-B8ED-005056822391 | 10001396    | Chilwell Comprehensive School                                                          |
| 1BE84C15-C7AE-E311-B8ED-005056822391 | NULL        | Abbylands School                                                                       |
| 3F3F451B-C7AE-E311-B8ED-005056822391 | 10010913    | The Holy Trinity School                                                                |
| E8D45197-C7AE-E311-B8ED-005056822391 | 10077020    | Greenfold School                                                                       |
| 88849443-C7AE-E311-B8ED-005056822391 | 10000497    | Bacup & Rawtenstall Grammar School                                                     |
| 1B96874F-C7AE-E311-B8ED-005056822391 | 10003628    | King Edward VI Grammar School                                                          |
| C38E540F-C7AE-E311-B8ED-005056822391 | 10003076    | Highfields School                                                                      |
| E7CCA8CD-C6AE-E311-B8ED-005056822391 | 10071019    | Cross In Hand Primary School                                                           |
| 23C0D6A9-C6AE-E311-B8ED-005056822391 | 10072121    | Woodingdean Primary School                                                             |
| 978282EB-C6AE-E311-B8ED-005056822391 | 10015337    | Forest Gate Community School                                                           |
| 25EE7F55-C7AE-E311-B8ED-005056822391 | 10017636    | Thamesmead School                                                                      |
| 10D250A3-C7AE-E311-B8ED-005056822391 | 10016885    | Market Field School                                                                    |
| F3365C09-C7AE-E311-B8ED-005056822391 | NULL        | Woodland Middle School                                                                 |
| 3417CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Clenchwarton Primary School                                                            |
| D78D540F-C7AE-E311-B8ED-005056822391 | 10016329    | Looe Community School                                                                  |
| 7C26A1D3-C6AE-E311-B8ED-005056822391 | 10069093    | Sibertswold CE Primary                                                                 |
| EFBFD6A9-C6AE-E311-B8ED-005056822391 | 10072124    | Elm Grove Primary School                                                               |
| 5B365C09-C7AE-E311-B8ED-005056822391 | 10016327    | Langdon Park Community School                                                          |
| AD3E451B-C7AE-E311-B8ED-005056822391 | 10015379    | Chatsmore Catholic High School                                                         |
| F15D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Notley Green Primary School                                                            |
| 69B40486-C6AE-E311-B8ED-005056822391 | 10080414    | Coteford Infant School                                                                 |
| 3765F591-C6AE-E311-B8ED-005056822391 | 10075870    | Heron Cross Primary                                                                    |
| 0619CFAF-C6AE-E311-B8ED-005056822391 | 10053285    | Firside Middle School                                                                  |
| E28282EB-C6AE-E311-B8ED-005056822391 | NULL        | Warsett School                                                                         |
| 7F63F591-C6AE-E311-B8ED-005056822391 | 10081251    | Edward Pauling School                                                                  |
| DD0CFD8B-C6AE-E311-B8ED-005056822391 | 10043582    | Heaton Primary School                                                                  |
| 4E64F591-C6AE-E311-B8ED-005056822391 | NULL        | Rosetta Primary School                                                                 |
| 752B8AE5-C6AE-E311-B8ED-005056822391 | 10074021    | St Peter's Church of England Primary School                                            |
| E317CFAF-C6AE-E311-B8ED-005056822391 | 10073159    | Catshill Road                                                                          |
| 06395C09-C7AE-E311-B8ED-005056822391 | 10016982    | The Hurst Community                                                                    |
| 652A8AE5-C6AE-E311-B8ED-005056822391 | 10072723    | St Marys & St Joseph RC School                                                         |
| AA63F591-C6AE-E311-B8ED-005056822391 | NULL        | Kempston Rural Lower School                                                            |
| 560D6579-C7AE-E311-B8ED-005056822391 | 10008366    | Loughborough High School                                                               |
| A769DEA3-C6AE-E311-B8ED-005056822391 | 10078005    | Copenhagen Primary School                                                              |
| 56D55197-C7AE-E311-B8ED-005056822391 | NULL        | Thornton House School                                                                  |
| 15395C09-C7AE-E311-B8ED-005056822391 | 10006886    | Thomas Alleyne's High School                                                           |
| 9FB60486-C6AE-E311-B8ED-005056822391 | NULL        | Newport Junior School                                                                  |
| 2EBAED97-C6AE-E311-B8ED-005056822391 | 10076345    | Claypool Primary School                                                                |
| 9B2D8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Anne's Catholic Primary School                                                      |
| B2DB7AF1-C6AE-E311-B8ED-005056822391 | 10007534    | Winchmore School                                                                       |
| 2BD87AF1-C6AE-E311-B8ED-005056822391 | 10016353    | King George V School                                                                   |
| 3513E69D-C6AE-E311-B8ED-005056822391 | NULL        | Castle Hill Primary School                                                             |
| 6275B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Broadacre Primary School                                                               |
| 296EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Greenacres Primary School                                                              |
| C47A589D-C7AE-E311-B8ED-005056822391 | 10014849    | Bishopswood School                                                                     |
| 5A76B0C7-C6AE-E311-B8ED-005056822391 | 10046669    | Bygrove Primary School                                                                 |
| 0924A1D3-C6AE-E311-B8ED-005056822391 | 10073763    | Icklesham Church of England Primary School                                             |
| 87096485-C7AE-E311-B8ED-005056822391 | 10008170    | Coke Thorpe School                                                                     |
| 12408F49-C7AE-E311-B8ED-005056822391 | 10006694    | The Hollyfield School                                                                  |
| EB0C6579-C7AE-E311-B8ED-005056822391 | 10000898    | Bristol Grammar School                                                                 |
| F03E8F49-C7AE-E311-B8ED-005056822391 | 10000664    | Beths Grammer School for Boys                                                          |
| 6C68DEA3-C6AE-E311-B8ED-005056822391 | 10070010    | Kings Norton Primary School                                                            |
| 6FD291DF-C6AE-E311-B8ED-005056822391 | 10076883    | St Mary's RC Primary School                                                            |
| 7325A1D3-C6AE-E311-B8ED-005056822391 | 10079335    | Etching Hill Primary School                                                            |
| A02A8AE5-C6AE-E311-B8ED-005056822391 | 10073753    | Rosh Pinah Jewish Primary School                                                       |
| 0BC0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Coombes Infant and Nursery School                                                      |
| 5FE54C15-C7AE-E311-B8ED-005056822391 | 10008661    | North Manchester High School for Girls                                                 |
| D4E74C15-C7AE-E311-B8ED-005056822391 | 10000615    | Bedlingtonshire Community High School                                                  |
| 6D876BFD-C6AE-E311-B8ED-005056822391 | NULL        | Theale Green Community School                                                          |
| ECC8BFBB-C6AE-E311-B8ED-005056822391 | 10075336    | South Farnborough Infant School                                                        |
| 9B3073F7-C6AE-E311-B8ED-005056822391 | 10005722    | Seaford Head Community College                                                         |
| 6EBAED97-C6AE-E311-B8ED-005056822391 | 10071912    | Whiteheath Infant School                                                               |
| FD8C540F-C7AE-E311-B8ED-005056822391 | NULL        | Warlingham School                                                                      |
| 47E64C15-C7AE-E311-B8ED-005056822391 | 10010031    | Stoke Newington School                                                                 |
| 600BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | William Barnes Primary School                                                          |
| 267B99D9-C6AE-E311-B8ED-005056822391 | 10073974    | St Michael's School                                                                    |
| A7849443-C7AE-E311-B8ED-005056822391 | 10002816    | George Abbot School                                                                    |
| EC3D451B-C7AE-E311-B8ED-005056822391 | 10002026    | Dr Challoners Grammar School                                                           |
| E911E69D-C6AE-E311-B8ED-005056822391 | NULL        | Davidson Junior School                                                                 |
| C05E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Prince Albert Junior and Infant School                                                 |
| A9615C8B-C7AE-E311-B8ED-005056822391 | 10002711    | The Godolphin and Latymer School                                                       |
| 0CD97AF1-C6AE-E311-B8ED-005056822391 | 10015957    | The Summerhill School                                                                  |
| B369DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Darlinghurst School                                                                    |
| 4BD87AF1-C6AE-E311-B8ED-005056822391 | NULL        | Radcliffe High School                                                                  |
| 1B2B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Bede's Catholic Primary School                                                      |
| 95BA41BA-C4AE-E311-B8ED-005056822391 | NULL        | Institute Of Education                                                                 |
| E76FC7B5-C6AE-E311-B8ED-005056822391 | 10069703    | Holmwood First School                                                                  |
| 7CD250A3-C7AE-E311-B8ED-005056822391 | 10016623    | Nightingale School                                                                     |
| 63AF3A27-C7AE-E311-B8ED-005056822391 | 10071702    | St Francesca Cabrini Primary School                                                    |
| 5FCDA8CD-C6AE-E311-B8ED-005056822391 | 10073741    | Summerseat Methodist Primary                                                           |
| D7CEA8CD-C6AE-E311-B8ED-005056822391 | 10077393    | Wormley Primary School                                                                 |
| 8ACEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Patrington Church of England Voluntary Controlled                                      |
| BD71C7B5-C6AE-E311-B8ED-005056822391 | 10075345    | Balksbury Junior School                                                                |
| 1F1B5791-C7AE-E311-B8ED-005056822391 | 10015574    | West London Academy                                                                    |
| EF849443-C7AE-E311-B8ED-005056822391 | 10001397    | Chingford School                                                                       |
| DC13E69D-C6AE-E311-B8ED-005056822391 | 10073145    | Dovecotes Primary School                                                               |
| DEB9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Gladstone Park Primary School                                                          |
| B163F591-C6AE-E311-B8ED-005056822391 | 10070800    | Marlbrook Primary School                                                               |
| 9D7B99D9-C6AE-E311-B8ED-005056822391 | 10070498    | Barton C of E VA Primary School                                                        |
| C0D150A3-C7AE-E311-B8ED-005056822391 | 10017437    | Redwood Park School                                                                    |
| BC1FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Hooe Primary School                                                                    |
| 5375B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Westbridge Primary School                                                              |
| 2CC9BFBB-C6AE-E311-B8ED-005056822391 | 10074587    | Manorfield Primary School                                                              |
| 8BE74C15-C7AE-E311-B8ED-005056822391 | 10016287    | Phoenix School                                                                         |
| 80E54C15-C7AE-E311-B8ED-005056822391 | 10031427    | St Paul's Way Trust School                                                             |
| 16385C09-C7AE-E311-B8ED-005056822391 | 10015056    | Brierton School                                                                        |
| F07B99D9-C6AE-E311-B8ED-005056822391 | 10070172    | St Cuthberts RC Primary School                                                         |
| E5B60486-C6AE-E311-B8ED-005056822391 | NULL        | Whitley Park Infant and Nursery School                                                 |
| 705EDE53-8E8A-E411-A03E-005056822390 | 10047333    | Victoria Academies Teacher Training.                                                   |
| A7ED7F55-C7AE-E311-B8ED-005056822391 | 10015063    | Cliff Park High School                                                                 |
| A13E451B-C7AE-E311-B8ED-005056822391 | NULL        | Spalding Grammar School                                                                |
| 3A876BFD-C6AE-E311-B8ED-005056822391 | NULL        | Farnham Heath End School                                                               |
| 666BDEA3-C6AE-E311-B8ED-005056822391 | 10068766    | Moldgreen Community Primary School                                                     |
| 2C1B5791-C7AE-E311-B8ED-005056822391 | 10008639    | Stockley Academy                                                                       |
| 243173F7-C6AE-E311-B8ED-005056822391 | 10007173    | Uplands Community College                                                              |
| 3E17CFAF-C6AE-E311-B8ED-005056822391 | 10076557    | Ladypool Primary School                                                                |
| 88E74C15-C7AE-E311-B8ED-005056822391 | 10002503    | Fleetwood High School                                                                  |
| DD17CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Hoo St Werburgh Primary School                                                         |
| 74C0D6A9-C6AE-E311-B8ED-005056822391 | 10041473    | Durdans Park Primary School                                                            |
| 2B8D540F-C7AE-E311-B8ED-005056822391 | 10006794    | Rawlett High School                                                                    |
| F710E69D-C6AE-E311-B8ED-005056822391 | 10078421    | Birdwell Primary School                                                                |
| 92C5BFBB-C6AE-E311-B8ED-005056822391 | 10069356    | Saltburn Primary School                                                                |
| 565D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Woodlands Park Nursery Centre                                                          |
| E3866BFD-C6AE-E311-B8ED-005056822391 | 10013296    | Caistor Yarborough School                                                              |
| 32E64C15-C7AE-E311-B8ED-005056822391 | 10015480    | Woodlands Community School                                                             |
| FA986F6D-C7AE-E311-B8ED-005056822391 | 10017012    | The Endeavour School                                                                   |
| 918282EB-C6AE-E311-B8ED-005056822391 | 10002202    | Egglescliffe School                                                                    |
| FF75B0C7-C6AE-E311-B8ED-005056822391 | 10078057    | Eleanor Palmer School                                                                  |
| D43273F7-C6AE-E311-B8ED-005056822391 | NULL        | Barden High School                                                                     |
| 3569DEA3-C6AE-E311-B8ED-005056822391 | 10081353    | Ham Dingle Primary                                                                     |
| 7A0D6579-C7AE-E311-B8ED-005056822391 | 10004447    | The Mount School                                                                       |
| BDE06303-C7AE-E311-B8ED-005056822391 | 10001494    | Claydon High School                                                                    |
| 06C0D6A9-C6AE-E311-B8ED-005056822391 | 10075071    | Swindon Primary School                                                                 |
| 9EC72DD3-C7AE-E311-B8ED-005056822391 | NULL        | The University of Newcastle-upon-Tyne                                                  |
| 54CEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Long Bennington                                                                        |
| 030BFD8B-C6AE-E311-B8ED-005056822391 | 10073304    | Lancaster Road Primary School                                                          |
| C48082EB-C6AE-E311-B8ED-005056822391 | NULL        | St Thomas Moore                                                                        |
| F4BFD6A9-C6AE-E311-B8ED-005056822391 | 10075388    | Buryfields Infant School                                                               |
| FFB30486-C6AE-E311-B8ED-005056822391 | 10076679    | Eldene Primary School                                                                  |
| 9A1B5791-C7AE-E311-B8ED-005056822391 | 10017876    | Marlborough School                                                                     |
| BF886BFD-C6AE-E311-B8ED-005056822391 | 10017619    | Spen Valley High School                                                                |
| BB2F73F7-C6AE-E311-B8ED-005056822391 | 10000349    | Archway School                                                                         |
| 986EC7B5-C6AE-E311-B8ED-005056822391 | 10070085    | Cheetwood Primary School                                                               |
| 0B8E540F-C7AE-E311-B8ED-005056822391 | 10010920    | John Flamsteed Community School                                                        |
| 9611E69D-C6AE-E311-B8ED-005056822391 | NULL        | Hamstel Infant School                                                                  |
| 5696874F-C7AE-E311-B8ED-005056822391 | 10003069    | Highcliffe School                                                                      |
| 553F8F49-C7AE-E311-B8ED-005056822391 | 10001937    | Desborough School                                                                      |
| 91C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Shireland Hall Primary School                                                          |
| 5413E69D-C6AE-E311-B8ED-005056822391 | 10074774    | Kingham Primary School                                                                 |
| 5AAF3A27-C7AE-E311-B8ED-005056822391 | 10069600    | Hawksmoor GM Primary School                                                            |
| 2219CFAF-C6AE-E311-B8ED-005056822391 | 10078774    | Westwood First School                                                                  |
| 5617CFAF-C6AE-E311-B8ED-005056822391 | 10074075    | Luton Infant School                                                                    |
| 1E1EB8C1-C6AE-E311-B8ED-005056822391 | 10066706    | Stillness Infant School                                                                |
| C71A5791-C7AE-E311-B8ED-005056822391 | 10080498    | St George's College Junior School                                                      |
| 7890540F-C7AE-E311-B8ED-005056822391 | NULL        | Halton High School                                                                     |
| AD3F8F49-C7AE-E311-B8ED-005056822391 | 10006615    | The Brooksbank School                                                                  |
| 5CC7BFBB-C6AE-E311-B8ED-005056822391 | 10040413    | Ireland Wood Primary School                                                            |
| B00CFD8B-C6AE-E311-B8ED-005056822391 | 10046467    | Sitwell Junior School                                                                  |
| 75E64C15-C7AE-E311-B8ED-005056822391 | 10007490    | Whitehaven School                                                                      |
| 8817CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Horbury Primary                                                                        |
| B5536A73-C7AE-E311-B8ED-005056822391 | NULL        | Reading Bluecoat School                                                                |
| 5C1DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Merlin Primary School                                                                  |
| 72D55197-C7AE-E311-B8ED-005056822391 | 10016499    | Newick House School                                                                    |
| 570A6485-C7AE-E311-B8ED-005056822391 | 10033875    | Ilford Preparatory School                                                              |
| C268DEA3-C6AE-E311-B8ED-005056822391 | 10069186    | Stockingford Infant School                                                             |
| 2176B0C7-C6AE-E311-B8ED-005056822391 | 10078038    | Linton Mead Primary School                                                             |
| 693073F7-C6AE-E311-B8ED-005056822391 | 10015687    | Elthorne Park High School                                                              |
| 516ADEA3-C6AE-E311-B8ED-005056822391 | 10074084    | Bushfield Road Infant School                                                           |
| 9D8D540F-C7AE-E311-B8ED-005056822391 | 10015484    | Cranbourne School                                                                      |
| 4E19CFAF-C6AE-E311-B8ED-005056822391 | 10068832    | Lindens Primary                                                                        |
| 1D23A1D3-C6AE-E311-B8ED-005056822391 | 10077408    | Wadhurst C of E Primary School                                                         |
| 366EC7B5-C6AE-E311-B8ED-005056822391 | 10077316    | Bramingham Primary School                                                              |
| 0397874F-C7AE-E311-B8ED-005056822391 | 10003488    | John Kelly Boys Technology College                                                     |
| BD996F6D-C7AE-E311-B8ED-005056822391 | 10006133    | St Catherine's School                                                                  |
| EC430124-1BB4-E911-A959-000D3A2AAD25 | 10083735    | Cambrai Primary School                                                                 |
| 7C6EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Newark Hill Primary School                                                             |
| EE25A1D3-C6AE-E311-B8ED-005056822391 | 10071564    | St John the Baptist CE Primary School                                                  |
| 0BDA7AF1-C6AE-E311-B8ED-005056822391 | 10001670    | Conyers School                                                                         |
| 7CC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Broad Street County Infants                                                            |
| 567C99D9-C6AE-E311-B8ED-005056822391 | 10078875    | Medstead Primary School                                                                |
| 9918CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Zetland Primary School                                                                 |
| AFBBED97-C6AE-E311-B8ED-005056822391 | NULL        | Red Lane County Primary School                                                         |
| 298482EB-C6AE-E311-B8ED-005056822391 | 10015806    | Hodge Hill Girls School                                                                |
| FDF63F21-C7AE-E311-B8ED-005056822391 | 10006599    | The Bishop Wand Church of England Secondary School                                     |
| 7468DEA3-C6AE-E311-B8ED-005056822391 | NULL        | St James Infant School                                                                 |
| C5996F6D-C7AE-E311-B8ED-005056822391 | 10014066    | Bridgewater School                                                                     |
| 231DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Barton Village First School                                                            |
| C93B451B-C7AE-E311-B8ED-005056822391 | 10017624    | The Byrchall High School                                                               |
| 92B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Whyteleafe School                                                                      |
| 7A7A99D9-C6AE-E311-B8ED-005056822391 | 10077915    | Blewbury CE Endowed Primary School                                                     |
| 47385C09-C7AE-E311-B8ED-005056822391 | 10007986    | Portchester Community School                                                           |
| 92C9BFBB-C6AE-E311-B8ED-005056822391 | NULL        | St Margaret's Infant                                                                   |
| C3866BFD-C6AE-E311-B8ED-005056822391 | 10005315    | Quarrendon Upper School                                                                |
| 36E06303-C7AE-E311-B8ED-005056822391 | 10006346    | Stoke High School                                                                      |
| D12B8AE5-C6AE-E311-B8ED-005056822391 | 10079305    | St Mary's C/E Primary School                                                           |
| 557D99D9-C6AE-E311-B8ED-005056822391 | 10075466    | St Andrews Primary Nuthurst                                                            |
| 0B25A1D3-C6AE-E311-B8ED-005056822391 | 10078862    | Oakham CE Primary School                                                               |
| 877D99D9-C6AE-E311-B8ED-005056822391 | 10075708    | St Mary's Amresham C of E School                                                       |
| A0ED667F-C7AE-E311-B8ED-005056822391 | 10007580    | Wolverhampton Grammar School                                                           |
| 57FE7261-C7AE-E311-B8ED-005056822391 | 10006611    | The Bradbourne School for Girls                                                        |
| A6C5BFBB-C6AE-E311-B8ED-005056822391 | 10081296    | Hilton Primary School                                                                  |
| 0A615C8B-C7AE-E311-B8ED-005056822391 | 10017995    | Clarence House School                                                                  |
| EA17CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Beechwood Junior School                                                                |
| 8E19CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Roskear School                                                                         |
| 3BAF3A27-C7AE-E311-B8ED-005056822391 | 10001721    | Coventry Blue Coat C of E School                                                       |
| FCD45197-C7AE-E311-B8ED-005056822391 | NULL        | Tyldesley Highfield School                                                             |
| 5C3E8F49-C7AE-E311-B8ED-005056822391 | 10003463    | Jeff Joseph Sale Moor Technology College                                               |
| B8298AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Bernadette's RC First and Middle School                                             |
| 5F17CFAF-C6AE-E311-B8ED-005056822391 | 10079771    | Manor Field Primary School                                                             |
| 04C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Hadrian Lower                                                                          |
| 15886BFD-C6AE-E311-B8ED-005056822391 | 10004987    | Parklands Girls' High School                                                           |
| B3996F6D-C7AE-E311-B8ED-005056822391 | 10017456    | Sidcot School                                                                          |
| C7876BFD-C6AE-E311-B8ED-005056822391 | 10003086    | Hillcrestschool                                                                        |
| 3E6ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Shafton Primary School                                                                 |
| 3676B0C7-C6AE-E311-B8ED-005056822391 | 10078019    | Holmleigh Primary School                                                               |
| 2E5E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Maple Tree Lower School                                                                |
| BE849443-C7AE-E311-B8ED-005056822391 | 10007990    | Queen Elizabeths Boys School                                                           |
| 437A589D-C7AE-E311-B8ED-005056822391 | 10015902    | High Furlong School                                                                    |
| EC849443-C7AE-E311-B8ED-005056822391 | 10004651    | Nonsuch High School                                                                    |
| 22896BFD-C6AE-E311-B8ED-005056822391 | 10016304    | Kings Heath Boys' School                                                               |
| 72876BFD-C6AE-E311-B8ED-005056822391 | 10005796    | Shelfield Community School                                                             |
| F0E44C15-C7AE-E311-B8ED-005056822391 | 10014922    | Abbeydale Grange School                                                                |
| B910E69D-C6AE-E311-B8ED-005056822391 | NULL        | Scott Wilkie Primary School                                                            |
| 6C3273F7-C6AE-E311-B8ED-005056822391 | NULL        | Fulbrook Middle School                                                                 |
| CBE74C15-C7AE-E311-B8ED-005056822391 | 10017912    | Christopher Whitehead High School                                                      |
| DF615C8B-C7AE-E311-B8ED-005056822391 | 10032843    | Eaton Square                                                                           |
| 118182EB-C6AE-E311-B8ED-005056822391 | NULL        | Bury St Edmunds County Upper School                                                    |
| F0E06303-C7AE-E311-B8ED-005056822391 | NULL        | Mereway Middle School                                                                  |
| 17A77A5B-C7AE-E311-B8ED-005056822391 | 10001355    | Cheltenham Bournside School                                                            |
| A05E0C80-C6AE-E311-B8ED-005056822391 | 10070791    | Ashwell Primary School                                                                 |
| 5B1EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Rokeby Primary School                                                                  |
| 9B1A7067-C7AE-E311-B8ED-005056822391 | 10001345    | Cecil Jones High School                                                                |
| 95B60486-C6AE-E311-B8ED-005056822391 | 10076613    | Glade Primary School                                                                   |
| 7AC8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Werrington Primary School                                                              |
| 26F83F21-C7AE-E311-B8ED-005056822391 | NULL        | Stalbridge Primary School                                                              |
| 20DE6303-C7AE-E311-B8ED-005056822391 | 10017359    | Sandhill View School                                                                   |
| DD7C99D9-C6AE-E311-B8ED-005056822391 | 10069048    | Aughton Sy Michaels C of E Primary School                                              |
| FD17CFAF-C6AE-E311-B8ED-005056822391 | 10069971    | Castlefort Junior Mixed and Infant School                                              |
| 893D451B-C7AE-E311-B8ED-005056822391 | 10006718    | John Roan                                                                              |
| 57996F6D-C7AE-E311-B8ED-005056822391 | 10008116    | Bradfield College                                                                      |
| 5F24A1D3-C6AE-E311-B8ED-005056822391 | 10070344    | Gunness & Burringham Primary School                                                    |
| 1B3C451B-C7AE-E311-B8ED-005056822391 | 10001871    | Davison CE Girls High School                                                           |
| 028482EB-C6AE-E311-B8ED-005056822391 | 10007860    | West Exe Technology College                                                            |
| E31EB8C1-C6AE-E311-B8ED-005056822391 | 10071958    | Torridon Primary School                                                                |
| ED0DFD8B-C6AE-E311-B8ED-005056822391 | 10075059    | Moss Hall Junior School                                                                |
| 5F3F8F49-C7AE-E311-B8ED-005056822391 | 10004730    | Northhampton School for Boys                                                           |
| DA355C09-C7AE-E311-B8ED-005056822391 | 10005975    | South Dartmoor Community College                                                       |
| 821FB8C1-C6AE-E311-B8ED-005056822391 | 10076433    | John Ray Infant School                                                                 |
| 98C9BFBB-C6AE-E311-B8ED-005056822391 | 10076685    | Anglesey Primary                                                                       |
| F91FB8C1-C6AE-E311-B8ED-005056822391 | 10074321    | Sedbergh Primary School                                                                |
| 528E540F-C7AE-E311-B8ED-005056822391 | 10002201    | Eggbuckland Community College                                                          |
| 8224A1D3-C6AE-E311-B8ED-005056822391 | NULL        | St Paul's Church of England Primary School                                             |
| E4F73F21-C7AE-E311-B8ED-005056822391 | 10001166    | Cardinal Pole RC School                                                                |
| 2190540F-C7AE-E311-B8ED-005056822391 | 10014958    | Accrington Moorhead High School                                                        |
| A7BAED97-C6AE-E311-B8ED-005056822391 | 10075237    | Wessex Gardens Primary School                                                          |
| 61886BFD-C6AE-E311-B8ED-005056822391 | 10004107    | Lordswood Girls' School and The Sixth Form Centre,                                     |
| 671B7067-C7AE-E311-B8ED-005056822391 | 10008513    | St Johns College                                                                       |
| 96ED7F55-C7AE-E311-B8ED-005056822391 | 10016519    | Parkview Community College of Technology                                               |
| 7E6FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Catton Grove First School                                                              |
| FAD191DF-C6AE-E311-B8ED-005056822391 | 10074217    | Sir Robert Hitcham's CE VA Primary                                                     |
| 3F5E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Belgrave Church of England Primary School                                              |
| BD1DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Rokeby Junior School                                                                   |
| E868DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Wyndcliffe Junior Community School                                                     |
| 583273F7-C6AE-E311-B8ED-005056822391 | 10000817    | Boundstone Community College                                                           |
| 8C7A589D-C7AE-E311-B8ED-005056822391 | NULL        | Leyland School                                                                         |
| 868282EB-C6AE-E311-B8ED-005056822391 | 10003107    | Hitchin Boy's School                                                                   |
| 801B5791-C7AE-E311-B8ED-005056822391 | 10016333    | Kingsdown School                                                                       |
| 961ACFAF-C6AE-E311-B8ED-005056822391 | NULL        | Northfield Manor Junior & Infant School                                                |
| B4876BFD-C6AE-E311-B8ED-005056822391 | NULL        | Tamworth Manor High School                                                             |
| 1B14E69D-C6AE-E311-B8ED-005056822391 | NULL        | Weston Coyney Junior School                                                            |
| DBDF6303-C7AE-E311-B8ED-005056822391 | 10005417    | Redhill School                                                                         |
| C12F73F7-C6AE-E311-B8ED-005056822391 | 10017727    | The Cooper School                                                                      |
| EB365C09-C7AE-E311-B8ED-005056822391 | 10005002    | Patchway Community College                                                             |
| 89896BFD-C6AE-E311-B8ED-005056822391 | 10002771    | Greenhead High School                                                                  |
| 00CC372D-C7AE-E311-B8ED-005056822391 | 10000739    | Bishopshalt School                                                                     |
| 1F896BFD-C6AE-E311-B8ED-005056822391 | 10015752    | Heathcote School                                                                       |
| 625F0C80-C6AE-E311-B8ED-005056822391 | NULL        | Hawes Down Infant School                                                               |
| A870C7B5-C6AE-E311-B8ED-005056822391 | NULL        | New Kings Primary School                                                               |
| E78C540F-C7AE-E311-B8ED-005056822391 | 10015976    | Tibshelf Community School                                                              |
| 556FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Stevenson Junior School                                                                |
| 6871C7B5-C6AE-E311-B8ED-005056822391 | 10079554    | Dundale Primary School                                                                 |
| 5DD591DF-C6AE-E311-B8ED-005056822391 | 10075805    | St Joseph's Catholic Primary School                                                    |
| 5E20B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Meon Junior                                                                            |
| 54FF7261-C7AE-E311-B8ED-005056822391 | 10006700    | The Howard School                                                                      |
| 7FEE667F-C7AE-E311-B8ED-005056822391 | 10006105    | St Albans High School for Girls                                                        |
| 50365C09-C7AE-E311-B8ED-005056822391 | 10007612    | Woodkirk Academy                                                                       |
| 2BE64C15-C7AE-E311-B8ED-005056822391 | NULL        | Walton High                                                                            |
| 65B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Lynch Hill School Primary Academy                                                      |
| 6170C7B5-C6AE-E311-B8ED-005056822391 | NULL        | James Watt Junior School                                                               |
| 08886BFD-C6AE-E311-B8ED-005056822391 | 10015180    | Broadwater School                                                                      |
| 3FEE667F-C7AE-E311-B8ED-005056822391 | 10015396    | Cheltenham College                                                                     |
| BA8182EB-C6AE-E311-B8ED-005056822391 | 10017462    | Priory School and Sports College                                                       |
| 31C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Armthorpe Southfield Primary School                                                    |
| 8EF53F21-C7AE-E311-B8ED-005056822391 | 10001162    | Cardinal Langley RC High School                                                        |
| F595874F-C7AE-E311-B8ED-005056822391 | 10003499    | John Port School                                                                       |
| 00F83F21-C7AE-E311-B8ED-005056822391 | 10005637    | Saint Paul's Catholic School                                                           |
| FE996F6D-C7AE-E311-B8ED-005056822391 | 10035869    | St Peter's School                                                                      |
| 3DC7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Middle Row Primary School                                                              |
| E0D191DF-C6AE-E311-B8ED-005056822391 | 10072464    | St Andrew's C of E (Aided) Primary School                                              |
| 45408F49-C7AE-E311-B8ED-005056822391 | 10015971    | Hurstmere School                                                                       |
| EB7D99D9-C6AE-E311-B8ED-005056822391 | 10069830    | St Michaels CE Primary School                                                          |
| 71ED667F-C7AE-E311-B8ED-005056822391 | 10018023    | Rossendale School                                                                      |
| B5E64C15-C7AE-E311-B8ED-005056822391 | 10003211    | Hurlingham and Chelsea School                                                          |
| 54E84C15-C7AE-E311-B8ED-005056822391 | 10007415    | West Gate Community College                                                            |
| 16E84C15-C7AE-E311-B8ED-005056822391 | NULL        | Sunbury Manor School                                                                   |
| 1A11E69D-C6AE-E311-B8ED-005056822391 | 10079987    | Ellen Wilkinson Primary School                                                         |
| 529A6F6D-C7AE-E311-B8ED-005056822391 | 10008297    | James Allen's Girls' School                                                            |
| 64EE667F-C7AE-E311-B8ED-005056822391 | 10017612    | The Cheltenham Ladies College                                                          |
| 4C11E69D-C6AE-E311-B8ED-005056822391 | NULL        | East Herrington Primary Academy                                                        |
| 5B7D99D9-C6AE-E311-B8ED-005056822391 | 10078681    | St Mary's Catholic Primary School, Edlington                                           |
| E56EC7B5-C6AE-E311-B8ED-005056822391 | 10076177    | Montrose Primary School                                                                |
| 37A87A5B-C7AE-E311-B8ED-005056822391 | 10006581    | The Archbishop's School                                                                |
| A0D87AF1-C6AE-E311-B8ED-005056822391 | 10016232    | Longhill School                                                                        |
| A61B5791-C7AE-E311-B8ED-005056822391 | 10015226    | Castle School                                                                          |
| 6E0BFD8B-C6AE-E311-B8ED-005056822391 | 10072893    | Livingtonstone School                                                                  |
| 09E84C15-C7AE-E311-B8ED-005056822391 | 10017467    | Preston School                                                                         |
| 7A615C8B-C7AE-E311-B8ED-005056822391 | 10008257    | Haberdashers' Aske's School for Girls                                                  |
| 1665F591-C6AE-E311-B8ED-005056822391 | 10076599    | Barley Lane Primary School                                                             |
| 8C18CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Woodhouse Primary School                                                               |
| BFDF6303-C7AE-E311-B8ED-005056822391 | 10015809    | Heywood Community High School                                                          |
| 20E54C15-C7AE-E311-B8ED-005056822391 | 10015629    | Woolston School                                                                        |
| B06EC7B5-C6AE-E311-B8ED-005056822391 | 10080391    | Wilkes Green Infant School                                                             |
| 2620B8C1-C6AE-E311-B8ED-005056822391 | 10076951    | Berrywood Primary School                                                               |
| B1BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Rochdale Road Junior School                                                            |
| 94D291DF-C6AE-E311-B8ED-005056822391 | 10076881    | St Pancras RC Primary School                                                           |
| 62DB7AF1-C6AE-E311-B8ED-005056822391 | 10017336    | Rooks Heath High School                                                                |
| 9ADF6303-C7AE-E311-B8ED-005056822391 | NULL        | Harlington Upper School                                                                |
| 0B6ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Parc Eglos Primary                                                                     |
| E8DF6303-C7AE-E311-B8ED-005056822391 | 10017249    | Balderstone Community High School                                                      |
| FA2B8AE5-C6AE-E311-B8ED-005056822391 | 10073427    | St Andrews CE Primary School                                                           |
| 78BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Ivy Bank CP School                                                                     |
| 168F540F-C7AE-E311-B8ED-005056822391 | NULL        | Village Community School                                                               |
| D08382EB-C6AE-E311-B8ED-005056822391 | 10017375    | Ribblesdale High School                                                                |
| 5A876BFD-C6AE-E311-B8ED-005056822391 | NULL        | Parkfields Middle School                                                               |
| 150EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Langley Primary School                                                                 |
| 2AC6BFBB-C6AE-E311-B8ED-005056822391 | 10069573    | Castle Primary School                                                                  |
| 71859443-C7AE-E311-B8ED-005056822391 | 10007988    | Queen Elizabeths Grammar School                                                        |
| 76E06303-C7AE-E311-B8ED-005056822391 | NULL        | Streetfield Middle School                                                              |
| D01B5791-C7AE-E311-B8ED-005056822391 | 10015079    | Cross Hill School                                                                      |
| C4536A73-C7AE-E311-B8ED-005056822391 | 10008129    | Bromsgrove School                                                                      |
| 721DB8C1-C6AE-E311-B8ED-005056822391 | 10069267    | Watlington Primary School                                                              |
| 3862F591-C6AE-E311-B8ED-005056822391 | 10072614    | Barugh Green Primary School                                                            |
| 2B1DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Treisker County Primary School                                                         |
| 67BCED97-C6AE-E311-B8ED-005056822391 | NULL        | Castlefields Infant School                                                             |
| B7849443-C7AE-E311-B8ED-005056822391 | 10014777    | Bishop Thomas Grant School                                                             |
| 451DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | The Echelford Primary School                                                           |
| 7C3273F7-C6AE-E311-B8ED-005056822391 | 10007494    | Coventry Secondary School                                                              |
| 7A68DEA3-C6AE-E311-B8ED-005056822391 | 10072126    | Somerhill Junior School                                                                |
| 4D74B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Hare Street Infant School                                                              |
| 6F18CFAF-C6AE-E311-B8ED-005056822391 | NULL        | The Ferrars Academy                                                                    |
| 90849443-C7AE-E311-B8ED-005056822391 | 10003685    | Kirkbie Kendal School                                                                  |
| 8164F591-C6AE-E311-B8ED-005056822391 | 10080057    | Welbourne Primary School                                                               |
| 53408F49-C7AE-E311-B8ED-005056822391 | 10015973    | John Paul Ii School                                                                    |
| 23408F49-C7AE-E311-B8ED-005056822391 | 10002258    | Enfield Grammar School                                                                 |
| 7717CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Thomas Harding Junior School                                                           |
| A3B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Halfway Houses Primary School                                                          |
| 592F73F7-C6AE-E311-B8ED-005056822391 | 10002180    | Sir Thomas Wharton Community College                                                   |
| 34F63F21-C7AE-E311-B8ED-005056822391 | NULL        | St Georges CE (Aided) Middle School                                                    |
| D595874F-C7AE-E311-B8ED-005056822391 | 10007519    | Wilmington Grammer School for Boys                                                     |
| B12A8AE5-C6AE-E311-B8ED-005056822391 | 10080466    | Clore Tikvah Primary School                                                            |
| 9C886BFD-C6AE-E311-B8ED-005056822391 | 10007676    | Wymondham High School                                                                  |
| AEEE667F-C7AE-E311-B8ED-005056822391 | NULL        | Quinton House School                                                                   |
| EB1A7067-C7AE-E311-B8ED-005056822391 | 10073752    | Mathilda Marks Kennedy                                                                 |
| 13EE667F-C7AE-E311-B8ED-005056822391 | 10014842    | Belmont School                                                                         |
| FBDB7AF1-C6AE-E311-B8ED-005056822391 | 10007449    | Westhoughton High School                                                               |
| 9CF73F21-C7AE-E311-B8ED-005056822391 | NULL        | Our Lady and Pope John Catholic School                                                 |
| 9E8382EB-C6AE-E311-B8ED-005056822391 | 10017108    | Teddington School                                                                      |
| 282B8AE5-C6AE-E311-B8ED-005056822391 | 10075986    | St Mary's RC School                                                                    |
| 49CDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Carlton Lower School                                                                   |
| F075B0C7-C6AE-E311-B8ED-005056822391 | 10078216    | Longsands Community Primary School                                                     |
| E020B8C1-C6AE-E311-B8ED-005056822391 | 10071316    | Sheringdale Primary School                                                             |
| CAED7F55-C7AE-E311-B8ED-005056822391 | 10006780    | The Pingle School                                                                      |
| E1DB7AF1-C6AE-E311-B8ED-005056822391 | 10003247    | Ilfield Community College                                                              |
| 18EE7F55-C7AE-E311-B8ED-005056822391 | NULL        | Flegg High School                                                                      |
| B3DA7AF1-C6AE-E311-B8ED-005056822391 | 10000591    | Beal High School                                                                       |
| C9BAED97-C6AE-E311-B8ED-005056822391 | NULL        | Peacefield Primary School                                                              |
| EAB30486-C6AE-E311-B8ED-005056822391 | 10069174    | The Downley School                                                                     |
| F18182EB-C6AE-E311-B8ED-005056822391 | 10015425    | Winterhill School                                                                      |
| 467D99D9-C6AE-E311-B8ED-005056822391 | 10073759    | St Mark's C of E Primary School                                                        |
| D38F540F-C7AE-E311-B8ED-005056822391 | 10000700    | Birchwood High School                                                                  |
| 3296874F-C7AE-E311-B8ED-005056822391 | 10006171    | St Gregory's RC High School                                                            |
| 9F8F540F-C7AE-E311-B8ED-005056822391 | 10005970    | South Camden Community School                                                          |
| CF7D99D9-C6AE-E311-B8ED-005056822391 | 10079235    | Forncett V.A. Primary                                                                  |
| 7C8D540F-C7AE-E311-B8ED-005056822391 | 10014908    | Bishop Barrington School                                                               |
| C72B8AE5-C6AE-E311-B8ED-005056822391 | 10071683    | St Vincent's RC Primary School                                                         |
| D86EC7B5-C6AE-E311-B8ED-005056822391 | 10076178    | Wyvern Primary School                                                                  |
| 3A3173F7-C6AE-E311-B8ED-005056822391 | 10007918    | Cottenham Village College                                                              |
| C313E69D-C6AE-E311-B8ED-005056822391 | 10070893    | Manorfield Infant and Nursery School                                                   |
| 6B20B8C1-C6AE-E311-B8ED-005056822391 | NULL        | St Cleer Primary School                                                                |
| CF876BFD-C6AE-E311-B8ED-005056822391 | 10016664    | Montgomery High School                                                                 |
| 55E06303-C7AE-E311-B8ED-005056822391 | NULL        | Sir Wilfred Martineau School                                                           |
| B320B8C1-C6AE-E311-B8ED-005056822391 | 10073386    | Hornsea Primary School                                                                 |
| BD8382EB-C6AE-E311-B8ED-005056822391 | 10006669    | The George Ward School                                                                 |
| D5CDA8CD-C6AE-E311-B8ED-005056822391 | 10074684    | St Lukes CE Primary School                                                             |
| AD7A99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Mark's Church of England Primary School                                             |
| 35B50486-C6AE-E311-B8ED-005056822391 | NULL        | Roe Lee Park Primary School                                                            |
| 1D70C7B5-C6AE-E311-B8ED-005056822391 | 10075110    | Bispham Drive Junior School                                                            |
| EB3173F7-C6AE-E311-B8ED-005056822391 | 10002628    | Garibaldi School                                                                       |
| 51E64C15-C7AE-E311-B8ED-005056822391 | 10007126    | Ullswater Community College                                                            |
| 3311E69D-C6AE-E311-B8ED-005056822391 | 10068964    | Francis Baily Primary School                                                           |
| 8A12E69D-C6AE-E311-B8ED-005056822391 | 10068834    | Bentley West Primary School                                                            |
| 3AA77A5B-C7AE-E311-B8ED-005056822391 | 10001226    | Castledown School                                                                      |
| 6F75B0C7-C6AE-E311-B8ED-005056822391 | 10069282    | Claremont Primary School                                                               |
| 3570C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Stramongate Primary School                                                             |
| ADB60486-C6AE-E311-B8ED-005056822391 | NULL        | Seedley Primary School                                                                 |
| 261B7067-C7AE-E311-B8ED-005056822391 | NULL        | St John's Foundation Special School                                                    |
| 71AF3A27-C7AE-E311-B8ED-005056822391 | NULL        | Oakington Manor Primary School                                                         |
| 5F71C7B5-C6AE-E311-B8ED-005056822391 | NULL        | The Hermitage School                                                                   |
| 2ECEA8CD-C6AE-E311-B8ED-005056822391 | 10070351    | Tickton C of E Voluntary Controlled Primary School                                     |
| B08282EB-C6AE-E311-B8ED-005056822391 | 10016078    | Huntcliff School                                                                       |
| C9CB372D-C7AE-E311-B8ED-005056822391 | 10005371    | Raine's Foundation School                                                              |
| 388382EB-C6AE-E311-B8ED-005056822391 | 10017731    | Skerton Community High School                                                          |
| EDA77A5B-C7AE-E311-B8ED-005056822391 | 10017015    | St George Catholic School for Boys                                                     |
| 3AE74C15-C7AE-E311-B8ED-005056822391 | 10017028    | The Lakelands School                                                                   |
| D2B30486-C6AE-E311-B8ED-005056822391 | 10078941    | Burton Road Primary School                                                             |
| B21EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Wandle Primary School                                                                  |
| 7212E69D-C6AE-E311-B8ED-005056822391 | 10074776    | Charlbury Primary School                                                               |
| 35CEA8CD-C6AE-E311-B8ED-005056822391 | 10068548    | St Paul's Peel CE School                                                               |
| 6364F591-C6AE-E311-B8ED-005056822391 | NULL        | Houghton Conquest Lower                                                                |
| 5AD291DF-C6AE-E311-B8ED-005056822391 | 10071006    | St Peter & St Paul CE Primary School                                                   |
| DB90540F-C7AE-E311-B8ED-005056822391 | 10000943    | The Broadway School                                                                    |
| 6B0A6485-C7AE-E311-B8ED-005056822391 | 10003675    | Kingston Grammer School                                                                |
| 6B1DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Kanes Hill Primary                                                                     |
| 31D87AF1-C6AE-E311-B8ED-005056822391 | 10015487    | Dorothy Stringer High School                                                           |
| CC2C8AE5-C6AE-E311-B8ED-005056822391 | 10079651    | All Saints C of E Infant School                                                        |
| 92DA7AF1-C6AE-E311-B8ED-005056822391 | 10013322    | George Farmer Technology College                                                       |
| B418CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Denbigh Infant School                                                                  |
| 0419CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Huntington Community Primary School                                                    |
| A36EC7B5-C6AE-E311-B8ED-005056822391 | 10052987    | Haseltine Primary School                                                               |
| 0ABAED97-C6AE-E311-B8ED-005056822391 | 10076693    | Hunters Hall Primary School                                                            |
| C02B8AE5-C6AE-E311-B8ED-005056822391 | 10078353    | St Paul's and St Timothy's Catholic Infant School                                      |
| FD23A1D3-C6AE-E311-B8ED-005056822391 | 10073841    | St Mary's CE Primary                                                                   |
| 84996F6D-C7AE-E311-B8ED-005056822391 | 10014811    | Abbot's Hill School                                                                    |
| 8DFF7261-C7AE-E311-B8ED-005056822391 | 10014806    | Alderman Blaxhill School                                                               |
| 02E84C15-C7AE-E311-B8ED-005056822391 | 10006013    | South Wolds Comprehensive                                                              |
| 467E99D9-C6AE-E311-B8ED-005056822391 | NULL        | Riseley Lower School                                                                   |
| EAE54C15-C7AE-E311-B8ED-005056822391 | 10002311    | Ernest Bevin College                                                                   |
| A58E540F-C7AE-E311-B8ED-005056822391 | NULL        | Tomlinscote School and Sixth Form College                                              |
| 7320B8C1-C6AE-E311-B8ED-005056822391 | 10079036    | Driffield Junior School                                                                |
| 4868DEA3-C6AE-E311-B8ED-005056822391 | 10074037    | Mundesley First School                                                                 |
| 62C2D6A9-C6AE-E311-B8ED-005056822391 | 10076230    | New Road CP School                                                                     |
| 830A6485-C7AE-E311-B8ED-005056822391 | 10005594    | Rye St Antony School                                                                   |
| 33385C09-C7AE-E311-B8ED-005056822391 | 10005376    | Ralph Allen School                                                                     |
| 64C1D6A9-C6AE-E311-B8ED-005056822391 | 10074039    | Eastfield Infant & Nursery School                                                      |
| 3923A1D3-C6AE-E311-B8ED-005056822391 | 10079281    | St Thomas CE Primary School                                                            |
| DBCBA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Bradley Church of England Voluntary Controlled Junior School                           |
| 5F3073F7-C6AE-E311-B8ED-005056822391 | 10015086    | Daventry William Parker School                                                         |
| 4513E69D-C6AE-E311-B8ED-005056822391 | 10076153    | Grindleford Primary School                                                             |
| 8FE16303-C7AE-E311-B8ED-005056822391 | 10005565    | Royds School Specialist Language College                                               |
| 6018CFAF-C6AE-E311-B8ED-005056822391 | NULL        | East Hunsbury Lower School                                                             |
| 2E876BFD-C6AE-E311-B8ED-005056822391 | 10000449    | Attleborough High School                                                               |
| 06D97AF1-C6AE-E311-B8ED-005056822391 | 10017414    | Shene School                                                                           |
| 5C1FB8C1-C6AE-E311-B8ED-005056822391 | 10072980    | Holland Moor Primary School                                                            |
| 440EFD8B-C6AE-E311-B8ED-005056822391 | 10075641    | Withinfields Primary School                                                            |
| BE0AFD8B-C6AE-E311-B8ED-005056822391 | 10079995    | Grange Primary School                                                                  |
| CAB50486-C6AE-E311-B8ED-005056822391 | NULL        | Pen Green Nursery School                                                               |
| 0C63F591-C6AE-E311-B8ED-005056822391 | NULL        | Churchfield Primary School                                                             |
| 5E1A7067-C7AE-E311-B8ED-005056822391 | 10006113    | St Anselms Catholic School                                                             |
| 563C451B-C7AE-E311-B8ED-005056822391 | 10018823    | Brewood CE Middle School                                                               |
| 9FB50486-C6AE-E311-B8ED-005056822391 | NULL        | Amherst Primary School                                                                 |
| 891A7067-C7AE-E311-B8ED-005056822391 | 10006712    | The John Bramston School                                                               |
| 7E2B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Waverley Abbey School                                                                  |
| E074B0C7-C6AE-E311-B8ED-005056822391 | 10081377    | Spelthorne Infant and Nursery School                                                   |
| F9E06303-C7AE-E311-B8ED-005056822391 | 10001198    | Carlton Bolling College                                                                |
| F9876BFD-C6AE-E311-B8ED-005056822391 | 10003384    | Invicta Grammar School                                                                 |
| C7866BFD-C6AE-E311-B8ED-005056822391 | 10000816    | Bosworth Community College                                                             |
| 59F73F21-C7AE-E311-B8ED-005056822391 | 10017783    | Saint Peter's Roman Catholic Comprehensive School                                      |
| BCF53F21-C7AE-E311-B8ED-005056822391 | 10005611    | Sacred Heart Catholic High School                                                      |
| C6375C09-C7AE-E311-B8ED-005056822391 | 10002155    | Eckington School                                                                       |
| D80BFD8B-C6AE-E311-B8ED-005056822391 | 10069501    | Sandylands Community Primary School                                                    |
| BF876BFD-C6AE-E311-B8ED-005056822391 | 10015659    | Westborough High School                                                                |
| DD3073F7-C6AE-E311-B8ED-005056822391 | 10016886    | Marple Hall School                                                                     |
| 3320B8C1-C6AE-E311-B8ED-005056822391 | NULL        | St Breock Primary                                                                      |
| 78D291DF-C6AE-E311-B8ED-005056822391 | 10069074    | Ashford St Mary's C of E Primary                                                       |
| E72F73F7-C6AE-E311-B8ED-005056822391 | 10004359    | Mildenhall Upper School                                                                |
| 04C2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Huntungton Primary School                                                              |
| 24DF6303-C7AE-E311-B8ED-005056822391 | 10006239    | Ernulf Community School                                                                |
| EA8382EB-C6AE-E311-B8ED-005056822391 | 10015143    | Breeze Hill School                                                                     |
| DE7B99D9-C6AE-E311-B8ED-005056822391 | 10075969    | Travis St Lawrence CofE Primary School                                                 |
| A10BFD8B-C6AE-E311-B8ED-005056822391 | 10071952    | Furze Infants' School                                                                  |
| F21CB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Billesley Primary School                                                               |
| 4484149F-C9AE-E311-B8ED-005056822391 | NULL        | Hexham and Newcastle Diocese Catholic Partnership (South)                              |
| DAC5BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Lewisham Bridge Primary School                                                         |
| B56EC7B5-C6AE-E311-B8ED-005056822391 | 10075364    | Crofton Hammond Infant School                                                          |
| 6D7B99D9-C6AE-E311-B8ED-005056822391 | 10073992    | Holy Trinity C of E Primary School                                                     |
| 48F83F21-C7AE-E311-B8ED-005056822391 | 10069651    | Barmby Moor Church of England Primary School                                           |
| 2AED7F55-C7AE-E311-B8ED-005056822391 | 10000508    | Balcarras School                                                                       |
| BCBAED97-C6AE-E311-B8ED-005056822391 | 10080028    | Frithwood Primary School                                                               |
| 53DF6303-C7AE-E311-B8ED-005056822391 | 10001363    | Chesham High                                                                           |
| 4618CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Lark Rise Lower School                                                                 |
| 81D87AF1-C6AE-E311-B8ED-005056822391 | 10003217    | Huxlow School                                                                          |
| 2F859443-C7AE-E311-B8ED-005056822391 | 10004854    | Oldfield School                                                                        |
| 023273F7-C6AE-E311-B8ED-005056822391 | 10015012    | Addington High School                                                                  |
| 4DC0D6A9-C6AE-E311-B8ED-005056822391 | 10074643    | Downderry Primary School                                                               |
| ECB40486-C6AE-E311-B8ED-005056822391 | NULL        | The Willows Primary School                                                             |
| 43859443-C7AE-E311-B8ED-005056822391 | 10001138    | Cannock Chase High School                                                              |
| 4864F591-C6AE-E311-B8ED-005056822391 | 10071515    | Yeading Junior                                                                         |
| 74D391DF-C6AE-E311-B8ED-005056822391 | 10070228    | St Martins Catholic                                                                    |
| 64D250A3-C7AE-E311-B8ED-005056822391 | NULL        | Churchfield School                                                                     |
| 0F91540F-C7AE-E311-B8ED-005056822391 | 10007678    | Wyndham School                                                                         |
| 99F63F21-C7AE-E311-B8ED-005056822391 | 10006829    | The Thomas Hardye School                                                               |
| 67C7BFBB-C6AE-E311-B8ED-005056822391 | 10077223    | St Clement's High School                                                               |
| A268DEA3-C6AE-E311-B8ED-005056822391 | 10073237    | Sandown Primary School                                                                 |
| 2762F591-C6AE-E311-B8ED-005056822391 | 10071517    | Whitehall Junior School                                                                |
| BD64F591-C6AE-E311-B8ED-005056822391 | 10076422    | Rabbs Farm Primary                                                                     |
| 791B5791-C7AE-E311-B8ED-005056822391 | 10017646    | The Avenue School                                                                      |
| 9F3173F7-C6AE-E311-B8ED-005056822391 | 10006356    | Stourport High School                                                                  |
| 008D540F-C7AE-E311-B8ED-005056822391 | 10016867    | Manor Park Community School                                                            |
| 8BDE6303-C7AE-E311-B8ED-005056822391 | 10018827    | Bungay Middle School                                                                   |
| 65CB372D-C7AE-E311-B8ED-005056822391 | 10073670    | Waltham Holy Cross Infant School                                                       |
| E7E74C15-C7AE-E311-B8ED-005056822391 | 10006437    | Sutherland School                                                                      |
| 3C6ADEA3-C6AE-E311-B8ED-005056822391 | 10070651    | Tollgate Community Junior School                                                       |
| 5ABBED97-C6AE-E311-B8ED-005056822391 | 10069167    | Poverest Primary School                                                                |
| E6298AE5-C6AE-E311-B8ED-005056822391 | 10074027    | St Marks C of E Primary School                                                         |
| 90E74C15-C7AE-E311-B8ED-005056822391 | 10016542    | Moor Park High School                                                                  |
| 2468DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Priestthorpe Primary School                                                            |
| B568DEA3-C6AE-E311-B8ED-005056822391 | 10072551    | The Sir Francis Hill Community School                                                  |
| A9355C09-C7AE-E311-B8ED-005056822391 | 10005205    | Priesthorpe School                                                                     |
| 15ED7F55-C7AE-E311-B8ED-005056822391 | 10015247    | Bradon Forest School                                                                   |
| F51DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | South Moreton School                                                                   |
| E9355C09-C7AE-E311-B8ED-005056822391 | 10014988    | Birkdale High School                                                                   |
| D2BBED97-C6AE-E311-B8ED-005056822391 | 10075239    | Lordship Lane Primary School                                                           |
| A0EE667F-C7AE-E311-B8ED-005056822391 | 10018884    | Forres Sandle Manor                                                                    |
| 453F451B-C7AE-E311-B8ED-005056822391 | 10004918    | Our Lady's RC High School                                                              |
| EA605C8B-C7AE-E311-B8ED-005056822391 | 10018448    | Millfield Preparatory School                                                           |
| C865F591-C6AE-E311-B8ED-005056822391 | 10071913    | Whitehall Infant & Nursery School                                                      |
| 870EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Hamilton Infants                                                                       |
| 6C0CFD8B-C6AE-E311-B8ED-005056822391 | 10076611    | Newbury Park Primary School                                                            |
| 840CFD8B-C6AE-E311-B8ED-005056822391 | 10070025    | Galliard Primary School                                                                |
| 850C6579-C7AE-E311-B8ED-005056822391 | 10017556    | Perrott Hill School Trust                                                              |
| 326EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Arden Grove First School                                                               |
| 3C8A6BFD-C6AE-E311-B8ED-005056822391 | 10016082    | The Stonehenge                                                                         |
| EB0BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Harry Dack Infant School                                                               |
| 10DA7AF1-C6AE-E311-B8ED-005056822391 | 10015754    | Edensor High                                                                           |
| 400D6579-C7AE-E311-B8ED-005056822391 | 10008315    | King Henry VIII School                                                                 |
| B90BFD8B-C6AE-E311-B8ED-005056822391 | 10080061    | Coldfall Primary School                                                                |
| E5886BFD-C6AE-E311-B8ED-005056822391 | 10000686    | Bideford College                                                                       |
| 2FD250A3-C7AE-E311-B8ED-005056822391 | 10015607    | West Oaks School                                                                       |
| 3DD250A3-C7AE-E311-B8ED-005056822391 | 10015163    | Brookfield School                                                                      |
| CDCDA8CD-C6AE-E311-B8ED-005056822391 | 10070333    | Bidborough CE Primary School                                                           |
| 8D5C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Hampden Way Nursery School                                                             |
| ACC1D6A9-C6AE-E311-B8ED-005056822391 | 10080076    | Oaklands Primary                                                                       |
| 3BD591DF-C6AE-E311-B8ED-005056822391 | NULL        | St Benedict's Catholic Primary School                                                  |
| 741B7067-C7AE-E311-B8ED-005056822391 | NULL        | The Atherley School                                                                    |
| 2C3C451B-C7AE-E311-B8ED-005056822391 | 10005848    | The Canterbury High School                                                             |
| 81365C09-C7AE-E311-B8ED-005056822391 | 10000815    | Boston Spa School                                                                      |
| DCD87AF1-C6AE-E311-B8ED-005056822391 | 10013314    | Walton Girls High School                                                               |
| 06D591DF-C6AE-E311-B8ED-005056822391 | NULL        | St Teresa's Catholic (Aided) Primary School                                            |
| 9396874F-C7AE-E311-B8ED-005056822391 | 10005806    | Sheringham High School                                                                 |
| CECCA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Margaret's C of E Primary School                                                    |
| 736FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Birchfield Community Sc                                                                |
| 8CED7F55-C7AE-E311-B8ED-005056822391 | NULL        | Torells School                                                                         |
| AE8382EB-C6AE-E311-B8ED-005056822391 | 10017348    | Salendine Nook Academy Trust                                                           |
| B496874F-C7AE-E311-B8ED-005056822391 | 10001161    | Cardinal Hinsley High School                                                           |
| 56C8BFBB-C6AE-E311-B8ED-005056822391 | 10077848    | Paston Ridings Primary School                                                          |
| 703C451B-C7AE-E311-B8ED-005056822391 | 10017653    | St Edmunds C of E Girls School                                                         |
| 08DF6303-C7AE-E311-B8ED-005056822391 | 10004485    | Aire Valley School                                                                     |
| CA6FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Loughton Middle School                                                                 |
| 673E451B-C7AE-E311-B8ED-005056822391 | 10006697    | The Holy Family College                                                                |
| C50BFD8B-C6AE-E311-B8ED-005056822391 | 10071296    | Goldbeaters JMI School                                                                 |
| C7F73F21-C7AE-E311-B8ED-005056822391 | 10007189    | Ursuline College                                                                       |
| DF3E451B-C7AE-E311-B8ED-005056822391 | 10006154    | St Edmund's Catholic School                                                            |
| A86ADEA3-C6AE-E311-B8ED-005056822391 | 10076381    | Newfield Park                                                                          |
| 423273F7-C6AE-E311-B8ED-005056822391 | NULL        | Towneley High School                                                                   |
| CFB40486-C6AE-E311-B8ED-005056822391 | NULL        | Chesterfield Junior School                                                             |
| B075B0C7-C6AE-E311-B8ED-005056822391 | 10078041    | De Lucy Primary School                                                                 |
| 78B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Coppins Green Primary School                                                           |
| 088382EB-C6AE-E311-B8ED-005056822391 | 10001373    | Chessington Community College                                                          |
| 70D250A3-C7AE-E311-B8ED-005056822391 | 10015711    | Garratt Park School                                                                    |
| 3E3073F7-C6AE-E311-B8ED-005056822391 | 10004249    | Mayfield School and College                                                            |
| FDCB372D-C7AE-E311-B8ED-005056822391 | 10006667    | Frances Bardsley School for Girls                                                      |
| 80FF7261-C7AE-E311-B8ED-005056822391 | 10006806    | The Sandon School                                                                      |
| 8E876BFD-C6AE-E311-B8ED-005056822391 | NULL        | Gillotts School                                                                        |
| 99526A73-C7AE-E311-B8ED-005056822391 | 10018104    | Moffats School                                                                         |
| 398A6BFD-C6AE-E311-B8ED-005056822391 | 10003123    | Holmer Green Upper School                                                              |
| 4796874F-C7AE-E311-B8ED-005056822391 | 10003622    | King Edward VI Camp Hill School for Girls                                              |
| 308182EB-C6AE-E311-B8ED-005056822391 | NULL        | East Brighton College of Media Arts                                                    |
| 343D451B-C7AE-E311-B8ED-005056822391 | NULL        | Cardinal Newman Catholic School                                                        |
| D7A67A5B-C7AE-E311-B8ED-005056822391 | NULL        | Hinchley Wood School                                                                   |
| CD8282EB-C6AE-E311-B8ED-005056822391 | 10013319    | The Ruskington Coteland's School                                                       |
| E2355C09-C7AE-E311-B8ED-005056822391 | 10001794    | Culverhay School                                                                       |
| 8677B0C7-C6AE-E311-B8ED-005056822391 | 10070357    | Beverley Minster CE Primary                                                            |
| A8632E87-C9AE-E311-B8ED-005056822391 | NULL        | GWIST                                                                                  |
| C31FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Beechwood School                                                                       |
| 7C75B0C7-C6AE-E311-B8ED-005056822391 | 10075292    | William Patten Primary                                                                 |
| 623C451B-C7AE-E311-B8ED-005056822391 | 10002411    | Farmor's School                                                                        |
| D374B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Glapton Primary and Nursery School                                                     |
| 4EB60486-C6AE-E311-B8ED-005056822391 | 10072589    | Salterhebble Junior and Infant School                                                  |
| BDC7BFBB-C6AE-E311-B8ED-005056822391 | 10071192    | Rydens School                                                                          |
| EE3273F7-C6AE-E311-B8ED-005056822391 | 10016054    | Tideway School                                                                         |
| 59CDA8CD-C6AE-E311-B8ED-005056822391 | 10070380    | Graveley Primary School                                                                |
| 917A99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Margarets at Troy Town CE Primary School                                            |
| 5877B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Bisham CofE Primary School                                                             |
| 81CEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Philip's CofE Primary School                                                        |
| 5ABCED97-C6AE-E311-B8ED-005056822391 | 10076539    | Allesley Primary School                                                                |
| 2E0A6485-C7AE-E311-B8ED-005056822391 | 10015882    | Hillingdon Manor School                                                                |
| 2CB60486-C6AE-E311-B8ED-005056822391 | 10074671    | Ashburnham School                                                                      |
| 7F8382EB-C6AE-E311-B8ED-005056822391 | 10017168    | Pleckgate High School                                                                  |
| 19C8BFBB-C6AE-E311-B8ED-005056822391 | 10071956    | Blue Gate Fields Infants' School                                                       |
| B62F73F7-C6AE-E311-B8ED-005056822391 | 10004187    | Manor School                                                                           |
| A7BFD6A9-C6AE-E311-B8ED-005056822391 | 10068957    | The Colleton Primary School                                                            |
| B4886BFD-C6AE-E311-B8ED-005056822391 | NULL        | Bishopsford Community School                                                           |
| B4CDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Milton Ernest Lower School                                                             |
| DCD45197-C7AE-E311-B8ED-005056822391 | 10017571    | Rectory Paddock School                                                                 |
| AFED667F-C7AE-E311-B8ED-005056822391 | 10008317    | Kings College                                                                          |
| 9B4D3F48-F010-E511-A3DA-005056822390 | 99999998    | Other EU countries                                                                     |
| B61FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Tamerton Vale Primary School                                                           |
| A9B9ED97-C6AE-E311-B8ED-005056822391 | 10080007    | Haslemere First                                                                        |
| 69F83F21-C7AE-E311-B8ED-005056822391 | 10002846    | Hagley Roman Catholic School                                                           |
| A1C2D6A9-C6AE-E311-B8ED-005056822391 | 10068833    | Rushall JMI School                                                                     |
| 3C24A1D3-C6AE-E311-B8ED-005056822391 | 10073768    | Sedlescombe CE School                                                                  |
| 6969DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Blacklands Primary School                                                              |
| D95D0C80-C6AE-E311-B8ED-005056822391 | 10069479    | Abingdon Primary School                                                                |
| B68082EB-C6AE-E311-B8ED-005056822391 | 10080565    | St Mary & St John CE Primary                                                           |
| 6112E69D-C6AE-E311-B8ED-005056822391 | NULL        | Whitchurch First and Nursery School                                                    |
| 8E7D99D9-C6AE-E311-B8ED-005056822391 | NULL        | Isham Primary School                                                                   |
| 62F73F21-C7AE-E311-B8ED-005056822391 | 10005478    | Ripley St Thomas CE High School                                                        |
| 080CFD8B-C6AE-E311-B8ED-005056822391 | 10071281    | Leopold Primary School                                                                 |
| 72A77A5B-C7AE-E311-B8ED-005056822391 | 10003559    | Kendrick School                                                                        |
| 1DB03A27-C7AE-E311-B8ED-005056822391 | NULL        | Katherines Primary School                                                              |
| CBC7BFBB-C6AE-E311-B8ED-005056822391 | 10074590    | Mowlem Primary School                                                                  |
| 9223A1D3-C6AE-E311-B8ED-005056822391 | 10069119    | St Mark's CE Primary School                                                            |
| 8C996F6D-C7AE-E311-B8ED-005056822391 | 10018039    | Lodge School                                                                           |
| DD298AE5-C6AE-E311-B8ED-005056822391 | 10071634    | St Mary's Catholic Primary School                                                      |
| 796ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Woodside Junior School                                                                 |
| 287C99D9-C6AE-E311-B8ED-005056822391 | 10045107    | St Mary & St Pauls C of E Primary School                                               |
| 9D615C8B-C7AE-E311-B8ED-005056822391 | 10015871    | Highfield School                                                                       |
| 6519CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Cardrew Junior School                                                                  |
| 98D97AF1-C6AE-E311-B8ED-005056822391 | 10017343    | Royton & Crompton School                                                               |
| 41D391DF-C6AE-E311-B8ED-005056822391 | 10074397    | St Joseph's Catholic Junior School                                                     |
| 93B40486-C6AE-E311-B8ED-005056822391 | NULL        | Clitterhouse Infant School                                                             |
| 3B18CFAF-C6AE-E311-B8ED-005056822391 | 10074073    | Delce Infant School                                                                    |
| 983E451B-C7AE-E311-B8ED-005056822391 | NULL        | Chester Catholic High School                                                           |
| 9BC1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Alban Wood School                                                                      |
| 2670C7B5-C6AE-E311-B8ED-005056822391 | 10077891    | Waddesdon C of E School                                                                |
| 4BC6BFBB-C6AE-E311-B8ED-005056822391 | 10072925    | Marlborough School - Chelsea - 207/2399                                                |
| 892D8AE5-C6AE-E311-B8ED-005056822391 | 10037983    | London Academy of Excellence                                                           |
| 6AAF3A27-C7AE-E311-B8ED-005056822391 | 10069593    | Dollis Junior School                                                                   |
| 8FCB372D-C7AE-E311-B8ED-005056822391 | 10080355    | St Helens Catholic Infant School                                                       |
| F37C99D9-C6AE-E311-B8ED-005056822391 | 10078667    | St Joseph's Catholic Primary School                                                    |
| A2B9ED97-C6AE-E311-B8ED-005056822391 | NULL        | The R J Mithchell Primary                                                              |
| 78298AE5-C6AE-E311-B8ED-005056822391 | 10079327    | St Bedes C of E Junior School                                                          |
| 5C2C8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Josephs RC Primary School                                                           |
| 0911E69D-C6AE-E311-B8ED-005056822391 | NULL        | Cann Hall Primary School                                                               |
| D81B5791-C7AE-E311-B8ED-005056822391 | 10014962    | Beckmead School                                                                        |
| 93BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Lower Park Primary School                                                              |
| DA0CFD8B-C6AE-E311-B8ED-005056822391 | 10078448    | Moorgate Primary School                                                                |
| 641ACFAF-C6AE-E311-B8ED-005056822391 | NULL        | Rossgate Primary School                                                                |
| 77C8BFBB-C6AE-E311-B8ED-005056822391 | 10072799    | Westwood Infant Nursery School                                                         |
| 0C876BFD-C6AE-E311-B8ED-005056822391 | 10007552    | Wirral County Grammar School for Girls                                                 |
| B67D99D9-C6AE-E311-B8ED-005056822391 | 10071539    | Dogmersfield CE Primary School                                                         |
| DD355C09-C7AE-E311-B8ED-005056822391 | 10017276    | Putteridge High School                                                                 |
| 49298AE5-C6AE-E311-B8ED-005056822391 | 10080277    | St. Marys Catholic Primary School                                                      |
| 12D87AF1-C6AE-E311-B8ED-005056822391 | 10015886    | Whitton School                                                                         |
| A7D591DF-C6AE-E311-B8ED-005056822391 | 10071706    | St George's Cathedral Catholic Primary School                                          |
| F717CFAF-C6AE-E311-B8ED-005056822391 | 10069348    | Treleigh County Primary School                                                         |
| DED97AF1-C6AE-E311-B8ED-005056822391 | 10002438    | The Feltham School                                                                     |
| 5A9A6F6D-C7AE-E311-B8ED-005056822391 | 10018770    | St Aubyn's School                                                                      |
| C46EC7B5-C6AE-E311-B8ED-005056822391 | 10079049    | Cunningham Hill Junior School                                                          |
| 4C876BFD-C6AE-E311-B8ED-005056822391 | 10018122    | Highfield High School                                                                  |
| 97E74C15-C7AE-E311-B8ED-005056822391 | 10002425    | Fazakerley High School                                                                 |
| 2D20B8C1-C6AE-E311-B8ED-005056822391 | NULL        | North Border Junior School                                                             |
| 38CDA8CD-C6AE-E311-B8ED-005056822391 | 10080204    | Orchard CE Primary School                                                              |
| C3C6BFBB-C6AE-E311-B8ED-005056822391 | 10077102    | Fairways Primary School                                                                |
| F0B50486-C6AE-E311-B8ED-005056822391 | 10072851    | The Russell School                                                                     |
| F0ED667F-C7AE-E311-B8ED-005056822391 | 10013309    | Stamford School                                                                        |
| D93E451B-C7AE-E311-B8ED-005056822391 | 10003254    | Ilford Ursuline High School                                                            |
| 68DA7AF1-C6AE-E311-B8ED-005056822391 | 10018779    | Marden Bridge Middle School                                                            |
| 1D69DEA3-C6AE-E311-B8ED-005056822391 | NULL        | The Sweyne Junior School                                                               |
| F6CB372D-C7AE-E311-B8ED-005056822391 | 10006733    | The Latymer School                                                                     |
| D87D99D9-C6AE-E311-B8ED-005056822391 | 10074742    | Christ Church Primary School                                                           |
| 7A71C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Lostock Hall Primary School                                                            |
| 6D13E69D-C6AE-E311-B8ED-005056822391 | 10077106    | Manor Primary School                                                                   |
| 2C7A589D-C7AE-E311-B8ED-005056822391 | 10016198    | Saltergill School                                                                      |
| 421EB8C1-C6AE-E311-B8ED-005056822391 | 10077176    | Thomas Fairchild Community School                                                      |
| E71FB8C1-C6AE-E311-B8ED-005056822391 | 10074058    | St Peter's Infant School                                                               |
| BC0C6579-C7AE-E311-B8ED-005056822391 | 10017342    | Queenswood                                                                             |
| D20DFD8B-C6AE-E311-B8ED-005056822391 | 10068783    | Russell Scott Primary School                                                           |
| 55BBED97-C6AE-E311-B8ED-005056822391 | 10076106    | Landscore Primary Score                                                                |
| 527A589D-C7AE-E311-B8ED-005056822391 | 10015833    | Walton Hall Community Special School                                                   |
| 14C2D6A9-C6AE-E311-B8ED-005056822391 | 10069871    | Newlands Community Primary School                                                      |
| 598F540F-C7AE-E311-B8ED-005056822391 | 10003952    | Littleover Community School                                                            |
| 3519CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Brampton Infant School                                                                 |
| 23536A73-C7AE-E311-B8ED-005056822391 | 10071139    | Deepdene School                                                                        |
| EE385C09-C7AE-E311-B8ED-005056822391 | 10017735    | The Clere School                                                                       |
| ED8C540F-C7AE-E311-B8ED-005056822391 | NULL        | Fulford School                                                                         |
| 2918CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Salt Way Primary School                                                                |
| DB1FB8C1-C6AE-E311-B8ED-005056822391 | 10073713    | Wyvill Primary School                                                                  |
| 47D291DF-C6AE-E311-B8ED-005056822391 | NULL        | St Giles CofE (VA) J&I Pontefract                                                      |
| A464F591-C6AE-E311-B8ED-005056822391 | 10077516    | Carville Primary School                                                                |
| DCE16303-C7AE-E311-B8ED-005056822391 | 10016191    | Kingsbury Comprehensive School                                                         |
| D6B40486-C6AE-E311-B8ED-005056822391 | 10080040    | Edwin Lambert School                                                                   |
| 485C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Tringle Nursery                                                                        |
| D3C8BFBB-C6AE-E311-B8ED-005056822391 | 10077726    | Ramsden Infant School                                                                  |
| 65896BFD-C6AE-E311-B8ED-005056822391 | 10017863    | Fernwood Compreehensive School                                                         |
| 62D391DF-C6AE-E311-B8ED-005056822391 | 10075025    | St Margaret Mary's Catholic Junior School                                              |
| 8DD87AF1-C6AE-E311-B8ED-005056822391 | 10016197    | Light Hall School                                                                      |
| 12C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Connaught County Junior School                                                         |
| D1C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Casterton Primary School                                                               |
| 4E6ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Langney Primary School                                                                 |
| 3D0A6485-C7AE-E311-B8ED-005056822391 | 10000381    | The Arts Educational School                                                            |
| C226A1D3-C6AE-E311-B8ED-005056822391 | NULL        | St Peter's Junior School                                                               |
| D670C7B5-C6AE-E311-B8ED-005056822391 | 10076403    | Chad Vale Primary School                                                               |
| 8D1B5791-C7AE-E311-B8ED-005056822391 | NULL        | Stubton Hall School                                                                    |
| AA3B451B-C7AE-E311-B8ED-005056822391 | 10000223    | All Saints College                                                                     |
| 42896BFD-C6AE-E311-B8ED-005056822391 | NULL        | Braim Wood Boys' High School                                                           |
| 0CEE667F-C7AE-E311-B8ED-005056822391 | 10071124    | The Grey House Shcool                                                                  |
| F8298AE5-C6AE-E311-B8ED-005056822391 | 10079503    | St Edward's Primary School                                                             |
| C23F8F49-C7AE-E311-B8ED-005056822391 | 10006790    | The Queen Katherine School                                                             |
| 65E74C15-C7AE-E311-B8ED-005056822391 | 10018577    | Bredon Hill Middle School                                                              |
| F6D191DF-C6AE-E311-B8ED-005056822391 | NULL        | St .Mary's Catholic Primary School                                                     |
| 7426A1D3-C6AE-E311-B8ED-005056822391 | 10075425    | Sutton Veny Church of England Primary School                                           |
| 1DB40486-C6AE-E311-B8ED-005056822391 | 10072873    | Colham Manor Primary School                                                            |
| CB1B5791-C7AE-E311-B8ED-005056822391 | 10017564    | The Priory School                                                                      |
| 747B99D9-C6AE-E311-B8ED-005056822391 | NULL        | St John's C of E Junior and Infant School                                              |
| B3F73F21-C7AE-E311-B8ED-005056822391 | NULL        | St Marys Catholic Middle School                                                        |
| 18B03A27-C7AE-E311-B8ED-005056822391 | 10077490    | Orton Wistow Primary School                                                            |
| C21A5791-C7AE-E311-B8ED-005056822391 | 10008153    | Caterham School                                                                        |
| BC8082EB-C6AE-E311-B8ED-005056822391 | 10074984    | St Theresa's Catholic Primary                                                          |
| 2711E69D-C6AE-E311-B8ED-005056822391 | NULL        | Knottingley England Lane Junior Infant and Nursery School                              |
| 4124A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Stoke By Nayland CE VC Primary School                                                  |
| EABBED97-C6AE-E311-B8ED-005056822391 | 10071492    | Tipton Green Junior School                                                             |
| 2B13E69D-C6AE-E311-B8ED-005056822391 | 10045829    | Larchwood Primary School                                                               |
| 1A63F591-C6AE-E311-B8ED-005056822391 | NULL        | Jubilee Park                                                                           |
| C6B9ED97-C6AE-E311-B8ED-005056822391 | 10075395    | Fair Oak Infant School                                                                 |
| 9B0EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Yardley Infant School                                                                  |
| BFBCED97-C6AE-E311-B8ED-005056822391 | NULL        | Dukesgate Primary School                                                               |
| D13D451B-C7AE-E311-B8ED-005056822391 | 10015468    | Debenham CE VC High School                                                             |
| 636ADEA3-C6AE-E311-B8ED-005056822391 | 10069522    | Kemsing Primary School                                                                 |
| 3E3F451B-C7AE-E311-B8ED-005056822391 | NULL        | Frideswide School                                                                      |
| 556EC7B5-C6AE-E311-B8ED-005056822391 | 10074071    | Westmeads CI School                                                                    |
| 9A1DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Leigh Junior, Infant and Nursery                                                       |
| 740DFD8B-C6AE-E311-B8ED-005056822391 | 10071915    | Newnham Infant & Nursery School                                                        |
| C2E54C15-C7AE-E311-B8ED-005056822391 | 10006251    | St Peter's High School and Technology College                                          |
| B226A1D3-C6AE-E311-B8ED-005056822391 | 10080485    | St Matthews Bloxam C of E Primary School                                               |
| 260A6485-C7AE-E311-B8ED-005056822391 | 10008235    | Finborough School                                                                      |
| 001EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Stebon Primary School                                                                  |
| 213373F7-C6AE-E311-B8ED-005056822391 | 10003172    | Batley Girls' High School - Visual Arts College                                        |
| AD10E69D-C6AE-E311-B8ED-005056822391 | 10072135    | Seaford Primary School                                                                 |
| 14BCED97-C6AE-E311-B8ED-005056822391 | 10076699    | Bruce Grove Primary                                                                    |
| 15056786-C8AE-E311-B8ED-005056822391 | NULL        | University Of Central England In Birmingham                                            |
| 8B71C7B5-C6AE-E311-B8ED-005056822391 | 10074635    | Kender Primary School                                                                  |
| 9012E69D-C6AE-E311-B8ED-005056822391 | NULL        | Cherry Tree Primary School                                                             |
| 94536A73-C7AE-E311-B8ED-005056822391 | 10018431    | Eltree School                                                                          |
| 2F996F6D-C7AE-E311-B8ED-005056822391 | 10077565    | The Gleddings School                                                                   |
| 7F8F540F-C7AE-E311-B8ED-005056822391 | 10015715    | Wheelers Lane Technology College                                                       |
| 229A6F6D-C7AE-E311-B8ED-005056822391 | 10006207    | St Joseph's Convent School                                                             |
| CCE16303-C7AE-E311-B8ED-005056822391 | 10001677    | Coombeshead College                                                                    |
| B577B0C7-C6AE-E311-B8ED-005056822391 | NULL        | St Matthew's CE Primary School                                                         |
| 3B5C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Winton Community Nursery                                                               |
| 37E84C15-C7AE-E311-B8ED-005056822391 | 10003654    | Kings International College for Business and The A                                     |
| 2BE74C15-C7AE-E311-B8ED-005056822391 | NULL        | Park Community School                                                                  |
| 042D8AE5-C6AE-E311-B8ED-005056822391 | 10076867    | St Josephs Catholic Primary School                                                     |
| FA75B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Tuxford Comprehensive School                                                           |
| AF7B99D9-C6AE-E311-B8ED-005056822391 | 10071542    | St Matthews C.E Primary School                                                         |
| F2046786-C8AE-E311-B8ED-005056822391 | NULL        | Nene College                                                                           |
| A876B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Whitton Community Primary School                                                       |
| 3C2D8AE5-C6AE-E311-B8ED-005056822391 | 10017750    | The City School                                                                        |
| 5CC8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Lannock JMI School                                                                     |
| 42C7BFBB-C6AE-E311-B8ED-005056822391 | 10077334    | Hawksworth Wood Primary School                                                         |
| 8750BF3B-C7AE-E311-B8ED-005056822391 | 10004563    | Netherthorpe School                                                                    |
| B7526A73-C7AE-E311-B8ED-005056822391 | 10006230    | St Mary's Hall                                                                         |
| 551B7067-C7AE-E311-B8ED-005056822391 | 10017077    | Prior Park College                                                                     |
| 1990540F-C7AE-E311-B8ED-005056822391 | 10000826    | Bournville School                                                                      |
| D9996F6D-C7AE-E311-B8ED-005056822391 | NULL        | Bryanston School                                                                       |
| E3E06303-C7AE-E311-B8ED-005056822391 | NULL        | Alameda Middle School                                                                  |
| E863F591-C6AE-E311-B8ED-005056822391 | 10070014    | Salisbury Primary School                                                               |
| 820EFD8B-C6AE-E311-B8ED-005056822391 | 10075671    | Marshfield Primary School                                                              |
| A68382EB-C6AE-E311-B8ED-005056822391 | 10015423    | Droylsden High School for Girls                                                        |
| C5876BFD-C6AE-E311-B8ED-005056822391 | 10004590    | New Mills School                                                                       |
| 0E7A589D-C7AE-E311-B8ED-005056822391 | 10000566    | Batchwood School                                                                       |
| 26C7BFBB-C6AE-E311-B8ED-005056822391 | 10070808    | Harwich Community Primary School & Nursurey                                            |
| 17536A73-C7AE-E311-B8ED-005056822391 | 10008196    | Leicester Independent School                                                           |
| 6D1EB8C1-C6AE-E311-B8ED-005056822391 | 10069257    | West Witney County Primary School                                                      |
| 721B5791-C7AE-E311-B8ED-005056822391 | 10017744    | The Sundridge School                                                                   |
| B3B60486-C6AE-E311-B8ED-005056822391 | NULL        | Colston's Primary School                                                               |
| D9526A73-C7AE-E311-B8ED-005056822391 | 10008622    | Aldenham School                                                                        |
| 3E1DB8C1-C6AE-E311-B8ED-005056822391 | 10077236    | The Leys Primary & Nursery School                                                      |
| 1E395C09-C7AE-E311-B8ED-005056822391 | 10002022    | Downend School                                                                         |
| 0F7C99D9-C6AE-E311-B8ED-005056822391 | 10070171    | St Joseph's RC VA Primary School                                                       |
| BD63F591-C6AE-E311-B8ED-005056822391 | 10071329    | Brandlehow School                                                                      |
| 8F5D0C80-C6AE-E311-B8ED-005056822391 | NULL        | North Islington Nursery School                                                         |
| EEC0D6A9-C6AE-E311-B8ED-005056822391 | 10072880    | Gifford Primary School                                                                 |
| AE64F591-C6AE-E311-B8ED-005056822391 | 10071272    | Newfield Primary School                                                                |
| C575B0C7-C6AE-E311-B8ED-005056822391 | 10070040    | Sir Francis Drake Primary School                                                       |
| 253E451B-C7AE-E311-B8ED-005056822391 | 10017174    | St Philip Howard School                                                                |
| F47D99D9-C6AE-E311-B8ED-005056822391 | 10068572    | St Philip's CE Primary                                                                 |
| 181EB8C1-C6AE-E311-B8ED-005056822391 | 10073103    | Briary County Primary School                                                           |
| AD1B5791-C7AE-E311-B8ED-005056822391 | 10015741    | Hillside Special School                                                                |
| 2A1ACFAF-C6AE-E311-B8ED-005056822391 | 10081299    | Woolacombe School                                                                      |
| C274B0C7-C6AE-E311-B8ED-005056822391 | 10074780    | Lambley County Primary                                                                 |
| 350EFD8B-C6AE-E311-B8ED-005056822391 | 10072935    | Hardwick Primary                                                                       |
| 29D491DF-C6AE-E311-B8ED-005056822391 | 10079971    | Thomas Gray Infants School                                                             |
| 59A77A5B-C7AE-E311-B8ED-005056822391 | 10006714    | John Henry Newman RC School                                                            |
| 130D6579-C7AE-E311-B8ED-005056822391 | 10078224    | Dair House School                                                                      |
| 050A6485-C7AE-E311-B8ED-005056822391 | 10018042    | Junior King's School                                                                   |
| 5EEE667F-C7AE-E311-B8ED-005056822391 | 10008188    | Dean Close School                                                                      |
| 131DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Sherwin Knight Community Infant School                                                 |
| EAA77A5B-C7AE-E311-B8ED-005056822391 | 10004236    | Matravers School                                                                       |
| 29E64C15-C7AE-E311-B8ED-005056822391 | 10017639    | The Alfred Barrow School                                                               |
| EDC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Brookside Primary School                                                               |
| A30DFD8B-C6AE-E311-B8ED-005056822391 | 10075050    | Rokesley Junior School                                                                 |
| FF0DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Helmdon Primary School                                                                 |
| B52D8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Archbishop Cranmer Church of England Primary School                                    |
| 96355C09-C7AE-E311-B8ED-005056822391 | NULL        | Worthing High School                                                                   |
| 3D6EC7B5-C6AE-E311-B8ED-005056822391 | 10073005    | Pixies Hill JMI School                                                                 |
| A2C6BFBB-C6AE-E311-B8ED-005056822391 | 10077335    | Bramhope Primary School                                                                |
| B0E16303-C7AE-E311-B8ED-005056822391 | 10000888    | Brigshaw High School and Language College                                              |
| ED24A1D3-C6AE-E311-B8ED-005056822391 | 10065385    | Wickhambreaux CE Primary School                                                        |
| 8E8D540F-C7AE-E311-B8ED-005056822391 | NULL        | Marston Middle School                                                                  |
| 95EE667F-C7AE-E311-B8ED-005056822391 | 10008162    | Churcher's College                                                                     |
| 4269DEA3-C6AE-E311-B8ED-005056822391 | 10077147    | Golden Flatts Primary School                                                           |
| A7C7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Riverside Community Primary                                                            |
| 71096485-C7AE-E311-B8ED-005056822391 | 10014817    | Birch House School                                                                     |
| 0918CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Thursfield Primary School                                                              |
| 91CEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Greens Norton CE Primary School                                                        |
| 37E54C15-C7AE-E311-B8ED-005056822391 | 10038070    | Redbridge Community School                                                             |
| 273D451B-C7AE-E311-B8ED-005056822391 | 10006279    | St Bede's Catholic Grammar School                                                      |
| 1F64F591-C6AE-E311-B8ED-005056822391 | 10071445    | Hunter's Bar Junior School                                                             |
| 6D6EC7B5-C6AE-E311-B8ED-005056822391 | 10077638    | Bramcote Hills Primary School                                                          |
| 5F7A589D-C7AE-E311-B8ED-005056822391 | 10015380    | Cornfields School                                                                      |
| E2BBED97-C6AE-E311-B8ED-005056822391 | NULL        | Conway Primary School                                                                  |
| 977B99D9-C6AE-E311-B8ED-005056822391 | 10081362    | St Mary and All Saints Church of England Voluntary Aided Primary School                |
| C6B30486-C6AE-E311-B8ED-005056822391 | NULL        | Court House Green Primary School                                                       |
| CA1A5791-C7AE-E311-B8ED-005056822391 | NULL        | Ripplevale School                                                                      |
| 5E6EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Kelsall Community Primary School                                                       |
| AAE16303-C7AE-E311-B8ED-005056822391 | 10002455    | Abbeywood Community School                                                             |
| 7DD491DF-C6AE-E311-B8ED-005056822391 | 10071627    | St Swithun Wells School                                                                |
| 531A7067-C7AE-E311-B8ED-005056822391 | 10005510    | The Rochester Grammar School for Girls                                                 |
| 947A99D9-C6AE-E311-B8ED-005056822391 | 10074251    | Southborough Church of England Primary School And                                      |
| 6EB98E86-C721-EC11-B6E6-000D3AB86145 | 10008026    | St Mary’s University College, Belfast                                                  |
| C5056786-C8AE-E311-B8ED-005056822391 | NULL        | Sherbourne School for girls                                                            |
| 96BBED97-C6AE-E311-B8ED-005056822391 | NULL        | Kempsey Primary School                                                                 |
| 42B9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Mercenfeld Primary School                                                              |
| 15642E87-C9AE-E311-B8ED-005056822391 | NULL        | St Mary's College                                                                      |
| 12D391DF-C6AE-E311-B8ED-005056822391 | 10069070    | Deal Parochial School                                                                  |
| 4E0BFD8B-C6AE-E311-B8ED-005056822391 | 10070019    | Isleworth Town Primary School                                                          |
| A51FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Austin Farm Primary School                                                             |
| 2676B0C7-C6AE-E311-B8ED-005056822391 | 10077246    | Lickey Hills Primary School                                                            |
| 620A6485-C7AE-E311-B8ED-005056822391 | NULL        | St Benedict's Junior School                                                            |
| 1C3E451B-C7AE-E311-B8ED-005056822391 | 10016974    | St Damian's RC Science College                                                         |
| 8A7B99D9-C6AE-E311-B8ED-005056822391 | 10070233    | English Martyrs' Catholic Primary School                                               |
| ECF73F21-C7AE-E311-B8ED-005056822391 | 10000753    | Blackheath Bluecoat CE Secondary School                                                |
| 1C3D451B-C7AE-E311-B8ED-005056822391 | 10006109    | St Angela's Ursuline Convent School                                                    |
| 6DD55197-C7AE-E311-B8ED-005056822391 | 10016875    | St Christopher's Fiest School                                                          |
| B174B0C7-C6AE-E311-B8ED-005056822391 | 10074651    | Allen Edwards Primary                                                                  |
| 2DB30486-C6AE-E311-B8ED-005056822391 | NULL        | Yardley Primary School                                                                 |
| A8BBED97-C6AE-E311-B8ED-005056822391 | NULL        | Marston Green Infant                                                                   |
| 7FBAED97-C6AE-E311-B8ED-005056822391 | 10076925    | Markyate Village School                                                                |
| 8FC5BFBB-C6AE-E311-B8ED-005056822391 | 10054924    | Round Diamond Primary School                                                           |
| 0BE84C15-C7AE-E311-B8ED-005056822391 | 10002218    | The Elizabethan High School                                                            |
| C5CDA8CD-C6AE-E311-B8ED-005056822391 | 10071016    | Etchingham CE Primary School                                                           |
| 060D6579-C7AE-E311-B8ED-005056822391 | 10016481    | St Martha's Convent RC School                                                          |
| 8F17CFAF-C6AE-E311-B8ED-005056822391 | 10070037    | Essendine Primary School                                                               |
| 795C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Claires Court School                                                                   |
| 3ADA7AF1-C6AE-E311-B8ED-005056822391 | 10006046    | Sowerby Bridge High School                                                             |
| 88C0D6A9-C6AE-E311-B8ED-005056822391 | 10074305    | Grange Primary                                                                         |
| 36E64C15-C7AE-E311-B8ED-005056822391 | 10003067    | Highbury Fields School                                                                 |
| C5CCA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Marylandsea Community School                                                           |
| 0DC6BFBB-C6AE-E311-B8ED-005056822391 | 10081451    | Pirbright County First Anmiddle School                                                 |
| 643273F7-C6AE-E311-B8ED-005056822391 | 10006859    | Alderman Callow School and Community College                                           |
| A311E69D-C6AE-E311-B8ED-005056822391 | NULL        | Hinguar Community Primary School                                                       |
| 7575B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Fairfield Primary School                                                               |
| 83C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Slade Junior and Infant School                                                         |
| 3DFE7261-C7AE-E311-B8ED-005056822391 | 10005870    | Sir Roger Manwood's School                                                             |
| 4EB9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Claydon Primary School                                                                 |
| 8BD591DF-C6AE-E311-B8ED-005056822391 | 10075988    | St Francis School                                                                      |
| 5FC0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Estcourt Primary School                                                                |
| 93DF6303-C7AE-E311-B8ED-005056822391 | 10016952    | The College High School                                                                |
| 0F12E69D-C6AE-E311-B8ED-005056822391 | 10075285    | Central Park Primary School                                                            |
| 1CD391DF-C6AE-E311-B8ED-005056822391 | 10076001    | Holy Cross Primary School                                                              |
| 8F24A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Freshford Church of England Primary School                                             |
| 433E8F49-C7AE-E311-B8ED-005056822391 | 10006696    | Holy Cross Convent School                                                              |
| D76ADEA3-C6AE-E311-B8ED-005056822391 | 10075263    | Athersley South Primary School                                                         |
| 042A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Anthony's Roman Catholic Primary School                                             |
| 7E96874F-C7AE-E311-B8ED-005056822391 | NULL        | Katherine Lady Berkeleys School                                                        |
| 32876BFD-C6AE-E311-B8ED-005056822391 | 10005210    | Prince William School                                                                  |
| 698282EB-C6AE-E311-B8ED-005056822391 | 10015879    | Wakefield City Academy                                                                 |
| A0B03A27-C7AE-E311-B8ED-005056822391 | 10073676    | Elmstead Primary                                                                       |
| 5D64F591-C6AE-E311-B8ED-005056822391 | 10068853    | The Glebe Primary School                                                               |
| FBE74C15-C7AE-E311-B8ED-005056822391 | 10007086    | Tuxford School                                                                         |
| 23E16303-C7AE-E311-B8ED-005056822391 | 10006681    | The Harvey Grammar School                                                              |
| C320B8C1-C6AE-E311-B8ED-005056822391 | 10070806    | Henham & Ugley Primary School                                                          |
| 9E74B0C7-C6AE-E311-B8ED-005056822391 | 10080704    | Priory Primary School                                                                  |
| 487C99D9-C6AE-E311-B8ED-005056822391 | NULL        | Lady Seaward Primary                                                                   |
| 225F0C80-C6AE-E311-B8ED-005056822391 | 10068835    | Busill Jones Primary School                                                            |
| C97B99D9-C6AE-E311-B8ED-005056822391 | 10076808    | St Mary's Catholic Primary School                                                      |
| 923173F7-C6AE-E311-B8ED-005056822391 | NULL        | Arnold Middle School                                                                   |
| E4375C09-C7AE-E311-B8ED-005056822391 | 10005028    | Pendeford High School                                                                  |
| BA25A1D3-C6AE-E311-B8ED-005056822391 | 10072073    | Bacup Holy Trinity CE Primary School                                                   |
| E690540F-C7AE-E311-B8ED-005056822391 | NULL        | Waltheof School                                                                        |
| 4619CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Icknield Primary School                                                                |
| C426A1D3-C6AE-E311-B8ED-005056822391 | 10077919    | Cumnor CE Primary                                                                      |
| EDE74C15-C7AE-E311-B8ED-005056822391 | 10002055    | Dukeries Community College                                                             |
| 2326A1D3-C6AE-E311-B8ED-005056822391 | 10069103    | Stelling Minnis CE Primary School                                                      |
| B926A1D3-C6AE-E311-B8ED-005056822391 | 10072438    | St John Church of England Voluntary Controlled Pri                                     |
| BCCEA8CD-C6AE-E311-B8ED-005056822391 | 10078494    | Middleton C of E Primary School                                                        |
| B73F8F49-C7AE-E311-B8ED-005056822391 | 10006164    | St George's College of Technology                                                      |
| E70C6579-C7AE-E311-B8ED-005056822391 | NULL        | Davenport Lodge School                                                                 |
| AE2D8AE5-C6AE-E311-B8ED-005056822391 | 10068612    | Meanwood Church of England Primary School                                              |
| B2AF3A27-C7AE-E311-B8ED-005056822391 | 10037956    | St Charles RC Primary School                                                           |
| 03375C09-C7AE-E311-B8ED-005056822391 | 10007450    | Westlands School                                                                       |
| 7CEE667F-C7AE-E311-B8ED-005056822391 | 10018776    | Solefield School                                                                       |
| C23E8F49-C7AE-E311-B8ED-005056822391 | NULL        | The Cornwallis School                                                                  |
| 6AB60486-C6AE-E311-B8ED-005056822391 | NULL        | New Briars Primary & Nursery School                                                    |
| CA8E540F-C7AE-E311-B8ED-005056822391 | 10015846    | Henry Beaufort School                                                                  |
| 2265F591-C6AE-E311-B8ED-005056822391 | NULL        | Undercliffe Primary School                                                             |
| 8250BF3B-C7AE-E311-B8ED-005056822391 | 10006310    | Bridgewater Hall School                                                                |
| 2B3F451B-C7AE-E311-B8ED-005056822391 | 10017357    | St Monica's RC High School                                                             |
| 842B8AE5-C6AE-E311-B8ED-005056822391 | 10078179    | St Sebastian's Catholic Primary School                                                 |
| B4B30486-C6AE-E311-B8ED-005056822391 | NULL        | Davidson Infant School                                                                 |
| E2D45197-C7AE-E311-B8ED-005056822391 | 10017906    | Green Park School                                                                      |
| F80DFD8B-C6AE-E311-B8ED-005056822391 | 10069560    | South Hill Primary School                                                              |
| F9DA7AF1-C6AE-E311-B8ED-005056822391 | 10004167    | Malvern, The Chase                                                                     |
| A7B40486-C6AE-E311-B8ED-005056822391 | NULL        | Rolls Crescent Primary School                                                          |
| BF90540F-C7AE-E311-B8ED-005056822391 | NULL        | Wilmslow High School                                                                   |
| E976B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Manton Primary School                                                                  |
| 4F1DB8C1-C6AE-E311-B8ED-005056822391 | 10071250    | Barely Hill Primary School                                                             |
| A4BBED97-C6AE-E311-B8ED-005056822391 | NULL        | Hillingdon Primary School                                                              |
| EAC1D6A9-C6AE-E311-B8ED-005056822391 | 10071327    | Earlsfield Primary School                                                              |
| 1E0BFD8B-C6AE-E311-B8ED-005056822391 | 10079702    | George Spicer Primary School                                                           |
| 87E16303-C7AE-E311-B8ED-005056822391 | 10005375    | Rainhill High School                                                                   |
| ADE64C15-C7AE-E311-B8ED-005056822391 | 10015895    | Warblington School                                                                     |
| 85DA7AF1-C6AE-E311-B8ED-005056822391 | NULL        | Chesterton Community College                                                           |
| ACED667F-C7AE-E311-B8ED-005056822391 | NULL        | Manor House                                                                            |
| 86C9BFBB-C6AE-E311-B8ED-005056822391 | 10073621    | Wattville Primary School                                                               |
| 415C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Bloomsbury Nursery School                                                              |
| 0D75B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Mersey Primary School                                                                  |
| 0F64F591-C6AE-E311-B8ED-005056822391 | NULL        | Anerley Primary School                                                                 |
| E0CEA8CD-C6AE-E311-B8ED-005056822391 | 10070514    | Steeple Morden CE Primary School                                                       |
| 26D250A3-C7AE-E311-B8ED-005056822391 | 10039935    | Treloar School                                                                         |
| 15F83F21-C7AE-E311-B8ED-005056822391 | NULL        | Park Hall Junior School                                                                |
| 3D62F591-C6AE-E311-B8ED-005056822391 | NULL        | Stile Common Junior School                                                             |
| 180CFD8B-C6AE-E311-B8ED-005056822391 | 10070953    | Whitehall Nursery and Infant School                                                    |
| 8E8382EB-C6AE-E311-B8ED-005056822391 | 10018411    | Ventnor Middle School                                                                  |
| D37C99D9-C6AE-E311-B8ED-005056822391 | 10073469    | Higham CE Primary School                                                               |
| 48DF6303-C7AE-E311-B8ED-005056822391 | NULL        | Boothville Middle School                                                               |
| 3A70C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Ashbury Primary School                                                                 |
| D136ECD4-C9AE-E311-B8ED-005056822391 | 99          | Northern College of Education                                                          |
| 39CCA8CD-C6AE-E311-B8ED-005056822391 | 10069825    | St John the Baptist CE VC Primary School                                               |
| CC7A589D-C7AE-E311-B8ED-005056822391 | 10014877    | Wilson Stuart School                                                                   |
| 78D87AF1-C6AE-E311-B8ED-005056822391 | 10017596    | The Charles Read High School                                                           |
| CBF73F21-C7AE-E311-B8ED-005056822391 | 10013630    | Mount Carmel RC                                                                        |
| F61FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | St Stephen's Primary School                                                            |
| 9450BF3B-C7AE-E311-B8ED-005056822391 | 10002893    | Hardley School & Sixth Form                                                            |
| 5C1ACFAF-C6AE-E311-B8ED-005056822391 | 10073631    | Claycots School                                                                        |
| B62C8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Keyham Barton Roman Catholic School                                                    |
| A8F73F21-C7AE-E311-B8ED-005056822391 | 10000225    | Mount School                                                                           |
| 4862F591-C6AE-E311-B8ED-005056822391 | 10069953    | Kennington Primary School                                                              |
| B71B5791-C7AE-E311-B8ED-005056822391 | 10077030    | The Woodsetton School                                                                  |
| A16FC7B5-C6AE-E311-B8ED-005056822391 | 10078769    | Woodcroft First School                                                                 |
| 630EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Oakdale Junior School                                                                  |
| 299A6F6D-C7AE-E311-B8ED-005056822391 | 10018787    | Ludgrove School                                                                        |
| 0CD291DF-C6AE-E311-B8ED-005056822391 | NULL        | St Joseph's Catholic Primary School                                                    |
| 89D391DF-C6AE-E311-B8ED-005056822391 | NULL        | Our Lady of Pity RC Primary School                                                     |
| F38382EB-C6AE-E311-B8ED-005056822391 | 10001845    | Danum Academy                                                                          |
| 306FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | High View Primary School                                                               |
| 42D250A3-C7AE-E311-B8ED-005056822391 | 10015185    | Chartfield Delicate School                                                             |
| 64C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | K Primary School                                                                       |
| 5A64F591-C6AE-E311-B8ED-005056822391 | NULL        | Novers Lane Junior School                                                              |
| E162F591-C6AE-E311-B8ED-005056822391 | NULL        | Scout Road Academy                                                                     |
| A7536A73-C7AE-E311-B8ED-005056822391 | 10008107    | Bishop's Stortford College                                                             |
| EB876BFD-C6AE-E311-B8ED-005056822391 | 10000475    | Aylesbury High School                                                                  |
| 046ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Brooklands Primary School                                                              |
| 95B9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Scotts Park Primary School                                                             |
| 7E2D8AE5-C6AE-E311-B8ED-005056822391 | 10015090    | Clifton Community School                                                               |
| B3DE6303-C7AE-E311-B8ED-005056822391 | 10003492    | John Mansfield School                                                                  |
| 4218CFAF-C6AE-E311-B8ED-005056822391 | 10076361    | Raeburn Primary School                                                                 |
| 29886BFD-C6AE-E311-B8ED-005056822391 | 10002630    | The Garth Hill                                                                         |
| AFC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Spilsby Primary School                                                                 |
| B7DA7AF1-C6AE-E311-B8ED-005056822391 | 10004133    | Lyng Hall School                                                                       |
| 358182EB-C6AE-E311-B8ED-005056822391 | 10016588    | Peacehaven Community School                                                            |
| 010DFD8B-C6AE-E311-B8ED-005056822391 | 10078447    | Heathfield Primary School                                                              |
| 55B40486-C6AE-E311-B8ED-005056822391 | 10073233    | St John's Green Primary School                                                         |
| D8C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Sandback Primary                                                                       |
| A6B50486-C6AE-E311-B8ED-005056822391 | NULL        | Gilbert Scott Infant School                                                            |
| AD615C8B-C7AE-E311-B8ED-005056822391 | 10008109    | Blackheath High School                                                                 |
| C06EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Willow Wood Nursery and Infant School                                                  |
| 1B17CFAF-C6AE-E311-B8ED-005056822391 | 10077902    | Butlers Court Combined School                                                          |
| B4EE667F-C7AE-E311-B8ED-005056822391 | 10001524    | Cobham Hall                                                                            |
| A3CEA8CD-C6AE-E311-B8ED-005056822391 | 10078337    | Boyne Hill CofE Infant and Nursery School                                              |
| 730A6485-C7AE-E311-B8ED-005056822391 | 10077612    | St Davids School                                                                       |
| 2E76B0C7-C6AE-E311-B8ED-005056822391 | 10078020    | Harrington Hill Primary School                                                         |
| B35E0C80-C6AE-E311-B8ED-005056822391 | NULL        | The Avenue Primary School                                                              |
| CD298AE5-C6AE-E311-B8ED-005056822391 | 10078349    | Cheadle Catholic Infant School                                                         |
| C662F591-C6AE-E311-B8ED-005056822391 | 10081363    | West Wycombe School                                                                    |
| F8096485-C7AE-E311-B8ED-005056822391 | 10015469    | Grateley House                                                                         |
| B3B40486-C6AE-E311-B8ED-005056822391 | 10074215    | Holly Trees Primary School                                                             |
| 043073F7-C6AE-E311-B8ED-005056822391 | 10017700    | Standish Community High School                                                         |
| 1BDF6303-C7AE-E311-B8ED-005056822391 | 10003885    | Leon School & Sports College                                                           |
| EEC1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Yew Tree Community School                                                              |
| 7812E69D-C6AE-E311-B8ED-005056822391 | NULL        | Occold Primary School                                                                  |
| 930EFD8B-C6AE-E311-B8ED-005056822391 | 10078955    | King's Road Primary School                                                             |
| 30CDA8CD-C6AE-E311-B8ED-005056822391 | 10075721    | St Michael's C of E School                                                             |
| 24C2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Heath Hayes County Primary School                                                      |
| 97996F6D-C7AE-E311-B8ED-005056822391 | 10007944    | Heathfield School                                                                      |
| C7E64C15-C7AE-E311-B8ED-005056822391 | 10003923    | Lilian Baylis School                                                                   |
| EC3073F7-C6AE-E311-B8ED-005056822391 | 10018670    | Guide Post Middle School                                                               |
| B68D540F-C7AE-E311-B8ED-005056822391 | 10003257    | Ilkeston School                                                                        |
| 8626A1D3-C6AE-E311-B8ED-005056822391 | 10073790    | St Margaret's C of E Primary School *                                                  |
| 7C7C99D9-C6AE-E311-B8ED-005056822391 | 10074220    | St Edmundsbury CE VA Primary School                                                    |
| 8A6BDEA3-C6AE-E311-B8ED-005056822391 | 10069563    | Shobdon Primary School                                                                 |
| 188E540F-C7AE-E311-B8ED-005056822391 | 10016549    | Mounts Bay School                                                                      |
| F16EC7B5-C6AE-E311-B8ED-005056822391 | 10076406    | Deykin Avenue Junior & Infant School                                                   |
| 872C8AE5-C6AE-E311-B8ED-005056822391 | 10072737    | St Thomas' Catholic Primary School                                                     |
| 8B0C6579-C7AE-E311-B8ED-005056822391 | NULL        | The Old Malthouse School                                                               |
| 6F3F451B-C7AE-E311-B8ED-005056822391 | 10017023    | St John Fisher Catholic High School                                                    |
| 3975B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Kingscroft Junior School                                                               |
| 07E64C15-C7AE-E311-B8ED-005056822391 | 10015169    | Clapton Girls' Technology College                                                      |
| 21B60486-C6AE-E311-B8ED-005056822391 | NULL        | Beechfield School                                                                      |
| B626A1D3-C6AE-E311-B8ED-005056822391 | 10077399    | St George's Beneficial C of E Voluntary Controlled P                                   |
| 846EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Hamstreet Primary School                                                               |
| 696FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Frederick Harrison Infant & Nursery School                                             |
| 922A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Archbishop Cranmer C of E Primary School                                               |
| 371DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Borstal Manor Community School                                                         |
| 820C6579-C7AE-E311-B8ED-005056822391 | 10016259    | The Lammas School                                                                      |
| FC355C09-C7AE-E311-B8ED-005056822391 | 10005931    | Sneyd Community School                                                                 |
| 7B2B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Philomena's RC Infant School                                                        |
| 8FED7F55-C7AE-E311-B8ED-005056822391 | 10007658    | Wrotham School                                                                         |
| 9826A1D3-C6AE-E311-B8ED-005056822391 | NULL        | The Henry Moore Primary School                                                         |
| 0AF63F21-C7AE-E311-B8ED-005056822391 | 10017454    | St Mary's RC High School                                                               |
| 906BDEA3-C6AE-E311-B8ED-005056822391 | 10069996    | Moseley Primary School                                                                 |
| 6D2A8AE5-C6AE-E311-B8ED-005056822391 | 10071636    | St Gildas RC Junior School                                                             |
| 601EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | St Mary's CP School                                                                    |
| EB69DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Ruskin Junior School                                                                   |
| DE24A1D3-C6AE-E311-B8ED-005056822391 | 10079668    | St Alphege C of E Infant School                                                        |
| 647C99D9-C6AE-E311-B8ED-005056822391 | 10076319    | Wells Central CE Junior School                                                         |
| E9DB7AF1-C6AE-E311-B8ED-005056822391 | 10015297    | Featherstone High                                                                      |
| DF605C8B-C7AE-E311-B8ED-005056822391 | 10007610    | Woodhouse Grove School                                                                 |
| 64CCA8CD-C6AE-E311-B8ED-005056822391 | 10070414    | Abberley Parochial Primary School                                                      |
| 337A589D-C7AE-E311-B8ED-005056822391 | 10014983    | Avalon School                                                                          |
| F93173F7-C6AE-E311-B8ED-005056822391 | 10018554    | West Sleekburn Middle School                                                           |
| A7BCED97-C6AE-E311-B8ED-005056822391 | NULL        | Tyning Hengrove Junior School                                                          |
| B1C0D6A9-C6AE-E311-B8ED-005056822391 | 10045835    | Falconer's Hill Academy                                                                |
| 191DB8C1-C6AE-E311-B8ED-005056822391 | 10076906    | Burleigh Primary School                                                                |
| 20CCA8CD-C6AE-E311-B8ED-005056822391 | 10078527    | Bayton Church of England Primary School                                                |
| 5564F591-C6AE-E311-B8ED-005056822391 | 10076214    | Tarleton Community Primary                                                             |
| 6C1B5791-C7AE-E311-B8ED-005056822391 | 10015929    | High Birch School                                                                      |
| 863E8F49-C7AE-E311-B8ED-005056822391 | 10006117    | St Bartholomew's School                                                                |
| D0986F6D-C7AE-E311-B8ED-005056822391 | 10018454    | Long Close Preparatory School                                                          |
| 23C6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Bessemer Park Community Infant                                                         |
| C93173F7-C6AE-E311-B8ED-005056822391 | 10018540    | Scissett Middle School                                                                 |
| 5CF63F21-C7AE-E311-B8ED-005056822391 | 10003741    | Lady Margaret Secondary School                                                         |
| 9C6F35CD-C7AE-E311-B8ED-005056822391 | NULL        | Central School of Speech and Drama                                                     |
| E30CFD8B-C6AE-E311-B8ED-005056822391 | 10073369    | Binstead Primary                                                                       |
| 48D491DF-C6AE-E311-B8ED-005056822391 | NULL        | All Saints Inter-Church Primary School                                                 |
| 7C896BFD-C6AE-E311-B8ED-005056822391 | 10003125    | Holmesdale Community School                                                            |
| BF375C09-C7AE-E311-B8ED-005056822391 | NULL        | Beauchamp Middle School                                                                |
| 888182EB-C6AE-E311-B8ED-005056822391 | NULL        | Etonbury Middle School                                                                 |
| C35D0C80-C6AE-E311-B8ED-005056822391 | 10070128    | Bedford Drive Primary                                                                  |
| DEED7F55-C7AE-E311-B8ED-005056822391 | 10016954    | The Commonweal School                                                                  |
| 1EDF6303-C7AE-E311-B8ED-005056822391 | 10017976    | Endon High School                                                                      |
| 0CDB7AF1-C6AE-E311-B8ED-005056822391 | NULL        | Swanage Middle School                                                                  |
| 86BA41BA-C4AE-E311-B8ED-005056822391 | NULL        | University Of Greenwich                                                                |
| 1FCEA8CD-C6AE-E311-B8ED-005056822391 | 10080197    | St Mary's C of E Primary School                                                        |
| 7111E69D-C6AE-E311-B8ED-005056822391 | NULL        | Newstead Primary School                                                                |
| 0A2C8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Avigdor Primary School                                                                 |
| 75D491DF-C6AE-E311-B8ED-005056822391 | NULL        | Whitefriars CE Primary School                                                          |
| FF69DEA3-C6AE-E311-B8ED-005056822391 | 10075654    | Stanbury Village School                                                                |
| F169DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Southwater Junior School                                                               |
| C31B5791-C7AE-E311-B8ED-005056822391 | 10016783    | Oakleigh Gardens School                                                                |
| C4896BFD-C6AE-E311-B8ED-005056822391 | 10005847    | Simon Balle School                                                                     |
| 9B50BF3B-C7AE-E311-B8ED-005056822391 | 10016996    | Long Field High School                                                                 |
| 3AC9BFBB-C6AE-E311-B8ED-005056822391 | 10075313    | Gayhurst Primary School                                                                |
| FFD77AF1-C6AE-E311-B8ED-005056822391 | 10018470    | Ixworth Middle School                                                                  |
| 453F8F49-C7AE-E311-B8ED-005056822391 | 10004311    | Merrill College                                                                        |
| 1C1FB8C1-C6AE-E311-B8ED-005056822391 | 10074120    | Meredith Infant School                                                                 |
| 830EFD8B-C6AE-E311-B8ED-005056822391 | 10077517    | Amberley Primary School                                                                |
| DDC7BFBB-C6AE-E311-B8ED-005056822391 | 10081367    | Wilmorton Community Primary School                                                     |
| 7AD45197-C7AE-E311-B8ED-005056822391 | 10017616    | Southbrook School                                                                      |
| 8DED667F-C7AE-E311-B8ED-005056822391 | 10018152    | Cambridge Arts and Science (Cats)                                                      |
| 9BD291DF-C6AE-E311-B8ED-005056822391 | 10068597    | St Luke's C of E Primary School                                                        |
| 0220B8C1-C6AE-E311-B8ED-005056822391 | 10076247    | Paull Primary                                                                          |
| CF298AE5-C6AE-E311-B8ED-005056822391 | 10076253    | St Mark's CE Primary School                                                            |
| A270C7B5-C6AE-E311-B8ED-005056822391 | 10075305    | Birchfields Primary School                                                             |
| 061ACFAF-C6AE-E311-B8ED-005056822391 | NULL        | Newport County Primary School                                                          |
| AAB50486-C6AE-E311-B8ED-005056822391 | 10071522    | Field End Junior School                                                                |
| F7ED667F-C7AE-E311-B8ED-005056822391 | NULL        | Lycee Francaise "Charles De Gaulle"                                                    |
| 786FC7B5-C6AE-E311-B8ED-005056822391 | 10077346    | High Green Primary School                                                              |
| A675B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Kents Hill Junior School                                                               |
| CB7A99D9-C6AE-E311-B8ED-005056822391 | 10072072    | Balderstone St Leonard's School                                                        |
| F43E451B-C7AE-E311-B8ED-005056822391 | NULL        | Princess Margaret Royal Free                                                           |
| D020B8C1-C6AE-E311-B8ED-005056822391 | 10074781    | Kinoulton Primary School                                                               |
| A2BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Riddings Infant School                                                                 |
| 4AFE7261-C7AE-E311-B8ED-005056822391 | 10001342    | Chatham Grammar School for Girls                                                       |
| 01D491DF-C6AE-E311-B8ED-005056822391 | 10074031    | Macaulay C of E School                                                                 |
| 77ED667F-C7AE-E311-B8ED-005056822391 | 10014063    | Queenswood School                                                                      |
| 747C99D9-C6AE-E311-B8ED-005056822391 | 10070397    | Broadwas C of E Primary School                                                         |
| 8A0C6579-C7AE-E311-B8ED-005056822391 | 10018605    | The Hall School                                                                        |
| DD64F591-C6AE-E311-B8ED-005056822391 | NULL        | Northwood Primary School                                                               |
| F274B0C7-C6AE-E311-B8ED-005056822391 | 10069168    | Kilmorie Primary School                                                                |
| 592A8AE5-C6AE-E311-B8ED-005056822391 | 10079501    | St Winefride's Catholic Primary School                                                 |
| FAE64C15-C7AE-E311-B8ED-005056822391 | 10017073    | Heartlands High School                                                                 |
| 236FC7B5-C6AE-E311-B8ED-005056822391 | 10070810    | John Bunyan Infant School and Nursery                                                  |
| A9C0D6A9-C6AE-E311-B8ED-005056822391 | 10078780    | Kingsfield First School                                                                |
| 523273F7-C6AE-E311-B8ED-005056822391 | NULL        | Guilsborough School                                                                    |
| 7C7A589D-C7AE-E311-B8ED-005056822391 | 10015072    | Breakspeare School                                                                     |
| 756ADEA3-C6AE-E311-B8ED-005056822391 | 10080139    | Woodrow First School*                                                                  |
| 591B5791-C7AE-E311-B8ED-005056822391 | 10015925    | Henry Tyndale School                                                                   |
| 0268DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Brookside CP School                                                                    |
| 4D536A73-C7AE-E311-B8ED-005056822391 | NULL        | Mount Carmel Preparatory School                                                        |
| 71D491DF-C6AE-E311-B8ED-005056822391 | 10070166    | St Margaret Clitherow Roman Catholic Primary Schoo                                     |
| A5996F6D-C7AE-E311-B8ED-005056822391 | 10014005    | The Priory                                                                             |
| E190540F-C7AE-E311-B8ED-005056822391 | 10015054    | Burnholme Community College                                                            |
| C7BBED97-C6AE-E311-B8ED-005056822391 | 10078376    | New York Primary School                                                                |
| 4F298AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Filumena's Primary School                                                           |
| 1390540F-C7AE-E311-B8ED-005056822391 | 10006498    | Tapton School                                                                          |
| B71EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Newbold Riverside Primary School                                                       |
| DB63F591-C6AE-E311-B8ED-005056822391 | 10072879    | Bishop Stopford's School                                                               |
| 9220B8C1-C6AE-E311-B8ED-005056822391 | 10076956    | Fairfields Primary School                                                              |
| 3119CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Water Hall Combined School                                                             |
| A871C7B5-C6AE-E311-B8ED-005056822391 | 10077157    | Highworth Combined School and Nusery                                                   |
| 2925A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Elworth C of E Primary School                                                          |
| 9D355C09-C7AE-E311-B8ED-005056822391 | 10015321    | Cowplain Community School                                                              |
| 127C99D9-C6AE-E311-B8ED-005056822391 | NULL        | All Saints Primary                                                                     |
| BA5E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Larkswood Infants School                                                               |
| D8298AE5-C6AE-E311-B8ED-005056822391 | 10071646    | St Edmund's RC Primary School                                                          |
| ADED7F55-C7AE-E311-B8ED-005056822391 | 10005202    | Preston Manor High School                                                              |
| 377A589D-C7AE-E311-B8ED-005056822391 | 10016338    | Kingshill School                                                                       |
| E8C8BFBB-C6AE-E311-B8ED-005056822391 | 10069170    | Robert Blair Primary School                                                            |
| 7C7D7D23-B4A5-EF11-B8E8-000D3AB15D9F | NULL        | Hampshire Education Authority                                                          |
| 94C7BFBB-C6AE-E311-B8ED-005056822391 | 10071204    | Burnwood Primary School                                                                |
| CABFD6A9-C6AE-E311-B8ED-005056822391 | 10077264    | Little Eaton Primary School                                                            |
| DA1DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Bybrook Junior School                                                                  |
| CE10E69D-C6AE-E311-B8ED-005056822391 | NULL        | Offmore First School                                                                   |
| 815C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Blackheath Bluecoat School                                                             |
| 67D150A3-C7AE-E311-B8ED-005056822391 | 10017081    | Priestley Smith                                                                        |
| 121B7067-C7AE-E311-B8ED-005056822391 | 10016876    | The Osborne School                                                                     |
| FF7C99D9-C6AE-E311-B8ED-005056822391 | 10078882    | St Bartholomew's CE Primary School                                                     |
| DFED667F-C7AE-E311-B8ED-005056822391 | 10015788    | Waterloo Lodge                                                                         |
| 81876BFD-C6AE-E311-B8ED-005056822391 | 10008601    | Witchford Village College                                                              |
| 26C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Aston Tower Community Primary School                                                   |
| 4FD491DF-C6AE-E311-B8ED-005056822391 | 10075995    | Sacred Heart Primary                                                                   |
| D368DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Aughton Primary School                                                                 |
| B5ED667F-C7AE-E311-B8ED-005056822391 | 10017332    | Seaford College                                                                        |
| 4DBAED97-C6AE-E311-B8ED-005056822391 | 10077460    | Western Road Primary School                                                            |
| 2877B0C7-C6AE-E311-B8ED-005056822391 | 10071375    | Bywell Church of England Voluntary Controlled Junior School                            |
| B424A1D3-C6AE-E311-B8ED-005056822391 | 10079278    | Weston Hills C of E Primary School                                                     |
| 1BD291DF-C6AE-E311-B8ED-005056822391 | NULL        | St Peter's RC Primary School                                                           |
| DE6EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Lingwood First & Nursery                                                               |
| 6BD45197-C7AE-E311-B8ED-005056822391 | 10015027    | Belvue School                                                                          |
| DFF53F21-C7AE-E311-B8ED-005056822391 | 10000734    | Bishops Walsh Catholic School                                                          |
| 28E06303-C7AE-E311-B8ED-005056822391 | 10018101    | Langleywood School                                                                     |
| 9E2D8AE5-C6AE-E311-B8ED-005056822391 | 10076056    | St Michael's RC Primary School                                                         |
| 1D9A6F6D-C7AE-E311-B8ED-005056822391 | 10015685    | Downe House School                                                                     |
| 4D859443-C7AE-E311-B8ED-005056822391 | 10009458    | Bassingbourn Village College                                                           |
| BBB50486-C6AE-E311-B8ED-005056822391 | 10069364    | Lockwood Primay                                                                        |
| C826A1D3-C6AE-E311-B8ED-005056822391 | 10078856    | Embsay CE Primary School                                                               |
| 2B7E99D9-C6AE-E311-B8ED-005056822391 | 10075463    | Heene C of E First School                                                              |
| 6AC6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Featherby Junior School                                                                |
| 18A87A5B-C7AE-E311-B8ED-005056822391 | 10000856    | Branston Community College                                                             |
| 4FD250A3-C7AE-E311-B8ED-005056822391 | 10016381    | Samuel Rhodes School                                                                   |
| 8CBAED97-C6AE-E311-B8ED-005056822391 | 10073201    | Peters Hill Primary School                                                             |
| 937A589D-C7AE-E311-B8ED-005056822391 | 10016028    | William Harrison School                                                                |
| 1FE84C15-C7AE-E311-B8ED-005056822391 | 10005808    | Sherwood Hall School and Sixth Form College                                            |
| 2EC0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | The Clara Grant Primary School                                                         |
| 8C90540F-C7AE-E311-B8ED-005056822391 | 10018866    | Settle Middle School                                                                   |
| 56ED667F-C7AE-E311-B8ED-005056822391 | 10014056    | St Dominic's Priory School                                                             |
| 2CEE7F55-C7AE-E311-B8ED-005056822391 | 10000221    | St John Rigby School                                                                   |
| 28E84C15-C7AE-E311-B8ED-005056822391 | NULL        | The Queen Elizabeth's School                                                           |
| B1C8BFBB-C6AE-E311-B8ED-005056822391 | 10079734    | Foxes Piece Combined School                                                            |
| D22A8AE5-C6AE-E311-B8ED-005056822391 | 10074025    | St Mary's Lewisham Church of England Primary School                                    |
| 4DE84C15-C7AE-E311-B8ED-005056822391 | NULL        | The Prittlewell School                                                                 |
| 327E99D9-C6AE-E311-B8ED-005056822391 | 10079318    | Christ Church C of E Primary School                                                    |
| B969DEA3-C6AE-E311-B8ED-005056822391 | 10077086    | Liss Junior School                                                                     |
| 9370C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Wingham County Primary School                                                          |
| CA64F591-C6AE-E311-B8ED-005056822391 | 10072107    | Home Farm Primary School                                                               |
| 7D76B0C7-C6AE-E311-B8ED-005056822391 | 10075309    | Arnhem Wharf Primary School                                                            |
| E7B50486-C6AE-E311-B8ED-005056822391 | NULL        | Fairholme School                                                                       |
| 1E68DEA3-C6AE-E311-B8ED-005056822391 | 10043201    | Freeman Community Primary School                                                       |
| 5170C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Morley Newlands Primary School                                                         |
| A5FF7261-C7AE-E311-B8ED-005056822391 | NULL        | St Nicholas School                                                                     |
| 8C20B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Hallgate Infants School                                                                |
| 38EE667F-C7AE-E311-B8ED-005056822391 | 10002373    | Exeter School                                                                          |
| 6770C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Warden House Primary School                                                            |
| FF0BFD8B-C6AE-E311-B8ED-005056822391 | 10069157    | Coombe Hill Junior School                                                              |
| E91DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Raynehurst Infant School & Nursery                                                     |
| C916CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Chiltern Primary School                                                                |
| DE18CFAF-C6AE-E311-B8ED-005056822391 | 10070684    | Glebe Junior School                                                                    |
| 4E5C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Adderley Nursery School                                                                |
| 5220B8C1-C6AE-E311-B8ED-005056822391 | 10079067    | Fernhurst Junior School                                                                |
| D6C6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Bitterne Park Infant School                                                            |
| AD6EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Greenfields Primary School                                                             |
| F861F591-C6AE-E311-B8ED-005056822391 | 10068969    | Beenham Primary School                                                                 |
| B0DE6303-C7AE-E311-B8ED-005056822391 | 10015814    | Highworth Warnford School                                                              |
| 4A615C8B-C7AE-E311-B8ED-005056822391 | NULL        | St Francis' College                                                                    |
| 3EC1D6A9-C6AE-E311-B8ED-005056822391 | 10076204    | Cedars County Infants                                                                  |
| 556BDEA3-C6AE-E311-B8ED-005056822391 | 10073197    | Roberts Primary                                                                        |
| 21625C8B-C7AE-E311-B8ED-005056822391 | 10018723    | Hoe Bridge School                                                                      |
| A8C2D6A9-C6AE-E311-B8ED-005056822391 | 10072986    | Holme Slack CP. School                                                                 |
| 2619CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Franciscan Primary School                                                              |
| B7C5BFBB-C6AE-E311-B8ED-005056822391 | NULL        | The Oaktree School                                                                     |
| 390EFD8B-C6AE-E311-B8ED-005056822391 | 10071929    | South Haringey Infants                                                                 |
| 49C2D6A9-C6AE-E311-B8ED-005056822391 | 10072546    | Seathorne C P School                                                                   |
| F8C6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Roche Primary School                                                                   |
| 29CCA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St James CE Junior School                                                              |
| 550DFD8B-C6AE-E311-B8ED-005056822391 | 10080695    | Ashchurch County Primary                                                               |
| DCC2D6A9-C6AE-E311-B8ED-005056822391 | 10077901    | Chartridge County Combined School                                                      |
| CBB60486-C6AE-E311-B8ED-005056822391 | NULL        | Scargill Infant School                                                                 |
| D4BFD6A9-C6AE-E311-B8ED-005056822391 | 10079577    | Woodlands Primary School                                                               |
| 5376B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Southrise Primary School                                                               |
| C6C2D6A9-C6AE-E311-B8ED-005056822391 | 10073899    | The Meadows Primary School                                                             |
| A8D291DF-C6AE-E311-B8ED-005056822391 | NULL        | St John's C of E Primary School                                                        |
| 2BF83F21-C7AE-E311-B8ED-005056822391 | NULL        | Magdalen Primary School                                                                |
| DE2B8AE5-C6AE-E311-B8ED-005056822391 | 10074729    | Holy Trinity JMI School                                                                |
| AFD150A3-C7AE-E311-B8ED-005056822391 | 10015591    | Greenside School                                                                       |
| 598382EB-C6AE-E311-B8ED-005056822391 | 10016267    | The Langley School                                                                     |
| 94D591DF-C6AE-E311-B8ED-005056822391 | NULL        | St Werburgh's Primary School                                                           |
| 3A2B8AE5-C6AE-E311-B8ED-005056822391 | 10078864    | Bryning With Warton St Paul's Church of England Pr                                     |
| E47A589D-C7AE-E311-B8ED-005056822391 | 10017165    | Baycroft School                                                                        |
| 8EBCED97-C6AE-E311-B8ED-005056822391 | 10075526    | Muswell Hill Primary School                                                            |
| 723D451B-C7AE-E311-B8ED-005056822391 | 10006222    | St Mary's Catholic High School                                                         |
| 9F6EC7B5-C6AE-E311-B8ED-005056822391 | 10077240    | Westfield First School                                                                 |
| AB0AFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Mayplace Primary School                                                                |
| 223E451B-C7AE-E311-B8ED-005056822391 | 10016861    | Manor Church of England School                                                         |
| 06056786-C8AE-E311-B8ED-005056822391 | 10007048    | Trinity College                                                                        |
| A3C5BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Viewley Hill Primary                                                                   |
| 6EF83F21-C7AE-E311-B8ED-005056822391 | 10005638    | St Philip Howard Catholic High School                                                  |
| 0D19CFAF-C6AE-E311-B8ED-005056822391 | 10073275    | West Down School                                                                       |
| CC2B8AE5-C6AE-E311-B8ED-005056822391 | 10076872    | St Joseph The Worker RC Primary School                                                 |
| 8DDF6303-C7AE-E311-B8ED-005056822391 | 10015803    | Hethersett High School                                                                 |
| 1D1ACFAF-C6AE-E311-B8ED-005056822391 | 10079856    | William Austin Junior School                                                           |
| CA615C8B-C7AE-E311-B8ED-005056822391 | NULL        | Centre Academy                                                                         |
| B6BBED97-C6AE-E311-B8ED-005056822391 | 10079007    | Fiddlers Lane Community Primary School                                                 |
| 3275B0C7-C6AE-E311-B8ED-005056822391 | 10078016    | Benthal Junior School                                                                  |
| 52CB372D-C7AE-E311-B8ED-005056822391 | NULL        | Leverton School                                                                        |
| 29D391DF-C6AE-E311-B8ED-005056822391 | NULL        | St Joseph's Catholic Primary Sch.                                                      |
| 57F83F21-C7AE-E311-B8ED-005056822391 | 10006192    | St John Fisher Catholic High School                                                    |
| F8886BFD-C6AE-E311-B8ED-005056822391 | 10002855    | Hall Cross Academy                                                                     |
| 27C2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Sandiway Primary School                                                                |
| 6A6EC7B5-C6AE-E311-B8ED-005056822391 | 10046141    | Hillborough Junior School                                                              |
| DF7D99D9-C6AE-E311-B8ED-005056822391 | NULL        | Our Lady of Walsingham Junior School                                                   |
| 702B8AE5-C6AE-E311-B8ED-005056822391 | 10076010    | St Peters RC Primary                                                                   |
| AF70C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Heathfield Primary                                                                     |
| 53D391DF-C6AE-E311-B8ED-005056822391 | 10070229    | St Finians RC Primary                                                                  |
| D31DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Dunmore Junior School                                                                  |
| DDA67A5B-C7AE-E311-B8ED-005056822391 | 10006111    | St Anne's Convent School                                                               |
| 2EC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Signhills Junior School                                                                |
| 2313E69D-C6AE-E311-B8ED-005056822391 | NULL        | Whiteknights Primary School                                                            |
| 95D491DF-C6AE-E311-B8ED-005056822391 | 10070211    | Sacred Heart RC Primary School                                                         |
| AC74B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Abbotsweld Primary School                                                              |
| E8BFD6A9-C6AE-E311-B8ED-005056822391 | 10075863    | Glynne Primary School                                                                  |
| 1B7A589D-C7AE-E311-B8ED-005056822391 | 10015608    | Harford Manor School                                                                   |
| F3F63F21-C7AE-E311-B8ED-005056822391 | NULL        | Agnes Stewart Church of England High School                                            |
| 04B40486-C6AE-E311-B8ED-005056822391 | NULL        | Northgate Primary School                                                               |
| 5E7C99D9-C6AE-E311-B8ED-005056822391 | 10075762    | Churchill Church of England Primary School                                             |
| F468DEA3-C6AE-E311-B8ED-005056822391 | 10075657    | Holycroft Primary School                                                               |
| 83D97AF1-C6AE-E311-B8ED-005056822391 | 10000515    | Banbury School                                                                         |
| 01E64C15-C7AE-E311-B8ED-005056822391 | 10006757    | Morton School                                                                          |
| 3A12E69D-C6AE-E311-B8ED-005056822391 | NULL        | Worsbrough Bank End Primary School                                                     |
| 3E0EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | The Amy Johnson Primary School                                                         |
| 4C1B5791-C7AE-E311-B8ED-005056822391 | 10015529    | Durants School                                                                         |
| 600CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Whitehill Community Academy                                                            |
| 8A62F591-C6AE-E311-B8ED-005056822391 | 10072146    | Brede Primary School                                                                   |
| 1B26A1D3-C6AE-E311-B8ED-005056822391 | 10079243    | St Nicholas Priory Middle School                                                       |
| 160D6579-C7AE-E311-B8ED-005056822391 | 10016548    | Repton School                                                                          |
| 81D591DF-C6AE-E311-B8ED-005056822391 | 10079322    | St Georges Centrel CE School                                                           |
| AA76B0C7-C6AE-E311-B8ED-005056822391 | 10069274    | Forest Fields Primary and Nursery School                                               |
| 4EF83F21-C7AE-E311-B8ED-005056822391 | NULL        | Hamilton Primary School                                                                |
| B019CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Waddington All Saints Primary School                                                   |
| 75BBED97-C6AE-E311-B8ED-005056822391 | NULL        | Outwood Primary Academy Ledger Lane                                                    |
| 3A385C09-C7AE-E311-B8ED-005056822391 | 10005961    | Somervale School Specialist Media Arts College                                         |
| 373073F7-C6AE-E311-B8ED-005056822391 | 10015038    | Acton High School                                                                      |
| 53395C09-C7AE-E311-B8ED-005056822391 | 10016103    | The John Ruskin School                                                                 |
| 8E7C99D9-C6AE-E311-B8ED-005056822391 | 10078726    | Holy Family RC Primary                                                                 |
| AF1FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Leigham Infant School                                                                  |
| 6B6ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Moor Green Infant School                                                               |
| E40A6485-C7AE-E311-B8ED-005056822391 | 10015002    | Abigdon School                                                                         |
| B8ED7F55-C7AE-E311-B8ED-005056822391 | 10006450    | Swaterleys School                                                                      |
| 2124A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Southbroom County Infants School                                                       |
| 5412E69D-C6AE-E311-B8ED-005056822391 | NULL        | Mandeville Primary School                                                              |
| 5ADF6303-C7AE-E311-B8ED-005056822391 | 10000870    | Bretton Woods School                                                                   |
| 793073F7-C6AE-E311-B8ED-005056822391 | 10010067    | Irlam and Cadishead Community High School                                              |
| 0EF63F21-C7AE-E311-B8ED-005056822391 | 10018046    | Cardinal Newman Catholic High School                                                   |
| 3A11E69D-C6AE-E311-B8ED-005056822391 | NULL        | Robert Shaw Primary School                                                             |
| CB0BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Alexandra First School                                                                 |
| 0EBBED97-C6AE-E311-B8ED-005056822391 | NULL        | Newlands Junior School                                                                 |
| 957D99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Felix RC Primary School                                                             |
| A9CEA8CD-C6AE-E311-B8ED-005056822391 | 10068641    | Burley and Woodhead CofE Primary School                                                |
| B2CBA8CD-C6AE-E311-B8ED-005056822391 | 10078649    | St James Church of England School                                                      |
| 58F63F21-C7AE-E311-B8ED-005056822391 | 10016879    | Our Lady and St Bede's RC School                                                       |
| CED150A3-C7AE-E311-B8ED-005056822391 | 10016314    | Moor Hey School                                                                        |
| 5D19CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Bobbing County Primary School                                                          |
| 875D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Leslie Manser Primary                                                                  |
| 558E540F-C7AE-E311-B8ED-005056822391 | 10000472    | Axton Chase                                                                            |
| D6D291DF-C6AE-E311-B8ED-005056822391 | 10074720    | Stepney Greencoat Church of England Primary School                                     |
| 78CCA8CD-C6AE-E311-B8ED-005056822391 | 10073982    | Parish Church CE Junior School                                                         |
| 026FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Copperfield Middle School                                                              |
| AE77B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Greenfield C of E VC Lower School                                                      |
| B4D150A3-C7AE-E311-B8ED-005056822391 | 10078211    | Southfield MLD School                                                                  |
| C0B30486-C6AE-E311-B8ED-005056822391 | NULL        | Barclay Junior School                                                                  |
| 02ED7F55-C7AE-E311-B8ED-005056822391 | 10016964    | The Archbishop Lanfranc                                                                |
| 5AD250A3-C7AE-E311-B8ED-005056822391 | 10016043    | Jack Taylor School                                                                     |
| 6A23A1D3-C6AE-E311-B8ED-005056822391 | 10075776    | Christ Church Hanham CofE Primary School                                               |
| AAD591DF-C6AE-E311-B8ED-005056822391 | 10056725    | Soho Parish C of E Primary School                                                      |
| FB615C8B-C7AE-E311-B8ED-005056822391 | 10077624    | Prospect House School                                                                  |
| 1CBBED97-C6AE-E311-B8ED-005056822391 | 10069319    | Benton Dene Primary School                                                             |
| 3A1DB8C1-C6AE-E311-B8ED-005056822391 | 10079530    | Belswains Primary School                                                               |
| DE1A7067-C7AE-E311-B8ED-005056822391 | 10007182    | Upton School FCJ                                                                       |
| AF298AE5-C6AE-E311-B8ED-005056822391 | 10076743    | St Thomas' Primary CE (Aided) School                                                   |
| 8D0DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Corpusty Primary School                                                                |
| 1C13E69D-C6AE-E311-B8ED-005056822391 | 10073239    | Polegate Primary School                                                                |
| 90DE6303-C7AE-E311-B8ED-005056822391 | 10006879    | Therfield School                                                                       |
| 523E8F49-C7AE-E311-B8ED-005056822391 | 10015807    | Fairfield High School for Girls                                                        |
| 223073F7-C6AE-E311-B8ED-005056822391 | 10018696    | Lockyer's Middle School                                                                |
| 11C9BFBB-C6AE-E311-B8ED-005056822391 | 10075333    | Parsonage Farm Nursery & Infant School                                                 |
| D07035CD-C7AE-E311-B8ED-005056822391 | NULL        | London South Bank University                                                           |
| C6385C09-C7AE-E311-B8ED-005056822391 | 10005862    | Sir James Smiths Community School                                                      |
| 12D591DF-C6AE-E311-B8ED-005056822391 | 10073452    | Mawdesley St Peters Church of England Primary Scho                                     |
| 8811E69D-C6AE-E311-B8ED-005056822391 | 10075633    | Ash Green Community Primary School                                                     |
| 740BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Parkhurst Infants                                                                      |
| 41BCED97-C6AE-E311-B8ED-005056822391 | 10079849    | John Rankin Junior School                                                              |
| AF0C6579-C7AE-E311-B8ED-005056822391 | 10008595    | Westonbirt Girls School                                                                |
| A13D451B-C7AE-E311-B8ED-005056822391 | NULL        | Alwood Church of England School                                                        |
| D32C8AE5-C6AE-E311-B8ED-005056822391 | 10080352    | Anderton St Josephs RC School                                                          |
| C0D45197-C7AE-E311-B8ED-005056822391 | NULL        | Whitworth Shcool                                                                       |
| 0BC7BFBB-C6AE-E311-B8ED-005056822391 | 10070008    | Wylde Green Primary School                                                             |
| AE13E69D-C6AE-E311-B8ED-005056822391 | 10074597    | Chisenhale Primary School                                                              |
| 61F63F21-C7AE-E311-B8ED-005056822391 | 10017155    | St Patrick's RC Comprehensive School                                                   |
| DCE74C15-C7AE-E311-B8ED-005056822391 | 10016999    | The Marches School                                                                     |
| D819CFAF-C6AE-E311-B8ED-005056822391 | 10079762    | The Meads Primary                                                                      |
| D875B0C7-C6AE-E311-B8ED-005056822391 | NULL        | East Tilbury Inf.School                                                                |
| B0D45197-C7AE-E311-B8ED-005056822391 | 10017680    | St Elizabeth School                                                                    |
| A513E69D-C6AE-E311-B8ED-005056822391 | NULL        | Bredbury Green Primary School                                                          |
| FA0CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Canterbury Cross Primary School                                                        |
| 20F73F21-C7AE-E311-B8ED-005056822391 | 10018780    | St Benedict's RC Middle School                                                         |
| 7224A1D3-C6AE-E311-B8ED-005056822391 | NULL        | James Bradfield V C Community Primary School                                           |
| DB95874F-C7AE-E311-B8ED-005056822391 | 10005209    | Prince Henry's High School                                                             |
| 466EC7B5-C6AE-E311-B8ED-005056822391 | 10074660    | Henry Cavendish Primary School                                                         |
| 6777B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Knowle CE Primary School                                                               |
| 0F1FB8C1-C6AE-E311-B8ED-005056822391 | 10081368    | Mount Street Primary School                                                            |
| 7B0BFD8B-C6AE-E311-B8ED-005056822391 | 10071519    | Minet Infants                                                                          |
| AD365C09-C7AE-E311-B8ED-005056822391 | NULL        | Langbaurgh Secondary School                                                            |
| 00CFA8CD-C6AE-E311-B8ED-005056822391 | 10079341    | St Michael's CE Primary School                                                         |
| 6AE64C15-C7AE-E311-B8ED-005056822391 | 10006302    | Stainburn School                                                                       |
| 271DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Farnborough Grange Nursery/Infant                                                      |
| C85E0C80-C6AE-E311-B8ED-005056822391 | 10075647    | Ferney Lee Primary School                                                              |
| B871C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Bracken Lane Primary School                                                            |
| 04B70486-C6AE-E311-B8ED-005056822391 | 10073163    | Leamore Primary School                                                                 |
| FBCDA8CD-C6AE-E311-B8ED-005056822391 | 10069831    | Theale C.E. Primary School                                                             |
| F10BFD8B-C6AE-E311-B8ED-005056822391 | 10075631    | Spring Grove Junior Infant and Nursery School                                          |
| 651DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Meriden Primary                                                                        |
| 6B17CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Lea Junior School                                                                      |
| DB70C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Horn Park Junior School                                                                |
| 9A2C8AE5-C6AE-E311-B8ED-005056822391 | 10072667    | St Mary's RC Primary School                                                            |
| 2A0CFD8B-C6AE-E311-B8ED-005056822391 | 10071520    | Lady Banks Junior School                                                               |
| E08182EB-C6AE-E311-B8ED-005056822391 | NULL        | Kingsway High School                                                                   |
| B41DB8C1-C6AE-E311-B8ED-005056822391 | 10069909    | Balshaw Lane Community Primary                                                         |
| 5CB60486-C6AE-E311-B8ED-005056822391 | NULL        | Bordon Junior School                                                                   |
| A8D45197-C7AE-E311-B8ED-005056822391 | NULL        | Weatherfield School                                                                    |
| 6325A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Standlake C of E Primary School                                                        |
| 9B298AE5-C6AE-E311-B8ED-005056822391 | 10076017    | St Josephs School                                                                      |
| 75B30486-C6AE-E311-B8ED-005056822391 | 10070138    | Linacre Primary School                                                                 |
| 433E451B-C7AE-E311-B8ED-005056822391 | 10018617    | St Louis Catholic Middle                                                               |
| 9D26A1D3-C6AE-E311-B8ED-005056822391 | 10078855    | Kirk Hammerton Primary School                                                          |
| B763F591-C6AE-E311-B8ED-005056822391 | NULL        | Albert Pye Community Primary                                                           |
| 35DF6303-C7AE-E311-B8ED-005056822391 | 10008748    | Brynteg Comprehensive School                                                           |
| 88CCA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Rayleigh Primary School                                                                |
| 44C8BFBB-C6AE-E311-B8ED-005056822391 | 10069306    | Jacksdale Primary School                                                               |
| D023A1D3-C6AE-E311-B8ED-005056822391 | 10070342    | The Humberston C of E Primary School                                                   |
| D012E69D-C6AE-E311-B8ED-005056822391 | NULL        | West Town Lane Junior School                                                           |
| D276B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Eastglade Primary School                                                               |
| BC6EC7B5-C6AE-E311-B8ED-005056822391 | 10073078    | Little Aston Primary                                                                   |
| 8D70C7B5-C6AE-E311-B8ED-005056822391 | 10079557    | Greenway First School                                                                  |
| 4EC82DD3-C7AE-E311-B8ED-005056822391 | 193         | Stranmillis University College                                                         |
| DBCCA8CD-C6AE-E311-B8ED-005056822391 | 10071373    | Staincliffe Church of England Voluntary Controlled Junior School                       |
| DE19CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Minister Primary School                                                                |
| B20BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Stile Common Infant and Nursery School                                                 |
| C03073F7-C6AE-E311-B8ED-005056822391 | 10004984    | Park View Academy                                                                      |
| 1A365C09-C7AE-E311-B8ED-005056822391 | 10005381    | Range High School                                                                      |
| 1A056786-C8AE-E311-B8ED-005056822391 | NULL        | University Of East London                                                              |
| 786BDEA3-C6AE-E311-B8ED-005056822391 | NULL        | Beech Primary School                                                                   |
| F43073F7-C6AE-E311-B8ED-005056822391 | 10016858    | Manhood Community College                                                              |
| 581A7067-C7AE-E311-B8ED-005056822391 | NULL        | Rochester Grammar School for Girls                                                     |
| C5615C8B-C7AE-E311-B8ED-005056822391 | NULL        | Portsmouth High School G.D.S.T.                                                        |
| 0424A1D3-C6AE-E311-B8ED-005056822391 | 10077356    | Scunthorpe CE Primary School                                                           |
| 568D540F-C7AE-E311-B8ED-005056822391 | NULL        | Chapel House Middle School                                                             |
| 4524A1D3-C6AE-E311-B8ED-005056822391 | NULL        | St Marks C of E Controlled Primary School                                              |
| F2526A73-C7AE-E311-B8ED-005056822391 | 10075154    | Wimbledon College Prep                                                                 |
| 09B60486-C6AE-E311-B8ED-005056822391 | NULL        | Rowlett Community Primary School                                                       |
| C8298AE5-C6AE-E311-B8ED-005056822391 | NULL        | All Saints Catholic Junior School                                                      |
| 3FE84C15-C7AE-E311-B8ED-005056822391 | 10000642    | Benfield School                                                                        |
| 218482EB-C6AE-E311-B8ED-005056822391 | 10006308    | Stanley Park High School                                                               |
| 3CE16303-C7AE-E311-B8ED-005056822391 | NULL        | Manor High School                                                                      |
| ABCBA8CD-C6AE-E311-B8ED-005056822391 | 10074284    | Church of the Ascension CE Primary School                                              |
| 067A589D-C7AE-E311-B8ED-005056822391 | 10017382    | Severndale                                                                             |
| E6DA7AF1-C6AE-E311-B8ED-005056822391 | 10006036    | Southgate School                                                                       |
| 6F6FC7B5-C6AE-E311-B8ED-005056822391 | 10070084    | Crab Lane Primary School                                                               |
| 1777B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Peterswood County Infant School                                                        |
| A90DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Monega Primary School                                                                  |
| 84DB7AF1-C6AE-E311-B8ED-005056822391 | 10000587    | Beacon Community College                                                               |
| 4FB50486-C6AE-E311-B8ED-005056822391 | 10079600    | Holdbrook Junior Mixed and Infants                                                     |
| A82D8AE5-C6AE-E311-B8ED-005056822391 | 10075946    | St Peter's Church of England Primary School, Leeds                                     |
| BF0BFD8B-C6AE-E311-B8ED-005056822391 | 10080069    | Fleecefield Primary School                                                             |
| D4C5BFBB-C6AE-E311-B8ED-005056822391 | 10070809    | Avelly County Primary School                                                           |
| 7ACEA8CD-C6AE-E311-B8ED-005056822391 | 10074899    | St Luke's C of E Primary School                                                        |
| 4212E69D-C6AE-E311-B8ED-005056822391 | 10072566    | Gomersal Primary School                                                                |
| 295F0C80-C6AE-E311-B8ED-005056822391 | NULL        | Deeplish Community Primary School                                                      |
| E87A99D9-C6AE-E311-B8ED-005056822391 | 10079487    | St Josephs Catholic Junior School                                                      |
| 87EE667F-C7AE-E311-B8ED-005056822391 | 10008244    | Frensham Heights School                                                                |
| 2D3E451B-C7AE-E311-B8ED-005056822391 | 10008838    | St Martins Catholic School                                                             |
| 41E06303-C7AE-E311-B8ED-005056822391 | 10004912    | Oulder Hill School                                                                     |
| A3D291DF-C6AE-E311-B8ED-005056822391 | 10075031    | St Brigid's Catholic Primary School                                                    |
| 287D99D9-C6AE-E311-B8ED-005056822391 | 10076329    | Coalbrookdale and Ironbridge CE Primary                                                |
| 1DF83F21-C7AE-E311-B8ED-005056822391 | NULL        | St John Fisher Catholic Primary School                                                 |
| 4675B0C7-C6AE-E311-B8ED-005056822391 | 10078040    | Cardwell Primary School                                                                |
| 1275B0C7-C6AE-E311-B8ED-005056822391 | 10077192    | Bosmere CP School                                                                      |
| 8D0C6579-C7AE-E311-B8ED-005056822391 | 10015352    | Coxlease School                                                                        |
| 25056786-C8AE-E311-B8ED-005056822391 | NULL        | University Of Leeds                                                                    |
| 80D291DF-C6AE-E311-B8ED-005056822391 | 10073412    | Tydd St Mary CE Primary School                                                         |
| 8924A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Weston Favell CE VA Primary School                                                     |
| 6E69DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Roecroft Lower School                                                                  |
| EC5E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Bovingham Primary School                                                               |
| 2CB03A27-C7AE-E311-B8ED-005056822391 | 10068634    | St Chad's CofE (VA) Primary School                                                     |
| D4CB372D-C7AE-E311-B8ED-005056822391 | 10001485    | Claremont High School                                                                  |
| 730D6579-C7AE-E311-B8ED-005056822391 | 10005424    | Reeds School                                                                           |
| C6A67A5B-C7AE-E311-B8ED-005056822391 | 10000535    | Barnhill Community High                                                                |
| A5E74C15-C7AE-E311-B8ED-005056822391 | 10006676    | The Grove School                                                                       |
| 7D996F6D-C7AE-E311-B8ED-005056822391 | 10007448    | Westholme School                                                                       |
| E9866BFD-C6AE-E311-B8ED-005056822391 | 10015922    | William Sharp School                                                                   |
| 7B7D99D9-C6AE-E311-B8ED-005056822391 | 10075941    | Ossett Holy Trinity CofE VA Primary School                                             |
| 6526A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Macclesfield St Barnabas Primary School                                                |
| 72D591DF-C6AE-E311-B8ED-005056822391 | 10071686    | St Edward's School                                                                     |
| 3BBAED97-C6AE-E311-B8ED-005056822391 | 10079010    | Bridgewater Primary School                                                             |
| B01DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Rush Common School                                                                     |
| 9DCEA8CD-C6AE-E311-B8ED-005056822391 | 10079653    | Selston C of E Infant & Nursery School                                                 |
| E818CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Ings Farm Primary                                                                      |
| 2D90540F-C7AE-E311-B8ED-005056822391 | 10015900    | Holyhead School                                                                        |
| 1F23A1D3-C6AE-E311-B8ED-005056822391 | 10078541    | Horndean Junior School                                                                 |
| E6526A73-C7AE-E311-B8ED-005056822391 | 10018522    | Quainton Hall School                                                                   |
| 69B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Surrey Square Junior School                                                            |
| 31EE667F-C7AE-E311-B8ED-005056822391 | 10008602    | Withington Girls School                                                                |
| F9BAED97-C6AE-E311-B8ED-005056822391 | 10079784    | Elm Grove First School                                                                 |
| 56B60486-C6AE-E311-B8ED-005056822391 | 10069392    | Simonside Primary School                                                               |
| 11D491DF-C6AE-E311-B8ED-005056822391 | 10071695    | Holy Ghost RC Primary                                                                  |
| 41876BFD-C6AE-E311-B8ED-005056822391 | 10003046    | Hessle High School                                                                     |
| B8615C8B-C7AE-E311-B8ED-005056822391 | 10017380    | Ripley Court School                                                                    |
| 9A13E69D-C6AE-E311-B8ED-005056822391 | NULL        | Gorton Mount Primary School                                                            |
| F1C8BFBB-C6AE-E311-B8ED-005056822391 | 10071179    | Curdworth Primary School                                                               |
| E63073F7-C6AE-E311-B8ED-005056822391 | 10014906    | Blurton High School                                                                    |
| B3298AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Judes CE Primary School                                                             |
| A10AFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Whybridge Junior School                                                                |
| 6AC0D6A9-C6AE-E311-B8ED-005056822391 | 10071253    | Ealing Primary School                                                                  |
| 6369DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Upavon Primary School                                                                  |
| C4B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Plumberow Primary School                                                               |
| 58886BFD-C6AE-E311-B8ED-005056822391 | 10014964    | Aveling Park School                                                                    |
| 98BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Southgate First School                                                                 |
| 08859443-C7AE-E311-B8ED-005056822391 | 10006262    | St Thomas More Catholic School                                                         |
| 46D55197-C7AE-E311-B8ED-005056822391 | 10002506    | Knowle DGE                                                                             |
| DA0C6579-C7AE-E311-B8ED-005056822391 | 10016211    | Kimbolton School                                                                       |
| CB26A1D3-C6AE-E311-B8ED-005056822391 | 10070990    | Doddinghurst C of E Junior School                                                      |
| 63C0D6A9-C6AE-E311-B8ED-005056822391 | 10073354    | East Peckham Primary School                                                            |
| 15B60486-C6AE-E311-B8ED-005056822391 | 10076419    | Sheen Mount Junior & Infant School                                                     |
| BC0A6485-C7AE-E311-B8ED-005056822391 | 10017412    | Radley College                                                                         |
| FCCEA8CD-C6AE-E311-B8ED-005056822391 | 10070407    | Goodrich CE VC School                                                                  |
| B0D491DF-C6AE-E311-B8ED-005056822391 | 10070733    | St Patricks RC Primary School                                                          |
| 9CEE667F-C7AE-E311-B8ED-005056822391 | 10015482    | Cotswold Chine Home School                                                             |
| F85E0C80-C6AE-E311-B8ED-005056822391 | 10071800    | Elizabeth Langsbury Primary/Nursery School                                             |
| D86FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Garlinge Junior School                                                                 |
| 61BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Manor Way Primary School                                                               |
| 680C6579-C7AE-E311-B8ED-005056822391 | NULL        | Hill House St Mary's School                                                            |
| E6866BFD-C6AE-E311-B8ED-005056822391 | 10002538    | The Forest School                                                                      |
| 5BE06303-C7AE-E311-B8ED-005056822391 | 10016916    | Middleton Technology College                                                           |
| 1E8182EB-C6AE-E311-B8ED-005056822391 | 10000669    | Bewdley High School                                                                    |
| 07ED7F55-C7AE-E311-B8ED-005056822391 | 10005589    | Ryburn Valley High School                                                              |
| 8696874F-C7AE-E311-B8ED-005056822391 | 10000405    | Ashlyns School                                                                         |
| 236ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Temple Sutton Primary School                                                           |
| F424A1D3-C6AE-E311-B8ED-005056822391 | 10079270    | Market Rasen CE Primary School                                                         |
| 3B1FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Dymchurch Primary School                                                               |
| 28BBED97-C6AE-E311-B8ED-005056822391 | 10078188    | Warren Primary                                                                         |
| 60859443-C7AE-E311-B8ED-005056822391 | 10016955    | The Corbet School                                                                      |
| AD7C99D9-C6AE-E311-B8ED-005056822391 | 10072444    | Canewdon School                                                                        |
| 7B298AE5-C6AE-E311-B8ED-005056822391 | 10080243    | St James Lower Darwen CE Primary School                                                |
| AF615C8B-C7AE-E311-B8ED-005056822391 | 10008333    | The Latymer Preparatory School                                                         |
| E7C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Rose Hill Primary School                                                               |
| F462F591-C6AE-E311-B8ED-005056822391 | 10073245    | Chiddingly Primary School                                                              |
| 84298AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Willibrord's RC Primary School                                                      |
| 6ED391DF-C6AE-E311-B8ED-005056822391 | 10072459    | St John's Mead CE Primary School                                                       |
| F17D99D9-C6AE-E311-B8ED-005056822391 | 10075464    | Broadwater CE School                                                                   |
| 958282EB-C6AE-E311-B8ED-005056822391 | 10017484    | Saltley School                                                                         |
| B9996F6D-C7AE-E311-B8ED-005056822391 | 10017408    | Scarborough College                                                                    |
| 49B03A27-C7AE-E311-B8ED-005056822391 | 10077521    | Longroyde Junior School                                                                |
| 28365C09-C7AE-E311-B8ED-005056822391 | 10017774    | The Earls High School                                                                  |
| 713273F7-C6AE-E311-B8ED-005056822391 | NULL        | John O'Gaunt Community School                                                          |
| 5FD291DF-C6AE-E311-B8ED-005056822391 | 10079257    | The Spalding Parish Church of England Day School                                       |
| 19CEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Edwards First School                                                                |
| 5BB50486-C6AE-E311-B8ED-005056822391 | 10076428    | Harold Court Primary                                                                   |
| 58C6BFBB-C6AE-E311-B8ED-005056822391 | 10078090    | Cooper Perry Primary School                                                            |
| DB16CFAF-C6AE-E311-B8ED-005056822391 | 10079774    | Leechpool C P School                                                                   |
| C4ED667F-C7AE-E311-B8ED-005056822391 | 10017226    | Taunton Preparatory School                                                             |
| A423A1D3-C6AE-E311-B8ED-005056822391 | 10070509    | St Andrews C of E                                                                      |
| 2F70C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Norton Primary School                                                                  |
| 1862F591-C6AE-E311-B8ED-005056822391 | NULL        | Benedict Primary School                                                                |
| 5C23A1D3-C6AE-E311-B8ED-005056822391 | 10076798    | Hadnall CE Primary School                                                              |
| 60BBED97-C6AE-E311-B8ED-005056822391 | 10075246    | Earlham Primary School                                                                 |
| 97D45197-C7AE-E311-B8ED-005056822391 | NULL        | Three Spires School                                                                    |
| 0D5C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Hornsea Community Primary                                                              |
| A55D0C80-C6AE-E311-B8ED-005056822391 | 10080741    | The Ashley School                                                                      |
| F410E69D-C6AE-E311-B8ED-005056822391 | 10078993    | Torkington Primary School                                                              |
| 058E540F-C7AE-E311-B8ED-005056822391 | 10015762    | Hayle Community School                                                                 |
| 7317CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Janet Duke Junior School                                                               |
| 90BAED97-C6AE-E311-B8ED-005056822391 | NULL        | Swinton Queen Primary School                                                           |
| 2317CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Priorslee Primary                                                                      |
| EB64F591-C6AE-E311-B8ED-005056822391 | 10079991    | William Davies Primary School                                                          |
| 1F3F8F49-C7AE-E311-B8ED-005056822391 | 10006179    | St Hilda's C of E High                                                                 |
| B60A6485-C7AE-E311-B8ED-005056822391 | 10013333    | The Harrodian School                                                                   |
| 886EC7B5-C6AE-E311-B8ED-005056822391 | 10072803    | Trent Vale Infant & Nursery School                                                     |
| 6B12E69D-C6AE-E311-B8ED-005056822391 | 10069241    | Ball Green Primary School                                                              |
| 750EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Oakdale Infant School                                                                  |
| 9877B0C7-C6AE-E311-B8ED-005056822391 | NULL        | St Paul's CE                                                                           |
| 58D491DF-C6AE-E311-B8ED-005056822391 | NULL        | St Clement's CE Primary School                                                         |
| F7BFD6A9-C6AE-E311-B8ED-005056822391 | 10077747    | Alexander First School                                                                 |
| EE0C6579-C7AE-E311-B8ED-005056822391 | NULL        | Belmont Grosvenor School                                                               |
| B210E69D-C6AE-E311-B8ED-005056822391 | 10078640    | Fleet Wood Lane School                                                                 |
| 71C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Hightown Primary School                                                                |
| 332C8AE5-C6AE-E311-B8ED-005056822391 | 10072418    | Tolleshunt D'Arcy St Nicholas C of E Primary School                                    |
| B11B5791-C7AE-E311-B8ED-005056822391 | NULL        | St Cuthman's School                                                                    |
| 18B30486-C6AE-E311-B8ED-005056822391 | NULL        | Sunny Bank Primary                                                                     |
| 8C615C8B-C7AE-E311-B8ED-005056822391 | 10018087    | The Education Centre                                                                   |
| 0590540F-C7AE-E311-B8ED-005056822391 | 10015322    | Chaucer Business and Enterprise College                                                |
| 95B03A27-C7AE-E311-B8ED-005056822391 | 10073410    | Butterwick Pinchb.                                                                     |
| 2E0BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Tweeddale Primary School                                                               |
| 01D391DF-C6AE-E311-B8ED-005056822391 | 10076878    | St Mary Star of the Sea Catholic                                                       |
| AD69DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Kingswood Infant School                                                                |
| D1CBA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Arborfield, Newland and Barkham Church of England Junior School                        |
| 9BBFD6A9-C6AE-E311-B8ED-005056822391 | 10071256    | John Perryn Primary School                                                             |
| 73EE667F-C7AE-E311-B8ED-005056822391 | 10018575    | Edgeborough School                                                                     |
| 9574B0C7-C6AE-E311-B8ED-005056822391 | 10074652    | Elm Wood Primary School                                                                |
| 505F0C80-C6AE-E311-B8ED-005056822391 | 10079733    | Wheatfield Primary School                                                              |
| 83375C09-C7AE-E311-B8ED-005056822391 | NULL        | Wilnecote High                                                                         |
| CB71C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Whinstone Primary School                                                               |
| 27E3B6F3-C1AE-E311-B8ED-005056822391 | 10005145    | Poole High School                                                                      |
| ECE44C15-C7AE-E311-B8ED-005056822391 | 10007995    | Riverside Community College                                                            |
| CD25A1D3-C6AE-E311-B8ED-005056822391 | 10074688    | Rainford C of E Primary School                                                         |
| A1536A73-C7AE-E311-B8ED-005056822391 | 10018812    | Cheam School                                                                           |
| BA0A6485-C7AE-E311-B8ED-005056822391 | 10017299    | The New School at West Heath                                                           |
| 9B365C09-C7AE-E311-B8ED-005056822391 | 10006017    | Souhtam College                                                                        |
| 9219CFAF-C6AE-E311-B8ED-005056822391 | 10075073    | Chesswood School                                                                       |
| 435F0C80-C6AE-E311-B8ED-005056822391 | 10071776    | Malmesbury Primary School                                                              |
| E770C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Manly Park Infant School                                                               |
| 383D451B-C7AE-E311-B8ED-005056822391 | 10017715    | The Bishop of Hereford's Bluecoat School                                               |
| 3E615C8B-C7AE-E311-B8ED-005056822391 | 10008587    | Wellington Junior School                                                               |
| 04C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Barncroft School                                                                       |
| 8AB03A27-C7AE-E311-B8ED-005056822391 | NULL        | Holland Park Primary School                                                            |
| B6A77A5B-C7AE-E311-B8ED-005056822391 | 10006252    | St Peter's                                                                             |
| F363F591-C6AE-E311-B8ED-005056822391 | 10078953    | Woodhouse Primary School                                                               |
| 0A7B589D-C7AE-E311-B8ED-005056822391 | 10018241    | The Oaks Secondary School                                                              |
| 575E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Hornton Primary School                                                                 |
| 1A2D8AE5-C6AE-E311-B8ED-005056822391 | 10005872    | Sir Thomas Rich's                                                                      |
| D77B99D9-C6AE-E311-B8ED-005056822391 | 10078170    | All Saints Catholic School                                                             |
| 54F63F21-C7AE-E311-B8ED-005056822391 | 10004616    | Newman Catholic School                                                                 |
| 7120B8C1-C6AE-E311-B8ED-005056822391 | 10077632    | John Blow County                                                                       |
| 76E54C15-C7AE-E311-B8ED-005056822391 | 10014932    | Bellemoor School for Boys                                                              |
| 6A96874F-C7AE-E311-B8ED-005056822391 | 10006654    | The Downs School                                                                       |
| F9CEA8CD-C6AE-E311-B8ED-005056822391 | 10079293    | St Anne's C of E Primary School                                                        |
| C769DEA3-C6AE-E311-B8ED-005056822391 | 10076566    | Lozells Primary School                                                                 |
| E916CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Blue Bell Hill Primary School                                                          |
| 51D55197-C7AE-E311-B8ED-005056822391 | 10016700    | Nortonthorpe Hall School                                                               |
| 8C1ACFAF-C6AE-E311-B8ED-005056822391 | 10069754    | Warden Hill Infant School                                                              |
| 9BB50486-C6AE-E311-B8ED-005056822391 | 10071189    | Clavendon Primary School                                                               |
| 2926A1D3-C6AE-E311-B8ED-005056822391 | 10079267    | Saxilby C of E Primary School                                                          |
| 5CB03A27-C7AE-E311-B8ED-005056822391 | 10077502    | Danesfield School                                                                      |
| B5298AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Lukes CE School                                                                     |
| 972D8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Augustines                                                                          |
| D3AF3A27-C7AE-E311-B8ED-005056822391 | NULL        | Northlands Junior School                                                               |
| 087C99D9-C6AE-E311-B8ED-005056822391 | 10068526    | St Peters CEP                                                                          |
| 77B40486-C6AE-E311-B8ED-005056822391 | NULL        | Glastonbury Primary School                                                             |
| 78D55197-C7AE-E311-B8ED-005056822391 | 10015037    | Baskerville School                                                                     |
| 3CBCED97-C6AE-E311-B8ED-005056822391 | NULL        | Oakthorpe Primary                                                                      |
| F62A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Bishop Lonsdale Primary School                                                         |
| 5311E69D-C6AE-E311-B8ED-005056822391 | 10069702    | Clarkson Infant School                                                                 |
| F0D391DF-C6AE-E311-B8ED-005056822391 | 10070788    | St Werburgh's Catholic Primary School                                                  |
| 94E74C15-C7AE-E311-B8ED-005056822391 | 10017720    | Lacon Childe School                                                                    |
| 9EFF7261-C7AE-E311-B8ED-005056822391 | 10006127    | St Bernards High School                                                                |
| F9ED7F55-C7AE-E311-B8ED-005056822391 | 10000478    | Aylesford School                                                                       |
| 02B60486-C6AE-E311-B8ED-005056822391 | 10069888    | Burton On The Wolds Primary School                                                     |
| D5B50486-C6AE-E311-B8ED-005056822391 | 10071949    | Deansbrook Infant School                                                               |
| FD8282EB-C6AE-E311-B8ED-005056822391 | 10014785    | Alder Community High School                                                            |
| 34E54C15-C7AE-E311-B8ED-005056822391 | 10017040    | Plant Hill High School                                                                 |
| B6096485-C7AE-E311-B8ED-005056822391 | 10015060    | Buckingham College Secondary School                                                    |
| 666EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Bedmond Village Primary and Nursery School                                             |
| 3F0CFD8B-C6AE-E311-B8ED-005056822391 | 10071295    | Holly Park Primary School                                                              |
| 42F83F21-C7AE-E311-B8ED-005056822391 | 10077525    | Russell Hall Primary School                                                            |
| 757A589D-C7AE-E311-B8ED-005056822391 | 10017094    | The Milestone School                                                                   |
| 9A11E69D-C6AE-E311-B8ED-005056822391 | NULL        | Bosworth Wood Primary School                                                           |
| 6CD491DF-C6AE-E311-B8ED-005056822391 | 10078705    | Holy Family Catholic Primary School Platt Bridge                                       |
| B78E540F-C7AE-E311-B8ED-005056822391 | 10016110    | Humphry Davy School                                                                    |
| 52C6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Kippax North Junior, Infant & Nursery School                                           |
| 2D5C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Allens Croft Nursery School                                                            |
| EE11E69D-C6AE-E311-B8ED-005056822391 | NULL        | Wykeham Primary School                                                                 |
| 81096485-C7AE-E311-B8ED-005056822391 | 10043881    | Francis Holland CE School                                                              |
| 0677B0C7-C6AE-E311-B8ED-005056822391 | 10071074    | The Dawnay School                                                                      |
| 18615C8B-C7AE-E311-B8ED-005056822391 | 10080806    | Merlin                                                                                 |
| D8C1D6A9-C6AE-E311-B8ED-005056822391 | 10079573    | Woodhall School                                                                        |
| 8C23A1D3-C6AE-E311-B8ED-005056822391 | 10071368    | Burley St Matthias Church of England Voluntary Controlled Primary School               |
| 8D1DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Applecroft School                                                                      |
| 888D540F-C7AE-E311-B8ED-005056822391 | 10005655    | Saltash Community School                                                               |
| 33F83F21-C7AE-E311-B8ED-005056822391 | 10069597    | Eardley Primary School                                                                 |
| B7E54C15-C7AE-E311-B8ED-005056822391 | 10018750    | Swanmead Community School                                                              |
| 3E76B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Holmleigh Primary School                                                               |
| 97E44C15-C7AE-E311-B8ED-005056822391 | NULL        | Danley Middle School                                                                   |
| 62298AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Giles RC Primary School                                                             |
| C47A99D9-C6AE-E311-B8ED-005056822391 | NULL        | Lisle Marsden C of E Aided Primary School                                              |
| 9B74B0C7-C6AE-E311-B8ED-005056822391 | 10070036    | Hallfield Junior School                                                                |
| 58D591DF-C6AE-E311-B8ED-005056822391 | 10072426    | Ingrave Johnstone Church of England Voluntary Aide                                     |
| 9671C7B5-C6AE-E311-B8ED-005056822391 | 10079553    | Holywell J M I                                                                         |
| 7FE06303-C7AE-E311-B8ED-005056822391 | 10005526    | Rosemary Musker High School, Thetford                                                  |
| 6D65F591-C6AE-E311-B8ED-005056822391 | 10076574    | Summerfield Primary                                                                    |
| 9BD591DF-C6AE-E311-B8ED-005056822391 | 10078753    | All Saints C of E First School                                                         |
| 0691540F-C7AE-E311-B8ED-005056822391 | 10015259    | Broughton Business and Enterprise College                                              |
| 2C63F591-C6AE-E311-B8ED-005056822391 | NULL        | Mellor Primary School                                                                  |
| 020B6485-C7AE-E311-B8ED-005056822391 | NULL        | Langley Prep School                                                                    |
| 8196874F-C7AE-E311-B8ED-005056822391 | 10003540    | Katharine Lady Berkeley's School                                                       |
| 3DF83F21-C7AE-E311-B8ED-005056822391 | 10069589    | Grange Park Junior                                                                     |
| 2FC6BFBB-C6AE-E311-B8ED-005056822391 | 10073887    | Moat Hall Primary School                                                               |
| EDD391DF-C6AE-E311-B8ED-005056822391 | 10079459    | St Bernadette's RC Primary School                                                      |
| A4D491DF-C6AE-E311-B8ED-005056822391 | NULL        | St Hubert's Catholic Primary School                                                    |
| FBC5BFBB-C6AE-E311-B8ED-005056822391 | 10078030    | Gainsborough Primary School                                                            |
| 7576B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Macaulay Infant School                                                                 |
| 5971C7B5-C6AE-E311-B8ED-005056822391 | 10077140    | Rift House Primary School                                                              |
| F974B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Brinkhill Primary School                                                               |
| A620B8C1-C6AE-E311-B8ED-005056822391 | 10069296    | Lantern Lane Primary School                                                            |
| 8FBFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Greenham Court Community Primary School                                                |
| 8E3273F7-C6AE-E311-B8ED-005056822391 | 10005208    | Primrose High School                                                                   |
| 2477B0C7-C6AE-E311-B8ED-005056822391 | 10074087    | Nascot Wood Infant and Nursery School                                                  |
| C60CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Crofton Junior School                                                                  |
| 6D7C99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Augustines RC Ptimary School                                                        |
| 6AD591DF-C6AE-E311-B8ED-005056822391 | NULL        | St Nicholas V A C of E Primary School                                                  |
| 3426A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Christ Church School                                                                   |
| 2EB40486-C6AE-E311-B8ED-005056822391 | NULL        | East Dene Primary School                                                               |
| 41C7BFBB-C6AE-E311-B8ED-005056822391 | 10079542    | Lordship Farm School                                                                   |
| F7B9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Bare Trees County Junior School                                                        |
| 221FB8C1-C6AE-E311-B8ED-005056822391 | 10069169    | Winton Primary School                                                                  |
| C7096485-C7AE-E311-B8ED-005056822391 | NULL        | Ealling College Upper School                                                           |
| 50C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Lordswood Infant School                                                                |
| B7D87AF1-C6AE-E311-B8ED-005056822391 | 10004925    | Overton Grange                                                                         |
| 38D591DF-C6AE-E311-B8ED-005056822391 | 10076821    | Sacred Heart Catholic Primary and Nursery School                                       |
| E8D291DF-C6AE-E311-B8ED-005056822391 | 10075811    | Netherside Hall School                                                                 |
| E524A1D3-C6AE-E311-B8ED-005056822391 | 10069045    | Brabins Endowed School                                                                 |
| A7E44C15-C7AE-E311-B8ED-005056822391 | 10017397    | Shotton Hall School                                                                    |
| 062B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Walter Evans C of E (Aided) Primary School                                             |
| FFAF3A27-C7AE-E311-B8ED-005056822391 | 10073678    | Great Totham Primary School                                                            |
| CD3C451B-C7AE-E311-B8ED-005056822391 | 10017764    | Stanley Technical High School                                                          |
| ECDA7AF1-C6AE-E311-B8ED-005056822391 | 10015204    | Bourne Community College                                                               |
| D5BAED97-C6AE-E311-B8ED-005056822391 | 10072238    | Stanley Grove Primary and Nursery School                                               |
| 6D63F591-C6AE-E311-B8ED-005056822391 | 10078643    | Deeping St James Community Primary School                                              |
| C6B50486-C6AE-E311-B8ED-005056822391 | 10079825    | St Ives Junior School                                                                  |
| 0318CFAF-C6AE-E311-B8ED-005056822391 | 10074788    | Coppice Farm Primary School                                                            |
| D11DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Dunmore Junior Sch                                                                     |
| D7605C8B-C7AE-E311-B8ED-005056822391 | NULL        | Noam Primary School                                                                    |
| 9975B0C7-C6AE-E311-B8ED-005056822391 | 10078066    | Hitherfield Primary School                                                             |
| 3D7A589D-C7AE-E311-B8ED-005056822391 | 10016468    | Park Special School                                                                    |
| 5919CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Chantry Infant School                                                                  |
| 65A77A5B-C7AE-E311-B8ED-005056822391 | 10016822    | Oldborough Manor Community School                                                      |
| B7C2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Springwood Community Primary School                                                    |
| 30896BFD-C6AE-E311-B8ED-005056822391 | 10017371    | Ratton School                                                                          |
| 6C2C8AE5-C6AE-E311-B8ED-005056822391 | 10072683    | Sacred Heart Catholic Primary School, Thornton Cleveleys                               |
| A426A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Hillhouse C of E Primary School                                                        |
| A317CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Southlands Lower School                                                                |
| 8CB60486-C6AE-E311-B8ED-005056822391 | 10080024    | Grove Park Primary School                                                              |
| 16D55197-C7AE-E311-B8ED-005056822391 | NULL        | Greenfield School                                                                      |
| 3F7D99D9-C6AE-E311-B8ED-005056822391 | NULL        | Holy Family Catholic Primary School                                                    |
| 478382EB-C6AE-E311-B8ED-005056822391 | NULL        | Hillcrest School                                                                       |
| BF12E69D-C6AE-E311-B8ED-005056822391 | NULL        | Fairchildes Primary School                                                             |
| DCCDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Paul's C of E Primary School                                                        |
| 978D540F-C7AE-E311-B8ED-005056822391 | 10017066    | Pool School and Community College                                                      |
| ACDA7AF1-C6AE-E311-B8ED-005056822391 | 10003077    | Highgate Wood School                                                                   |
| 0312E69D-C6AE-E311-B8ED-005056822391 | 10061422    | Cayley Primary School                                                                  |
| 74B9ED97-C6AE-E311-B8ED-005056822391 | 10077874    | Robert Arkenstall Primary School                                                       |
| D2B9ED97-C6AE-E311-B8ED-005056822391 | 10069748    | Alwyn Infant School                                                                    |
| 8E5F0C80-C6AE-E311-B8ED-005056822391 | 10077251    | Lexden Primary School                                                                  |
| E6986F6D-C7AE-E311-B8ED-005056822391 | 10008395    | New Hall School                                                                        |
| AF68DEA3-C6AE-E311-B8ED-005056822391 | 10073904    | Albrighton Infant School                                                               |
| 955E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Thornhill Primary School                                                               |
| 59DA7AF1-C6AE-E311-B8ED-005056822391 | 10007605    | Woodford High School                                                                   |
| 5B8482EB-C6AE-E311-B8ED-005056822391 | 10004164    | Maltby Community School - Specialising in Business and Enterprise                      |
| 60876BFD-C6AE-E311-B8ED-005056822391 | 10007525    | Wilsthorpe Community School                                                            |
| CB1A7067-C7AE-E311-B8ED-005056822391 | 10017603    | St Paul's Community Foundation School                                                  |
| 5076B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Greenwood Primary School                                                               |
| E56ADEA3-C6AE-E311-B8ED-005056822391 | 10070614    | New Milton Junior School                                                               |
| 8BAF3A27-C7AE-E311-B8ED-005056822391 | 10077497    | Brookmead School                                                                       |
| 875C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Brentwood Nursery School                                                               |
| 4B23A1D3-C6AE-E311-B8ED-005056822391 | 10079247    | Thurton Primary School                                                                 |
| DF3C451B-C7AE-E311-B8ED-005056822391 | NULL        | Greig City Academy                                                                     |
| D5E16303-C7AE-E311-B8ED-005056822391 | 10010033    | Ashdown School                                                                         |
| 7465F591-C6AE-E311-B8ED-005056822391 | NULL        | Skelton-in-Cleveland Infant School                                                     |
| 605C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Bentilee Nursery School                                                                |
| DB7A99D9-C6AE-E311-B8ED-005056822391 | 10071429    | William Ford C of E Junior School                                                      |
| 60D491DF-C6AE-E311-B8ED-005056822391 | 10076840    | St Georges Catholic Primary School                                                     |
| 163F451B-C7AE-E311-B8ED-005056822391 | 10006246    | St Paul's School for Girls                                                             |
| 0D2C8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Larmenier & Sacred Heart Catholic Primary School                                       |
| 215E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Allerton Primary School                                                                |
| 7863F591-C6AE-E311-B8ED-005056822391 | 10080056    | Welbourne Primary School                                                               |
| 941B5791-C7AE-E311-B8ED-005056822391 | 10016178    | The Orchard School                                                                     |
| F6526A73-C7AE-E311-B8ED-005056822391 | 10078238    | Keser Torah Boys' School                                                               |
| 54365C09-C7AE-E311-B8ED-005056822391 | 10016595    | Grange School                                                                          |
| 9DF53F21-C7AE-E311-B8ED-005056822391 | 10006897    | Thornleigh Salesian College                                                            |
| 1B0CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Howes Primary School                                                                   |
| B3E16303-C7AE-E311-B8ED-005056822391 | 10004152    | Maghull High School                                                                    |
| E98282EB-C6AE-E311-B8ED-005056822391 | 10016610    | Mortimer Community College                                                             |
| 43B50486-C6AE-E311-B8ED-005056822391 | NULL        | North Grecian Street Primary School                                                    |
| 77B50486-C6AE-E311-B8ED-005056822391 | 10071488    | Sharmans Cross                                                                         |
| 6C6BDEA3-C6AE-E311-B8ED-005056822391 | NULL        | Pakefield Primary School                                                               |
| ED8F540F-C7AE-E311-B8ED-005056822391 | 10000477    | Aylesford School                                                                       |
| 95096485-C7AE-E311-B8ED-005056822391 | 10007456    | Westminster School                                                                     |
| ACD45197-C7AE-E311-B8ED-005056822391 | 10015579    | Trench Hall School                                                                     |
| CDC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | The Firs Lower School                                                                  |
| 37408F49-C7AE-E311-B8ED-005056822391 | 10003551    | Kelsey Park School                                                                     |
| 7A2C8AE5-C6AE-E311-B8ED-005056822391 | 10079893    | Our Lady & St Gerard's RC Primary School                                               |
| 817C99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Michaels C of E Primary School                                                      |
| AEBFD6A9-C6AE-E311-B8ED-005056822391 | 10072838    | Longford Park Primary School                                                           |
| 2011E69D-C6AE-E311-B8ED-005056822391 | 10072569    | Netherthong Primary School                                                             |
| 815F0C80-C6AE-E311-B8ED-005056822391 | 10070900    | Birkby Infant and Nursery School                                                       |
| 218F540F-C7AE-E311-B8ED-005056822391 | 10017584    | Sunnydale School                                                                       |
| 26B60486-C6AE-E311-B8ED-005056822391 | NULL        | Studfall Junior School                                                                 |
| 3DC8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Trescott Primary School                                                                |
| 0525A1D3-C6AE-E311-B8ED-005056822391 | 10042462    | Blundeston CE VC Primary School                                                        |
| 68F63F21-C7AE-E311-B8ED-005056822391 | 10015911    | Hesketh Fletcher CE High School                                                        |
| 9519CFAF-C6AE-E311-B8ED-005056822391 | 10073155    | Meadow View JMI School                                                                 |
| 81298AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Dominic's RC Primary School                                                         |
| 0720B8C1-C6AE-E311-B8ED-005056822391 | 10069293    | Everton Primary School                                                                 |
| 0D0DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Outlane Junior School                                                                  |
| EDD491DF-C6AE-E311-B8ED-005056822391 | NULL        | St George and St Martin's Catholic Primary School                                      |
| AB26A1D3-C6AE-E311-B8ED-005056822391 | 10073420    | Ashby Church of England Primary School                                                 |
| D617CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Harlands Primary School                                                                |
| 47395C09-C7AE-E311-B8ED-005056822391 | 10018692    | Ponteland Middle School                                                                |
| 42BAED97-C6AE-E311-B8ED-005056822391 | 10070934    | Stalyhill Infant School                                                                |
| FA896BFD-C6AE-E311-B8ED-005056822391 | NULL        | Chatham Grammar School for Boys                                                        |
| E31B5791-C7AE-E311-B8ED-005056822391 | 10015310    | Chase School                                                                           |
| 64D291DF-C6AE-E311-B8ED-005056822391 | 10076884    | St Mary Magdalene's Catholic Primary School                                            |
| EF605C8B-C7AE-E311-B8ED-005056822391 | NULL        | Banham Marshalls College                                                               |
| 7D1EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Torriano Infants School                                                                |
| 5C0CFD8B-C6AE-E311-B8ED-005056822391 | 10078378    | Shiremoor Primary School                                                               |
| F20DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Berridge Infant & Nursery School                                                       |
| 66D591DF-C6AE-E311-B8ED-005056822391 | NULL        | St Dominics RC Junior School                                                           |
| 2F7A589D-C7AE-E311-B8ED-005056822391 | 10016960    | Ramsgate School                                                                        |
| 0264F591-C6AE-E311-B8ED-005056822391 | 10070903    | Stubbings Infant School                                                                |
| 4562F591-C6AE-E311-B8ED-005056822391 | 10045355    | Monkwick Junior School                                                                 |
| 78F53F21-C7AE-E311-B8ED-005056822391 | 10006276    | St Anthony's Girls' Catholic Academy                                                   |
| 76C7BFBB-C6AE-E311-B8ED-005056822391 | 10079541    | Lime Walk Primary School                                                               |
| EBED7F55-C7AE-E311-B8ED-005056822391 | NULL        | Slough and Eton Church of England School                                               |
| EBE74C15-C7AE-E311-B8ED-005056822391 | 10006656    | Duchess's Community High School                                                        |
| 19896BFD-C6AE-E311-B8ED-005056822391 | 10006742    | Littlehampton Community School                                                         |
| 1411E69D-C6AE-E311-B8ED-005056822391 | 10079997    | William Morris Middle                                                                  |
| B02C8AE5-C6AE-E311-B8ED-005056822391 | 10077909    | St Lawrence CE Primary School                                                          |
| A40BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Alverton County Primary School                                                         |
| DEC6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Porters Grange Primary School                                                          |
| DAC6BFBB-C6AE-E311-B8ED-005056822391 | 10076088    | Broadhempston Primary School                                                           |
| 3D65F591-C6AE-E311-B8ED-005056822391 | 10077875    | Lionel Walden Primary School                                                           |
| 60B03A27-C7AE-E311-B8ED-005056822391 | 10076022    | St Marys RC Primary                                                                    |
| DC7A589D-C7AE-E311-B8ED-005056822391 | 10015888    | Hazel Court School                                                                     |
| 7874B0C7-C6AE-E311-B8ED-005056822391 | 10069290    | Misterton Primary                                                                      |
| AA3073F7-C6AE-E311-B8ED-005056822391 | 10015742    | Welfield High School                                                                   |
| DE2F73F7-C6AE-E311-B8ED-005056822391 | 10017407    | Ridgewood School                                                                       |
| 5365F591-C6AE-E311-B8ED-005056822391 | 10071328    | Broadwater Primary School                                                              |
| 9B7A99D9-C6AE-E311-B8ED-005056822391 | 10079304    | The Moat School                                                                        |
| 9BD150A3-C7AE-E311-B8ED-005056822391 | 10016471    | Mayfield School                                                                        |
| 2CD391DF-C6AE-E311-B8ED-005056822391 | 10071396    | St John C of E Primary School, Kearsley                                                |
| F8D491DF-C6AE-E311-B8ED-005056822391 | NULL        | Guru Nanak Sikh VA Secondary School                                                    |
| 9CB03A27-C7AE-E311-B8ED-005056822391 | NULL        | St Clare's RC School                                                                   |
| 27D291DF-C6AE-E311-B8ED-005056822391 | NULL        | St Gerard's Catholic Primary School                                                    |
| BC11E69D-C6AE-E311-B8ED-005056822391 | NULL        | Martlesham Beacon Hill Primary School                                                  |
| ACB9ED97-C6AE-E311-B8ED-005056822391 | 10074998    | Blackbrook St Marys Catholic Primary School                                            |
| B9B9ED97-C6AE-E311-B8ED-005056822391 | 10068891    | Little Kingshill Combined School                                                       |
| DB25A1D3-C6AE-E311-B8ED-005056822391 | 10071406    | St Helens Primary School                                                               |
| 45D87AF1-C6AE-E311-B8ED-005056822391 | 10015258    | Bowring Comprehensive                                                                  |
| C210E69D-C6AE-E311-B8ED-005056822391 | 10077315    | Farley Hill Primary School                                                             |
| 4FE74C15-C7AE-E311-B8ED-005056822391 | 10003245    | Idsall School                                                                          |
| 4E1FB8C1-C6AE-E311-B8ED-005056822391 | 10070050    | Woodberry Down Junior                                                                  |
| B664F591-C6AE-E311-B8ED-005056822391 | NULL        | Nightingale Primary                                                                    |
| 67D491DF-C6AE-E311-B8ED-005056822391 | 10071529    | Swanmore C of E (Aided) Primary School                                                 |
| EC1B5791-C7AE-E311-B8ED-005056822391 | 10017674    | St Nicholas School                                                                     |
| B2D391DF-C6AE-E311-B8ED-005056822391 | 10070227    | St Dominic Savio RC Primary School                                                     |
| 5BED667F-C7AE-E311-B8ED-005056822391 | 10018477    | Leaden Hall School                                                                     |
| BDD491DF-C6AE-E311-B8ED-005056822391 | 10072427    | Bentley St Pauls C of E                                                                |
| A7B03A27-C7AE-E311-B8ED-005056822391 | 10079252    | Barkston and Syston CE Primary School                                                  |
| 795E0C80-C6AE-E311-B8ED-005056822391 | 10079871    | Barnes Junior School                                                                   |
| 80D391DF-C6AE-E311-B8ED-005056822391 | NULL        | Ss. Peter and Paul Catholic Primary School, A Voluntary Academy                        |
| A090540F-C7AE-E311-B8ED-005056822391 | 10004778    | Norwood School                                                                         |
| 456FC7B5-C6AE-E311-B8ED-005056822391 | 10076175    | Shenton Primary School                                                                 |
| CC70C7B5-C6AE-E311-B8ED-005056822391 | 10074095    | Wheatfields Infant School                                                              |
| C396874F-C7AE-E311-B8ED-005056822391 | 10007307    | Wallington County Grammar School                                                       |
| BCDA7AF1-C6AE-E311-B8ED-005056822391 | 10002975    | Heart of England School                                                                |
| 1169DEA3-C6AE-E311-B8ED-005056822391 | NULL        | The New Waltham Primary School                                                         |
| 9DE44C15-C7AE-E311-B8ED-005056822391 | 10017842    | Castle Vale School                                                                     |
| 6EDF6303-C7AE-E311-B8ED-005056822391 | 10017752    | South Leys Comprehensive                                                               |
| 9AF53F21-C7AE-E311-B8ED-005056822391 | 10006100    | St Aidans CE High School                                                               |
| FF3C451B-C7AE-E311-B8ED-005056822391 | NULL        | King Edward VI School                                                                  |
| 44C6BFBB-C6AE-E311-B8ED-005056822391 | 10077208    | Westfiled Primary School                                                               |
| 8FB03A27-C7AE-E311-B8ED-005056822391 | 10069634    | Old Leake Primary                                                                      |
| 3A6BDEA3-C6AE-E311-B8ED-005056822391 | 10070650    | West Rise Junior School                                                                |
| 02BCED97-C6AE-E311-B8ED-005056822391 | 10069487    | Church Langley Community Primary School                                                |
| 2AFF7261-C7AE-E311-B8ED-005056822391 | NULL        | Anglo European School                                                                  |
| D2DA7AF1-C6AE-E311-B8ED-005056822391 | 10002561    | Foxford School and Commmunity Arts College                                             |
| 5A3D451B-C7AE-E311-B8ED-005056822391 | 10006221    | St. Mary's Menston, a Catholic Voluntary Academy                                       |
| 09D250A3-C7AE-E311-B8ED-005056822391 | 10018198    | Thriftwood School                                                                      |
| F0DA7AF1-C6AE-E311-B8ED-005056822391 | 10014045    | Middlecott School, Kirton                                                              |
| 5C26A1D3-C6AE-E311-B8ED-005056822391 | 10079264    | St Nicholas Primary                                                                    |
| CDA77A5B-C7AE-E311-B8ED-005056822391 | 10007512    | William Farr CE High School                                                            |
| 82B9ED97-C6AE-E311-B8ED-005056822391 | 10072932    | Combs Ford Primary School                                                              |
| 785D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Dale House Independent                                                                 |
| C7E74C15-C7AE-E311-B8ED-005056822391 | NULL        | Woodlands School                                                                       |
| C47D99D9-C6AE-E311-B8ED-005056822391 | 10075465    | The March CE Primary                                                                   |
| 5CDF6303-C7AE-E311-B8ED-005056822391 | 10001011    | Bushfield Community School                                                             |
| F8D391DF-C6AE-E311-B8ED-005056822391 | NULL        | Our Lady of Victories Catholic School                                                  |
| 2B3273F7-C6AE-E311-B8ED-005056822391 | 10009884    | Chailey School                                                                         |
| 10E16303-C7AE-E311-B8ED-005056822391 | 10018515    | College Heath Middle School                                                            |
| C42B8AE5-C6AE-E311-B8ED-005056822391 | 10072716    | St Bernadette's Catholic Primary School                                                |
| 661B5791-C7AE-E311-B8ED-005056822391 | NULL        | Hamblett School                                                                        |
| 860DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Sealand Primary School                                                                 |
| 53B03A27-C7AE-E311-B8ED-005056822391 | 10075979    | St Mary's RC Primary School                                                            |
| FE3F8F49-C7AE-E311-B8ED-005056822391 | 10003749    | Laisterdyke Business and Enterprise College                                            |
| D064F591-C6AE-E311-B8ED-005056822391 | 10076927    | Breachwood Green JMI School                                                            |
| DA986F6D-C7AE-E311-B8ED-005056822391 | 10006233    | St. Mary's School                                                                      |
| E37C99D9-C6AE-E311-B8ED-005056822391 | 10078757    | St Michaels First School                                                               |
| E0D491DF-C6AE-E311-B8ED-005056822391 | NULL        | St Catherine's Catholic Primary School                                                 |
| 061EB8C1-C6AE-E311-B8ED-005056822391 | 10076899    | Bowmandle Primary                                                                      |
| AC24A1D3-C6AE-E311-B8ED-005056822391 | 10071572    | Long Sutton Church of England Primary School                                           |
| 71B60486-C6AE-E311-B8ED-005056822391 | 10069531    | Haylands Primary                                                                       |
| 78EE667F-C7AE-E311-B8ED-005056822391 | 10015132    | Stover School                                                                          |
| 2B1FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Brompton Westbrook CP School                                                           |
| 83A77A5B-C7AE-E311-B8ED-005056822391 | 10006792    | School Rowensbourne                                                                    |
| 4517CFAF-C6AE-E311-B8ED-005056822391 | 10075302    | Ash Grove Junior & Infant School                                                       |
| 88B60486-C6AE-E311-B8ED-005056822391 | NULL        | Scargill Junior School                                                                 |
| F0886BFD-C6AE-E311-B8ED-005056822391 | 10015279    | George Mitchell School                                                                 |
| 830BFD8B-C6AE-E311-B8ED-005056822391 | 10072869    | Lionel Primary                                                                         |
| 48B60486-C6AE-E311-B8ED-005056822391 | 10076542    | Gosford Park                                                                           |
| 496EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Cornhill First School                                                                  |
| 4C24A1D3-C6AE-E311-B8ED-005056822391 | 10079384    | Ranby C of E Primary                                                                   |
| 30408F49-C7AE-E311-B8ED-005056822391 | 10015341    | Coulsdon High School                                                                   |
| 4F11E69D-C6AE-E311-B8ED-005056822391 | 10077107    | St Michaels Primary School                                                             |
| E3BAED97-C6AE-E311-B8ED-005056822391 | 10080051    | Broadwater Farm Primary School                                                         |
| C9C6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Richmond Infants                                                                       |
| 6BED667F-C7AE-E311-B8ED-005056822391 | 10070259    | Sacred Heart School                                                                    |
| 3B2C8AE5-C6AE-E311-B8ED-005056822391 | 10078092    | Simon Marks Jewish Primary School                                                      |
| CF1EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Abbots Hall Junior School                                                              |
| 0E6FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Reffley Community Primary School                                                       |
| 46E84C15-C7AE-E311-B8ED-005056822391 | 10003578    | Kenton School                                                                          |
| 827C99D9-C6AE-E311-B8ED-005056822391 | 10075511    | Kineton C of E Primary School                                                          |
| E513E69D-C6AE-E311-B8ED-005056822391 | 10076366    | Somerville Primary School                                                              |
| 1E6ADEA3-C6AE-E311-B8ED-005056822391 | 10079055    | Cassiobury Junior School                                                               |
| 2A69DEA3-C6AE-E311-B8ED-005056822391 | 10074036    | Ormlsby County First School                                                            |
| 30615C8B-C7AE-E311-B8ED-005056822391 | 10005556    | The Royal Masonic School                                                               |
| B2526A73-C7AE-E311-B8ED-005056822391 | 10013252    | Badminton School                                                                       |
| 8CFE7261-C7AE-E311-B8ED-005056822391 | 10017046    | Pittville School                                                                       |
| FEBAED97-C6AE-E311-B8ED-005056822391 | 10075308    | Alexandra Primary School                                                               |
| 27D391DF-C6AE-E311-B8ED-005056822391 | NULL        | The Gainsborough Parish Church Primary School                                          |
| FEC6BFBB-C6AE-E311-B8ED-005056822391 | 10074609    | Michael Faraday School                                                                 |
| E3365C09-C7AE-E311-B8ED-005056822391 | 10005929    | Smestow School                                                                         |
| 31D491DF-C6AE-E311-B8ED-005056822391 | 10070364    | Wiggington CE Primary School                                                           |
| BFB9ED97-C6AE-E311-B8ED-005056822391 | 10073243    | Laughton Community Primary School                                                      |
| 4F62F591-C6AE-E311-B8ED-005056822391 | 10074670    | Bevington Primary School                                                               |
| 7F62F591-C6AE-E311-B8ED-005056822391 | 10069136    | Waterloo Primary School                                                                |
| C7E06303-C7AE-E311-B8ED-005056822391 | 10005800    | Shenley Brook End School                                                               |
| 3D2A8AE5-C6AE-E311-B8ED-005056822391 | 10073961    | The Priory School                                                                      |
| BBD291DF-C6AE-E311-B8ED-005056822391 | 10073458    | Church St Nicolas CE Primary                                                           |
| 1364F591-C6AE-E311-B8ED-005056822391 | 10080058    | Coleridge Primary                                                                      |
| 145C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Gainsborough Nursery                                                                   |
| 158482EB-C6AE-E311-B8ED-005056822391 | 10003671    | Kingsmead School                                                                       |
| 4075B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Heathmere Primary School                                                               |
| CFE74C15-C7AE-E311-B8ED-005056822391 | 10015080    | Nunnery Wood High School                                                               |
| 0A1DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Alston Primary                                                                         |
| D325A1D3-C6AE-E311-B8ED-005056822391 | NULL        | St Michael's Primary School                                                            |
| BD7D99D9-C6AE-E311-B8ED-005056822391 | 10075730    | Stockcross Church of England School                                                    |
| 70615C8B-C7AE-E311-B8ED-005056822391 | 10075163    | Bute House Preparatory School                                                          |
| 0F0D6579-C7AE-E311-B8ED-005056822391 | 10005336    | Queen Ethelburga's College                                                             |
| 601B7067-C7AE-E311-B8ED-005056822391 | 10014907    | Ashdown House School                                                                   |
| 8F6EC7B5-C6AE-E311-B8ED-005056822391 | 10069512    | Rolvenden Primary School                                                               |
| FC3E451B-C7AE-E311-B8ED-005056822391 | NULL        | Notre Dame Roman Catholic School                                                       |
| CB605C8B-C7AE-E311-B8ED-005056822391 | 10016226    | Tiferes High School                                                                    |
| F4C6BFBB-C6AE-E311-B8ED-005056822391 | 10073029    | Iveson Primary School                                                                  |
| 2F1EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Chadwell St Mary Primary School                                                        |
| 6168DEA3-C6AE-E311-B8ED-005056822391 | 10069694    | The Cobbs Infant School                                                                |
| 55D250A3-C7AE-E311-B8ED-005056822391 | 10015953    | Tuke School                                                                            |
| E4D391DF-C6AE-E311-B8ED-005056822391 | 10078499    | Bishop Wood C of E Junior School                                                       |
| 8019CFAF-C6AE-E311-B8ED-005056822391 | 10080416    | Lyndhurst First School                                                                 |
| 513F8F49-C7AE-E311-B8ED-005056822391 | 10001159    | Cardinal Griffin Catholic High School                                                  |
| 9CBAED97-C6AE-E311-B8ED-005056822391 | NULL        | York Road Junior School and Language Unit                                              |
| C8DA7AF1-C6AE-E311-B8ED-005056822391 | 10002665    | George Stephenson High School                                                          |
| 3FCEA8CD-C6AE-E311-B8ED-005056822391 | 10076784    | St Aldhelm's Church of England Primary                                                 |
| 7F3E8F49-C7AE-E311-B8ED-005056822391 | 10017737    | South Wigston High School                                                              |
| F3CEA8CD-C6AE-E311-B8ED-005056822391 | 10048093    | Burchetts Green CofE Infants' School                                                   |
| 4BD491DF-C6AE-E311-B8ED-005056822391 | 10076833    | St Joseph's RC JMI & Nursery                                                           |
| 68B50486-C6AE-E311-B8ED-005056822391 | 10076590    | Downsell Junior School                                                                 |
| 99B40486-C6AE-E311-B8ED-005056822391 | NULL        | Cowley Infant School                                                                   |
| D371C7B5-C6AE-E311-B8ED-005056822391 | 10076164    | Scraptoft Valley Primary School                                                        |
| 5918CFAF-C6AE-E311-B8ED-005056822391 | 10078608    | The Richmond School, Skegness                                                          |
| BC68DEA3-C6AE-E311-B8ED-005056822391 | 10072968    | Millfield Primary School                                                               |
| 7DD87AF1-C6AE-E311-B8ED-005056822391 | 10047451    | Oriel High School                                                                      |
| 426BDEA3-C6AE-E311-B8ED-005056822391 | NULL        | Castle Park Infants School                                                             |
| 4BD291DF-C6AE-E311-B8ED-005056822391 | NULL        | St Edward's RC Primary                                                                 |
| 4E12E69D-C6AE-E311-B8ED-005056822391 | 10080676    | Tibberton Community Primary School                                                     |
| F5D45197-C7AE-E311-B8ED-005056822391 | 10077044    | Oakleigh School                                                                        |
| 25B40486-C6AE-E311-B8ED-005056822391 | 10070116    | Clarendon County Primary School                                                        |
| 5B62F591-C6AE-E311-B8ED-005056822391 | NULL        | Northwood Infants' School                                                              |
| 08C9BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Hawtonville Junior School                                                              |
| 1D0DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Belvedere Junior School                                                                |
| FB70C7B5-C6AE-E311-B8ED-005056822391 | 10075607    | Woodlesford Primary School                                                             |
| 85C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Eastfield Primary                                                                      |
| A2BAED97-C6AE-E311-B8ED-005056822391 | 10080423    | Maidstone Infants School                                                               |
| 21D591DF-C6AE-E311-B8ED-005056822391 | NULL        | St Augustines CE Primary                                                               |
| BEA77A5B-C7AE-E311-B8ED-005056822391 | 10004361    | Mill Hill School                                                                       |
| 5B25A1D3-C6AE-E311-B8ED-005056822391 | 10073836    | St John's Primary School                                                               |
| 233D451B-C7AE-E311-B8ED-005056822391 | 10006606    | Blue Coat School                                                                       |
| FD7A99D9-C6AE-E311-B8ED-005056822391 | 10078686    | Our Lady of Mount Carmel Catholic Primary School                                       |
| 685F0C80-C6AE-E311-B8ED-005056822391 | 10071937    | Berrymede Infant School                                                                |
| 47CEA8CD-C6AE-E311-B8ED-005056822391 | 10070511    | Duxford Community Primary School                                                       |
| C9DEF3CE-C9AE-E311-B8ED-005056822391 | 10033896    | The Willows Primary School                                                             |
| 9864F591-C6AE-E311-B8ED-005056822391 | 10072846    | Oakhill Primary                                                                        |
| 1412E69D-C6AE-E311-B8ED-005056822391 | NULL        | Greenhill Primary School                                                               |
| 73B03A27-C7AE-E311-B8ED-005056822391 | 10073690    | Chinley Primary School                                                                 |
| 36BAED97-C6AE-E311-B8ED-005056822391 | NULL        | Crossacres Primary School                                                              |
| D6AF3A27-C7AE-E311-B8ED-005056822391 | 10069619    | Moorfield Primary                                                                      |
| EB1CB8C1-C6AE-E311-B8ED-005056822391 | 10069399    | Westwood Primary School                                                                |
| 1B24A1D3-C6AE-E311-B8ED-005056822391 | 10069861    | Wellesbourne Primary School                                                            |
| A013E69D-C6AE-E311-B8ED-005056822391 | 10068799    | Stanley Road Primary School                                                            |
| 0D625C8B-C7AE-E311-B8ED-005056822391 | 10070524    | The Mulberry House School                                                              |
| 87615C8B-C7AE-E311-B8ED-005056822391 | 10070241    | Manor Lodge School                                                                     |
| 62D591DF-C6AE-E311-B8ED-005056822391 | 10071384    | St John's CofE Infant School, Leigh                                                    |
| A6615C8B-C7AE-E311-B8ED-005056822391 | NULL        | St James Independent School                                                            |
| 5A63F591-C6AE-E311-B8ED-005056822391 | NULL        | Alder Park Primary School                                                              |
| 3D2B8AE5-C6AE-E311-B8ED-005056822391 | 10076852    | St Bernadette's Catholic                                                               |
| 86C5BFBB-C6AE-E311-B8ED-005056822391 | 10079551    | Mill Mead School                                                                       |
| 27C1D6A9-C6AE-E311-B8ED-005056822391 | 10077501    | Bedgrove Junior School                                                                 |
| 9D8182EB-C6AE-E311-B8ED-005056822391 | 10003142    | Honiton Community College                                                              |
| 20B30486-C6AE-E311-B8ED-005056822391 | NULL        | Hillsgrove Primary School                                                              |
| E91B5791-C7AE-E311-B8ED-005056822391 | 10016637    | Northern Counties School                                                               |
| E1E74C15-C7AE-E311-B8ED-005056822391 | 10000657    | Berwick On Tweed High School                                                           |
| A1C7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | St Martin at Shouldham CE (VA) Primary School                                          |
| DC5E0C80-C6AE-E311-B8ED-005056822391 | 10070625    | Portway Junior School                                                                  |
| 78E44C15-C7AE-E311-B8ED-005056822391 | 10015014    | Aireville School                                                                       |
| 011FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Charles Dickens Junior School                                                          |
| 4C18CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Hockliffe Lower                                                                        |
| E52C8AE5-C6AE-E311-B8ED-005056822391 | 10080344    | St Mary's RC Primary                                                                   |
| AA23A1D3-C6AE-E311-B8ED-005056822391 | 10074216    | Walsh CE Junior School                                                                 |
| 0D7E99D9-C6AE-E311-B8ED-005056822391 | 10070395    | St Mary's C of E Primary School                                                        |
| 51C0D6A9-C6AE-E311-B8ED-005056822391 | 10073181    | Lodge Primary School                                                                   |
| AF65F591-C6AE-E311-B8ED-005056822391 | 10076443    | Brightlingsea Infant School                                                            |
| 79CB372D-C7AE-E311-B8ED-005056822391 | NULL        | Heybridge Primary School                                                               |
| 04CDA8CD-C6AE-E311-B8ED-005056822391 | 10074694    | St Matthews School                                                                     |
| D06FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Brocks Hill Primary School                                                             |
| C6C1D6A9-C6AE-E311-B8ED-005056822391 | 10080074    | The Viking School                                                                      |
| 1C65F591-C6AE-E311-B8ED-005056822391 | 10072845    | South Grove Primary School                                                             |
| 240BFD8B-C6AE-E311-B8ED-005056822391 | 10071917    | Lady Bankes Infant School                                                              |
| 453273F7-C6AE-E311-B8ED-005056822391 | NULL        | Townelly High School                                                                   |
| FEE06303-C7AE-E311-B8ED-005056822391 | 10001702    | Cotham School                                                                          |
| 57E64C15-C7AE-E311-B8ED-005056822391 | 10015407    | Cantell School                                                                         |
| EF76B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Pyrcroft Grange Primary School                                                         |
| E2DF6303-C7AE-E311-B8ED-005056822391 | 10017281    | Taverham High School                                                                   |
| 8EC8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Whitebridge Infant School                                                              |
| 83D150A3-C7AE-E311-B8ED-005056822391 | 10015677    | Dorton House School (Royal London Society for the                                      |
| 5CC9BFBB-C6AE-E311-B8ED-005056822391 | 10069921    | Northfold CP School                                                                    |
| 0DD591DF-C6AE-E311-B8ED-005056822391 | NULL        | Audley Junior School                                                                   |
| 667A589D-C7AE-E311-B8ED-005056822391 | 10077018    | Camberwell Park Special School                                                         |
| 40536A73-C7AE-E311-B8ED-005056822391 | NULL        | The Grange School                                                                      |
| 6F8F540F-C7AE-E311-B8ED-005056822391 | 10016053    | Tiverton High School                                                                   |
| F063F591-C6AE-E311-B8ED-005056822391 | NULL        | Chadderton Hall Junior School                                                          |
| F7B40486-C6AE-E311-B8ED-005056822391 | NULL        | Chesterfield Infant School                                                             |
| 7B17CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Northview Primary School                                                               |
| 2C6EC7B5-C6AE-E311-B8ED-005056822391 | 10079102    | Poulner Junior School                                                                  |
| 9FBFD6A9-C6AE-E311-B8ED-005056822391 | 10070009    | Allens Croft Road                                                                      |
| B1CB372D-C7AE-E311-B8ED-005056822391 | NULL        | Rochford Primary School                                                                |
| BAC72DD3-C7AE-E311-B8ED-005056822391 | NULL        | The University of Southampton                                                          |
| 605E0C80-C6AE-E311-B8ED-005056822391 | 10071290    | Danson Primary School                                                                  |
| FD385C09-C7AE-E311-B8ED-005056822391 | 10004876    | Onslow St Audrey's                                                                     |
| C0B40486-C6AE-E311-B8ED-005056822391 | NULL        | Chipping Warden Primary School                                                         |
| 931DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Victoria Primary School                                                                |
| B37C99D9-C6AE-E311-B8ED-005056822391 | 10079618    | Abbey Infants School                                                                   |
| 0BC72DD3-C7AE-E311-B8ED-005056822391 | NULL        | The University of Exeter                                                               |
| 59D55197-C7AE-E311-B8ED-005056822391 | 10016604    | Moorbrook School                                                                       |
| 7C69DEA3-C6AE-E311-B8ED-005056822391 | 10080407    | Lyndon Green Infant School                                                             |
| F7C2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Fifth Avenue Primary School                                                            |
| 64D491DF-C6AE-E311-B8ED-005056822391 | 10076839    | St Josephs RC Primary School                                                           |
| 09C1D6A9-C6AE-E311-B8ED-005056822391 | 10069249    | Ladygrove                                                                              |
| 9B70C7B5-C6AE-E311-B8ED-005056822391 | 10071217    | Ilchester Community School                                                             |
| 92D391DF-C6AE-E311-B8ED-005056822391 | NULL        | St Anthonys RC School                                                                  |
| 801DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | St Margarets School                                                                    |
| 7AB9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Westbury Primary and Nursary School                                                    |
| A624A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Sandringham Primary School                                                             |
| A21A7067-C7AE-E311-B8ED-005056822391 | 10018151    | Montgomery School                                                                      |
| EA2B8AE5-C6AE-E311-B8ED-005056822391 | 10075991    | St Joan of Arc RC Primary School                                                       |
| 3CC0D6A9-C6AE-E311-B8ED-005056822391 | 10041470    | Blair Peach Primary School                                                             |
| 12C0D6A9-C6AE-E311-B8ED-005056822391 | 10077310    | Carrington Junior School                                                               |
| A2D591DF-C6AE-E311-B8ED-005056822391 | 10070756    | St Francis RC Prmary. School                                                           |
| CBC0D6A9-C6AE-E311-B8ED-005056822391 | 10078777    | Springfield First School                                                               |
| FC10E69D-C6AE-E311-B8ED-005056822391 | 10072134    | Westfield School                                                                       |
| 455E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Peatmoor Community School                                                              |
| BC1B5791-C7AE-E311-B8ED-005056822391 | NULL        | Whittington Grange School                                                              |
| E5C7BFBB-C6AE-E311-B8ED-005056822391 | 10077333    | Manor Wood Primary School                                                              |
| 3F0DFD8B-C6AE-E311-B8ED-005056822391 | 10075683    | Whiteways Primary School                                                               |
| 69BBED97-C6AE-E311-B8ED-005056822391 | NULL        | Sheringham Junior School                                                               |
| DEB50486-C6AE-E311-B8ED-005056822391 | NULL        | Down Lane Junior School                                                                |
| 747A99D9-C6AE-E311-B8ED-005056822391 | 10077916    | St Nicolas Church of England Primary School, Abing                                     |
| 7CC1D6A9-C6AE-E311-B8ED-005056822391 | 10080656    | Abbeymead Parimary School                                                              |
| 099A6F6D-C7AE-E311-B8ED-005056822391 | 10077541    | Prince's Mead School                                                                   |
| 60375C09-C7AE-E311-B8ED-005056822391 | NULL        | Alsagar Secondary School                                                               |
| DC10E69D-C6AE-E311-B8ED-005056822391 | 10078639    | Gedney Church End School                                                               |
| A674B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Kippax Ash Tree Primary School                                                         |
| 69886BFD-C6AE-E311-B8ED-005056822391 | 10018200    | Thornhill Community Academy Trust                                                      |
| 6D7D99D9-C6AE-E311-B8ED-005056822391 | 10070784    | St Ethelbert's RC Primary School                                                       |
| 63B60486-C6AE-E311-B8ED-005056822391 | 10080150    | Sidemoor First School                                                                  |
| 96B30486-C6AE-E311-B8ED-005056822391 | 10068946    | Bledlow Ridge School                                                                   |
| 9F20B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Middlewich Primary School                                                              |
| 1B2A8AE5-C6AE-E311-B8ED-005056822391 | 10073976    | Latymer All Saints School                                                              |
| D319CFAF-C6AE-E311-B8ED-005056822391 | 10079567    | Thorley Hill Primary School                                                            |
| A67D99D9-C6AE-E311-B8ED-005056822391 | 10068559    | Middleton Parish C of E Primary School                                                 |
| 623073F7-C6AE-E311-B8ED-005056822391 | 10015546    | Haling Manor High School                                                               |
| 5F7D99D9-C6AE-E311-B8ED-005056822391 | 10076885    | St Bernadette's Catholic Primary School                                                |
| 32C8BFBB-C6AE-E311-B8ED-005056822391 | 10079536    | Richard Whittington Primary                                                            |
| 2663F591-C6AE-E311-B8ED-005056822391 | 10070096    | Cavendish Primary                                                                      |
| A325A1D3-C6AE-E311-B8ED-005056822391 | 10074290    | Westbury Leigh CE Primary School                                                       |
| ACD291DF-C6AE-E311-B8ED-005056822391 | 10076836    | St Augustine's RC JMI School                                                           |
| 8771C7B5-C6AE-E311-B8ED-005056822391 | 10078289    | Glastonbury Thorn First School                                                         |
| B46FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Yateley Infant School                                                                  |
| B8BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Southfield County Primary School                                                       |
| 9D0CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | East Park Junior                                                                       |
| 53D491DF-C6AE-E311-B8ED-005056822391 | NULL        | St Alphonsus' RC Primary School                                                        |
| 14E64C15-C7AE-E311-B8ED-005056822391 | 10004427    | Morecambe High School                                                                  |
| 09E16303-C7AE-E311-B8ED-005056822391 | NULL        | Barton and Bayswater Combined School                                                   |
| D30CFD8B-C6AE-E311-B8ED-005056822391 | 10073064    | Dorchester Primary School                                                              |
| 732C8AE5-C6AE-E311-B8ED-005056822391 | 10072743    | St Anselms Primary School                                                              |
| B27A99D9-C6AE-E311-B8ED-005056822391 | 10073972    | Bishop Winnington Ingram CE School                                                     |
| 8975B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Warren Primary School                                                                  |
| E4B30486-C6AE-E311-B8ED-005056822391 | NULL        | Castle Lower School                                                                    |
| B0CDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Mary's Primary School                                                               |
| D463F591-C6AE-E311-B8ED-005056822391 | 10074337    | Thursby Primary School                                                                 |
| 94886BFD-C6AE-E311-B8ED-005056822391 | 10002397    | Fairham Community College                                                              |
| 8E64F591-C6AE-E311-B8ED-005056822391 | NULL        | Brookside Primary School                                                               |
| FC3D451B-C7AE-E311-B8ED-005056822391 | 10000131    | The Thomas Adams School                                                                |
| 0311E69D-C6AE-E311-B8ED-005056822391 | 10042648    | Lambs Lane Primary School                                                              |
| 77D45197-C7AE-E311-B8ED-005056822391 | 10015245    | Brackenfield School                                                                    |
| 260D6579-C7AE-E311-B8ED-005056822391 | 10015817    | Heathfield School                                                                      |
| DED150A3-C7AE-E311-B8ED-005056822391 | 10016176    | The Ridgeway Community School                                                          |
| A12D8AE5-C6AE-E311-B8ED-005056822391 | 10073730    | Brodetsky Primary School                                                               |
| DD23A1D3-C6AE-E311-B8ED-005056822391 | 10080193    | Newbold CE Primary School                                                              |
| 42E74C15-C7AE-E311-B8ED-005056822391 | NULL        | The Avely School                                                                       |
| 03E16303-C7AE-E311-B8ED-005056822391 | 10018259    | The Gedling School                                                                     |
| A171C7B5-C6AE-E311-B8ED-005056822391 | 10075320    | Drayton Park Combined                                                                  |
| 6DCEA8CD-C6AE-E311-B8ED-005056822391 | 10081488    | Eastnor C of E Primary School                                                          |
| BC96874F-C7AE-E311-B8ED-005056822391 | 10017485    | Shirley High School                                                                    |
| 067B589D-C7AE-E311-B8ED-005056822391 | 10015369    | Cambridge Park School                                                                  |
| EF68DEA3-C6AE-E311-B8ED-005056822391 | 10069989    | Amblecote Primary School                                                               |
| CC62F591-C6AE-E311-B8ED-005056822391 | 10076113    | Newton St Cyres Primary School                                                         |
| 7CED667F-C7AE-E311-B8ED-005056822391 | 10008695    | Warwick School                                                                         |
| 87355C09-C7AE-E311-B8ED-005056822391 | 10015883    | Hope Valley College                                                                    |
| D1D191DF-C6AE-E311-B8ED-005056822391 | NULL        | Our Lady of Lourdes Catholic Primary School                                            |
| 11B03A27-C7AE-E311-B8ED-005056822391 | NULL        | William Gilbert Endowed C of E Aided                                                   |
| 8618CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Northlands Wood Community Primary School                                               |
| 3BE64C15-C7AE-E311-B8ED-005056822391 | 10017235    | The Neville Lovett Community School                                                    |
| 0717CFAF-C6AE-E311-B8ED-005056822391 | 10072967    | Browick Road Infants                                                                   |
| 29536A73-C7AE-E311-B8ED-005056822391 | 10000607    | Bedales School                                                                         |
| FEB9ED97-C6AE-E311-B8ED-005056822391 | 10079850    | Courthouse Junior School                                                               |
| C823A1D3-C6AE-E311-B8ED-005056822391 | NULL        | St Johns CE First School                                                               |
| 017035CD-C7AE-E311-B8ED-005056822391 | NULL        | Newman College                                                                         |
| E168DEA3-C6AE-E311-B8ED-005056822391 | NULL        | John Paxton Junior School                                                              |
| 45536A73-C7AE-E311-B8ED-005056822391 | 10071137    | Widford Lodge School                                                                   |
| E52B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St John Vianney Primary School                                                         |
| 0B1B7067-C7AE-E311-B8ED-005056822391 | NULL        | Beaucroft Foundation School                                                            |
| F4D150A3-C7AE-E311-B8ED-005056822391 | 10017131    | Portesbery School                                                                      |
| 5219CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Killisick Junior School                                                                |
| CCC5BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Wallace Fields First School                                                            |
| CC876BFD-C6AE-E311-B8ED-005056822391 | 10007960    | Manor Community College                                                                |
| 502A8AE5-C6AE-E311-B8ED-005056822391 | 10071628    | St Albans                                                                              |
| 26D55197-C7AE-E311-B8ED-005056822391 | 10015461    | Frank Wise School                                                                      |
| B565F591-C6AE-E311-B8ED-005056822391 | 10075105    | Elmhurst Junior School                                                                 |
| 1B5E0C80-C6AE-E311-B8ED-005056822391 | 10073124    | Allanson St CP School                                                                  |
| E4996F6D-C7AE-E311-B8ED-005056822391 | NULL        | Licensed Victuallers' School                                                           |
| F816CFAF-C6AE-E311-B8ED-005056822391 | 10076558    | Stetchford Junior & Infant School                                                      |
| 1420B8C1-C6AE-E311-B8ED-005056822391 | 10073381    | Walkington Primary School                                                              |
| A46FC7B5-C6AE-E311-B8ED-005056822391 | 10070043    | Jessop Primary School                                                                  |
| B8C7BFBB-C6AE-E311-B8ED-005056822391 | 10076908    | Samuel Lucas Primary School                                                            |
| 9717CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Kingsthorpe Grove Lower School                                                         |
| 771ACFAF-C6AE-E311-B8ED-005056822391 | NULL        | Lindfield Primary School                                                               |
| C2096485-C7AE-E311-B8ED-005056822391 | NULL        | The Hampshire Schools - The Knightsbridge Under Sc - 213/6055                          |
| 39D291DF-C6AE-E311-B8ED-005056822391 | 10069035    | Benjamin Hargreaves VA C of E Primary School                                           |
| 7CC7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Peartree Infants School                                                                |
| 8D5E0C80-C6AE-E311-B8ED-005056822391 | 10069601    | Portfields Combined School                                                             |
| E55D0C80-C6AE-E311-B8ED-005056822391 | 10073619    | Caldecote Community Primary School                                                     |
| 43E74C15-C7AE-E311-B8ED-005056822391 | 10017576    | Aveley School                                                                          |
| D316CFAF-C6AE-E311-B8ED-005056822391 | 10079705    | Ealing Primary School 1                                                                |
| E7F53F21-C7AE-E311-B8ED-005056822391 | 10007966    | The Minster School                                                                     |
| 8AEE667F-C7AE-E311-B8ED-005056822391 | NULL        | Newlands Manor School                                                                  |
| AD76B0C7-C6AE-E311-B8ED-005056822391 | 10076688    | Cedarwood CP School                                                                    |
| E563F591-C6AE-E311-B8ED-005056822391 | 10080031    | Hermitage. Primary School                                                              |
| 39D250A3-C7AE-E311-B8ED-005056822391 | 10018111    | Oswaldtwistle White Ash School                                                         |
| 03D291DF-C6AE-E311-B8ED-005056822391 | 10072462    | Groombridge St Th                                                                      |
| A2849443-C7AE-E311-B8ED-005056822391 | NULL        | Sexey's School                                                                         |
| 0E14E69D-C6AE-E311-B8ED-005056822391 | 10074167    | Yew Tree Community School                                                              |
| 4C3273F7-C6AE-E311-B8ED-005056822391 | 10018100    | Reepham High School                                                                    |
| 72298AE5-C6AE-E311-B8ED-005056822391 | 10070750    | St Peter's RC Primary                                                                  |
| DDD291DF-C6AE-E311-B8ED-005056822391 | NULL        | Piddle Valley Church of England First School                                           |
| 1514E69D-C6AE-E311-B8ED-005056822391 | NULL        | Marshlands Primary                                                                     |
| A23173F7-C6AE-E311-B8ED-005056822391 | 10004754    | Northfleet School for Girls                                                            |
| 79BAED97-C6AE-E311-B8ED-005056822391 | NULL        | Harewood Infant                                                                        |
| 49E06303-C7AE-E311-B8ED-005056822391 | 10008022    | Wexham School                                                                          |
| E674B0C7-C6AE-E311-B8ED-005056822391 | 10071338    | Shapla Primary School                                                                  |
| B819CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Queensway Infant School and Nursery, Thetford                                          |
| 6C5E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Bushbury Hill Junior School                                                            |
| BC2B8AE5-C6AE-E311-B8ED-005056822391 | 10076009    | St Thomas More Catholic Primary School                                                 |
| 5A11E69D-C6AE-E311-B8ED-005056822391 | 10080421    | The Mead Infant School                                                                 |
| FFE44C15-C7AE-E311-B8ED-005056822391 | NULL        | Oakfield School                                                                        |
| F48E540F-C7AE-E311-B8ED-005056822391 | 10001921    | Derby Moor Community School                                                            |
| 4BDE6303-C7AE-E311-B8ED-005056822391 | 10016013    | Kepier                                                                                 |
| 143273F7-C6AE-E311-B8ED-005056822391 | 10015115    | Colne Valley High School                                                               |
| 82ED667F-C7AE-E311-B8ED-005056822391 | 10008631    | Chetham's School of Music                                                              |
| 732A8AE5-C6AE-E311-B8ED-005056822391 | 10080474    | Moriah Jewish Primary School                                                           |
| 9665F591-C6AE-E311-B8ED-005056822391 | 10077515    | Battle Hill Primary School                                                             |
| 59615C8B-C7AE-E311-B8ED-005056822391 | 10018789    | Great Walstead School                                                                  |
| 1769DEA3-C6AE-E311-B8ED-005056822391 | 10070813    | Barons Court Infant School and Nursery                                                 |
| 220EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Parley First School                                                                    |
| E23F8F49-C7AE-E311-B8ED-005056822391 | 10007180    | Uppingham Community College                                                            |
| 5626A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Horspath C of E Primary School                                                         |
| 45C1D6A9-C6AE-E311-B8ED-005056822391 | 10081391    | Mereway Lower School                                                                   |
| 481EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Pegasus School                                                                         |
| 9D19CFAF-C6AE-E311-B8ED-005056822391 | 10079843    | Calcot Junior School                                                                   |
| D1DA7AF1-C6AE-E311-B8ED-005056822391 | 10002002    | Don Valley Academy                                                                     |
| 4F20B8C1-C6AE-E311-B8ED-005056822391 | 10073935    | Willow Brook Primary School                                                            |
| 27C0D6A9-C6AE-E311-B8ED-005056822391 | 10070797    | Weobley Primary School                                                                 |
| EDF63F21-C7AE-E311-B8ED-005056822391 | 10000218    | All Saints RC School                                                                   |
| AB1FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Thoresby Primary                                                                       |
| 8DB40486-C6AE-E311-B8ED-005056822391 | 10072903    | Northbury Junior School                                                                |
| D57A99D9-C6AE-E311-B8ED-005056822391 | NULL        | Christ Church Brondesbury CE Primary                                                   |
| 5725A1D3-C6AE-E311-B8ED-005056822391 | 10077925    | St Kenelms Primary School                                                              |
| BFE16303-C7AE-E311-B8ED-005056822391 | 10005138    | Polesworth High School                                                                 |
| 0D7B99D9-C6AE-E311-B8ED-005056822391 | 10080607    | Holy Trinity CE Junior School                                                          |
| 56EE667F-C7AE-E311-B8ED-005056822391 | 10015255    | Brentwood School                                                                       |
| 7864F591-C6AE-E311-B8ED-005056822391 | 10069959    | Lander Road Primary                                                                    |
| 8CB30486-C6AE-E311-B8ED-005056822391 | 10078425    | Broomwood Primary School                                                               |
| 28A87A5B-C7AE-E311-B8ED-005056822391 | 10015388    | Cirencester Kingshill School                                                           |
| 33C9BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Fleetdown Junior School                                                                |
| 6D5C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Dorothy Gardner Centre School                                                          |
| 58EE7F55-C7AE-E311-B8ED-005056822391 | 10003629    | King Edward Iv Grammar School                                                          |
| BFD291DF-C6AE-E311-B8ED-005056822391 | 10068698    | St Andrews Church of England Primary School                                            |
| CF1FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Mablins Lane County Primary                                                            |
| 3CD55197-C7AE-E311-B8ED-005056822391 | 10015961    | Humberston Park School                                                                 |
| 828182EB-C6AE-E311-B8ED-005056822391 | 10018211    | Risedale Community College                                                             |
| 2CC8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Stoke Fleming Community Primary School                                                 |
| 841EB8C1-C6AE-E311-B8ED-005056822391 | 10079197    | St John the Evangelist                                                                 |
| 2F056786-C8AE-E311-B8ED-005056822391 | NULL        | University Of Plymouth                                                                 |
| 37298AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Mary's Cof E V A Primary School                                                     |
| 431B5791-C7AE-E311-B8ED-005056822391 | 10014780    | Woodside School                                                                        |
| B223A1D3-C6AE-E311-B8ED-005056822391 | 10078492    | St Barnabas C of E Primary School                                                      |
| 6E24A1D3-C6AE-E311-B8ED-005056822391 | 10075737    | Binfield Church of England Primary School                                              |
| A3C82DD3-C7AE-E311-B8ED-005056822391 | NULL        | Council for National Academic Awards                                                   |
| A62A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | The Lisle Marsden C of E VA School                                                     |
| E2B40486-C6AE-E311-B8ED-005056822391 | NULL        | Aspinall Primary School                                                                |
| 551DB8C1-C6AE-E311-B8ED-005056822391 | 10053224    | Austhorpe Primary School                                                               |
| 38D55197-C7AE-E311-B8ED-005056822391 | NULL        | Hardman Fold School                                                                    |
| 2F1DB8C1-C6AE-E311-B8ED-005056822391 | 10080438    | Manor Farm Infant School                                                               |
| 37FE7261-C7AE-E311-B8ED-005056822391 | 10006028    | Southend High School Fo Girls                                                          |
| D58082EB-C6AE-E311-B8ED-005056822391 | 10004435    | Mortimer Wilson School                                                                 |
| 228282EB-C6AE-E311-B8ED-005056822391 | 10017975    | Rock Ferry High School                                                                 |
| F23C451B-C7AE-E311-B8ED-005056822391 | 10018582    | St. John's CE Middle School                                                            |
| 452A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Joachims RC Promary School                                                          |
| 400A6485-C7AE-E311-B8ED-005056822391 | 10079175    | Norlington School for Boys                                                             |
| BED87AF1-C6AE-E311-B8ED-005056822391 | 10018061    | Bedford High School                                                                    |
| 1CDB7AF1-C6AE-E311-B8ED-005056822391 | 10017594    | Streford High School                                                                   |
| 14625C8B-C7AE-E311-B8ED-005056822391 | 10075161    | Kensington Preparatory School for Girls                                                |
| CF7B99D9-C6AE-E311-B8ED-005056822391 | 10073989    | St Marys CE Primary School                                                             |
| 38395C09-C7AE-E311-B8ED-005056822391 | 10018102    | Filey School                                                                           |
| BE8282EB-C6AE-E311-B8ED-005056822391 | 10003253    | Ilford County High School                                                              |
| 8368DEA3-C6AE-E311-B8ED-005056822391 | 10068960    | Long Lane Primary School                                                               |
| B5C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Signhills Infants' School                                                              |
| 0CF83F21-C7AE-E311-B8ED-005056822391 | 10016684    | Trinity Church of England School, Lewisham                                             |
| D5D45197-C7AE-E311-B8ED-005056822391 | 10000589    | Beacon Hill School                                                                     |
| 27876BFD-C6AE-E311-B8ED-005056822391 | 10002790    | Groby Community College                                                                |
| 4CD55197-C7AE-E311-B8ED-005056822391 | NULL        | The Meadows School                                                                     |
| 65395C09-C7AE-E311-B8ED-005056822391 | NULL        | Gillingham Community Sc                                                                |
| 161ACFAF-C6AE-E311-B8ED-005056822391 | 10071211    | Hassell Primary School                                                                 |
| 44DE6303-C7AE-E311-B8ED-005056822391 | 10018570    | Pakefield Middle School                                                                |
| FFD191DF-C6AE-E311-B8ED-005056822391 | NULL        | All Saints C of E Primary                                                              |
| A3D45197-C7AE-E311-B8ED-005056822391 | 10018036    | Furrowfield School                                                                     |
| 0C62F591-C6AE-E311-B8ED-005056822391 | 10071939    | Winterbourne Junior Boys School                                                        |
| EA298AE5-C6AE-E311-B8ED-005056822391 | 10071640    | St Ignatius RC Primary School                                                          |
| D0A67A5B-C7AE-E311-B8ED-005056822391 | 10000320    | Applemore College                                                                      |
| 11B50486-C6AE-E311-B8ED-005056822391 | 10076543    | Edgewick Community Primary School                                                      |
| F1A77A5B-C7AE-E311-B8ED-005056822391 | 10003656    | Kings Norton Boys School                                                               |
| A31A5791-C7AE-E311-B8ED-005056822391 | 10008259    | Halliford School                                                                       |
| F40CFD8B-C6AE-E311-B8ED-005056822391 | 10072097    | William Torbitt Junior School                                                          |
| D98082EB-C6AE-E311-B8ED-005056822391 | 10003070    | Highcrest Community School                                                             |
| 951A5791-C7AE-E311-B8ED-005056822391 | 10018734    | St John's Beaumont                                                                     |
| 3A7B99D9-C6AE-E311-B8ED-005056822391 | 10078685    | St Francis Xavier Catholic Primary School                                              |
| A5F83F21-C7AE-E311-B8ED-005056822391 | 10000955    | Broughton Hall High School                                                             |
| 63BAED97-C6AE-E311-B8ED-005056822391 | NULL        | The Hyde School                                                                        |
| C9ED667F-C7AE-E311-B8ED-005056822391 | 10008448    | Queen's Gate School                                                                    |
| 1C3273F7-C6AE-E311-B8ED-005056822391 | NULL        | Margaret Beaufort Middle School                                                        |
| 6ADE6303-C7AE-E311-B8ED-005056822391 | NULL        | Big Wood School                                                                        |
| F2B30486-C6AE-E311-B8ED-005056822391 | NULL        | Golden Hill Primary School                                                             |
| 2D6BDEA3-C6AE-E311-B8ED-005056822391 | 10076901    | Priory Lane Infant School                                                              |
| 42E16303-C7AE-E311-B8ED-005056822391 | 10005283    | Pudsey Grammar School                                                                  |
| F98182EB-C6AE-E311-B8ED-005056822391 | 10016546    | Mount Grace High School                                                                |
| C4ED7F55-C7AE-E311-B8ED-005056822391 | NULL        | Ashton Church of England Middle                                                        |
| 640D6579-C7AE-E311-B8ED-005056822391 | 10008307    | Kent College Pembury                                                                   |
| 0D3E451B-C7AE-E311-B8ED-005056822391 | NULL        | Gillingham School                                                                      |
| AA526A73-C7AE-E311-B8ED-005056822391 | 10018788    | Priory School                                                                          |
| 9BB30486-C6AE-E311-B8ED-005056822391 | 10072991    | Lower Darwen Primary School                                                            |
| 79D591DF-C6AE-E311-B8ED-005056822391 | 10075555    | St Cuthbert's Catholic Primary School                                                  |
| EEBAED97-C6AE-E311-B8ED-005056822391 | NULL        | The Four Dwellings Infant School                                                       |
| 768182EB-C6AE-E311-B8ED-005056822391 | 10017010    | The Elton High School                                                                  |
| 31395C09-C7AE-E311-B8ED-005056822391 | 10016609    | Morpeth School                                                                         |
| 4FED7F55-C7AE-E311-B8ED-005056822391 | 10017651    | Studley High School                                                                    |
| 331FB8C1-C6AE-E311-B8ED-005056822391 | 10070637    | Limes Farm Junior School                                                               |
| FA62F591-C6AE-E311-B8ED-005056822391 | 10077640    | Cantrell Primary School                                                                |
| DBB40486-C6AE-E311-B8ED-005056822391 | NULL        | Davis Lane Junior School                                                               |
| CDA67A5B-C7AE-E311-B8ED-005056822391 | 10015686    | West Park Community School                                                             |
| 680CFD8B-C6AE-E311-B8ED-005056822391 | 10076588    | Greenleaf Primary School                                                               |
| 40FF7261-C7AE-E311-B8ED-005056822391 | 10003194    | Hugh Christie Tech College                                                             |
| 12896BFD-C6AE-E311-B8ED-005056822391 | 10018771    | Beccles Middle School                                                                  |
| BD1EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Sunnyhill Infant School                                                                |
| 006FC7B5-C6AE-E311-B8ED-005056822391 | 10075133    | Ormesby Middle School                                                                  |
| 2C056786-C8AE-E311-B8ED-005056822391 | NULL        | University Of Nottingham                                                               |
| 8DF73F21-C7AE-E311-B8ED-005056822391 | 10007531    | Wimbledon College                                                                      |
| 137A589D-C7AE-E311-B8ED-005056822391 | 10016248    | St Francis Special School                                                              |
| 97CEA8CD-C6AE-E311-B8ED-005056822391 | 10070352    | North Ferriby C of E School                                                            |
| 801ACFAF-C6AE-E311-B8ED-005056822391 | 10078454    | Cathcart Street Primary                                                                |
| 91D591DF-C6AE-E311-B8ED-005056822391 | 10079315    | St George The Martyr                                                                   |
| 4F68DEA3-C6AE-E311-B8ED-005056822391 | 10079023    | Thorpe Acre Junior                                                                     |
| 5F7B99D9-C6AE-E311-B8ED-005056822391 | NULL        | Lindley Church of England Infant School                                                |
| E7D191DF-C6AE-E311-B8ED-005056822391 | 10068565    | St Martin's CE Primary School                                                          |
| F30A6485-C7AE-E311-B8ED-005056822391 | 10000845    | Bradford Girls' Grammar School                                                         |
| 5F26A1D3-C6AE-E311-B8ED-005056822391 | 10071053    | Charlesworth Primary School                                                            |
| 42D55197-C7AE-E311-B8ED-005056822391 | 10017227    | Penn Hall Special School                                                               |
| 7FB60486-C6AE-E311-B8ED-005056822391 | 10074664    | Ashmole Primary Sc                                                                     |
| 4B3F451B-C7AE-E311-B8ED-005056822391 | 10005530    | The Rossington All Saints Church of England (VA) S                                     |
| 98D591DF-C6AE-E311-B8ED-005056822391 | NULL        | St Edward's RC Primary School                                                          |
| 94849443-C7AE-E311-B8ED-005056822391 | NULL        | Skegness Grammar School                                                                |
| FD7D99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Bernadette's Catholic Primary School                                                |
| FA1DB8C1-C6AE-E311-B8ED-005056822391 | 10076900    | Holme Valley Primary                                                                   |
| 49AB7134-CAAE-E311-B8ED-005056822391 | 10041615    | Liverpool College                                                                      |
| 7163F591-C6AE-E311-B8ED-005056822391 | NULL        | Sir James Barrie Primary School                                                        |
| 886ADEA3-C6AE-E311-B8ED-005056822391 | 10080661    | Dunalley Primary School                                                                |
| 6C996F6D-C7AE-E311-B8ED-005056822391 | 10008520    | Abberley Hall School                                                                   |
| 55DA7AF1-C6AE-E311-B8ED-005056822391 | 10016189    | Lister Community School                                                                |
| B2B9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Ladywood School                                                                        |
| 3763F591-C6AE-E311-B8ED-005056822391 | 10071305    | Thomas Arnold Primary                                                                  |
| 26DE6303-C7AE-E311-B8ED-005056822391 | 10017474    | Raincliffe School                                                                      |
| 38C0D6A9-C6AE-E311-B8ED-005056822391 | 10075077    | Pound Hill Middle School'                                                              |
| 25B50486-C6AE-E311-B8ED-005056822391 | 10076219    | Roe Lee Park Primary School                                                            |
| 3EC6BFBB-C6AE-E311-B8ED-005056822391 | 10074593    | Marion Richardson Primary School                                                       |
| 880A6485-C7AE-E311-B8ED-005056822391 | NULL        | Bickley Park School                                                                    |
| EFD191DF-C6AE-E311-B8ED-005056822391 | NULL        | Saint Sebastian's CE Primary                                                           |
| B670C7B5-C6AE-E311-B8ED-005056822391 | 10075609    | Robin Hood Primary School                                                              |
| BC13E69D-C6AE-E311-B8ED-005056822391 | NULL        | Wednesfield Village                                                                    |
| B8375C09-C7AE-E311-B8ED-005056822391 | 10017633    | South Luton High School                                                                |
| E210E69D-C6AE-E311-B8ED-005056822391 | NULL        | Canberra Primary School                                                                |
| 4F1EB8C1-C6AE-E311-B8ED-005056822391 | 10074665    | Thomas Jones Primary School                                                            |
| B8CBA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Mary's Primary School                                                               |
| 956BDEA3-C6AE-E311-B8ED-005056822391 | NULL        | Netherbrook Primary School                                                             |
| 5DCB372D-C7AE-E311-B8ED-005056822391 | NULL        | Robert Drake Primary School                                                            |
| 88CEA8CD-C6AE-E311-B8ED-005056822391 | 10074294    | St. Andrew's CE Primary School                                                         |
| 062D8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Richmond Avenue Primary School                                                         |
| 920A6485-C7AE-E311-B8ED-005056822391 | 10002228    | Eltham College                                                                         |
| 28F73F21-C7AE-E311-B8ED-005056822391 | 10016878    | Our Lady Queen of Peace Catholic High School                                           |
| 32C2D6A9-C6AE-E311-B8ED-005056822391 | 10073349    | Park Way Primary School                                                                |
| 8ED491DF-C6AE-E311-B8ED-005056822391 | NULL        | St Aloysius Roman Catholic Infant School                                               |
| E0A77A5B-C7AE-E311-B8ED-005056822391 | 10017721    | Soham Village College                                                                  |
| 94BCED97-C6AE-E311-B8ED-005056822391 | NULL        | Pyrgo Priory School                                                                    |
| 931ACFAF-C6AE-E311-B8ED-005056822391 | NULL        | Woodlands Junior School                                                                |
| E1BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Hardwick Infant School                                                                 |
| 2C64F591-C6AE-E311-B8ED-005056822391 | NULL        | Novers Lane Infant School                                                              |
| 46C7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Horsendale Primary School                                                              |
| 2271C7B5-C6AE-E311-B8ED-005056822391 | NULL        | John Hilton Primary School                                                             |
| 380D6579-C7AE-E311-B8ED-005056822391 | 10015420    | Dorset House                                                                           |
| 4AEAC5F2-C9AE-E311-B8ED-005056822391 | 10032979    | Comberton Village College                                                              |
| 00C82DD3-C7AE-E311-B8ED-005056822391 | 10007855    | Swansea University                                                                     |
| 80F63F21-C7AE-E311-B8ED-005056822391 | 10006167    | St George's VA School                                                                  |
| BF3D451B-C7AE-E311-B8ED-005056822391 | 10000308    | Anthony Gell School                                                                    |
| FA63F591-C6AE-E311-B8ED-005056822391 | NULL        | Thames View Junior School                                                              |
| D9E74C15-C7AE-E311-B8ED-005056822391 | 10007052    | Trinity High School                                                                    |
| 8AC5BFBB-C6AE-E311-B8ED-005056822391 | 10077282    | Grange Primary School                                                                  |
| C91FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Wallerscote Primary School                                                             |
| 9269DEA3-C6AE-E311-B8ED-005056822391 | 10072235    | Streethouse, Junior, Infant and Nursery                                                |
| 051B7067-C7AE-E311-B8ED-005056822391 | 10015623    | Whitefileds Schools and Centre                                                         |
| 0776B0C7-C6AE-E311-B8ED-005056822391 | 10069486    | Montem Primary School                                                                  |
| 640CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Matthew Boulton Community Primary School                                               |
| 3A8F540F-C7AE-E311-B8ED-005056822391 | NULL        | Plymstock School                                                                       |
| C625A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Rothersthorpe CE Primary School                                                        |
| DA76B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Crompton View Primary                                                                  |
| 0826A1D3-C6AE-E311-B8ED-005056822391 | 10076283    | Lapworth Primary School                                                                |
| 116FC7B5-C6AE-E311-B8ED-005056822391 | 10079095    | Kempshott Junior School                                                                |
| 8863F591-C6AE-E311-B8ED-005056822391 | 10080013    | Links First School                                                                     |
| C863F591-C6AE-E311-B8ED-005056822391 | 10069243    | Oakhill Primary School                                                                 |
| 09CCA8CD-C6AE-E311-B8ED-005056822391 | 10073948    | St Martins C of E Primary School                                                       |
| 5F2D8AE5-C6AE-E311-B8ED-005056822391 | 10003062    | High School for Girls                                                                  |
| D3385C09-C7AE-E311-B8ED-005056822391 | NULL        | Victoria Community Technology School                                                   |
| 211B7067-C7AE-E311-B8ED-005056822391 | NULL        | Willowfields School                                                                    |
| 92385C09-C7AE-E311-B8ED-005056822391 | 10014935    | Billingham Campus                                                                      |
| 3AB50486-C6AE-E311-B8ED-005056822391 | 10072872    | Deanesfield Primary School                                                             |
| 7913E69D-C6AE-E311-B8ED-005056822391 | NULL        | Shotley Community Primary School                                                       |
| 65D87AF1-C6AE-E311-B8ED-005056822391 | 10017990    | Pennywell School                                                                       |
| 2BA87A5B-C7AE-E311-B8ED-005056822391 | 10006748    | The Malling School                                                                     |
| 61ED7F55-C7AE-E311-B8ED-005056822391 | 10000045    | Abbotsfield School                                                                     |
| D8B9ED97-C6AE-E311-B8ED-005056822391 | 10076944    | Trinity Primary School                                                                 |
| CF11E69D-C6AE-E311-B8ED-005056822391 | NULL        | Byron Wood Primary School                                                              |
| 90B60486-C6AE-E311-B8ED-005056822391 | NULL        | Scotts Park Primary School                                                             |
| B7D491DF-C6AE-E311-B8ED-005056822391 | NULL        | St Peter's Catholic Primary                                                            |
| 70DE6303-C7AE-E311-B8ED-005056822391 | 10014846    | Big Wood                                                                               |
| A123A1D3-C6AE-E311-B8ED-005056822391 | 10080164    | Verwood CE First School                                                                |
| D56ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Craven Park School                                                                     |
| 0B69DEA3-C6AE-E311-B8ED-005056822391 | 10069988    | Hurst Green Primary School                                                             |
| 0FE74C15-C7AE-E311-B8ED-005056822391 | 10017602    | Stewards School                                                                        |
| 0A64F591-C6AE-E311-B8ED-005056822391 | NULL        | Bursted Wood Primary School                                                            |
| 6263F591-C6AE-E311-B8ED-005056822391 | 10037946    | Cherbourg Primary School                                                               |
| 8C13E69D-C6AE-E311-B8ED-005056822391 | 10079848    | Furze Platt Junior School                                                              |
| FCD87AF1-C6AE-E311-B8ED-005056822391 | 10015674    | Hatch End High                                                                         |
| 17C1D6A9-C6AE-E311-B8ED-005056822391 | 10079707    | Greenwood Primary School                                                               |
| 1E2A8AE5-C6AE-E311-B8ED-005056822391 | 10078367    | St Ursula's RC Infant School                                                           |
| 63B50486-C6AE-E311-B8ED-005056822391 | NULL        | Earlham Primary School                                                                 |
| F88D540F-C7AE-E311-B8ED-005056822391 | NULL        | Summerbee Comprehensive                                                                |
| 4E7C99D9-C6AE-E311-B8ED-005056822391 | NULL        | St John's CE Infant School                                                             |
| 216BDEA3-C6AE-E311-B8ED-005056822391 | 10080663    | Glenfall Community Primary School                                                      |
| BCD45197-C7AE-E311-B8ED-005056822391 | NULL        | Hitchmead School                                                                       |
| E8ED667F-C7AE-E311-B8ED-005056822391 | 10003662    | Kings Hawford School                                                                   |
| 7B0AFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Lime Tree Primary School                                                               |
| 64C6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Blyth Crofton First School                                                             |
| 6EC7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | High Halstow County Primary School                                                     |
| 9FD591DF-C6AE-E311-B8ED-005056822391 | 10074008    | St George's Church of England Primary School                                           |
| 3423A1D3-C6AE-E311-B8ED-005056822391 | NULL        | St James' Church of England Voluntary Controlled P                                     |
| 4DC7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Millbank Primary School                                                                |
| 57D491DF-C6AE-E311-B8ED-005056822391 | NULL        | Skelton Primary School                                                                 |
| 537C99D9-C6AE-E311-B8ED-005056822391 | 10071055    | Woodville C of E Junior School                                                         |
| BE70C7B5-C6AE-E311-B8ED-005056822391 | 10077141    | Barnet Grove Primary School                                                            |
| 652B8AE5-C6AE-E311-B8ED-005056822391 | 10076729    | Laleham C of E Primary School                                                          |
| C97D99D9-C6AE-E311-B8ED-005056822391 | 10074962    | St Peter's Roman Catholic Primary School                                               |
| 2EDA7AF1-C6AE-E311-B8ED-005056822391 | 10014896    | Altrincham College of Arts                                                             |
| 2E2C8AE5-C6AE-E311-B8ED-005056822391 | 10076002    | St Scholastica's RC Primary School                                                     |
| 87CB372D-C7AE-E311-B8ED-005056822391 | NULL        | Wyburns Primary School                                                                 |
| 5C75B0C7-C6AE-E311-B8ED-005056822391 | NULL        | John Ray Junior School                                                                 |
| C695874F-C7AE-E311-B8ED-005056822391 | 10006101    | St Aidan's County High School                                                          |
| 8C26A1D3-C6AE-E311-B8ED-005056822391 | 10071558    | West Meon CE Primary                                                                   |
| 4220B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Pennoweth Primary School                                                               |
| B38E540F-C7AE-E311-B8ED-005056822391 | 10015769    | Haywood High School*                                                                   |
| DDBFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Shinewater CP School                                                                   |
| 490DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Beecroft Lower School                                                                  |
| CE096485-C7AE-E311-B8ED-005056822391 | 10015466    | East Court School                                                                      |
| D6D87AF1-C6AE-E311-B8ED-005056822391 | 10018794    | Lake Middle School                                                                     |
| 786EC7B5-C6AE-E311-B8ED-005056822391 | 10079561    | Gade Valley JMI and Nursery School                                                     |
| 32C7BFBB-C6AE-E311-B8ED-005056822391 | 10079543    | Wheatcroft Primary School                                                              |
| 086FC7B5-C6AE-E311-B8ED-005056822391 | 10073109    | Mersham Primary School                                                                 |
| E5C6BFBB-C6AE-E311-B8ED-005056822391 | 10073284    | Dale Community Primary School                                                          |
| 74CEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Great Missenden Church of England Combined School                                      |
| 68D45197-C7AE-E311-B8ED-005056822391 | 10077042    | St. Nicholas School                                                                    |
| 023F451B-C7AE-E311-B8ED-005056822391 | 10004783    | Notre Dame RC School                                                                   |
| 5B6BDEA3-C6AE-E311-B8ED-005056822391 | NULL        | Hampden Park Infant School                                                             |
| 8A2B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | All Saints Primary School                                                              |
| D2849443-C7AE-E311-B8ED-005056822391 | 10000954    | Broomfield School                                                                      |
| 48ED7F55-C7AE-E311-B8ED-005056822391 | 10001884    | De Stafford Community College                                                          |
| E30C6579-C7AE-E311-B8ED-005056822391 | 10008516    | St Margaret's School Bushey                                                            |
| 07CEA8CD-C6AE-E311-B8ED-005056822391 | 10078326    | Marlow C of E First School                                                             |
| 2120B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Newlyn Jnr & Infant School                                                             |
| 665C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Randolph Beresford Early Years Centre                                                  |
| E22A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St.Laurence Primary                                                                    |
| EF355C09-C7AE-E311-B8ED-005056822391 | 10014833    | Bartley Green Tech College                                                             |
| 6DC1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Spring Lane Lower School                                                               |
| 2369DEA3-C6AE-E311-B8ED-005056822391 | 10074101    | Field Infant School                                                                    |
| C86ADEA3-C6AE-E311-B8ED-005056822391 | 10069521    | Otford Primary School                                                                  |
| 411B5791-C7AE-E311-B8ED-005056822391 | 10016738    | Oak Lodge                                                                              |
| 80CDA8CD-C6AE-E311-B8ED-005056822391 | 10081482    | Christchurch C of E (C) Primary School                                                 |
| BECBA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Bradley Church of England Voluntary Controlled Infant and Nursery School               |
| 76896BFD-C6AE-E311-B8ED-005056822391 | 10005861    | Sir Henry Floyd Grammar School                                                         |
| 5C996F6D-C7AE-E311-B8ED-005056822391 | 10008444    | Queen Anne's School                                                                    |
| DFC62DD3-C7AE-E311-B8ED-005056822391 | NULL        | The University of Birmingham                                                           |
| DE876BFD-C6AE-E311-B8ED-005056822391 | 10005038    | Pensby High School for Girls                                                           |
| 19B60486-C6AE-E311-B8ED-005056822391 | NULL        | Mayville Junior School                                                                 |
| A2298AE5-C6AE-E311-B8ED-005056822391 | 10080268    | Ss Peter & Paul RC Primary School                                                      |
| 45E06303-C7AE-E311-B8ED-005056822391 | 10014980    | Aldecar Community School                                                               |
| CB886BFD-C6AE-E311-B8ED-005056822391 | 10003504    | John Taylor High School                                                                |
| 142A8AE5-C6AE-E311-B8ED-005056822391 | 10072725    | St Anne's RC Primary                                                                   |
| D5C7BFBB-C6AE-E311-B8ED-005056822391 | 10068862    | Brook Acre Community Primary School                                                    |
| E21DB8C1-C6AE-E311-B8ED-005056822391 | 10073920    | Harwell Primary                                                                        |
| FCBBED97-C6AE-E311-B8ED-005056822391 | 10077871    | Beaupre Community School                                                               |
| 4576B0C7-C6AE-E311-B8ED-005056822391 | 10078017    | Parkwood Primary School                                                                |
| CDE54C15-C7AE-E311-B8ED-005056822391 | NULL        | St Chads School                                                                        |
| D723A1D3-C6AE-E311-B8ED-005056822391 | 10075924    | Westcott Church of England School                                                      |
| 1CE54C15-C7AE-E311-B8ED-005056822391 | 10016324    | Sholing Girls School                                                                   |
| 9E8E540F-C7AE-E311-B8ED-005056822391 | 10006345    | Stoke Damerel Community College                                                        |
| 4FDE6303-C7AE-E311-B8ED-005056822391 | NULL        | Westfield Middle School                                                                |
| DC2A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Edmunds RC Primary School                                                           |
| FAC8BFBB-C6AE-E311-B8ED-005056822391 | 10077182    | Rhyl Primary School                                                                    |
| 41BBED97-C6AE-E311-B8ED-005056822391 | 10072237    | Newton Hill Community School                                                           |
| 421FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Kingfisher CP School                                                                   |
| 120A6485-C7AE-E311-B8ED-005056822391 | 10017753    | St John's Preparatory and Senior School                                                |
| DCD491DF-C6AE-E311-B8ED-005056822391 | 10078657    | St Patrick's Catholic Primary School, Birstall                                         |
| 49C7BFBB-C6AE-E311-B8ED-005056822391 | 10072761    | South Bookham School                                                                   |
| 17E16303-C7AE-E311-B8ED-005056822391 | 10001730    | Cowley Language College                                                                |
| E1CBA8CD-C6AE-E311-B8ED-005056822391 | 10068583    | Holy Trinity CE Primary School                                                         |
| 4D0A6485-C7AE-E311-B8ED-005056822391 | 10008403    | Notting Hill & Ealing High School                                                      |
| 73D45197-C7AE-E311-B8ED-005056822391 | 10015173    | Cedar Special School                                                                   |
| E1C5BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Bromet Primary School                                                                  |
| 03F63F21-C7AE-E311-B8ED-005056822391 | 10005905    | Skinners' Company's School For Girls                                                   |
| 7CB30486-C6AE-E311-B8ED-005056822391 | NULL        | Freehold Community Primary School                                                      |
| 9920B8C1-C6AE-E311-B8ED-005056822391 | 10064119    | Belle Vue Infant School                                                                |
| 247A589D-C7AE-E311-B8ED-005056822391 | 10015292    | Glyne Gap School                                                                       |
| D724A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Sturry CE Primary School                                                               |
| 06E16303-C7AE-E311-B8ED-005056822391 | 10006626    | Cavendish School                                                                       |
| AD896BFD-C6AE-E311-B8ED-005056822391 | 10000764    | Blatchington Mill School & Sixth Form College                                          |
| 57408F49-C7AE-E311-B8ED-005056822391 | 10016723    | Notre Dame Roman Catholic Girls' School                                                |
| DAC7BFBB-C6AE-E311-B8ED-005056822391 | 10076226    | Oaklands Junior School                                                                 |
| 3B5F0C80-C6AE-E311-B8ED-005056822391 | 10052469    | Anton Junior School                                                                    |
| 692D8AE5-C6AE-E311-B8ED-005056822391 | 10015254    | Brittons School                                                                        |
| ED615C8B-C7AE-E311-B8ED-005056822391 | 10017778    | Southbank International School                                                         |
| F0986F6D-C7AE-E311-B8ED-005056822391 | 10016284    | The St Anne's College Grammar School                                                   |
| F4F73F21-C7AE-E311-B8ED-005056822391 | 10015015    | Bishop Wulstan School                                                                  |
| ED1FB8C1-C6AE-E311-B8ED-005056822391 | 10074675    | Yerbury Primary School                                                                 |
| 87AF3A27-C7AE-E311-B8ED-005056822391 | NULL        | Salterlee Primary School                                                               |
| A6876BFD-C6AE-E311-B8ED-005056822391 | 10006731    | The Lakes School                                                                       |
| F470C7B5-C6AE-E311-B8ED-005056822391 | 10074575    | Whitehall Primary School                                                               |
| 3E7E99D9-C6AE-E311-B8ED-005056822391 | 10077740    | Trinity St. Stephen CE Aided First School                                              |
| C27B99D9-C6AE-E311-B8ED-005056822391 | NULL        | Bradfield Primary School                                                               |
| AA2B8AE5-C6AE-E311-B8ED-005056822391 | 10070711    | St Boniface School                                                                     |
| 7B5F0C80-C6AE-E311-B8ED-005056822391 | 10072590    | Copley Primary School                                                                  |
| 50876BFD-C6AE-E311-B8ED-005056822391 | 10015327    | Danetre School                                                                         |
| 62996F6D-C7AE-E311-B8ED-005056822391 | 10070253    | St Michaels C of E Prep School                                                         |
| 8FDF6303-C7AE-E311-B8ED-005056822391 | 10016062    | The Icknield Community College                                                         |
| 113373F7-C6AE-E311-B8ED-005056822391 | 10001527    | Cockburn                                                                               |
| 340BFD8B-C6AE-E311-B8ED-005056822391 | 10077351    | Britannia Bridge Primary School                                                        |
| 0570C7B5-C6AE-E311-B8ED-005056822391 | 10072280    | Gildersome Primary School                                                              |
| 03C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Stephenson Lower School                                                                |
| 68298AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Johns Rl Primary School                                                             |
| F82C8AE5-C6AE-E311-B8ED-005056822391 | 10072050    | Grindleton Church of England Voluntary Aided Prima                                     |
| 92375C09-C7AE-E311-B8ED-005056822391 | NULL        | Donnington CP School                                                                   |
| 705D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Worsley Mesnes Community Primary School                                                |
| A5526A73-C7AE-E311-B8ED-005056822391 | 10014856    | Bloxham School                                                                         |
| 32E74C15-C7AE-E311-B8ED-005056822391 | 10008925    | Glendale Middle School                                                                 |
| 628E540F-C7AE-E311-B8ED-005056822391 | 10015946    | Turves Green Girls' School                                                             |
| 70CDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Jude's CE Primary School                                                            |
| 6317CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Ravensworth Junior School                                                              |
| 57D87AF1-C6AE-E311-B8ED-005056822391 | 10016098    | Two Trees High School                                                                  |
| 7370C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Archibald Primary School                                                               |
| 17D391DF-C6AE-E311-B8ED-005056822391 | 10062132    | Friskney All Saints CE (Aided) Primary School                                          |
| DDD191DF-C6AE-E311-B8ED-005056822391 | 10078664    | St Joseph's Catholic Primary School, Brighouse                                         |
| 8013E69D-C6AE-E311-B8ED-005056822391 | 10044229    | Bleakhouse Junior School                                                               |
| DD12E69D-C6AE-E311-B8ED-005056822391 | 10074775    | Enstone Primary School                                                                 |
| 836ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Vernon Infant School                                                                   |
| E912E69D-C6AE-E311-B8ED-005056822391 | NULL        | Rough Hay JMI School                                                                   |
| 87CDA8CD-C6AE-E311-B8ED-005056822391 | 10071005    | Copford C of E VC Primary School                                                       |
| E6CBA8CD-C6AE-E311-B8ED-005056822391 | 10071413    | St Mary Magdalene C of E Primary                                                       |
| 8465F591-C6AE-E311-B8ED-005056822391 | 10071301    | Becontree Primary                                                                      |
| 090B6485-C7AE-E311-B8ED-005056822391 | 10008485    | Shaw House School                                                                      |
| 8FC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Howard Infant & Nursery School                                                         |
| 3E68DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Park Hall Primary                                                                      |
| 338282EB-C6AE-E311-B8ED-005056822391 | NULL        | Highcrest Community School                                                             |
| 61F83F21-C7AE-E311-B8ED-005056822391 | NULL        | St Thoma's More Catholic Secondary School                                              |
| BF365C09-C7AE-E311-B8ED-005056822391 | 10008564    | Torquay Community College                                                              |
| 50D391DF-C6AE-E311-B8ED-005056822391 | 10071365    | Collingham Lady Elizabeth Hastings Church of England Voluntary Aided Primary School    |
| E3298AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Anne's RC Primary School                                                            |
| 271B5791-C7AE-E311-B8ED-005056822391 | 10015671    | Greig City                                                                             |
| CCD391DF-C6AE-E311-B8ED-005056822391 | NULL        | St Mary's C of E School                                                                |
| 44D591DF-C6AE-E311-B8ED-005056822391 | 10076252    | St Barnabas & St Philip C Primary School                                               |
| 082A8AE5-C6AE-E311-B8ED-005056822391 | 10071652    | St John Fisher RC Primary School                                                       |
| 0BB50486-C6AE-E311-B8ED-005056822391 | 10071778    | James Watt Primary School                                                              |
| C7D150A3-C7AE-E311-B8ED-005056822391 | 10017673    | Spa School                                                                             |
| D1615C8B-C7AE-E311-B8ED-005056822391 | 10079898    | Jewish Preparatory School                                                              |
| 66536A73-C7AE-E311-B8ED-005056822391 | 10008322    | The Kingsley School                                                                    |
| C78082EB-C6AE-E311-B8ED-005056822391 | 10008698    | Wyvern College                                                                         |
| 92F53F21-C7AE-E311-B8ED-005056822391 | 10006202    | St Joseph's Catholic College                                                           |
| DE3D451B-C7AE-E311-B8ED-005056822391 | NULL        | Ramsey Abbey School                                                                    |
| C4D87AF1-C6AE-E311-B8ED-005056822391 | 10017381    | Seaham School of Technology                                                            |
| 725C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Seasons Pre-School                                                                     |
| 56CC372D-C7AE-E311-B8ED-005056822391 | 10000697    | Bingley Grammar School                                                                 |
| 377B99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Mary's Church of England Primary School, Barnsley                                   |
| E163F591-C6AE-E311-B8ED-005056822391 | 10065820    | Priestmead Middle School                                                               |
| 43CCA8CD-C6AE-E311-B8ED-005056822391 | 10068542    | All Saints C of E Primary School                                                       |
| BC0CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Steeple Bumpstead Primary School                                                       |
| 0D996F6D-C7AE-E311-B8ED-005056822391 | 10014848    | Babington House School                                                                 |
| FC2C8AE5-C6AE-E311-B8ED-005056822391 | 10072049    | Waddington & West Bradford CE Primary School                                           |
| 6DC0D6A9-C6AE-E311-B8ED-005056822391 | 10078927    | Park Primary School                                                                    |
| C4CEA8CD-C6AE-E311-B8ED-005056822391 | 10068666    | St Thomas' CofE Primary School, Leigh                                                  |
| 0E3173F7-C6AE-E311-B8ED-005056822391 | 10003503    | John Spence Community High School                                                      |
| 2D12E69D-C6AE-E311-B8ED-005056822391 | 10069565    | Ledbury Primary School                                                                 |
| 76849443-C7AE-E311-B8ED-005056822391 | 10006277    | St Augustine's Catholic College                                                        |
| 0BE16303-C7AE-E311-B8ED-005056822391 | 10014834    | Bishop Fox's Communinty School                                                         |
| 0B71C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Tuckswood First School                                                                 |
| 6613E69D-C6AE-E311-B8ED-005056822391 | 10073003    | Broughton Infant School                                                                |
| 0912E69D-C6AE-E311-B8ED-005056822391 | 10080042    | Vaughan Nursery, First and Middle School                                               |
| 18E74C15-C7AE-E311-B8ED-005056822391 | 10014966    | Alec Hunter High School                                                                |
| E57D99D9-C6AE-E311-B8ED-005056822391 | 10068624    | St John's Church of England Voluntary Aided Junior and Infant School                   |
| BEE74C15-C7AE-E311-B8ED-005056822391 | 10006716    | The John Kyrle High School                                                             |
| C5D45197-C7AE-E311-B8ED-005056822391 | 10014926    | Abbey Hill School                                                                      |
| F776B0C7-C6AE-E311-B8ED-005056822391 | 10078080    | Lingfield Primary                                                                      |
| FA1EB8C1-C6AE-E311-B8ED-005056822391 | 10076959    | Woodcot Primary School                                                                 |
| 2AB50486-C6AE-E311-B8ED-005056822391 | 10069533    | Niton Primary School                                                                   |
| 57385C09-C7AE-E311-B8ED-005056822391 | NULL        | Keldholme School                                                                       |
| 327D99D9-C6AE-E311-B8ED-005056822391 | 10070148    | St James's Roman Catholic Primary School                                               |
| B02B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Mary's Catholic Primary School                                                      |
| F924A1D3-C6AE-E311-B8ED-005056822391 | NULL        | William Martin Church of England (V/C) Junior Scho                                     |
| 7A0A6485-C7AE-E311-B8ED-005056822391 | NULL        | Vinehall School                                                                        |
| D7CDA8CD-C6AE-E311-B8ED-005056822391 | 10081456    | Trinity CE Primary School                                                              |
| 9FC5BFBB-C6AE-E311-B8ED-005056822391 | 10080383    | John of Gaunt Infant and Nursery School                                                |
| B9536A73-C7AE-E311-B8ED-005056822391 | 10015343    | Chigwell School                                                                        |
| BC24A1D3-C6AE-E311-B8ED-005056822391 | 10077432    | Walton-on-Trent C of E School                                                          |
| 78CDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Andrews CE Primary                                                                  |
| 6BD591DF-C6AE-E311-B8ED-005056822391 | 10071697    | St Edmund's Catholic School                                                            |
| 1A6BDEA3-C6AE-E311-B8ED-005056822391 | 10079022    | Burbage CE Infant School                                                               |
| 0FD55197-C7AE-E311-B8ED-005056822391 | 10015000    | Athelstane School                                                                      |
| A2526A73-C7AE-E311-B8ED-005056822391 | 10013292    | The Bolitho School                                                                     |
| 191B5791-C7AE-E311-B8ED-005056822391 | 10015295    | Capital City Academy                                                                   |
| D6ED667F-C7AE-E311-B8ED-005056822391 | 10016228    | La Retraite Swan                                                                       |
| 8DD391DF-C6AE-E311-B8ED-005056822391 | 10070735    | St Herberts RC Primary School                                                          |
| 6A615C8B-C7AE-E311-B8ED-005056822391 | NULL        | Garden House Boys School                                                               |
| 35D391DF-C6AE-E311-B8ED-005056822391 | NULL        | St Georges CE Primary                                                                  |
| 7926A1D3-C6AE-E311-B8ED-005056822391 | NULL        | King William Street CE Primary                                                         |
| 02615C8B-C7AE-E311-B8ED-005056822391 | 10016483    | Merchant Taylors' School for Girls                                                     |
| C9C82DD3-C7AE-E311-B8ED-005056822391 | NULL        | Crewe And Alsager                                                                      |
| FD6ADEA3-C6AE-E311-B8ED-005056822391 | 10068848    | Manor Park Primary School                                                              |
| 85D55197-C7AE-E311-B8ED-005056822391 | 10017883    | Crown School and William Shrewsbury Primary School                                     |
| 8EC0D6A9-C6AE-E311-B8ED-005056822391 | 10041459    | Allenby Infant and Nursey School                                                       |
| 12CCA8CD-C6AE-E311-B8ED-005056822391 | 10075887    | St Oswald's Infant School                                                              |
| 2B7D99D9-C6AE-E311-B8ED-005056822391 | 10071428    | Trent CE Primary                                                                       |
| 5AE84C15-C7AE-E311-B8ED-005056822391 | 10006587    | Astley Cooper School                                                                   |
| 6C24A1D3-C6AE-E311-B8ED-005056822391 | 10070340    | Wrawby St Mary's Church of England Primary School                                      |
| 6DBCED97-C6AE-E311-B8ED-005056822391 | NULL        | Two Mile Hill Infant School                                                            |
| 6A2B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Joseph's Roman Catholic Junior School                                               |
| 3025A1D3-C6AE-E311-B8ED-005056822391 | 10079246    | Swnaton Morley Primary School                                                          |
| 96375C09-C7AE-E311-B8ED-005056822391 | 10005855    | Sir Bernard Lovell School                                                              |
| D88382EB-C6AE-E311-B8ED-005056822391 | NULL        | Ashmead Community School                                                               |
| 538A6BFD-C6AE-E311-B8ED-005056822391 | 10006012    | South Wirral High School                                                               |
| 0D536A73-C7AE-E311-B8ED-005056822391 | NULL        | Torah Academy                                                                          |
| 76C2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Toothill Primary School                                                                |
| 520A6485-C7AE-E311-B8ED-005056822391 | 10018715    | Downsend School                                                                        |
| 68D250A3-C7AE-E311-B8ED-005056822391 | NULL        | Willow Dene School                                                                     |
| DD5D0C80-C6AE-E311-B8ED-005056822391 | 10074181    | Bay Primary School                                                                     |
| 38C6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Ghost Hill First School                                                                |
| F23173F7-C6AE-E311-B8ED-005056822391 | 10003664    | Kingsbrook School                                                                      |
| FE19CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Silverdale Community Primary School                                                    |
| C871C7B5-C6AE-E311-B8ED-005056822391 | 10075344    | Petersgate Infant School                                                               |
| A61EB8C1-C6AE-E311-B8ED-005056822391 | 10075327    | Siskin Infant School                                                                   |
| A3D150A3-C7AE-E311-B8ED-005056822391 | 10017498    | Queen's Croft Community School                                                         |
| FD25A1D3-C6AE-E311-B8ED-005056822391 | 10079239    | Lyng C of E Primary School                                                             |
| F5BAED97-C6AE-E311-B8ED-005056822391 | 10076385    | Blanford Mere Primary School                                                           |
| 4AC8BFBB-C6AE-E311-B8ED-005056822391 | 10076080    | Hazeldown Primary School                                                               |
| 41AF3A27-C7AE-E311-B8ED-005056822391 | 10017374    | Saint Thomas More Catholic High School                                                 |
| 81C9BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Wyndcliffe Junior School                                                               |
| AED291DF-C6AE-E311-B8ED-005056822391 | 10076733    | St Peter's C of E Aided Primary School                                                 |
| 2EAF3A27-C7AE-E311-B8ED-005056822391 | 10016492    | Mount Carmel RC High School                                                            |
| 7B536A73-C7AE-E311-B8ED-005056822391 | 10071151    | Hale Prep School                                                                       |
| B812E69D-C6AE-E311-B8ED-005056822391 | NULL        | Victoria Infant School                                                                 |
| 56AF3A27-C7AE-E311-B8ED-005056822391 | NULL        | The John Downe Primary School                                                          |
| F8C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Benyon County Primary School                                                           |
| 2F18CFAF-C6AE-E311-B8ED-005056822391 | 10069929    | Earby Springfield Primary School                                                       |
| 6D1FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Swindon Primary School 2                                                               |
| DF69DEA3-C6AE-E311-B8ED-005056822391 | 10076382    | Russells Hall Primary School                                                           |
| 932B8AE5-C6AE-E311-B8ED-005056822391 | 10072053    | Mere Brow CE Primary School                                                            |
| 933D451B-C7AE-E311-B8ED-005056822391 | NULL        | Rawlins Community College.                                                             |
| DB11E69D-C6AE-E311-B8ED-005056822391 | NULL        | Leigh North Street Junior School                                                       |
| FF8182EB-C6AE-E311-B8ED-005056822391 | 10002945    | Haybridge High School                                                                  |
| DF996F6D-C7AE-E311-B8ED-005056822391 | 10017780    | St Dominic's School                                                                    |
| A8CB372D-C7AE-E311-B8ED-005056822391 | 10069676    | Upshire Primary Foundation School                                                      |
| 30E54C15-C7AE-E311-B8ED-005056822391 | 10015600    | Woodbrook Vale High School                                                             |
| DEC7BFBB-C6AE-E311-B8ED-005056822391 | 10069172    | Netley Primary School                                                                  |
| BAE74C15-C7AE-E311-B8ED-005056822391 | 10004853    | Oldbury Wells School                                                                   |
| FA7D99D9-C6AE-E311-B8ED-005056822391 | 10068723    | Cumberworth Church of England Voluntary Aided First School                             |
| 89CB372D-C7AE-E311-B8ED-005056822391 | NULL        | R A Butler Schools                                                                     |
| 5A13E69D-C6AE-E311-B8ED-005056822391 | 10075295    | Ridgeway Primary School                                                                |
| 12B60486-C6AE-E311-B8ED-005056822391 | NULL        | Feltham Hill Jm School                                                                 |
| 537B99D9-C6AE-E311-B8ED-005056822391 | 10068568    | St Thomas (Moorside)                                                                   |
| 4E17CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Werrington Primary School                                                              |
| C0DA7AF1-C6AE-E311-B8ED-005056822391 | 10017274    | Avondale High School                                                                   |
| 0DCDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Thomas CE Primary School                                                            |
| A40C6579-C7AE-E311-B8ED-005056822391 | 10015203    | Wycliffe College                                                                       |
| 731A7067-C7AE-E311-B8ED-005056822391 | 10015575    | Hassenbrook School                                                                     |
| 67DF6303-C7AE-E311-B8ED-005056822391 | 10005794    | Sheldon Heath Community School                                                         |
| B97C99D9-C6AE-E311-B8ED-005056822391 | NULL        | Tenbury C of E Primary School                                                          |
| 7F5E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Aspley Guise Lower School                                                              |
| 33D291DF-C6AE-E311-B8ED-005056822391 | NULL        | St Peters Catholic Primary School                                                      |
| 701ACFAF-C6AE-E311-B8ED-005056822391 | 10081265    | Mosborough Primary School                                                              |
| 96C2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Norham Community First School                                                          |
| 169A6F6D-C7AE-E311-B8ED-005056822391 | 10006235    | St Marys School, Ascot                                                                 |
| ACA77A5B-C7AE-E311-B8ED-005056822391 | 10005849    | Simon Langton Grammar School for Boys                                                  |
| A610E69D-C6AE-E311-B8ED-005056822391 | 10076572    | Grendon Primary School                                                                 |
| EA886BFD-C6AE-E311-B8ED-005056822391 | NULL        | Mitcham Vale High School                                                               |
| 287A589D-C7AE-E311-B8ED-005056822391 | 10069454    | Willow Grove Primary School                                                            |
| D175B0C7-C6AE-E311-B8ED-005056822391 | 10069895    | Shakespeare Primary School                                                             |
| 86E06303-C7AE-E311-B8ED-005056822391 | 10006872    | Wye Valley School                                                                      |
| 72BAED97-C6AE-E311-B8ED-005056822391 | 10075666    | Wibsey Primary School                                                                  |
| CFD45197-C7AE-E311-B8ED-005056822391 | 10015890    | Highfields Special School                                                              |
| 30DB7AF1-C6AE-E311-B8ED-005056822391 | 10004617    | Newmarket Upper School                                                                 |
| 1317CFAF-C6AE-E311-B8ED-005056822391 | 10080748    | South Kirkby Common Road Infant and Nursery School                                     |
| ED62F591-C6AE-E311-B8ED-005056822391 | 10077150    | Whitehouse Primary School                                                              |
| 34D250A3-C7AE-E311-B8ED-005056822391 | NULL        | Townhouse School                                                                       |
| 791FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Glenbrook Infants School                                                               |
| 7DBFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | William Reynolds Infant                                                                |
| F98082EB-C6AE-E311-B8ED-005056822391 | NULL        | Kimberworth Comprehensive School                                                       |
| BAFE7261-C7AE-E311-B8ED-005056822391 | 10006227    | St Marys High School                                                                   |
| 7EF83F21-C7AE-E311-B8ED-005056822391 | NULL        | All Hollows Catholic High School                                                       |
| 606BDEA3-C6AE-E311-B8ED-005056822391 | 10076529    | Frederick Bird Primary School                                                          |
| FB13E69D-C6AE-E311-B8ED-005056822391 | NULL        | Maulden Lower School                                                                   |
| F119CFAF-C6AE-E311-B8ED-005056822391 | 10080440    | Hiltingbury Infant School                                                              |
| 527C99D9-C6AE-E311-B8ED-005056822391 | 10079437    | Holy Name RC School                                                                    |
| 2C75B0C7-C6AE-E311-B8ED-005056822391 | 10080700    | Warley Primary School                                                                  |
| 3EAF3A27-C7AE-E311-B8ED-005056822391 | 10001414    | Christ The King School                                                                 |
| 756F35CD-C7AE-E311-B8ED-005056822391 | NULL        | The Open University                                                                    |
| FDCCA8CD-C6AE-E311-B8ED-005056822391 | 10080559    | Brockton CE Primary School                                                             |
| A57A589D-C7AE-E311-B8ED-005056822391 | 10014882    | Amwell View School                                                                     |
| 09365C09-C7AE-E311-B8ED-005056822391 | 10007474    | Wheldon School                                                                         |
| AB6EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Greenleas Lower School                                                                 |
| EDBFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Newbridge Junior School                                                                |
| D81A7067-C7AE-E311-B8ED-005056822391 | 10006114    | St Anselm's College                                                                    |
| 7D23A1D3-C6AE-E311-B8ED-005056822391 | 10071012    | Pevensey & Westham                                                                     |
| A15D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Somerset Nursery School and Children's Centre                                          |
| 3AC7BFBB-C6AE-E311-B8ED-005056822391 | 10080419    | Badshot Lea Villa                                                                      |
| D8F73F21-C7AE-E311-B8ED-005056822391 | 10001167    | Cardinal Wiseman RC School                                                             |
| 0D3D451B-C7AE-E311-B8ED-005056822391 | 10000344    | Archbishop Tenison's C of E High School                                                |
| C270C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Norman First & Nursery School                                                          |
| 9E7D99D9-C6AE-E311-B8ED-005056822391 | 10074687    | St Aidan's C of E School                                                               |
| CCB30486-C6AE-E311-B8ED-005056822391 | 10078424    | Marsh Green Primary School                                                             |
| C2F53F21-C7AE-E311-B8ED-005056822391 | NULL        | The Bishop's Blue Coat C of E High School                                              |
| 15CEA8CD-C6AE-E311-B8ED-005056822391 | 10078921    | Mortimer St Mary's C.E. Junior School                                                  |
| 20D291DF-C6AE-E311-B8ED-005056822391 | NULL        | St Mary's RC Primary                                                                   |
| 8E671799-C9AE-E311-B8ED-005056822391 | NULL        | Gloucester Initial Teacher Education Partnership GTP                                   |
| B012E69D-C6AE-E311-B8ED-005056822391 | 10080674    | Walmore Hill Primary School                                                            |
| AC0A6485-C7AE-E311-B8ED-005056822391 | NULL        | The Avenue                                                                             |
| 95D150A3-C7AE-E311-B8ED-005056822391 | NULL        | Ifield School                                                                          |
| 60CEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Cheetham C of E Community School                                                       |
| EC63F591-C6AE-E311-B8ED-005056822391 | 10076584    | Woodford Green Primary                                                                 |
| 4A2C8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Bethany CE VA Junior School                                                            |
| 3FFE7261-C7AE-E311-B8ED-005056822391 | NULL        | Holmshill School                                                                       |
| 5B77B0C7-C6AE-E311-B8ED-005056822391 | 10077745    | Braywood CofE First School                                                             |
| 990A6485-C7AE-E311-B8ED-005056822391 | 10004699    | North London Collegiate School                                                         |
| 679A6F6D-C7AE-E311-B8ED-005056822391 | 10071115    | Lancashire Independent School                                                          |
| 1DA77A5B-C7AE-E311-B8ED-005056822391 | 10006185    | St Joan of Arc Catholic School                                                         |
| 97DF6303-C7AE-E311-B8ED-005056822391 | 10017642    | South Axholme Community School                                                         |
| D33E451B-C7AE-E311-B8ED-005056822391 | 10000727    | Bishop Luffa C of E Comprehensive School                                               |
| A3AF3A27-C7AE-E311-B8ED-005056822391 | 10073719    | Elmwood Primary School                                                                 |
| CBB40486-C6AE-E311-B8ED-005056822391 | NULL        | Portfield Community Primary                                                            |
| EC7A589D-C7AE-E311-B8ED-005056822391 | 10016331    | Kennel Lane School                                                                     |
| 977A589D-C7AE-E311-B8ED-005056822391 | 10076994    | Springwell School                                                                      |
| ACE54C15-C7AE-E311-B8ED-005056822391 | 10016241    | Longfield School                                                                       |
| 1CEE7F55-C7AE-E311-B8ED-005056822391 | 10007648    | Wrenn School                                                                           |
| CA375C09-C7AE-E311-B8ED-005056822391 | 10006668    | The Friary School                                                                      |
| 59B9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Ray Lodge Primary                                                                      |
| FC526A73-C7AE-E311-B8ED-005056822391 | 10078223    | Milton Keynes Prep School                                                              |
| 3524A1D3-C6AE-E311-B8ED-005056822391 | 10075714    | Wavendon C of E First                                                                  |
| 9E096485-C7AE-E311-B8ED-005056822391 | 10008320    | The King's School                                                                      |
| 3CB30486-C6AE-E311-B8ED-005056822391 | 10072151    | Coombe Road School                                                                     |
| EB5D0C80-C6AE-E311-B8ED-005056822391 | 10081406    | Abbey Meads Community Primary School                                                   |
| 0C1EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Noak Bridge Primary School                                                             |
| 7BBBED97-C6AE-E311-B8ED-005056822391 | 10077149    | Durham Lane Primary School                                                             |
| A8CDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | The Blue Coat School                                                                   |
| 51EE667F-C7AE-E311-B8ED-005056822391 | 10008223    | Embley Park School                                                                     |
| DFDA7AF1-C6AE-E311-B8ED-005056822391 | 10003153    | Hornsey School for Girls                                                               |
| 1EC7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Delce Junior School                                                                    |
| 0F8282EB-C6AE-E311-B8ED-005056822391 | 10001399    | Chiping Norton School                                                                  |
| 712D8AE5-C6AE-E311-B8ED-005056822391 | 10001898    | Debden Park High School                                                                |
| 8A7A99D9-C6AE-E311-B8ED-005056822391 | 10080603    | Wistow Parochial C.E Primary School                                                    |
| 63C9BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Bligh Junior School                                                                    |
| 0E8382EB-C6AE-E311-B8ED-005056822391 | NULL        | John Penrose School                                                                    |
| F75D0C80-C6AE-E311-B8ED-005056822391 | 10057468    | Widden CP School                                                                       |
| 8FB50486-C6AE-E311-B8ED-005056822391 | NULL        | Gillingham Primary School                                                              |
| BC17CFAF-C6AE-E311-B8ED-005056822391 | 10071326    | Falcon Brook Primary School                                                            |
| AACCA8CD-C6AE-E311-B8ED-005056822391 | 10076903    | Millbrook JMI School                                                                   |
| 5FC1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Hollington Primary School                                                              |
| F670C7B5-C6AE-E311-B8ED-005056822391 | 10076401    | Woodthorpe Junior & Infants School                                                     |
| 9D2A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Bede's Catholic Comprehensive School                                                |
| 4AE54C15-C7AE-E311-B8ED-005056822391 | 10014987    | Abraham Moss High School                                                               |
| 7D19CFAF-C6AE-E311-B8ED-005056822391 | 10068948    | Hillside Primary School                                                                |
| 0F0BFD8B-C6AE-E311-B8ED-005056822391 | 10078056    | Bannockburn Primary School                                                             |
| D6D150A3-C7AE-E311-B8ED-005056822391 | 10015110    | Clifton Hill School                                                                    |
| 47F63F21-C7AE-E311-B8ED-005056822391 | 10016880    | Our Lady and St John RC High School                                                    |
| 6D70C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Oughton Primary and Nursery School                                                     |
| 9F7A589D-C7AE-E311-B8ED-005056822391 | 10015739    | Wennington Hall School                                                                 |
| BEBFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Pound Hill First School                                                                |
| 29408F49-C7AE-E311-B8ED-005056822391 | 10004758    | Northolt High School                                                                   |
| 7977B0C7-C6AE-E311-B8ED-005056822391 | 10068636    | Triangle CofE VC Primary School                                                        |
| 836BDEA3-C6AE-E311-B8ED-005056822391 | NULL        | Rusnul Lower School                                                                    |
| 566ADEA3-C6AE-E311-B8ED-005056822391 | 10069010    | Wansdyke Primary School                                                                |
| E9E06303-C7AE-E311-B8ED-005056822391 | 10017694    | Tanfield Comprehensive School                                                          |
| BC6ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Glenmere Community Primary School                                                      |
| E90CFD8B-C6AE-E311-B8ED-005056822391 | 10072556    | Church Lane Infant School                                                              |
| 48AF3A27-C7AE-E311-B8ED-005056822391 | 10073710    | Oak Farm Junior School                                                                 |
| CBBBED97-C6AE-E311-B8ED-005056822391 | 10073242    | Portsdale Infants School                                                               |
| B3C6BFBB-C6AE-E311-B8ED-005056822391 | 10076965    | Bitterne Park School                                                                   |
| 7EE74C15-C7AE-E311-B8ED-005056822391 | 10003121    | Holly Lodge Girls College                                                              |
| D4886BFD-C6AE-E311-B8ED-005056822391 | 10002142    | Easthampstead Park School                                                              |
| DCCEA8CD-C6AE-E311-B8ED-005056822391 | 10071000    | Heathlands C of E Primary School                                                       |
| 955F0C80-C6AE-E311-B8ED-005056822391 | 10076221    | Intack Primary School                                                                  |
| F97C99D9-C6AE-E311-B8ED-005056822391 | 10075789    | Wakefield St Johns Church of England Voluntary Aided Junior and Infant School          |
| D576B0C7-C6AE-E311-B8ED-005056822391 | 10076233    | Castledyke Primary School                                                              |
| F9B30486-C6AE-E311-B8ED-005056822391 | 10079758    | Bratton Primary School                                                                 |
| 237E99D9-C6AE-E311-B8ED-005056822391 | NULL        | Sacred Heart RC Primary School                                                         |
| C41EB8C1-C6AE-E311-B8ED-005056822391 | 10075296    | Newdigate Primary School                                                               |
| 551EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Thornhill Pr. Sch                                                                      |
| 0465F591-C6AE-E311-B8ED-005056822391 | NULL        | North Cray Primary School                                                              |
| D920B8C1-C6AE-E311-B8ED-005056822391 | 10073376    | Martongate Primary School                                                              |
| FF61F591-C6AE-E311-B8ED-005056822391 | 10069559    | Abel Smith School                                                                      |
| 9AF83F21-C7AE-E311-B8ED-005056822391 | 10069592    | Osidge Primary School                                                                  |
| 62DA7AF1-C6AE-E311-B8ED-005056822391 | 10015889    | Fred Longworth High School                                                             |
| 9E1FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Acresfield Community Primary School                                                    |
| FE76B0C7-C6AE-E311-B8ED-005056822391 | 10069614    | Chandlers Field School                                                                 |
| 94C5BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Valley Park Community School                                                           |
| 10615C8B-C7AE-E311-B8ED-005056822391 | NULL        | St Margarets Senior School                                                             |
| A01DB8C1-C6AE-E311-B8ED-005056822391 | 10079533    | Sheredes Primary School                                                                |
| 96526A73-C7AE-E311-B8ED-005056822391 | 10016265    | King's School                                                                          |
| 5CC0D6A9-C6AE-E311-B8ED-005056822391 | 10068804    | Moston Fields Primary School                                                           |
| ED20B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Winter Gardens School                                                                  |
| 0D68DEA3-C6AE-E311-B8ED-005056822391 | 10070952    | New Invention Infant School                                                            |
| BDCCA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Potter Street Primary School                                                           |
| AEBCED97-C6AE-E311-B8ED-005056822391 | NULL        | Noel Park Infant School                                                                |
| 6768DEA3-C6AE-E311-B8ED-005056822391 | 10072481    | Walter Halls Primary School                                                            |
| D9D491DF-C6AE-E311-B8ED-005056822391 | NULL        | St Gregory's RC School                                                                 |
| 4FCEA8CD-C6AE-E311-B8ED-005056822391 | 10068625    | Crowlees Church of England Voluntary Controlled Junior and Infant School               |
| 763C451B-C7AE-E311-B8ED-005056822391 | 10018661    | Ryecroft Middle School                                                                 |
| 970A6485-C7AE-E311-B8ED-005056822391 | 10077614    | Ashgrove School                                                                        |
| 562D8AE5-C6AE-E311-B8ED-005056822391 | 10004669    | North Bromsgrove High School                                                           |
| F769DEA3-C6AE-E311-B8ED-005056822391 | 10080666    | Callowell Primary School                                                               |
| 307C99D9-C6AE-E311-B8ED-005056822391 | 10070232    | St Paul's Catholic Primary School                                                      |
| 8020B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Southway Primary                                                                       |
| 3A9A6F6D-C7AE-E311-B8ED-005056822391 | 10015523    | Ellesmere College                                                                      |
| 47876BFD-C6AE-E311-B8ED-005056822391 | NULL        | Maiden Erlegh School                                                                   |
| 8FD150A3-C7AE-E311-B8ED-005056822391 | 10015350    | Chorley Astley Park School                                                             |
| AA2A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Elizabeths Catholic Primary School                                                  |
| A10C6579-C7AE-E311-B8ED-005056822391 | 10008175    | Combe Bank Preparatory School                                                          |
| 5BD291DF-C6AE-E311-B8ED-005056822391 | 10077739    | St Edward's Catholic First School                                                      |
| F6CBA8CD-C6AE-E311-B8ED-005056822391 | 10078489    | Brading VC Primary School                                                              |
| 4C7A589D-C7AE-E311-B8ED-005056822391 | 10018551    | The Walnuts School                                                                     |
| FE64F591-C6AE-E311-B8ED-005056822391 | NULL        | Ludlow Infant School                                                                   |
| A47C99D9-C6AE-E311-B8ED-005056822391 | 10079277    | The Swineshead St Mary's Church of England Primary                                     |
| 0EC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Tilehouse Combined School                                                              |
| D47A589D-C7AE-E311-B8ED-005056822391 | 10014843    | Beaumont Hill School                                                                   |
| DB096485-C7AE-E311-B8ED-005056822391 | 10071146    | Roselyon School                                                                        |
| E4BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Handford Hall County Primary School                                                    |
| 27B30486-C6AE-E311-B8ED-005056822391 | NULL        | Cypress Infant School                                                                  |
| 98CDA8CD-C6AE-E311-B8ED-005056822391 | 10070379    | Ponsbourne St                                                                          |
| C75D0C80-C6AE-E311-B8ED-005056822391 | 10070119    | Brandwood Community School                                                             |
| 9CAF3A27-C7AE-E311-B8ED-005056822391 | NULL        | Hayes School                                                                           |
| F4C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Lordswood Junior School                                                                |
| 0775B0C7-C6AE-E311-B8ED-005056822391 | 10071336    | Halley Primary School                                                                  |
| D60CFD8B-C6AE-E311-B8ED-005056822391 | 10069960    | Norwood Primary                                                                        |
| BF7A99D9-C6AE-E311-B8ED-005056822391 | 10073958    | Wanstead Church School                                                                 |
| 5CD491DF-C6AE-E311-B8ED-005056822391 | 10071780    | Caedmon Primary School                                                                 |
| 231ACFAF-C6AE-E311-B8ED-005056822391 | NULL        | Bushmead Junior School                                                                 |
| 2819CFAF-C6AE-E311-B8ED-005056822391 | 10076554    | Yorkmead Junior and Infant School                                                      |
| 286BDEA3-C6AE-E311-B8ED-005056822391 | NULL        | Kessingland Cvp School                                                                 |
| 5724A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Thurston C of E Promary School                                                         |
| EBB9ED97-C6AE-E311-B8ED-005056822391 | 10080006    | Poplar First School                                                                    |
| 9A0C6579-C7AE-E311-B8ED-005056822391 | 10006333    | Stockport Grammar School                                                               |
| 56D291DF-C6AE-E311-B8ED-005056822391 | 10074874    | Bishop Purseglove C of E Primary School                                                |
| 16B40486-C6AE-E311-B8ED-005056822391 | NULL        | Carterhatch Junior School                                                              |
| 4CF63F21-C7AE-E311-B8ED-005056822391 | 10017607    | St Michael's RC VA Comprehensive School                                                |
| CBDB7AF1-C6AE-E311-B8ED-005056822391 | 10006612    | The Brakenhale School                                                                  |
| E561F591-C6AE-E311-B8ED-005056822391 | 10070941    | Glodwick Infant & Nursery                                                              |
| C6DE6303-C7AE-E311-B8ED-005056822391 | 10007390    | Wensleydale School                                                                     |
| 9576B0C7-C6AE-E311-B8ED-005056822391 | 10069435    | Eglington Primary School                                                               |
| E7D491DF-C6AE-E311-B8ED-005056822391 | 10074754    | Winterslow CE (Aided) Primary School                                                   |
| 23B03A27-C7AE-E311-B8ED-005056822391 | 10079656    | Dereham Church                                                                         |
| 2D19CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Poulton Lancelyn Primary School                                                        |
| B47A589D-C7AE-E311-B8ED-005056822391 | NULL        | Bridge View Special School                                                             |
| 94D250A3-C7AE-E311-B8ED-005056822391 | NULL        | The College of Richard Collyer                                                         |
| 5B6FC7B5-C6AE-E311-B8ED-005056822391 | 10070080    | Manley Park Primary School                                                             |
| F611E69D-C6AE-E311-B8ED-005056822391 | 10077749    | Hilltop First School                                                                   |
| CAD291DF-C6AE-E311-B8ED-005056822391 | 10071398    | Westhoughton Parochial C of E Primary School                                           |
| 98C5BFBB-C6AE-E311-B8ED-005056822391 | 10073020    | Fordingbridge Junior School                                                            |
| 3964F591-C6AE-E311-B8ED-005056822391 | 10079015    | Fakenham Junior School                                                                 |
| 663F8F49-C7AE-E311-B8ED-005056822391 | NULL        | Charlton                                                                               |
| FD2A8AE5-C6AE-E311-B8ED-005056822391 | 10080237    | Garstang St Thomas Church of England School                                            |
| 78A77A5B-C7AE-E311-B8ED-005056822391 | 10006223    | St Mary's High School                                                                  |
| 9D18CFAF-C6AE-E311-B8ED-005056822391 | 10070881    | Stonebroom Primary School                                                              |
| 53CC372D-C7AE-E311-B8ED-005056822391 | 10006685    | The Hayfield School                                                                    |
| A5ED667F-C7AE-E311-B8ED-005056822391 | 10043894    | Chase Academy Isc                                                                      |
| D826A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Debden Park High School                                                                |
| A374B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Boxgrove Primary School                                                                |
| 3A26A1D3-C6AE-E311-B8ED-005056822391 | 10076282    | All Saints CE Junior School                                                            |
| AB996F6D-C7AE-E311-B8ED-005056822391 | 10008557    | Royal High School GDST                                                                 |
| 366ADEA3-C6AE-E311-B8ED-005056822391 | 10080398    | Westwood Farm Infant School                                                            |
| CA20B8C1-C6AE-E311-B8ED-005056822391 | 10077137    | Marlborough School                                                                     |
| B2C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Lingfield Primary School                                                               |
| 27DE6303-C7AE-E311-B8ED-005056822391 | 10000761    | Blake High School                                                                      |
| A2E64C15-C7AE-E311-B8ED-005056822391 | NULL        | Kingsland School                                                                       |
| 9F62F591-C6AE-E311-B8ED-005056822391 | 10078645    | Belton Lane C.P School                                                                 |
| 71E74C15-C7AE-E311-B8ED-005056822391 | 10002921    | Haslingden High School                                                                 |
| A7896BFD-C6AE-E311-B8ED-005056822391 | 10014999    | Biddick School Sports College                                                          |
| 6CCB372D-C7AE-E311-B8ED-005056822391 | 10072413    | The Cathedral School                                                                   |
| 5C0EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Pinner Wood Middle School                                                              |
| 49BCED97-C6AE-E311-B8ED-005056822391 | 10077870    | Payne Primary School                                                                   |
| D8B30486-C6AE-E311-B8ED-005056822391 | NULL        | Bradford Moor Community Primary School                                                 |
| 1B7E99D9-C6AE-E311-B8ED-005056822391 | NULL        | Seer Green CE Combined School                                                          |
| 4B19CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Bell Lane Combined School                                                              |
| 1DC9BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Stanford-le-Hope Infant School                                                         |
| E1986F6D-C7AE-E311-B8ED-005056822391 | 10008531    | Stoodley Knowle School                                                                 |
| 8C68DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Lutley Primary School                                                                  |
| 1C5F0C80-C6AE-E311-B8ED-005056822391 | NULL        | Longshaw Primary School                                                                |
| 29B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Charville Primary School                                                               |
| AC1EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Yealmpton Primary School                                                               |
| D4355C09-C7AE-E311-B8ED-005056822391 | 10017224    | Perry Beeches Secondary School                                                         |
| 8A1EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Millhouse Infant School and Nursery                                                    |
| 968382EB-C6AE-E311-B8ED-005056822391 | 10018564    | Clare Middle School                                                                    |
| 6D77B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Shanklin CE Primary School                                                             |
| 9612E69D-C6AE-E311-B8ED-005056822391 | NULL        | Upperwood Academy                                                                      |
| 970BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Park Lane Primary School                                                               |
| 37D87AF1-C6AE-E311-B8ED-005056822391 | 10017745    | St James' High School                                                                  |
| A9385C09-C7AE-E311-B8ED-005056822391 | 10015256    | Worle Community School                                                                 |
| 2A6FC7B5-C6AE-E311-B8ED-005056822391 | 10075111    | Albany Junior School                                                                   |
| 7BC0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Alderney Middle School                                                                 |
| 7DD591DF-C6AE-E311-B8ED-005056822391 | 10070759    | St Chad's RC Primary School                                                            |
| F6C7BFBB-C6AE-E311-B8ED-005056822391 | 10069546    | High Beeches Primary School                                                            |
| C6AF3A27-C7AE-E311-B8ED-005056822391 | 10069657    | Parkside Community School                                                              |
| A83D451B-C7AE-E311-B8ED-005056822391 | 10000262    | Altwood C of E Secondary School                                                        |
| 783173F7-C6AE-E311-B8ED-005056822391 | NULL        | Berkeley Vale Community                                                                |
| 03BAED97-C6AE-E311-B8ED-005056822391 | 10079588    | Hertford Heath Primary School                                                          |
| F618CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Somerford Nursery & Infant Community School                                            |
| 1D0EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Bricknell Primary                                                                      |
| 812C8AE5-C6AE-E311-B8ED-005056822391 | 10072674    | St Michael Amd St Johns RC Primary School                                              |
| 3D13E69D-C6AE-E311-B8ED-005056822391 | 10069974    | Kings Hill Primary School                                                              |
| 08BBED97-C6AE-E311-B8ED-005056822391 | 10070940    | Springhead Infant and Nursery School                                                   |
| C81B5791-C7AE-E311-B8ED-005056822391 | 10015005    | Beverley School for Autism                                                             |
| A3B60486-C6AE-E311-B8ED-005056822391 | 10071476    | Whitgreave Junior                                                                      |
| BBED667F-C7AE-E311-B8ED-005056822391 | 10008103    | Birkenhead High School                                                                 |
| 18D591DF-C6AE-E311-B8ED-005056822391 | NULL        | St Johns RC Primary School                                                             |
| F2CCA8CD-C6AE-E311-B8ED-005056822391 | 10075749    | Burghfield St Mary's C.E. Primary School                                               |
| B571C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Mowmacre Hill Primary School                                                           |
| D262F591-C6AE-E311-B8ED-005056822391 | NULL        | Seal Primary School                                                                    |
| 338E540F-C7AE-E311-B8ED-005056822391 | 10015981    | James Brindley High School                                                             |
| 7477B0C7-C6AE-E311-B8ED-005056822391 | 10076299    | Christ Church CE Junior School                                                         |
| 222C8AE5-C6AE-E311-B8ED-005056822391 | 10045987    | Holy Family Catholic Primary School                                                    |
| 5F1B5791-C7AE-E311-B8ED-005056822391 | 10016162    | Lansdowne School                                                                       |
| D126A1D3-C6AE-E311-B8ED-005056822391 | 10077917    | Sunningwell Primary School                                                             |
| EE866BFD-C6AE-E311-B8ED-005056822391 | 10015924    | Haydock High School                                                                    |
| 1875B0C7-C6AE-E311-B8ED-005056822391 | 10078036    | Charlton Manor Primary School                                                          |
| 49B40486-C6AE-E311-B8ED-005056822391 | 10069154    | Heathfield Junior School                                                               |
| 46EE667F-C7AE-E311-B8ED-005056822391 | 10018625    | Arnold House School                                                                    |
| 980CFD8B-C6AE-E311-B8ED-005056822391 | 10076526    | Mount Pleasant Primary School                                                          |
| 8A0CFD8B-C6AE-E311-B8ED-005056822391 | 10070015    | Lovelace Primary School                                                                |
| C5BFD6A9-C6AE-E311-B8ED-005056822391 | 10069142    | Quarry Bank Primary School                                                             |
| 47BBED97-C6AE-E311-B8ED-005056822391 | 10069378    | Forest Hall Primary School                                                             |
| 95ED667F-C7AE-E311-B8ED-005056822391 | 10014937    | Crookey Hall School                                                                    |
| 44876BFD-C6AE-E311-B8ED-005056822391 | 10017422    | Redmoor High School                                                                    |
| 331DB8C1-C6AE-E311-B8ED-005056822391 | 10075334    | Talavera Infant School                                                                 |
| 483173F7-C6AE-E311-B8ED-005056822391 | 10007573    | Wollaston School                                                                       |
| FC11E69D-C6AE-E311-B8ED-005056822391 | 10079584    | St Peters School                                                                       |
| 28BAED97-C6AE-E311-B8ED-005056822391 | NULL        | Thomas Gamuel Primary                                                                  |
| C40DFD8B-C6AE-E311-B8ED-005056822391 | 10073153    | Claregate Primary School                                                               |
| FEBFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Princethorpe Infant & Nursery School                                                   |
| AA1B5791-C7AE-E311-B8ED-005056822391 | NULL        | St John's School (Brighton)                                                            |
| DCAF3A27-C7AE-E311-B8ED-005056822391 | NULL        | Send C of E First School                                                               |
| D17A99D9-C6AE-E311-B8ED-005056822391 | NULL        | Barrow C.E. (Aided)                                                                    |
| 4D1B7067-C7AE-E311-B8ED-005056822391 | 10078255    | Manchester Jewish Grammar School                                                       |
| 4BD591DF-C6AE-E311-B8ED-005056822391 | 10073451    | Downholland Haskayne Primary School                                                    |
| A98282EB-C6AE-E311-B8ED-005056822391 | 10008539    | Swavesey Village College                                                               |
| 7EFF7261-C7AE-E311-B8ED-005056822391 | 10001343    | Chatham House Grammar                                                                  |
| 396EC7B5-C6AE-E311-B8ED-005056822391 | 10069228    | Oakridge Primary School                                                                |
| FEE74C15-C7AE-E311-B8ED-005056822391 | NULL        | Isaac Newton School                                                                    |
| 81CCA8CD-C6AE-E311-B8ED-005056822391 | 10069457    | St John and St James Primary School                                                    |
| FCC7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Joydens Wood Junior School                                                             |
| 470CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Woolpit CP School                                                                      |
| AE11E69D-C6AE-E311-B8ED-005056822391 | NULL        | Fleetville Infant School                                                               |
| 841B5791-C7AE-E311-B8ED-005056822391 | 10017421    | Rigby Special Day School                                                               |
| EDCEA8CD-C6AE-E311-B8ED-005056822391 | 10078841    | Great Waldingford CE VC Primary School                                                 |
| 86BCED97-C6AE-E311-B8ED-005056822391 | 10079698    | Eversley Primary School                                                                |
| F096874F-C7AE-E311-B8ED-005056822391 | 10007689    | The Yarborough School, Lincoln                                                         |
| 5474B0C7-C6AE-E311-B8ED-005056822391 | 10071312    | George Eliot Junior                                                                    |
| 2A6ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Bromley-Pensnett School                                                                |
| 5F395C09-C7AE-E311-B8ED-005056822391 | 10016695    | Norton College                                                                         |
| EB6ADEA3-C6AE-E311-B8ED-005056822391 | 10071215    | Tower View Primary                                                                     |
| 695E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Benson Primary School                                                                  |
| 2FBBED97-C6AE-E311-B8ED-005056822391 | 10075176    | Beis Yaakov Primary School                                                             |
| 1920B8C1-C6AE-E311-B8ED-005056822391 | 10077250    | Bentfield Primary School                                                               |
| 2E2A8AE5-C6AE-E311-B8ED-005056822391 | 10072037    | Bishop King CE Primary                                                                 |
| 03D250A3-C7AE-E311-B8ED-005056822391 | 10017902    | Lindsworth School                                                                      |
| 76E74C15-C7AE-E311-B8ED-005056822391 | 10015030    | Albany High School                                                                     |
| DA68DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Shillington Lower School                                                               |
| 063D451B-C7AE-E311-B8ED-005056822391 | 10006728    | The Kings of Wessex Community School                                                   |
| E662F591-C6AE-E311-B8ED-005056822391 | 10072577    | Fixby Junior and Infant School                                                         |
| 807A99D9-C6AE-E311-B8ED-005056822391 | 10080572    | Hagbourne CE Primary School                                                            |
| CF70C7B5-C6AE-E311-B8ED-005056822391 | 10075097    | Meath Green Junior School                                                              |
| 0ECEA8CD-C6AE-E311-B8ED-005056822391 | 10068575    | St Luke's CE Primary School                                                            |
| 7A7B99D9-C6AE-E311-B8ED-005056822391 | 10079427    | St Patricks Catholic Primary School                                                    |
| 6177B0C7-C6AE-E311-B8ED-005056822391 | 10074866    | Withycombe Raleigh CE Primary                                                          |
| E3C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Castle Hill Infant School                                                              |
| 335E0C80-C6AE-E311-B8ED-005056822391 | 10073647    | Waulud Primary School                                                                  |
| 6BD97AF1-C6AE-E311-B8ED-005056822391 | NULL        | Moor End Academy                                                                       |
| B511E69D-C6AE-E311-B8ED-005056822391 | 10072554    | Kirton County Primary School                                                           |
| 6CE44C15-C7AE-E311-B8ED-005056822391 | 10016737    | Oak Farm Community School                                                              |
| 66B03A27-C7AE-E311-B8ED-005056822391 | NULL        | West Flegg Middle School                                                               |
| 033F8F49-C7AE-E311-B8ED-005056822391 | 10000668    | Beverley Boys School                                                                   |
| 45AF3A27-C7AE-E311-B8ED-005056822391 | 10017706    | St Bernard's Catholic High School, Specialist School for the Arts and Applied Learning |
| E87C99D9-C6AE-E311-B8ED-005056822391 | 10077939    | Kneesall C of E Primary                                                                |
| 761FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Mowden Infants' School                                                                 |
| 04C9BFBB-C6AE-E311-B8ED-005056822391 | 10077707    | Hillside Community Primary School                                                      |
| 37E74C15-C7AE-E311-B8ED-005056822391 | 10005152    | Portland School                                                                        |
| 0662F591-C6AE-E311-B8ED-005056822391 | NULL        | Hamsey Green Infant School                                                             |
| 0919CFAF-C6AE-E311-B8ED-005056822391 | 10075212    | Cooper and Jordan CE Primary                                                           |
| 2AC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Eastfield Lower School                                                                 |
| 390DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Briscoe Lane Primary School                                                            |
| 60BCED97-C6AE-E311-B8ED-005056822391 | 10068787    | Rose Hill Primary School                                                               |
| 9CB9ED97-C6AE-E311-B8ED-005056822391 | 10069163    | Houndfield Primary School                                                              |
| 82D491DF-C6AE-E311-B8ED-005056822391 | NULL        | St Josephs RC Primary School                                                           |
| B57B99D9-C6AE-E311-B8ED-005056822391 | NULL        | St. Andrews C of E Primary                                                             |
| 8E76B0C7-C6AE-E311-B8ED-005056822391 | NULL        | South Parade Junior                                                                    |
| C8C8BFBB-C6AE-E311-B8ED-005056822391 | 10071320    | Riversdale Primary School                                                              |
| 793D451B-C7AE-E311-B8ED-005056822391 | NULL        | King Edward Vi                                                                         |
| AABAED97-C6AE-E311-B8ED-005056822391 | NULL        | Sudbury Primary School                                                                 |
| 7FD55197-C7AE-E311-B8ED-005056822391 | 10034223    | Lady Zia Wernher School                                                                |
| 4FB40486-C6AE-E311-B8ED-005056822391 | 10077488    | Copplestone County Primary School                                                      |
| BE3B451B-C7AE-E311-B8ED-005056822391 | 10018494    | Bishop Lovett Middle School                                                            |
| 2163F591-C6AE-E311-B8ED-005056822391 | NULL        | D'Eyncourt Primary School                                                              |
| D80AFD8B-C6AE-E311-B8ED-005056822391 | 10075060    | Garden Suburb Junior School                                                            |
| E9CDA8CD-C6AE-E311-B8ED-005056822391 | 10069057    | Aughton Christ Church C.E. School                                                      |
| 6F7C99D9-C6AE-E311-B8ED-005056822391 | 10078895    | Warrington St Ann's C of E Primary School                                              |
| 8B365C09-C7AE-E311-B8ED-005056822391 | 10006950    | Torquay Grammar School for Girls                                                       |
| 443073F7-C6AE-E311-B8ED-005056822391 | 10007615    | Woodway Park School & Community College                                                |
| 0564F591-C6AE-E311-B8ED-005056822391 | NULL        | Ketley Infant School                                                                   |
| F80AFD8B-C6AE-E311-B8ED-005056822391 | NULL        | North Primary School                                                                   |
| 747D99D9-C6AE-E311-B8ED-005056822391 | 10068524    | Canon Burrows CE Primary School                                                        |
| E975B0C7-C6AE-E311-B8ED-005056822391 | 10077696    | Longton CP                                                                             |
| 17C7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Woodlands Primary                                                                      |
| 41375C09-C7AE-E311-B8ED-005056822391 | 10006918    | Tividale High                                                                          |
| C20C6579-C7AE-E311-B8ED-005056822391 | 10008372    | Malvern College                                                                        |
| A9AF3A27-C7AE-E311-B8ED-005056822391 | NULL        | Stoneleigh First School                                                                |
| 0197874F-C7AE-E311-B8ED-005056822391 | NULL        | The Deacons School                                                                     |
| 37C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Northwold Primary School                                                               |
| 65D55197-C7AE-E311-B8ED-005056822391 | 10018226    | Stretton Brook School                                                                  |
| 82F53F21-C7AE-E311-B8ED-005056822391 | 10015382    | Christ The King Catholic High School                                                   |
| 7EF63F21-C7AE-E311-B8ED-005056822391 | 10006134    | St Chadscatholic High School                                                           |
| 8B17CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Hawes Side Primary School                                                              |
| 8077B0C7-C6AE-E311-B8ED-005056822391 | 10075783    | St George Church of England Primary School                                             |
| A6A77A5B-C7AE-E311-B8ED-005056822391 | NULL        | St Peters School                                                                       |
| EC2C8AE5-C6AE-E311-B8ED-005056822391 | 10080342    | St Chads Catholic Primary                                                              |
| B8AF3A27-C7AE-E311-B8ED-005056822391 | 10072081    | Holy Trinity Church of England Primary School                                          |
| 837B99D9-C6AE-E311-B8ED-005056822391 | 10078691    | Holy Rood Catholic Primary School                                                      |
| 9969DEA3-C6AE-E311-B8ED-005056822391 | 10071451    | Scawsby Saltersgate Junior School                                                      |
| 146BDEA3-C6AE-E311-B8ED-005056822391 | 10079841    | Farnham Common Junior                                                                  |
| 96B50486-C6AE-E311-B8ED-005056822391 | 10080154    | Finstall First School                                                                  |
| F890540F-C7AE-E311-B8ED-005056822391 | 10005732    | Sedgefield Community College                                                           |
| D9D191DF-C6AE-E311-B8ED-005056822391 | 10079475    | St Augustines RC                                                                       |
| 34A77A5B-C7AE-E311-B8ED-005056822391 | 10017957    | Linton Village College                                                                 |
| B73B451B-C7AE-E311-B8ED-005056822391 | 10002885    | Hanley Castle High School                                                              |
| 885F0C80-C6AE-E311-B8ED-005056822391 | NULL        | Whitehawk Primary School                                                               |
| 8026A1D3-C6AE-E311-B8ED-005056822391 | 10079263    | The William Struck                                                                     |
| 2920B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Purford Green Infants Primary                                                          |
| ADC7BFBB-C6AE-E311-B8ED-005056822391 | 10079041    | Holtsmere End Jounior School                                                           |
| AB0C6579-C7AE-E311-B8ED-005056822391 | 10018818    | St Neots Preparatory School                                                            |
| 333073F7-C6AE-E311-B8ED-005056822391 | 10000370    | Arrow Vale High School                                                                 |
| 73C9BFBB-C6AE-E311-B8ED-005056822391 | 10078027    | Randal Cremer Primary School                                                           |
| 17D250A3-C7AE-E311-B8ED-005056822391 | 10016484    | Milestone School                                                                       |
| 7E12E69D-C6AE-E311-B8ED-005056822391 | 10073071    | Hinchley Wood School                                                                   |
| 9E63F591-C6AE-E311-B8ED-005056822391 | 10078571    | Hunton & Arrathorne Community Primary School                                           |
| B513E69D-C6AE-E311-B8ED-005056822391 | NULL        | Kingsley Primary School                                                                |
| 9C5E0C80-C6AE-E311-B8ED-005056822391 | 10076449    | St George's New Town Junior School                                                     |
| 5E3273F7-C6AE-E311-B8ED-005056822391 | 10003081    | Highlands Secondary School                                                             |
| ACD591DF-C6AE-E311-B8ED-005056822391 | NULL        | Parbold Douglas CE Primary School                                                      |
| F86EC7B5-C6AE-E311-B8ED-005056822391 | 10069710    | Greenleys First School                                                                 |
| A23F8F49-C7AE-E311-B8ED-005056822391 | 10003627    | King Edward VI Five Ways School                                                        |
| 52E06303-C7AE-E311-B8ED-005056822391 | 10001358    | Chenderit School                                                                       |
| 00CEA8CD-C6AE-E311-B8ED-005056822391 | 10075956    | Battyeford CofE (VC) Primary School                                                    |
| ED74B0C7-C6AE-E311-B8ED-005056822391 | 10071337    | Hermitage Primary School                                                               |
| 81C6BFBB-C6AE-E311-B8ED-005056822391 | 10042358    | Boldmere Infant and Nursery School                                                     |
| 042C8AE5-C6AE-E311-B8ED-005056822391 | 10071688    | St Anselm's Catholic Primary School                                                    |
| F25E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Forest Community Primary School                                                        |
| D069DEA3-C6AE-E311-B8ED-005056822391 | 10077839    | Ravenbank Community Primary School                                                     |
| 080EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Bentworth Primary School                                                               |
| 9823A1D3-C6AE-E311-B8ED-005056822391 | 10079286    | Ropsley CE Primary School                                                              |
| 8CC7BFBB-C6AE-E311-B8ED-005056822391 | 10078387    | Bankside Primary School                                                                |
| 99B03A27-C7AE-E311-B8ED-005056822391 | 10069632    | Wrangle Primary School                                                                 |
| A0CDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Stone St Mary's CE Primary                                                             |
| 8375B0C7-C6AE-E311-B8ED-005056822391 | 10069281    | Snape Wood Primary and Nursery School                                                  |
| AF2F73F7-C6AE-E311-B8ED-005056822391 | 10015771    | Fairfield High School                                                                  |
| 85D391DF-C6AE-E311-B8ED-005056822391 | 10078542    | St Marks CE Junior School                                                              |
| 227C99D9-C6AE-E311-B8ED-005056822391 | 10073950    | Jesson's CE Primary School                                                             |
| 88C8BFBB-C6AE-E311-B8ED-005056822391 | 10078381    | Swinnow Primary School                                                                 |
| 09395C09-C7AE-E311-B8ED-005056822391 | NULL        | Shavington High School                                                                 |
| 5CBAED97-C6AE-E311-B8ED-005056822391 | 10070642    | Broomgrove Junior                                                                      |
| D761F591-C6AE-E311-B8ED-005056822391 | NULL        | Woodhall CP School                                                                     |
| B017CFAF-C6AE-E311-B8ED-005056822391 | 10077452    | Chambersbury Primary School                                                            |
| 66B30486-C6AE-E311-B8ED-005056822391 | NULL        | Beechwood Community Primary                                                            |
| EA6EC7B5-C6AE-E311-B8ED-005056822391 | 10079560    | Homerswood School                                                                      |
| B2E74C15-C7AE-E311-B8ED-005056822391 | 10017032    | The Lord Silkin School                                                                 |
| C2D291DF-C6AE-E311-B8ED-005056822391 | NULL        | All Saints C.E Primary School                                                          |
| 7F6ADEA3-C6AE-E311-B8ED-005056822391 | 10078396    | Eastburn Junior and Infant School                                                      |
| A496874F-C7AE-E311-B8ED-005056822391 | NULL        | Rugby High School                                                                      |
| 1F3D451B-C7AE-E311-B8ED-005056822391 | 10006385    | Stuart Bathurst RC High School                                                         |
| 5C11E69D-C6AE-E311-B8ED-005056822391 | NULL        | Earlsmead First and Middle School                                                      |
| D1B50486-C6AE-E311-B8ED-005056822391 | 10073870    | West Green First                                                                       |
| 442B8AE5-C6AE-E311-B8ED-005056822391 | 10076043    | Our Lady's Catholic Primary School                                                     |
| C1849443-C7AE-E311-B8ED-005056822391 | 10006140    | St Columbas Catholic Boys School                                                       |
| B86EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Primrose Hill Community School                                                         |
| D513E69D-C6AE-E311-B8ED-005056822391 | 10073200    | Wollescote Primary School                                                              |
| FE63F591-C6AE-E311-B8ED-005056822391 | NULL        | James Dixon Primary School                                                             |
| CD95874F-C7AE-E311-B8ED-005056822391 | 10014796    | Abraham Darby School                                                                   |
| 6C74B0C7-C6AE-E311-B8ED-005056822391 | 10074627    | John Ball Primary School                                                               |
| CE24A1D3-C6AE-E311-B8ED-005056822391 | 10077422    | St Michael's C of E Primary School                                                     |
| C870C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Darnhall Primary School                                                                |
| 511B5791-C7AE-E311-B8ED-005056822391 | 10077026    | Hayfield School                                                                        |
| 5169DEA3-C6AE-E311-B8ED-005056822391 | 10076533    | Coundon Primary School                                                                 |
| FCCBA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St Anne's Middle School                                                                |
| 8613E69D-C6AE-E311-B8ED-005056822391 | NULL        | Ettingshall Primary School and Nursery                                                 |
| B820B8C1-C6AE-E311-B8ED-005056822391 | 10073385    | Hutton Cranswick Community Primary School                                              |
| 08D291DF-C6AE-E311-B8ED-005056822391 | 10076036    | St Catherine's Catholic School                                                         |
| 1D5C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Bradley Nursery School                                                                 |
| D82B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Baines Endowed CE (Aided) Primary School                                               |
| 6624A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Westwoodside CE Primary School                                                         |
| E569DEA3-C6AE-E311-B8ED-005056822391 | 10069998    | Stivichall Primary School                                                              |
| F33B451B-C7AE-E311-B8ED-005056822391 | 10008553    | King Edward VI School                                                                  |
| 246EC7B5-C6AE-E311-B8ED-005056822391 | 10073097    | Bure Valley School                                                                     |
| 9FC0D6A9-C6AE-E311-B8ED-005056822391 | 10074304    | Longmoor Primary School                                                                |
| A00CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Eileen Wade Lower School                                                               |
| 0BB03A27-C7AE-E311-B8ED-005056822391 | 10069595    | Albermarle Primary School                                                              |
| 0CC8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Shaldon Primary School                                                                 |
| E90AFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Tootal Drive Primary School                                                            |
| 995E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Drove Primary School                                                                   |
| DF75B0C7-C6AE-E311-B8ED-005056822391 | NULL        | East Tilbury Infant School                                                             |
| 46298AE5-C6AE-E311-B8ED-005056822391 | 10073824    | Lynton Church of England Primary School                                                |
| 0276B0C7-C6AE-E311-B8ED-005056822391 | 10071311    | Queens Park Primary                                                                    |
| DF61F591-C6AE-E311-B8ED-005056822391 | NULL        | Radford Primary                                                                        |
| A05C0C80-C6AE-E311-B8ED-005056822391 | 10045579    | Heathfield Nursery School                                                              |
| 7119CFAF-C6AE-E311-B8ED-005056822391 | 10073850    | Gainsborough Primary School                                                            |
| 0D23A1D3-C6AE-E311-B8ED-005056822391 | 10076750    | St Chad's CE VC Primary School                                                         |
| 1162F591-C6AE-E311-B8ED-005056822391 | NULL        | West Drayton School                                                                    |
| 4D65F591-C6AE-E311-B8ED-005056822391 | 10070805    | Soundley School                                                                        |
| 26996F6D-C7AE-E311-B8ED-005056822391 | 10078261    | Mayfield Preparatoty School                                                            |
| 512C8AE5-C6AE-E311-B8ED-005056822391 | 10076805    | St Michaels RC School                                                                  |
| 352B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Alban's Catholic Primary School                                                     |
| 1CBCED97-C6AE-E311-B8ED-005056822391 | NULL        | Pinkwell Primary School                                                                |
| AEB30486-C6AE-E311-B8ED-005056822391 | 10071299    | Brunswick Park Primary School                                                          |
| 536EC7B5-C6AE-E311-B8ED-005056822391 | 10077241    | Eastbury Farm J.M.I & Nursery School                                                   |
| A5DE6303-C7AE-E311-B8ED-005056822391 | 10000998    | Burnham Upper School                                                                   |
| FDC1D6A9-C6AE-E311-B8ED-005056822391 | 10079038    | Winterton Junior School                                                                |
| F3996F6D-C7AE-E311-B8ED-005056822391 | 10017719    | St. Edward's School                                                                    |
| 9974B0C7-C6AE-E311-B8ED-005056822391 | 10077173    | Sir Thomas Abney Primary School                                                        |
| 027C99D9-C6AE-E311-B8ED-005056822391 | 10074983    | St Monicas Primary School                                                              |
| 42E54C15-C7AE-E311-B8ED-005056822391 | 10001773    | Crown Woods School                                                                     |
| 7618CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Iwade CP School                                                                        |
| 7911E69D-C6AE-E311-B8ED-005056822391 | NULL        | St Ann's Well Infant School                                                            |
| FD12E69D-C6AE-E311-B8ED-005056822391 | NULL        | Thorpe Greenways School                                                                |
| 7E3173F7-C6AE-E311-B8ED-005056822391 | 10017581    | The Kingsway School                                                                    |
| 4EC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Westbourne Primary School                                                              |
| 7F849443-C7AE-E311-B8ED-005056822391 | 10006116    | St Augustines School                                                                   |
| C5E54C15-C7AE-E311-B8ED-005056822391 | 10006853    | The West Somerset                                                                      |
| 79849443-C7AE-E311-B8ED-005056822391 | 10005400    | Reading Girls School                                                                   |
| B0D87AF1-C6AE-E311-B8ED-005056822391 | NULL        | Willesden High School                                                                  |
| AA0EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | City Road JI                                                                           |
| CF526A73-C7AE-E311-B8ED-005056822391 | 10003659    | The King's School                                                                      |
| A8EE667F-C7AE-E311-B8ED-005056822391 | 10071094    | St Hilary's School                                                                     |
| 02536A73-C7AE-E311-B8ED-005056822391 | 10018638    | Derby Grammar School                                                                   |
| 58CEA8CD-C6AE-E311-B8ED-005056822391 | 10077409    | South Malling CE Primary School                                                        |
| 2DBCED97-C6AE-E311-B8ED-005056822391 | 10071448    | Kiveton Park Meadows Junior School                                                     |
| 4B17CFAF-C6AE-E311-B8ED-005056822391 | NULL        | The Meadow Community Primary School                                                    |
| C00CFD8B-C6AE-E311-B8ED-005056822391 | 10071278    | Park Lane School                                                                       |
| 39F83F21-C7AE-E311-B8ED-005056822391 | NULL        | Dormers Wells Junior School                                                            |
| D85E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Chestnuts Combined School                                                              |
| F413E69D-C6AE-E311-B8ED-005056822391 | NULL        | Edwards Hallcounty Funoir School                                                       |
| E426A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Wantage CE Primary                                                                     |
| 9A5F0C80-C6AE-E311-B8ED-005056822391 | 10069353    | Heamoor CP School                                                                      |
| B5D45197-C7AE-E311-B8ED-005056822391 | NULL        | Thomas Wolsey School                                                                   |
| CE7D99D9-C6AE-E311-B8ED-005056822391 | NULL        | Chalfont St Peter Church of England Junior School                                      |
| 306ADEA3-C6AE-E311-B8ED-005056822391 | 10070973    | Marlborough Infant School                                                              |
| 7F3C451B-C7AE-E311-B8ED-005056822391 | 10005771    | Shaftesbury School                                                                     |
| C32C8AE5-C6AE-E311-B8ED-005056822391 | 10072658    | St Marys RC Primary School                                                             |
| 1A7B99D9-C6AE-E311-B8ED-005056822391 | 10077984    | Christ Church CE School                                                                |
| 5218CFAF-C6AE-E311-B8ED-005056822391 | 10073160    | Watling Street JMI School                                                              |
| 47C9BFBB-C6AE-E311-B8ED-005056822391 | 10074666    | Avondale Park Primary                                                                  |
| 81C0D6A9-C6AE-E311-B8ED-005056822391 | 10074555    | Oldfield Primary School                                                                |
| 3768DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Parkhall Primary School                                                                |
| 5C5E0C80-C6AE-E311-B8ED-005056822391 | 10071300    | Barnfield School                                                                       |
| B4D291DF-C6AE-E311-B8ED-005056822391 | 10069794    | St Benets RC Primary School                                                            |
| 35C6BFBB-C6AE-E311-B8ED-005056822391 | 10074089    | Kingsway Infant School                                                                 |
| 956ADEA3-C6AE-E311-B8ED-005056822391 | 10073195    | Queen Victoria Primary School                                                          |
| 948082EB-C6AE-E311-B8ED-005056822391 | NULL        | St Petrocs C of E                                                                      |
| 8417CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Denfield Park Junior School                                                            |
| 50E84C15-C7AE-E311-B8ED-005056822391 | 10002985    | Heaton Manor School                                                                    |
| E9AF3A27-C7AE-E311-B8ED-005056822391 | NULL        | Lightcliffe CofE VA Primary School                                                     |
| 636EC7B5-C6AE-E311-B8ED-005056822391 | 10072910    | Hague Primary School                                                                   |
| 66E06303-C7AE-E311-B8ED-005056822391 | 10000367    | Arnold Hill School                                                                     |
| 49D55197-C7AE-E311-B8ED-005056822391 | 10008515    | St Luke's School (With Forest House Education CEnt                                     |
| 66859443-C7AE-E311-B8ED-005056822391 | 10004218    | Marling                                                                                |
| 41DF6303-C7AE-E311-B8ED-005056822391 | NULL        | Sandy Upper School                                                                     |
| 92876BFD-C6AE-E311-B8ED-005056822391 | 10000507    | Balby Carr Community Sports and Science College                                        |
| 002D8AE5-C6AE-E311-B8ED-005056822391 | 10080336    | St Bernadette's Catholic Primary                                                       |
| FE096485-C7AE-E311-B8ED-005056822391 | 10018423    | Christ Church Cathedral School                                                         |
| F9FE7261-C7AE-E311-B8ED-005056822391 | 10017644    | The Charles Dickens School                                                             |
| 27E84C15-C7AE-E311-B8ED-005056822391 | 10014862    | Asn Manor School                                                                       |
| F6C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Horton Park Primary School                                                             |
| ACE74C15-C7AE-E311-B8ED-005056822391 | NULL        | Cramlington High School                                                                |
| F22B8AE5-C6AE-E311-B8ED-005056822391 | 10076008    | St Thomas A Becket Primary School                                                      |
| FA68DEA3-C6AE-E311-B8ED-005056822391 | 10069543    | Messingham Primary School                                                              |
| 35AF3A27-C7AE-E311-B8ED-005056822391 | 10001141    | Canon Palmer Catholic High School                                                      |
| 15CDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Toddington St George Lower School                                                      |
| 9162F591-C6AE-E311-B8ED-005056822391 | 10075324    | Forest Park Primary School                                                             |
| 5BC2D6A9-C6AE-E311-B8ED-005056822391 | 10073011    | Oldbury Park Primary School                                                            |
| 3CD291DF-C6AE-E311-B8ED-005056822391 | 10079473    | St Chad's                                                                              |
| 0F859443-C7AE-E311-B8ED-005056822391 | 10004437    | St Thomas More Catholic School                                                         |
| 2C70C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Seabridge Junior School                                                                |
| 8F25A1D3-C6AE-E311-B8ED-005056822391 | 10079686    | St Thomas Infants School                                                               |
| 6371C7B5-C6AE-E311-B8ED-005056822391 | 10074636    | John Stainer Community Primary School                                                  |
| FCB60486-C6AE-E311-B8ED-005056822391 | 10071509    | Gowin Junior School                                                                    |
| DCD391DF-C6AE-E311-B8ED-005056822391 | 10074943    | St Joseph's Catholic Combined School                                                   |
| CDF73F21-C7AE-E311-B8ED-005056822391 | 10018871    | St Edward's Royal Free Ecumenical Middle School                                        |
| C471C7B5-C6AE-E311-B8ED-005056822391 | 10079552    | Panshanger Primary School                                                              |
| CD3F8F49-C7AE-E311-B8ED-005056822391 | 10000504    | Baines School                                                                          |
| 7823A1D3-C6AE-E311-B8ED-005056822391 | 10074267    | Mundy C of E School                                                                    |
| BA2D8AE5-C6AE-E311-B8ED-005056822391 | 10068614    | Adel St John the Baptist Church of England Primary School                              |
| E7615C8B-C7AE-E311-B8ED-005056822391 | NULL        | Holly Park Montessori School                                                           |
| 01625C8B-C7AE-E311-B8ED-005056822391 | NULL        | St Nicholas's Montessori School                                                        |
| A20EFD8B-C6AE-E311-B8ED-005056822391 | 10068768    | Ashbrow School                                                                         |
| 78096485-C7AE-E311-B8ED-005056822391 | 10018786    | Yateley Manor School                                                                   |
| 2D23A1D3-C6AE-E311-B8ED-005056822391 | 10074584    | Corton CE VC Primary                                                                   |
| 7F0DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Palmers Cross Primary School                                                           |
| 5D615C8B-C7AE-E311-B8ED-005056822391 | 10007642    | Worth School                                                                           |
| 077B99D9-C6AE-E311-B8ED-005056822391 | 10077987    | Ripley Endowed Church of England School                                                |
| 3F19CFAF-C6AE-E311-B8ED-005056822391 | 10077281    | Stithians Community Primary School                                                     |
| F7B50486-C6AE-E311-B8ED-005056822391 | NULL        | Oakenrod Primary School                                                                |
| ED3E8F49-C7AE-E311-B8ED-005056822391 | NULL        | Westminster G M School                                                                 |
| 3E3173F7-C6AE-E311-B8ED-005056822391 | NULL        | Ormskirk School                                                                        |
| 46C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Thurlton First School                                                                  |
| FBBFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | North County CP School                                                                 |
| 1F7D99D9-C6AE-E311-B8ED-005056822391 | 10075978    | St Mary Magdalen's Catholic Primary School                                             |
| 9BC7BFBB-C6AE-E311-B8ED-005056822391 | 10077709    | Torrisholme County Primary                                                             |
| DA8282EB-C6AE-E311-B8ED-005056822391 | NULL        | Blacon High School                                                                     |
| 26E74C15-C7AE-E311-B8ED-005056822391 | 10018079    | Ladymead Community School                                                              |
| 86BAED97-C6AE-E311-B8ED-005056822391 | 10043346    | Causton Junior School                                                                  |
| F1B40486-C6AE-E311-B8ED-005056822391 | 10069534    | Nine Acres Primary School                                                              |
| A010E69D-C6AE-E311-B8ED-005056822391 | 10069585    | Slimbridge Primary                                                                     |
| 680BFD8B-C6AE-E311-B8ED-005056822391 | 10075881    | Manland Primary School                                                                 |
| 25E64C15-C7AE-E311-B8ED-005056822391 | 10007982    | Oaklands Community School                                                              |
| 995C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Thomas Wall Nursery School                                                             |
| FD5D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Abinger Common First School                                                            |
| B4B50486-C6AE-E311-B8ED-005056822391 | 10048632    | Birdsedge First School                                                                 |
| D0CEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Hartest CE VC Primary School                                                           |
| C41DB8C1-C6AE-E311-B8ED-005056822391 | 10076224    | Swingate Infant School                                                                 |
| FD3E8F49-C7AE-E311-B8ED-005056822391 | 10002031    | Drayton Manor High School                                                              |
| 6A3D451B-C7AE-E311-B8ED-005056822391 | 10016477    | St Mary's RC High School                                                               |
| 52D591DF-C6AE-E311-B8ED-005056822391 | 10076819    | St. Alban and St Stephen RC Junior School                                              |
| 3819CFAF-C6AE-E311-B8ED-005056822391 | 10068802    | Rack House County Primary School                                                       |
| BD2C8AE5-C6AE-E311-B8ED-005056822391 | NULL        | The Priory Catholic Primary School                                                     |
| A51A7067-C7AE-E311-B8ED-005056822391 | 10004572    | New Brompton College                                                                   |
| 92F63F21-C7AE-E311-B8ED-005056822391 | 10000838    | Bradford Cathedral Community College                                                   |
| D23273F7-C6AE-E311-B8ED-005056822391 | 10007076    | Tunbridge Wells Grammar School for Boys                                                |
| ABC6BFBB-C6AE-E311-B8ED-005056822391 | 10067536    | Woolenwick Infant School                                                               |
| E2615C8B-C7AE-E311-B8ED-005056822391 | 10072285    | Hornsby House School                                                                   |
| B6355C09-C7AE-E311-B8ED-005056822391 | 10015903    | Warley High School                                                                     |
| 7C8282EB-C6AE-E311-B8ED-005056822391 | 10006793    | The Ravenscroft School                                                                 |
| 962A8AE5-C6AE-E311-B8ED-005056822391 | 10073754    | Rosh Pinah Primary School                                                              |
| 9A096485-C7AE-E311-B8ED-005056822391 | 10071125    | Dormer House PNEU School                                                               |
| 1C7C99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Helen's Catholic Infant School                                                      |
| 55C9BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Orchard Meadowcroft First School                                                       |
| 6076B0C7-C6AE-E311-B8ED-005056822391 | 10078187    | James Wolfe Primary School                                                             |
| 1419CFAF-C6AE-E311-B8ED-005056822391 | 10075560    | Castlewood Primary                                                                     |
| 523073F7-C6AE-E311-B8ED-005056822391 | NULL        | Stewartby Middle School                                                                |
| 526FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Wheatfields Junior School                                                              |
| 8B0EFD8B-C6AE-E311-B8ED-005056822391 | 10072887    | Barham Primary School                                                                  |
| 630DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Martin Junior School                                                                   |
| 971EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | South Tawton Primary School                                                            |
| F2096485-C7AE-E311-B8ED-005056822391 | NULL        | Northampton High School                                                                |
| FA6FC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Treyew Primary School                                                                  |
| 486BDEA3-C6AE-E311-B8ED-005056822391 | NULL        | Meadow Community Primary School                                                        |
| A78082EB-C6AE-E311-B8ED-005056822391 | 10080295    | St Aloysius' RC Primary School                                                         |
| 28BCED97-C6AE-E311-B8ED-005056822391 | 10078930    | New Pastures Primary School                                                            |
| 0F5E0C80-C6AE-E311-B8ED-005056822391 | 10081252    | Avenue Primary School                                                                  |
| 46D97AF1-C6AE-E311-B8ED-005056822391 | 10000527    | Barking Abbey School                                                                   |
| 495F0C80-C6AE-E311-B8ED-005056822391 | NULL        | Broadfields Junior School                                                              |
| 9ED45197-C7AE-E311-B8ED-005056822391 | 10016480    | Merefield School                                                                       |
| B5BCED97-C6AE-E311-B8ED-005056822391 | NULL        | Campion School                                                                         |
| 00D591DF-C6AE-E311-B8ED-005056822391 | 10070208    | St Augustine's Catholic Primary School                                                 |
| B4AF3CC4-8A17-E611-8528-00505682090B | NULL        | Dudley College of Education                                                            |
| 6E1A7067-C7AE-E311-B8ED-005056822391 | 10003033    | Herne Bay High School                                                                  |
| 2FF83F21-C7AE-E311-B8ED-005056822391 | 10076306    | St Johns C of E Primary School                                                         |
| 6D298AE5-C6AE-E311-B8ED-005056822391 | 10079300    | St Johns Upper Holloway                                                                |
| 1E20B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Withernsea Junior School                                                               |
| 471B5791-C7AE-E311-B8ED-005056822391 | 10015311    | Woodfield School                                                                       |
| 661FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Stuart Road Primary School                                                             |
| 5E5D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Ronald Openshaw Education Centre                                                       |
| 160BFD8B-C6AE-E311-B8ED-005056822391 | 10041484    | Oldfield Primary School                                                                |
| 2E0D6579-C7AE-E311-B8ED-005056822391 | NULL        | Marist Convent Prep School                                                             |
| 9B6ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Oxhey Wood School                                                                      |
| F58282EB-C6AE-E311-B8ED-005056822391 | 10003793    | Lawnswood School                                                                       |
| B8D291DF-C6AE-E311-B8ED-005056822391 | NULL        | Finedon Mulso C of E Junior School                                                     |
| 1B625C8B-C7AE-E311-B8ED-005056822391 | NULL        | Holborn College                                                                        |
| 38C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Norton Canes Primary School                                                            |
| E32F73F7-C6AE-E311-B8ED-005056822391 | NULL        | Trinity High School (Bridley Moor Campus)                                              |
| AA7C99D9-C6AE-E311-B8ED-005056822391 | NULL        | Frodsham C of E Primary                                                                |
| 180D6579-C7AE-E311-B8ED-005056822391 | 10008120    | Brighton and Hove High School                                                          |
| C17A589D-C7AE-E311-B8ED-005056822391 | 10016272    | Kingswode Hoe School                                                                   |
| 173073F7-C6AE-E311-B8ED-005056822391 | 10033598    | Robert Bloomfield Academy                                                              |
| 377E99D9-C6AE-E311-B8ED-005056822391 | 10074697    | The Queens CE Primary School                                                           |
| 72BFD6A9-C6AE-E311-B8ED-005056822391 | 10073194    | Belle Vue Primary School                                                               |
| 810CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Hallsville Primary School                                                              |
| 5675B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Thorngumbald Infant School                                                             |
| 3CB9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Yarm Primary School                                                                    |
| 96298AE5-C6AE-E311-B8ED-005056822391 | 10076012    | St Joseph's RC Primary School                                                          |
| 55C7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Rokeby Junior School (RB)                                                              |
| B9526A73-C7AE-E311-B8ED-005056822391 | 10071113    | Brooke Priory School                                                                   |
| 8469DEA3-C6AE-E311-B8ED-005056822391 | 10072987    | Water Primary School                                                                   |
| B0CEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | St John's CE (C) First School                                                          |
| 1224A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Warton Nethersoles CE Primary.                                                         |
| 0ACEA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Thorner Church of England Voluntary Controlled Primary School                          |
| E1CDA8CD-C6AE-E311-B8ED-005056822391 | 10079665    | St Andrew's CE Infant School                                                           |
| 3C69DEA3-C6AE-E311-B8ED-005056822391 | 10072564    | Upperthong Junior and Infant School                                                    |
| 6AD291DF-C6AE-E311-B8ED-005056822391 | 10081448    | Saint Matthew's CE VA Primary School                                                   |
| 55D45197-C7AE-E311-B8ED-005056822391 | 10015894    | Warren Community Special School                                                        |
| A3F73F21-C7AE-E311-B8ED-005056822391 | 10017849    | St Marys High School Croyden                                                           |
| 151FB8C1-C6AE-E311-B8ED-005056822391 | 10073102    | Swalecliffe CP School                                                                  |
| 03385C09-C7AE-E311-B8ED-005056822391 | 10005125    | Plumstead Manor School                                                                 |
| D5CBA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Cranfield V C Lower School                                                             |
| 1F19CFAF-C6AE-E311-B8ED-005056822391 | NULL        | The Oaks First & Middle School                                                         |
| C1E64C15-C7AE-E311-B8ED-005056822391 | NULL        | Admiral Lord Nelson School                                                             |
| 9C0BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Slade Green Junior School                                                              |
| 716BDEA3-C6AE-E311-B8ED-005056822391 | 10069328    | Rowley Hall Primary School                                                             |
| 0D17CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Broadfield East Middle School                                                          |
| ED896BFD-C6AE-E311-B8ED-005056822391 | 10003169    | Hove Park School                                                                       |
| 16B50486-C6AE-E311-B8ED-005056822391 | NULL        | Holmfield Primary School                                                               |
| 5FD55197-C7AE-E311-B8ED-005056822391 | NULL        | The Wrekin Special School                                                              |
| 94C6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Holly Lodge Primary School                                                             |
| 532B8AE5-C6AE-E311-B8ED-005056822391 | 10079311    | St Alfege With St. Peter's C.E Primary School                                          |
| 3677B0C7-C6AE-E311-B8ED-005056822391 | 10073785    | Burwash C of E School                                                                  |
| 77D391DF-C6AE-E311-B8ED-005056822391 | 10074955    | Christ The King Catholic Primary                                                       |
| 5B3F451B-C7AE-E311-B8ED-005056822391 | 10002309    | Ermysted's Grammer School                                                              |
| 5B96874F-C7AE-E311-B8ED-005056822391 | 10017776    | Testwood School                                                                        |
| 4B13E69D-C6AE-E311-B8ED-005056822391 | NULL        | Eaton Park Primary School                                                              |
| 403E451B-C7AE-E311-B8ED-005056822391 | 10014972    | Archbishop Sancroft High School                                                        |
| E312E69D-C6AE-E311-B8ED-005056822391 | 10080380    | Thames Ditton Infant School                                                            |
| 44DB7AF1-C6AE-E311-B8ED-005056822391 | 10002847    | Hailsham Community College                                                             |
| 21D491DF-C6AE-E311-B8ED-005056822391 | 10070214    | St Thomas Moore RC Primary School                                                      |
| 1F1DB8C1-C6AE-E311-B8ED-005056822391 | 10077332    | Parklands Primary School                                                               |
| D0F63F21-C7AE-E311-B8ED-005056822391 | NULL        | Queen Elizabeth School                                                                 |
| 2FCC372D-C7AE-E311-B8ED-005056822391 | 10004852    | Old Swinford Hospital                                                                  |
| 652C8AE5-C6AE-E311-B8ED-005056822391 | 10072684    | St John's Catholic Primary School                                                      |
| C1E74C15-C7AE-E311-B8ED-005056822391 | 10002731    | Gosforth Academy                                                                       |
| 0569DEA3-C6AE-E311-B8ED-005056822391 | 10079057    | Field Junior School                                                                    |
| DA0DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Bovington First School                                                                 |
| 7CB03A27-C7AE-E311-B8ED-005056822391 | NULL        | Woodville Primary School                                                               |
| E419CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Bathampton Primary School                                                              |
| E1866BFD-C6AE-E311-B8ED-005056822391 | 10006661    | The Emmbrook School                                                                    |
| AF7D99D9-C6AE-E311-B8ED-005056822391 | NULL        | Sir Henry Fermor CE Primary                                                            |
| 147B99D9-C6AE-E311-B8ED-005056822391 | 10077985    | Fountains C of E School                                                                |
| 6524A1D3-C6AE-E311-B8ED-005056822391 | 10073766    | Ticehurst C of E School                                                                |
| A97A589D-C7AE-E311-B8ED-005056822391 | 10016070    | Willoughby School                                                                      |
| EF1DB8C1-C6AE-E311-B8ED-005056822391 | 10069913    | Abbey Village Primary School                                                           |
| 5224A1D3-C6AE-E311-B8ED-005056822391 | 10071578    | Froxfiald Infant School                                                                |
| 58C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Hatch Ride Primary School                                                              |
| CBF53F21-C7AE-E311-B8ED-005056822391 | 10015262    | St Aloysius' College                                                                   |
| 321B5791-C7AE-E311-B8ED-005056822391 | 10018124    | Manchester Academy                                                                     |
| D2BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Whitelands Park Primary School                                                         |
| AB13E69D-C6AE-E311-B8ED-005056822391 | 10074104    | Letchmore Infants' and Nursery School                                                  |
| 5BE54C15-C7AE-E311-B8ED-005056822391 | 10015161    | Bradfield School                                                                       |
| 9226A1D3-C6AE-E311-B8ED-005056822391 | 10078127    | Banks Methodist                                                                        |
| 4A996F6D-C7AE-E311-B8ED-005056822391 | 10005137    | Polam Hall School                                                                      |
| 735E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Pemberton Primary and Nursery School                                                   |
| B7B40486-C6AE-E311-B8ED-005056822391 | NULL        | Penzance Junior School                                                                 |
| A3D97AF1-C6AE-E311-B8ED-005056822391 | 10016621    | Newsome High School                                                                    |
| E0F73F21-C7AE-E311-B8ED-005056822391 | 10005503    | Robert Sutton Catholic School                                                          |
| 7AC6BFBB-C6AE-E311-B8ED-005056822391 | 10074592    | Marner Primary School                                                                  |
| 4296874F-C7AE-E311-B8ED-005056822391 | 10006254    | St Philomenas School                                                                   |
| D6D97AF1-C6AE-E311-B8ED-005056822391 | NULL        | Thamesmead Community College                                                           |
| 071FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Wilberforce Primary School                                                             |
| 5077B0C7-C6AE-E311-B8ED-005056822391 | 10071374    | Headfield Church of England Voluntary Controlled Junior School                         |
| 3C375C09-C7AE-E311-B8ED-005056822391 | 10004962    | Paignton Community College                                                             |
| 63D45197-C7AE-E311-B8ED-005056822391 | 10016883    | Marjorie Mcclure School                                                                |
| 597C99D9-C6AE-E311-B8ED-005056822391 | NULL        | Ringsfield Primary School                                                              |
| 4A24A1D3-C6AE-E311-B8ED-005056822391 | NULL        | St Andrews C of E Primary School                                                       |
| 3518CFAF-C6AE-E311-B8ED-005056822391 | 10068803    | Plymouth Grove Primary                                                                 |
| 0E6BDEA3-C6AE-E311-B8ED-005056822391 | NULL        | Wellgate Primary School                                                                |
| 5296874F-C7AE-E311-B8ED-005056822391 | 10007968    | Murray Park School                                                                     |
| 87BFD6A9-C6AE-E311-B8ED-005056822391 | NULL        | Watling Lower School                                                                   |
| 97C0D6A9-C6AE-E311-B8ED-005056822391 | 10070092    | Mauldeth Road Primary School                                                           |
| 9A71C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Silver End County Primary School                                                       |
| 97615C8B-C7AE-E311-B8ED-005056822391 | 10015191    | Brambletye School                                                                      |
| 5C6ADEA3-C6AE-E311-B8ED-005056822391 | 10078934    | Ladywood Primary School                                                                |
| 00EE667F-C7AE-E311-B8ED-005056822391 | 10014931    | Windlesham House School                                                                |
| 0BC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Harlington Lower                                                                       |
| 6AC8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Great Barr Primary                                                                     |
| 80D150A3-C7AE-E311-B8ED-005056822391 | 10016557    | Inglesea School                                                                        |
| 30F73F21-C7AE-E311-B8ED-005056822391 | 10015996    | Hull Trinity House                                                                     |
| 9CC5BFBB-C6AE-E311-B8ED-005056822391 | 10072963    | Fred Nicholson School                                                                  |
| 1B395C09-C7AE-E311-B8ED-005056822391 | 10002999    | Helston School                                                                         |
| 3CC2D6A9-C6AE-E311-B8ED-005056822391 | 10080396    | Chestnut Lane School                                                                   |
| 5CC1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Talbor Combined                                                                        |
| A92C8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Riverside Primary School                                                               |
| 1018CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Bratton Fleming Community Primary                                                      |
| 88876BFD-C6AE-E311-B8ED-005056822391 | NULL        | St George's High School                                                                |
| 917B99D9-C6AE-E311-B8ED-005056822391 | NULL        | Easton CE Primary School                                                               |
| 8770C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Pudsey Tyersal Primary School                                                          |
| 0E2A8AE5-C6AE-E311-B8ED-005056822391 | 10079432    | Our Lady of Compassion Catholic Primary School                                         |
| 4771C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Stanley Grove Primary School                                                           |
| 82D250A3-C7AE-E311-B8ED-005056822391 | 10000878    | Bridgewater College Early Excellence Centre                                            |
| 16375C09-C7AE-E311-B8ED-005056822391 | 10017266    | Pensnett School of Technology                                                          |
| 0063F591-C6AE-E311-B8ED-005056822391 | 10074108    | Strathmore Infant & Nursery School                                                     |
| 7424A1D3-C6AE-E311-B8ED-005056822391 | 10069051    | Middleforth CE Primary School                                                          |
| E0526A73-C7AE-E311-B8ED-005056822391 | 10008253    | Greshams Preparatory School                                                            |
| 8D2A8AE5-C6AE-E311-B8ED-005056822391 | 10080461    | St Antony's RC Junior School                                                           |
| F525A1D3-C6AE-E311-B8ED-005056822391 | NULL        | St John's C of E (C) Primary School                                                    |
| 8620B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Southway Primary School                                                                |
| F02A8AE5-C6AE-E311-B8ED-005056822391 | 10074007    | St Mary's C of E Primary School                                                        |
| FE16CFAF-C6AE-E311-B8ED-005056822391 | 10073890    | West Hill Primary School                                                               |
| C069DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Jesmond Road Primary School                                                            |
| DFD87AF1-C6AE-E311-B8ED-005056822391 | 10006775    | The Park High School                                                                   |
| ECB60486-C6AE-E311-B8ED-005056822391 | NULL        | Valley Primary School                                                                  |
| BCDB7AF1-C6AE-E311-B8ED-005056822391 | 10013316    | Gleed Boys' School                                                                     |
| B9886BFD-C6AE-E311-B8ED-005056822391 | NULL        | St Bonaventure's School                                                                |
| FA95874F-C7AE-E311-B8ED-005056822391 | 10015371    | Crofton School                                                                         |
| F7385C09-C7AE-E311-B8ED-005056822391 | 10000787    | Bodmin Community School                                                                |
| 8A2F73F7-C6AE-E311-B8ED-005056822391 | 10015241    | Broad Oak High School                                                                  |
| 805D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Dorking Nursery                                                                        |
| 5762F591-C6AE-E311-B8ED-005056822391 | 10076585    | Winns Primary School                                                                   |
| 35BBED97-C6AE-E311-B8ED-005056822391 | 10034308    | Churchfield                                                                            |
| 40D55197-C7AE-E311-B8ED-005056822391 | 10077074    | Colnbrook School                                                                       |
| 19CCA8CD-C6AE-E311-B8ED-005056822391 | 10073955    | Christ Church C of E Primary School                                                    |
| 6E5F0C80-C6AE-E311-B8ED-005056822391 | NULL        | Brimsdown Infant School                                                                |
| C80C6579-C7AE-E311-B8ED-005056822391 | 10004907    | Alton Reynold School                                                                   |
| 98AF3A27-C7AE-E311-B8ED-005056822391 | NULL        | Kettleshulme St James Primary School                                                   |
| 3B7E99D9-C6AE-E311-B8ED-005056822391 | 10079477    | Holy Souls RC Primary                                                                  |
| 143C451B-C7AE-E311-B8ED-005056822391 | 10018842    | St Felix CE VC Middle School                                                           |
| 7EE54C15-C7AE-E311-B8ED-005056822391 | 10014857    | Birley Community College                                                               |
| A4C8BFBB-C6AE-E311-B8ED-005056822391 | 10078059    | Richard Cobden Primary School                                                          |
| B1849443-C7AE-E311-B8ED-005056822391 | 10003733    | La Sainte Union Catholic Secondary School                                              |
| 1EE16303-C7AE-E311-B8ED-005056822391 | 10004433    | The Morley Academy                                                                     |
| 6062F591-C6AE-E311-B8ED-005056822391 | 10080047    | Little Stanmore First and Middle School                                                |
| 4A77B0C7-C6AE-E311-B8ED-005056822391 | 10068610    | Holy Trinity CE Primary School*                                                        |
| B4365C09-C7AE-E311-B8ED-005056822391 | 10005332    | Queen Elizabeth Mercian High School                                                    |
| 7D7B99D9-C6AE-E311-B8ED-005056822391 | 10078727    | St Hugh's RC Primary School                                                            |
| 449A6F6D-C7AE-E311-B8ED-005056822391 | 10008607    | Wrekin College                                                                         |
| 8FD250A3-C7AE-E311-B8ED-005056822391 | 10001474    | City of Stoke On Trent Sixth Form College                                              |
| 597B99D9-C6AE-E311-B8ED-005056822391 | 10068635    | St Augustine's CofE VA Junior and Infant School                                        |
| E11FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Firthmoor Primary School                                                               |
| 8719CFAF-C6AE-E311-B8ED-005056822391 | 10074045    | Inglehurst Junior School                                                               |
| 89D250A3-C7AE-E311-B8ED-005056822391 | 10005032    | Pendleton College                                                                      |
| 1919CFAF-C6AE-E311-B8ED-005056822391 | 10068977    | The Meads Primary School                                                               |
| 4E408F49-C7AE-E311-B8ED-005056822391 | 10006237    | St Michael's Catholic Grammar School                                                   |
| CAE64C15-C7AE-E311-B8ED-005056822391 | 10006344    | Stockwell Park School                                                                  |
| BBC0D6A9-C6AE-E311-B8ED-005056822391 | 10071198    | Dormansland Primary School                                                             |
| 486FC7B5-C6AE-E311-B8ED-005056822391 | 10074638    | Brindishe Green School                                                                 |
| F61A7067-C7AE-E311-B8ED-005056822391 | 10075544    | Al Fauqan Primary School                                                               |
| 67ED667F-C7AE-E311-B8ED-005056822391 | 10080496    | Warwick Preparatory School                                                             |
| 8ED291DF-C6AE-E311-B8ED-005056822391 | NULL        | Sacred Heart Primary School                                                            |
| 695D0C80-C6AE-E311-B8ED-005056822391 | NULL        | St Matthews CE Infant School                                                           |
| 35D491DF-C6AE-E311-B8ED-005056822391 | 10076290    | St Mary's CE Junior School                                                             |
| C619CFAF-C6AE-E311-B8ED-005056822391 | 10079106    | Westfields Junior School                                                               |
| 852A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Richards RC Primary                                                                 |
| FE0C6579-C7AE-E311-B8ED-005056822391 | 10014069    | Wisbech Grammar                                                                        |
| 6A64F591-C6AE-E311-B8ED-005056822391 | 10068907    | Fox Hill Primary School                                                                |
| 586ADEA3-C6AE-E311-B8ED-005056822391 | 10075619    | Kirkheaton Primary School                                                              |
| 9D24A1D3-C6AE-E311-B8ED-005056822391 | NULL        | St Leonards C.E Primary School                                                         |
| 9D7A99D9-C6AE-E311-B8ED-005056822391 | 10080571    | Beckley Primary School                                                                 |
| 4D75B0C7-C6AE-E311-B8ED-005056822391 | NULL        | The Alton School                                                                       |
| 5DD45197-C7AE-E311-B8ED-005056822391 | 10077043    | Vernon House School                                                                    |
| 4869DEA3-C6AE-E311-B8ED-005056822391 | NULL        | College Town Junior School                                                             |
| 7BD150A3-C7AE-E311-B8ED-005056822391 | 10076689    | Westfield Sch                                                                          |
| 1A71C7B5-C6AE-E311-B8ED-005056822391 | 10078771    | Springfields First School                                                              |
| FDD150A3-C7AE-E311-B8ED-005056822391 | NULL        | St Benedicts Pre School                                                                |
| 4017CFAF-C6AE-E311-B8ED-005056822391 | 10069715    | The John Hampden School                                                                |
| F323A1D3-C6AE-E311-B8ED-005056822391 | 10079284    | St Michael's CE Primary School                                                         |
| 6113E69D-C6AE-E311-B8ED-005056822391 | 10075659    | Brackenhill Primary School                                                             |
| 08625C8B-C7AE-E311-B8ED-005056822391 | NULL        | Royal School Hampstead                                                                 |
| 292D8AE5-C6AE-E311-B8ED-005056822391 | NULL        | The Edgware School                                                                     |
| A671C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Richmond Primary School                                                                |
| C3B60486-C6AE-E311-B8ED-005056822391 | 10071298    | Edgeware Infant School                                                                 |
| 22615C8B-C7AE-E311-B8ED-005056822391 | 10017691    | Stanborough Secondary School                                                           |
| 29896BFD-C6AE-E311-B8ED-005056822391 | 10005537    | Roundhay School                                                                        |
| F763F591-C6AE-E311-B8ED-005056822391 | 10073910    | Lawley Primary School                                                                  |
| B30C6579-C7AE-E311-B8ED-005056822391 | NULL        | Wilmslow Preparatory School                                                            |
| 02408F49-C7AE-E311-B8ED-005056822391 | 10006605    | The Liverpool Blue Coat School                                                         |
| AD20B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Hurworth Primary School                                                                |
| C3BBED97-C6AE-E311-B8ED-005056822391 | 10072580    | Woodhouse Primary School                                                               |
| B9C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Lanlivery CP School                                                                    |
| 390CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Bedonwell Junior School                                                                |
| 4DE06303-C7AE-E311-B8ED-005056822391 | NULL        | Diss High School                                                                       |
| EA096485-C7AE-E311-B8ED-005056822391 | 10077594    | Snaresbrook College                                                                    |
| BC7B99D9-C6AE-E311-B8ED-005056822391 | 10070496    | Great and Little Shelford CE School                                                    |
| 5C0BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Redbridge Junior School                                                                |
| 62B40486-C6AE-E311-B8ED-005056822391 | NULL        | Pelham Primary School                                                                  |
| 75D250A3-C7AE-E311-B8ED-005056822391 | NULL        | Elsley School                                                                          |
| 992A8AE5-C6AE-E311-B8ED-005056822391 | 10068543    | St Mark's C of E Primary School                                                        |
| 782D8AE5-C6AE-E311-B8ED-005056822391 | 10016217    | King James's School                                                                    |
| 7FC1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Lea Farm Junior School                                                                 |
| 26C9BFBB-C6AE-E311-B8ED-005056822391 | 10075314    | Rushmore Primary School                                                                |
| 971DB8C1-C6AE-E311-B8ED-005056822391 | 10075094    | Brambletye Junior School                                                               |
| 9F69DEA3-C6AE-E311-B8ED-005056822391 | NULL        | St Helen's Primary School                                                              |
| 3BE06303-C7AE-E311-B8ED-005056822391 | 10015918    | High Ridge School Specialist Sports College                                            |
| 5864F591-C6AE-E311-B8ED-005056822391 | 10075047    | Pinner Park Middle School                                                              |
| D61DB8C1-C6AE-E311-B8ED-005056822391 | 10069915    | Lever House Primary School                                                             |
| 1571C7B5-C6AE-E311-B8ED-005056822391 | 10075354    | Bishopswood Infant School                                                              |
| E93E451B-C7AE-E311-B8ED-005056822391 | 10006152    | Cardinal Hume Catholic School                                                          |
| 74B03A27-C7AE-E311-B8ED-005056822391 | NULL        | Lyndhurst Junior School                                                                |
| CFC8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Padstow Primary School                                                                 |
| 1268DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Reynolds Jnior                                                                         |
| 40CDA8CD-C6AE-E311-B8ED-005056822391 | 10078137    | Wray with Botton Endowed Primary                                                       |
| A41DB8C1-C6AE-E311-B8ED-005056822391 | 10070868    | Meadow Farm Community Primary School                                                   |
| 0B13E69D-C6AE-E311-B8ED-005056822391 | 10068826    | Warstones Primary School                                                               |
| 096BDEA3-C6AE-E311-B8ED-005056822391 | 10074618    | Crawford Primary School                                                                |
| 116ADEA3-C6AE-E311-B8ED-005056822391 | 10074083    | Frodingham Infant School                                                               |
| 4171C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Badocks Wood Primary School & Children's Centre                                        |
| 031DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Cottingley Primary School                                                              |
| 5568DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Pebsham Community Primary School                                                       |
| 1A0A6485-C7AE-E311-B8ED-005056822391 | 10017707    | St Michael's School                                                                    |
| 55536A73-C7AE-E311-B8ED-005056822391 | 10008149    | Casterton School                                                                       |
| 7A20B8C1-C6AE-E311-B8ED-005056822391 | 10079513    | Eastrington Primary School                                                             |
| E0C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Gorse Hill Community Primary School                                                    |
| BC0DFD8B-C6AE-E311-B8ED-005056822391 | 10071951    | Marks Gate Infant School                                                               |
| 4319CFAF-C6AE-E311-B8ED-005056822391 | 10074198    | St Matthews Catholic Primary School                                                    |
| EA986F6D-C7AE-E311-B8ED-005056822391 | 10000212    | The Alice Ottley School                                                                |
| ADC5BFBB-C6AE-E311-B8ED-005056822391 | 10072982    | Moorside Primary School                                                                |
| 3D8382EB-C6AE-E311-B8ED-005056822391 | 10018541    | Beyton Middle School                                                                   |
| 4D7B99D9-C6AE-E311-B8ED-005056822391 | 10074708    | St John's Primary                                                                      |
| DE26A1D3-C6AE-E311-B8ED-005056822391 | 10072432    | Elsemham Church of England Primary School                                              |
| 5FFE7261-C7AE-E311-B8ED-005056822391 | NULL        | Rainsford High School                                                                  |
| 19D491DF-C6AE-E311-B8ED-005056822391 | 10077369    | St John's Voluntary Aided Church of England Primar                                     |
| A0B40486-C6AE-E311-B8ED-005056822391 | 10070137    | Netherton Moss Primary School                                                          |
| 48D250A3-C7AE-E311-B8ED-005056822391 | NULL        | Dorin Park School                                                                      |
| 01C6BFBB-C6AE-E311-B8ED-005056822391 | NULL        | The Dales Community Junior School                                                      |
| F0DE6303-C7AE-E311-B8ED-005056822391 | 10005468    | Richmond School                                                                        |
| 0A68DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Kings Heath Infant & Nursery School                                                    |
| 1B2C8AE5-C6AE-E311-B8ED-005056822391 | 10076014    | Our Lady's RC Primary School                                                           |
| 78C0D6A9-C6AE-E311-B8ED-005056822391 | 10078559    | Leeming (RAF) Community Primary School                                                 |
| 6EB30486-C6AE-E311-B8ED-005056822391 | 10073134    | Banks Road Primary School                                                              |
| 2BC1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Turlin Moor Community Middle School                                                    |
| DCDE6303-C7AE-E311-B8ED-005056822391 | NULL        | Newham Secondary School                                                                |
| 4BEE667F-C7AE-E311-B8ED-005056822391 | 10001834    | Dame Allan's Senior School                                                             |
| D35D0C80-C6AE-E311-B8ED-005056822391 | 10078951    | Beech Hill Community Primary School                                                    |
| 90CCA8CD-C6AE-E311-B8ED-005056822391 | 10073658    | Harry Gosling Primary School                                                           |
| 1F77B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Wyndham Primary School                                                                 |
| FE70C7B5-C6AE-E311-B8ED-005056822391 | 10076974    | South Wonston Primary School                                                           |
| F164F591-C6AE-E311-B8ED-005056822391 | 10072582    | Midgley School                                                                         |
| 24D97AF1-C6AE-E311-B8ED-005056822391 | 10015365    | Fartown High School                                                                    |
| ED13E69D-C6AE-E311-B8ED-005056822391 | NULL        | Greenways Primary School                                                               |
| 218382EB-C6AE-E311-B8ED-005056822391 | 10015632    | Drayton School                                                                         |
| EED291DF-C6AE-E311-B8ED-005056822391 | NULL        | Longdon St.Mary's CE                                                                   |
| 970D6579-C7AE-E311-B8ED-005056822391 | 10017219    | St Lawrence College                                                                    |
| EE7A99D9-C6AE-E311-B8ED-005056822391 | 10073947    | All Saints C of E Primary School                                                       |
| B68182EB-C6AE-E311-B8ED-005056822391 | 10015710    | Grange School                                                                          |
| 6471C7B5-C6AE-E311-B8ED-005056822391 | 10075346    | Liphook Infant School                                                                  |
| EC61F591-C6AE-E311-B8ED-005056822391 | NULL        | Hursthead Junior School                                                                |
| 97C6BFBB-C6AE-E311-B8ED-005056822391 | 10076089    | Bovey Tracey Primary School                                                            |
| 07C6BFBB-C6AE-E311-B8ED-005056822391 | 10076193    | Edisford County Primary School                                                         |
| 53D291DF-C6AE-E311-B8ED-005056822391 | 10068662    | St Catharine's CofE Primary School                                                     |
| 0CA87A5B-C7AE-E311-B8ED-005056822391 | 10001398    | Chipping Campden School*                                                               |
| 67C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Orwell Junior School                                                                   |
| C4298AE5-C6AE-E311-B8ED-005056822391 | 10079508    | Ss Peter and Paul RC Primary School                                                    |
| 6B365C09-C7AE-E311-B8ED-005056822391 | 10004561    | Netherhall School                                                                      |
| 830AFD8B-C6AE-E311-B8ED-005056822391 | 10078940    | Shawlands Primary School                                                               |
| 51375C09-C7AE-E311-B8ED-005056822391 | 10016268    | King Ethelbert School                                                                  |
| 990DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Wells Hall Community Primary School                                                    |
| 2CC7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Nelson Castercliff Community Primary School                                            |
| 0F77B0C7-C6AE-E311-B8ED-005056822391 | NULL        | The Redlands C of E Primary School                                                     |
| 5D375C09-C7AE-E311-B8ED-005056822391 | 10005475    | The Ridings High School                                                                |
| 9D76B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Strand Infants School                                                                  |
| 3312E69D-C6AE-E311-B8ED-005056822391 | 10075068    | Blue Gate Fields Junior School                                                         |
| E0CDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Pamphill Voluntary Controlled Church of England Fi                                     |
| 4E0CFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Churchend Primary Academy                                                              |
| 8FB9ED97-C6AE-E311-B8ED-005056822391 | 10071945    | Queenswell Infant                                                                      |
| BF526A73-C7AE-E311-B8ED-005056822391 | 10015393    | Denstone College                                                                       |
| 50CDA8CD-C6AE-E311-B8ED-005056822391 | 10071587    | Breamore School                                                                        |
| 0DF73F21-C7AE-E311-B8ED-005056822391 | NULL        | St John Cass's Foundation and Redcoat School                                           |
| 19F73F21-C7AE-E311-B8ED-005056822391 | 10014878    | All Hallows RC High School                                                             |
| FFC2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Howard Primary School                                                                  |
| 80BCED97-C6AE-E311-B8ED-005056822391 | NULL        | Westwood Primary School                                                                |
| 148D540F-C7AE-E311-B8ED-005056822391 | 10000627    | Belgrave High School                                                                   |
| 50859443-C7AE-E311-B8ED-005056822391 | 10006949    | Torquay Boys Grammar                                                                   |
| 4A1ACFAF-C6AE-E311-B8ED-005056822391 | NULL        | New Christ Church VA Primary School                                                    |
| 18876BFD-C6AE-E311-B8ED-005056822391 | 10014827    | Bedale High School                                                                     |
| B87A99D9-C6AE-E311-B8ED-005056822391 | 10073959    | St Luke's Primary School                                                               |
| 7F17CFAF-C6AE-E311-B8ED-005056822391 | 10079053    | Wheatfields School                                                                     |
| 011ACFAF-C6AE-E311-B8ED-005056822391 | 10073336    | Blean Primary School                                                                   |
| AF71C7B5-C6AE-E311-B8ED-005056822391 | 10069490    | Emerson Valley Combined School                                                         |
| 0A408F49-C7AE-E311-B8ED-005056822391 | 10003631    | King Edward VI School                                                                  |
| 8AD591DF-C6AE-E311-B8ED-005056822391 | 10074400    | Christ The King Catholic Primary School, Thornbury                                     |
| 9B6EC7B5-C6AE-E311-B8ED-005056822391 | 10069576    | Weyford Junior School                                                                  |
| 6BCDA8CD-C6AE-E311-B8ED-005056822391 | 10069060    | Howick CE Primary School                                                               |
| 4DEE7F55-C7AE-E311-B8ED-005056822391 | 10001950    | Devizes School                                                                         |
| E5B9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Roxeth Mannor Middle School                                                            |
| 7EDB7AF1-C6AE-E311-B8ED-005056822391 | 10002987    | Hedingham School                                                                       |
| 7862F591-C6AE-E311-B8ED-005056822391 | NULL        | Woodside Junior School                                                                 |
| 0DB03A27-C7AE-E311-B8ED-005056822391 | NULL        | St Francis C of E School                                                               |
| 8F3E451B-C7AE-E311-B8ED-005056822391 | 10005380    | Ranelagh Church of England School                                                      |
| 1D25A1D3-C6AE-E311-B8ED-005056822391 | NULL        | North Leverton County Primary School                                                   |
| 00C92DD3-C7AE-E311-B8ED-005056822391 | NULL        | The College of Ripon and York St John                                                  |
| C5C0D6A9-C6AE-E311-B8ED-005056822391 | 10073572    | Limbrick Wood Primary School                                                           |
| 67BFD6A9-C6AE-E311-B8ED-005056822391 | 10069557    | Watton-at-Stone Primary & Nursery School                                               |
| CC0DFD8B-C6AE-E311-B8ED-005056822391 | 10070112    | Blackshaw CP                                                                           |
| 9A7A589D-C7AE-E311-B8ED-005056822391 | 10015238    | Brookfields Special School                                                             |
| EFBBED97-C6AE-E311-B8ED-005056822391 | 10069376    | Denbigh Community Primary School                                                       |
| 8D0BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Ludgvan Community Primary School                                                       |
| 25EE667F-C7AE-E311-B8ED-005056822391 | 10017941    | Hill House School                                                                      |
| EE056786-C8AE-E311-B8ED-005056822391 | NULL        | Burntwood School                                                                       |
| 3DCC372D-C7AE-E311-B8ED-005056822391 | 10006161    | St Francis Xavier College                                                              |
| 53CCA8CD-C6AE-E311-B8ED-005056822391 | 10073751    | Fosters Primary School                                                                 |
| 4BCEA8CD-C6AE-E311-B8ED-005056822391 | 10078922    | Shinfield St Mary's CofE Junior School                                                 |
| 4DED667F-C7AE-E311-B8ED-005056822391 | 10008436    | Prior's Field School                                                                   |
| 15996F6D-C7AE-E311-B8ED-005056822391 | 10008156    | Channing School                                                                        |
| 6674B0C7-C6AE-E311-B8ED-005056822391 | 10073934    | Mission C P School                                                                     |
| E1B50486-C6AE-E311-B8ED-005056822391 | 10071919    | Field End Junior                                                                       |
| 2B68DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Crowle Primary School                                                                  |
| 5B298AE5-C6AE-E311-B8ED-005056822391 | NULL        | The Marist Catholic Primary School                                                     |
| 922D8AE5-C6AE-E311-B8ED-005056822391 | 10080324    | St Alban's Roman Catholic Primary School Blackburn                                     |
| 6FD591DF-C6AE-E311-B8ED-005056822391 | 10080178    | St John the Baptist Primary School                                                     |
| 806EC7B5-C6AE-E311-B8ED-005056822391 | NULL        | Manor Field First School                                                               |
| 11B40486-C6AE-E311-B8ED-005056822391 | 10072896    | Childs Hill Primary School                                                             |
| BA7A589D-C7AE-E311-B8ED-005056822391 | NULL        | Littledown School                                                                      |
| A77B99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Paulinus RC VA Primary School                                                       |
| DD1DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Sir William Burrough Primary School                                                    |
| 687C99D9-C6AE-E311-B8ED-005056822391 | NULL        | St Joseph's RC Primary School                                                          |
| DB2C8AE5-C6AE-E311-B8ED-005056822391 | 10080351    | St Joseph's Catholic Primary School                                                    |
| CAE44C15-C7AE-E311-B8ED-005056822391 | 10015211    | Conisborough College                                                                   |
| 5063F591-C6AE-E311-B8ED-005056822391 | 10068844    | Great Bridge Primary School                                                            |
| E625A1D3-C6AE-E311-B8ED-005056822391 | 10078823    | Ovingham Church of England First School                                                |
| 587A589D-C7AE-E311-B8ED-005056822391 | 10016475    | Meadowgate School                                                                      |
| 038A6BFD-C6AE-E311-B8ED-005056822391 | 10002095    | Earlham High School                                                                    |
| 790EFD8B-C6AE-E311-B8ED-005056822391 | 10078198    | Culvers House Primary School                                                           |
| 01CCA8CD-C6AE-E311-B8ED-005056822391 | 10074690    | Little Bloxwich CE Primary School                                                      |
| B70CFD8B-C6AE-E311-B8ED-005056822391 | 10070896    | Reinwood Infant and Nursery School                                                     |
| 63CDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Crigglestone St James CofE Primary Academy                                             |
| 7B25A1D3-C6AE-E311-B8ED-005056822391 | 10078131    | Leyland Methodist Junior School                                                        |
| C2C0D6A9-C6AE-E311-B8ED-005056822391 | 10076563    | Raddlebarn Junior and Infant School                                                    |
| 846FC7B5-C6AE-E311-B8ED-005056822391 | 10068801    | Broad Oak Primary School                                                               |
| 6B0C6579-C7AE-E311-B8ED-005056822391 | 10008423    | The Perse School for Girls                                                             |
| 078182EB-C6AE-E311-B8ED-005056822391 | 10006965    | Townley Grammar School                                                                 |
| 3FC9BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Wolvercote First School                                                                |
| 2075B0C7-C6AE-E311-B8ED-005056822391 | 10072755    | Busbridge Infant School                                                                |
| A40A6485-C7AE-E311-B8ED-005056822391 | 10008414    | Our Lady's Convent Senior School                                                       |
| AEB03A27-C7AE-E311-B8ED-005056822391 | 10069631    | North Somercotes C of E Primary School                                                 |
| 7D6BDEA3-C6AE-E311-B8ED-005056822391 | NULL        | Priory Lane Junior School                                                              |
| 1618CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Balfour Junior School                                                                  |
| 7ED45197-C7AE-E311-B8ED-005056822391 | 10010424    | Lancaster School                                                                       |
| C88F540F-C7AE-E311-B8ED-005056822391 | 10001791    | Culcheth High School                                                                   |
| 4377B0C7-C6AE-E311-B8ED-005056822391 | 10075453    | Bishopstone CE Primary School                                                          |
| F5B60486-C6AE-E311-B8ED-005056822391 | NULL        | Kingsley Primary                                                                       |
| B07A589D-C7AE-E311-B8ED-005056822391 | 10076683    | Springwood Primary School                                                              |
| 9E1B5791-C7AE-E311-B8ED-005056822391 | NULL        | Dysart School                                                                          |
| D3876BFD-C6AE-E311-B8ED-005056822391 | NULL        | Trinity School                                                                         |
| C7D97AF1-C6AE-E311-B8ED-005056822391 | 10004443    | Moulton School                                                                         |
| AB95874F-C7AE-E311-B8ED-005056822391 | 10015432    | Gordons School                                                                         |
| A73B451B-C7AE-E311-B8ED-005056822391 | 10007480    | Whitcliffe Mount School                                                                |
| A18082EB-C6AE-E311-B8ED-005056822391 | 10076803    | Appleton Primary School                                                                |
| 3FE06303-C7AE-E311-B8ED-005056822391 | NULL        | Kings Houghton Middle School                                                           |
| C164F7FC-3EC6-E411-8070-005056822391 | NULL        | Hatfield Polytechnic                                                                   |
| BDBBED97-C6AE-E311-B8ED-005056822391 | 10072595    | Stocks Lane Primary School                                                             |
| B990540F-C7AE-E311-B8ED-005056822391 | 10017410    | Selby High School                                                                      |
| 477B99D9-C6AE-E311-B8ED-005056822391 | 10073984    | Princess Frederica School                                                              |
| 9862F591-C6AE-E311-B8ED-005056822391 | 10076114    | Newton Poppleford Primary School                                                       |
| 60B9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Linden Road CPs                                                                        |
| 991ACFAF-C6AE-E311-B8ED-005056822391 | 10079834    | Manor Farm Community Junior School                                                     |
| E3C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Epping Junior School                                                                   |
| F6CDA8CD-C6AE-E311-B8ED-005056822391 | 10071369    | St James' Church of England Voluntary Controlled Primary School                        |
| 770CFD8B-C6AE-E311-B8ED-005056822391 | 10077245    | Cowley Hill Primary School                                                             |
| 72C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Holmes Chapel Primary School                                                           |
| 1C18CFAF-C6AE-E311-B8ED-005056822391 | 10078610    | Charles Baines                                                                         |
| C718CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Great Torrington Junior                                                                |
| 95C8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | East Ardsley Primary School                                                            |
| 7970C7B5-C6AE-E311-B8ED-005056822391 | 10075231    | Hook Junior School                                                                     |
| 1FB50486-C6AE-E311-B8ED-005056822391 | 10076117    | Culmstock School                                                                       |
| BCD391DF-C6AE-E311-B8ED-005056822391 | 10070222    | St Thomas Aquinas Catholic Combined School                                             |
| 4565F591-C6AE-E311-B8ED-005056822391 | NULL        | St Andrew's Junior School                                                              |
| 610C6579-C7AE-E311-B8ED-005056822391 | 10008132    | Bury Grammer School (Girls)                                                            |
| 6662F591-C6AE-E311-B8ED-005056822391 | NULL        | Woodside Infant and Nursery School                                                     |
| B774B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Felmore Infant School                                                                  |
| 88C1D6A9-C6AE-E311-B8ED-005056822391 | 10072591    | Addingham Primary School                                                               |
| 2F5F0C80-C6AE-E311-B8ED-005056822391 | 10069016    | Ashton Gate Primary School                                                             |
| 5BCCA8CD-C6AE-E311-B8ED-005056822391 | 10073409    | Acle St Edmond Primary School                                                          |
| 3EB60486-C6AE-E311-B8ED-005056822391 | NULL        | Mayville Infant School                                                                 |
| F6E54C15-C7AE-E311-B8ED-005056822391 | 10003506    | John Wilmott School                                                                    |
| 7F355C09-C7AE-E311-B8ED-005056822391 | 10007326    | Walton High School                                                                     |
| E176B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Jesse Boot Primary School                                                              |
| 48E74C15-C7AE-E311-B8ED-005056822391 | 10015074    | Chafford Hundred Campus                                                                |
| 19C2D6A9-C6AE-E311-B8ED-005056822391 | 10069234    | Five Ways Primary School                                                               |
| 8BA77A5B-C7AE-E311-B8ED-005056822391 | 10005647    | Salesian School, Chertsey                                                              |
| 42E64C15-C7AE-E311-B8ED-005056822391 | NULL        | Haygrove School                                                                        |
| 7D0CFD8B-C6AE-E311-B8ED-005056822391 | 10076417    | Edinburgh Primary School                                                               |
| AE18CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Cottesbrooke Junior                                                                    |
| 4AD391DF-C6AE-E311-B8ED-005056822391 | 10075026    | St Columba's RC Primary                                                                |
| CC3E451B-C7AE-E311-B8ED-005056822391 | 10014029    | Saint Bede's Catholic School                                                           |
| 4C9A6F6D-C7AE-E311-B8ED-005056822391 | 10013990    | Longridge Towers School                                                                |
| 23D391DF-C6AE-E311-B8ED-005056822391 | 10076865    | St Catharine's Catholic Primary School                                                 |
| 0FE54C15-C7AE-E311-B8ED-005056822391 | 10017330    | Regent Park Girls School                                                               |
| 240DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Darrick Wood School                                                                    |
| A72F73F7-C6AE-E311-B8ED-005056822391 | 10002170    | Edgbarrow School                                                                       |
| 9E8282EB-C6AE-E311-B8ED-005056822391 | 10000750    | Blackfen School for Girls                                                              |
| AA7B99D9-C6AE-E311-B8ED-005056822391 | 10075470    | St Peter's C of E Primary School                                                       |
| 2E1FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Sidmouth Primary                                                                       |
| 21375C09-C7AE-E311-B8ED-005056822391 | 10017321    | Redhill School                                                                         |
| 16D291DF-C6AE-E311-B8ED-005056822391 | NULL        | St Benets RC Primary School                                                            |
| D6E74C15-C7AE-E311-B8ED-005056822391 | 10015910    | Henry Mellish Comprehensive School                                                     |
| 7AB60486-C6AE-E311-B8ED-005056822391 | 10072937    | Exning Primary School                                                                  |
| F5375C09-C7AE-E311-B8ED-005056822391 | NULL        | Woodford Lodge High School                                                             |
| 8C69DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Dudley Infant School                                                                   |
| 387D99D9-C6AE-E311-B8ED-005056822391 | 10070170    | St Patrick RC Primary School                                                           |
| 4FBCED97-C6AE-E311-B8ED-005056822391 | 10076690    | Rinsley Avenue Primary                                                                 |
| CB36ECD4-C9AE-E311-B8ED-005056822391 | 99999999    | Moray House Institute of Education                                                     |
| 1626A1D3-C6AE-E311-B8ED-005056822391 | NULL        | John Mayne C of E                                                                      |
| CE18CFAF-C6AE-E311-B8ED-005056822391 | 10079766    | Hawthorns First School                                                                 |
| 302D8AE5-C6AE-E311-B8ED-005056822391 | 10001234    | Cator Park School for Girls                                                            |
| A811E69D-C6AE-E311-B8ED-005056822391 | 10073366    | Ruskin Junior                                                                          |
| 0FD97AF1-C6AE-E311-B8ED-005056822391 | 10016706    | Lyndon School                                                                          |
| AFCB372D-C7AE-E311-B8ED-005056822391 | 10069675    | Barfield Road                                                                          |
| 861ACFAF-C6AE-E311-B8ED-005056822391 | 10077893    | Long Crendon School                                                                    |
| A9876BFD-C6AE-E311-B8ED-005056822391 | 10001688    | Cordeaux School                                                                        |
| F9E74C15-C7AE-E311-B8ED-005056822391 | 10017688    | Stanchester Community School                                                           |
| 7CBA41BA-C4AE-E311-B8ED-005056822391 | NULL        | University Of Durham                                                                   |
| ED23A1D3-C6AE-E311-B8ED-005056822391 | 10078143    | Stanford Junior and Infant School                                                      |
| C16ADEA3-C6AE-E311-B8ED-005056822391 | 10068901    | Owlsmoor Primary School                                                                |
| 987D99D9-C6AE-E311-B8ED-005056822391 | 10074732    | Christ Church CE School                                                                |
| FDC0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Parkwood Infant School                                                                 |
| B10EFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Hotwells Primary School                                                                |
| 7D1B7067-C7AE-E311-B8ED-005056822391 | 10008450    | Ratcliffe College                                                                      |
| BA23A1D3-C6AE-E311-B8ED-005056822391 | 10075949    | St Bartholomew's CofE Voluntary Controlled Primary School                              |
| 920C6579-C7AE-E311-B8ED-005056822391 | 10016184    | Kirkstone House School                                                                 |
| 54B9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Hazelbury Junior School                                                                |
| F20C6579-C7AE-E311-B8ED-005056822391 | NULL        | Hooke Court School                                                                     |
| AF096485-C7AE-E311-B8ED-005056822391 | 10017671    | St Edmunds School                                                                      |
| 430BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Tollgate Primary School                                                                |
| B2F63F21-C7AE-E311-B8ED-005056822391 | 10000605    | The Beckett School                                                                     |
| F918CFAF-C6AE-E311-B8ED-005056822391 | 10078606    | The Bythams Primary                                                                    |
| 14BBED97-C6AE-E311-B8ED-005056822391 | 10041979    | Outwood Primary Academy Lofthouse Gate                                                 |
| EDD150A3-C7AE-E311-B8ED-005056822391 | 10014800    | Abbey Court School                                                                     |
| F77A99D9-C6AE-E311-B8ED-005056822391 | 10071386    | Bowdon Church Primary School                                                           |
| 6D9A6F6D-C7AE-E311-B8ED-005056822391 | 10014050    | Nottingham High School                                                                 |
| 871DB8C1-C6AE-E311-B8ED-005056822391 | 10073925    | Stadhampton Primary                                                                    |
| 7E71C7B5-C6AE-E311-B8ED-005056822391 | 10072918    | Kelvin Grove Primary School                                                            |
| 1163F591-C6AE-E311-B8ED-005056822391 | 10080059    | Tiverton Primary School                                                                |
| 12876BFD-C6AE-E311-B8ED-005056822391 | 10015725    | Harrop Fold School                                                                     |
| AF3173F7-C6AE-E311-B8ED-005056822391 | 10016167    | Long Stratton High School                                                              |
| 301FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Abbey Junior School                                                                    |
| D510E69D-C6AE-E311-B8ED-005056822391 | NULL        | Radford Primary School                                                                 |
| 878382EB-C6AE-E311-B8ED-005056822391 | 10017287    | Barnwood Park High School                                                              |
| 20C2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Kingsway Primary School                                                                |
| 7262F591-C6AE-E311-B8ED-005056822391 | NULL        | Monkwick School                                                                        |
| 12F83F21-C7AE-E311-B8ED-005056822391 | NULL        | Deanery Primary School                                                                 |
| 8CD491DF-C6AE-E311-B8ED-005056822391 | NULL        | St John and St Francis Primary School                                                  |
| F220B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Thornbury Primary School                                                               |
| 0BC9BFBB-C6AE-E311-B8ED-005056822391 | 10072917    | Rushey Green Primary School                                                            |
| 25F73F21-C7AE-E311-B8ED-005056822391 | NULL        | Bishop Reindorp C of E School                                                          |
| FDE44C15-C7AE-E311-B8ED-005056822391 | 10003063    | High Storrs School                                                                     |
| F7E44C15-C7AE-E311-B8ED-005056822391 | 10007471    | Whalley Range 11-18 High School                                                        |
| 458E540F-C7AE-E311-B8ED-005056822391 | 10007896    | Belmont Comprehensive School                                                           |
| A11ACFAF-C6AE-E311-B8ED-005056822391 | NULL        | Hampton County Primary School                                                          |
| 1E77B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Tudor Court Primary School                                                             |
| 265C0C80-C6AE-E311-B8ED-005056822391 | NULL        | Wingate Community Nursery School                                                       |
| 19C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Gipsey Bridge Primary School                                                           |
| 0F5F0C80-C6AE-E311-B8ED-005056822391 | 10075051    | Bounds Green Junior School                                                             |
| 371EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Fairhouse Community Junior School                                                      |
| 13E16303-C7AE-E311-B8ED-005056822391 | 10016791    | Oakmeeds Community College                                                             |
| 9D7C99D9-C6AE-E311-B8ED-005056822391 | NULL        | Blackpool Church of England Primary School                                             |
| 27D591DF-C6AE-E311-B8ED-005056822391 | NULL        | St Gregory's RC (Aided) Infants School                                                 |
| 1625A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Malmesbury CE Primary                                                                  |
| 51EE7F55-C7AE-E311-B8ED-005056822391 | 10004096    | Longsands College                                                                      |
| D52A8AE5-C6AE-E311-B8ED-005056822391 | 10073999    | St Mary's Bryanston Square Primary School                                              |
| 91D45197-C7AE-E311-B8ED-005056822391 | 10016611    | Moselle School                                                                         |
| 810D6579-C7AE-E311-B8ED-005056822391 | NULL        | Port Regis, Motcombe                                                                   |
| 8A3173F7-C6AE-E311-B8ED-005056822391 | 10000230    | Allerton Grange School                                                                 |
| C4355C09-C7AE-E311-B8ED-005056822391 | 10000962    | Bruntcliffe School                                                                     |
| C3D491DF-C6AE-E311-B8ED-005056822391 | 10074731    | St Andrew's Church of England Primary                                                  |
| 623D451B-C7AE-E311-B8ED-005056822391 | 10001882    | De Lisle Catholic School                                                               |
| C324A1D3-C6AE-E311-B8ED-005056822391 | 10080541    | St. Andrew's CE Primary School                                                         |
| 4025A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Rougham Primary School                                                                 |
| E364F591-C6AE-E311-B8ED-005056822391 | NULL        | Kensal Rise Primary School                                                             |
| DDB60486-C6AE-E311-B8ED-005056822391 | 10078393    | Westmoor Primary School                                                                |
| 1F8A6BFD-C6AE-E311-B8ED-005056822391 | 10018421    | The Harris Middle School                                                               |
| 1D536A73-C7AE-E311-B8ED-005056822391 | NULL        | Brooklands School                                                                      |
| 84B03A27-C7AE-E311-B8ED-005056822391 | NULL        | G M School                                                                             |
| 88D491DF-C6AE-E311-B8ED-005056822391 | NULL        | St Marys Catholic Primary School                                                       |
| 2D625C8B-C7AE-E311-B8ED-005056822391 | 10080502    | Greenfield School                                                                      |
| 83849443-C7AE-E311-B8ED-005056822391 | 10006670    | The Grammar School for Girls Wilmington                                                |
| 6812E69D-C6AE-E311-B8ED-005056822391 | NULL        | Pipworth Junior School                                                                 |
| C91EB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Abbots Hall Junior Sch                                                                 |
| 9318CFAF-C6AE-E311-B8ED-005056822391 | 10076360    | Irby Primary School                                                                    |
| A075B0C7-C6AE-E311-B8ED-005056822391 | NULL        | John Bunyan Junior School                                                              |
| 55BCED97-C6AE-E311-B8ED-005056822391 | 10076703    | Cherry Lane Primary School                                                             |
| D369DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Ouston Junior School                                                                   |
| 0571C7B5-C6AE-E311-B8ED-005056822391 | 10069551    | Westfield CP JMI School                                                                |
| 9A2B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St Nicolas CE (VA) Primary School                                                      |
| 61EE667F-C7AE-E311-B8ED-005056822391 | 10013351    | Bethany School                                                                         |
| BA3B451B-C7AE-E311-B8ED-005056822391 | 10000877    | Bridgnorth Endowed School                                                              |
| 965D0C80-C6AE-E311-B8ED-005056822391 | NULL        | Thomas Coram Early Childhood Centre                                                    |
| 99D291DF-C6AE-E311-B8ED-005056822391 | 10075461    | St Mary's Church of England Primary School                                             |
| 0169DEA3-C6AE-E311-B8ED-005056822391 | 10080667    | Rodborough Community Primary School                                                    |
| 3069DEA3-C6AE-E311-B8ED-005056822391 | 10073862    | Arunside Primary                                                                       |
| D6F63F21-C7AE-E311-B8ED-005056822391 | NULL        | Queen Elizabeth Girls Upper School                                                     |
| 131EB8C1-C6AE-E311-B8ED-005056822391 | 10072798    | West Bridgford Infant School                                                           |
| 658182EB-C6AE-E311-B8ED-005056822391 | 10006803    | The Robert Smythe School                                                               |
| 427E99D9-C6AE-E311-B8ED-005056822391 | 10078156    | St Bega's RC VA Primary School                                                         |
| D62F73F7-C6AE-E311-B8ED-005056822391 | 10007204    | Valentines High School                                                                 |
| 7223A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Crazies Hill CE Primary School                                                         |
| 1ABAED97-C6AE-E311-B8ED-005056822391 | 10080020    | Beavers Community Primary School                                                       |
| 9724A1D3-C6AE-E311-B8ED-005056822391 | 10077433    | Stoney Middleton Primary School                                                        |
| EA0DFD8B-C6AE-E311-B8ED-005056822391 | 10071307    | Marsh Green School                                                                     |
| 2717CFAF-C6AE-E311-B8ED-005056822391 | 10076142    | Overseal Primary School                                                                |
| 515E0C80-C6AE-E311-B8ED-005056822391 | NULL        | Flamstead End Primary School                                                           |
| 2FE74C15-C7AE-E311-B8ED-005056822391 | 10007205    | Valley Comprehensive School                                                            |
| 172C8AE5-C6AE-E311-B8ED-005056822391 | 10072697    | Holy Family RC Primary School                                                          |
| 41395C09-C7AE-E311-B8ED-005056822391 | 10015410    | Dowdales School                                                                        |
| 032B8AE5-C6AE-E311-B8ED-005056822391 | 10074734    | St Mary's CE Primary School                                                            |
| FBBAED97-C6AE-E311-B8ED-005056822391 | 10078064    | Brookfield                                                                             |
| BF2A8AE5-C6AE-E311-B8ED-005056822391 | NULL        | Independent Jewish Day School                                                          |
| ACC8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Tiverton Primary School                                                                |
| 456ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Great Wakering Primary School                                                          |
| 8E0EFD8B-C6AE-E311-B8ED-005056822391 | 10076586    | Thorpe Hall Primary School                                                             |
| 85F83F21-C7AE-E311-B8ED-005056822391 | 10002077    | Dyson Perrins CE High School                                                           |
| 0E2B8AE5-C6AE-E311-B8ED-005056822391 | NULL        | St George's Roman Catholic Primary School                                              |
| 54A77A5B-C7AE-E311-B8ED-005056822391 | 10005502    | Robert Pattinson School                                                                |
| 4925A1D3-C6AE-E311-B8ED-005056822391 | 10081308    | High Beech C of E VC Primary                                                           |
| 301ACFAF-C6AE-E311-B8ED-005056822391 | 10076408    | Blakesley Hall Primary School                                                          |
| A6096485-C7AE-E311-B8ED-005056822391 | NULL        | Bedgebury School                                                                       |
| BB65F591-C6AE-E311-B8ED-005056822391 | 10077201    | Carlton Colville Primary                                                               |
| 9DE74C15-C7AE-E311-B8ED-005056822391 | NULL        | The Queen Mary School                                                                  |
| 2FD591DF-C6AE-E311-B8ED-005056822391 | NULL        | St Bernadette's Primary School                                                         |
| E5C2D6A9-C6AE-E311-B8ED-005056822391 | 10072985    | Ashton Primary                                                                         |
| 3A17CFAF-C6AE-E311-B8ED-005056822391 | NULL        | The Arbours Lower School                                                               |
| 01886BFD-C6AE-E311-B8ED-005056822391 | NULL        | Sandhurst School                                                                       |
| 7413E69D-C6AE-E311-B8ED-005056822391 | NULL        | New Ford Primary School                                                                |
| 12DB7AF1-C6AE-E311-B8ED-005056822391 | 10003009    | Hengrove Community Arts College                                                        |
| 44D391DF-C6AE-E311-B8ED-005056822391 | 10070493    | St Pauls Church of England (Aided) Primary School                                      |
| 8F71C7B5-C6AE-E311-B8ED-005056822391 | 10069315    | Grangetown Primary School                                                              |
| AD056786-C8AE-E311-B8ED-005056822391 | NULL        | Heworth Grange Comprehensive School                                                    |
| F461F591-C6AE-E311-B8ED-005056822391 | 10078956    | Gorse Hill Primary School                                                              |
| 5D536A73-C7AE-E311-B8ED-005056822391 | 10003779    | Langley School                                                                         |
| C51A7067-C7AE-E311-B8ED-005056822391 | 10016987    | The John Loughborough School                                                           |
| 897C99D9-C6AE-E311-B8ED-005056822391 | 10073990    | St Johns CE Primary School                                                             |
| 6E6ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Lincoln Gardens Infant School                                                          |
| 9C0DFD8B-C6AE-E311-B8ED-005056822391 | 10075056    | Roe Green Junior School                                                                |
| C07C99D9-C6AE-E311-B8ED-005056822391 | 10076778    | Abbas and Templecombe Church of England Primary Sc                                     |
| 06BE1658-B9B6-E311-8A4F-005056822390 | 10001111    | Department for Education                                                               |
| F3298AE5-C6AE-E311-B8ED-005056822391 | 10070151    | St Peter's Catholic Primary School                                                     |
| 7C65F591-C6AE-E311-B8ED-005056822391 | NULL        | Hangleton Junior School                                                                |
| A126A1D3-C6AE-E311-B8ED-005056822391 | 10041524    | Cherry Tree Primary School and Nursery                                                 |
| 5A74B0C7-C6AE-E311-B8ED-005056822391 | 10073374    | Molescroft Primary School                                                              |
| 067D99D9-C6AE-E311-B8ED-005056822391 | NULL        | Pulford Lower School                                                                   |
| 9B68DEA3-C6AE-E311-B8ED-005056822391 | 10077148    | Marton Grove Primary School                                                            |
| 9D11E69D-C6AE-E311-B8ED-005056822391 | NULL        | Marlborough Road Primary                                                               |
| 1D76B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Goose Green Primary School                                                             |
| 5A0A6485-C7AE-E311-B8ED-005056822391 | 10008521    | Colet Court School                                                                     |
| 3168DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Oakley Lower School                                                                    |
| F5615C8B-C7AE-E311-B8ED-005056822391 | 10077626    | The White House Preparatory School                                                     |
| A76EC7B5-C6AE-E311-B8ED-005056822391 | 10075865    | Cofton Primary School                                                                  |
| 70ED7F55-C7AE-E311-B8ED-005056822391 | 10005144    | Poole Grammar School                                                                   |
| 66B9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Harewood Junior School                                                                 |
| 51C7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Eastgate Primary School                                                                |
| AD3B451B-C7AE-E311-B8ED-005056822391 | 10000879    | Bridlington School Sports College                                                      |
| DF6ADEA3-C6AE-E311-B8ED-005056822391 | NULL        | Parkland Junior School                                                                 |
| 9DE06303-C7AE-E311-B8ED-005056822391 | 10006578    | The Amersham School                                                                    |
| A2365C09-C7AE-E311-B8ED-005056822391 | 10014824    | Bordesley Green Girls School                                                           |
| BDE64C15-C7AE-E311-B8ED-005056822391 | 10003120    | Holland Park School                                                                    |
| 4D056786-C8AE-E311-B8ED-005056822391 | NULL        | University Of Wales Institute, Cardiff                                                 |
| 80BA41BA-C4AE-E311-B8ED-005056822391 | NULL        | Rose Bruford College                                                                   |
| F58C540F-C7AE-E311-B8ED-005056822391 | 10007284    | Wadebridge School                                                                      |
| 5CCC372D-C7AE-E311-B8ED-005056822391 | 10015121    | Castle Hall Academy Trust                                                              |
| FF3B451B-C7AE-E311-B8ED-005056822391 | 10003258    | Ilkley Grammar School                                                                  |
| 1E0D6579-C7AE-E311-B8ED-005056822391 | 10015187    | Chafyn Grove School                                                                    |
| AB0BFD8B-C6AE-E311-B8ED-005056822391 | 10073303    | West End Primary School                                                                |
| B30DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Woodlands Infants                                                                      |
| BBE44C15-C7AE-E311-B8ED-005056822391 | 10004995    | Parrs Wood Technology College                                                          |
| C813E69D-C6AE-E311-B8ED-005056822391 | NULL        | Birch Copse Primary School                                                             |
| B976B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Wybers Wood Infant School                                                              |
| 69ED7F55-C7AE-E311-B8ED-005056822391 | 10002581    | Friesland School                                                                       |
| A71DB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Stockwell Junior School                                                                |
| 142D8AE5-C6AE-E311-B8ED-005056822391 | 10016859    | Manning Comprehensive School                                                           |
| 82896BFD-C6AE-E311-B8ED-005056822391 | 10015495    | Durrington High School                                                                 |
| F017CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Beechwood Infant School                                                                |
| 4DC9BFBB-C6AE-E311-B8ED-005056822391 | 10078025    | Sebright School                                                                        |
| 05EE667F-C7AE-E311-B8ED-005056822391 | 10018609    | Beacon School                                                                          |
| BF1A5791-C7AE-E311-B8ED-005056822391 | 10013281    | Sir William Perkins                                                                    |
| E723A1D3-C6AE-E311-B8ED-005056822391 | 10078876    | Grayshott Primary School                                                               |
| A8BA41BA-C4AE-E311-B8ED-005056822391 | NULL        | University Of Wolverhampton                                                            |
| BCBFD6A9-C6AE-E311-B8ED-005056822391 | 10072775    | Fetcham Village Infant School                                                          |
| EF18CFAF-C6AE-E311-B8ED-005056822391 | 10070607    | Church Crookham Junior School                                                          |
| 52F73F21-C7AE-E311-B8ED-005056822391 | 10017233    | Newlands School FCJ                                                                    |
| FDC8BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Sythwood Primary School                                                                |
| C33173F7-C6AE-E311-B8ED-005056822391 | 10000630    | Belle Vue Girls' School                                                                |
| 18CC372D-C7AE-E311-B8ED-005056822391 | 10006628    | The Chadwell Heath Foundation                                                          |
| 913D451B-C7AE-E311-B8ED-005056822391 | 10013291    | Thomas Cowley High School                                                              |
| F5866BFD-C6AE-E311-B8ED-005056822391 | 10000997    | Burnham Grammar School                                                                 |
| C3D391DF-C6AE-E311-B8ED-005056822391 | NULL        | Trent Young's Endowed CE VA Primary School                                             |
| 6817CFAF-C6AE-E311-B8ED-005056822391 | 10070685    | Ripley Junior School                                                                   |
| BCCDA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Brill C of E Combined School                                                           |
| 79385C09-C7AE-E311-B8ED-005056822391 | 10015435    | Golden Hillock Community School and Sports College                                     |
| E5D150A3-C7AE-E311-B8ED-005056822391 | 10077078    | Saxon Wood School                                                                      |
| 3120B8C1-C6AE-E311-B8ED-005056822391 | NULL        | Cottingham Croxby Primary School                                                       |
| D10AFD8B-C6AE-E311-B8ED-005056822391 | 10077205    | Stanton CP School                                                                      |
| C276B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Wybers Wood Junior School                                                              |
| B43E8F49-C7AE-E311-B8ED-005056822391 | 10007053    | Trinity School                                                                         |
| 51D87AF1-C6AE-E311-B8ED-005056822391 | NULL        | Kersal High School                                                                     |
| 8A0BFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Earls Barton Junior School                                                             |
| 82C2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Malvern, Somers Park Primary School                                                    |
| 3C056786-C8AE-E311-B8ED-005056822391 | NULL        | University Of The West Of England, Bristol                                             |
| FA8382EB-C6AE-E311-B8ED-005056822391 | 10016018    | Hylton Red House School                                                                |
| 1570C7B5-C6AE-E311-B8ED-005056822391 | 10078290    | Germander Park First School                                                            |
| 85896BFD-C6AE-E311-B8ED-005056822391 | 10015630    | Willowfield School                                                                     |
| 65876BFD-C6AE-E311-B8ED-005056822391 | NULL        | Hermitage School                                                                       |
| A965F591-C6AE-E311-B8ED-005056822391 | 10077692    | Little Bowden Primary School                                                           |
| 9D0C6579-C7AE-E311-B8ED-005056822391 | 10017423    | St John's School                                                                       |
| 6BC9BFBB-C6AE-E311-B8ED-005056822391 | 10072250    | Raynville Primary School                                                               |
| 35C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Little Ridge Community Primary                                                         |
| 960AFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Roebuck Primary School                                                                 |
| 407B99D9-C6AE-E311-B8ED-005056822391 | 10070234    | St Anne's Catholic Primary School                                                      |
| 6A6FC7B5-C6AE-E311-B8ED-005056822391 | 10077126    | Old Hall Junior School                                                                 |
| A619CFAF-C6AE-E311-B8ED-005056822391 | NULL        | Whitby Heath Primary School                                                            |
| 3ED97AF1-C6AE-E311-B8ED-005056822391 | 10018782    | Uplands Middle School                                                                  |
| 84E54C15-C7AE-E311-B8ED-005056822391 | 10015720    | Handsworth Grange Community Sports College                                             |
| 461FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Dodmire Infants' School                                                                |
| DB62F591-C6AE-E311-B8ED-005056822391 | NULL        | Ward Green Primary School                                                              |
| 4D2A8AE5-C6AE-E311-B8ED-005056822391 | 10081250    | St Georges RC First and Middle School                                                  |
| C974B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Highlands Primary School                                                               |
| 77F63F21-C7AE-E311-B8ED-005056822391 | 10017211    | St Georges RC High School                                                              |
| 62FE7261-C7AE-E311-B8ED-005056822391 | 10001311    | Chancellor's School                                                                    |
| CB75B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Python Hill Infant School                                                              |
| E170C7B5-C6AE-E311-B8ED-005056822391 | 10076402    | Heath Mount School                                                                     |
| A562F591-C6AE-E311-B8ED-005056822391 | 10075063    | Warren Junior School                                                                   |
| 27DA7AF1-C6AE-E311-B8ED-005056822391 | 10007599    | Wood Green High School College of Sport                                                |
| 39D391DF-C6AE-E311-B8ED-005056822391 | 10076031    | St Joseph's Catholic School                                                            |
| 09D55197-C7AE-E311-B8ED-005056822391 | 10016870    | Whitefield                                                                             |
| 8A7A589D-C7AE-E311-B8ED-005056822391 | 10016795    | Oakwood High School                                                                    |
| ACE44C15-C7AE-E311-B8ED-005056822391 | 10005804    | Sherburn High School                                                                   |
| 983D451B-C7AE-E311-B8ED-005056822391 | NULL        | King Alfred's Middle School                                                            |
| D67C99D9-C6AE-E311-B8ED-005056822391 | 10073416    | St Faith and St Martin Junior School                                                   |
| 8070C7B5-C6AE-E311-B8ED-005056822391 | 10071181    | Newburgh Primary School                                                                |
| 8FAF3A27-C7AE-E311-B8ED-005056822391 | NULL        | Cippenham Middle School                                                                |
| AE1A5791-C7AE-E311-B8ED-005056822391 | 10006147    | St David's School                                                                      |
| 51886BFD-C6AE-E311-B8ED-005056822391 | 10002436    | Felpham Community College                                                              |
| F675B0C7-C6AE-E311-B8ED-005056822391 | 10074650    | Glenbrook Infants School                                                               |
| E7CEA8CD-C6AE-E311-B8ED-005056822391 | 10079344    | Berkswich Primary School                                                               |
| 2CEE667F-C7AE-E311-B8ED-005056822391 | NULL        | Spratton Hall School                                                                   |
| 8EC6BFBB-C6AE-E311-B8ED-005056822391 | 10072943    | St Leonard's Primary                                                                   |
| 438482EB-C6AE-E311-B8ED-005056822391 | 10016157    | Kaskenmoor School                                                                      |
| 9164F591-C6AE-E311-B8ED-005056822391 | 10076601    | John Bramstone Primary                                                                 |
| 25385C09-C7AE-E311-B8ED-005056822391 | 10017472    | Perins Community School                                                                |
| 4896874F-C7AE-E311-B8ED-005056822391 | 10000880    | Brighouse High School                                                                  |
| A3385C09-C7AE-E311-B8ED-005056822391 | NULL        | Didcot Girls' School                                                                   |
| 9371C7B5-C6AE-E311-B8ED-005056822391 | NULL        | Cumbria Primary School                                                                 |
| 2D3D451B-C7AE-E311-B8ED-005056822391 | NULL        | St Peter's School                                                                      |
| E10AFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Woodfield Infant School                                                                |
| F93D451B-C7AE-E311-B8ED-005056822391 | 10018886    | Blackbourne CE VC Middle School                                                        |
| 96CCA8CD-C6AE-E311-B8ED-005056822391 | NULL        | Manor Fields Junior Mixed and Infant School                                            |
| 755F0C80-C6AE-E311-B8ED-005056822391 | 10069978    | Butts Primary School                                                                   |
| 26CEA8CD-C6AE-E311-B8ED-005056822391 | 10073774    | Bonners C of E School                                                                  |
| F47B99D9-C6AE-E311-B8ED-005056822391 | 10076267    | Bishop Tufnell CE Junior School                                                        |
| 3DB50486-C6AE-E311-B8ED-005056822391 | NULL        | Drew Primary School                                                                    |
| 5FA77A5B-C7AE-E311-B8ED-005056822391 | 10001268    | Central Technical College                                                              |
| 6EB03A27-C7AE-E311-B8ED-005056822391 | NULL        | Northwick Park Infant and Nursery School                                               |
| 551FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Southcoates Primary                                                                    |
| 1A12E69D-C6AE-E311-B8ED-005056822391 | NULL        | Craven Primary                                                                         |
| 7C8382EB-C6AE-E311-B8ED-005056822391 | 10002374    | Exmouth Community College                                                              |
| 0BB40486-C6AE-E311-B8ED-005056822391 | 10071310    | Manor Infant School                                                                    |
| 31C1D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Outwoods Edge Community Primary School                                                 |
| 7D375C09-C7AE-E311-B8ED-005056822391 | 10007522    | Wilnecote High School                                                                  |
| 8E3073F7-C6AE-E311-B8ED-005056822391 | 10018507    | West End Middle School                                                                 |
| 981FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Linden Grove Primary School                                                            |
| A176B0C7-C6AE-E311-B8ED-005056822391 | NULL        | Broadmere Community Primary School                                                     |
| 43C2D6A9-C6AE-E311-B8ED-005056822391 | 10076231    | St Katherine's School                                                                  |
| 70D491DF-C6AE-E311-B8ED-005056822391 | 10072065    | Chorley St James' C.E. Primary School                                                  |
| CCB9ED97-C6AE-E311-B8ED-005056822391 | NULL        | Adderley Green Infants School                                                          |
| 82D45197-C7AE-E311-B8ED-005056822391 | NULL        | Calder View Community Special School                                                   |
| CB896BFD-C6AE-E311-B8ED-005056822391 | 10017141    | Queen Eleanor Community School                                                         |
| 6F8D540F-C7AE-E311-B8ED-005056822391 | 10017034    | Magna Carta School                                                                     |
| E123CB9C-7CA5-E511-B8CB-005056822390 | NULL        | Newcastle Polytechnic                                                                  |
| EF1B5791-C7AE-E311-B8ED-005056822391 | 10017119    | Pinewood School                                                                        |
| 281FB8C1-C6AE-E311-B8ED-005056822391 | NULL        | Winton Primary School                                                                  |
| ECC7BFBB-C6AE-E311-B8ED-005056822391 | NULL        | Glenfield Infant School                                                                |
| BABCED97-C6AE-E311-B8ED-005056822391 | NULL        | Redbridge School                                                                       |
| DE0DFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Cauldon Primary School                                                                 |
| 50ED667F-C7AE-E311-B8ED-005056822391 | 10005574    | Rugby School                                                                           |
| 88C2D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Oaklands Junior School                                                                 |
| 671EB8C1-C6AE-E311-B8ED-005056822391 | 10073722    | Sunnymede Juniors                                                                      |
| CB2B8AE5-C6AE-E311-B8ED-005056822391 | 10070981    | Little Hallingbury C of E (VA) Primary School                                          |
| 1CDA7AF1-C6AE-E311-B8ED-005056822391 | 10015612    | Westwood High School                                                                   |
| A65E0C80-C6AE-E311-B8ED-005056822391 | 10071284    | Brentfield Primary School                                                              |
| 1CC6BFBB-C6AE-E311-B8ED-005056822391 | 10080432    | The Giles Nursery and Infant School                                                    |
| EBAF3A27-C7AE-E311-B8ED-005056822391 | NULL        | Moggerhanger Lower School                                                              |
| 622D8AE5-C6AE-E311-B8ED-005056822391 | 10007979    | Northgate High School                                                                  |
| 7A24A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Prees CE Primary School                                                                |
| 4C2B8AE5-C6AE-E311-B8ED-005056822391 | 10073433    | Singleton CE Primary School                                                            |
| 5FDF6303-C7AE-E311-B8ED-005056822391 | 10003082    | Highsted Grammar School for Girls                                                      |
| 67CEA8CD-C6AE-E311-B8ED-005056822391 | 10079290    | The Leasingham St Andrew's Church of England Prima                                     |
| FBDE6303-C7AE-E311-B8ED-005056822391 | NULL        | Chapter School                                                                         |
| 137D99D9-C6AE-E311-B8ED-005056822391 | 10075733    | Englefield C of E Primary School                                                       |
| 9468DEA3-C6AE-E311-B8ED-005056822391 | NULL        | Eastwood Community School                                                              |
| AA1A7067-C7AE-E311-B8ED-005056822391 | NULL        | Upbury Manor School                                                                    |
| 6C0D6579-C7AE-E311-B8ED-005056822391 | 10008404    | Nottingham High School for Girls                                                       |
| 6023A1D3-C6AE-E311-B8ED-005056822391 | 10075739    | Warfield Church of England Primary School                                              |
| 4A63F591-C6AE-E311-B8ED-005056822391 | 10076605    | Woodlands Junior School                                                                |
| CF63F591-C6AE-E311-B8ED-005056822391 | 10076213    | Lostock Hall Community Primary School                                                  |
| 3D7C99D9-C6AE-E311-B8ED-005056822391 | 10073979    | St Andrew's C of E Primary School                                                      |
| D7615C8B-C7AE-E311-B8ED-005056822391 | 10072289    | Cameron House School                                                                   |
| 8FC1D6A9-C6AE-E311-B8ED-005056822391 | 10074081    | Bottesford Infants School                                                              |
| FC1A7067-C7AE-E311-B8ED-005056822391 | 10015994    | Turney Primary and Secondary Special School                                            |
| ADD391DF-C6AE-E311-B8ED-005056822391 | 10070388    | Pencombe C of E Primary School                                                         |
| 048382EB-C6AE-E311-B8ED-005056822391 | 10017464    | Ruffwood School                                                                        |
| 63E16303-C7AE-E311-B8ED-005056822391 | 10017133    | The Norton School                                                                      |
| 8B0AFD8B-C6AE-E311-B8ED-005056822391 | NULL        | Ravensthorpe Infant and Nursery School                                                 |
| E01B5791-C7AE-E311-B8ED-005056822391 | 10016250    | Old Park School                                                                        |
| 3FC0D6A9-C6AE-E311-B8ED-005056822391 | 10073182    | Lyng Primary School                                                                    |
| 7B8E540F-C7AE-E311-B8ED-005056822391 | 10016040    | Turves Green Primary School                                                            |
| 3E0BFD8B-C6AE-E311-B8ED-005056822391 | 10073087    | Abbey Hulton Primary School                                                            |
| 85F73F21-C7AE-E311-B8ED-005056822391 | 10006128    | St Bernards Convent School                                                             |
| 4E7D99D9-C6AE-E311-B8ED-005056822391 | NULL        | Rockbeare CE Primary School                                                            |
| DD1EB8C1-C6AE-E311-B8ED-005056822391 | 10076470    | Hyde Park Infants' School                                                              |
| 15C0D6A9-C6AE-E311-B8ED-005056822391 | NULL        | Worth Primary School                                                                   |
| 7CDE6303-C7AE-E311-B8ED-005056822391 | NULL        | Goldings Middle School                                                                 |
| BC8F540F-C7AE-E311-B8ED-005056822391 | 10017372    | Springwell Community School                                                            |
| 9BC8BFBB-C6AE-E311-B8ED-005056822391 | 10070866    | Redwood Infants' School                                                                |
| 15C6BFBB-C6AE-E311-B8ED-005056822391 | 10074610    | Lyndhurst Primary School                                                               |
| C52F73F7-C6AE-E311-B8ED-005056822391 | 10009664    | Jfs School                                                                             |
| 4E6EC7B5-C6AE-E311-B8ED-005056822391 | 10072983    | Gisburn Primary School                                                                 |
| 20F83F21-C7AE-E311-B8ED-005056822391 | 10077496    | The Cottesloe School                                                                   |
| AF26A1D3-C6AE-E311-B8ED-005056822391 | 10080575    | New Hinksey Church of England First School                                             |
| 5B24A1D3-C6AE-E311-B8ED-005056822391 | NULL        | Potters Gate Primary School                                                            |

</details>
