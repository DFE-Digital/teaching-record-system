namespace TeachingRecordSystem.SupportUi.Pages.ApiKeys.EditApiKey;

public class EditApiKeyLinkGenerator(LinkGenerator linkGenerator)
{
    public string Index(Guid apiKeyId) =>
        linkGenerator.GetRequiredPathByPage("/ApiKeys/EditApiKey/Index", routeValues: new { apiKeyId });

    public string Expire(Guid apiKeyId) =>
        linkGenerator.GetRequiredPathByPage("/ApiKeys/EditApiKey/Index", handler: "Expire", routeValues: new { apiKeyId });
}
