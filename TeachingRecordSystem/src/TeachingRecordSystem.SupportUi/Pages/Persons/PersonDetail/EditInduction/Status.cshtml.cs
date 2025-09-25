using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.ValidationAttributes;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class StatusModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IClock clock,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    private bool _inductionStatusManagedByCpd;

    [BindProperty]
    [Display(Name = "What is their induction status?")]
    [NotEqual(InductionStatus.None, ErrorMessage = "Select a status")]
    public InductionStatus InductionStatus { get; set; }
    public InductionStatus CurrentInductionStatus { get; set; }

    public IEnumerable<InductionStatusDescription> StatusChoices
    {
        get
        {
            return _inductionStatusManagedByCpd && (CurrentInductionStatus is not InductionStatus.FailedInWales and not InductionStatus.Exempt) ?
                InductionStatusRegistry.ValidStatusChangesWhenManagedByCpd
                    .Append(InductionStatusRegistry.All.Single(i => i.InductionStatus == CurrentInductionStatus))
                    .OrderBy(i => i.InductionStatus)
                    .ToArray()
                : InductionStatusRegistry.All.ToArray()[1..];
        }
    }

    public string InductionIsManagedByCpdWarning => CurrentInductionStatus switch
    {
        InductionStatus.RequiredToComplete => InductionWarnings.InductionIsManagedByCpdWarningRequiredToComplete,
        InductionStatus.InProgress => InductionWarnings.InductionIsManagedByCpdWarningInProgress,
        InductionStatus.Passed => InductionWarnings.InductionIsManagedByCpdWarningPassed,
        InductionStatus.Failed => InductionWarnings.InductionIsManagedByCpdWarningFailed,
        _ => InductionWarnings.InductionIsManagedByCpdWarningOther
    };

    public string? StatusWarningMessage => _inductionStatusManagedByCpd && (CurrentInductionStatus is not InductionStatus.FailedInWales and not InductionStatus.Exempt) ? InductionIsManagedByCpdWarning : null;

    public InductionJourneyPage NextPage =>
     InductionStatus switch
     {
         _ when InductionStatus.RequiresExemptionReasons() => InductionJourneyPage.ExemptionReason,
         _ when InductionStatus.RequiresStartDate() => InductionJourneyPage.StartDate,
         _ => InductionJourneyPage.ChangeReasons
     };

    public string BackLink
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckYourAnswersPage.CheckYourAnswers)
            {
                return GetPageLink(InductionJourneyPage.CheckAnswers);
            }
            return LinkGenerator.PersonInduction(PersonId);
        }
    }

    public void OnGet()
    {
        InductionStatus = JourneyInstance!.State.InductionStatus;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.InductionStatus = InductionStatus;
            if (!InductionStatus.RequiresStartDate())
            {
                state.StartDate = null;
            }
            if (!InductionStatus.RequiresCompletedDate())
            {
                state.CompletedDate = null;
            }
            if (!InductionStatus.RequiresExemptionReasons())
            {
                state.ExemptionReasonIds = Array.Empty<Guid>();
            }
        });

        return Redirect(GetPageLink(NextPage));
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        var person = await DbContext.Persons.SingleAsync(q => q.PersonId == PersonId);
        _inductionStatusManagedByCpd = person.InductionStatusManagedByCpd(clock.Today);
        CurrentInductionStatus = JourneyInstance!.State.CurrentInductionStatus;
    }
}
