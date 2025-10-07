using TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.AddPerson;

public class AddPersonPostRequestContentBuilder : PostRequestContentBuilder
{
    private string? FirstName { get; set; }
    private string? MiddleName { get; set; }
    private string? LastName { get; set; }
    private DateOnly? DateOfBirth { get; set; }
    private string? EmailAddress { get; set; }
    private string? MobileNumber { get; set; }
    private string? NationalInsuranceNumber { get; set; }
    private Gender? Gender { get; set; }
    private AddPersonReasonOption? CreateReason { get; set; }
    private string? CreateReasonDetail { get; set; }
    private bool? UploadEvidence { get; set; }
    private (HttpContent, string)? EvidenceFile { get; set; }
    private Guid? EvidenceFileId { get; set; }
    private string? EvidenceFileName { get; set; }
    private string? EvidenceFileSizeDescription { get; set; }
    private string? EvidenceFileUrl { get; set; }

    public AddPersonPostRequestContentBuilder WithFirstName(string? firstName)
    {
        FirstName = firstName;
        return this;
    }

    public AddPersonPostRequestContentBuilder WithMiddleName(string? middleName)
    {
        MiddleName = middleName;
        return this;
    }

    public AddPersonPostRequestContentBuilder WithLastName(string? lastName)
    {
        LastName = lastName;
        return this;
    }

    public AddPersonPostRequestContentBuilder WithDateOfBirth(DateOnly? dateOfBirth)
    {
        DateOfBirth = dateOfBirth;
        return this;
    }

    public AddPersonPostRequestContentBuilder WithEmailAddress(string? emailAddress)
    {
        EmailAddress = emailAddress;
        return this;
    }

    public AddPersonPostRequestContentBuilder WithMobileNumber(string? mobileNumber)
    {
        MobileNumber = mobileNumber;
        return this;
    }

    public AddPersonPostRequestContentBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
    {
        NationalInsuranceNumber = nationalInsuranceNumber;
        return this;
    }

    public AddPersonPostRequestContentBuilder WithGender(Gender? gender)
    {
        Gender = gender;
        return this;
    }

    public AddPersonPostRequestContentBuilder WithReason(AddPersonReasonOption createReason, string? detail = null)
    {
        CreateReason = createReason;
        CreateReasonDetail = detail;
        return this;
    }

    public AddPersonPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFile = evidenceFile;
        return this;
    }

    public AddPersonPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, Guid? evidenceFileId, string? otherDetailsChangeEvidenceFileName, string? otherDetailsChangeEvidenceFileSizeDescription, string? otherDetailsChangeUploadedEvidenceFileUrl)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFileId = evidenceFileId;
        EvidenceFileName = otherDetailsChangeEvidenceFileName;
        EvidenceFileSizeDescription = otherDetailsChangeEvidenceFileSizeDescription;
        EvidenceFileUrl = otherDetailsChangeUploadedEvidenceFileUrl;
        return this;
    }
}
