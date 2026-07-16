using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;

public class TeacherPensionsSupportTaskService(
    SupportTaskService supportTaskService,
    TrsDbContext dbContext,
    IEventPublisher eventPublisher,
    PersonService personService,
    TrnRequestService trnRequestService)
{
    public Task<SupportTask> CreatePotentialDuplicateAsync(
        CreateTeacherPensionsPotentialDuplicateOptions options,
        ProcessContext processContext) =>
        supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.TeacherPensionsPotentialDuplicate,
                Data = new TeacherPensionsPotentialDuplicateData
                {
                    FileName = options.FileName,
                    IntegrationTransactionId = options.IntegrationTransactionId
                },
                PersonId = options.PersonId,
                OneLoginUserSubject = null,
                TrnRequest = (options.TrnRequest.ApplicationUserId, options.TrnRequest.RequestId),
                Subject = SupportTask.Subject.FromTrnRequest(options.TrnRequest)
            },
            processContext);

    /// Merges the task's record into the record in <paramref name="options"/>, resolves the request to it and closes
    /// the task.
    public async Task ResolveWithMergeAsync(
        ResolveTeacherPensionsPotentialDuplicateWithMergeOptions options,
        ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var supportTask = await GetOpenSupportTaskAsync(options.SupportTaskReference);

        await personService.DeactivatePersonViaMergeAsync(
            new DeactivatePersonViaMergeOptions(supportTask.PersonId!.Value, options.ExistingPersonId),
            processContext);

        await trnRequestService.ResolveTrnRequestWithMatchedPersonAsync(
            supportTask.TrnRequestApplicationUserId!.Value,
            supportTask.TrnRequestId!,
            options.ExistingPersonId,
            options.AttributesToUpdate,
            processContext);

        await supportTaskService.UpdateSupportTaskAsync<TeacherPensionsPotentialDuplicateData>(
            new UpdateSupportTaskOptions<TeacherPensionsPotentialDuplicateData>
            {
                SupportTaskReference = options.SupportTaskReference,
                UpdateData = data => data with
                {
                    ResolvedAttributes = options.ResolvedAttributes,
                    SelectedPersonAttributes = options.SelectedPersonAttributes
                },
                Status = SupportTaskStatus.Closed,
                Comments = options.Comments
            },
            processContext);
    }

    // Both resolutions update the person and the request before closing the task, so check up-front that the task can
    // be closed rather than leaving those behind when it can't
    private async Task<SupportTask> GetOpenSupportTaskAsync(string supportTaskReference)
    {
        var supportTask = await dbContext.SupportTasks.FindOrThrowAsync(supportTaskReference);

        if (supportTask.Status is SupportTaskStatus.Closed)
        {
            throw new InvalidOperationException("Support task is closed.");
        }

        return supportTask;
    }

    /// Keeps the task's record separate, resolving the request to it and closing the task.
    public async Task ResolveWithoutMergeAsync(
        ResolveTeacherPensionsPotentialDuplicateWithoutMergeOptions options,
        ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var supportTask = await GetOpenSupportTaskAsync(options.SupportTaskReference);

        await trnRequestService.ResolveTrnRequestWithMatchedPersonAsync(
            supportTask.TrnRequestApplicationUserId!.Value,
            supportTask.TrnRequestId!,
            supportTask.PersonId!.Value,
            processContext);

        await supportTaskService.UpdateSupportTaskAsync<TeacherPensionsPotentialDuplicateData>(
            new UpdateSupportTaskOptions<TeacherPensionsPotentialDuplicateData>
            {
                SupportTaskReference = options.SupportTaskReference,
                UpdateData = data => data with
                {
                    ResolvedAttributes = null,
                    SelectedPersonAttributes = null
                },
                Status = SupportTaskStatus.Closed,
                Comments = options.Comments
            },
            processContext);
    }
}
