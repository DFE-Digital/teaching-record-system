using TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

public class MergeStateBuilder
{
    private bool Initialized { get; set; }
    private string? PersonATrn { get; set; }
    private string? PersonBTrn { get; set; }
    private Guid? PersonAId { get; set; }
    private Guid? PersonBId { get; set; }
    private Guid? PrimaryPersonId { get; set; }
    private PersonAttributeSource? FirstNameSource { get; set; }
    private PersonAttributeSource? MiddleNameSource { get; set; }
    private PersonAttributeSource? LastNameSource { get; set; }
    private PersonAttributeSource? DateOfBirthSource { get; set; }
    private PersonAttributeSource? EmailAddressSource { get; set; }
    private PersonAttributeSource? NationalInsuranceNumberSource { get; set; }
    private PersonAttributeSource? GenderSource { get; set; }
    private bool PersonAttributeSourcesSet { get; set; }
    private bool? UploadEvidence { get; set; }
    private Guid? EvidenceFileId { get; set; }
    private string? EvidenceFileName { get; set; }
    private string? EvidenceFileSizeDescription { get; set; }
    private string? Comments { get; set; }

    public MergeStateBuilder WithInitializedState(TestData.CreatePersonResult personA)
    {
        Initialized = true;
        PersonAId = personA.PersonId;
        PersonATrn = personA.Trn;
        return this;
    }

    public MergeStateBuilder WithPersonB(TestData.CreatePersonResult personB)
    {
        PersonBId = personB.PersonId;
        PersonBTrn = personB.Trn;
        return this;
    }

    public MergeStateBuilder WithPrimaryPerson(TestData.CreatePersonResult person)
    {
        PrimaryPersonId = person.PersonId;
        return this;
    }

    public MergeStateBuilder WithFirstNameSource(PersonAttributeSource attributeSource)
    {
        FirstNameSource = attributeSource;
        return this;
    }

    public MergeStateBuilder WithMiddleNameSource(PersonAttributeSource attributeSource)
    {
        MiddleNameSource = attributeSource;
        return this;
    }

    public MergeStateBuilder WithLastNameSource(PersonAttributeSource attributeSource)
    {
        LastNameSource = attributeSource;
        return this;
    }

    public MergeStateBuilder WithDateOfBirthSource(PersonAttributeSource attributeSource)
    {
        DateOfBirthSource = attributeSource;
        return this;
    }

    public MergeStateBuilder WithEmailAddressSource(PersonAttributeSource attributeSource)
    {
        EmailAddressSource = attributeSource;
        return this;
    }

    public MergeStateBuilder WithNationalInsuranceNumberSource(PersonAttributeSource attributeSource)
    {
        NationalInsuranceNumberSource = attributeSource;
        return this;
    }

    public MergeStateBuilder WithGenderSource(PersonAttributeSource attributeSource)
    {
        GenderSource = attributeSource;
        return this;
    }

    public MergeStateBuilder WithAttributeSourcesSet()
    {
        PersonAttributeSourcesSet = true;
        return this;
    }

    public MergeStateBuilder WithUploadEvidenceChoice(bool uploadEvidence, Guid? evidenceFileId = null, string? evidenceFileName = "evidence.jpeg", string evidenceFileSizeDescription = "5MB")
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

    public MergeStateBuilder WithComments(string? comments)
    {
        Comments = comments;
        return this;
    }

    public MergeState Build()
    {
        return new MergeState
        {
            Initialized = Initialized,
            PersonAId = PersonAId,
            PersonATrn = PersonATrn,
            PersonBId = PersonBId,
            PersonBTrn = PersonBTrn,
            PrimaryPersonId = PrimaryPersonId,

            PersonAttributeSourcesSet = PersonAttributeSourcesSet,
            FirstNameSource = FirstNameSource,
            MiddleNameSource = MiddleNameSource,
            LastNameSource = LastNameSource,
            DateOfBirthSource = DateOfBirthSource,
            EmailAddressSource = EmailAddressSource,
            NationalInsuranceNumberSource = NationalInsuranceNumberSource,
            GenderSource = GenderSource,

            UploadEvidence = UploadEvidence,
            EvidenceFileId = EvidenceFileId,
            EvidenceFileName = EvidenceFileName,
            EvidenceFileSizeDescription = EvidenceFileSizeDescription,
            Comments = Comments,
        };
    }
}
