using TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

public class MergeStateBuilder
{
    private bool Initialized { get; set; }

    public MergeStateBuilder WithInitializedState()
    {
        Initialized = true;

        return this;
    }

    public MergeState Build()
    {
        return new MergeState
        {
            Initialized = Initialized
        };
    }
}
