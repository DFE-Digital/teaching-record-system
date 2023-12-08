using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Result;

[Journey(JourneyNames.EditMqResult), ActivatesJourney, RequireJourneyInstance]
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

    public JourneyInstance<EditMqResultState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public dfeta_qualification_dfeta_MQ_Status? Result { get; set; }

    [BindProperty]
    [Display(Name = "End date")]
    public DateOnly? EndDate { get; set; }

    public void OnGet()
    {
        Result ??= JourneyInstance!.State.Result;
        EndDate ??= JourneyInstance!.State.EndDate;
    }

    public async Task<IActionResult> OnPost()
    {
        if (Result is null)
        {
            ModelState.AddModelError(nameof(Result), "Select a result");
        }

        if (Result == dfeta_qualification_dfeta_MQ_Status.Passed && EndDate is null)
        {
            ModelState.AddModelError(nameof(EndDate), "Enter an end date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(
            state =>
            {
                state.Result = Result;
                state.EndDate = Result == dfeta_qualification_dfeta_MQ_Status.Passed ? EndDate : null;
            });

        return Redirect(_linkGenerator.MqEditResultConfirm(QualificationId, JourneyInstance!.InstanceId));
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
