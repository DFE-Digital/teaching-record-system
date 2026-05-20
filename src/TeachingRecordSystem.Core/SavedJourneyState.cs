using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.Core;

[JsonConverter(typeof(SavedJourneyStateJsonConverter))]
public class SavedJourneyState(
    string pageName,
    IReadOnlyDictionary<string, string?> modelStateValues,
    object state,
    Type stateType) : IEquatable<SavedJourneyState>
{
    internal static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web);

    public string PageName => pageName;

    public IReadOnlyDictionary<string, string?> ModelStateValues => modelStateValues;

    public T GetState<T>() => (T)state;

    private object State => state;

    public bool Equals(SavedJourneyState? other)
    {
        if (other is null)
        {
            return false;
        }

        if (!string.Equals(PageName, other.PageName, StringComparison.Ordinal))
        {
            return false;
        }

        if (ModelStateValues.Count != other.ModelStateValues.Count)
        {
            return false;
        }

        if (!ModelStateValues.All(kvp =>
            other.ModelStateValues.TryGetValue(kvp.Key, out var otherValue) &&
            string.Equals(kvp.Value, otherValue, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return State.Equals(other.State);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((SavedJourneyState)obj);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(PageName, StringComparer.Ordinal);
        hashCode.Add(State);

        foreach (var kvp in ModelStateValues.OrderBy(kvp => kvp.Key, StringComparer.Ordinal))
        {
            hashCode.Add(kvp.Key, StringComparer.Ordinal);
            hashCode.Add(kvp.Value, StringComparer.Ordinal);
        }

        return hashCode.ToHashCode();
    }

    internal static SavedJourneyState? ReadJson(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var deserialized = JsonSerializer.Deserialize<SerializedSavedJourneyState>(ref reader, options);

        if (deserialized is null)
        {
            return null;
        }

        var stateType = Type.GetType(deserialized.StateTypeName)!;
        var state = deserialized.State.Deserialize(stateType, options)!;

        return new SavedJourneyState(
            deserialized.PageName,
            deserialized.ModelStateValues,
            state,
            stateType);
    }

    internal void WriteJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        var serializedState = JsonSerializer.SerializeToElement(state, stateType, options);

        var serializedSavedJourneyState = new SerializedSavedJourneyState(
            PageName,
            new Dictionary<string, string?>(ModelStateValues),
            serializedState,
            stateType.AssemblyQualifiedName!);

        JsonSerializer.Serialize(writer, serializedSavedJourneyState);
    }

    private sealed record SerializedSavedJourneyState(
        string PageName,
        Dictionary<string, string?> ModelStateValues,
        JsonElement State,
        string StateTypeName);
}

internal class SavedJourneyStateJsonConverter : JsonConverter<SavedJourneyState>
{
    public override SavedJourneyState? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return SavedJourneyState.ReadJson(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, SavedJourneyState value, JsonSerializerOptions options)
    {
        value.WriteJson(writer, options);
    }
}
