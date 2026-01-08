namespace TeachingRecordSystem.Core.Services.Persons;

public record DeactivatePersonViaMergeOptions(Guid DeactivatingPersonId, Guid RetainedPersonId);
