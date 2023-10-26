using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditName;

[Journey(JourneyNames.EditName), ActivatesJourney, RequireJourneyInstance]
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

    public JourneyInstance<EditNameState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    [Display(Name = "First name")]
    [MaxLength(100, ErrorMessage = "First name must be 100 characters or less")]
    public string? FirstName { get; set; }

    [BindProperty]
    [Display(Name = "Middle name")]
    [MaxLength(100, ErrorMessage = "Middle name must be 100 characters or less")]
    public string? MiddleName { get; set; }

    [BindProperty]
    [Display(Name = "Last name")]
    [MaxLength(100, ErrorMessage = "Last name must be 100 characters or less")]
    public string? LastName { get; set; }

    public void OnGet()
    {
        FirstName ??= JourneyInstance!.State.FirstName;
        MiddleName ??= JourneyInstance!.State.MiddleName;
        LastName ??= JourneyInstance!.State.LastName;
    }

    public async Task<IActionResult> OnPost()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
        {
            ModelState.AddModelError(nameof(FirstName), "Enter a first name");
        }

        if (string.IsNullOrWhiteSpace(LastName))
        {
            ModelState.AddModelError(nameof(LastName), "Enter a last name");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s =>
        {
            s.FirstName = FirstName;
            s.MiddleName = MiddleName;
            s.LastName = LastName;
        });

        return Redirect(_linkGenerator.PersonEditNameConfirm(PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitialized(_crmQueryDispatcher, PersonId);

        await next();
    }
}
