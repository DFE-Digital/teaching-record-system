using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

public class RequestTrnJourneyState()
{
    public const string JourneyName = "RequestTrnJourney";

    public static JourneyDescriptor JourneyDescriptor { get; } =
        new JourneyDescriptor(JourneyName, typeof(RequestTrnJourneyState), requestDataKeys: [], appendUniqueKey: true);

    public string? Email { get; set; }
    public string? Name { get; set; }
    public bool? HasPreviousName { get; set; }
    public string? PreviousName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
}
