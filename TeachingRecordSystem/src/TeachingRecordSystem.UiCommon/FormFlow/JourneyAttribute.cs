namespace TeachingRecordSystem.UiCommon.FormFlow;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class JourneyAttribute : Attribute
{
    public JourneyAttribute(string journeyName)
    {
        JourneyName = journeyName ?? throw new ArgumentNullException(nameof(journeyName));
    }

    public string JourneyName { get; }
}
