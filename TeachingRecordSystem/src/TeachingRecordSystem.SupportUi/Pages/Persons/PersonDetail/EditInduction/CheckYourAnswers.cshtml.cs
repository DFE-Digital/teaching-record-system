using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class CheckYourAnswersModel : CommonJourneyPage
{
    private readonly TrsDbContext _dbContext;
    public string? PersonName { get; set; }
    public InductionStatus InductionStatus { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public string? ExemptionReasonsDisplayString { get; set; }
    public InductionChangeReasonOption? ChangeReason { get; set; }
    public string? ChangeReasonDetail { get; set; }
    public string? UploadedEvidenceFileUrl { get; set; }
    public string? EvidenceFileName { get; set; }
    public bool ShowStartDate =>
        InductionStatus == InductionStatus.InProgress ||
        InductionStatus == InductionStatus.Passed ||
        InductionStatus == InductionStatus.Failed ||
        InductionStatus == InductionStatus.FailedInWales;
    public bool ShowCompletedDate =>
        InductionStatus == InductionStatus.Failed ||
        InductionStatus == InductionStatus.FailedInWales ||
        InductionStatus == InductionStatus.Passed;
    public bool ShowExemptionReasons =>
        InductionStatus == InductionStatus.Exempt;

    public CheckYourAnswersModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext) : base(linkGenerator)
    {
        _dbContext = dbContext;
    }

    public string BackLink => PageLink(InductionJourneyPage.ChangeReasons);

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        // TODO - end of journey logic

        return Redirect(NextPage()(PersonId, JourneyInstance!.InstanceId));
    }

    public Func<Guid, JourneyInstanceId, string> NextPage()
    {
        return (Id, journeyInstanceId) => LinkGenerator.PersonInduction(Id);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_dbContext, PersonId, InductionJourneyPage.Status);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        //EvidenceFileId = JourneyInstance!.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        //UploadedEvidenceFileUrl = JourneyInstance!.State.UploadedEvidenceFileUrl;
        ChangeReason = JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance!.State.ChangeReasonDetail;
        InductionStatus = JourneyInstance!.State.InductionStatus;
        StartDate = JourneyInstance!.State.StartDate;
        CompletedDate = JourneyInstance!.State.CompletedDate;

        //EvidenceFileSizeDescription = JourneyInstance!.State.EvidenceFileSizeDescription;
        await next();
    }
}
