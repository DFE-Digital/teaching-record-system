using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[Journey(JourneyNames.SetStatus), RequireJourneyInstance]
[AllowDeactivatedPerson]
public class ReasonModel(
    SupportUiLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    EvidenceUploadManager evidenceController)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    [BindProperty]
    [Display(Name = "Why are you deactivating this record?")]
    public DeactivateReasonOption? DeactivateReason { get; set; }

    [BindProperty]
    [Display(Name = "Enter details")]
    [MaxLength(UiDefaults.DetailMaxCharacterCount, ErrorMessage = $"Reason details {UiDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? DeactivateReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Why are you reactivating this record?")]
    public ReactivateReasonOption? ReactivateReason { get; set; }

    [BindProperty]
    [Display(Name = "Enter details")]
    [MaxLength(UiDefaults.DetailMaxCharacterCount, ErrorMessage = $"Reason details {UiDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? ReactivateReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public string BackLink => FromCheckAnswers
        ? LinkGenerator.Persons.PersonDetail.SetStatus.CheckAnswers(PersonId, TargetStatus, JourneyInstance!.InstanceId)
        : LinkGenerator.Persons.PersonDetail.Index(PersonId);

    public void OnGet()
    {
        DeactivateReason = JourneyInstance!.State.DeactivateReason;
        DeactivateReasonDetail = JourneyInstance!.State.DeactivateReasonDetail;
        ReactivateReason = JourneyInstance!.State.ReactivateReason;
        ReactivateReasonDetail = JourneyInstance!.State.ReactivateReasonDetail;
        Evidence = JourneyInstance.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (TargetStatus == PersonStatus.Deactivated)
        {
            if (DeactivateReason is null)
            {
                ModelState.AddModelError(nameof(DeactivateReason), "Select a reason");
            }

            if (DeactivateReason == DeactivateReasonOption.AnotherReason && DeactivateReasonDetail is null)
            {
                ModelState.AddModelError(nameof(DeactivateReasonDetail), "Enter a reason");
            }
        }
        else
        {
            if (ReactivateReason is null)
            {
                ModelState.AddModelError(nameof(ReactivateReason), "Select a reason");
            }

            if (ReactivateReason == ReactivateReasonOption.AnotherReason && ReactivateReasonDetail is null)
            {
                ModelState.AddModelError(nameof(ReactivateReasonDetail), "Enter a reason");
            }
        }

        await EvidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.DeactivateReason = DeactivateReason;
            state.DeactivateReasonDetail = DeactivateReason is DeactivateReasonOption.AnotherReason ? DeactivateReasonDetail : null;
            state.ReactivateReason = ReactivateReason;
            state.ReactivateReasonDetail = ReactivateReason is ReactivateReasonOption.AnotherReason ? ReactivateReasonDetail : null;
            state.Evidence = Evidence;
        });

        return Redirect(LinkGenerator.Persons.PersonDetail.SetStatus.CheckAnswers(PersonId, TargetStatus, JourneyInstance!.InstanceId));
    }
}
