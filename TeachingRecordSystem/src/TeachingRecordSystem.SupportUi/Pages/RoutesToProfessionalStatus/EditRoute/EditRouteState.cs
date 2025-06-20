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

    public EditRouteStatusState? EditStatusState { get; set; } // store temp data while completing a route (moving it to 'holds')

    public QualificationType? QualificationType { get; set; }
    public Guid RouteToProfessionalStatusId { get; set; }
    public RouteToProfessionalStatusStatus CurrentStatus { get; set; }
    public RouteToProfessionalStatusStatus Status { get; set; }
    public DateOnly? HoldsFrom { get; set; }
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
    public bool IsCompletingRoute => EditStatusState != null; // status page initialises EditStatusState when the status is set to 'holds'

    public void EnsureInitialized(RouteToProfessionalStatus routeToProfessionalStatus)
    {
        if (Initialized)
        {
            return;
        }

        QualificationType = routeToProfessionalStatus.QualificationType;
        RouteToProfessionalStatusId = routeToProfessionalStatus.RouteToProfessionalStatusTypeId;
        CurrentStatus = routeToProfessionalStatus.Status;
        Status = routeToProfessionalStatus.Status;
        HoldsFrom = routeToProfessionalStatus.HoldsFrom;
        TrainingStartDate = routeToProfessionalStatus.TrainingStartDate;
        TrainingEndDate = routeToProfessionalStatus.TrainingEndDate;
        TrainingSubjectIds = routeToProfessionalStatus.TrainingSubjectIds;
        TrainingAgeSpecialismType = routeToProfessionalStatus.TrainingAgeSpecialismType;
        TrainingAgeSpecialismRangeFrom = routeToProfessionalStatus.TrainingAgeSpecialismRangeFrom;
        TrainingAgeSpecialismRangeTo = routeToProfessionalStatus.TrainingAgeSpecialismRangeTo;
        TrainingCountryId = routeToProfessionalStatus.TrainingCountryId;
        TrainingProviderId = routeToProfessionalStatus.TrainingProviderId;
        IsExemptFromInduction = routeToProfessionalStatus.ExemptFromInduction;
        DegreeTypeId = routeToProfessionalStatus.DegreeTypeId;
        Initialized = true;
    }
}
