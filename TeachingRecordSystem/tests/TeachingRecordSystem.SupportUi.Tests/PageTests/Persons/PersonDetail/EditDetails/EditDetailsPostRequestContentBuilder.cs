using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class EditDetailsPostRequestContentBuilder : PostRequestContentBuilder
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? EmailAddress { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public Gender? Gender { get; set; }
    public object? Reason { get; set; }
    public string? ReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public (HttpContent, string)? EvidenceFile { get; set; }
    public UploadedEvidenceFile? UploadedEvidenceFile { get; set; }

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

    public EditDetailsPostRequestContentBuilder WithGender(Gender? gender)
    {
        Gender = gender;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithReason(EditDetailsNameChangeReasonOption nameChangeReason)
    {
        Reason = nameChangeReason;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithReason(EditDetailsOtherDetailsChangeReasonOption otherDetailsChangeReason, string? detail = null)
    {
        Reason = otherDetailsChangeReason;
        ReasonDetail = detail;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFile = evidenceFile;
        return this;
    }

    public EditDetailsPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, Guid? evidenceFileId, string? otherDetailsChangeEvidenceFileName, string? otherDetailsChangeEvidenceFileSizeDescription)
    {
        UploadEvidence = uploadEvidence;
        UploadedEvidenceFile = evidenceFileId is not Guid id ? null : new()
        {
            FileId = id,
            FileName = otherDetailsChangeEvidenceFileName ?? "filename.jpg",
            FileSizeDescription = otherDetailsChangeEvidenceFileSizeDescription ?? "5 MB"
        };
        return this;
    }
}
