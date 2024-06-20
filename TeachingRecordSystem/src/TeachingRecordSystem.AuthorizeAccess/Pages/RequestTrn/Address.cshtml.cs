using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.UiCommon.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;

[Journey(RequestTrnJourneyState.JourneyName), RequireJourneyInstance]
public class AddressModel(AuthorizeAccessLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<RequestTrnJourneyState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool? FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "Address line 1")]
    [Required(ErrorMessage = "Enter address line 1")]
    [MaxLength(200, ErrorMessage = "Address line 1 must be 200 characters or less")]
    public string? AddressLine1 { get; set; }

    [BindProperty]
    [Display(Name = "Address line 2 (optional)")]
    [MaxLength(200, ErrorMessage = "Address line 2 must be 200 characters or less")]
    public string? AddressLine2 { get; set; }

    [BindProperty]
    [Display(Name = "Town or city")]
    [Required(ErrorMessage = "Enter town or city")]
    [MaxLength(200, ErrorMessage = "Town or city must be 200 characters or less")]
    public string? TownOrCity { get; set; }

    [BindProperty]
    [Display(Name = "Postal code")]
    [Required(ErrorMessage = "Enter postal code")]
    [MaxLength(50, ErrorMessage = "Postal code must be 50 characters or less")]
    public string? PostalCode { get; set; }

    [BindProperty]
    [Display(Name = "Country")]
    [Required(ErrorMessage = "Enter country")]
    [MaxLength(200, ErrorMessage = "Country must be 200 characters or less")]
    public string? Country { get; set; }

    public void OnGet()
    {
        AddressLine1 ??= JourneyInstance!.State.AddressLine1;
        AddressLine2 ??= JourneyInstance!.State.AddressLine2;
        TownOrCity ??= JourneyInstance!.State.TownOrCity;
        PostalCode ??= JourneyInstance!.State.PostalCode;
        Country ??= JourneyInstance!.State.Country;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.AddressLine1 = AddressLine1;
            state.AddressLine2 = AddressLine2;
            state.TownOrCity = TownOrCity;
            state.Country = Country;
            state.PostalCode = PostalCode;
        });

        return Redirect(linkGenerator.RequestTrnCheckAnswers(JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var state = JourneyInstance!.State;
        if (state.HasPendingTrnRequest)
        {
            context.Result = Redirect(linkGenerator.RequestTrnSubmitted(JourneyInstance!.InstanceId));
        }
        else if (state.HasNationalInsuranceNumber is null)
        {
            context.Result = Redirect(linkGenerator.RequestTrnNationalInsuranceNumber(JourneyInstance.InstanceId));
        }
    }
}
