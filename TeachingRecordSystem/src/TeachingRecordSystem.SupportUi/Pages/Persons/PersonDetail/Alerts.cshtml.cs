using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Common;

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

    [FromQuery]
    public PersonSubNavigationTab? SelectedTab { get; set; }

    public void OnGet()
    {
    }
}
