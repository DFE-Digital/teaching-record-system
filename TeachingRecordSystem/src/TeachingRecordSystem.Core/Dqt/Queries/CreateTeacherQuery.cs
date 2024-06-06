namespace TeachingRecordSystem.Core.Dqt.Queries;

public class CreateContactQuery : ICrmQuery<Guid>
{
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required string StatedFirstName { get; init; }
    public required string StatedMiddleName { get; init; }
    public required string StatedLastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required FindPotentialDuplicateContactsResult[] PotentialDuplicates { get; init; }
    public required string? Trn { get; init; }
}
