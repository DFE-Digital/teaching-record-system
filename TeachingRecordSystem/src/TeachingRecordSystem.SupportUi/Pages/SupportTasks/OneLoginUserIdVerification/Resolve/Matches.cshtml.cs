using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.PersonMatching;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), RequireJourneyInstance]
public class Matches(TrsDbContext dbContext, IPersonMatchingService matchingService) : PageModel
{
    public JourneyInstance<ResolveOneLoginUserIdVerificationState>? JourneyInstance { get; set; }

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public string? Trn { get; set; }

    public IReadOnlyCollection<SuggestedMatch>? SuggestedMatches { get; set; }

    public async Task OnGet()
    {
        var request = new GetSuggestedOneLoginUserMatchesRequest(
            Names: new List<string[]> { PersonSearchAttribute.SplitName(Name!) }, // CML TODO check PersonSearchAttribute.SplitName
            DatesOfBirth: new List<DateOnly> { DateOfBirth },
            NationalInsuranceNumber: NationalInsuranceNumber,
            Trn: Trn,
            null
        );
        SuggestedMatches = await matchingService.GetSuggestedOneLoginUserMatchesAsync(request);
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        var oneLoginUserIdVerificationRequestData = (OneLoginUserIdVerificationData)supportTask.Data;

        Name = StringHelper.JoinNonEmpty(' ', oneLoginUserIdVerificationRequestData.StatedFirstName, oneLoginUserIdVerificationRequestData.StatedLastName);
        DateOfBirth = oneLoginUserIdVerificationRequestData.StatedDateOfBirth;
        NationalInsuranceNumber = oneLoginUserIdVerificationRequestData.StatedNationalInsuranceNumber;
        Trn = oneLoginUserIdVerificationRequestData.StatedTrn;

        Email = dbContext.OneLoginUsers
            .Where(u => u.Subject == oneLoginUserIdVerificationRequestData.OneLoginUserSubject)? // CML TODO
            .FirstOrDefault()?
            .EmailAddress;
    }
}
