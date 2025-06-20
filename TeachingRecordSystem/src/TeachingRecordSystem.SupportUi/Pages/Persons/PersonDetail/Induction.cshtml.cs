using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class InductionModel(
    TrsDbContext dbContext,
    IClock clock,
    ReferenceDataCache referenceDataCache,
    IAuthorizationService authorizationService,
    IFeatureProvider featureProvider) : PageModel
{
    private const string NoQualifiedTeacherStatusWarning = "This teacher does not hold QTS and is therefore ineligible for induction.";
    private bool _statusIsManagedByCpd;

    public bool HasQts { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    [BindProperty]
    public InductionStatus Status { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? CompletedDate { get; set; }

    public Guid[]? ExemptionReasonIdsHeldOnPerson { get; set; }

    public bool ShowStartDate => Status.RequiresStartDate();

    public bool ShowCompletedDate => Status.RequiresCompletedDate();

    public IEnumerable<string>? ExemptionReasonNames { get; set; }

    public IEnumerable<RouteToProfessionalStatus>? InductionExemptedRoutes { get; set; }

    public string InductionIsManagedByCpdWarning => Status switch
    {
        InductionStatus.RequiredToComplete => InductionWarnings.InductionIsManagedByCpdWarningRequiredToComplete,
        InductionStatus.InProgress => InductionWarnings.InductionIsManagedByCpdWarningInProgress,
        InductionStatus.Passed => InductionWarnings.InductionIsManagedByCpdWarningPassed,
        InductionStatus.Failed => InductionWarnings.InductionIsManagedByCpdWarningFailed,
        _ => InductionWarnings.InductionIsManagedByCpdWarningOther
    };

    public string? StatusWarningMessage =>
        _statusIsManagedByCpd && CanWrite && (Status is not InductionStatus.FailedInWales and not InductionStatus.Exempt) ? InductionIsManagedByCpdWarning :
        !HasQts ? NoQualifiedTeacherStatusWarning :
        null;

    public bool CanWrite { get; set; }

    public async Task OnGetAsync()
    {
        var person = await dbContext.Persons
            .Include(p => p.Qualifications)
            .SingleAsync(q => q.PersonId == PersonId);

        Status = person.InductionStatus;
        StartDate = person.InductionStartDate;
        CompletedDate = person.InductionCompletedDate;
        ExemptionReasonIdsHeldOnPerson = person.InductionExemptionReasonIds;
        ExemptionReasonNames = (await referenceDataCache
            .GetPersonLevelInductionExemptionReasonsAsync())
            .Where(i => ExemptionReasonIdsHeldOnPerson.Contains(i.InductionExemptionReasonId))
            .Select(i => i.Name)
            .OrderDescending();
        _statusIsManagedByCpd = person.InductionStatusManagedByCpd(clock.Today);
        HasQts = person.QtsDate is not null;

        if (featureProvider.IsEnabled(FeatureNames.RoutesToProfessionalStatus))
        {
            InductionExemptedRoutes = dbContext.RouteToProfessionalStatuses
                .Include(r => r.RouteToProfessionalStatusType)
                .ThenInclude(r => r != null ? r.InductionExemptionReason : null)
                .Where(r => r.PersonId == PersonId && r.RouteToProfessionalStatusType != null && r.ExemptFromInduction == true);
        }

        CanWrite = (await authorizationService.AuthorizeAsync(User, AuthorizationPolicies.InductionReadWrite))
            .Succeeded;
    }
}
