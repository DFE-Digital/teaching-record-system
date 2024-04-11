# Add/remove QTLS API

Draft specification v0.3.

There are two endpoints; one sets or removes the QTLS date for a person and another queries the existing QTLS status.
In all cases the person is identified by a TRN.

## `GET` `/persons/<trn>/qtls`

If no record exists with the specified TRN, a `404 Not Found` status code will be returned.

Response body structure:
```json
{
  "trn": "",
  "qtsDate": ""
}
```

The `qtsDate` property will be formatted `yyyy-MM-dd`. If the person does not have QTLS awarded then `qtsDate` will be `null`.


## `PUT` `/persons/<trn>/qtls`

Request body structure:
```json
{
  "qtsDate": ""
}
```

If no record exists with the specified TRN, a `404 Not Found` status code will be returned.

The `qtsDate` property must be formatted `yyyy-MM-dd` or be `null`. The date cannot be in the future [TBD].

If the `qtsDate` is not `null` then the QTLS status will be added or updated with the provided date and a `200 OK` status code will be returned.

If the `qtsDate` is `null` then any existing QTLS status on the teaching record will be removed and a `200 OK` status code will be returned.

Response body structure:
```json
{
  "trn": "",
  "qtsDate": ""
}
```

The response returned is identical to that of the [`GET` endpoint](#get-personstrnqtls).

In some cases the API cannot update the status automatically; in those cases a task is raised internally for review and the API will return a `202 Accepted` status code and an empty response body.
