namespace TeachingRecordSystem.WebCommon.Tests.FormFlow;

internal static class RouteValueDictionaryExtensions
{
    public static void AddRange(
        this RouteValueDictionary routeValueDictionary,
        RouteValueDictionary collection)
    {
        foreach (var kvp in collection)
        {
            routeValueDictionary.Add(kvp.Key, kvp.Value);
        }
    }
}
