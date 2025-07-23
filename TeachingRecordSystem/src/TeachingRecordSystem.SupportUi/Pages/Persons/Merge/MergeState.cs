
namespace TeachingRecordSystem.SupportUi.Pages.Persons.Merge;

public class MergeState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.MergePerson,
        typeof(MergeState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }
    public string? OtherTrn { get; set; }

    public void EnsureInitialized()
    {
        if (Initialized)
        {
            return;
        }

        Initialized = true;
    }
}
