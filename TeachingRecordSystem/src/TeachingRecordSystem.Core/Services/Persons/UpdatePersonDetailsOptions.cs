namespace TeachingRecordSystem.Core.Services.Persons;

public record UpdatePersonDetailsOptions(
    Guid PersonId,
    PersonDetailsToUpdate PersonDetails,
    Justification<PersonNameChangeReason>? NameChangeJustification,
    Justification<PersonDetailsChangeReason>? DetailsChangeJustification);
