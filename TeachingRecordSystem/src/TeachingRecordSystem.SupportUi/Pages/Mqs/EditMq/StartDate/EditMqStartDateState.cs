using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

public class EditMqStartDateState
{
    public bool Initialized { get; set; }

    public DateOnly? CurrentStartDate { get; set; }

    public DateOnly? StartDate { get; set; }

    [JsonIgnore]
    [MemberNotNullWhen(true, nameof(StartDate))]
    public bool IsComplete => StartDate is not null;

    public void EnsureInitialized(CurrentMandatoryQualificationFeature qualificationInfo)
    {
        if (Initialized)
        {
            return;
        }

        StartDate = CurrentStartDate = qualificationInfo.MandatoryQualification.StartDate;
        Initialized = true;
    }
}
