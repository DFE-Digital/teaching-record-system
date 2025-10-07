namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string AddApiKey(Guid applicationUserId) => GetRequiredPathByPage("/ApiKeys/AddApiKey/Index", routeValues: new { applicationUserId });

    public string EditApiKey(Guid apiKeyId) => GetRequiredPathByPage("/ApiKeys/EditApiKey/Index", routeValues: new { apiKeyId });

    public string ExpireApiKey(Guid apiKeyId) => GetRequiredPathByPage("/ApiKeys/EditApiKey/Index", handler: "Expire", routeValues: new { apiKeyId });
}
