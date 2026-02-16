using Optional;

namespace TeachingRecordSystem.Core.Services.Persons;

public record MergePersonsOptions
{
    public required Guid DeactivatingPersonId { get; init; }
    public required Guid RetainedPersonId { get; init; }
    public required Option<string> FirstName { get; init; }
    public required Option<string> MiddleName { get; init; }
    public required Option<string> LastName { get; init; }
    public required Option<DateOnly?> DateOfBirth { get; init; }
    public required Option<EmailAddress?> EmailAddress { get; init; }
    public required Option<NationalInsuranceNumber?> NationalInsuranceNumber { get; init; }
    public required Option<Gender?> Gender { get; init; }
}
