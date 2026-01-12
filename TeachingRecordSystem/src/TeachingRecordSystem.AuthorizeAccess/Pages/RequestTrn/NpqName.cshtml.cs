using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.WebCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[WebCommon.FormFlow.Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class WhatNpqModel(RequestTrnLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter the NPQ you plan to take")]
    [MaxLength(200, ErrorMessage = "NPQ Name must be 200 characters or less")]
    public string? NpqName { get; set; }

    public void OnGet()
    {
        NpqName = JourneyInstance!.State.NpqName;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.NpqName = NpqName;
        });

        return FromCheckAnswers == true ?
            Redirect(linkGenerator.CheckAnswers(JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.NpqTrainingProvider(JourneyInstance!.InstanceId));
    }
}
