# API Changelog

## 20240814

### `POST /v3/persons/find`

New endpoint added for bulk person lookup by TRN and date of birth.

### `GET /v3/persons?findBy=LastNameAndDateOfBirth&lastName={lastName}&dateOfBirth={dateOfBirth}`

`inductionStatus`, `qts` and `eyts` members have been added to align with the bulk `POST` endpoint.

## 20240606

All endpoints under `/teacher` and `/teachers` have been moved to `/person` and `/persons`, respectively.

The `email` property on the response from `/teacher` (now `/person`) and `/teachers/<trn>` (now `/persons/<trn>`) has been renamed to `emailAddress`.

The `email` property on the request to `/teacher/name-changes` (now `/person/name-changes`) and `/teacher/date-of-birth-changes` (now `/person/date-of-birth-changes`) has been renamed to `emailAddress`.

### `PUT /v3/trn-requests`

The scalar `email` property in the request has been replaced with an `emailAddresses` collection property so that multiple email addresses can be provided to match on.
The `person` property has been removed from the response.

### `GET /v3/trn-requests`

The `person` property has been removed from the response.

## 20240416

### `GET /v3/teachers/<trn>`

An additional query parameter can be specified for the `GET /v3/teachers/<trn>` operation - `dateOfBirth`.
When provided, it will be checked against the date of birth on the teaching record with `<trn>`; if it does not match a `404` will be returned.
The query parameter should be formatted `YYYY-MM-DD` e.g. `2024-04-16`.


## 20240412

### Change of name & Change of date of birth requests

These endpoints have been amended to use the ID/Teacher auth access token authorization mechanism.
The `POST /v3/teachers/<trn>/name-changes` and `POST /v3/teachers/<trn>/date-of-birth-changes` have been removed and
`POST /v3/teacher/name-changes` and `POST /v3/teacher/date-of-birth-changes` take their place.


## 20240307

### TRN Requests

Added `POST /v3/trn-requests` and `GET /v3/trn-requests` endpoints.


## 20240101

Initial V3 API release.
