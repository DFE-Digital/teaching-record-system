using System;
using System.Text.Json.Serialization;
using QualifiedTeachersApi.DataStore.Crm.Models;

namespace QualifiedTeachersApi.V1.Responses;

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
