using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

public class MergePersonState
{
    /// <summary>
    /// The record the journey was started from.
    /// </summary>
    public required Guid PersonAId { get; set; }

    public required string PersonATrn { get; set; }

    /// <summary>
    /// The record whose TRN was entered on the first question.
    /// </summary>
    public Guid? PersonBId { get; set; }

    public string? PersonBTrn { get; set; }

    public Guid? PrimaryPersonId { get; set; }
    public PersonAttributeSource? FirstNameSource { get; set; }
    public PersonAttributeSource? MiddleNameSource { get; set; }
    public PersonAttributeSource? LastNameSource { get; set; }
    public PersonAttributeSource? DateOfBirthSource { get; set; }
    public PersonAttributeSource? EmailAddressSource { get; set; }
    public PersonAttributeSource? NationalInsuranceNumberSource { get; set; }
    public PersonAttributeSource? GenderSource { get; set; }
    public EvidenceUploadModel Evidence { get; set; } = new();
    public string? Comments { get; set; }
}
