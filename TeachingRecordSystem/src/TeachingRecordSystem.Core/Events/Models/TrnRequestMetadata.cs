using Dapper;

namespace TeachingRecordSystem.Core.Events.Models;

public record TrnRequestMetadata
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
    public required string? PreviousFirstName { get; init; }
    public required string? PreviousLastName { get; init; }
    public required string[] Name { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required bool? PotentialDuplicate { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
    public required string? AddressLine1 { get; init; }
    public required string? AddressLine2 { get; init; }
    public required string? AddressLine3 { get; init; }
    public required string? City { get; init; }
    public required string? Postcode { get; init; }
    public required string? Country { get; init; }
    public required string? TrnToken { get; set; }
    public required Guid? ResolvedPersonId { get; set; }
    public required TrnRequestMatches? Matches { get; set; }
    public required bool? NpqWorkingInEducationalSetting { get; init; }
    public required string? NpqApplicationId { get; init; }
    public required string? NpqName { get; init; }
    public required string? NpqTrainingProvider { get; init; }
    public required Guid? NpqEvidenceFileId { get; init; }
    public required string? NpqEvidenceFileName { get; init; }

    // TODO Make this required and non-nullable when we've migrated fully to new event system
    public TrnRequestStatus? Status { get; init; }

    public static TrnRequestMetadata FromModel(DataStore.Postgres.Models.TrnRequestMetadata model) =>
        new()
        {
            ApplicationUserId = model.ApplicationUserId,
            RequestId = model.RequestId,
            CreatedOn = model.CreatedOn,
            IdentityVerified = model.IdentityVerified,
            EmailAddress = model.EmailAddress,
            OneLoginUserSubject = model.OneLoginUserSubject,
            FirstName = model.FirstName,
            MiddleName = model.MiddleName,
            LastName = model.LastName,
            PreviousFirstName = model.PreviousFirstName,
            PreviousLastName = model.PreviousLastName,
            Name = model.Name.ToArray(),
            DateOfBirth = model.DateOfBirth,
            PotentialDuplicate = model.PotentialDuplicate,
            NationalInsuranceNumber = model.NationalInsuranceNumber,
            Gender = model.Gender,
            AddressLine1 = model.AddressLine1,
            AddressLine2 = model.AddressLine2,
            AddressLine3 = model.AddressLine3,
            City = model.City,
            Postcode = model.Postcode,
            Country = model.Country,
            TrnToken = model.TrnToken,
            ResolvedPersonId = model.ResolvedPersonId,
            Status = model.Status ?? (model.ResolvedPersonId.HasValue ? TrnRequestStatus.Completed : TrnRequestStatus.Pending),
            Matches = model.Matches is not null && model.Matches.MatchedPersons is not null ?
                new TrnRequestMatches()
                {
                    MatchedPersons = model.Matches.MatchedPersons
                        .Select(m => new TrnRequestMatchedPerson()
                        {
                            PersonId = m.PersonId
                        })
                        .AsList()
                } :
                null,
            NpqApplicationId = model.NpqApplicationId,
            NpqEvidenceFileId = model.NpqEvidenceFileId,
            NpqEvidenceFileName = model.NpqEvidenceFileName,
            NpqName = model.NpqName,
            NpqTrainingProvider = model.NpqTrainingProvider,
            NpqWorkingInEducationalSetting = model.NpqWorkingInEducationalSetting
        };
}

public record TrnRequestMatches
{
    public required IReadOnlyList<TrnRequestMatchedPerson> MatchedPersons { get; init; }
}

public record TrnRequestMatchedPerson
{
    public required Guid PersonId { get; init; }
}
