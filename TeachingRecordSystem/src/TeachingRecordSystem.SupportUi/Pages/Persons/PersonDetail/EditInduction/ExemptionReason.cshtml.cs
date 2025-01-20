using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), ActivatesJourney, RequireJourneyInstance]
public class ExemptionReasonModel : CommonJourneyPage
{
    protected TrsDbContext _dbContext;
    protected ReferenceDataCache _referenceDataCache;

    protected InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;

    [FromQuery]
    public JourneyFromCheckYourAnswersPage FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "Why are they exempt from induction?")]
    public Guid[] ExemptionReasonIds { get; set; } = Array.Empty<Guid>();

    public string? PersonName { get; set; }
    public InductionExemptionReason[] ExemptionReasons { get; set; } = Array.Empty<InductionExemptionReason>();
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
                return PageLink(InductionJourneyPage.CheckAnswers);
            }
            return JourneyInstance!.State.JourneyStartPage == InductionJourneyPage.ExemptionReason
                ? LinkGenerator.PersonInduction(PersonId)
                : PageLink(InductionJourneyPage.Status);
        }
    }

    public ExemptionReasonModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext, ReferenceDataCache referenceDataCache) : base(linkGenerator)
    {
        _dbContext = dbContext;
        _referenceDataCache = referenceDataCache;
    }

    public IActionResult OnGet()
    {
        if (InductionStatus != InductionStatus.Exempt)
        {
            return NotFound();
        }
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
            if (state.JourneyStartPage == null)
            {
                state.JourneyStartPage = InductionJourneyPage.ExemptionReason;
            }
        });

        return Redirect(PageLink(NextPage));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_dbContext, PersonId, InductionJourneyPage.ExemptionReason);

        ExemptionReasons = await _referenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        await next();
    }
}
