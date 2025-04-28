namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TrnRequestMetadata
{
    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required bool? IdentityVerified { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? OneLoginUserSubject { get; init; }
    public required string? FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string? LastName { get; init; }
    public string? PreviousFirstName { get; init; }
    public string? PreviousLastName { get; init; }
    public required string[] Name { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public bool? PotentialDuplicate { get; init; }
    public string? NationalInsuranceNumber { get; init; }
    public int? Gender { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? AddressLine3 { get; init; }
    public string? City { get; init; }
    public string? Postcode { get; init; }
    public string? Country { get; init; }
    public string? TrnToken { get; set; }
    public Guid? ResolvedPersonId { get; set; }
}
