namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests;

public class ResolveApiTrnRequestState : IRegisterJourney
{
    public static Guid CreateNewRecordPersonIdSentinel => Guid.Empty;

    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.ResolveApiTrnRequest,
        typeof(ResolveApiTrnRequestState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public Guid? PersonId { get; set; }
}
