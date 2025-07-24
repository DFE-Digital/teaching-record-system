using TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

public class MergeStateBuilder
{
    private bool Initialized { get; set; }
    private string? PersonATrn { get; set; }
    private string? PersonBTrn { get; set; }
    private Guid? PersonAId { get; set; }
    private Guid? PersonBId { get; set; }
    private Guid? PrimaryRecordId { get; set; }

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

    public MergeStateBuilder WithPrimaryRecord(TestData.CreatePersonResult person)
    {
        PrimaryRecordId = person.PersonId;

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
            PrimaryRecordId = PrimaryRecordId
        };
    }
}
