namespace TeachingRecordSystem.Core.Services.Persons;

public record MergePersonsOptions(Guid DeactivatingPersonId, Guid RetainedPersonId);

public record MergePersons2Options(
    Guid DeactivatingPersonId,
    Guid RetainedPersonId,
    PersonDetails PersonDetails,
    File? Evidence,
    string? Comments);
