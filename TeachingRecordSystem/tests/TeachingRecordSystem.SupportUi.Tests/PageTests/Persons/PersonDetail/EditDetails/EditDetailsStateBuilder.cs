using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class EditDetailsStateBuilder
{
    public string FirstName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateOnly? DateOfBirth { get; set; }
    public string? EmailAddress { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public Gender? Gender { get; set; }

    public string OriginalFirstName { get; set; } = "";
    public string OriginalMiddleName { get; set; } = "";
    public string OriginalLastName { get; set; } = "";
    public DateOnly? OriginalDateOfBirth { get; set; }
    public string? OriginalEmailAddress { get; set; }
    public string? OriginalNationalInsuranceNumber { get; set; }
    public Gender? OriginalGender { get; set; }

    public EditDetailsNameChangeReasonOption? NameChangeReason { get; set; }
    public bool? NameChangeUploadEvidence { get; set; }
    public UploadedEvidenceFile? NameChangeEvidenceFile { get; set; }

    public EditDetailsOtherDetailsChangeReasonOption? OtherDetailsChangeReason { get; set; }
    public string? OtherDetailsChangeReasonDetail { get; set; }
    public bool? OtherDetailsChangeUploadEvidence { get; set; }
    public UploadedEvidenceFile? OtherDetailsChangeEvidenceFile { get; set; }

    private bool Initialized { get; set; }

    public EditDetailsStateBuilder WithInitializedState(CreatePersonResult person)
    {
        Initialized = true;
        FirstName = person.FirstName;
        MiddleName = person.MiddleName;
        LastName = person.LastName;
        DateOfBirth = person.DateOfBirth;
        EmailAddress = person.EmailAddress;
        NationalInsuranceNumber = person.NationalInsuranceNumber;
        Gender = person.Gender;

        OriginalFirstName = person.FirstName;
        OriginalMiddleName = person.MiddleName;
        OriginalLastName = person.LastName;
        OriginalDateOfBirth = person.DateOfBirth;
        OriginalEmailAddress = person.EmailAddress;
        OriginalNationalInsuranceNumber = person.NationalInsuranceNumber;
        OriginalGender = person.Gender;

        return this;
    }

    public EditDetailsStateBuilder WithName(string? firstName, string? middleName, string? lastName)
    {
        FirstName = firstName ?? "";
        MiddleName = middleName ?? "";
        LastName = lastName ?? "";
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

    public EditDetailsStateBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
    {
        NationalInsuranceNumber = nationalInsuranceNumber;
        return this;
    }

    public EditDetailsStateBuilder WithGender(Gender? gender)
    {
        Gender = gender;
        return this;
    }

    public EditDetailsStateBuilder WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption option)
    {
        NameChangeReason = option;
        return this;
    }

    public EditDetailsStateBuilder WithNameChangeUploadEvidenceChoice(bool uploadEvidence, Guid? evidenceFileId = null, string? evidenceFileName = null, string? evidenceFileSizeDescription = null)
    {
        NameChangeUploadEvidence = uploadEvidence;
        if (evidenceFileId is Guid id)
        {
            NameChangeEvidenceFile = new()
            {
                FileId = id,
                FileName = evidenceFileName ?? "evidence.jpeg",
                FileSizeDescription = evidenceFileSizeDescription ?? "5MB",
            };
        }
        return this;
    }

    public EditDetailsStateBuilder WithOtherDetailsChangeReasonChoice(EditDetailsOtherDetailsChangeReasonOption option, string? detailText = null)
    {
        OtherDetailsChangeReason = option;
        OtherDetailsChangeReasonDetail = detailText;
        return this;
    }

    public EditDetailsStateBuilder WithOtherDetailsChangeUploadEvidenceChoice(bool uploadEvidence, Guid? evidenceFileId = null, string? evidenceFileName = null, string? evidenceFileSizeDescription = null)
    {
        OtherDetailsChangeUploadEvidence = uploadEvidence;
        if (evidenceFileId is Guid id)
        {
            OtherDetailsChangeEvidenceFile = new()
            {
                FileId = id,
                FileName = evidenceFileName ?? "evidence.jpeg",
                FileSizeDescription = evidenceFileSizeDescription ?? "5MB",
            };
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
            EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(EmailAddress),
            NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(NationalInsuranceNumber),
            Gender = Gender,

            OriginalFirstName = OriginalFirstName,
            OriginalMiddleName = OriginalMiddleName,
            OriginalLastName = OriginalLastName,
            OriginalDateOfBirth = OriginalDateOfBirth,
            OriginalEmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(OriginalEmailAddress),
            OriginalNationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(OriginalNationalInsuranceNumber),
            OriginalGender = OriginalGender,

            NameChangeReason = NameChangeReason,
            NameChangeEvidence = new()
            {
                UploadEvidence = NameChangeUploadEvidence,
                UploadedEvidenceFile = NameChangeEvidenceFile
            },
            OtherDetailsChangeReason = OtherDetailsChangeReason,
            OtherDetailsChangeReasonDetail = OtherDetailsChangeReasonDetail,
            OtherDetailsChangeEvidence = new()
            {
                UploadEvidence = OtherDetailsChangeUploadEvidence,
                UploadedEvidenceFile = OtherDetailsChangeEvidenceFile
            },
            Initialized = Initialized
        };
    }
}
