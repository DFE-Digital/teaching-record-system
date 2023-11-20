# Webhooks

TRS uses [webhooks](https://en.wikipedia.org/wiki/Webhook) to push notifications to other services when interesting things happen.

Notifications are sent as JSON and have a common outer schema called the envelope.

```json
{
  "notificationId": "",
  "timeUtc": "",
  "messageType": "",
  "message": {
    //...
  }
}
```

`notificationId` is a unique identifier GUID for this notification. If a notification message is sent multiple times (e.g. when retrying after receiving an error code) this ID will remain consistent.

`timeUtc` is a UTC ISO 8601 timestamp describing when the notification was generated.

`messageType` is a string identifying the type of event that generated the notification. See [message types](#message-types).

`message` is a type-specific object with the details of the notification. Each `messageType` has its own message schema.


## Message types

### `Ping`

`Ping` is generated manually for verifying that a webhook endpoint is reachable and can successfully process messages:

```json
{
  "pingId": ""
}
```

### `AlertCreated`

`AlertCreated` is generated whenever a new alert is added to a `person`:

```json
{
  "trn": "",
  "alert": {
    "alertId": "",
    "alertType": {
      "alertTypeId": "",
      "description": "",
    },
    "alertCategory": {
      "alertCategoryId": "",
      "description": ""
    },
    "startDate": "",
    "endDate": ""
  }
}
```

### `AlertUpdated`

`AlertUpdated` is generated whenever an existing alert updated for a `person`:

```json
{
  "trn": "",
  "alert": {
    "alertId": "",
    "alertType": {
      "alertTypeId": "",
      "description": "",
    },
    "alertCategory": {
      "alertCategoryId": "",
      "description": ""
    },
    "startDate": "",
    "endDate": ""
  }
}
```

### `AlertDeleted`

`AlertDeleted` is generated whenever an alert is deleted for a `person`:

```json
{
  "trn": "",
  "alert": {
    "alertId": "",
    "alertType": {
      "alertTypeId": "",
      "description": "",
    },
    "alertCategory": {
      "alertCategoryId": "",
      "description": ""
    },
    "startDate": "",
    "endDate": ""
  }
}
```


## Receiving webhooks

You need a publicly-accessible HTTPS endpoint that accepts JSON using the POST method. Ask one of the TRS developers to configure your endpoint.
When the endpoint is configured you will receive a secret; this can be used to [verify the webhook's payload](#verifying-the-webhook).

Your endpoint should return a success status code (200-299) when the webhook has been processed successfully.
If an error code is returned, or the endpoint takes longer than 30 seconds to respond, the message will be retried later. The retry intervals are:
- 30 seconds,
- 2 minutes,
- 10 minutes,
- 1 hour,
- 2 hours,
 8 hours.

If after the final retry the message was still not delivered successfully no further attempts will be made to deliver that message.


## Verifying the webhook

When your endpoint receives a message it should verify that it has been sent by TRS.
Each HTTP request includes a header - `X-Hub-Signature-256`. This is an HMAC hex digest of the request body generated using the SHA-256 algorithm using the secret above as the key.
To verify, recalculate this signature and compare it to the header; if the values do not match the message should be disgarded.

The [Ping](#ping) message can be used to aid verification.
