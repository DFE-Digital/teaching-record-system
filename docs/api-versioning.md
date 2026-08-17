# API versioning

This document describes how the TRS API is versioned and the rules that must be followed when changing it.

The decision to version the V3 API this way is recorded in [ADR 0002](adr/0002-api-minor-versioning.md).

## Overview

There are two levels of versioning:

- **Major versions** (`v1`, `v2`, `v3`) are part of the URL — e.g. `GET /v3/persons/<trn>`. A major version is an
  entirely separate API surface; consumers have historically been slow to migrate between them, so we no longer
  create new ones for routine changes.
- **Minor versions** apply to V3 only. They are date-stamped (`20240101`, `20250627`, `20260612`, …) and are selected
  per request with the `X-Api-Version` header:

  ```
  X-Api-Version: 20260612
  ```

The set of valid versions lives in
[`VersionRegistry`](../src/TeachingRecordSystem.Core/ApiSchema/VersionRegistry.cs). If the header is omitted the
request is served as `VersionRegistry.DefaultV3MinorVersion` (`20240101`); if it holds a value that isn't a declared
version, no endpoint matches and the request 404s.

Each minor version *inherits* from the one before it: an endpoint declared in `V20250425` continues to serve
requests for `20250627`, `20260120` and every later version until a later version redefines the same route and HTTP
method. This is done by
[`BackFillVersionedEndpointsConvention`](../src/TeachingRecordSystem.Api/Infrastructure/ApplicationModel/BackFillVersionedEndpointsConvention.cs),
which walks forward from each declared endpoint and attaches an action constraint listing every version it should
answer for. The consequence is that a new minor version only has to contain the things that actually changed.

Endpoints are bound to a version by their namespace, not by an attribute.
[`ApiVersionConvention`](../src/TeachingRecordSystem.Api/Infrastructure/ApplicationModel/ApiVersionConvention.cs)
derives the major version from the first namespace segment (`TeachingRecordSystem.Api.V3.…`) and the minor version
from the second (`…V3.V20260515.Controllers`). A controller in a namespace whose minor version isn't listed in
`VersionRegistry.AllV3MinorVersions` fails at startup.

Each version also gets its own OpenAPI document, named `v3_<minor version>` and served from
`/swagger/v3_<minor version>.json`.

## 1. Every schema change must be made in a new minor version

Published versions are immutable. Once a minor version has been released, the shape of its requests, responses and
webhook payloads must not change — consumers pin to a version precisely so that it stays still.

A change needs a new minor version if it alters what a consumer can send or what they receive. That includes:

