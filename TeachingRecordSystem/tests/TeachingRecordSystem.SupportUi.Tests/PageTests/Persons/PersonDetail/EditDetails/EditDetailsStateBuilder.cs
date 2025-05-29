using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class EditDetailsStateBuilder
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? EmailAddress { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalInsuranceNumber { get; set; }

    public string? OriginalFirstName { get; set; }
    public string? OriginalMiddleName { get; set; }
    public string? OriginalLastName { get; set; }
    public DateOnly? OriginalDateOfBirth { get; set; }
    public string? OriginalEmailAddress { get; set; }
    public string? OriginalMobileNumber { get; set; }
    public string? OriginalNationalInsuranceNumber { get; set; }

    public EditDetailsNameChangeReasonOption? NameChangeReason { get; set; }
    public bool? NameChangeUploadEvidence { get; set; }
    public Guid? NameChangeEvidenceFileId { get; set; }
    public string? NameChangeEvidenceFileName { get; set; }
    public string? NameChangeEvidenceFileSizeDescription { get; set; }

    public EditDetailsOtherDetailsChangeReasonOption? OtherDetailsChangeReason { get; set; }
    public string? OtherDetailsChangeReasonDetail { get; set; }
    public bool? OtherDetailsChangeUploadEvidence { get; set; }
    public Guid? OtherDetailsChangeEvidenceFileId { get; set; }
    public string? OtherDetailsChangeEvidenceFileName { get; set; }
    public string? OtherDetailsChangeEvidenceFileSizeDescription { get; set; }

    private bool Initialized { get; set; }

    public EditDetailsStateBuilder WithInitializedState(CreatePersonResult person)
    {
        Initialized = true;
        FirstName = person.FirstName;
        MiddleName = person.MiddleName;
        LastName = person.LastName;
        DateOfBirth = person.DateOfBirth;
        EmailAddress = person.Email;
        MobileNumber = person.MobileNumber;
        NationalInsuranceNumber = person.NationalInsuranceNumber;

        OriginalFirstName = person.FirstName;
        OriginalMiddleName = person.MiddleName;
        OriginalLastName = person.LastName;
        OriginalDateOfBirth = person.DateOfBirth;
        OriginalEmailAddress = person.Email;
        OriginalMobileNumber = person.MobileNumber;
        OriginalNationalInsuranceNumber = person.NationalInsuranceNumber;

        return this;
    }

    public EditDetailsStateBuilder WithName(string? firstName, string? middleName, string? lastName)
    {
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        return this;
    }

    public EditDetailsStateBuilder WithDateOfBirth(DateOnly? dateOfBirth)
    {
        DateOfBirth = dateOfBirth;
        return this;
    }

    public EditDetailsStateBuilder WithEmail(string? emailAddress)
    {
        EmailAddress = emailAddress;
        return this;
    }

    public EditDetailsStateBuilder WithMobileNumber(string? mobileNumber)
    {
        MobileNumber = mobileNumber;
        return this;
    }

    public EditDetailsStateBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
    {
        NationalInsuranceNumber = nationalInsuranceNumber;
        return this;
    }

    public EditDetailsStateBuilder WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption option)
    {
        NameChangeReason = option;
        return this;
    }

    public EditDetailsStateBuilder WithNameChangeUploadEvidenceChoice(bool uploadEvidence, Guid? evidenceFileId = null, string? evidenceFileName = "evidence.jpeg", string evidenceFileSizeDescription = "5MB")
    {
        NameChangeUploadEvidence = uploadEvidence;
        if (evidenceFileId.HasValue)
        {
            NameChangeEvidenceFileId = evidenceFileId;
            NameChangeEvidenceFileName = evidenceFileName;
            NameChangeEvidenceFileSizeDescription = evidenceFileSizeDescription;
        }
        return this;
    }

    public EditDetailsStateBuilder WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption option, string? detailText = null)
    {
        OtherDetailsChangeReason = option;
        OtherDetailsChangeReasonDetail = detailText;
        return this;
    }

    public EditDetailsStateBuilder WithOtherDetailsChangeUploadEvidenceChoice(bool uploadEvidence, Guid? evidenceFileId = null, string? evidenceFileName = "evidence.jpeg", string evidenceFileSizeDescription = "5MB")
    {
        OtherDetailsChangeUploadEvidence = uploadEvidence;
        if (evidenceFileId.HasValue)
        {
            OtherDetailsChangeEvidenceFileId = evidenceFileId;
            OtherDetailsChangeEvidenceFileName = evidenceFileName;
            OtherDetailsChangeEvidenceFileSizeDescription = evidenceFileSizeDescription;
        }
        return this;
    }

    public EditDetailsState Build()
    {
        return new EditDetailsState
        {
            FirstName = FirstName,
            MiddleName = MiddleName,
            LastName = LastName,
            DateOfBirth = DateOfBirth,
            EmailAddress = EditDetailsFieldState<Core.EmailAddress>.FromRawValue(EmailAddress),
            MobileNumber = EditDetailsFieldState<Core.MobileNumber>.FromRawValue(MobileNumber),
            NationalInsuranceNumber = EditDetailsFieldState<Core.NationalInsuranceNumber>.FromRawValue(NationalInsuranceNumber),

            OriginalFirstName = OriginalFirstName,
            OriginalMiddleName = OriginalMiddleName,
            OriginalLastName = OriginalLastName,
            OriginalDateOfBirth = OriginalDateOfBirth,
            OriginalEmailAddress = EditDetailsFieldState<Core.EmailAddress>.FromRawValue(OriginalEmailAddress),
            OriginalMobileNumber = EditDetailsFieldState<Core.MobileNumber>.FromRawValue(OriginalMobileNumber),
            OriginalNationalInsuranceNumber = EditDetailsFieldState<Core.NationalInsuranceNumber>.FromRawValue(OriginalNationalInsuranceNumber),

            NameChangeReason = NameChangeReason,
            NameChangeUploadEvidence = NameChangeUploadEvidence,
            NameChangeEvidenceFileId = NameChangeEvidenceFileId,
            NameChangeEvidenceFileName = NameChangeEvidenceFileName,
            NameChangeEvidenceFileSizeDescription = NameChangeEvidenceFileSizeDescription,
            OtherDetailsChangeReason = OtherDetailsChangeReason,
            OtherDetailsChangeReasonDetail = OtherDetailsChangeReasonDetail,
            OtherDetailsChangeUploadEvidence = OtherDetailsChangeUploadEvidence,
            OtherDetailsChangeEvidenceFileId = OtherDetailsChangeEvidenceFileId,
            OtherDetailsChangeEvidenceFileName = OtherDetailsChangeEvidenceFileName,
            OtherDetailsChangeEvidenceFileSizeDescription = OtherDetailsChangeEvidenceFileSizeDescription,

            Initialized = Initialized
        };
    }
}
