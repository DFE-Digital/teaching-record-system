using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Cases;

[Authorize(Roles = $"{UserRoles.Helpdesk},{UserRoles.Administrator}")]
public class IndexModel : PageModel
{
    private readonly IDataverseAdapter _dataverseAdapter;

    public IndexModel(IDataverseAdapter dataverseAdapter)
    {
        _dataverseAdapter = dataverseAdapter;
    }

    public CaseInfo[]? Cases { get; set; }

    public async Task OnGet()
    {
        var incidents = await _dataverseAdapter.GetActiveIncidents();
        Cases = incidents
            .OrderBy(i => i.CreatedOn)
            .Select(MapIncident)
            .ToArray();
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
        public required string CaseReference { get; set; }
        public required string Customer { get; set; }
        public required string CaseType { get; set; }
        public required DateTime CreatedOn { get; set; }
    }
}
