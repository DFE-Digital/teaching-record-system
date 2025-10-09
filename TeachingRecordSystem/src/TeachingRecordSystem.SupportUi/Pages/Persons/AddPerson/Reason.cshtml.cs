using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[Journey(JourneyNames.AddPerson), RequireJourneyInstance]
public class ReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    EvidenceController evidenceController)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceController)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you creating this record?")]
    public AddPersonReasonOption? CreateReason { get; set; }

    [BindProperty]
    [Display(Name = "Enter details")]
    [MaxLength(FileUploadDefaults.DetailMaxCharacterCount, ErrorMessage = $"Reason details {FileUploadDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? CreateReasonDetail { get; set; }

    [BindProperty]
    public EvidenceModel Evidence { get; set; } = new();

    public string BackLink => GetPageLink(
        FromCheckAnswers
            ? AddPersonJourneyPage.CheckAnswers
            : AddPersonJourneyPage.PersonalDetails);

    public string NextPage => GetPageLink(
        CreateJourneyPage.CheckAnswers,
        FromCheckAnswers is true ? true : null);

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete && NextIncompletePage < AddPersonJourneyPage.CreateReason)
        {
            context.Result = Redirect(GetPageLink(NextIncompletePage));
            return;
        }
    }

    public void OnGet()
    {
        CreateReason = JourneyInstance!.State.CreateReason;
        CreateReasonDetail = JourneyInstance.State.CreateReasonDetail;
        Evidence = JourneyInstance.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (CreateReason is not null && CreateReason.Value == AddPersonReasonOption.AnotherReason && CreateReasonDetail is null)
        {
            ModelState.AddModelError(nameof(CreateReasonDetail), "Enter a reason");
        }

        await EvidenceController.ValidateAndUploadAsync(Evidence, ModelState);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.CreateReason = CreateReason;
            state.CreateReasonDetail = CreateReason is CreateReasonOption.AnotherReason ? CreateReasonDetail : null;
            state.UploadEvidence = UploadEvidence;
            state.EvidenceFileId = UploadEvidence is true ? EvidenceFileId : null;
            state.EvidenceFileName = UploadEvidence is true ? EvidenceFileName : null;
            state.EvidenceFileSizeDescription = UploadEvidence is true ? EvidenceFileSizeDescription : null;
        });

        return Redirect(NextPage);
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete && NextIncompletePage < CreateJourneyPage.CreateReason)
        {
            context.Result = Redirect(GetPageLink(NextIncompletePage));
            return;
        }
    }
}
