using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

public class EditMqResultState
{
    public bool Initialized { get; set; }

    public MandatoryQualificationStatus? CurrentStatus { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? CurrentEndDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [JsonIgnore]
    public bool IsComplete => Status.HasValue &&
        (Status != MandatoryQualificationStatus.Passed || Status == MandatoryQualificationStatus.Passed && EndDate.HasValue);

    public void EnsureInitialized(CurrentMandatoryQualificationFeature qualificationInfo)
    {
        if (Initialized)
        {
            return;
        }

        Status = CurrentStatus = qualificationInfo.MandatoryQualification.Status;
        EndDate = CurrentEndDate = qualificationInfo.MandatoryQualification.EndDate;
        Initialized = true;
    }
}
