# Events emitted per `ProcessType`

This document lists the events that can be created for each [`ProcessType`](../src/TeachingRecordSystem.Core/Models/ProcessType.cs).

## How to read this document

Every change that goes through the event system runs inside a `ProcessContext` created with a
single `ProcessType` (see [`EventPublisher`](../src/TeachingRecordSystem.Core/EventPublisher.cs)).
Any events published while that context is open (via `PublishEventAsync` /
`PublishSingleEventAsync`) are attached to the resulting `Process`. These events are the `IEvent`
types in [`Events/`](../src/TeachingRecordSystem.Core/Events) and are what the tables below document.

The **Emitted** column uses:

- **Always** — emitted on every successful run of the process.
- **Sometimes** — only under the condition described in **Scenario**.

> An "update"-style operation returns early (and emits nothing) when the submitted values are
> identical to the stored record. These are marked **Sometimes** for correctness even though the UI
> that drives them normally guarantees a change.

---

## Persons

### `PersonCreating` (24)
Support UI *Add person*.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonCreatedEvent` | Always | — |

### `PersonDetailsUpdating` (25)
API `SetPii`; Support UI *Edit details*.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonDetailsUpdatedEvent` | Sometimes | Only when at least one detail actually changes. |

### `PersonDeactivating` (26)
Support UI *Set status → deactivate*.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonDeactivatedEvent` | Always | — |

### `PersonReactivating` (27)
Support UI *Set status → reactivate*.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonReactivatedEvent` | Always | — |

### `PersonDeceased` (42)
API `SetDeceased`.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonDeactivatedEvent` | Always | — |

### `PersonMerging` (29)
Support UI *Merge person*.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonDeactivatedEvent` | Always | The secondary record is deactivated and linked to the retained record. |
| `PersonDetailsUpdatedEvent` | Sometimes | The retained record's attributes are changed to values taken from the secondary record. |
| `OneLoginUserUpdatedEvent` | Sometimes | The deactivated record had linked One Login users, which are re-pointed to the retained record. |

