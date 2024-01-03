using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

public class EditMqProviderState
{
    public bool Initialized { get; set; }

    public string? CurrentMqEstablishmentName { get; set; }

    public string? MqEstablishmentValue { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(MqEstablishmentValue))]
    public bool IsComplete => !string.IsNullOrWhiteSpace(MqEstablishmentValue);

    public void EnsureInitialized(CurrentMandatoryQualificationFeature qualificationInfo)
    {
        if (Initialized)
        {
            return;
        }

        CurrentMqEstablishmentName = qualificationInfo.DqtEstablishmentName;
        MqEstablishmentValue = qualificationInfo.DqtEstablishmentValue;
        Initialized = true;
    }
}
