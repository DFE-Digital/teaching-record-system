using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDateOfBirth;

[Journey(JourneyNames.EditDateOfBirth), ActivatesJourney, RequireJourneyInstance]
public class IndexModel : PageModel
{
    private readonly TrsLinkGenerator _linkGenerator;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public IndexModel(
        TrsLinkGenerator linkGenerator,
        ICrmQueryDispatcher crmQueryDispatcher)
    {
        _linkGenerator = linkGenerator;
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    public JourneyInstance<EditDateOfBirthState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    [Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    public void OnGet()
    {
        DateOfBirth ??= JourneyInstance!.State.DateOfBirth;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (DateOfBirth is null)
        {
            ModelState.AddModelError(nameof(DateOfBirth), "Enter a date of birth");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s => s.DateOfBirth = DateOfBirth);

        return Redirect(_linkGenerator.PersonEditDateOfBirthConfirm(PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_crmQueryDispatcher, PersonId);

        await next();
    }
}
