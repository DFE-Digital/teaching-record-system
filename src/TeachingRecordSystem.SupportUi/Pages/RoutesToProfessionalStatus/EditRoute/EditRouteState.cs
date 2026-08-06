namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

public class EditRouteState
{
    // Holds the answers for a route being moved to 'holds' until they've all been given.
    public EditRouteStatusState? EditStatusState { get; set; }

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

    // Whether the journey was started from the person's induction, which is where cancelling and the
    // detail page's back link return to.
    public bool FromInductions { get; set; }

    // The questions this route and status ask. Kept here so that step validation stays synchronous.
    public EditRoutePage[] AvailablePages { get; set; } = [];

    public ChangeReasonOption? ChangeReason { get; set; }
    public ChangeReasonDetailsState ChangeReasonDetail { get; set; } = new();
}
