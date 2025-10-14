namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

public class AddMqLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/AddMq/Index", routeValues: new { personId });

    public string Provider(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/AddMq/Provider", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string ProviderCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/AddMq/Provider", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Specialism(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/AddMq/Specialism", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string SpecialismCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/AddMq/Specialism", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string StartDate(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/AddMq/StartDate", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string StartDateCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/AddMq/StartDate", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string Status(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/AddMq/Status", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string StatusCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/AddMq/Status", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswers(Guid personId, JourneyInstanceId journeyInstanceId, bool? fromCheckAnswers = null) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/AddMq/CheckAnswers", routeValues: new { personId, fromCheckAnswers }, journeyInstanceId: journeyInstanceId);

    public string CheckAnswersCancel(Guid personId, JourneyInstanceId journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Mqs/AddMq/CheckAnswers", "cancel", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);

}
