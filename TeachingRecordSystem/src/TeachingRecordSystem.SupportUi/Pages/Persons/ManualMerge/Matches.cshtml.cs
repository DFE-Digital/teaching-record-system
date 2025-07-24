using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.ManualMerge;

[RequireFeatureEnabledFilterFactory(FeatureNames.ContactsMigrated)]
[Journey(JourneyNames.ManualMergePerson), RequireJourneyInstance]
public class MatchesModel(
    TrsDbContext dbContext,
    TrsLinkGenerator linkGenerator,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    public string BackLink => GetPageLink(ManualMergeJourneyPage.EnterTrn);

    public string? CannotMergeReason { get; private set; }

    public IReadOnlyList<PotentialDuplicate>? PotentialDuplicates { get; private set; }

    [Display(Name = "Which is the primary record?")]
    [Required(ErrorMessage = "Select primary record")]
    [BindProperty]
    public Guid? PrimaryRecordId { get; set; }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (JourneyInstance!.State.PersonAId is not Guid personAId || JourneyInstance!.State.PersonBId is not Guid personBId)
        {
            context.Result = Redirect(GetPageLink(ManualMergeJourneyPage.EnterTrn));
            return;
        }

        PotentialDuplicates = await GetPotentialDuplicatesAsync(personAId, personBId);

        foreach (var potentialDuplicate in PotentialDuplicates)
        {
            if (potentialDuplicate.HasBeenDeactivated)
            {
                CannotMergeReason = "One of these records has been deactivated.";
                break;
            }

            if (potentialDuplicate.HasActiveAlerts && potentialDuplicate.HasInvalidInductionStatus)
            {
                CannotMergeReason = $"One of these records has an alert and an induction status of {potentialDuplicate.InductionStatus.GetDisplayName()}.";
                break;
            }

            if (potentialDuplicate.HasActiveAlerts)
            {
                CannotMergeReason = "One of these records has an alert.";
                break;
            }

            if (potentialDuplicate.HasInvalidInductionStatus)
            {
                CannotMergeReason = $"The induction status of one of these records is {potentialDuplicate.InductionStatus.GetDisplayName()}.";
                break;
            }
        }
    }

    public IActionResult OnGet()
    {
        PrimaryRecordId = JourneyInstance!.State.PrimaryRecordId;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (PotentialDuplicates!.Any(p => p.IsInvalid))
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.PrimaryRecordId = PrimaryRecordId;
        });

        return Redirect(GetPageLink(ManualMergeJourneyPage.Merge));
    }
}
