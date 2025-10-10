namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;

public class RejectNpqTrnRequestLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Reject/Index", routeValues: new { supportTaskReference });

    public string Reason(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Reject/Reason", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ReasonCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Reject/Reason", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Reject/CheckAnswers", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/NpqTrnRequests/Reject/CheckAnswers", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);
}
