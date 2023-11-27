using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

[Journey(JourneyNames.EditMqStartDate), ActivatesJourney, RequireJourneyInstance]
public class IndexModel : PageModel
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly TrsLinkGenerator _linkGenerator;

    public IndexModel(
        ICrmQueryDispatcher crmQueryDispatcher,
        TrsLinkGenerator linkGenerator)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _linkGenerator = linkGenerator;
    }

    public JourneyInstance<EditMqStartDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Start date")]
    public DateOnly? StartDate { get; set; }

    public void OnGet()
    {
        StartDate ??= JourneyInstance!.State.StartDate;
    }

    public async Task<IActionResult> OnPost()
    {
        if (StartDate is null)
        {
            ModelState.AddModelError(nameof(StartDate), "Enter a start date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.StartDate = StartDate);

        return Redirect(_linkGenerator.MqEditStartDateConfirm(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var qualification = await _crmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(QualificationId));
        if (qualification is null || qualification.dfeta_Type != dfeta_qualification_dfeta_Type.MandatoryQualification)
        {
            context.Result = NotFound();
            return;
        }

        await JourneyInstance!.State.EnsureInitialized(_crmQueryDispatcher, qualification);

        PersonId = JourneyInstance!.State.PersonId;
        PersonName = JourneyInstance!.State.PersonName;

        await next();
    }
}
