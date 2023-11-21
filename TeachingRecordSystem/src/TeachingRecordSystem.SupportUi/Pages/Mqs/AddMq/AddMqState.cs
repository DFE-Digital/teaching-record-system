using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

public class AddMqState
{
    public string? MqEstablishmentValue { get; set; }

    public string? SpecialismValue { get; set; }

    public DateOnly? StartDate { get; set; }

    public dfeta_qualification_dfeta_MQ_Status? Result { get; set; }

    public DateOnly? EndDate { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(MqEstablishmentValue), nameof(SpecialismValue), nameof(StartDate), nameof(Result))]
    public bool IsComplete => !string.IsNullOrWhiteSpace(MqEstablishmentValue) &&
        !string.IsNullOrEmpty(SpecialismValue) &&
        StartDate.HasValue &&
        Result.HasValue &&
        (Result!.Value != dfeta_qualification_dfeta_MQ_Status.Passed || (Result.Value == dfeta_qualification_dfeta_MQ_Status.Passed && EndDate.HasValue));
}
