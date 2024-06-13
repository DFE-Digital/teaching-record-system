#nullable disable
using Microsoft.Xrm.Sdk;

namespace TeachingRecordSystem.Core.Dqt;

internal static class DataCollectionExtensions
{
    public static U MapCollection<T, U>(this U sourceCollection, Func<KeyValuePair<string, T>, T> getValue, string prefix)
        where U : DataCollection<string, T>, new()
    {
        var destinationCollection = new U();

        foreach (var keyValuePair in sourceCollection.Where(kvp => kvp.Key.StartsWith(prefix + ".")))
        {
            var newKey = keyValuePair.Key.Remove(0, prefix.Length + 1);
            destinationCollection.Add(newKey, getValue(keyValuePair));
        }

        return destinationCollection;
    }
}
