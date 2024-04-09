using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDateOfBirth;

[Journey(JourneyNames.EditDateOfBirth), RequireJourneyInstance]
public class ConfirmModel : PageModel
{
    private readonly TrsLinkGenerator _linkGenerator;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public ConfirmModel(
        TrsLinkGenerator linkGenerator,
        ICrmQueryDispatcher crmQueryDispatcher)
    {
        _linkGenerator = linkGenerator;
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    public JourneyInstance<EditDateOfBirthState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public string? CurrentValue { get; set; }

    public string? NewValue { get; set; }

    public async Task<IActionResult> OnPost()
    {
        await _crmQueryDispatcher.ExecuteQuery(
            new UpdateContactDateOfBirthQuery(
                PersonId,
                JourneyInstance!.State.DateOfBirth));

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Record has been updated");

        return Redirect(_linkGenerator.PersonDetail(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var state = JourneyInstance!.State;
        if (!state.IsComplete)
        {
            context.Result = Redirect(_linkGenerator.PersonEditDateOfBirth(PersonId, JourneyInstance!.InstanceId));
            return;
        }

        var person = await _crmQueryDispatcher.ExecuteQuery(
            new GetActiveContactDetailByIdQuery(
                PersonId,
                new ColumnSet(
                    Contact.PrimaryIdAttribute,
                    Contact.Fields.BirthDate)));

        CurrentValue = person!.Contact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false)!.Value.ToString("dd/MM/yyyy");
        NewValue = state.DateOfBirth!.Value.ToString("dd/MM/yyyy");

        await next();
    }
}
