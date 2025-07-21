# Logical Data Model

Logical representation of the key data entities and their relationships held within the Teaching Record System (TRS). To note, this does not represent how the data is stored physically.

```mermaid
---
title: Teaching Record System (TRS) Logical Data Model
id: 76caafdd-b8c0-4da8-a61a-77ae82c070ef
---
erDiagram

  Person ::: TRS

  Teaching-Alerts ::: TRS
  Teaching-Sanction-Prohibition ::: TMU

  Professional-Status ::: TRS
  Route-to-a-Professional-Status ::: TRS
  Recognition ::: AfQTS
  Teacher-training ::: Register
  Equivalence ::: SET
  Equivalence ::: EWC-Wales


  Teacher-Pension-Eligibility ::: TPS
  Employment ::: TRS
  Educational-Establishment ::: TRS
  Provider-Type ::: TRS

  Induction ::: TRS
  Induction-Status ::: TRS
  Induction-Exemption-Reason ::: TRS
  Induction-Period ::: CPD


  Person ||--o{ Teaching-Alerts : "Can Have"
  Person ||--o{ National-Professional-Qualification : "Can Have"
  Teaching-Alerts ||--o| Teaching-Sanction-Prohibition : "Reported as"


  Induction-Exemption-Reason||--o{ Induction-Status : "Affects"

  Route-to-a-Professional-Status ||--o| Induction-Exemption-Reason : "Might result in an"
  Person ||--o{ Induction-Exemption-Reason : "Can have an"
  Person ||--o{ Induction-Status : "Might need to complete an"
  Induction-Period }o--o| Induction : "Makes up"
  Induction ||--|| Induction-Status : "contributes to"



  Person ||--o{ Teacher-training : "Can do"
  Route-to-a-Professional-Status ||--o{ Teacher-training : "Might require"


  Person ||--o{ Recognition : "Might apply for"
  Recognition ||--|| Route-to-a-Professional-Status : "Which is a"

  Person ||--o{ Equivalence : "Might be granted"
  Equivalence ||--|| Route-to-a-Professional-Status : "Which is a"

  Teacher-training ||--o{ Teaching-Qualification : "Can Lead To"

  Person ||--o{ Teaching-Qualification : "Can hold"
  Professional-Status |o--o{ Teaching-Qualification  : "Can be a"


  Person ||--o{ Route-to-a-Professional-Status : "Could take a"
  Route-to-a-Professional-Status ||--o| Professional-Status : "And obtain"
  Person ||--o{ Professional-Status : "Can hold"



  Person ||--o{ Teacher-Pension-Eligibility : "Normally has"
  Teacher-Pension-Eligibility ||--|| Employment : "Reported as"
  Person ||--|{ Employment : "Could be in"
  Employment o|--|| Educational-Establishment : "at a"
  Educational-Establishment o|--|{ Provider : "Can be a"
  Provider ||--|| Provider-Type : "has a"

  Teacher-training ||--o{ Provider : "With a"


  Provider ::: TRS
  Teaching-Qualification ::: TRS

  classDef TRS fill:#054fb9
  classDef CPD fill:#c44601
  classDef TMU fill:#e6308a
  classDef Register fill:#8babf1
  classDef AfQTS fill:#5ba300
  classDef EWC-Wales fill:#fcc9b5,color:#000000
  classDef SET fill:#b3c7f7
  classDef TPS fill:#9b8bf4
```
