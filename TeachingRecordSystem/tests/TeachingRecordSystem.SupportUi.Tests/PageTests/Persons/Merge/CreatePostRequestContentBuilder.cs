namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

public class MergePostRequestContentBuilder : PostRequestContentBuilder
{
    private string? OtherTrn { get; set; }

    public MergePostRequestContentBuilder WithOtherTrn(string? otherTrn)
    {
        OtherTrn = otherTrn;
        return this;
    }
}
