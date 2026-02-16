using Optional;

namespace TeachingRecordSystem.Core.Services.Persons;

public record CreatePersonOptions
{
    public Option<string> Trn { get; init; }  // For TPS records
    public required (Guid ApplicationUserId, string RequestId)? SourceTrnRequest { get; init; }
    public required string FirstName { get; init; }
    public required string MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly? DateOfBirth { get; init; }
    public required EmailAddress? EmailAddress { get; init; }
    public required NationalInsuranceNumber? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
}
