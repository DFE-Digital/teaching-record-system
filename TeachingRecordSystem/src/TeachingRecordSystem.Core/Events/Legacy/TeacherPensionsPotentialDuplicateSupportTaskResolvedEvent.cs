using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record TeacherPensionsPotentialDuplicateSupportTaskResolvedEvent : SupportTaskUpdatedEvent, IEventWithPersonId, IEventWithPersonAttributes
{
    public required Guid PersonId { get; init; }
    public required EventModels.TrnRequestMetadata RequestData { get; init; }
    public required TeacherPensionsPotentialDuplicateSupportTaskResolvedReason ChangeReason { get; set; }
    public required TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges Changes { get; init; }
    public required EventModels.PersonAttributes PersonAttributes { get; init; }
    public required EventModels.PersonAttributes? OldPersonAttributes { get; init; }
    public required string? Comments { get; init; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }
    public string? SecondaryPersonTrn { get; set; }
}

public enum TeacherPensionsPotentialDuplicateSupportTaskResolvedReason
{
    [Display(Name = "Record kept - no existing person identified during task resolution")]
    RecordKept = 1,
    [Display(Name = "Records merged - identified as same person during task resolution")]
    RecordMerged = 2
}

[Flags]
public enum TeacherPensionsPotentialDuplicateSupportTaskResolvedEventChanges
{
    None = 0,
    Status = 1 << 0,
    PersonFirstName = PersonAttributesChanges.FirstName,
    PersonMiddleName = PersonAttributesChanges.MiddleName,
    PersonLastName = PersonAttributesChanges.LastName,
    PersonDateOfBirth = PersonAttributesChanges.DateOfBirth,
    PersonNationalInsuranceNumber = PersonAttributesChanges.NationalInsuranceNumber,
    PersonGender = PersonAttributesChanges.Gender,
    PersonNameChange = PersonFirstName | PersonMiddleName | PersonLastName,
    AllChanges = PersonNameChange | PersonDateOfBirth | PersonNationalInsuranceNumber | PersonGender | Status
}

