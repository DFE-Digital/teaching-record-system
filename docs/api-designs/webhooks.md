# Webhooks

TRS uses [webhooks](https://en.wikipedia.org/wiki/Webhook) to push notifications to other services when interesting things happen.

Webhooks are sent as [CloudEvents](https://cloudevents.io/) formatted with JSON.
Messages can be uniquely identified by their type and ID (the `ce-type` and `ce-id` headers, respectively).

Each message is signed with an [HTTP Message Signature](https://www.rfc-editor.org/rfc/rfc9421.html) with
[ECDSA Using Curve P-384 DSS and SHA-384](https://www.rfc-editor.org/rfc/rfc9421.html#section-3.3.5).

An example message is shown below:
```
POST /trs-webhooks HTTP/1.1
Host: example-host.com
User-Agent: TeachingRecordSystem
signature-input: sig1=("@target-uri" "content-digest" "content-length" "ce-id" "ce-type" "ce-time");created=1721657853;expires=1721658153;nonce="0c8b2a1f4d7e4a9cb3f6e2d5a8c1b4e7";alg="ecdsa-p384-sha384";keyid="key123";tag="trs-webhooks"
signature: sig1=:umon9i06iXAbphdsz4julfGHvY8H17j81IdivYnEv7OeGXIuYroXhr9lX0wipzEDEFv9bCaPrbBVLhoOC4hRWzVtOO4qOAHJNWsQirbPC/MjYSQcvlCztY0LJvXVydWq:
Content-Type: application/json; charset=utf-8
ce-specversion: 1.0
ce-id: 34921f5b-e623-401e-87ea-5a754dd262c3
ce-source: https://preprod.teacher-qualifications-api.education.gov.uk/
ce-type: alert.created
ce-dataschema: https://preprod.teacher-qualifications-api.education.gov.uk/swagger/v3_20240606.json
ce-time: 2024-07-22T14:17:33.7685924Z
content-digest: sha-256=:lsafmD8Uf0oYdoWkn2U1+Eba3DKPzzrJ3XgZsgPCpTI=:
Content-Length: 326

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

The body is shown formatted for readability; it is sent without any whitespace, and it is that form that `content-digest` and
`content-length` are computed over.


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

See [the spec](https://www.rfc-editor.org/rfc/rfc9421.html#name-verifying-a-signature) for the details of how to verify the webhook's signature.

Signatures are made up of `content-digest`, `content-length`, `ce-id`, `ce-type`, and `ce-time` HTTP header components and the `target-uri` derived component.

Each environment publishes the certificates to use for verification at `/webhook-jwks` (see [README.md](../../README.md#Environments) for the environment-specific base URL).
Each HTTP message signature contains the ID of the key that was used to sign the request.
Find the corresponding keys in the `/webhook-jwks` (identified by the `kid` parameter) and use this to verify the signature.

Note that certificates are rotated from time-to-time so these should not be hard-coded anywhere or cached for too long.


## Testing verification with the `ping` message

The `ping` message lets you test your signature verification without waiting for a real event to happen in TRS.
A ping is created, signed and delivered by exactly the same code as every other message, so an endpoint that successfully
verifies a ping will successfully verify real messages too.

Ask a TRS developer to send a ping to your endpoint. It arrives with a `ce-type` of `ping` and a body containing a single,
randomly-generated identifier:
```
POST /trs-webhooks HTTP/1.1
Host: example-host.com
User-Agent: TeachingRecordSystem
signature-input: sig1=("@target-uri" "content-digest" "content-length" "ce-id" "ce-type" "ce-time");created=1721657853;expires=1721658153;nonce="3632fab03b0645388697c3601ab5fbc4";alg="ecdsa-p384-sha384";keyid="key123";tag="trs-webhooks"
signature: sig1=:xwaDnbhoHIfXMKVHvLE/e2+AlqdSrZKEjoHcimlqM8H9isRgNNybntPcWW4AA4HzhXOoJQDmm4fYhXOroy/rbh7LkApTOJjjSjMPrx+GnR/Hn+svJMRcHaUKRQMozD3p:
Content-Type: application/json; charset=utf-8
ce-specversion: 1.0
ce-id: 6ba0dd1c-2b2b-4a2c-96a4-31a30cf3b6a2
ce-source: https://preprod.teacher-qualifications-api.education.gov.uk/
ce-type: ping
ce-dataschema: https://preprod.teacher-qualifications-api.education.gov.uk/swagger/v3_20240606.json
ce-time: 2024-07-22T14:17:33.7685924Z
content-digest: sha-256=:DWkhKXy6DE3LJ1Ts/HGyNtgR/uBg980WaN/i+oGGL+M=:
Content-Length: 49

{"pingId":"b1c5f1f0-6f7e-4f2e-9a0f-0e2f2a6d1e3c"}
```

Verify the signature exactly as you would for a real message and return a success status code only if verification succeeded.
The `pingId` is a fresh value for each ping and is worth logging, so you can tell one test delivery from another; there is
nothing else you need to do with the body.

If verification fails, return a non-success status code. The failure is recorded against the message in TRS, so a TRS developer
can tell you whether the message was rejected as well as what was sent.

A few things worth knowing when you're testing:
- The ping is sent to your endpoint regardless of which message types it is registered for, so you don't need to subscribe
  to `ping`. Your endpoint does need to be enabled.
- Pings are queued in the same way as real messages and delivered by a background service that runs every minute, so a ping can
  take up to a minute to arrive.
- A ping that doesn't get a success status code is retried on the [same schedule as a real message](#receiving-webhooks), so if
  you're deliberately rejecting messages while testing, expect that ping to keep being redelivered for the next few days.
- Each delivery attempt is signed afresh, so the `signature` and `signature-input` headers differ between attempts. The signed
  content — the body, `ce-id` and `ce-time` — stays the same, so `ce-id` still identifies a redelivery of the same message.

### Sending a ping (TRS developers)

Find the endpoint to ping, then send it a ping message:
```shell
> just cli webhook-endpoint list
> just cli webhook-endpoint ping --id <webhook-endpoint-id>
```


## Message types

Reference the API Swagger document for the message schemas for the API version your webhook is registered to use
e.g. https://preprod.teacher-qualifications-api.education.gov.uk/swagger/v3_20240307.json

| Message `type` | Swagger schema name | Description |
| - | - | - |
| `ping` | `PingNotification` | Used for verifying that a webhook endpoint is reachable and can successfully process messages. These messages are sent manually by a TRS developer. |
| `alert.created` | `AlertCreatedNotification` | Generated whenever a new alert is added to a `person`. |
| `alert.updated` | `AlertUpdatedNotification` | Generated whenever an existing alert updated for a `person`. |
| `alert.deleted` | `AlertDeletedNotification` | Generated whenever an alert is deleted for a `person`. |
