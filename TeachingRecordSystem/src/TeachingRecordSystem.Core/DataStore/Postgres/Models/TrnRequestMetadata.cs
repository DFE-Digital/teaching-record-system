namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TrnRequestMetadata
{
    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required bool? IdentityVerified { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? OneLoginUserSubject { get; init; }
    public required string[] Name { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public bool? PotentialDuplicate { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public int? Gender { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? AddressLine3 { get; set; }
    public string? City { get; set; }
    public string? Postcode { get; set; }
    public string? Country { get; set; }
}
