using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class InductionModel(TrsDbContext dbContext) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    public InductionStatus Status { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? CompletionDate { get; set; }

    public InductionExemptionReasons ExemptionReasons { get; set; }

    public bool ShowStartDate =>
        Status is InductionStatus.Failed
        or InductionStatus.FailedInWales
        or InductionStatus.InProgress
        or InductionStatus.Passed;

    public bool ShowCompletionDate =>
        Status is InductionStatus.Passed
        or InductionStatus.Failed
        or InductionStatus.FailedInWales;

    public string? ExemptionReasonsText
    {
        get => string.Join(", ", ExemptionReasons.SplitFlags());
    }

    public string StatusWarningMessage // CML TODO change logic to match test spec (criteria based on cpd fields)
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
            .SingleAsync(q => q.PersonId == PersonId);

        Status = person!.InductionStatus;
        StartDate = person!.InductionStartDate;
        CompletionDate = person!.InductionCompletedDate;
        ExemptionReasons = person!.InductionExemptionReasons;
    }
}
