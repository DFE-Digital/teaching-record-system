namespace TeachingRecordSystem.Core;

public static class CacheKeys
{
    public static object PersonInfo(Guid personId) => $"person_info:{personId}";

    public static object EnabledWebhookEndpoints() => "webhook_endpoints";
}
