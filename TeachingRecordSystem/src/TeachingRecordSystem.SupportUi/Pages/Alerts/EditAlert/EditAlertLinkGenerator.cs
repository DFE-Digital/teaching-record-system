using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert;

public class EditAlertLinkGenerator(LinkGenerator linkGenerator)
{
    public EditAlertDetailsLinkGenerator Details => new(linkGenerator);
    public EditAlertStartDateLinkGenerator StartDate => new(linkGenerator);
    public EditAlertEndDateLinkGenerator EndDate => new(linkGenerator);
    public EditAlertLinkLinkGenerator Link => new(linkGenerator);
}
