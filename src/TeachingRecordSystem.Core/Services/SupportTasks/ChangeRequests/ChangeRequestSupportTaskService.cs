using System.Diagnostics;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.Core.Services.SupportTasks.ChangeRequests;

public class ChangeRequestSupportTaskService(
    SupportTaskService supportTaskService,
    TrsDbContext dbContext,
    PersonService personService,
    IBackgroundJobScheduler backgroundJobScheduler)
{
    public async Task<SupportTask> CreateNameChangeRequestAsync(
        CreateNameChangeRequestSupportTaskOptions options,
        ProcessContext processContext)
    {
        var person = (await dbContext.Persons.FindAsync(options.PersonId))!;

        return await supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.ChangeNameRequest,
                Data = new ChangeNameRequestData
                {
                    FirstName = options.FirstName,
                    MiddleName = options.MiddleName,
                    LastName = options.LastName,
                    EvidenceFileId = options.EvidenceFileId,
                    EvidenceFileName = options.EvidenceFileName,
                    EmailAddress = options.EmailAddress,
                    ChangeRequestOutcome = null
                },
                PersonId = options.PersonId,
                OneLoginUserSubject = null,
                TrnRequest = null,
                Subject = SupportTask.Subject.FromPerson(person)
            },
            processContext);
    }

    public async Task<SupportTask> CreateDateOfBirthChangeRequestAsync(
        CreateDateOfBirthChangeRequestSupportTaskOptions options,
        ProcessContext processContext)
    {
        var person = (await dbContext.Persons.FindAsync(options.PersonId))!;

        return await supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.ChangeDateOfBirthRequest,
                Data = new ChangeDateOfBirthRequestData
                {
                    DateOfBirth = options.DateOfBirth,
                    EvidenceFileId = options.EvidenceFileId,
                    EvidenceFileName = options.EvidenceFileName,
                    EmailAddress = options.EmailAddress,
                    ChangeRequestOutcome = null
                },
                PersonId = options.PersonId,
                OneLoginUserSubject = null,
                TrnRequest = null,
                Subject = SupportTask.Subject.FromPerson(person)
            },
            processContext);
    }

    public async Task ApproveChangeRequestAsync(ApproveChangeRequestSupportTaskOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;
        var person = await dbContext.Persons.FindOrThrowAsync(supportTask.PersonId!.Value);

        string? requestEmailAddress;
        string emailTemplateId;

        if (supportTask.SupportTaskType is SupportTaskType.ChangeNameRequest)
        {
            var data = supportTask.GetData<ChangeNameRequestData>();

            await personService.UpdatePersonDetailsAsync(
                new UpdatePersonDetailsOptions
                {
                    PersonId = person.PersonId,
                    FirstName = Option.Some(data.FirstName),
                    MiddleName = Option.Some(data.MiddleName ?? string.Empty),
                    LastName = Option.Some(data.LastName),
                    DateOfBirth = default,
                    CreatePreviousName = true,
                    EmailAddress = default,
                    NationalInsuranceNumber = default,
                    Gender = default
                },
                processContext);

            requestEmailAddress = data.EmailAddress;
            emailTemplateId = EmailTemplateIds.GetAnIdentityChangeOfNameApprovedEmailConfirmation;
        }
        else
        {
            Debug.Assert(supportTask.SupportTaskType is SupportTaskType.ChangeDateOfBirthRequest);

            var data = supportTask.GetData<ChangeDateOfBirthRequestData>();

            await personService.UpdatePersonDetailsAsync(
                new UpdatePersonDetailsOptions
                {
                    PersonId = person.PersonId,
                    FirstName = default,
                    MiddleName = default,
                    LastName = default,
                    DateOfBirth = Option.Some<DateOnly?>(data.DateOfBirth),
                    CreatePreviousName = false,
                    EmailAddress = default,
                    NationalInsuranceNumber = default,
                    Gender = default
                },
                processContext);

            requestEmailAddress = data.EmailAddress;
            emailTemplateId = EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthApprovedEmailConfirmation;
        }

        await ResolveChangeRequestAsync(supportTask, SupportRequestOutcome.Approved, rejectionReason: null, processContext);

        var emailAddress = !string.IsNullOrEmpty(requestEmailAddress) ? requestEmailAddress : person.EmailAddress;

        if (!string.IsNullOrEmpty(emailAddress))
        {
            // person is updated by now, so a name change is confirmed to the person under their new name
            await SendEmailAsync(
                emailTemplateId,
                emailAddress,
                new Dictionary<string, string>
                {
                    { ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey, person.FirstName }
                });
        }
    }

    public Task RejectChangeRequestAsync(RejectChangeRequestSupportTaskOptions options, ProcessContext processContext) =>
        ResolveChangeRequestAsync(options.SupportTask, SupportRequestOutcome.Rejected, options.RejectionReason, processContext);

    public Task CancelChangeRequestAsync(CancelChangeRequestSupportTaskOptions options, ProcessContext processContext) =>
        ResolveChangeRequestAsync(options.SupportTask, SupportRequestOutcome.Cancelled, rejectionReason: null, processContext);

    private async Task SendEmailAsync(string templateId, string emailAddress, Dictionary<string, string> personalization)
    {
        var email = new Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = templateId,
            EmailAddress = emailAddress,
            Personalization = personalization
        };

        dbContext.Emails.Add(email);
        await dbContext.SaveChangesAsync();

        await backgroundJobScheduler.EnqueueAsync<SendEmailJob>(j => j.ExecuteAsync(email.EmailId));
    }

    private Task ResolveChangeRequestAsync(
        SupportTask supportTask,
        SupportRequestOutcome outcome,
        string? rejectionReason,
        ProcessContext processContext)
    {
        return supportTask.SupportTaskType switch
        {
            SupportTaskType.ChangeNameRequest => supportTaskService.UpdateSupportTaskAsync(
                new UpdateSupportTaskOptions<ChangeNameRequestData>
                {
                    UpdateData = data => data with { ChangeRequestOutcome = outcome },
                    SupportTaskReference = supportTask.SupportTaskReference,
                    Status = SupportTaskStatus.Closed,
                    RejectionReason = rejectionReason
                },
                processContext),
            SupportTaskType.ChangeDateOfBirthRequest => supportTaskService.UpdateSupportTaskAsync(
                new UpdateSupportTaskOptions<ChangeDateOfBirthRequestData>
                {
                    UpdateData = data => data with { ChangeRequestOutcome = outcome },
                    SupportTaskReference = supportTask.SupportTaskReference,
                    Status = SupportTaskStatus.Closed,
                    RejectionReason = rejectionReason
                },
                processContext),
            _ => throw new ArgumentException(
                $"Unexpected support task type: '{supportTask.SupportTaskType}'.", nameof(supportTask))
        };
    }
}
