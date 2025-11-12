using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class ReasonModel(
    SupportUiLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    EvidenceUploadManager evidenceController)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    public InductionChangeReasonOption? ChangeReason { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select yes if you want to add more information about why you’re changing the induction details")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [MaxLength(UiDefaults.DetailMaxCharacterCount, ErrorMessage = $"Additional detail {UiDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    protected InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;

    public InductionJourneyPage NextPage => InductionJourneyPage.CheckAnswers;

    public string BackLink
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckAnswersPage.CheckAnswers)
            {
                return GetPageLink(InductionJourneyPage.CheckAnswers);
            }
            return InductionStatus switch
            {
                _ when InductionStatus.RequiresCompletedDate() => GetPageLink(InductionJourneyPage.CompletedDate),
                _ when InductionStatus.RequiresStartDate() => GetPageLink(InductionJourneyPage.StartDate),
                _ when InductionStatus.RequiresExemptionReasons() => GetPageLink(InductionJourneyPage.ExemptionReason),
                _ => GetPageLink(InductionJourneyPage.Status)
            };
        }
    }

    public void OnGet()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        HasAdditionalReasonDetail = JourneyInstance.State.HasAdditionalReasonDetail;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        Evidence = JourneyInstance.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HasAdditionalReasonDetail == true && ChangeReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ChangeReasonDetail), "Enter additional detail");
        }

        await EvidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail = HasAdditionalReasonDetail is true ? ChangeReasonDetail : null;
            state.Evidence = Evidence;
        });

        return Redirect(GetPageLink(NextPage));
    }
}
