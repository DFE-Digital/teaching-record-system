using Microsoft.Xrm.Sdk;

namespace DqtApi.DataStore.Crm
{
    internal static class FormattedValueCollectionExtensions
    {
        public static string ValueOrNull(this FormattedValueCollection collection, string key) =>
            collection.Contains(key) ? collection[key] : null;
    }
}
