# Logical Data Model

Logical representation of the key data entities and their relationships held within the Teaching Record System (TRS). To note, this does not represent how the data is stored physically.
```mermaid
---
title: Teaching Record System (TRS) Logical Data Model
---
erDiagram
  Person ||--|{ Professional-Status : "Can Hold"
  Person ||--|{ Route-To-Professional-Status : "Could Take a"
  Person ||--|{ Teaching-Qualifications : "Can Hold"
  Person ||--|{ Teacher-training : "Can Take"
  Person ||--|{ Teacher-Pension-Eligibility : "Normally has"
  Teacher-Pension-Eligibility ||--|{ Employment : "Could be in"
  Employment ||--|{ Educational-Establishment : "at a"
  Educational-Establishment ||--|{ Provider : "Can be a"
  Professional-Status }o--o{ Teaching-Qualifications  : "Can be a"
  Route-To-Professional-Status ||--o{ Teacher-training : "Might Require"
  Route-To-Professional-Status ||--o{ Induction : "Might Need To Complete an"
  Route-To-Professional-Status ||--o{ Equivelence : "Might apply for"
  Equivelence ||--o{ Professional-Status : "And be awarded"
  Teacher-training ||--o{ Provider : "With a"
  Induction||--o{ Professional-Status : "Before They are Awarded"
  Teacher-training ||--o{ Teaching-Qualifications : "Can Lead To"
  Teacher-training ||--o{ Teaching-Training-Type : "is a"
  Provider ||--o{ Provider-Type : "is a"
  Person ||--|{ Teaching-Alerts : "Can Have"
  Teaching-Alerts ||--|{ Teaching-Sanction-Prohibition : "Can Result In"
```
