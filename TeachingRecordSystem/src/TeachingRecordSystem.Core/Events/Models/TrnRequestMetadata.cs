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
    public required int? Gender { get; init; }
    public required string? AddressLine1 { get; init; }
    public required string? AddressLine2 { get; init; }
    public required string? AddressLine3 { get; init; }
    public required string? City { get; init; }
    public required string? Postcode { get; init; }
    public required string? Country { get; init; }
    public required string? TrnToken { get; set; }
    public required Guid? ResolvedPersonId { get; set; }
    public required TrnRequestMatches? Matches { get; set; }

    public static TrnRequestMetadata FromModel(Core.DataStore.Postgres.Models.TrnRequestMetadata model) =>
        new()
        {
            ApplicationUserId = model.ApplicationUserId,
            RequestId = model.RequestId,
            CreatedOn = model.CreatedOn,
            IdentityVerified = model.IdentityVerified,
            EmailAddress = model.EmailAddress,
            OneLoginUserSubject = model.OneLoginUserSubject,
            FirstName = model.FirstName,
            MiddleName = model.LastName,
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
            Matches = model.Matches is not null ?
                new TrnRequestMatches()
                {
                    MatchedRecords = model.Matches.MatchedRecords
                        .Select(m => new TrnRequestMatchedRecord()
                        {
                            PersonId = m.PersonId
                        })
                        .AsList()
                } :
                null
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
