# Alerts API

Draft specification v0.1.


## `POST /alerts`

Creates an alert.

Request body structure:
```json
{
  "trn": "",
  "alertTypeId": "",
  "startDate": "",
  "endDate": "",
  "details": ""
}
```

If no record exists with the specified TRN, a `400 Bad Request` status code will be returned.

The `startDate` and `endDate` properties must be formatted `yyyy-MM-dd`.
`startDate` cannot be `null`.
The `endDate` and `details` properties are optional.
If specified, the `endDate` property must be after the `startDate`.

If the alert was created successfully, a `201 Created` status code will be returned.

The response returned is identical to that of the [`GET` endpoint](#get-alertsalertid).


## `GET /alerts/<alertId>`

Retrieves an alert.

If no alert exists with the specified ID, a `404 Not Found` status code will be returned.

If the alert exists, a `200 OK` status code will be returned with the following response body:

```json
{
  "alertId": "",
  "alertType": {
    "alertTypeId": "",
    "name": "",
    "alertCategory": {
      "alertCategoryId": "",
      "name": ""
    }
  },
  "startDate": "",
  "endDate": "",
  "person": {
    "trn": "",
    "firstName": "",
    "middleName": "",
    "lastName": "",
    "dateOfBirth": ""
  }
}
```


## `PATCH /alerts/<alertId>`

Updates an alert.

Request body structure:
```json
{
  "alertId": "",
  "startDate": "",
  "endDate": "",
  "details": ""
}
```

If no alert exists with the specified ID, a `404 Not Found` status code will be returned.

The `startDate` and `endDate` properties must be formatted `yyyy-MM-dd`.
All properties are optional; only the properties specified will be updated.
If specified, the `endDate` property must be after the `startDate`.

If the alert was updated successfully, an `200 OK` status code will be returned.

The response returned is identical to that of the [`GET` endpoint](#get-alertsalertid).


## `DELETE /alerts/<alertId>`

Deletes an alert.

If no alert exists with the specified ID, a `404 Not Found` status code will be returned.

If the alert was deleted successfully, a `204 No Content` status code will be returned.