- adding, renaming or removing a property on a request or response DTO;
- changing a property's type, nullability, or the members of an enum it uses;
- changing the meaning of an existing property, or the status codes an endpoint returns;
- moving or removing an endpoint;
- changing the payload of a webhook message (see [section 4](#4-webhook-message-schemas)).

A change does *not* need a new minor version if the observable schema and behaviour are unchanged — bug fixes that
bring behaviour in line with the documented contract, performance work, internal refactoring, and changes to code
under `V3/Operations` that all versions share.

### Adding a new minor version

Versions are named for the date the change is expected to be released, in `yyyyMMdd` form.

1. Add the constant to `VersionRegistry.V3MinorVersions` and add it to `AllV3MinorVersions`. The collection is kept
   in ascending order, with `VNext` last.
2. Create `src/TeachingRecordSystem.Api/V3/V<version>/` and add **only** the controllers that change. Copy the
   controller from the previous version, apply the change, and leave everything else alone — the backfill
   convention keeps the untouched endpoints serving the new version.
3. If a DTO's shape changes, add the new version of it under
   `src/TeachingRecordSystem.Core/ApiSchema/V3/V<version>/Dtos/` alongside a `Mappings.cs` in the corresponding API
   folder. Don't edit the previous version's DTO.
4. To remove an endpoint from this version onwards, declare a stub action for the same route and HTTP method
   marked `[RemovesFromApi]` — see
   [`V20240606/Controllers/TeacherController.cs`](../src/TeachingRecordSystem.Api/V3/V20240606/Controllers/TeacherController.cs).
   The stub stops the backfill and drops the endpoint from the API and its OpenAPI document.
5. Add integration tests under `tests/TeachingRecordSystem.Api.IntegrationTests/V3/V<version>/`. Every version
   folder carries tests for the operations it serves, so that a version's behaviour stays pinned even as later
   versions change.
6. Add a `CHANGELOG.md` entry (see below).

## 2. Changes are documented in `CHANGELOG.md`

[`CHANGELOG.md`](../CHANGELOG.md) at the root of the repository is the consumer-facing record of what changed
between versions, and is what the README points people at when they upgrade. It is not optional: a minor version
with no changelog entry is invisible to the people who have to migrate onto it.

- Entries are ordered newest-first, under a heading that is the version itself (`## 20260612`), with unreleased
  work under `## vNext`.
- Describe the change from the consumer's point of view — the endpoint, the property, the old and new names — not
  the implementation. Group by endpoint with `###` sub-headings when a version touches several.
- Every user-visible change in the version gets a line, including new endpoints, renamed or removed properties,
  changed status codes, and new or changed webhook messages.

## 3. The `vNext` staging version

`VNext` (the literal version string `Next`) is where changes are staged before they are given a date and released.
It behaves like a normal minor version — its own namespaces, its own test folder, its own entry in
`AllV3MinorVersions` — with one difference: it is only exposed where `AllowVNextEndpoints` is set.

| Environment | `AllowVNextEndpoints` |
| --- | --- |
| Local development | `true` |
| Tests | `true` |
| `dev` | `true` |
| `pre-production` | `true` |
| `production`, `pentest`, `tps-sandbox` | not set (`false`) |

Where it's disabled, `ApiVersionConvention` removes VNext controllers from the application model entirely and
`VersionRegistry.GetAllVersions` omits it, so there is no `v3_Next` OpenAPI document and requests carrying
`X-Api-Version: Next` 404.

This lets a schema change be merged, deployed and exercised against dev and pre-production before it is committed
to. Because `VNext` sorts last in `AllV3MinorVersions`, it also inherits from the most recent dated version in the
usual way.

### Releasing `vNext`

When the change is ready to go to production, promote it to a dated version:

1. Add the new dated constant to `VersionRegistry` (immediately before `VNext`).
2. Move the `VNext` folders — `V3/VNext/` in the API project and `ApiSchema/V3/VNext/` in Core — to `V<date>/`, and
   update the namespaces.
3. Move the tests to `tests/TeachingRecordSystem.Api.IntegrationTests/V3/V<date>/`.
4. Move the `## vNext` changelog entries under a `## <date>` heading.

[`df722ed03`](https://github.com/DFE-Digital/teaching-record-system/commit/df722ed03) ("Release
`person.deactivated` webhook message") is a small worked example.

Anything still in flight stays in `VNext`; only the parts being released move.

## 4. Webhook message schemas

Webhook messages are versioned with the same minor versions as the API, so that a consumer sees one consistent
schema across both.

- The payload types live in `src/TeachingRecordSystem.Core/ApiSchema/V3/V<version>/WebhookData/`, mirroring the
  layout of the versioned DTOs. Each implements
  [`IWebhookMessageData`](../src/TeachingRecordSystem.Core/ApiSchema/V3/IWebhookMessageData.cs), which supplies the
  CloudEvent type (`trn_request.completed`, `person.deactivated`, `alert.created`, …), and is paired with an
  [`IEventMapper<TEvent, TData>`](../src/TeachingRecordSystem.Core/ApiSchema/V3/IEventMapper.cs) that maps a domain
  event onto it. They reuse the versioned DTOs from the same version's `Dtos/` folder — for example
  [`V20260515.WebhookData.TrnRequestCompletedNotification`](../src/TeachingRecordSystem.Core/ApiSchema/V3/V20260515/WebhookData/TrnRequestCompletedNotification.cs)
  carries a `V20260515.Dtos.TrnRequestInfo`, the same type the `POST /v3/trn-requests` response uses at that
  version.
- [`EventMapperRegistry`](../src/TeachingRecordSystem.Core/Services/Webhooks/EventMapperRegistry.cs) discovers
  mappers by reflection and keys them on (event type, CloudEvent type, version) — the version being taken from the
  `V<version>` namespace segment. Nothing needs registering by hand.
- Each webhook endpoint is registered against a single API version (`WebhookEndpoint.ApiVersion`, set with the
  `webhook-endpoint` CLI command, which validates it against `AllV3MinorVersions`). When an event fires,
  [`WebhookMessageFactory`](../src/TeachingRecordSystem.Core/Services/Webhooks/WebhookMessageFactory.cs) walks
  backwards from the endpoint's version to find the most recent mapper at or before it. So an endpoint on
  `20260612` receiving `trn_request.completed` gets the `20260515` payload, because that's the latest version in
  which that message's schema changed — the same inheritance rule the HTTP endpoints follow.
- A message whose first mapper appears at a version *later* than the endpoint's is never delivered to that
  endpoint. Adding a new message type is therefore a schema change like any other: consumers have to move their
  endpoint's version forward to receive it.
- Delivered messages advertise their schema. `ce-dataschema` on the CloudEvent points at the OpenAPI document for
  the endpoint's version — `https://<host>/swagger/v3_<version>.json` — set by
  [`WebhookSender`](../src/TeachingRecordSystem.Core/Services/Webhooks/WebhookSender.cs).

Changing an existing message's payload follows the same rule as any other schema change: add the new payload type
under a new version's `WebhookData/` folder, leave the old one in place, and record it in the changelog. Endpoints
stay on their old version and keep receiving the old shape until they are moved forward.

See [the webhooks design doc](api-designs/webhooks.md) for the delivery mechanism, signing and retry behaviour.

The payload schemas are written into each version's OpenAPI document by
[`AddWebHookMessagesDocumentFilter`](../src/TeachingRecordSystem.Api/Infrastructure/OpenApi/AddWebHookMessagesDocumentFilter.cs),
which resolves them through `EventMapperRegistry` using the same rule as delivery. A payload type must therefore
not reuse a type name that the endpoint DTOs at that version already use — both are generated into one document
and schema ids are just the type name. Name types after the notification when there's any doubt, as
`PersonDeactivatedNotificationPersonInfo` does.
