using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Events;

public record NpqTrnRequestSupportTaskResolvedEvent : SupportTaskUpdatedEvent, IEventWithPersonAttributes
{
    public required Guid PersonId { get; init; }
    public required EventModels.TrnRequestMetadata RequestData { get; init; }
    public NpqTrnRequestResolvedReason ChangeReason { get; set; }
    public required NpqTrnRequestSupportTaskResolvedEventChanges Changes { get; init; }
    public required EventModels.PersonAttributes PersonAttributes { get; init; }
    public required EventModels.PersonAttributes? OldPersonAttributes { get; init; }
    public required string? Comments { get; init; }
}

public enum NpqTrnRequestResolvedReason
{
    [Display(Name = "Record created - no existing person identified during task resolution")]
    RecordCreated = 1,
    [Display(Name = "Records merged - identified as same person during task resolution")]
    RecordMerged = 2
}

[Flags]
public enum NpqTrnRequestSupportTaskResolvedEventChanges
{
    None = 0,
    Status = 1 << 0,
    PersonFirstName = PersonAttributesChanges.FirstName,
    PersonMiddleName = PersonAttributesChanges.MiddleName,
    PersonLastName = PersonAttributesChanges.LastName,
    PersonDateOfBirth = PersonAttributesChanges.DateOfBirth,
    PersonEmailAddress = PersonAttributesChanges.EmailAddress,
    PersonNationalInsuranceNumber = PersonAttributesChanges.NationalInsuranceNumber,
    PersonGender = PersonAttributesChanges.Gender
}

