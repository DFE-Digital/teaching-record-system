using TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

public class MergePostRequestContentBuilder : PostRequestContentBuilder
{
    private string? OtherTrn { get; set; }
    private Guid? PrimaryPersonId { get; set; }
    private PersonAttributeSource? FirstNameSource { get; set; }
    private PersonAttributeSource? MiddleNameSource { get; set; }
    private PersonAttributeSource? LastNameSource { get; set; }
    private PersonAttributeSource? DateOfBirthSource { get; set; }
    private PersonAttributeSource? EmailAddressSource { get; set; }
    private PersonAttributeSource? NationalInsuranceNumberSource { get; set; }
    private PersonAttributeSource? GenderSource { get; set; }
    private bool? UploadEvidence { get; set; }
    private (HttpContent, string)? EvidenceFile { get; set; }
    private Guid? EvidenceFileId { get; set; }
    private string? EvidenceFileName { get; set; }
    private string? EvidenceFileSizeDescription { get; set; }
    private string? EvidenceFileUrl { get; set; }
    private string? Comments { get; set; }

    public MergePostRequestContentBuilder WithOtherTrn(string? otherTrn)
    {
        OtherTrn = otherTrn;
        return this;
    }

    public MergePostRequestContentBuilder WithPrimaryPersonId(Guid? primaryPersonId)
    {
        PrimaryPersonId = primaryPersonId;
        return this;
    }

    public MergePostRequestContentBuilder WithFirstNameSource(PersonAttributeSource attributeSource)
    {
        FirstNameSource = attributeSource;
        return this;
    }

    public MergePostRequestContentBuilder WithMiddleNameSource(PersonAttributeSource attributeSource)
    {
        MiddleNameSource = attributeSource;
        return this;
    }

    public MergePostRequestContentBuilder WithLastNameSource(PersonAttributeSource attributeSource)
    {
        LastNameSource = attributeSource;
        return this;
    }

    public MergePostRequestContentBuilder WithDateOfBirthSource(PersonAttributeSource attributeSource)
    {
        DateOfBirthSource = attributeSource;
        return this;
    }

    public MergePostRequestContentBuilder WithEmailAddressSource(PersonAttributeSource attributeSource)
    {
        EmailAddressSource = attributeSource;
        return this;
    }

    public MergePostRequestContentBuilder WithNationalInsuranceNumberSource(PersonAttributeSource attributeSource)
    {
        NationalInsuranceNumberSource = attributeSource;
        return this;
    }

    public MergePostRequestContentBuilder WithGenderSource(PersonAttributeSource attributeSource)
    {
        GenderSource = attributeSource;
        return this;
    }

    public MergePostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFile = evidenceFile;
        return this;
    }

    public MergePostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, Guid? evidenceFileId, string? otherDetailsChangeEvidenceFileName, string? otherDetailsChangeEvidenceFileSizeDescription, string? otherDetailsChangeUploadedEvidenceFileUrl)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFileId = evidenceFileId;
        EvidenceFileName = otherDetailsChangeEvidenceFileName;
        EvidenceFileSizeDescription = otherDetailsChangeEvidenceFileSizeDescription;
        EvidenceFileUrl = otherDetailsChangeUploadedEvidenceFileUrl;
        return this;
    }

    public MergePostRequestContentBuilder WithComments(string? comments)
    {
        Comments = comments;
        return this;
    }
}
