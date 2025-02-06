using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class InductionModel(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    IClock clock,
    ReferenceDataCache referenceDataCache,
    IAuthorizationService authorizationService) : PageModel
{
    private const string NoQualifiedTeacherStatusWarning = "This teacher has not been awarded QTS and is therefore ineligible for induction.";
    private bool _statusIsManagedByCpd;
    private bool _teacherHoldsQualifiedTeacherStatus;

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public InductionStatus Status { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? CompletedDate { get; set; }

    public Guid[]? ExemptionReasonIds { get; set; }

    public bool ShowStartDate => Status.RequiresStartDate();

    public bool ShowCompletedDate => Status.RequiresCompletedDate();

    public IEnumerable<string>? ExemptionReasonValues { get; set; }

    public string InductionIsManagedByCpdWarning => Status switch
    {
        InductionStatus.RequiredToComplete => InductionWarnings.InductionIsManagedByCpdWarningRequiredToComplete,
        InductionStatus.InProgress => InductionWarnings.InductionIsManagedByCpdWarningInProgress,
        InductionStatus.Passed => InductionWarnings.InductionIsManagedByCpdWarningPassed,
        InductionStatus.Failed => InductionWarnings.InductionIsManagedByCpdWarningFailed,
        _ => InductionWarnings.InductionIsManagedByCpdWarningOther
    };

    public string? StatusWarningMessage =>
         _teacherHoldsQualifiedTeacherStatus ? NoQualifiedTeacherStatusWarning :
         _statusIsManagedByCpd && CanWrite ? InductionIsManagedByCpdWarning :
        null;

    public bool CanWrite { get; set; }

    public async Task OnGetAsync()
    {
        var person = await dbContext.Persons
            .SingleAsync(q => q.PersonId == PersonId);

        GetActiveContactDetailByIdQuery query = new(
            person.PersonId,
            ColumnSet: new ColumnSet(Contact.Fields.dfeta_QTSDate));

        var result = await crmQueryDispatcher.ExecuteQueryAsync(query);

        Status = person.InductionStatus;
        StartDate = person.InductionStartDate;
        CompletedDate = person.InductionCompletedDate;
        ExemptionReasonIds = person.InductionExemptionReasonIds;
        _statusIsManagedByCpd = person.InductionStatusManagedByCpd(clock.Today);
        _teacherHoldsQualifiedTeacherStatus = TeacherHoldsQualifiedTeacherStatusRule(result?.Contact.dfeta_QTSDate);

        var allExemptionReasons = await referenceDataCache.GetInductionExemptionReasonsAsync();
        ExemptionReasonValues = allExemptionReasons.Where(r => ExemptionReasonIds.Contains(r.InductionExemptionReasonId)).Select(r => r.Name);

        CanWrite = (await authorizationService.AuthorizeAsync(User, AuthorizationPolicies.InductionReadWrite))
            .Succeeded;
    }

    private bool TeacherHoldsQualifiedTeacherStatusRule(DateTime? qtsDate)
    {
        return qtsDate is null;
    }
}
