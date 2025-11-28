namespace TeachingRecordSystem.SupportUi.Tests;

public static class FormFlowExtensions
{
    public static string GetUniqueIdQueryParameter(this JourneyInstance journeyInstance) =>
        journeyInstance.InstanceId.UniqueKey is string uniqueKey ?
            $"{Constants.UniqueKeyQueryParameterName}={Uri.EscapeDataString(uniqueKey)}" :
            string.Empty;

    public static string GetUniqueIdQueryParameter(this JourneyInstanceId instanceId) =>
        instanceId.UniqueKey is string uniqueKey ?
            $"{Constants.UniqueKeyQueryParameterName}={Uri.EscapeDataString(uniqueKey)}" :
            string.Empty;
}
