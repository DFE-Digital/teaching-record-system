using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

public class EditMqSpecialismState
{
    public bool Initialized { get; set; }

    public MandatoryQualificationSpecialism? CurrentSpecialism { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(Specialism))]
    public bool IsComplete => Specialism is not null;

    public void EnsureInitialized(CurrentMandatoryQualificationFeature qualificationInfo)
    {
        if (Initialized)
        {
            return;
        }

        Specialism = CurrentSpecialism = qualificationInfo.MandatoryQualification.Specialism;
        Initialized = true;
    }
}
