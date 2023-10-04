namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public partial class AlertsModel
{
    public record AlertInfo
    {
        public required Guid AlertId { get; init; }
        public required string Description { get; init; }
        public required string Details { get; init; }
        public required DateOnly? StartDate { get; init; }
        public required DateOnly? EndDate { get; init; }
        public required AlertStatus Status { get; init; }
    }
}
