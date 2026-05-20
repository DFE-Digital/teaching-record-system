namespace TeachingRecordSystem.Core.Services.Persons;

public record DeactivatePersonOptions(Guid PersonId, DateOnly? DateOfDeath);
