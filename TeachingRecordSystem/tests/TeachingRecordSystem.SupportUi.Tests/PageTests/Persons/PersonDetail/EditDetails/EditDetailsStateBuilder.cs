using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
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
    public EditDetailsChangeReasonOption? ChangeReason { get; set; }
    public string? ChangeReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }
    private bool Initialized { get; set; }

    public EditDetailsStateBuilder WithInitializedState()
    {
        Initialized = true;
        return this;
    }

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

    public EditDetailsStateBuilder WithChangeReasonChoice(EditDetailsChangeReasonOption option, string? detailText = null)
    {
        ChangeReason = option;
        ChangeReasonDetail = detailText;
        return this;
    }

    public EditDetailsStateBuilder WithUploadEvidenceChoice(bool uploadEvidence, Guid? evidenceFileId = null, string? evidenceFileName = "evidence.jpeg", string evidenceFileSizeDescription = "5MB")
    {
        UploadEvidence = uploadEvidence;
        if (evidenceFileId.HasValue)
        {
            EvidenceFileId = evidenceFileId;
            EvidenceFileName = evidenceFileName;
            EvidenceFileSizeDescription = evidenceFileSizeDescription;
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
            MobileNumber = EditDetailsFieldState<MobileNumber>.FromRawValue(MobileNumber),
            NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(NationalInsuranceNumber),
            ChangeReason = ChangeReason,
            ChangeReasonDetail = ChangeReasonDetail,
            UploadEvidence = UploadEvidence,
            EvidenceFileId = EvidenceFileId,
            EvidenceFileName = EvidenceFileName,
            EvidenceFileSizeDescription = EvidenceFileSizeDescription,
            Initialized = Initialized
        };
    }
}
