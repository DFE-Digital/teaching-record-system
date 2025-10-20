using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ChangeRequests.EditChangeRequest;

[Authorize(Policy = AuthorizationPolicies.SupportTasksEdit)]
public class RejectModel(
    TrsDbContext dbContext,
    IBackgroundJobScheduler backgroundJobScheduler,
    SupportUiLinkGenerator linkGenerator,
    IClock clock) : PageModel
{
    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public SupportTaskType? ChangeType { get; set; }

    public string? PersonName { get; set; }

    SupportTask? SupportTask { get; set; }

    Person? Person { get; set; }

    [BindProperty]
    [Display(Name = " ")]
    [Required(ErrorMessage = "Select the reason for rejecting this change")]
    public CaseRejectionReasonOption? RejectionReasonChoice { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var requestStatus = "rejected";
        var flashMessage = "The user’s record has not been changed and they have been notified.";

        var changeNameRequestData = ChangeType == SupportTaskType.ChangeNameRequest ? EventModels.ChangeNameRequestData.FromModel((ChangeNameRequestData)SupportTask!.Data) : null;
        var changeDateOfBirthRequestData = ChangeType == SupportTaskType.ChangeDateOfBirthRequest ? EventModels.ChangeDateOfBirthRequestData.FromModel((ChangeDateOfBirthRequestData)SupportTask!.Data) : null;
        var oldSupportTask = EventModels.SupportTask.FromModel(SupportTask!);
        SupportTask!.Status = SupportTaskStatus.Closed;
        SupportTask.UpdatedOn = clock.UtcNow;

        if (RejectionReasonChoice!.Value == CaseRejectionReasonOption.ChangeNoLongerRequired)
        {
            requestStatus = "cancelled";
            flashMessage = "The user’s record has not been changed and they have not been notified.";

            EventBase cancelledEvent = null!;
            if (ChangeType == SupportTaskType.ChangeNameRequest)
            {
                SupportTask.UpdateData<ChangeNameRequestData>(data => data with
                {
                    ChangeRequestOutcome = SupportRequestOutcome.Cancelled
                });

                cancelledEvent = new ChangeNameRequestSupportTaskCancelledEvent()
                {
                    PersonId = Person!.PersonId,
                    RequestData = changeNameRequestData!,
                    SupportTask = EventModels.SupportTask.FromModel(SupportTask!),
                    OldSupportTask = oldSupportTask,
                    EventId = Guid.NewGuid(),
                    CreatedUtc = clock.UtcNow,
                    RaisedBy = User.GetUserId()
                };
            }
            else if (ChangeType == SupportTaskType.ChangeDateOfBirthRequest)
            {
                SupportTask.UpdateData<ChangeDateOfBirthRequestData>(data => data with
                {
                    ChangeRequestOutcome = SupportRequestOutcome.Cancelled
                });

                cancelledEvent = new ChangeDateOfBirthRequestSupportTaskCancelledEvent()
                {
                    PersonId = Person!.PersonId,
                    RequestData = changeDateOfBirthRequestData!,
                    SupportTask = EventModels.SupportTask.FromModel(SupportTask!),
                    OldSupportTask = oldSupportTask,
                    EventId = Guid.NewGuid(),
                    CreatedUtc = clock.UtcNow,
                    RaisedBy = User.GetUserId()
                };
            }

            await dbContext.AddEventAndBroadcastAsync(cancelledEvent);
        }
        else
        {
            EventBase rejectedEvent = null!;
            string? emailAddress = null;
            string emailTemplateId = null!;
            if (ChangeType == SupportTaskType.ChangeNameRequest)
            {
                SupportTask.UpdateData<ChangeNameRequestData>(data => data with
                {
                    ChangeRequestOutcome = SupportRequestOutcome.Rejected
                });
                rejectedEvent = new ChangeNameRequestSupportTaskRejectedEvent()
                {
                    PersonId = Person!.PersonId,
                    RequestData = changeNameRequestData!,
                    SupportTask = EventModels.SupportTask.FromModel(SupportTask!),
                    OldSupportTask = oldSupportTask,
                    RejectionReason = RejectionReasonChoice.Value.GetDisplayName()!,
                    EventId = Guid.NewGuid(),
                    CreatedUtc = clock.UtcNow,
                    RaisedBy = User.GetUserId()
                };

                emailAddress = string.IsNullOrEmpty(changeNameRequestData!.EmailAddress) ? Person!.EmailAddress : changeNameRequestData.EmailAddress;
                emailTemplateId = EmailTemplateIds.NewGetAnIdentityChangeOfNameRejectedEmailConfirmation;
            }
            else if (ChangeType == SupportTaskType.ChangeDateOfBirthRequest)
            {
                SupportTask.UpdateData<ChangeDateOfBirthRequestData>(data => data with
                {
                    ChangeRequestOutcome = SupportRequestOutcome.Rejected
                });

                rejectedEvent = new ChangeDateOfBirthRequestSupportTaskRejectedEvent()
                {
                    PersonId = Person!.PersonId,
                    RequestData = changeDateOfBirthRequestData!,
                    SupportTask = EventModels.SupportTask.FromModel(SupportTask!),
                    OldSupportTask = oldSupportTask,
                    RejectionReason = RejectionReasonChoice.Value.GetDisplayName()!,
                    EventId = Guid.NewGuid(),
                    CreatedUtc = clock.UtcNow,
                    RaisedBy = User.GetUserId()
                };

                emailAddress = string.IsNullOrEmpty(changeDateOfBirthRequestData!.EmailAddress) ? Person!.EmailAddress : changeDateOfBirthRequestData.EmailAddress;
                emailTemplateId = EmailTemplateIds.NewGetAnIdentityChangeOfDateOfBirthRejectedEmailConfirmation;
            }

            await dbContext.AddEventAndBroadcastAsync(rejectedEvent);

            if (!string.IsNullOrEmpty(emailAddress))
            {
                var email = new Email
                {
                    EmailId = Guid.NewGuid(),
                    TemplateId = emailTemplateId,
                    EmailAddress = emailAddress!,
                    Personalization = new Dictionary<string, string>
                    {
                        [ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey] = Person!.FirstName,
                        [ChangeRequestEmailConstants.RejectionReasonEmailPersonalisationKey] = RejectionReasonChoice.GetDescription()!
                    }
                };

                dbContext.Emails.Add(email);
                await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId));
            }
        }

        await dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess(
            $"The request has been {requestStatus}",
            flashMessage);

        return Redirect(linkGenerator.SupportTasks.Index(categories: [SupportTaskCategory.ChangeRequests], sortBy: SupportTasks.IndexModel.SortByOption.DateRequested, filtersApplied: true));
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

        PersonName = StringHelper.JoinNonEmpty(
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
        [Display(Name = "Request and proof don’t match", Description = "This is because the proof you provided did not match your request.")]
        RequestAndProofDontMatch,
        [Display(Name = "Wrong type of document", Description = "This is because you provided the wrong type of document.")]
        WrongTypeOfDocument,
        [Display(Name = "Image quality", Description = "This is because the image you provided was not clear enough.")]
        ImageQuality,
        [Display(Name = "Change no longer required", Description = "This is because the change is no longer required.")]
        ChangeNoLongerRequired
    }
}
