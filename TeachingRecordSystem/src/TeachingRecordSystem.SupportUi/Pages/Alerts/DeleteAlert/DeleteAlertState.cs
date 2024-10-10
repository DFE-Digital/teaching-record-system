namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

public class DeleteAlertState : IRegisterJourney
{
    public static JourneyDescriptor Journey => new(
        JourneyNames.DeleteAlert,
        typeof(DeleteAlertState),
        requestDataKeys: ["alertId"],
        appendUniqueKey: true);

    public bool? ConfirmDelete { get; set; }
}
