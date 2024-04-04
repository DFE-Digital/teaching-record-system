# Webhooks

TRS uses [webhooks](https://en.wikipedia.org/wiki/Webhook) to push notifications to other services when interesting things happen.

Webhooks are sent as JSON and follow the guidelines in [Standard Webhooks](https://www.standardwebhooks.com/).

```json
{
  "type": "",
  "timestamp": "",
  "apiVersion": "",
  "data": {
    //...
  }
}
```

`type` is a string identifying the type of event that generated the webhook. See [message types](#message-types).

`timestamp` is a UTC ISO 8601 timestamp describing when the webhook was generated.

`apiVersion` is the schema version used to format the `data` and aligns with the `X-Api-Version` header values used when calling the API.

`data` is a type-specific object with the details of the event. Each `type` has its own message schema.


## Receiving webhooks

You need a publicly-accessible HTTPS endpoint that accepts JSON using the POST method. Ask one of the TRS developers to configure your endpoint.
You will also need to specify the V3 API minor version you want to receive messages with; see [the README.md](../../README.md) for more information on versions.
You will be given a public key with which you can [verify the webhook](#verifying-the-webhook).

Your endpoint should return a success status code (200-299) when the webhook has been processed successfully.
If any other status code is returned, or the endpoint takes longer than 30 seconds to respond, the message will be retried later. The retry intervals are:
- 5 seconds,
- 5 minutes,
- 30 minutes,
- 2 hours,
- 5 hours,
- 10 hours,
- 14 hours,
- 20 hours,
- 24 hours.

If after the final retry the message was still not delivered successfully no further attempts will be made to deliver that message.


## Verifying the webhook

Follow [the spec](https://github.com/standard-webhooks/standard-webhooks/blob/main/spec/standard-webhooks.md#verifying-signatures) for verifying the webhook's signature.
We use asymmetric signatures; you will be given the public key to use for verifying webhooks when your endpoint is configured.

The `ping` message can be used to aid verification.


## Message types

Reference the API Swagger document for the message schemas for the API version your webhook is registered to use
e.g. https://preprod.teacher-qualifications-api.education.gov.uk/swagger/v3_20240307.json

| Message `type` | Swagger schema name | Description |
| - | - | - |
| `ping` | `PingNotification` | Used for verifying that a webhook endpoint is reachable and can successfully process messages. These messages are sent manually by a TRS developer. |
| `alert.created` | `AlertCreatedNotification` | Generated whenever a new alert is added to a `person`. |
| `alert.updated` | `AlertUpdatedNotification` | Generated whenever an existing alert updated for a `person`. |
| `alert.deleted` | `AlertDeletedNotification` | Generated whenever an alert is deleted for a `person`. |
