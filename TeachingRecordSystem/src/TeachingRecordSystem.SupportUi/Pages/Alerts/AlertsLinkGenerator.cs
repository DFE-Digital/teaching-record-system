using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;
using TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;
using TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert;
using TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

namespace TeachingRecordSystem.SupportUi;

public class AlertsLinkGenerator(LinkGenerator linkGenerator)
{
    public AddAlertLinkGenerator AddAlert => new(linkGenerator);

    public CloseAlertLinkGenerator CloseAlert => new(linkGenerator);

    public DeleteAlertLinkGenerator DeleteAlert => new(linkGenerator);

    public EditAlertLinkGenerator EditAlert => new(linkGenerator);

    public ReopenAlertLinkGenerator ReopenAlert => new(linkGenerator);

    public string AlertDetail(Guid alertId) =>
        linkGenerator.GetRequiredPathByPage("/Alerts/AlertDetail", routeValues: new { alertId });
}
