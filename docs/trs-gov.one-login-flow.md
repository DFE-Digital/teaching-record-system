# High level flow of signing in to a service that requires authorisation to the teaching record via GOV.UK One Login

The `Start` represents the point at which a user visits a service domain for example [Access your teaching qualifications](https://access-your-teaching-qualifications.education.gov.uk/qualifications/start).
They would be re-directed to [GOV.UK One Login](https://www.sign-in.service.gov.uk/) to sign in. The Teaching Record System will handle the OAuth flow between GOV.UK One Login and the calling service. As part of this flow it will provide matching and authorisation against the teaching record, only returning to the calling service when this authorisation succeeded. Details of both the One Login user and the teaching record will be passed to the calling service, where it can apply any service-specific rules, if required.

```mermaid
  flowchart TD
  Start[Start] --> OneLogin[Sign in with One Login]
  OneLogin --> Verified{Identity verified?}
  Verified -->|Yes| TrnKnown{TRN known for user?}
  TrnKnown -->|Yes| Done
  TrnKnown -->|No| Nino[Ask for NINO & lookup record]
  Nino --> FoundAfterNino{Teaching record found?}
  FoundAfterNino -->|Yes| RecordTrn[Record TRN for user]
  RecordTrn --> Done
  FoundAfterNino -->|No| Trn[Ask for TRN & lookup record]
  Trn --> FoundAfterTrn{Teaching record found?}
  FoundAfterTrn -->|Yes| RecordTrn
  FoundAfterTrn -->|No| LookupFailed[Support ticket & error page]
  Verified -->|No| VerificationFailed[Error page]
  Done[Done: redirect to calling service]
```


## TRN allocation

The flow below covers services where users may not yet have a TRN and teaching record but they require one (e.g. Register for an NPQ).

```mermaid
  flowchart TD
  Start[Start] --> OneLogin[Sign in with One Login]
  OneLogin --> Verified{Identity verified?}
  Verified -->|Yes| TrnKnown{TRN known for user?}
  TrnKnown -->|Yes| Done
  TrnKnown -->|No| Nino[Ask for NINO & lookup record]
  Nino --> FoundAfterNino{Teaching record found?}
  FoundAfterNino -->|Yes| RecordTrn[Record TRN for user]
  RecordTrn --> Done
  FoundAfterNino -->|No| DoYouHaveTrn{Do you have a TRN?}
  DoYouHaveTrn -->|Yes| Trn[Ask for TRN & lookup record]
  DoYouHaveTrn -->|No| CreateTrnRequest[Create TRN request with verified info from One Login]
  CreateTrnRequest --> CanAllocateImmediately{Can TRN be allocated immediately? *}
  CanAllocateImmediately -->|Yes| CreateTrn[Create new TRN and teaching record]
  CreateTrn -->RecordTrn
  CanAllocateImmediately -->|No| HoldingPage[Holding page]
  Trn --> FoundAfterTrn{Teaching record found?}
  FoundAfterTrn -->|Yes| RecordTrn
  FoundAfterTrn -->|No| LookupFailed[Support ticket & error page]
  Verified -->|No| VerificationFailed[Error page]
  Done[Done: redirect to calling service]
```

\* If the TRN request cannot be allocated immediately (e.g. it requires manual intervention for resolving a potential duplicate), the user is shown a holding page.
When the TRN is eventually allocated it is associated with the One Login user ID and the user is sent an email with the TRN and a link to sign in.
When they return, they need to sign in with One Login only; from there they can proceed to the calling service.
