using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class ExemptionReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    [BindProperty]
    [Display(Name = "Why are they exempt from induction?")]
    public Guid[] ExemptionReasonIds { get; set; } = [];

    public InductionExemptionReason[] ExemptionReasons { get; set; } = [];

    public InductionJourneyPage NextPage
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckYourAnswersPage.CheckYourAnswers)
            {
                return InductionJourneyPage.CheckAnswers;
            }
            return InductionJourneyPage.ChangeReasons;
        }
    }

    public string BackLink
    {
        get
        {
            if (FromCheckAnswers == JourneyFromCheckYourAnswersPage.CheckYourAnswers)
            {
                return GetPageLink(InductionJourneyPage.CheckAnswers);
            }
            return JourneyInstance!.State.JourneyStartPage == InductionJourneyPage.ExemptionReason
                ? LinkGenerator.PersonInduction(PersonId)
                : GetPageLink(InductionJourneyPage.Status);
        }
    }

    public IActionResult OnGet()
    {
        if (JourneyInstance!.State.ExemptionReasonIds != null)
        {
            ExemptionReasonIds = JourneyInstance!.State.ExemptionReasonIds;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ExemptionReasonIds.Length == 0)
        {
            ModelState.AddModelError(nameof(ExemptionReasonIds), "Select the reason for a teacher’s exemption to induction");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ExemptionReasonIds = ExemptionReasonIds;
        });

        return Redirect(GetPageLink(NextPage));
    }

    protected override InductionJourneyPage StartPage => InductionJourneyPage.ExemptionReason;

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (JourneyInstance!.State.InductionStatus != InductionStatus.Exempt)
        {
            context.Result = Redirect(GetPageLink(JourneyInstance!.State.JourneyStartPage));
            return;
        }

        ExemptionReasons = await referenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true);
    }
}
