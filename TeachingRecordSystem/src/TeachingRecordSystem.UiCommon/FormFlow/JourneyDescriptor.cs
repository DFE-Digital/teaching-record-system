namespace TeachingRecordSystem.UiCommon.FormFlow;

public class JourneyDescriptor(
    string journeyName,
    Type stateType,
    IEnumerable<string> requestDataKeys,
    bool appendUniqueKey)
{
    public bool AppendUniqueKey { get; } = appendUniqueKey;

    public string JourneyName { get; } = journeyName;

    public IReadOnlyCollection<string> RequestDataKeys { get; } = requestDataKeys?.ToArray() ?? [];

    public Type StateType { get; } = stateType;
}
