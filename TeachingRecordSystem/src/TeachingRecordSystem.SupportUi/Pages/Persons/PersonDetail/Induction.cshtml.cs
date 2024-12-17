using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class InductionModel(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher, IClock clock) : PageModel
{
    private const string NoQualifiedTeacherStatusWarning = "This teacher has not been awarded QTS and is therefore ineligible for induction.";
    private const string InductionIsManagedByCpdWarning = "To change a teacher\u2019s induction status to passed, failed, or in progress, use the Record inductions as an appropriate body service.";
    private bool _statusIsManagedByCpd;
    private bool _teacherHoldsQualifiedTeacherStatus;

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public InductionStatus Status { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? CompletionDate { get; set; }

    public InductionExemptionReasons ExemptionReasons { get; set; }

    public bool ShowStartDate => Status.RequiresStartDate();

    public bool ShowCompletionDate => Status.RequiresCompletedDate();

    public string? ExemptionReasonsText
    {
        get => string.Join(", ", ExemptionReasons.SplitFlags());
    }

    public string? StatusWarningMessage
    {
        get
        {
            if (_statusIsManagedByCpd)
            {
                return InductionIsManagedByCpdWarning;
            }
            else if (_teacherHoldsQualifiedTeacherStatus)
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

        Status = person!.InductionStatus;
        StartDate = person!.InductionStartDate;
        CompletionDate = person!.InductionCompletedDate;
        ExemptionReasons = person!.InductionExemptionReasons;
        _statusIsManagedByCpd = person.InductionStatusManagedByCpd(clock.Today);
        _teacherHoldsQualifiedTeacherStatus = TeacherHoldsQualifiedTeacherStatusRule(result?.Contact.dfeta_QTSDate);
    }

    private bool TeacherHoldsQualifiedTeacherStatusRule(DateTime? qtsDate)
    {
        return qtsDate is null;
    }
}
