using System.Text.Json;
using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.UiCommon.FormFlow.State;

public class JsonStateSerializer(IOptions<JsonOptions> jsonOptionsAccessor) : IStateSerializer
{
    public object Deserialize(Type type, string serialized) =>
        JsonSerializer.Deserialize(serialized, type, jsonOptionsAccessor.Value.JsonSerializerOptions) ??
            throw new InvalidOperationException("Data is empty.");

    public string Serialize(Type type, object state) =>
        JsonSerializer.Serialize(state, type, jsonOptionsAccessor.Value.JsonSerializerOptions);
}
