using TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.MergePerson;

public class MergePersonPostRequestContentBuilder : PostRequestContentBuilder
{
    public string? OtherTrn { get; set; }
    public Guid? PrimaryPersonId { get; set; }
    public PersonAttributeSource? FirstNameSource { get; set; }
    public PersonAttributeSource? MiddleNameSource { get; set; }
    public PersonAttributeSource? LastNameSource { get; set; }
    public PersonAttributeSource? DateOfBirthSource { get; set; }
    public PersonAttributeSource? EmailAddressSource { get; set; }
    public PersonAttributeSource? NationalInsuranceNumberSource { get; set; }
    public PersonAttributeSource? GenderSource { get; set; }
    public bool? UploadEvidence { get; set; }
    public (HttpContent, string)? EvidenceFile { get; set; }
    public UploadedEvidenceFile? UploadedEvidenceFile { get; set; }
    public string? Comments { get; set; }

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

    public MergePersonPostRequestContentBuilder WithUploadEvidence(bool uploadEvidence, Guid? evidenceFileId, string? evidenceFileName, string? evidenceFileSizeDescription)
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

    public MergePersonPostRequestContentBuilder WithComments(string? comments)
    {
        Comments = comments;
        return this;
    }
}
