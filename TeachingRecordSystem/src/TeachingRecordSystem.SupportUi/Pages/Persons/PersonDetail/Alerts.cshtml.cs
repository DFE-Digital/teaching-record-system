using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class AlertsModel : PageModel
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
    public ContactSearchSortByOption SortBy { get; set; }

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
            .Where(alert => alert.Status == AlertStatus.Active)
            .OrderBy(a => a.StartDate)
            .ToArray();

        return Page();
    }

    private AlertInfo MapSanction(SanctionDetailResult sanction)
    {
        var alertStatus = AlertStatus.Inactive;
        if (sanction.Sanction.StateCode == dfeta_sanctionState.Active)
        {
            if (sanction.Sanction.dfeta_EndDate is null)
            {
                alertStatus = AlertStatus.Active;
            }
            else
            {
                alertStatus = AlertStatus.Closed;
            }
        }

        return new AlertInfo()
        {
            AlertId = sanction.Sanction.Id,
            Description = sanction.Description,
            Details = sanction.Sanction.dfeta_SanctionDetails,
            StartDate = sanction.Sanction.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            EndDate = sanction.Sanction.dfeta_EndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            Status = alertStatus
        };
    }

    public record AlertInfo
    {
        public required Guid AlertId { get; init; }
        public required string Description { get; init; }
        public required string Details { get; init; }
        public required DateOnly? StartDate { get; init; }
        public required DateOnly? EndDate { get; init; }
        public required AlertStatus Status { get; init; }
    }

    public enum AlertStatus
    {
        Active,
        Inactive,
        Closed
    }
}
