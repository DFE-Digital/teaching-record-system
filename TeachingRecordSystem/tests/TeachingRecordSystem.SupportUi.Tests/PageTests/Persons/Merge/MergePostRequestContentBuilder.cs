namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

public class MergePostRequestContentBuilder : PostRequestContentBuilder
{
    private string? OtherTrn { get; set; }
    private Guid? PrimaryRecordId { get; set; }

    public MergePostRequestContentBuilder WithOtherTrn(string? otherTrn)
    {
        OtherTrn = otherTrn;
        return this;
    }

    public MergePostRequestContentBuilder WithPrimaryRecordId(Guid? primaryRecordId)
    {
        PrimaryRecordId = primaryRecordId;
        return this;
    }
}
