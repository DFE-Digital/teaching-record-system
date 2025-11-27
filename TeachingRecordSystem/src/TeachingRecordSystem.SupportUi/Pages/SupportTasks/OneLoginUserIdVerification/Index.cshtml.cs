using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserIdVerification;

public class Index(TrsDbContext dbContext) : PageModel
{
    [BindProperty(SupportsGet = true)]
    [FromQuery]
    public string? Search { get; set; }

    public IQueryable<Result>? Results { get; set; }

    public void OnGet()
    {
        var tasks = dbContext.SupportTasks
            .Where(t => t.SupportTaskType == SupportTaskType.OneLoginUserIdVerification && t.Status == SupportTaskStatus.Open)
            .OrderBy(t => t.CreatedOn);

        Results = tasks
            .Select(t => new
            {
                t,
                Data = t.Data as OneLoginUserIdVerificationData
            })
            .Where(item => item.Data != null)
            .Join(
                dbContext.OneLoginUsers,
                y => y.Data!.OneLoginUserSubject,
                user => user.Subject,
                (item, user) => new Result(
                    item.t.SupportTaskReference,
                    item.Data!.StatedFirstName,
                    item.Data!.StatedLastName,
                    user.EmailAddress,
                    item.t.CreatedOn));
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
