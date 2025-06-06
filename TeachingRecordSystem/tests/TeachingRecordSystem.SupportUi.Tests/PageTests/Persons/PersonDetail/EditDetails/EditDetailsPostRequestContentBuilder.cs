using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class EditDetailsPostRequestContentBuilder : PostRequestContentBuilder
{
    private string? FirstName { get; set; }
    private string? MiddleName { get; set; }
    private string? LastName { get; set; }
    private DateOnly? DateOfBirth { get; set; }
    private string? EmailAddress { get; set; }
    private string? MobileNumber { get; set; }
    private string? NationalInsuranceNumber { get; set; }
    private EditDetailsNameChangeReasonOption? NameChangeReason { get; set; }
    private bool? NameChangeUploadEvidence { get; set; }
    private (HttpContent, string)? NameChangeEvidenceFile { get; set; }
    private Guid? NameChangeEvidenceFileId { get; set; }
    private string? NameChangeEvidenceFileName { get; set; }
    private string? NameChangeEvidenceFileSizeDescription { get; set; }
    private string? NameChangeUploadedEvidenceFileUrl { get; set; }
    private EditDetailsOtherDetailsChangeReasonOption? OtherDetailsChangeReason { get; set; }
    private string? OtherDetailsChangeReasonDetail { get; set; }
    private bool? OtherDetailsChangeUploadEvidence { get; set; }
    private (HttpContent, string)? OtherDetailsChangeEvidenceFile { get; set; }
    private Guid? OtherDetailsChangeEvidenceFileId { get; set; }
    private string? OtherDetailsChangeEvidenceFileName { get; set; }
    private string? OtherDetailsChangeEvidenceFileSizeDescription { get; set; }
    private string? OtherDetailsChangeUploadedEvidenceFileUrl { get; set; }

    public EditDetailsPostRequestContentBuilder WithFirstName(string? firstName)
    {
        FirstName = firstName;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithMiddleName(string? middleName)
    {
        MiddleName = middleName;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithLastName(string? lastName)
    {
        LastName = lastName;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithDateOfBirth(DateOnly? dateOfBirth)
    {
        DateOfBirth = dateOfBirth;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithEmailAddress(string? emailAddress)
    {
        EmailAddress = emailAddress;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithMobileNumber(string? mobileNumber)
    {
        MobileNumber = mobileNumber;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
    {
        NationalInsuranceNumber = nationalInsuranceNumber;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithNameChangeReason(EditDetailsNameChangeReasonOption nameChangeReason)
    {
        NameChangeReason = nameChangeReason;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithNameChangeEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        NameChangeUploadEvidence = uploadEvidence;
        NameChangeEvidenceFile = evidenceFile;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithNameChangeEvidence(bool uploadEvidence, Guid? evidenceFileId, string? nameChangeEvidenceFileName, string? nameChangeEvidenceFileSizeDescription, string? nameChangeUploadedEvidenceFileUrl)
    {
        NameChangeUploadEvidence = uploadEvidence;
        NameChangeEvidenceFileId = evidenceFileId;
        NameChangeEvidenceFileName = nameChangeEvidenceFileName;
        NameChangeEvidenceFileSizeDescription = nameChangeEvidenceFileSizeDescription;
        NameChangeUploadedEvidenceFileUrl = nameChangeUploadedEvidenceFileUrl;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption otherDetailsChangeReason, string? detail = null)
    {
        OtherDetailsChangeReason = otherDetailsChangeReason;
        OtherDetailsChangeReasonDetail = detail;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithOtherDetailsChangeEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        OtherDetailsChangeUploadEvidence = uploadEvidence;
        OtherDetailsChangeEvidenceFile = evidenceFile;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithOtherDetailsChangeEvidence(bool uploadEvidence, Guid? evidenceFileId, string? otherDetailsChangeEvidenceFileName, string? otherDetailsChangeEvidenceFileSizeDescription, string? otherDetailsChangeUploadedEvidenceFileUrl)
    {
        OtherDetailsChangeUploadEvidence = uploadEvidence;
        OtherDetailsChangeEvidenceFileId = evidenceFileId;
        OtherDetailsChangeEvidenceFileName = otherDetailsChangeEvidenceFileName;
        OtherDetailsChangeEvidenceFileSizeDescription = otherDetailsChangeEvidenceFileSizeDescription;
        OtherDetailsChangeUploadedEvidenceFileUrl = otherDetailsChangeUploadedEvidenceFileUrl;
        return this;
    }
}
