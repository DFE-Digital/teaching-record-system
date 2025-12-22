namespace TeachingRecordSystem.Core.Services.Persons;

public record ReactivatePersonOptions(
    Guid PersonId,
    Justification<PersonReactivateReason> Justification);
