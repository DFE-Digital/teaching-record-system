using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification.Resolve;

[Journey(JourneyNames.ResolveOneLoginUserIdVerification), RequireJourneyInstance]
public class Matches(TrsDbContext dbContext) : PageModel
{
    public JourneyInstance<ResolveOneLoginUserIdVerificationState>? JourneyInstance { get; set; }

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public OneLoginUserIdVerificationData? OneLoginUserIdVerificationRequestData { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string? NationalInsuranceNumber { get; set; }
    public string? Trn { get; set; }

    public void OnGet()
    {

    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        OneLoginUserIdVerificationRequestData = (OneLoginUserIdVerificationData)supportTask.Data;

        Name = StringHelper.JoinNonEmpty(' ', OneLoginUserIdVerificationRequestData.StatedFirstName, OneLoginUserIdVerificationRequestData.StatedLastName);
        DateOfBirth = OneLoginUserIdVerificationRequestData.StatedDateOfBirth;
        NationalInsuranceNumber = OneLoginUserIdVerificationRequestData.StatedNationalInsuranceNumber;
        Trn = OneLoginUserIdVerificationRequestData.StatedTrn;

        Email = dbContext.OneLoginUsers
            .Where(u => u.Subject == OneLoginUserIdVerificationRequestData.OneLoginUserSubject)? // CML TODO
            .FirstOrDefault()?
            .EmailAddress;
    }
}
