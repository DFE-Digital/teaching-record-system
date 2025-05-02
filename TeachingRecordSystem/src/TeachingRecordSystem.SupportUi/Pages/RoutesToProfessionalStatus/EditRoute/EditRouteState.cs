using System.Text.Json.Serialization;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

public class EditRouteState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditRouteToProfessionalStatus,
        typeof(EditRouteState),
        requestDataKeys: ["qualificationId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public EditRouteStatusState? EditStatusState { get; set; } // store temp data while completing a route (moving it to awarded or approved)

    public QualificationType? QualificationType { get; set; }
    public Guid RouteToProfessionalStatusId { get; set; }
    public ProfessionalStatusStatus CurrentStatus { get; set; }
    public ProfessionalStatusStatus Status { get; set; }
    public DateOnly? AwardedDate { get; set; }
    public DateOnly? TrainingStartDate { get; set; }
    public DateOnly? TrainingEndDate { get; set; }
    public Guid[] TrainingSubjectIds { get; set; } = [];
    public TrainingAgeSpecialismType? TrainingAgeSpecialismType { get; set; }
    public int? TrainingAgeSpecialismRangeFrom { get; set; }
    public int? TrainingAgeSpecialismRangeTo { get; set; }
    public string? TrainingCountryId { get; set; }
    public Guid? TrainingProviderId { get; set; }
    public bool? IsExemptFromInduction { get; set; }
    public Guid? DegreeTypeId { get; set; }

    public ChangeReasonOption? ChangeReason { get; set; }
    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();

    [JsonIgnore]
    public bool ChangeReasonIsComplete => ChangeReason is not null && ChangeReasonDetail is not null && ChangeReasonDetail.IsComplete;

    [JsonIgnore]
    public bool IsCompletingRoute => EditStatusState != null; // status page initialises EditStatusState when the status is set to awarded / approved 

    public void EnsureInitialized(ProfessionalStatus professionalStatus)
    {
        if (Initialized)
        {
            return;
        }

        QualificationType = professionalStatus.QualificationType;
        RouteToProfessionalStatusId = professionalStatus.RouteToProfessionalStatusId;
        CurrentStatus = professionalStatus.Status;
        Status = professionalStatus.Status;
        AwardedDate = professionalStatus.AwardedDate;
        TrainingStartDate = professionalStatus.TrainingStartDate;
        TrainingEndDate = professionalStatus.TrainingEndDate;
        TrainingSubjectIds = professionalStatus.TrainingSubjectIds;
        TrainingAgeSpecialismType = professionalStatus.TrainingAgeSpecialismType;
        TrainingAgeSpecialismRangeFrom = professionalStatus.TrainingAgeSpecialismRangeFrom;
        TrainingAgeSpecialismRangeTo = professionalStatus.TrainingAgeSpecialismRangeTo;
        TrainingCountryId = professionalStatus.TrainingCountryId;
        TrainingProviderId = professionalStatus.TrainingProviderId;
        IsExemptFromInduction = professionalStatus.ExemptFromInduction;
        DegreeTypeId = professionalStatus.DegreeTypeId;
        Initialized = true;
    }
}
