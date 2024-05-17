using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

public class RequestTrnJourneyState
{
    public const string JourneyName = "RequestTrnJourney";

    public static JourneyDescriptor JourneyDescriptor { get; } =
    new JourneyDescriptor(JourneyName, typeof(RequestTrnJourneyState), requestDataKeys: [], appendUniqueKey: true);
}
