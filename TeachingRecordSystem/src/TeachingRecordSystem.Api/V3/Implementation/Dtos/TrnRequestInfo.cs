namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record TrnRequestInfo
{
    public required string RequestId { get; init; }
    public required TrnRequestInfoPerson Person { get; init; }
    public required TrnRequestStatus Status { get; init; }
    public required string? Trn { get; init; }
}

public record TrnRequestInfoPerson
{
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
}
