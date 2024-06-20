using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace TeachingRecordSystem.UiCommon.FormFlow;

[DebuggerDisplay("{Serialize(),nq}")]
public readonly struct JourneyInstanceId(string journeyName, IReadOnlyDictionary<string, StringValues> keys) : IEquatable<JourneyInstanceId>
{
    public string JourneyName { get; } = journeyName;

    public IReadOnlyDictionary<string, StringValues> Keys { get; } = keys;

    public string? UniqueKey => Keys.GetValueOrDefault(Constants.UniqueKeyQueryParameterName);

    public static JourneyInstanceId Deserialize(string serialized)
    {
        var parts = serialized.Split('?');
        var journeyName = parts[0];
        var qs = parts[1];

        var keys = QueryHelpers.ParseQuery(qs);

        return new JourneyInstanceId(journeyName, keys);
    }

    public string Serialize()
    {
        var urlEncoder = UrlEncoder.Default;

        var url = urlEncoder.Encode(JourneyName);

        foreach (var kvp in Keys)
        {
            var value = kvp.Value;

            foreach (var sv in value)
            {
                if (sv is not null)
                {
                    url = QueryHelpers.AddQueryString(url, kvp.Key, sv);
                }
            }
        }

        return url;
    }

    public static JourneyInstanceId Create(JourneyDescriptor journeyDescriptor, IValueProvider valueProvider)
    {
        var instanceKeysBuilder = new KeysBuilder();

        foreach (var key in journeyDescriptor.RequestDataKeys)
        {
            var keyIsOptional = IsKeyOptional(key, out var normalizedKey);

            var keyValueProviderResult = valueProvider.GetValue(normalizedKey);

            if (keyValueProviderResult.Length == 0)
            {
                if (!keyIsOptional)
                {
                    throw new InvalidOperationException($"Cannot resolve '{key}' from request.");
                }
                else
                {
                    continue;
                }
            }

            instanceKeysBuilder.With(normalizedKey, keyValueProviderResult.Values);
        }

        if (journeyDescriptor.AppendUniqueKey)
        {
            var uniqueKey = Guid.NewGuid().ToString();

            instanceKeysBuilder.With(Constants.UniqueKeyQueryParameterName, uniqueKey);
        }

        return new JourneyInstanceId(journeyDescriptor.JourneyName, instanceKeysBuilder.Build());
    }

    public static bool TryResolve(
        JourneyDescriptor journeyDescriptor,
        IValueProvider valueProvider,
        out JourneyInstanceId instanceId)
    {
        var instanceKeysBuilder = new KeysBuilder();

        foreach (var key in journeyDescriptor.RequestDataKeys)
        {
            var keyIsOptional = IsKeyOptional(key, out var normalizedKey);

            var keyValueProviderResult = valueProvider.GetValue(normalizedKey);

            if (keyValueProviderResult.Length == 0)
            {
                if (!keyIsOptional)
                {
                    instanceId = default;
                    return false;
                }
                else
                {
                    continue;
                }
            }

            instanceKeysBuilder.With(normalizedKey, keyValueProviderResult.Values);
        }

        if (journeyDescriptor.AppendUniqueKey)
        {
            var uniqueKeyValueProviderResult = valueProvider.GetValue(Constants.UniqueKeyQueryParameterName);

            if (uniqueKeyValueProviderResult.Length == 0)
            {
                instanceId = default;
                return false;
            }

            instanceKeysBuilder.With(
                Constants.UniqueKeyQueryParameterName,
                uniqueKeyValueProviderResult.FirstValue!);
        }

        instanceId = new JourneyInstanceId(journeyDescriptor.JourneyName, instanceKeysBuilder.Build());
        return true;
    }

    public bool Equals([AllowNull] JourneyInstanceId other) =>
        JourneyName == other.JourneyName && Keys.SequenceEqual(other.Keys);

    public override bool Equals(object? obj) => obj is JourneyInstanceId x && x.Equals(this);

    public override int GetHashCode() => HashCode.Combine(JourneyName, Keys);

    public override string ToString() => Serialize();

    public static bool operator ==(JourneyInstanceId left, JourneyInstanceId right) => left.Equals(right);

    public static bool operator !=(JourneyInstanceId left, JourneyInstanceId right) => !(left == right);

    public static implicit operator string(JourneyInstanceId instanceId) => instanceId.ToString();

    private static bool IsKeyOptional(string key, out string normalizedKey)
    {
        if (key.EndsWith("?"))
        {
            normalizedKey = key[0..^1];
            return true;
        }
        else
        {
            normalizedKey = key;
            return false;
        }
    }
}
