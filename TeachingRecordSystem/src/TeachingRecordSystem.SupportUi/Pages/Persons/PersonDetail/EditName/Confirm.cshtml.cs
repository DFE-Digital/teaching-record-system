using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditName;

[Journey(JourneyNames.EditName), RequireJourneyInstance]
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

    public JourneyInstance<EditNameState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public string? CurrentValue { get; set; }

    public string? NewValue { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        await _crmQueryDispatcher.WithDqtUserImpersonation().ExecuteQueryAsync(
            new UpdateContactNameQuery(
                PersonId,
                JourneyInstance!.State.FirstName,
                JourneyInstance!.State.MiddleName,
                JourneyInstance!.State.LastName));

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Record has been updated");

        return Redirect(_linkGenerator.PersonDetail(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var state = JourneyInstance!.State;
        if (!state.IsComplete)
        {
            context.Result = Redirect(_linkGenerator.PersonEditName(PersonId, JourneyInstance!.InstanceId));
            return;
        }

        var person = await _crmQueryDispatcher.WithDqtUserImpersonation().ExecuteQueryAsync(
            new GetActiveContactDetailByIdQuery(
                PersonId,
                new ColumnSet(
                    Contact.PrimaryIdAttribute,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName)));

        CurrentValue = string.IsNullOrEmpty(person!.Contact.MiddleName)
            ? $"{person.Contact.FirstName} {person.Contact.LastName}"
            : $"{person.Contact.FirstName} {person.Contact.MiddleName} {person.Contact.LastName}";

        NewValue = string.IsNullOrEmpty(state.MiddleName)
            ? $"{state.FirstName} {state.LastName}"
            : $"{state.FirstName} {state.MiddleName} {state.LastName}";

        await next();
    }
}
