namespace TeachingRecordSystem.Core.Services.Persons;

public record UpdatePersonDetailsResult(
    PersonDetailsUpdatedEventChanges Changes,
    PersonDetails PersonDetails,
    PersonDetails OldPersonDetails);
