using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class ExemptionReasonModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache)
    : CommonJourneyPage(linkGenerator)
{
    [FromQuery]
    public JourneyFromCheckYourAnswersPage FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "Why are they exempt from induction?")]
    public Guid[] ExemptionReasonIds { get; set; } = [];

    public string? PersonName { get; set; }

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

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(dbContext, PersonId, InductionJourneyPage.ExemptionReason);

        if (JourneyInstance!.State.InductionStatus != InductionStatus.Exempt)
        {
            context.Result = Redirect(GetPageLink(JourneyInstance!.State.JourneyStartPage));
            return;
        }

        ExemptionReasons = await referenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        await next();
    }
}
