namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

public class ResolveTeacherPensionsPotentialDuplicateLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Resolve/Index", routeValues: new { supportTaskReference });

    public string Matches(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Resolve/Matches", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MatchesCancel(string supportTaskReference, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Resolve/Matches", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string Merge(string supportTaskReference, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Resolve/Merge", routeValues: new { supportTaskReference, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string MergeCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Resolve/Merge", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string KeepRecordSeparate(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Resolve/KeepRecordSeparate", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string KeepRecordSeparateCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Resolve/KeepRecordSeparate", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string ConfirmKeepRecordSeparateReason(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Resolve/ConfirmKeepRecordSeparateReason", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string ConfirmKeepRecordSeparateReasonCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Resolve/ConfirmKeepRecordSeparateReason", handler: "Cancel", routeValues: new { supportTaskReference }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool keepRecordSeparate = false) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Resolve/CheckAnswers", routeValues: new { supportTaskReference, keepRecordSeparate }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(string supportTaskReference, JourneyInstanceId? journeyInstanceId = null, bool keepRecordSeparate = false) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Resolve/CheckAnswers", handler: "Cancel", routeValues: new { supportTaskReference, keepRecordSeparate }, journeyInstanceId: journeyInstanceId);
}
