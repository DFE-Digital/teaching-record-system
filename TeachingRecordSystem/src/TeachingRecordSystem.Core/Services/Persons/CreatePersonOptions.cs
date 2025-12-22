namespace TeachingRecordSystem.Core.Services.Persons;

public record CreatePersonOptions(
    PersonDetails PersonDetails,
    Justification<PersonCreateReason> Justification);
