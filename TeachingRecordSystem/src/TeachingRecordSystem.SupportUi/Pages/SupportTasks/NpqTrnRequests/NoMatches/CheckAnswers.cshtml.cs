using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.NoMatches;

public class CheckAnswersModel(
    TrsDbContext dbContext,
    TrnRequestService trnRequestService,
    SupportTaskService supportTaskService,
    SupportUiLinkGenerator linkGenerator,
    IClock clock,
    IBackgroundJobScheduler backgroundJobScheduler) : PageModel
{
    public string? SourceApplicationUserName { get; set; }
    public Guid? SourceApplicationUserId { get; set; }

    public string? SupportTaskReference { get; set; }
    public Guid? PersonId { get; set; }

    public string? FirstName { get; set; }

    public string? MiddleName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? EmailAddress { get; set; }

    public string? NationalInsuranceNumber { get; set; }

    public bool PersonNameChange { get; set; }
    public bool PersonDetailsChange { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var requestData = supportTask.TrnRequestMetadata!;

        var processContext = new ProcessContext(ProcessType.NpqTrnRequestApproving, clock.UtcNow, User.GetUserId());

        var newTrn = await trnRequestService.ResolveTrnRequestWithNewRecordAsync(requestData, processContext);

        var personAttributes = new NpqTrnRequestDataPersonAttributes()
        {
            DateOfBirth = requestData.DateOfBirth,
            EmailAddress = requestData.EmailAddress,
            FirstName = requestData.FirstName ?? string.Empty,
            MiddleName = requestData.MiddleName ?? string.Empty,
            LastName = requestData.LastName ?? string.Empty,
            NationalInsuranceNumber = requestData.NationalInsuranceNumber,
            Gender = requestData.Gender
        };

        await supportTaskService.UpdateSupportTaskAsync(
            new UpdateSupportTaskOptions<NpqTrnRequestData>
            {
                SupportTask = SupportTaskReference!,
                UpdateData = data => data with
                {
                    SupportRequestOutcome = SupportRequestOutcome.Approved,
                    ResolvedAttributes = personAttributes,
                    SelectedPersonAttributes = null
                },
                Status = SupportTaskStatus.Closed,
                Comments = null,
                RejectionReason = null
            },
            processContext);

        if (!string.IsNullOrEmpty(requestData.EmailAddress))
        {
            var email = new Email
            {
                EmailId = Guid.NewGuid(),
                TemplateId = EmailTemplateIds.TrnGeneratedForNpq,
                EmailAddress = requestData.EmailAddress,
                Personalization = new Dictionary<string, string>
                    {
                        { "first name", requestData.FirstName! },
                        { "last name", requestData.LastName! },
                        { "trn", newTrn! }
                    },
            };

            await dbContext.Emails.AddAsync(email);
            await dbContext.SaveChangesAsync();
            await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId, processContext.ProcessId));
        }

        TempData.SetFlashSuccess(
            $"TRN request for {' '.JoinNonEmpty(FirstName, MiddleName, LastName)} completed and the user has been notified by email",
            buildMessageHtml: LinkTagBuilder.BuildViewRecordLink(linkGenerator.Persons.PersonDetail.Index(requestData.ResolvedPersonId!.Value)));

        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Index());
    }

    public IActionResult OnPostCancel()
    {
        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Index());
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var requestData = supportTask.TrnRequestMetadata!;

        PersonId = Guid.NewGuid();

        FirstName = requestData.FirstName;
        MiddleName = requestData.MiddleName;
        LastName = requestData.LastName;
        DateOfBirth = requestData.DateOfBirth;
        EmailAddress = requestData.EmailAddress;
        NationalInsuranceNumber = requestData.NationalInsuranceNumber;
        SupportTaskReference = supportTask.SupportTaskReference;
        SourceApplicationUserName = requestData.ApplicationUser!.Name;
        SourceApplicationUserId = requestData.ApplicationUser!.UserId;

        return next();
    }
}
