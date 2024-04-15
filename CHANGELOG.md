# API Changelog

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
