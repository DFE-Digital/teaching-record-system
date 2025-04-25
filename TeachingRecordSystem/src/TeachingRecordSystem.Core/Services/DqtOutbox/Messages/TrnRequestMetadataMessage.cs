using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

public record TrnRequestMetadataMessage
{
    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required DateTime CreatedOn { get; init; }
    public required bool? IdentityVerified { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? OneLoginUserSubject { get; init; }
    public required string[] Name { get; init; }
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public string? PreviousFirstName { get; init; }
    public string? PreviousLastName { get; init; }
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
    public string? TrnToken { get; init; }
    public Guid? ResolvedPersonId { get; set; }

    // public static TrnRequestMetadataMessage FromModel(TrnRequestMetadata model) => new()
    // {
    //     ApplicationUserId = model.ApplicationUserId,
    //     RequestId = model.RequestId,
    //     CreatedOn = model.CreatedOn,
    //     IdentityVerified = model.IdentityVerified,
    //     EmailAddress = model.EmailAddress,
    //     OneLoginUserSubject = model.OneLoginUserSubject,
    //     Name = model.Name,
    //     FirstName = model.FirstName,
    //     MiddleName = model.MiddleName,
    //     LastName = model.LastName,
    //     PreviousFirstName = model.PreviousFirstName,
    //     PreviousLastName = model.PreviousLastName,
    //     DateOfBirth = model.DateOfBirth,
    //     PotentialDuplicate = model.PotentialDuplicate,
    //     NationalInsuranceNumber = model.NationalInsuranceNumber,
    //     Gender = model.Gender,
    //     AddressLine1 = model.AddressLine1,
    //     AddressLine2 = model.AddressLine2,
    //     AddressLine3 = model.AddressLine3,
    //     City = model.City,
    //     Postcode = model.Postcode,
    //     Country = model.Country,
    //     TrnToken = model.TrnToken,
    //     ResolvedPersonId = model.ResolvedPersonId
    // };
}
