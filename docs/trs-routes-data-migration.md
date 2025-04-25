# Routes and Professional Statuses Data Migration

## DQT data structure

In DQT the Initial Teacher Training (ITT) and QTS Registration (QTS Reg) were not linked directly. They did not explicitly detail how a teacher gained Qualified Teacher Status (QTS), Partial QTS (PQTS), Early Years Practitioner Status (EYPS), or Early Years Teacher Status (EYTS). The data fields within them were not clearly mapped from external integrations, did not fit the conceptual model that DfE has developed, and did not contain good quality data.

## TRS data structure

TRS is an opportunity to define a sustainable, conceptually rigorous data architecture. There is a clear path from the Route into Teaching, to a Qualification.

## Data migration

### Data Fields

#### DQT

1. ITT
   1. Programme Type
   2. Qualification Type
   3. Result
2. QTS Reg
   1. Teacher Status

#### TRS

1. Route
   1. Route Type
   2. Regulatory Condition Type
   3. Qualification Type
   4. Professional Status
   5. Degree Type

### Mapping Structure

Mermaid of the map in the lucid

### Data Mapping Flow

1. TRS Reference Data
   1. We chose the TRS Route Types
   2. We chose the TRS Regulatory Conditions
   3. We attributed each Route Type to a Regulatory Condition
   4. We chose the TRS Qualification Types
   5. We attributed each Regulatory Condition to a Qualification Type
   6. We chose the TRS Professional Status Statuses
2. Blanket Mapping Rules
   1. We created blanket mapping rules for the DQT Teacher Statuses to Route Types
   2. We created blanket mapping rules for the DQT ITT Programme Types to Route Types
   3. We created blanket mapping rules for the DQT ITT Qualification Types to Route Types
3. Categorisation of TRS reference data
   1. We categorised DQT Qualification Types to TRS Degree Types
4. Yucky stuff
   1. Hardcoded mappings
   2. Single mappings (actually done with a bit more finesse than that - see the Precedence Field - when is it FALSE - then direct mappings)
   3. Bad mappings - new rules field this is all ELSEIF
      1. not the oldest ITT row AND no programme type - use QT
      2. manual case lookup returns PTQT, AND not oldest ITT row - use PTQT
      3. manual case lookup
      4. not the oldest ITT row - use PTQT
      5. bad hardcoded cases which are basically just from precedence but into DQT values
      6. programme type not null - use TSPT
      7. Use the field text identifier combination to check the one or two fields (TS = Teacher Status, PT = Programme Type, QT = Qualification Type) and to check the blanket field mappings in that precedence order. Take the first one which matches
5. Status mapping
   1.
