using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification;

public class Index(TrsDbContext dbContext, SupportUiLinkGenerator linkGenerator) : PageModel
{
    private const int TasksPerPage = 20;

    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public string? Search { get; set; }

    public IQueryable<Result>? Results { get; set; }

    public void OnGet()
    {
        var tasks = dbContext.SupportTasks
            .Where(t => t.SupportTaskType == SupportTaskType.OneLoginUserIdVerification && t.Status == SupportTaskStatus.Open);

        Results = tasks.Select(t => new Result(
            t.SupportTaskReference,
            ((OneLoginUserIdVerificationData)t.Data).StatedFirstName,
            ((OneLoginUserIdVerificationData)t.Data).StatedLastName,
            dbContext.OneLoginUsers.Single(u => u.Subject == ((OneLoginUserIdVerificationData)t.Data).OneLoginUserSubject).EmailAddress,
            t.CreatedOn));
    }

    public record Result(
    string SupportTaskReference,
    string FirstName,
    string LastName,
    string? EmailAddress,
    DateTime CreatedOn)
    {
        public string Name => $"{FirstName} {LastName}";
    }
}
