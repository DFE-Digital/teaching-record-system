using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.ChangeRequests;

public class ChangeRequestSupportTaskService(SupportTaskService supportTaskService)
{
    public Task<SupportTask> CreateNameChangeRequestAsync(
        CreateNameChangeRequestSupportTaskOptions options,
        ProcessContext processContext)
    {
        return supportTaskService.CreateSupportTaskAsync(
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
                TrnRequest = null
            },
            processContext);
    }

    public Task<SupportTask> CreateDateOfBirthChangeRequestAsync(
        CreateDateOfBirthChangeRequestSupportTaskOptions options,
        ProcessContext processContext)
    {
        return supportTaskService.CreateSupportTaskAsync(
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
                TrnRequest = null
            },
            processContext);
    }

    public Task ApproveChangeRequestAsync(ApproveChangeRequestSupportTaskOptions options, ProcessContext processContext) =>
        ResolveChangeRequestAsync(options.SupportTask, SupportRequestOutcome.Approved, rejectionReason: null, processContext);

    public Task RejectChangeRequestAsync(RejectChangeRequestSupportTaskOptions options, ProcessContext processContext) =>
        ResolveChangeRequestAsync(options.SupportTask, SupportRequestOutcome.Rejected, options.RejectionReason, processContext);

    public Task CancelChangeRequestAsync(CancelChangeRequestSupportTaskOptions options, ProcessContext processContext) =>
        ResolveChangeRequestAsync(options.SupportTask, SupportRequestOutcome.Cancelled, rejectionReason: null, processContext);

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
