#nullable disable
using Microsoft.Xrm.Sdk;

namespace TeachingRecordSystem.Core.Dqt;

public static class FormattedValueCollectionExtensions
{
    public static string ValueOrNull(this FormattedValueCollection collection, string key) =>
        collection.Contains(key) ? collection[key] : null;
}
