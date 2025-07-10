# Logical Data Model

Logical representation of the key data entities and their relationships held within the Teaching Record System (TRS). To note, this does not represent how the data is stored physically.

```mermaid
---
title: Teaching Record System (TRS) Logical Data Model
id: 76caafdd-b8c0-4da8-a61a-77ae82c070ef
---
erDiagram

  Person ||--o{ Teaching-Alerts : "Can Have"
  Teaching-Alerts ||--o| Teaching-Sanction-Prohibition : "Can Result In"


  Induction-Exemption-Reason||--o{ Induction : "Affects"

  Route-To-Professional-Status ||--o| Induction-Exemption-Reason : "Might result in an"
  Person ||--o{ Induction-Exemption-Reason : "Can have an"
  Person ||--o{ Induction : "Might need to complete an"



  Person ||--o{ Teacher-training : "Can do"
  Route-To-Professional-Status ||--o{ Teacher-training : "Might require"


  Person ||--o{ Recognition : "Might apply for"
  Recognition ||--|| Route-To-Professional-Status : "Which is a"

  Person ||--o{ Equivalence : "Might be granted"
  Equivalence ||--|| Route-To-Professional-Status : "Which is a"

  Teacher-training ||--o{ Teaching-Qualification : "Can Lead To"

  Person ||--o{ Teaching-Qualification : "Can hold"
  Professional-Status |o--o{ Teaching-Qualification  : "Can be a"


  Person ||--o{ Route-To-Professional-Status : "Could take a"
  Route-To-Professional-Status ||--o| Professional-Status : "And obtain"
  Person ||--o{ Professional-Status : "Can hold"



  Person ||--o{ Teacher-Pension-Eligibility : "Normally has"
  Teacher-Pension-Eligibility ||--|{ Employment : "Could be in"
  Employment o|--|| Educational-Establishment : "at a"
  Educational-Establishment o|--|{ Provider : "Can be a"
  Provider ||--|| Provider-Type : "has a"

  Teacher-training ||--o{ Provider : "With a"


```
