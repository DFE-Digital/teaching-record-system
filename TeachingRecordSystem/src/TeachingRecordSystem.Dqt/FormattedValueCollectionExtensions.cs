using Microsoft.Xrm.Sdk;

namespace TeachingRecordSystem.Dqt;

public static class FormattedValueCollectionExtensions
{
    public static string ValueOrNull(this FormattedValueCollection collection, string key) =>
        collection.Contains(key) ? collection[key] : null;
}
