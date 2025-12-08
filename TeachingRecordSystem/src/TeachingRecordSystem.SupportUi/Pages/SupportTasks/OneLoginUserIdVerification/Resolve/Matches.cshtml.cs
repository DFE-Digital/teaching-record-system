using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.PersonMatching;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), RequireJourneyInstance]
public class Matches(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator, IPersonMatchingService matchingService) : PageModel
{
    public JourneyInstance<ResolveOneLoginUserIdVerificationState>? JourneyInstance { get; set; }

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    [BindProperty]
    [Required(ErrorMessage = "Select what you want to do with this GOV.UK One Login account")]
    public Guid? MatchedPersonId { get; set; }

    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public string? Trn { get; set; }

    public IList<SuggestedMatchViewModel>? SuggestedMatches { get; set; }

    public IActionResult OnGet()
    {
        return SuggestedMatches == null || SuggestedMatches.Count == 0
            ? Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.ResolveNoMatches(SupportTaskReference, JourneyInstance!.InstanceId))
            : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }
        await JourneyInstance!.UpdateStateAsync(state => state.MatchedPersonId = MatchedPersonId);

        return MatchedPersonId != ResolveOneLoginUserIdVerificationState.DoNotConnectARecordPersonIdSentinel ?
            Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.ResolveConfirmConnect(SupportTaskReference, JourneyInstance!.InstanceId)) :
            Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.ResolveNotConnecting(SupportTaskReference, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();

        return Redirect(linkGenerator.SupportTasks.OneLoginUserIdVerification.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        var oneLoginUserIdVerificationRequestData = (OneLoginUserIdVerificationData)supportTask.Data;

        Name = StringHelper.JoinNonEmpty(' ', oneLoginUserIdVerificationRequestData.StatedFirstName, oneLoginUserIdVerificationRequestData.StatedLastName);
        DateOfBirth = oneLoginUserIdVerificationRequestData.StatedDateOfBirth;
        NationalInsuranceNumber = oneLoginUserIdVerificationRequestData.StatedNationalInsuranceNumber;
        Trn = oneLoginUserIdVerificationRequestData.StatedTrn;

        Email = dbContext.OneLoginUsers
            .Where(u => u.Subject == oneLoginUserIdVerificationRequestData.OneLoginUserSubject)? // CML TODO ?
            .FirstOrDefault()?
            .EmailAddress;

        var request = new GetSuggestedOneLoginUserMatchesRequest(
            Names: [Name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)],
            DatesOfBirth: [DateOfBirth],
            NationalInsuranceNumber: NationalInsuranceNumber,
            Trn: Trn,
            null
        );

        SuggestedMatches = (await matchingService.GetSuggestedOneLoginUserMatchesWithMatchedAttributesInfoAsync(request))
            .Select((match, idx) => new SuggestedMatchViewModel
            {
                Identifier = (char)('A' + idx),
                PersonId = match.PersonId,
                Trn = match.Trn,
                EmailAddress = match.EmailAddress,
                FirstName = match.FirstName,
                LastName = match.LastName,
                DateOfBirth = match.DateOfBirth,
                NationalInsuranceNumber = match.NationalInsuranceNumber,
                PreviousNames = dbContext.Persons
                    .Include(p => p.PreviousNames)
                    .Where(p => p.PersonId == match.PersonId)?.SingleOrDefault()?
                    .PreviousNames?.Select(n => $"{n.FirstName} {n.LastName}")
                    .ToList(),
                MatchedAttributes = match.MatchedAttributes
            }).ToList();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
