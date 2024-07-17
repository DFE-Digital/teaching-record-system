# Data Model Decisions
## Context
In order to migrate data away from DQT, we must make decisions on what data entities and data is required to support TRS and the digital services it supports.The decisions will be made from both technical and business analysis and user research. The decisions will inform the the ERD (entitiy relationship diagram), migration plan, development of the TRS code base. We require one simple file to record these decisions.



| No. | Entity | Decision | Context | Consequences
| -------- | -------- | -------- |-------- |-------- |
| 1.| Qualification | Migrate    |Qualification is one of the main entities within TRS| We can record QTS, EY, NPQ, QTLS, MQ for a teaching record. Satisfying the statutory obligation on DfE to record QTS status.
 2.| Teaching Alert / Sanction | Migrate    |Alert is one of the main entities within TRS| We can record sanctions, decisions and other alerts. Satisfying the statutory obligation on DfE to record alerts for child safeguarding and make available to the appropriate stakeholders.
 3.| Induction | Migrate    |Mandatory induction is one of the main entities within TRS| We can record sanctions, decisions and other alerts. Satisfying the statutory obligation on DfE to record inductions for newly qualified teachers.
 4.| Mandatory Qualification | Migrate    |Mandatory qualification (sensory needs) is one of the main entities within TRS| We can record teachers with MQ's. Satisfying the statutory obligation on DfE to record MQ's for teachers who have this qualification.
 5.| Text | Text | Text |Text




## Status

Accepted: The analysis, reasons, UR supporting this decision has been presented and agreed by the appropriate delivery and policy stakeholders.


Repos Affected:
https://github.com/DFE-Digital/teaching-record-system






