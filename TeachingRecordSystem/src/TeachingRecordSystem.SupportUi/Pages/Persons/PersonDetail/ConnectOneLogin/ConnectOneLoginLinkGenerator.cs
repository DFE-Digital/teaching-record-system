namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

public class ConnectOneLoginLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid personId) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/ConnectOneLogin/Index", routeValues: new { personId });

    public string Match(Guid personId, string subject) =>
        linkGenerator.GetRequiredPathByPage("/Persons/PersonDetail/ConnectOneLogin/Match", routeValues: new { personId, subject });
}
