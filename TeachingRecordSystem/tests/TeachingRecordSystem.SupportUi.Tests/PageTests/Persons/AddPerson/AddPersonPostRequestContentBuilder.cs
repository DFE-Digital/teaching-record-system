using TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.AddPerson;

public class AddPersonPostRequestContentBuilder : PostRequestContentBuilder
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? EmailAddress { get; set; }
    public string? MobileNumber { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public Gender? Gender { get; set; }
    public AddPersonReasonOption? Reason { get; set; }
    public string? ReasonDetail { get; set; }
    public bool? UploadEvidence { get; set; }
    public (HttpContent, string)? EvidenceFile { get; set; }
    public UploadedEvidenceFile? UploadedEvidenceFile { get; set; }

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

    public AddPersonPostRequestContentBuilder WithReason(AddPersonReasonOption reason, string? detail = null)
    {
        Reason = reason;
        ReasonDetail = detail;
        return this;
    }

    public AddPersonPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFile = evidenceFile;
        return this;
    }

    public AddPersonPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, Guid? evidenceFileId, string? evidenceFileName, string? evidenceFileSizeDescription)
    {
        UploadEvidence = uploadEvidence;
        UploadedEvidenceFile = evidenceFileId is not Guid id ? null : new()
        {
            FileId = id,
            FileName = evidenceFileName ?? "filename.jpg",
            FileSizeDescription = evidenceFileSizeDescription ?? "5 MB"
        };
        return this;
    }
}
