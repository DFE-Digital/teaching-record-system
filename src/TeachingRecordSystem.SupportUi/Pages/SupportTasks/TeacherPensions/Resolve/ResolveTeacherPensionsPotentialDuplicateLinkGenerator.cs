namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.TeacherPensions.Resolve;

public class ResolveTeacherPensionsPotentialDuplicateLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(string supportTaskReference) =>
        linkGenerator.GetRequiredPathByPage("/SupportTasks/TeacherPensions/Resolve/Index", routeValues: new { supportTaskReference });

    public string Matches(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/SupportTasks/TeacherPensions/Resolve/Matches", journeyInstanceId, returnUrl);

    public string Merge(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/SupportTasks/TeacherPensions/Resolve/Merge", journeyInstanceId, returnUrl);

    public string KeepRecordSeparate(JourneyInstanceId journeyInstanceId, string? returnUrl = null) =>
        linkGenerator.GetJourneyPage("/SupportTasks/TeacherPensions/Resolve/KeepRecordSeparate", journeyInstanceId, returnUrl);

    public string ConfirmKeepRecordSeparateReason(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/SupportTasks/TeacherPensions/Resolve/ConfirmKeepRecordSeparateReason", journeyInstanceId);

    public string CheckAnswers(JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetJourneyPage("/SupportTasks/TeacherPensions/Resolve/CheckAnswers", journeyInstanceId);
}
