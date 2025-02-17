namespace TeachingRecordSystem.SupportUi.Pages.Routes.EditRoute;

public class EditRouteState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new (
        JourneyNames.EditRouteToProfessionalStatus,
        typeof(EditRouteState),
        requestDataKeys: ["qualificationId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public QualificationType QualificationType { get; set; }
    public Guid RouteToProfessionalStatusId { get; set; }
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
    public Guid? InductionExemptionReasonId { get; set; }

    public ChangeReasonState ChangeReasonDetail { get; set; } = new();

    public void EnsureInitialized(CurrentProfessionalStatusFeature professionalStatusInfo)
    {
        if (Initialized)
        {
            return;
        }

        QualificationType = professionalStatusInfo.ProfessionalStatus.QualificationType;
        RouteToProfessionalStatusId = professionalStatusInfo.ProfessionalStatus.RouteToProfessionalStatusId;
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
        InductionExemptionReasonId = professionalStatusInfo.ProfessionalStatus.InductionExemptionReasonId;

        Initialized = true;
    }
}
