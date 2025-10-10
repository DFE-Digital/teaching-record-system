namespace TeachingRecordSystem.SupportUi.Pages.ApiKeys.AddApiKey;

public class AddApiKeyLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid applicationUserId) =>
        linkGenerator.GetRequiredPathByPage("/ApiKeys/Index", routeValues: new { applicationUserId });
}
