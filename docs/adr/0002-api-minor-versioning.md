# 1. Record architecture decisions

Date: 2023-12-22

## Status

Draft

## Context

The TRS API is versioned so that we can make large, breaking changes without affecting existing consumers.
We use a prefix on each endpoint to indicate the version (e.g. `/v3/teachers/<trn>`).
However, each version is completely new and API consumers have historically been slow to update to the latest version.
In addition, with TRS evolving we want to update the API as the new data model is fleshed out to remove as many DQT references as we can.
We need a way to evolve the API in a more iterative way, including making breaking changes, that does not require a completely new API version.

## Decision

* Add minor versioning to the V3 API.
* Clients indicate the minor version they want using an `X-Api-Version` header.
* Each minor version should 'inherit' from the last.
* Maintain a `CHANGELOG` in this repository which records the changes made in each minor version.
* Every API schema change should create a new minor version.

## Consequences
API consumers will need to specify the minor version they want in a header.

Any additional functionality added to the API will be added to the most recent minor version only.

Repos Affected:
https://github.com/DFE-Digital/teaching-record-system
