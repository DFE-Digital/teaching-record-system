namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;

public class RejectNpqTrnRequestState : IRegisterJourney
{
    public static JourneyDescriptor Journey { get; } = new(
        JourneyNames.RejectNpqTrnRequest,
        typeof(RejectNpqTrnRequestState),
        ["supportTaskReference"],
        appendUniqueKey: true);

    public RejectionReasonOption? RejectionReason { get; set; }
}
