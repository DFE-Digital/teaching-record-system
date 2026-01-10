using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.Core;

[JsonConverter(typeof(SavedJourneyStateJsonConverter))]
public class SavedJourneyState(
    string pageName,
    IReadOnlyDictionary<string, string?> modelStateValues,
    object state,
    Type stateType)
{
    internal static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web);

    public string PageName => pageName;

    public IReadOnlyDictionary<string, string?> ModelStateValues => modelStateValues;

    public T GetState<T>() => (T)state;

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
