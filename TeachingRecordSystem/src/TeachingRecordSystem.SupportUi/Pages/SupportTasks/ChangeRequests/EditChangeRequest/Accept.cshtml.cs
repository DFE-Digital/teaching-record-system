using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.ChangeRequests.EditChangeRequest;

[Authorize(Policy = AuthorizationPolicies.SupportTasksEdit)]
public class AcceptModel(
    TrsDbContext dbContext,
    PersonService personService,
    SupportTaskService supportTaskService,
    IBackgroundJobScheduler backgroundJobScheduler,
    SupportUiLinkGenerator linkGenerator,
    TimeProvider timeProvider) : PageModel
{
    private SupportTask? _supportTask;

    [FromRoute]
    public required string SupportTaskReference { get; init; }

    public SupportTaskType ChangeType { get; set; }

    public string? PersonName { get; set; }

    public NameChangeRequestInfo? NameChangeRequest { get; set; }

    public DateOfBirthChangeRequestInfo? DateOfBirthChangeRequest { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var changeNameRequestData =
            ChangeType is SupportTaskType.ChangeNameRequest ? _supportTask!.GetData<ChangeNameRequestData>() : null;
        var changeDateOfBirthRequestData =
            ChangeType is SupportTaskType.ChangeDateOfBirthRequest ? _supportTask!.GetData<ChangeDateOfBirthRequestData>() : null;

        var processContext = new ProcessContext(
            ChangeType is SupportTaskType.ChangeNameRequest ? ProcessType.ChangeOfNameRequestApproving : ProcessType.ChangeOfDateOfBirthRequestApproving,
            timeProvider.UtcNow,
            User.GetUserId());

        string? emailAddress = _supportTask!.Person!.EmailAddress;
        string emailTemplateId;

        if (ChangeType is SupportTaskType.ChangeNameRequest)
        {
            await personService.UpdatePersonDetailsAsync(
                new()
                {
                    PersonId = _supportTask!.Person!.PersonId,
                    FirstName = Option.Some(changeNameRequestData!.FirstName),
                    MiddleName = Option.Some(changeNameRequestData.MiddleName),
                    LastName = Option.Some(changeNameRequestData.LastName),
                    DateOfBirth = default,
                    CreatePreviousName = true,
                    EmailAddress = default,
                    NationalInsuranceNumber = default,
                    Gender = default
                },
                processContext);

            await supportTaskService.UpdateSupportTaskAsync(
                new UpdateSupportTaskOptions<ChangeNameRequestData>
                {
                    UpdateData = data => data with { ChangeRequestOutcome = SupportRequestOutcome.Approved },
                    SupportTaskReference = _supportTask.SupportTaskReference,
                    Status = SupportTaskStatus.Closed
                },
                processContext);

            if (!string.IsNullOrEmpty(changeNameRequestData.EmailAddress))
            {
                emailAddress = changeNameRequestData.EmailAddress;
            }

            emailTemplateId = EmailTemplateIds.GetAnIdentityChangeOfNameApprovedEmailConfirmation;
        }
        else
        {
            Debug.Assert(ChangeType is SupportTaskType.ChangeDateOfBirthRequest);

            await personService.UpdatePersonDetailsAsync(
                new()
                {
                    PersonId = _supportTask!.Person!.PersonId,
                    FirstName = default,
                    MiddleName = default,
                    LastName = default,
                    DateOfBirth = Option.Some<DateOnly?>(changeDateOfBirthRequestData!.DateOfBirth),
                    CreatePreviousName = false,
                    EmailAddress = default,
                    NationalInsuranceNumber = default,
                    Gender = default
                },
                processContext);

            await supportTaskService.UpdateSupportTaskAsync(
                new UpdateSupportTaskOptions<ChangeDateOfBirthRequestData>
                {
                    UpdateData = data => data with { ChangeRequestOutcome = SupportRequestOutcome.Approved },
                    SupportTaskReference = _supportTask.SupportTaskReference,
                    Status = SupportTaskStatus.Closed
                },
                processContext);

            if (!string.IsNullOrEmpty(changeDateOfBirthRequestData.EmailAddress))
            {
                emailAddress = changeDateOfBirthRequestData.EmailAddress;
            }

            emailTemplateId = EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthApprovedEmailConfirmation;
        }

        if (!string.IsNullOrEmpty(emailAddress))
        {
            var email = new Email
            {
                EmailId = Guid.NewGuid(),
                TemplateId = emailTemplateId,
                EmailAddress = emailAddress,
                Personalization = new Dictionary<string, string>
                {
                    { ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey, _supportTask.Person.FirstName }
                }
            };

            dbContext.Emails.Add(email);
            await dbContext.SaveChangesAsync();

            await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId));
        }

        TempData.SetFlashSuccess(
            "The request has been accepted",
            "The user’s record has been changed and they have been notified.");

        return Redirect(linkGenerator.SupportTasks.ChangeRequests.Index());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        _supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;

        PersonName = StringHelper.JoinNonEmpty(
            ' ',
            _supportTask.Person!.FirstName,
            _supportTask.Person.MiddleName,
            _supportTask.Person.LastName);

        ChangeType = _supportTask.SupportTaskType;

        if (ChangeType is SupportTaskType.ChangeNameRequest)
        {
            var data = _supportTask.GetData<ChangeNameRequestData>();

            NameChangeRequest = new NameChangeRequestInfo
            {
                CurrentFirstName = _supportTask.Person.FirstName,
                CurrentMiddleName = _supportTask.Person.MiddleName,
                CurrentLastName = _supportTask.Person.LastName,
                NewFirstName = data.FirstName,
                NewMiddleName = data.MiddleName,
                NewLastName = data.LastName
            };
        }
        else
        {
            Debug.Assert(ChangeType is SupportTaskType.ChangeDateOfBirthRequest);

            var data = _supportTask.GetData<ChangeDateOfBirthRequestData>();

            DateOfBirthChangeRequest = new DateOfBirthChangeRequestInfo
            {
                CurrentDateOfBirth = _supportTask.Person.DateOfBirth!.Value,
                NewDateOfBirth = data.DateOfBirth
            };
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
