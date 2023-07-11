#nullable disable
using System.Text.Json.Serialization;
using TeachingRecordSystem.Dqt.Models;

namespace TeachingRecordSystem.Api.V1.Responses;

public class Induction
{
    [JsonPropertyName("start_date")]
    public DateTime? StartDate { get; set; }

    [JsonPropertyName("completion_date")]
    public DateTime? CompletionDate { get; set; }

    [JsonPropertyName("status")]
    public string InductionStatusName { get; set; }

    [JsonPropertyName("state")]
    public dfeta_inductionState State { get; set; }

    [JsonPropertyName("state_name")]
    public string StateName { get; set; }
}
