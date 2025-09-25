using System.Text.Json;
using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.Models.SupportTasks;

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
#pragma warning disable CS0618 // Type or member is obsolete
        get => JsonSerializer.Deserialize(_data, SupportTaskType.GetDataType(), DataStore.Postgres.Models.SupportTask.SerializerOptions)!;
#pragma warning restore CS0618 // Type or member is obsolete
        init => _data = JsonSerializer.SerializeToDocument(value, SupportTaskType.GetDataType(), ISupportTaskData.SerializerOptions);
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
