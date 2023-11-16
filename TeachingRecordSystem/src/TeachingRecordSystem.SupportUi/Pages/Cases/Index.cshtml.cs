using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Cases;

[Authorize(Policy = AuthorizationPolicies.CaseManagement)]
public class IndexModel : PageModel
{
    private const int PageSize = 15;
    private const int MaxCrmResultCount = 5000;
    private readonly TrsLinkGenerator _linkGenerator;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public IndexModel(
        TrsLinkGenerator linkGenerator,
        ICrmQueryDispatcher crmQueryDispatcher)
    {
        _linkGenerator = linkGenerator;
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    public CaseInfo[]? Cases { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PageNumber { get; set; }

    public int[]? PaginationPages { get; set; }

    public int TotalKnownPages { get; set; }

    public bool DisplayPageNumbers { get; set; }

    public int? PreviousPage { get; set; }

    public int? NextPage { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (PageNumber < 1)
        {
            return BadRequest();
        }

        PageNumber ??= 1;

        var incidentsResult = await _crmQueryDispatcher.ExecuteQuery(new GetActiveIncidentsQuery(PageNumber.Value, PageSize));
        TotalKnownPages = Math.Max((int)Math.Ceiling((double)incidentsResult.TotalRecordCount / PageSize), 1);

        if (PageNumber > TotalKnownPages)
        {
            // Redirect to first page
            return Redirect(_linkGenerator.Cases());
        }

        if (incidentsResult.TotalRecordCount < MaxCrmResultCount)
        {
            DisplayPageNumbers = true;
            // In the pagination control, show the first page, last page, current page and two pages either side of the current page
            PaginationPages = Enumerable.Range(-2, 5).Select(offset => PageNumber.Value + offset)
                .Append(1)
                .Append(TotalKnownPages)
                .Where(page => page <= TotalKnownPages && page >= 1)
                .Distinct()
                .Order()
                .ToArray();
        }

        PreviousPage = PageNumber > 1 ? PageNumber - 1 : null;
        NextPage = PageNumber < TotalKnownPages ? PageNumber + 1 : null;

        Cases = incidentsResult.Incidents
            .Select(MapIncident)
            .OrderBy(c => c.CreatedOn)
            .ToArray();

        return Page();
    }

    private CaseInfo MapIncident(Incident incident)
    {
        var customer = incident.Extract<Contact>("contact", Contact.PrimaryIdAttribute);
        var subject = incident.Extract<Subject>("subject", Subject.PrimaryIdAttribute);

        return new CaseInfo()
        {
            CaseReference = incident.TicketNumber,
            Customer = customer.dfeta_StatedFirstName is not null ? $"{customer.dfeta_StatedFirstName} {customer.dfeta_StatedLastName}" : $"{customer.FirstName} {customer.LastName}",
            CaseType = subject.Title,
            CreatedOn = incident.CreatedOn!.Value
        };
    }

    public record CaseInfo
    {
        public required string CaseReference { get; init; }
        public required string Customer { get; init; }
        public required string CaseType { get; init; }
        public required DateTime CreatedOn { get; init; }
    }
}
