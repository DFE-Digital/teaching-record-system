namespace TeachingRecordSystem.SupportUi.Pages.ApiKeys.AddApiKey;

public class AddApiKeyLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid applicationUserId) =>
        linkGenerator.GetRequiredPathByPage("/ApiKeys/AddApiKey/Index", routeValues: new { applicationUserId });
}
