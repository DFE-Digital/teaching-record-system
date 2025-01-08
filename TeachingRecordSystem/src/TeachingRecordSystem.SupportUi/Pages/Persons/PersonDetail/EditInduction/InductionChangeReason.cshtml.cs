using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.SupportUi.Pages.Alerts;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class InductionChangeReasonModel : CommonJourneyPage
{
    protected TrsDbContext _dbContext;
    public string? PersonName { get; set; }
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you changing the induction details?")]
    public InductionChangeReasonOption ChangeReason { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to add more information about why you’re changing the induction details?")]
    [Required(ErrorMessage = "Select yes if you want to add more information about why you’re changing the induction details")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Add additional detail")]
    [MaxLength(AlertDefaults.DetailMaxCharacterCount, ErrorMessage = "Additional detail must be 4000 characters or less")]
    public string? ChangeReasonDetail { get; set; }

    protected InductionStatus InductionStatus => JourneyInstance!.State.InductionStatus;
    public InductionJourneyPage NextPage => InductionJourneyPage.CheckAnswers;
    public string BackLink
    {
        get
        {
            return InductionStatus switch
            {
                _ when InductionStatus.RequiresCompletedDate() => PageLink(InductionJourneyPage.CompletedDate),
                _ when InductionStatus.RequiresStartDate() => PageLink(InductionJourneyPage.StartDate),
                _ when InductionStatus.RequiresExemptionReasons() => PageLink(InductionJourneyPage.ExemptionReason),
                _ => PageLink(InductionJourneyPage.Status),
            };
        }
    }

    public InductionChangeReasonModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext) : base(linkGenerator)
    {
        _dbContext = dbContext;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await JourneyInstance!.UpdateStateAsync(state =>
        {
            // TODO - store the change reason
        });

        return Redirect(PageLink(NextPage));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_dbContext, PersonId, InductionJourneyPage.Status);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        await next();
    }
}
