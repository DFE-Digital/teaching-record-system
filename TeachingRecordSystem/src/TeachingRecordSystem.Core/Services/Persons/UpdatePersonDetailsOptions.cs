namespace TeachingRecordSystem.Core.Services.Persons;

public record UpdatePersonDetailsOptions(
    Guid PersonId,
    PersonDetails PersonDetails,
    Justification<PersonNameChangeReason>? NameChangeJustification,
    Justification<PersonDetailsChangeReason>? DetailsChangeJustification);
