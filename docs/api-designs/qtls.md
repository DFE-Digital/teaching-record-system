# Add/remove QTLS API

Draft specification v0.1.

There are two endpoints; one sets or removes the QTLS date for a person and another queries the existing QTLS status.
In all cases the person is identified by a TRN.

## `GET` `/persons/<trn>/qtls`

If no record exists with the specified TRN, a `404 Not Found` status code will be returned.

Response body structure:
```json
{
  "trn": "",
  "awardedDate": ""
}
```

The `awardedDate` property will be formatted `yyyy-MM-dd`. If the person does not have QTLS awarded then `awardedDate` will be `null`.


## `PUT` `/persons/<trn>/qtls`

Request body structure:
```json
{
  "awardedDate": ""
}
```

If no record exists with the specified TRN, a `404 Not Found` status code will be returned.

The `awardedDate` property must be formatted `yyyy-MM-dd` or be `null`. The date cannot be in the future [TBD].

If the `awardedDate` is not `null` then the QTLS status will be added or updated with the provided date.

If the `awardedDate` is `null` then any existing QTLS status on the teaching record will be removed.

Response body structure:
```json
{
  "trn": "",
  "awardedDate": ""
}
```

The response returned is identical to that of the [`GET` endpoint](#get-personstrnqtls).
