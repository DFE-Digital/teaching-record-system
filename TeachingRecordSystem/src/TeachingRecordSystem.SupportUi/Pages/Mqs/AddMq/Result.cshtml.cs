using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class ResultModel : PageModel
{
    private readonly TrsLinkGenerator _linkGenerator;

    public ResultModel(
        TrsLinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public dfeta_qualification_dfeta_MQ_Status? Result { get; set; }

    [BindProperty]
    [Display(Name = "End date")]
    public DateOnly? EndDate { get; set; }

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

        return Redirect(_linkGenerator.MqAddCheckAnswers(PersonId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personDetail = (ContactDetail?)context.HttpContext.Items["CurrentPersonDetail"];

        PersonName = personDetail!.Contact.ResolveFullName(includeMiddleName: false);
        Result ??= JourneyInstance!.State.Result;
        EndDate ??= JourneyInstance!.State.EndDate;

        await next();
    }
}
