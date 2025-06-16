using TeachingRecordSystem.SupportUi.Pages.Persons.Create;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Create;

public class CreatePostRequestContentBuilder : PostRequestContentBuilder
{
    private string? FirstName { get; set; }
    private string? MiddleName { get; set; }
    private string? LastName { get; set; }
    private DateOnly? DateOfBirth { get; set; }
    private string? EmailAddress { get; set; }
    private string? MobileNumber { get; set; }
    private string? NationalInsuranceNumber { get; set; }
    private CreateReasonOption? CreateReason { get; set; }
    private string? CreateReasonDetail { get; set; }
    private bool? UploadEvidence { get; set; }
    private (HttpContent, string)? EvidenceFile { get; set; }
    private Guid? EvidenceFileId { get; set; }
    private string? EvidenceFileName { get; set; }
    private string? EvidenceFileSizeDescription { get; set; }
    private string? EvidenceFileUrl { get; set; }

    public CreatePostRequestContentBuilder WithFirstName(string? firstName)
    {
        FirstName = firstName;
        return this;
    }

    public CreatePostRequestContentBuilder WithMiddleName(string? middleName)
    {
        MiddleName = middleName;
        return this;
    }

    public CreatePostRequestContentBuilder WithLastName(string? lastName)
    {
        LastName = lastName;
        return this;
    }

    public CreatePostRequestContentBuilder WithDateOfBirth(DateOnly? dateOfBirth)
    {
        DateOfBirth = dateOfBirth;
        return this;
    }

    public CreatePostRequestContentBuilder WithEmailAddress(string? emailAddress)
    {
        EmailAddress = emailAddress;
        return this;
    }

    public CreatePostRequestContentBuilder WithMobileNumber(string? mobileNumber)
    {
        MobileNumber = mobileNumber;
        return this;
    }

    public CreatePostRequestContentBuilder WithNationalInsuranceNumber(string? nationalInsuranceNumber)
    {
        NationalInsuranceNumber = nationalInsuranceNumber;
        return this;
    }

    public CreatePostRequestContentBuilder WithCreateReason(CreateReasonOption createReason, string? detail = null)
    {
        CreateReason = createReason;
        CreateReasonDetail = detail;
        return this;
    }

    public CreatePostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFile = evidenceFile;
        return this;
    }

    public CreatePostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, Guid? evidenceFileId, string? otherDetailsChangeEvidenceFileName, string? otherDetailsChangeEvidenceFileSizeDescription, string? otherDetailsChangeUploadedEvidenceFileUrl)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFileId = evidenceFileId;
        EvidenceFileName = otherDetailsChangeEvidenceFileName;
        EvidenceFileSizeDescription = otherDetailsChangeEvidenceFileSizeDescription;
        EvidenceFileUrl = otherDetailsChangeUploadedEvidenceFileUrl;
        return this;
    }
}
