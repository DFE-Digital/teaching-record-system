using System.Text.Json.Serialization;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

public class EditRouteState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.EditRouteToProfessionalStatus,
        typeof(EditRouteState),
        requestDataKeys: ["qualificationId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

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
    public bool IsComplete => ChangeReason is not null && ChangeReasonDetail is not null && ChangeReasonDetail.IsComplete; // CML TODO - required fields also depends on the route and status

    [JsonIgnore]
    public bool StatusAwardedOrApprovedJourney => Status == ProfessionalStatusStatus.Awarded && CurrentStatus != ProfessionalStatusStatus.Awarded
        || Status == ProfessionalStatusStatus.Approved && CurrentStatus != ProfessionalStatusStatus.Approved;

    public void EnsureInitialized(CurrentProfessionalStatusFeature professionalStatusInfo)
    {
        if (Initialized)
        {
            return;
        }

        QualificationType = professionalStatusInfo.ProfessionalStatus.QualificationType;
        RouteToProfessionalStatusId = professionalStatusInfo.ProfessionalStatus.RouteToProfessionalStatusId;
        CurrentStatus = professionalStatusInfo.ProfessionalStatus.Status;
        Status = professionalStatusInfo.ProfessionalStatus.Status;
        AwardedDate = professionalStatusInfo.ProfessionalStatus.AwardedDate;
        TrainingStartDate = professionalStatusInfo.ProfessionalStatus.TrainingStartDate;
        TrainingEndDate = professionalStatusInfo.ProfessionalStatus.TrainingEndDate;
        TrainingSubjectIds = professionalStatusInfo.ProfessionalStatus.TrainingSubjectIds;
        TrainingAgeSpecialismType = professionalStatusInfo.ProfessionalStatus.TrainingAgeSpecialismType;
        TrainingAgeSpecialismRangeFrom = professionalStatusInfo.ProfessionalStatus.TrainingAgeSpecialismRangeFrom;
        TrainingAgeSpecialismRangeTo = professionalStatusInfo.ProfessionalStatus.TrainingAgeSpecialismRangeTo;
        TrainingCountryId = professionalStatusInfo.ProfessionalStatus.TrainingCountryId;
        TrainingProviderId = professionalStatusInfo.ProfessionalStatus.TrainingProviderId;
        IsExemptFromInduction = professionalStatusInfo.ProfessionalStatus.ExemptFromInduction;
        DegreeTypeId = professionalStatusInfo.ProfessionalStatus.DegreeTypeId;
        Initialized = true;
    }
}
