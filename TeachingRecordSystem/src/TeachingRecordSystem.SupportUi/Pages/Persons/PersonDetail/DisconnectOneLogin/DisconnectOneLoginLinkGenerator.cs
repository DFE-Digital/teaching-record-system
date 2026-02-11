namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

public class DisconnectOneLoginLinkGenerator(LinkGenerator linkGenerator)
{
    public string DisconnectOneLoginSubject(Guid personId, string subject, JourneyInstanceId? journeyInstanceId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/DisconnectOneLogin/Index", routeValues: new { personId }, journeyInstanceId: journeyInstanceId);
}
