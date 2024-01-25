using System.Text.Json;

namespace FormFlow.State;

public class JsonOptions
{
    public JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions()
    {
        WriteIndented = false
    };
}
