using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

public class EditMqProviderState
{
    public bool Initialized { get; set; }

    public Guid? CurrentProviderId { get; set; }

    public Guid? ProviderId { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(ProviderId))]
    public bool IsComplete => ProviderId.HasValue;

    public void EnsureInitialized(CurrentMandatoryQualificationFeature qualificationInfo)
    {
        if (Initialized)
        {
            return;
        }

        ProviderId = CurrentProviderId = qualificationInfo.MandatoryQualification.ProviderId;
        Initialized = true;
    }
}
