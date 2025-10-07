using TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.MergePerson;

public class MergePersonPostRequestContentBuilder : PostRequestContentBuilder
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

    public MergePersonPostRequestContentBuilder WithOtherTrn(string? otherTrn)
    {
        OtherTrn = otherTrn;
        return this;
    }

    public MergePersonPostRequestContentBuilder WithPrimaryPersonId(Guid? primaryPersonId)
    {
        PrimaryPersonId = primaryPersonId;
        return this;
    }

    public MergePersonPostRequestContentBuilder WithFirstNameSource(PersonAttributeSource attributeSource)
    {
        FirstNameSource = attributeSource;
        return this;
    }

    public MergePersonPostRequestContentBuilder WithMiddleNameSource(PersonAttributeSource attributeSource)
    {
        MiddleNameSource = attributeSource;
        return this;
    }

    public MergePersonPostRequestContentBuilder WithLastNameSource(PersonAttributeSource attributeSource)
    {
        LastNameSource = attributeSource;
        return this;
    }

    public MergePersonPostRequestContentBuilder WithDateOfBirthSource(PersonAttributeSource attributeSource)
    {
        DateOfBirthSource = attributeSource;
        return this;
    }

    public MergePersonPostRequestContentBuilder WithEmailAddressSource(PersonAttributeSource attributeSource)
    {
        EmailAddressSource = attributeSource;
        return this;
    }

    public MergePersonPostRequestContentBuilder WithNationalInsuranceNumberSource(PersonAttributeSource attributeSource)
    {
        NationalInsuranceNumberSource = attributeSource;
        return this;
    }

    public MergePersonPostRequestContentBuilder WithGenderSource(PersonAttributeSource attributeSource)
    {
        GenderSource = attributeSource;
        return this;
    }

    public MergePersonPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, (HttpContent content, string filename)? evidenceFile = null)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFile = evidenceFile;
        return this;
    }

    public MergePersonPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, Guid? evidenceFileId, string? otherDetailsChangeEvidenceFileName, string? otherDetailsChangeEvidenceFileSizeDescription, string? otherDetailsChangeUploadedEvidenceFileUrl)
    {
        UploadEvidence = uploadEvidence;
        EvidenceFileId = evidenceFileId;
        EvidenceFileName = otherDetailsChangeEvidenceFileName;
        EvidenceFileSizeDescription = otherDetailsChangeEvidenceFileSizeDescription;
        EvidenceFileUrl = otherDetailsChangeUploadedEvidenceFileUrl;
        return this;
    }

    public MergePersonPostRequestContentBuilder WithComments(string? comments)
    {
        Comments = comments;
        return this;
    }
}
