using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.SupportTasks.ChangeRequests;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests.EditChangeRequest;

[Authorize(Policy = AuthorizationPolicies.SupportTasksEdit)]
public class RejectModel(
    TrsDbContext dbContext,
    ChangeRequestSupportTaskService changeRequestSupportTaskService,
    IBackgroundJobScheduler backgroundJobScheduler,
    SupportUiLinkGenerator linkGenerator,
    TimeProvider timeProvider) : PageModel
{
    private readonly InlineValidator<RejectModel> _validator = new()
    {
        v => v.RuleFor(m => m.RejectionReasonChoice)
            .NotNull().WithMessage("Select the reason for rejecting this change")
    };

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public SupportTaskType? ChangeType { get; set; }

    public string? PersonName { get; set; }

    SupportTask? SupportTask { get; set; }

    Person? Person { get; set; }

    [BindProperty]
    [Display(Name = " ")]
    public CaseRejectionReasonOption? RejectionReasonChoice { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        _validator.ValidateAndThrow(this);

        string requestStatus;
        string flashMessage;

        if (RejectionReasonChoice!.Value == CaseRejectionReasonOption.ChangeNoLongerRequired)
        {
            requestStatus = "cancelled";
            flashMessage = "The user’s record has not been changed and they have not been notified.";

            var processContext = new ProcessContext(
                ChangeType is SupportTaskType.ChangeNameRequest
                    ? ProcessType.ChangeOfNameRequestCancelling
                    : ProcessType.ChangeOfDateOfBirthRequestCancelling,
                timeProvider.UtcNow,
                User.GetUserId());

            await changeRequestSupportTaskService.CancelChangeRequestAsync(
                new CancelChangeRequestSupportTaskOptions { SupportTask = SupportTask! },
                processContext);
        }
        else
        {
            requestStatus = "rejected";
            flashMessage = "The user’s record has not been changed and they have been notified.";

            var processContext = new ProcessContext(
                ChangeType is SupportTaskType.ChangeNameRequest
                    ? ProcessType.ChangeOfNameRequestRejecting
                    : ProcessType.ChangeOfDateOfBirthRequestRejecting,
                timeProvider.UtcNow,
                User.GetUserId());

            await changeRequestSupportTaskService.RejectChangeRequestAsync(
                new RejectChangeRequestSupportTaskOptions
                {
                    SupportTask = SupportTask!,
                    RejectionReason = RejectionReasonChoice.Value.GetDisplayName()!
                },
                processContext);

            var (requestEmailAddress, emailTemplateId) = ChangeType switch
            {
                SupportTaskType.ChangeNameRequest =>
                    (((ChangeNameRequestData)SupportTask!.Data).EmailAddress, EmailTemplateIds.GetAnIdentityChangeOfNameRejectedEmailConfirmation),
                SupportTaskType.ChangeDateOfBirthRequest =>
                    (((ChangeDateOfBirthRequestData)SupportTask!.Data).EmailAddress, EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthRejectedEmailConfirmation),
                _ => throw new InvalidOperationException($"Unexpected change type: '{ChangeType}'.")
            };

            var emailAddress = string.IsNullOrEmpty(requestEmailAddress) ? Person!.EmailAddress : requestEmailAddress;

            if (!string.IsNullOrEmpty(emailAddress))
            {
                var email = new Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = emailTemplateId,
                    EmailAddress = emailAddress,
                    Personalization = new Dictionary<string, string>
                    {
                        [ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey] = Person!.FirstName,
                        [ChangeRequestEmailConstants.RejectionReasonEmailPersonalisationKey] = RejectionReasonChoice.Value.EmailReason()
                    }
                };

                dbContext.Emails.Add(email);
                await dbContext.SaveChangesAsync();
                await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId));
            }
        }

        TempData.SetFlashNotificationBanner(
            $"The request has been {requestStatus}",
            flashMessage);

        return Redirect(linkGenerator.SupportTasks.ChangeRequests.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var person = await dbContext.Persons
           .AsNoTracking()
           .SingleOrDefaultAsync(p => p.PersonId == supportTask.PersonId);
        if (person is null)
        {
            context.Result = NotFound();
            return;
        }

        PersonName = string.JoinNonEmpty(
            ' ',
            person.FirstName,
            person.MiddleName,
            person.LastName);

        ChangeType = supportTask.SupportTaskType;
        SupportTask = supportTask;
        Person = person;

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public enum CaseRejectionReasonOption
    {
        [Display(Name = "Request and proof don’t match")]
        RequestAndProofDontMatch,
        [Display(Name = "Wrong type of document")]
        WrongTypeOfDocument,
        [Display(Name = "Image quality")]
        ImageQuality,
        [Display(Name = "Change no longer required")]
        ChangeNoLongerRequired
    }
}
