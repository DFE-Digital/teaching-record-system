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

    public async Task RejectChangeRequestAsync(RejectChangeRequestSupportTaskOptions options, ProcessContext processContext)
    {
        var supportTask = options.SupportTask;

        await ResolveChangeRequestAsync(
            supportTask,
            SupportRequestOutcome.Rejected,
            options.RejectionReason.GetDisplayName()!,
            processContext);

        var (requestEmailAddress, emailTemplateId) = supportTask.SupportTaskType switch
        {
            SupportTaskType.ChangeNameRequest =>
                (supportTask.GetData<ChangeNameRequestData>().EmailAddress, EmailTemplateIds.GetAnIdentityChangeOfNameRejectedEmailConfirmation),
            SupportTaskType.ChangeDateOfBirthRequest =>
                (supportTask.GetData<ChangeDateOfBirthRequestData>().EmailAddress, EmailTemplateIds.GetAnIdentityChangeOfDateOfBirthRejectedEmailConfirmation),
            _ => throw new ArgumentException(
                $"Unexpected support task type: '{supportTask.SupportTaskType}'.", nameof(options))
        };

        var person = await dbContext.Persons.FindOrThrowAsync(supportTask.PersonId!.Value);
        var emailAddress = !string.IsNullOrEmpty(requestEmailAddress) ? requestEmailAddress : person.EmailAddress;

        if (!string.IsNullOrEmpty(emailAddress))
        {
            await SendEmailAsync(
                emailTemplateId,
                emailAddress,
                new Dictionary<string, string>
                {
                    [ChangeRequestEmailConstants.FirstNameEmailPersonalisationKey] = person.FirstName,
                    [ChangeRequestEmailConstants.RejectionReasonEmailPersonalisationKey] = GetRejectionReasonEmailText(options.RejectionReason)
                });
        }
    }

    private static string GetRejectionReasonEmailText(ChangeRequestRejectReason reason) => reason switch
    {
        ChangeRequestRejectReason.RequestAndProofDontMatch => "This is because the proof you provided did not match your request.",
        ChangeRequestRejectReason.WrongTypeOfDocument => "This is because you provided the wrong type of document.",
        ChangeRequestRejectReason.ImageQuality => "This is because the image you provided was not clear enough.",
        _ => throw new ArgumentOutOfRangeException(nameof(reason), reason, null)
    };

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
        SupportRequestOutcome supportRequestOutcome,
        string? rejectionReason,
        ProcessContext processContext)
    {
#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).
        var outcome = (supportTask.SupportTaskType, supportRequestOutcome) switch
        {
            (SupportTaskType.ChangeDateOfBirthRequest, SupportRequestOutcome.Approved) => SupportTaskOutcome.ChangeDateOfBirthRequest_Approved,
            (SupportTaskType.ChangeDateOfBirthRequest, SupportRequestOutcome.Rejected) => SupportTaskOutcome.ChangeDateOfBirthRequest_Rejected,
            (SupportTaskType.ChangeDateOfBirthRequest, SupportRequestOutcome.Cancelled) => SupportTaskOutcome.ChangeDateOfBirthRequest_Cancelled,
            (SupportTaskType.ChangeNameRequest, SupportRequestOutcome.Approved) => SupportTaskOutcome.ChangeNameRequest_Approved,
            (SupportTaskType.ChangeNameRequest, SupportRequestOutcome.Rejected) => SupportTaskOutcome.ChangeNameRequest_Rejected,
            (SupportTaskType.ChangeNameRequest, SupportRequestOutcome.Cancelled) => SupportTaskOutcome.ChangeNameRequest_Cancelled
        };
#pragma warning restore CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

        return supportTask.SupportTaskType switch
        {
            SupportTaskType.ChangeNameRequest => supportTaskService.UpdateSupportTaskAsync(
                new UpdateSupportTaskOptions<ChangeNameRequestData>
                {
                    UpdateData = data => data with { ChangeRequestOutcome = supportRequestOutcome },
                    SupportTaskReference = supportTask.SupportTaskReference,
                    Status = SupportTaskStatus.Closed,
                    Outcome = outcome,
                    RejectionReason = rejectionReason
                },
                processContext),
            SupportTaskType.ChangeDateOfBirthRequest => supportTaskService.UpdateSupportTaskAsync(
                new UpdateSupportTaskOptions<ChangeDateOfBirthRequestData>
                {
                    UpdateData = data => data with { ChangeRequestOutcome = supportRequestOutcome },
                    SupportTaskReference = supportTask.SupportTaskReference,
                    Status = SupportTaskStatus.Closed,
                    Outcome = outcome,
                    RejectionReason = rejectionReason
                },
                processContext),
            _ => throw new ArgumentException(
                $"Unexpected support task type: '{supportTask.SupportTaskType}'.", nameof(supportTask))
        };
    }
}
