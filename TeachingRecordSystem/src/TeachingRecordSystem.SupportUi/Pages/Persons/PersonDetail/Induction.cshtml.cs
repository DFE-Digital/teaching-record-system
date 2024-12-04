using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class InductionModel(TrsDbContext dbContext) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    public InductionStatus Status { get; set; }

    public string? ExemptionReason { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? CompletionDate { get; set; }

    public string StatusWarningMessage
    {
        get
        {
            var message = "Changing status to In Progress, Passed or Failed will be handled in CPD";
            return Status switch
            {
                InductionStatus.RequiredToComplete => message,
                InductionStatus.InProgress => message,
                InductionStatus.Passed => message,
                InductionStatus.Failed => message,
                _ => string.Empty
            };
        }
    }

    public async Task OnGetAsync()
    {
        var person = await dbContext.Persons
            .SingleOrDefaultAsync(q => q.PersonId == PersonId);
        Status = person?.InductionStatus ?? InductionStatus.None;

        StartDate = person?.InductionStartDate;

        CompletionDate = person?.InductionCompletedDate;
    }

}
