using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Pages.Common;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public partial class AlertsModel : PageModel
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public AlertsModel(ICrmQueryDispatcher crmQueryDispatcher)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    [FromRoute]
    public Guid PersonId { get; set; }

    [FromQuery]
    public string? Search { get; set; }

    [FromQuery]
    public int? PageNumber { get; set; }

    [FromQuery]
    public ContactSearchSortByOption? SortBy { get; set; }

    public string? Name { get; set; }

    public AlertInfo[]? CurrentAlerts { get; set; }

    public AlertInfo[]? PreviousAlerts { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var contactDetail = await _crmQueryDispatcher.ExecuteQuery(
            new GetContactDetailByIdQuery(
                PersonId,
                new ColumnSet(
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_StatedLastName)));

        Name = contactDetail!.Contact.ResolveFullName(includeMiddleName: false);

        var sanctions = await _crmQueryDispatcher.ExecuteQuery(new GetSanctionDetailsByContactIdQuery(PersonId));

        var allAlerts = sanctions!.Select(MapSanction);

        CurrentAlerts = allAlerts
            .Where(alert => alert.Status == AlertStatus.Active)
            .OrderBy(a => a.StartDate)
            .ToArray();

        PreviousAlerts = allAlerts
            .Except(CurrentAlerts)
            .OrderBy(a => a.StartDate)
            .ToArray();

        return Page();
    }

    private AlertInfo MapSanction(SanctionDetailResult sanction)
    {
        var alertStatus = sanction.Sanction.StateCode == dfeta_sanctionState.Inactive ? AlertStatus.Inactive :
            sanction.Sanction.dfeta_EndDate is null ? AlertStatus.Active :
            AlertStatus.Closed;

        return new AlertInfo()
        {
            AlertId = sanction.Sanction.Id,
            Description = sanction.Description,
            Details = sanction.Sanction.dfeta_SanctionDetails,
            DetailsLink = sanction.Sanction.dfeta_DetailsLink,
            StartDate = sanction.Sanction.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            EndDate = sanction.Sanction.dfeta_EndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            Status = alertStatus
        };
    }
}
