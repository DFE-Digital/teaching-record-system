using System.Text.Json;

namespace TeachingRecordSystem.WebCommon.FormFlow.State;

public class JsonOptions
{
    public JsonSerializerOptions JsonSerializerOptions { get; } = new JsonSerializerOptions()
    {
        WriteIndented = false
    };
}
