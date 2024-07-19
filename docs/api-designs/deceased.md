# Deceased API

## `PUT` `/persons/deceased/<trn>`

Where `<trn>` is the person's TRN.

Request body structure:
```json
{
  "dateOfDeath": ""
}
```

The `dateOfDeath` property is mandatory and should be formatted `yyyy-MM-dd`.

If no record exists with the specified TRN, a `400 Bad Request` status code will be returned.

If the request is valid, a `204 No Content` status code will be returned.
