using Optional;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record UpdateContactQuery : ICrmQuery<bool>
{
    public required Guid ContactId { get; init; }
    public required Option<string> FirstName { get; init; }
    public required Option<string> MiddleName { get; init; }
    public required Option<string> LastName { get; init; }
    public Option<string> StatedFirstName { get; init; }
    public Option<string> StatedMiddleName { get; init; }
    public Option<string> StatedLastName { get; init; }
    public required Option<DateOnly> DateOfBirth { get; init; }
    public required Option<Contact_GenderCode> Gender { get; init; }
    public required Option<string?> EmailAddress { get; init; }
    public required Option<string?> NationalInsuranceNumber { get; init; }
}
