using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class TrnRequestMetadata
{
    public required Guid ApplicationUserId { get; init; }
    public ApplicationUser? ApplicationUser { get; }
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
    public bool? PotentialDuplicate { get; set; }
    public string? NationalInsuranceNumber { get; init; }
    public int? Gender { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? AddressLine3 { get; init; }
    public string? City { get; init; }
    public string? Postcode { get; init; }
    public string? Country { get; init; }
    public string? TrnToken { get; set; }
    public Guid? ResolvedPersonId { get; private set; }
    public TrnRequestStatus? Status { get; private set; } = TrnRequestStatus.Pending;
    public TrnRequestMatches? Matches { get; set; }
    public bool? NpqWorkingInEducationalSetting { get; init; }
    public string? NpqApplicationId { get; init; }
    public string? NpqName { get; init; }
    public string? NpqTrainingProvider { get; init; }

    public Guid? NpqEvidenceFileId { get; init; }
    public string? NpqEvidenceFileName { get; init; }

    public void SetResolvedPerson(Guid personId, TrnRequestStatus requestStatus = TrnRequestStatus.Completed)
    {
        ResolvedPersonId = personId;
        Status = requestStatus;
    }

    public void SetCompleted()
    {
        if (ResolvedPersonId is null)
        {
            throw new InvalidOperationException($"{nameof(ResolvedPersonId)} is not set.");
        }

        Status = TrnRequestStatus.Completed;
    }

    public static TrnRequestMetadata FromOutboxMessage(TrnRequestMetadataMessage message) =>
        new()
        {
            ApplicationUserId = message.ApplicationUserId,
            RequestId = message.RequestId,
            CreatedOn = message.CreatedOn,
            IdentityVerified = message.IdentityVerified,
            OneLoginUserSubject = message.OneLoginUserSubject,
            EmailAddress = message.EmailAddress,
            Name = message.Name,
            FirstName = message.FirstName,
            MiddleName = message.MiddleName,
            LastName = message.LastName,
            PreviousFirstName = message.PreviousFirstName,
            PreviousLastName = message.PreviousLastName,
            DateOfBirth = message.DateOfBirth,
            PotentialDuplicate = message.PotentialDuplicate,
            NationalInsuranceNumber = message.NationalInsuranceNumber,
            Gender = message.Gender,
            AddressLine1 = message.AddressLine1,
            AddressLine2 = message.AddressLine2,
            AddressLine3 = message.AddressLine3,
            City = message.City,
            Postcode = message.Postcode,
            Country = message.Country,
            TrnToken = message.TrnToken,
            Status = null  // Requests resolved in DQT have their status deduced later
        };
}

public record TrnRequestMatches
{
    public required IReadOnlyList<TrnRequestMatchedRecord> MatchedRecords { get; init; }
}

public record TrnRequestMatchedRecord
{
    public required Guid PersonId { get; init; }
}
