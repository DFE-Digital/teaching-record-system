# API Changelog

## vNext

An endpoint has been added at `GET /v3/trns/<trn>` to check whether a record with the given TRN exists and is active.

The following new endpoints have been added:
- `PUT /v3/persons/<trn>/welsh-induction` - to set a person's induction for teachers in Wales.

## 20250804
- Webhook messages have been added to notify when an alert is created, updated, or deleted.

## 20250627

### `PUT /v3/persons/<trn>/professional-statuses/<reference>`
- This endpoint has been moved to `/v3/persons/<trn>/routes-to-professional-statuses/<reference>`.
- The `routeTypeId` property has been renamed to `routeToProfessionalStatusTypeId`.
- The `Awarded` and `Approved` statuses have been replaced with `Holds`.
- The `awardedDate` property has been replaced with `holdsFrom`.

### `GET /v3/persons/<trn>` and `GET /v3/person` changes
- The `awarded` property in each member of `mandatoryQualifications` has been renamed to `endDate`.
- A `mandatoryQualificationId` property has been added to each member of `mandatoryQualifications`.
- `NpqQualifications` and `InitialTeacherTraining` can no longer be requested in the `include` query parameter.
- `RoutesToProfessionalStatuses` can be requested in the `include` query parameter.
- An `exemptions` property has been added to the `induction` property.
- The `awarded`, `certificateUrl` and `statusDescription` properties are no longer present on the `qts` and `eyts` objects.
In their place is `holdsFrom` and a list of the route types that apply.

### `GET /v3/persons?findBy=LastNameAndDateOfBirth` and `GET /v3/persons/find`
- The `inductionStatus` property has been replaced by an `induction` object.
- The `awarded`, `certificateUrl` and `statusDescription` properties are no longer present on the `qts` and `eyts` objects.
In their place is `holdsFrom` and a list of the route types that apply.

## 20250425

The following new endpoints have been added:
- `PUT /v3/persons/<trn>` - to set a person's PII.
- `PUT /v3/persons/<trn>/professional-statuses/<reference>` - to set a professional status.

The `GET /v3/trn-requests` and `POST /v3/trn-requests` endpoints return the following additional properties:
- `potentialDuplicate`;
- `accessYourTeachingQualificationsLink`.
  `accessYourTeachingQualificationsLink` will only be populated for `Completed` requests.

## 20250327

The `qts` object in responses to the following endpoints has a new `awardedOrApprovedCount` property:
- `GET /v3/persons/<trn>`
- `GET /v3/person`
- `GET /v3/persons?findBy=LastNameAndDateOfBirth`
- `GET /v3/persons/find`.

## 20250203

The `PUT /v3/persons/<trn>/cpd-induction` endpoint has been added.

The `GET /v3/persons/<trn>` endpoint now supports passing a `nationalInsuranceNumber` query parameter.
If specified, it must match the National Insurance number on the teaching record with TRN `<trn>`.

The TRN request endpoint `PUT /v3/trn-requests` has the following additional properties:
- `oneLoginUserSubject` (if the TRN request is for a One Login user);
- `identityVerified` (if the One Login user's identity has been verified).
- `gender`.

The ability to return higher education qualifications has been removed from `GET /v3/person` and `GET /v3/persons/<trn>`.

The `induction` object in responses to the following endpoints has the `statusDescription` property removed and
the `endDate` property replaced by `completedDate`:
- `GET /v3/persons/<trn>`
- `GET /v3/person`
- `GET /v3/persons?findBy=LastNameAndDateOfBirth`
- `GET /v3/persons/find`.

In addition, the status field will now only contain the following values:
- `None`
- `RequiredToComplete`
- `Exempt`
- `InProgress`
- `Passed`
- `Failed`
- `FailedInWales`.

Note that `null` will no longer be returned.

The responses for the following endpoints now contain a `qtlsStatus` property:
- `GET /v3/persons/<trn>`
- `GET /v3/person`
- `GET /v3/persons?findBy=LastNameAndDateOfBirth`
- `GET /v3/persons/find`.

The following certificate endpoints have been removed:
- `GET /v3/certificates/qts`
- `GET /v3/certificates/eyts`
- `GET /v3/certificates/induction`
- `GET /v3/certificates/npq/{qualificationId}`

## 20240920

All references to `sanctions` have been removed and replaced with `alerts`; the following endpoints are affected:
- `GET /v3/persons/<trn>`
- `GET /v3/person`
- `GET /v3/persons?findBy=LastNameAndDateOfBirth`
- `GET /v3/persons/find`

An endpoint has been added to mark a person as deceased: `PUT /v3/persons/deceased/<trn>`.


## 20240912

Endpoints have been added for setting and retrieving the QTS via QTLS date.
- `GET /v3/persons/<trn>/qtls`
- `PUT /v3/persons/<trn>/qtls`


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
