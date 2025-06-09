namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

public class AddRouteState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.AddRouteToProfessionalStatus,
        typeof(AddRouteState),
        requestDataKeys: ["personId"],
        appendUniqueKey: true);

    public bool Initialized { get; set; }

    public Guid? RouteToProfessionalStatusId { get; set; }
    public ProfessionalStatusStatus? Status { get; set; }
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

    public void EnsureInitialized(CurrentProfessionalStatusFeature professionalStatusInfo)
    {
        if (Initialized)
        {
            return;
        }

        RouteToProfessionalStatusId = professionalStatusInfo.RouteToProfessionalStatus.RouteToProfessionalStatusTypeId;
        Status = professionalStatusInfo.RouteToProfessionalStatus.Status;
        AwardedDate = professionalStatusInfo.RouteToProfessionalStatus.AwardedDate;
        TrainingStartDate = professionalStatusInfo.RouteToProfessionalStatus.TrainingStartDate;
        TrainingEndDate = professionalStatusInfo.RouteToProfessionalStatus.TrainingEndDate;
        TrainingSubjectIds = professionalStatusInfo.RouteToProfessionalStatus.TrainingSubjectIds;
        TrainingAgeSpecialismType = professionalStatusInfo.RouteToProfessionalStatus.TrainingAgeSpecialismType;
        TrainingAgeSpecialismRangeFrom = professionalStatusInfo.RouteToProfessionalStatus.TrainingAgeSpecialismRangeFrom;
        TrainingAgeSpecialismRangeTo = professionalStatusInfo.RouteToProfessionalStatus.TrainingAgeSpecialismRangeTo;
        TrainingCountryId = professionalStatusInfo.RouteToProfessionalStatus.TrainingCountryId;
        TrainingProviderId = professionalStatusInfo.RouteToProfessionalStatus.TrainingProviderId;
        IsExemptFromInduction = professionalStatusInfo.RouteToProfessionalStatus.ExemptFromInduction;
        DegreeTypeId = professionalStatusInfo.RouteToProfessionalStatus.DegreeTypeId;
        Initialized = true;
    }
}
