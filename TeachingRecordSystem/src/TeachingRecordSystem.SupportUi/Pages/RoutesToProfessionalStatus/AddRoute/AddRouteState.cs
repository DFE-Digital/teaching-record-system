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
    public RouteToProfessionalStatusStatus? Status { get; set; }
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
    public ChangeReasonDetailsState ChangeReasonDetail { get; init; } = new();

    public bool IsComplete(AddRoutePage page)
    {
        return page switch
        {
            AddRoutePage.Route =>
                RouteToProfessionalStatusId != null,
            AddRoutePage.Status =>
                Status != null,
            AddRoutePage.StartAndEndDate =>
                TrainingStartDate != null ||
                TrainingEndDate != null,
            AddRoutePage.HoldsFrom =>
                HoldsFrom != null,
            AddRoutePage.InductionExemption =>
                IsExemptFromInduction != null,
            AddRoutePage.TrainingProvider =>
                TrainingProviderId != null,
            AddRoutePage.DegreeType =>
                DegreeTypeId != null,
            AddRoutePage.Country =>
                TrainingCountryId != null,
            AddRoutePage.AgeRangeSpecialism =>
                TrainingAgeSpecialismType != null ||
                TrainingAgeSpecialismRangeFrom != null ||
                TrainingAgeSpecialismRangeTo != null,
            AddRoutePage.SubjectSpecialisms =>
                TrainingSubjectIds.Any(),
            AddRoutePage.ChangeReason =>
                ChangeReason != null ||
                ChangeReasonDetail.ChangeReasonDetail != null ||
                ChangeReasonDetail.HasAdditionalReasonDetail != null ||
                ChangeReasonDetail.UploadEvidence != null ||
                ChangeReasonDetail.EvidenceFileId != null,
            AddRoutePage.CheckYourAnswers => false,
            _ => throw new ArgumentOutOfRangeException(nameof(page))
        };
    }

    public void EnsureInitialized(CurrentProfessionalStatusFeature professionalStatusInfo)
    {
        if (Initialized)
        {
            return;
        }

        RouteToProfessionalStatusId = professionalStatusInfo.RouteToProfessionalStatus.RouteToProfessionalStatusTypeId;
        Status = professionalStatusInfo.RouteToProfessionalStatus.Status;
        HoldsFrom = professionalStatusInfo.RouteToProfessionalStatus.HoldsFrom;
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
