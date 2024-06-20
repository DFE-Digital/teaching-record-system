namespace TeachingRecordSystem.UiCommon.FormFlow;

public class JourneyRegistry
{
    private readonly object _gate = new();
    private readonly Dictionary<string, JourneyDescriptor> _journeys = new(StringComparer.OrdinalIgnoreCase);

    public JourneyDescriptor? GetJourneyByName(string journeyName)
    {
        ArgumentNullException.ThrowIfNull(journeyName);

        lock (_gate)
        {
            return _journeys.GetValueOrDefault(journeyName);
        }
    }

    public void RegisterJourney(JourneyDescriptor journey)
    {
        ArgumentNullException.ThrowIfNull(journey);

        lock (_gate)
        {
            _journeys.Add(journey.JourneyName, journey);
        }
    }
}
