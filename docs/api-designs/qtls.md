# Add/remove QTLS API

Draft specification v0.2.

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

If the `qtsDate` is not `null` then the QTLS status will be added or updated with the provided date.

If the `qtsDate` is `null` then any existing QTLS status on the teaching record will be removed.

Response body structure:
```json
{
  "trn": "",
  "qtsDate": ""
}
```

The response returned is identical to that of the [`GET` endpoint](#get-personstrnqtls).
