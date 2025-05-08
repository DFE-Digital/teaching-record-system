using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

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

    public EditDetailsStateBuilder WithInitializedState(string? firstName, string? middleName, string? lastName, DateOnly? dateOfBirth, string? emailAddress, string? mobileNumber, string? nationalInsuranceNumber)
    {
        this.Initialized = true;
        FirstName = firstName;
        MiddleName = middleName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
        EmailAddress = emailAddress;
        MobileNumber = mobileNumber;
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
            MobileNumber = (MobileNumber?)MobileNumber,
            EmailAddress = (EmailAddress?)EmailAddress,
            NationalInsuranceNumber = (NationalInsuranceNumber?)NationalInsuranceNumber,
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
