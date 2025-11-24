#nullable disable
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.Api.V1.Responses;

public class QualifiedTeacherStatus
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("state")]
    public dfeta_qtsregistrationState State { get; set; }

    [JsonPropertyName("state_name")]
    public string StateName { get; set; }

    [JsonPropertyName("qts_date")]
    public DateTime? QtsDate { get; set; }
}

#pragma warning disable CA1707
public enum dfeta_qtsregistrationState
#pragma warning restore CA1707
{
    Active = 0
}
