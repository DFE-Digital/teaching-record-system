namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator
{
    public string AddApiKey(Guid applicationUserId) => GetRequiredPathByPage("/ApiKeys/AddApiKey", routeValues: new { applicationUserId });

    public string EditApiKey(Guid apiKeyId) => GetRequiredPathByPage("/ApiKeys/EditApiKey", routeValues: new { apiKeyId });

    public string ExpireApiKey(Guid apiKeyId) => GetRequiredPathByPage("/ApiKeys/EditApiKey", handler: "Expire", routeValues: new { apiKeyId });
}
