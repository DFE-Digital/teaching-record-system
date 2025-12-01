using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[Journey(JourneyNames.SetStatus), RequireJourneyInstance]
[AllowDeactivatedPerson]
public class ReasonModel(
    SupportUiLinkGenerator linkGenerator,
    PersonService personService,
    EvidenceUploadManager evidenceController)
    : CommonJourneyPage(personService, linkGenerator, evidenceController)
{
    [BindProperty]
    public PersonDeactivateReason? DeactivateReason { get; set; }

    [BindProperty]
    [MaxLength(UiDefaults.DetailMaxCharacterCount, ErrorMessage = $"Reason details {UiDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? DeactivateReasonDetail { get; set; }

    [BindProperty]
    public PersonReactivateReason? ReactivateReason { get; set; }

    [BindProperty]
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

            if (DeactivateReason == Core.Services.Persons.PersonDeactivateReason.AnotherReason && DeactivateReasonDetail is null)
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

            if (ReactivateReason == Core.Services.Persons.PersonReactivateReason.AnotherReason && ReactivateReasonDetail is null)
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
            state.DeactivateReasonDetail = DeactivateReason is Core.Services.Persons.PersonDeactivateReason.AnotherReason ? DeactivateReasonDetail : null;
            state.ReactivateReason = ReactivateReason;
            state.ReactivateReasonDetail = ReactivateReason is Core.Services.Persons.PersonReactivateReason.AnotherReason ? ReactivateReasonDetail : null;
            state.Evidence = Evidence;
        });

        return Redirect(LinkGenerator.Persons.PersonDetail.SetStatus.CheckAnswers(PersonId, TargetStatus, JourneyInstance!.InstanceId));
    }
}
