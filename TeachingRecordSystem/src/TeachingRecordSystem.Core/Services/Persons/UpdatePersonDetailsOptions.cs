using Optional;

namespace TeachingRecordSystem.Core.Services.Persons;

public record UpdatePersonDetailsOptions
{
    public required Guid PersonId { get; init; }
    public required bool CreatePreviousName { get; init; }
    public required Option<string> FirstName { get; init; }
    public required Option<string> MiddleName { get; init; }
    public required Option<string> LastName { get; init; }
    public required Option<DateOnly?> DateOfBirth { get; init; }
    public required Option<EmailAddress?> EmailAddress { get; init; }
    public required Option<NationalInsuranceNumber?> NationalInsuranceNumber { get; init; }
    public required Option<Gender?> Gender { get; init; }
}
