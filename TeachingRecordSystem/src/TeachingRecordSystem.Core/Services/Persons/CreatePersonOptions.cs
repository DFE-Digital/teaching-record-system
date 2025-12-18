namespace TeachingRecordSystem.Core.Services.Persons;

public abstract record CreatePersonOptions(
    string? Trn,
    PersonDetails PersonDetails,
    (Guid ApplicationUserId, string RequestId)? SourceTrnRequest,
    Justification<PersonCreateReason>? Justification);

public record CreatePersonViaTrnRequestOptions(
    PersonDetails PersonDetails,
    (Guid ApplicationUserId, string RequestId)? SourceTrnRequest)
    : CreatePersonOptions(null, PersonDetails, SourceTrnRequest, null);

public record CreatePersonViaTpsImportOptions(
    string? Trn,
    PersonDetails PersonDetails,
    (Guid ApplicationUserId, string RequestId)? SourceTrnRequest)
    : CreatePersonOptions(Trn, PersonDetails, SourceTrnRequest, null);

public record CreatePersonViaSupportUIOptions(
    PersonDetails PersonDetails,
    Justification<PersonCreateReason>? Justification)
    : CreatePersonOptions(null, PersonDetails, null, Justification);