### `TeacherPensionsRecordImporting` (28)
[`CapitaImportJob`](../src/TeachingRecordSystem.Core/Jobs/CapitaImportJob.cs) (Teachers' Pensions import).

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonCreatedEvent` | Sometimes | The imported TPS record does not already exist in TRS, so a new person is created. |

---

## TRN requests

### `TrnRequestCreating` (14)
API `CreateTrnRequest`.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `TrnRequestCreatedEvent` | Always | — |
| `PersonCreatedEvent` | Sometimes | The request is auto-resolved and no existing record matches, so a new record is created. |
| `SupportTaskCreatedEvent` | Sometimes | Auto-resolution finds potential duplicates (a `TrnRequest` task) or the matched record needs further checks (a `TrnRequestManualChecksNeeded` task). |
| `OneLoginUserUpdatedEvent` | Sometimes | The request carries a verified One Login user that is connected to the resolved person. |

### `TrnRequestActivating` (43)
API `ActivateTrnRequest`.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `TrnRequestUpdatedEvent` | Always | The dormant request is moved to pending (and resolved where possible). |
| `PersonCreatedEvent` | Sometimes | Activation triggers resolution with no match, so a new record is created. |
| `SupportTaskCreatedEvent` | Sometimes | Activation triggers resolution that finds potential duplicates or needs further checks. |
| `OneLoginUserUpdatedEvent` | Sometimes | A verified One Login user on the request is connected to the resolved person. |

### `TrnRequestResolving` (16)
Support UI *Resolve TRN request*.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `TrnRequestUpdatedEvent` | Always | — |
| `SupportTaskUpdatedEvent` | Always | The support task is closed. |
| `PersonCreatedEvent` | Sometimes | The support user chooses to create a new record. |
| `PersonDetailsUpdatedEvent` | Sometimes | The support user merges into an existing record and updates its attributes. |
| `SupportTaskCreatedEvent` | Sometimes | The matched record needs further checks (a `TrnRequestManualChecksNeeded` task). |
| `OneLoginUserUpdatedEvent` | Sometimes | A verified One Login user on the request is connected to the resolved person. |

### `TrnRequestManualChecksNeededTaskCompleting` (20)
Support UI *TRN request manual checks needed → confirm*.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `TrnRequestUpdatedEvent` | Always | The resolved request is completed. |
| `SupportTaskUpdatedEvent` | Always | The support task is closed. |

### `TrnRequestResetting` (9)
CLI `reset-trn-request`.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskCreatedEvent` | Always | A new `TrnRequest` support task is created. |
| `TrnRequestUpdatedEvent` | Always | The request is reset to pending. |

---

## Teachers' Pensions duplicates

### `TeacherPensionsDuplicateSupportTaskResolvingWithMerge` (21)
Support UI *Teachers' Pensions duplicate → merge*.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonDeactivatedEvent` | Always | The imported record is deactivated via merge into the existing record. |
| `TrnRequestUpdatedEvent` | Always | The request is resolved to the retained record. |
| `SupportTaskUpdatedEvent` | Always | The support task is closed. |
| `PersonDetailsUpdatedEvent` | Sometimes | The retained record's attributes are updated from the imported record. |
| `OneLoginUserUpdatedEvent` | Sometimes | Linked One Login users are re-pointed to the retained record, or a verified One Login user on the request is connected. |
| `SupportTaskCreatedEvent` | Sometimes | The retained record needs further checks (a `TrnRequestManualChecksNeeded` task). |

### `TeacherPensionsDuplicateSupportTaskResolvingWithoutMerge` (22)
Support UI *Teachers' Pensions duplicate → keep records separate*.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `TrnRequestUpdatedEvent` | Always | The request is resolved to the kept record. |
| `SupportTaskUpdatedEvent` | Always | The support task is closed. |
| `OneLoginUserUpdatedEvent` | Sometimes | A verified One Login user on the request is connected to the resolved person. |
| `SupportTaskCreatedEvent` | Sometimes | The kept record needs further checks (a `TrnRequestManualChecksNeeded` task). |

---

## One Login user matching

The Support UI resolution pages choose the process type from the task type, and the events depend on
the outcome the support user selects.

### `OneLoginUserIdVerificationSupportTaskCompleting` (23)
Resolve an ID-verification task.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskUpdatedEvent` | Always | The support task is closed. |
| `OneLoginUserUpdatedEvent` | Sometimes | Any outcome that verifies the user (i.e. all outcomes except "not verified"). |
| `EmailSentEvent` | Sometimes | An outcome email is sent to the user (not-verified, record-not-found or record-matched; or not-connected under a deferred matching policy). |

### `OneLoginUserRecordMatchingSupportTaskCompleting` (34)
Resolve a record-matching task.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskUpdatedEvent` | Always | The support task is closed. |
| `OneLoginUserUpdatedEvent` | Sometimes | The "connected" outcome matches the One Login user to a record. |
| `TrnRequestUpdatedEvent` | Sometimes | The task is linked to a TRN request that gets resolved. |
| `PersonCreatedEvent` | Sometimes | A linked TRN request is auto-resolved with no match, so a new record is created. |
| `SupportTaskCreatedEvent` | Sometimes | A linked TRN request has potential duplicates or needs further checks. |
| `EmailSentEvent` | Sometimes | An outcome email is sent to the user (record-matched, record-not-found or not-connected, depending on outcome and matching policy). |

### `OneLoginUserIdVerificationSupportTaskSaving` (32)
### `OneLoginUserRecordMatchingSupportTaskSaving` (33)
*Save and return* on the matching page (partial progress).

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskUpdatedEvent` | Always | The task is set to *in progress* and the journey state is saved. |

---

## One Login connect / disconnect (Support UI)

### `PersonOneLoginUserConnecting` (37)
### `OneLoginUserPersonConnecting` (44)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `OneLoginUserUpdatedEvent` | Always | The One Login user is matched (and verified, if it was not already). |

### `PersonOneLoginUserDisconnecting` (38)
### `OneLoginUserPersonDisconnecting` (45)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `OneLoginUserUpdatedEvent` | Always | The One Login user is unmatched (and unverified, unless the user chose to stay verified). |

---

## Teacher sign-in (Authorize access)

### `TeacherSigningIn` (36)
[`OAuth2Controller`](../src/TeachingRecordSystem.AuthorizeAccess/Controllers/OAuth2Controller.cs) /
[`SignInJourneyCoordinator`](../src/TeachingRecordSystem.AuthorizeAccess/SignInJourneyCoordinator.cs).
A single process spans the whole sign-in journey.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `AuthorizeAccessRequestStartedEvent` | Always | The sign-in journey is started. |
| `OneLoginUserSignedInEvent` | Always | The user authenticates with One Login. |
| `OneLoginUserCreatedEvent` | Sometimes | The first time this One Login subject signs in. |
| `OneLoginUserUpdatedEvent` | Sometimes | A returning user's email/verification/match state changes (email change, identity verification, or interactive matching). |
| `TrnRequestCreatedEvent` | Sometimes | Under a deferred matching policy, a dormant TRN request is created. |
| `SupportTaskCreatedEvent` | Sometimes | The user submits a record-matching or ID-verification support request. |
| `AuthorizeAccessRequestCompletedEvent` | Sometimes | The sign-in completes with the user authenticated and matched to a record. |

---

## Notifications

### `NotifyingTrnRecipient` (10)
[`SendTrnRecipientEmailJob`](../src/TeachingRecordSystem.Core/Jobs/SendTrnRecipientEmailJob.cs).

| Event | Emitted | Scenario |
| --- | --- | --- |
| `EmailSentEvent` | Always | The TRN recipient email is sent. |

---

## Change requests (created via the API)

### `ChangeOfNameRequestCreating` (13)
API `CreateNameChangeRequest`.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskCreatedEvent` | Always | A `ChangeNameRequest` support task is created. |
| `EmailSentEvent` | Sometimes | A confirmation email is sent when an email address is available. |

### `ChangeOfDateOfBirthRequestCreating` (12)
API `CreateDateOfBirthChangeRequest`.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskCreatedEvent` | Always | A `ChangeDateOfBirthRequest` support task is created. |
| `EmailSentEvent` | Sometimes | A confirmation email is sent when an email address is available. |

---

## Change requests (resolved in the Support UI)

### `ChangeOfNameRequestApproving` (30)
Support UI *Change requests → accept* (name change).

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskUpdatedEvent` | Always | The support task is closed as approved. |
| `PersonDetailsUpdatedEvent` | Sometimes | Only when the approved name differs from the current record. |

*The approval email is sent without a process context, so it produces no `EmailSentEvent`.*

### `ChangeOfDateOfBirthRequestApproving` (31)
Support UI *Change requests → accept* (date-of-birth change).

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskUpdatedEvent` | Always | The support task is closed as approved. |
| `PersonDetailsUpdatedEvent` | Sometimes | Only when the approved date of birth differs from the current record. |

*The approval email is sent without a process context, so it produces no `EmailSentEvent`.*

### `ChangeOfNameRequestRejecting` (57)
Support UI *Change requests → reject* (name change).

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskUpdatedEvent` | Always | The support task is closed as rejected. |

*The rejection email is sent without a process context, so it produces no `EmailSentEvent`.*

### `ChangeOfDateOfBirthRequestRejecting` (58)
Support UI *Change requests → reject* (date-of-birth change).

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskUpdatedEvent` | Always | The support task is closed as rejected. |

*The rejection email is sent without a process context, so it produces no `EmailSentEvent`.*

### `ChangeOfNameRequestCancelling` (59)
Support UI *Change requests → reject* with reason *Change no longer required* (name change).

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskUpdatedEvent` | Always | The support task is closed as cancelled. |

### `ChangeOfDateOfBirthRequestCancelling` (60)
Support UI *Change requests → reject* with reason *Change no longer required* (date-of-birth change).

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskUpdatedEvent` | Always | The support task is closed as cancelled. |

---

## Support tasks (generic)

### `SupportTaskDeleting` (8)
CLI `delete-support-task`.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskDeletedEvent` | Always | — |

### `SupportTaskNoteCreating` (61)
Support UI *Add note* (support task).

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskNoteCreatedEvent` | Always | — |

### `SupportTaskAllocating` (62)
Support UI *Allocate* (support task) — sets the status and assigned user.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskUpdatedEvent` | Always | — |

---

## Notes

### `NoteCreating` (11)
Support UI *Add note*.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `NoteCreatedEvent` | Always | — |

---

## Alerts

### `AlertCreating` (39)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `AlertCreatedEvent` | Always | — |

### `AlertUpdating` (40)
Edit / close / reopen alert.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `AlertUpdatedEvent` | Sometimes | Only when a field (details, link, start/end date) actually changes. |

### `AlertDeleting` (41)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `AlertDeletedEvent` | Always | — |

---

## Mandatory qualifications

### `MandatoryQualificationCreating` (54)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `MandatoryQualificationCreatedEvent` | Always | — |

### `MandatoryQualificationUpdating` (55)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `MandatoryQualificationUpdatedEvent` | Sometimes | Only when a field (provider, specialism, status, start/end date) actually changes. |

### `MandatoryQualificationDeleting` (56)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `MandatoryQualificationDeletedEvent` | Always | — |

---

## Users, application users and API keys

### `UserAdding` (46)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `UserAddedEvent` | Always | — |

### `UserUpdating` (47)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `UserUpdatedEvent` | Sometimes | Only when the name or roles change. |

### `UserActivating` (48)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `UserActivatedEvent` | Always | — |

### `UserDeactivating` (49)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `UserDeactivatedEvent` | Always | — |

### `ApplicationUserCreating` (50)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `ApplicationUserCreatedEvent` | Always | — |

### `ApplicationUserUpdating` (51)
Support UI *Edit application user*; CLI `app-content`.

| Event | Emitted | Scenario |
| --- | --- | --- |
| `ApplicationUserUpdatedEvent` | Sometimes | Only when a field actually changes. |

### `ApiKeyCreating` (52)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `ApiKeyCreatedEvent` | Always | — |

### `ApiKeyUpdating` (53)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `ApiKeyUpdatedEvent` | Sometimes | Only when the expiry date changes. |

---

## NPQ TRN requests (legacy)

### `NpqTrnRequestTaskCreating` (15)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskCreatedEvent` | Always | — |
| `TrnRequestCreatedEvent` | Always | — |

### `NpqTrnRequestApproving` (18)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskUpdatedEvent` | Always | — |
| `TrnRequestUpdatedEvent` | Always | — |
| `PersonCreatedEvent` | Sometimes | The support user chooses to create a new record. |
| `PersonDetailsUpdatedEvent` | Sometimes | The support user merges into an existing record and updates its attributes. |
| `EmailSentEvent` | Sometimes | A 'TRN Generated for NPQ' email was sent to the person. |

### `NpqTrnRequestRejecting` (19)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `SupportTaskUpdatedEvent` | Always | — |
| `TrnRequestUpdatedEvent` | Always | — |

---

## Historical / migration-only

These "…InDqt" / "…FromDqt" process types are not produced by current application code. They were
assigned during data migration to wrap pre-existing DQT-era records into the process model, and are
only used for display grouping (e.g. in [`ChangeHistoryService`](../src/TeachingRecordSystem.SupportUi/Services/ChangeHistory/ChangeHistoryService.cs)).

### `PersonCreatingInDqt` (2)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonCreatedEvent` | Always | — |

### `PersonImportingIntoDqt` (3)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonImportedIntoDqtEvent` | Always | — |

### `PersonUpdatingInDqt` (4) |

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonUpdatedInDqtEvent` | Always | — |

### `PersonDeactivatingInDqt` (5)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonDeactivatedEvent` | Always | — |

### `PersonReactivatingInDqt` (6)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonReactivatedEvent` | Always | — |

### `PersonMergingInDqt` (7)

| Event | Emitted | Scenario |
| --- | --- | --- |
| `PersonDeactivatedEvent` | Always | — |
| `PersonDeactivatedEvent` | Sometimes | The retained record's attributes are changed to values taken from the secondary record. |
