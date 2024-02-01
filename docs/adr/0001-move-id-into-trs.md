# 1. Move ID into TRS

Date: 2023-11-30

## Status

Accepted

## Context

With the adoption of Gov.OneLogin into our services, it renders a proportion of Teaching Identity's functionality either duplicated or obsolete. Therefore, we reviewed how best to enable our services to
integrate with Gov.OneLogin and also provide the necessary teaching record authorisation (that Teaching Identity currently provides). We chose to keep the OAUTH based journey that allows services to pass a set of PII data to be checked against the database of qualified teachers (soon to be Teaching Record System).

## Decision

* Keep OAUTH journey code to authorise access to teaching record
* Move the code from Get an Identity into Teaching record system
* Archive / stop updating the Get an Identity code base
* Remove the uid (of teaching identity) from use within teaching services
* Use the GOV.OneLogin uid to join identities across services


## Consequences
Register for a national qualification and claim teacher payments services will need to  make relatively mionor changes to their matching code.

We will need to re-factor Teaching Record Sytem accordingly to provide teaching record authorisation via the OAUTH journey (copied from Get an Identity)

Repos Affected:
https://github.com/DFE-Digital/get-an-identity
https://github.com/DFE-Digital/teaching-record-system

