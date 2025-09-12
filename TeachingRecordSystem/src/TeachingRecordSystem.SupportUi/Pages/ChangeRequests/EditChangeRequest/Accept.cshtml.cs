using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ChangeRequests.EditChangeRequest;

[Authorize(Policy = AuthorizationPolicies.SupportTasksEdit)]
public class AcceptModel(
    TrsDbContext dbContext,
    IBackgroundJobScheduler backgroundJobScheduler,
    TrsLinkGenerator linkGenerator,
    IClock clock) : PageModel
{
    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public SupportTaskType? ChangeType { get; set; }

    public string? PersonName { get; set; }

    SupportTask? SupportTask { get; set; }

    Person? Person { get; set; }

    public NameChangeRequestInfo? NameChangeRequest { get; set; }

    public DateOfBirthChangeRequestInfo? DateOfBirthChangeRequest { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var now = clock.UtcNow;
        var changeNameRequestData = ChangeType == SupportTaskType.ChangeNameRequest ? EventModels.ChangeNameRequestData.FromModel((ChangeNameRequestData)SupportTask!.Data) : null;
        var changeDateOfBirthRequestData = ChangeType == SupportTaskType.ChangeDateOfBirthRequest ? EventModels.ChangeDateOfBirthRequestData.FromModel((ChangeDateOfBirthRequestData)SupportTask!.Data) : null;
        var oldSupportTask = EventModels.SupportTask.FromModel(SupportTask!);
        SupportTask!.Status = SupportTaskStatus.Closed;
        SupportTask.UpdatedOn = now;

        var updateResult = Person!.UpdateDetails(
            ChangeType == SupportTaskType.ChangeNameRequest ? Option.Some(changeNameRequestData!.FirstName ?? string.Empty) : Option.None<string>(),
            ChangeType == SupportTaskType.ChangeNameRequest ? Option.Some(changeNameRequestData!.MiddleName ?? string.Empty) : Option.None<string>(),
            ChangeType == SupportTaskType.ChangeNameRequest ? Option.Some(changeNameRequestData!.LastName ?? string.Empty) : Option.None<string>(),
            ChangeType == SupportTaskType.ChangeDateOfBirthRequest ? Option.Some<DateOnly?>(changeDateOfBirthRequestData!.DateOfBirth) : Option.None<DateOnly?>(),
            Option.None<EmailAddress?>(),
            Option.None<NationalInsuranceNumber?>(),
            Option.None<Gender?>(),
            now);

        string? emailAddress = null;
        string emailTemplateId = null!;
        if (ChangeType == SupportTaskType.ChangeNameRequest)
        {
            SupportTask.UpdateData<ChangeNameRequestData>(data => data with
            {
                ChangeRequestOutcome = SupportRequestOutcome.Approved
            });

            var approvedEvent = new ChangeNameRequestSupportTaskApprovedEvent()
            {
                PersonId = Person!.PersonId,
                RequestData = changeNameRequestData!,
                SupportTask = EventModels.SupportTask.FromModel(SupportTask!),
                OldSupportTask = oldSupportTask,
                PersonAttributes = updateResult.PersonAttributes,
                OldPersonAttributes = updateResult.OldPersonAttributes,
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = User.GetUserId(),
                Changes = (ChangeNameRequestSupportTaskApprovedEventChanges)updateResult.Changes
            };

            if (approvedEvent.Changes.HasAnyFlag(ChangeNameRequestSupportTaskApprovedEventChanges.NameChange))
            {
                dbContext.PreviousNames.Add(new PreviousName
                {
                    PreviousNameId = Guid.NewGuid(),
                    PersonId = Person!.PersonId,
                    FirstName = updateResult.OldPersonAttributes.FirstName,
                    MiddleName = updateResult.OldPersonAttributes.MiddleName,
                    LastName = updateResult.OldPersonAttributes.LastName,
                    CreatedOn = now,
                    UpdatedOn = now
                });
            }

            emailAddress = string.IsNullOrEmpty(changeNameRequestData!.EmailAddress) ? Person!.EmailAddress : changeNameRequestData.EmailAddress;
            emailTemplateId = ChangeRequestEmailConstants.GetAnIdentityChangeOfNameApprovedEmailConfirmationTemplateId;

            await dbContext.AddEventAndBroadcastAsync(approvedEvent);
        }
        else if (ChangeType == SupportTaskType.ChangeDateOfBirthRequest)
        {
            SupportTask.UpdateData<ChangeDateOfBirthRequestData>(data => data with
            {
                ChangeRequestOutcome = SupportRequestOutcome.Approved
            });

            var approvedEvent = new ChangeDateOfBirthRequestSupportTaskApprovedEvent()
            {
                PersonId = Person!.PersonId,
                RequestData = changeDateOfBirthRequestData!,
                SupportTask = EventModels.SupportTask.FromModel(SupportTask!),
                OldSupportTask = oldSupportTask,
                PersonAttributes = updateResult.PersonAttributes,
                OldPersonAttributes = updateResult.OldPersonAttributes,
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = User.GetUserId(),
                Changes = (ChangeDateOfBirthRequestSupportTaskApprovedEventChanges)updateResult.Changes
            };

            emailAddress = string.IsNullOrEmpty(changeDateOfBirthRequestData!.EmailAddress) ? Person!.EmailAddress : changeDateOfBirthRequestData.EmailAddress;
            emailTemplateId = ChangeRequestEmailConstants.GetAnIdentityChangeOfDateOfBirthApprovedEmailConfirmationTemplateId;

            await dbContext.AddEventAndBroadcastAsync(approvedEvent);
        }

        if (!string.IsNullOrEmpty(emailAddress))
        {
            var email = new Email
            {
                EmailId = Guid.NewGuid(),
                TemplateId = emailTemplateId,
                EmailAddress = emailAddress!,
                Personalization = new Dictionary<string, string>() { { ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey, Person!.FirstName } }
            };

            dbContext.Emails.Add(email);
            await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId));
        }

        await dbContext.SaveChangesAsync();

        TempData.SetFlashSuccess(
            $"The request has been accepted",
            "The user’s record has been changed and they have been notified.");

        return Redirect(linkGenerator.SupportTasks(categories: [SupportTaskCategory.ChangeRequests], sortBy: SupportTasks.IndexModel.SortByOption.DateRequested, filtersApplied: true));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        var person = await dbContext.Persons
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

        if (supportTask.SupportTaskType == SupportTaskType.ChangeNameRequest)
        {
            var data = (ChangeNameRequestData)supportTask.Data;
            NameChangeRequest = new NameChangeRequestInfo()
            {
                CurrentFirstName = person!.FirstName,
                CurrentMiddleName = person.MiddleName,
                CurrentLastName = person.LastName,
                NewFirstName = data.FirstName,
                NewMiddleName = data.MiddleName,
                NewLastName = data.LastName
            };
        }

        if (supportTask.SupportTaskType == SupportTaskType.ChangeDateOfBirthRequest)
        {
            var data = (ChangeDateOfBirthRequestData)supportTask.Data;
            DateOfBirthChangeRequest = new DateOfBirthChangeRequestInfo()
            {
                CurrentDateOfBirth = person.DateOfBirth!.Value,
                NewDateOfBirth = data.DateOfBirth
            };
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
