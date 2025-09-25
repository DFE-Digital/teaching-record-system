using System.Text.Json;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.Core.Events.Models;

public record SupportTask
{
    [JsonInclude]
    [JsonPropertyName("Data")]
    private JsonDocument _data = null!;

    public required string SupportTaskReference { get; init; }
    public required SupportTaskType SupportTaskType { get; init; }
    public required SupportTaskStatus Status { get; init; }
    public required string? OneLoginUserSubject { get; init; }
    public required Guid? PersonId { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public object Data
    {
        get => JsonSerializer.Deserialize(_data, DataStore.Postgres.Models.SupportTask.GetDataType(SupportTaskType), DataStore.Postgres.Models.SupportTask.SerializerOptions)!;
        init => _data = JsonSerializer.SerializeToDocument(value, DataStore.Postgres.Models.SupportTask.GetDataType(SupportTaskType), DataStore.Postgres.Models.SupportTask.SerializerOptions);
    }

    public static SupportTask FromModel(DataStore.Postgres.Models.SupportTask model) => new()
    {
        SupportTaskReference = model.SupportTaskReference,
        SupportTaskType = model.SupportTaskType,
        Status = model.Status,
        OneLoginUserSubject = model.OneLoginUserSubject,
        PersonId = model.PersonId,
        Data = model.Data
    };
}
