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

We pair QTS Registration rows and ITT rows based on the following logic:

1. Manual mapping by the operations teams of ITT to QTS Reg
2. Oldest ITT row -> QTS Reg row

Known as the Paired QTS/ITT combo.

Other ITT rows should not use Teacher Status to get Route ID (`statusDerivedRouteId`) but do still need to populate the Professional Status Award Date from that QTS Reg row if they create a new Route.

1. TRS Reference Data
   1. We chose the TRS Route Types
   2. We chose the TRS Regulatory Conditions
   3. We attributed each Route Type to a Regulatory Condition
   4. We chose the TRS Qualification Types
   5. We attributed each Regulatory Condition to a Qualification Type
   6. We chose the TRS Professional Status Statuses
2. Blanket Mapping Rules
   1. We created blanket mapping rules for the DQT Teacher Statuses to Route Types - `statusDerivedRouteId`
   2. We created blanket mapping rules for the DQT ITT Programme Types to Route Types - `programmeTypeDerivedRouteId`
   3. We created blanket mapping rules for the DQT ITT Qualification Types to Route Types - `TBC`
3. Yucky stuff
   1. Hardcoded mappings (top of the "TRS Route Type" Column - matching on combinations of TS, PT, QT - that don't go to blanket mapping available Route Types) - regardless of Paired QTS/ITT combo
   2. Single mappings (actually done with a bit more finesse than that - see the Precedence Field - when is it FALSE - then direct mappings) - When there isn't a precedence then `statusDerivedRouteId` > `programmeTypeDerivedRouteId` > `qualificationTypeDerivedRouteId`
   3. Bad mappings - new rules field this is all ELSEIF
      1. Paired QTS/ITT combo - use QT
      2. manual case lookup using a combination of TS,PT,QT onto the "Manual cases" sheet returns TS, AND paired QTS/ITT combo - use PTQT
      3. manual case lookup using a combination of TS,PT,QT onto the "Manual cases" sheet
      4. NOT Paired QTS/ITT combo - use PTQT
      5. Other hardcoded cases which are defined using DQT values (Simplified into combinations of TS, PT, QT with ORs and ANDs to reduce cases, making up the bulk of the "New Rules" column)
      6. DQT programme type not null - use TSPT
   4. We then use the field text identifier to check the one or two fields ("TS" = Teacher Status, "PT" = Programme Type, "QT" = Qualification Type, or a combination of them) and to check the blanket field mappings in that precedence order. Take the first one which matches (this logic is stored in the "TRS Route Type")
4. Status mapping
   1. We're getting rid of some records corresponding to certain statuses - see governance deck for more details
5. Other data fields - Categorisation of TRS reference data
   1. We categorised DQT Qualification Types to TRS Degree Types
   2. Country Mapping
   3. Provider Mapping
   4. Subject Mapping
   5. Age Range Mapping
   6. Induction Exemption Mapping
