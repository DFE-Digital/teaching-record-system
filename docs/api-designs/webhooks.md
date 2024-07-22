# Webhooks

TRS uses [webhooks](https://en.wikipedia.org/wiki/Webhook) to push notifications to other services when interesting things happen.

Webhooks are sent as [CloudEvents](https://cloudevents.io/) formatted with JSON.
Messages can be uniquely identified by their type and ID (the `ce-type` and `ce-id` headers, respectively).

Each message is signed with an [HTTP Message Signature](https://www.rfc-editor.org/rfc/rfc9421.html) with
[ECDSA Using Curve P-384 DSS and SHA-384](https://www.rfc-editor.org/rfc/rfc9421.html#section-3.3.5).

An example message is shown below:
```
POST /trs-webhooks HTTP/1.1
Host: localhost:8080
signature-input: whsig=("@target-uri" "content-digest" "content-length" "ce-id" "ce-type" "ce-time");created=1721657853;expires=1721658153;alg="ecdsa-p384-sha384";keyid="key123"
signature: whsig=:umon9i06iXAbphdsz4julfGHvY8H17j81IdivYnEv7OeGXIuYroXhr9lX0wipzEDEFv9bCaPrbBVLhoOC4hRWzVtOO4qOAHJNWsQirbPC/MjYSQcvlCztY0LJvXVydWq:
Content-Type: application/json
ce-specversion: 1.0
ce-id: 34921f5b-e623-401e-87ea-5a754dd262c3
ce-source: https://preprod.teacher-qualifications-api.education.gov.uk/
ce-type: alert.created
ce-dataschema: https://preprod.teacher-qualifications-api.education.gov.uk/swagger/v3_20240606.json
ce-time: 2024-07-22T14:17:33.7685924Z
content-digest: SHA-256=BKUBa2HuBCsCeb29BexPok4WhWLwqNcqrIwCfv1YaA0=
Content-Length: 433

{
  "trn": "1234567",
  "alert": {
    "alertId": "32c934c8-3aa4-4edc-aff2-f46e72385bb1",
    "alertType": {
      "alertTypeId": "17440175-d2bc-488e-ad4b-9975a1d16cc1",
      "alertCategory": {
        "alertCategoryId": "47d5fd72-a8cc-4f4f-b78d-63f65bdcc4ef",
        "name": "Prohibitions"
      },
      "name": "Prohibited by the Secretary of State"
    },
    "startDate": "2024-07-22",
    "endDate": null
  }
}
```


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

Follow [the spec](https://www.rfc-editor.org/rfc/rfc9421.html#name-verifying-a-signature) for verifying the webhook's signature.
You will be given a public key to use for verifying webhooks when your endpoint is configured.

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
