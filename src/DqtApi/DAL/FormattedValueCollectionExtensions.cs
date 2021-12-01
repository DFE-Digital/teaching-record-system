using Microsoft.Xrm.Sdk;

namespace DqtApi.DAL
{
    internal static class FormattedValueCollectionExtensions
    {
        public static string ValueOrNull(this FormattedValueCollection collection, string key) =>
            collection.Contains(key) ? collection[key] : null;
    }
}
