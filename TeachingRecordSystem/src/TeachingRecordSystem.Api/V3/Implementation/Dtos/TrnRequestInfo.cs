namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record TrnRequestInfo
{
    public required string RequestId { get; init; }
#pragma warning disable TRS0001
    public required TrnRequestInfoPerson Person { get; init; }
#pragma warning restore TRS0001
    public required TrnRequestStatus Status { get; init; }
    public required string? Trn { get; init; }
    public required bool PotentialDuplicate { get; init; }
    public required string? AccessYourTeachingQualificationsLink { get; init; }
}

// The CreateTrnRequest API populates this with the data from the original request
// but the GetTrnRequest endpoint uses the resolved record.
// To remove the inconsistency newer API versions don't return this object at all.
[Obsolete("Maintained for older API versions", DiagnosticId = "TRS0001")]
public record TrnRequestInfoPerson
{
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
}
