namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

public class AddAlertState
{
    public Guid? AlertTypeId { get; set; }

    public string? Details { get; set; }

    public string? Link { get; set; }

    public DateOnly? StartDate { get; set; }
}
