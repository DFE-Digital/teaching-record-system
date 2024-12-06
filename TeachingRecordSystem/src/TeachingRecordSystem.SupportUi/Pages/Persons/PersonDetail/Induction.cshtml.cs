using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class InductionModel(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher, IClock clock) : PageModel
{
    private static readonly string NoQualifiedTeacherStatusWarning = "This teacher doesn\u2019t have QTS and, therefore, is ineligible for induction.";
    private static readonly string InductionManagedByCpdWarning = "To change a teacher\u2019s induction status to passed, failed, or in progress, use the Record inductions as an appropriate body service.";
    private bool StatusManagedByCPD;
    private bool TeacherHoldsQualifiedTeacherStatus;

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

    public string? StatusWarningMessage
    {
        get
        {
            if (StatusManagedByCPD)
            {
                return InductionManagedByCpdWarning;
            }
            else if (TeacherHoldsQualifiedTeacherStatus)
            {
                return NoQualifiedTeacherStatusWarning;
            }
            else
            {
                return null;
            }
        }
    }

    public async Task OnGetAsync()
    {
        var person = await dbContext.Persons
            .SingleAsync(q => q.PersonId == PersonId);

        GetActiveContactDetailByIdQuery query = new(
            person.PersonId,
            ColumnSet: new ColumnSet(Contact.Fields.dfeta_QTSDate));

        var result = await crmQueryDispatcher.ExecuteQueryAsync(query);

        TeacherHoldsQualifiedTeacherStatus = result?.Contact.dfeta_QTSDate is null;

        Status = person!.InductionStatus;
        StartDate = person!.InductionStartDate;
        CompletionDate = person!.InductionCompletedDate;
        ExemptionReasons = person!.InductionExemptionReasons;
        var sevenYearsAgo = clock.Today.AddYears(-7);
        StatusManagedByCPD = person!.CpdInductionStatus is not null
            && person.CpdInductionCompletedDate is not null
            && person.CpdInductionCompletedDate < sevenYearsAgo;
    }
}
