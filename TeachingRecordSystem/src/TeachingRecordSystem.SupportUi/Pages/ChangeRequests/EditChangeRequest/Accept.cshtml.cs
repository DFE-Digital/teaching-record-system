using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ChangeRequests.EditChangeRequest;

[Authorize(Policy = AuthorizationPolicies.ChangeRequestManagement)]
public class AcceptModel : PageModel
{
    private readonly TrsLinkGenerator _linkGenerator;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public AcceptModel(
        TrsLinkGenerator linkGenerator,
        ICrmQueryDispatcher crmQueryDispatcher)
    {
        _linkGenerator = linkGenerator;
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    [FromRoute]
    public string TicketNumber { get; set; } = null!;

    public string? CaseType { get; set; }

    public string? PersonName { get; set; }

    IncidentDetail? IncidentDetail { get; set; }

    public NameChangeRequestInfo? NameChangeRequest { get; set; }

    public DateOfBirthChangeRequestInfo? DateOfBirthChangeRequest { get; set; }

    public IActionResult OnGet()
    {
        SetModelFromIncidentDetail();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _crmQueryDispatcher.WithDqtUserImpersonation().ExecuteQueryAsync(new ApproveIncidentQuery(IncidentDetail!.Incident.Id));

        TempData.SetFlashSuccess(
            $"The request has been accepted",
            "The user’s record has been changed and they have been notified.");

        return Redirect(_linkGenerator.SupportTasks());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        IncidentDetail = await _crmQueryDispatcher.WithDqtUserImpersonation().ExecuteQueryAsync(new GetIncidentByTicketNumberQuery(TicketNumber));
        if (IncidentDetail is null)
        {
            context.Result = NotFound();
            return;
        }

        if (IncidentDetail.Incident.StateCode != IncidentState.Active)
        {
            context.Result = BadRequest();
            return;
        }

        await next();
    }

    private void SetModelFromIncidentDetail()
    {
        var incident = IncidentDetail!.Incident;
        var customer = IncidentDetail.Contact;
        var subject = IncidentDetail.Subject;

        CaseType = subject.Title;
        PersonName = customer.ResolveFullName(includeMiddleName: false);

        if (subject.Title == DqtConstants.NameChangeSubjectTitle)
        {
            NameChangeRequest = new NameChangeRequestInfo()
            {
                CurrentFirstName = customer.FirstName,
                CurrentMiddleName = customer.MiddleName,
                CurrentLastName = customer.LastName,
                NewFirstName = incident.dfeta_NewFirstName,
                NewMiddleName = incident.dfeta_NewMiddleName,
                NewLastName = incident.dfeta_NewLastName
            };
        }

        if (subject.Title == DqtConstants.DateOfBirthChangeSubjectTitle)
        {
            DateOfBirthChangeRequest = new DateOfBirthChangeRequestInfo()
            {
                CurrentDateOfBirth = customer.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false)!.Value,
                NewDateOfBirth = incident.dfeta_NewDateofBirth.ToDateOnlyWithDqtBstFix(isLocalTime: true)!.Value
            };
        }
    }
}
