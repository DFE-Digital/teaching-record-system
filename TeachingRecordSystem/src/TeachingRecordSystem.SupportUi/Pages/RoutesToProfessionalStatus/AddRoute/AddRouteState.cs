using System.Text.Json.Serialization;

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

    public Guid? NewRouteToProfessionalStatusId { get; set; }
    public RouteToProfessionalStatusStatus? NewStatus { get; set; }
    public DateOnly? NewHoldsFrom { get; set; }
    public DateOnly? NewTrainingStartDate { get; set; }
    public DateOnly? NewTrainingEndDate { get; set; }
    public Guid[] NewTrainingSubjectIds { get; set; } = [];
    public TrainingAgeSpecialismType? NewTrainingAgeSpecialismType { get; set; }
    public int? NewTrainingAgeSpecialismRangeFrom { get; set; }
    public int? NewTrainingAgeSpecialismRangeTo { get; set; }
    public string? NewTrainingCountryId { get; set; }
    public Guid? NewTrainingProviderId { get; set; }
    public bool? NewIsExemptFromInduction { get; set; }
    public Guid? NewDegreeTypeId { get; set; }

    public ChangeReasonOption? ChangeReason { get; set; }
    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();

    public ChangeReasonOption? NewChangeReason { get; set; }
    public ChangeReasonDetailsState NewChangeReasonDetail { get; set; } = new();

    [JsonIgnore]
    public bool ChangeReasonIsComplete => ChangeReason is not null && ChangeReasonDetail is not null && ChangeReasonDetail.IsComplete;

    public void Commit()
    {
        RouteToProfessionalStatusId = NewRouteToProfessionalStatusId;
        Status = NewStatus;
        HoldsFrom = NewHoldsFrom;
        TrainingStartDate = NewTrainingStartDate;
        TrainingEndDate = NewTrainingEndDate;
        TrainingSubjectIds = NewTrainingSubjectIds;
        TrainingAgeSpecialismType = NewTrainingAgeSpecialismType;
        TrainingAgeSpecialismRangeFrom = NewTrainingAgeSpecialismRangeFrom;
        TrainingAgeSpecialismRangeTo = NewTrainingAgeSpecialismRangeTo;
        TrainingCountryId = NewTrainingCountryId;
        TrainingProviderId = NewTrainingProviderId;
        IsExemptFromInduction = NewIsExemptFromInduction;
        DegreeTypeId = NewDegreeTypeId;
        ChangeReason = NewChangeReason;
        ChangeReasonDetail.HasAdditionalReasonDetail = NewChangeReasonDetail.HasAdditionalReasonDetail;
        ChangeReasonDetail.ChangeReasonDetail = NewChangeReasonDetail.ChangeReasonDetail;
        ChangeReasonDetail.HasAdditionalReasonDetail = NewChangeReasonDetail.HasAdditionalReasonDetail;
        ChangeReasonDetail.UploadEvidence = NewChangeReasonDetail.UploadEvidence;
        ChangeReasonDetail.EvidenceFileId = NewChangeReasonDetail.EvidenceFileId;
        ChangeReasonDetail.EvidenceFileName = NewChangeReasonDetail.EvidenceFileName;
        ChangeReasonDetail.EvidenceFileSizeDescription = NewChangeReasonDetail.EvidenceFileSizeDescription;

        NewRouteToProfessionalStatusId = null;
        NewStatus = null;
        NewHoldsFrom = null;
        NewTrainingStartDate = null;
        NewTrainingEndDate = null;
        NewTrainingSubjectIds = [];
        NewTrainingAgeSpecialismType = null;
        NewTrainingAgeSpecialismRangeFrom = null;
        NewTrainingAgeSpecialismRangeTo = null;
        NewTrainingCountryId = null;
        NewTrainingProviderId = null;
        NewIsExemptFromInduction = null;
        NewDegreeTypeId = null;
        NewChangeReason = null;
        NewChangeReasonDetail.HasAdditionalReasonDetail = null;
        NewChangeReasonDetail.ChangeReasonDetail = null;
        NewChangeReasonDetail.HasAdditionalReasonDetail = null;
        NewChangeReasonDetail.UploadEvidence = null;
        NewChangeReasonDetail.EvidenceFileId = null;
        NewChangeReasonDetail.EvidenceFileName = null;
        NewChangeReasonDetail.EvidenceFileSizeDescription = null;
    }

    public void Begin()
    {
        NewRouteToProfessionalStatusId = RouteToProfessionalStatusId;
        NewStatus = Status;
        NewHoldsFrom = HoldsFrom;
        NewTrainingStartDate = TrainingStartDate;
        NewTrainingEndDate = TrainingEndDate;
        NewTrainingSubjectIds = TrainingSubjectIds;
        NewTrainingAgeSpecialismType = TrainingAgeSpecialismType;
        NewTrainingAgeSpecialismRangeFrom = TrainingAgeSpecialismRangeFrom;
        NewTrainingAgeSpecialismRangeTo = TrainingAgeSpecialismRangeTo;
        NewTrainingCountryId = TrainingCountryId;
        NewTrainingProviderId = TrainingProviderId;
        NewIsExemptFromInduction = IsExemptFromInduction;
        NewDegreeTypeId = DegreeTypeId;
        NewChangeReason = ChangeReason;
        NewChangeReasonDetail.HasAdditionalReasonDetail = ChangeReasonDetail.HasAdditionalReasonDetail;
        NewChangeReasonDetail.ChangeReasonDetail = ChangeReasonDetail.ChangeReasonDetail;
        NewChangeReasonDetail.HasAdditionalReasonDetail = ChangeReasonDetail.HasAdditionalReasonDetail;
        NewChangeReasonDetail.UploadEvidence = ChangeReasonDetail.UploadEvidence;
        NewChangeReasonDetail.EvidenceFileId = ChangeReasonDetail.EvidenceFileId;
        NewChangeReasonDetail.EvidenceFileName = ChangeReasonDetail.EvidenceFileName;
        NewChangeReasonDetail.EvidenceFileSizeDescription = ChangeReasonDetail.EvidenceFileSizeDescription;
    }

    //public void EnsureInitialized(CurrentProfessionalStatusFeature professionalStatusInfo)
    //{
    //    if (Initialized)
    //    {
    //        return;
    //    }

    //    RouteToProfessionalStatusId = professionalStatusInfo.RouteToProfessionalStatus.RouteToProfessionalStatusTypeId;
    //    Status = professionalStatusInfo.RouteToProfessionalStatus.Status;
    //    HoldsFrom = professionalStatusInfo.RouteToProfessionalStatus.HoldsFrom;
    //    TrainingStartDate = professionalStatusInfo.RouteToProfessionalStatus.TrainingStartDate;
    //    TrainingEndDate = professionalStatusInfo.RouteToProfessionalStatus.TrainingEndDate;
    //    TrainingSubjectIds = professionalStatusInfo.RouteToProfessionalStatus.TrainingSubjectIds;
    //    TrainingAgeSpecialismType = professionalStatusInfo.RouteToProfessionalStatus.TrainingAgeSpecialismType;
    //    TrainingAgeSpecialismRangeFrom = professionalStatusInfo.RouteToProfessionalStatus.TrainingAgeSpecialismRangeFrom;
    //    TrainingAgeSpecialismRangeTo = professionalStatusInfo.RouteToProfessionalStatus.TrainingAgeSpecialismRangeTo;
    //    TrainingCountryId = professionalStatusInfo.RouteToProfessionalStatus.TrainingCountryId;
    //    TrainingProviderId = professionalStatusInfo.RouteToProfessionalStatus.TrainingProviderId;
    //    IsExemptFromInduction = professionalStatusInfo.RouteToProfessionalStatus.ExemptFromInduction;
    //    DegreeTypeId = professionalStatusInfo.RouteToProfessionalStatus.DegreeTypeId;
    //    Initialized = true;
    //}
}
