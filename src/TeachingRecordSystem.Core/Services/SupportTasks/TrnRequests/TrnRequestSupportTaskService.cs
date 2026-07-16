using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.TrnRequests;

public class TrnRequestSupportTaskService(SupportTaskService supportTaskService)
{
    public Task<SupportTask> CreateTrnRequestSupportTaskAsync(
        CreateTrnRequestSupportTaskOptions options,
        ProcessContext processContext) =>
        supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.TrnRequest,
                Data = new TrnRequestData(),
                PersonId = null,
                OneLoginUserSubject = null,
                TrnRequest = (options.TrnRequest.ApplicationUserId, options.TrnRequest.RequestId),
                Subject = SupportTask.Subject.FromTrnRequest(options.TrnRequest)
            },
            processContext);

    public Task<SupportTask> CreateManualChecksNeededSupportTaskAsync(
        CreateManualChecksNeededSupportTaskOptions options,
        ProcessContext processContext) =>
        supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.TrnRequestManualChecksNeeded,
                Data = new TrnRequestManualChecksNeededData(),
                PersonId = options.Person.PersonId,
                OneLoginUserSubject = null,
                TrnRequest = (options.TrnRequest.ApplicationUserId, options.TrnRequest.RequestId),
                Subject = SupportTask.Subject.FromPerson(options.Person)
            },
            processContext);

    public Task ResolveTrnRequestSupportTaskAsync(
        ResolveTrnRequestSupportTaskOptions options,
        ProcessContext processContext) =>
        supportTaskService.UpdateSupportTaskAsync<TrnRequestData>(
            new UpdateSupportTaskOptions<TrnRequestData>
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

    public Task CompleteManualChecksNeededSupportTaskAsync(
        CompleteManualChecksNeededSupportTaskOptions options,
        ProcessContext processContext) =>
        supportTaskService.UpdateSupportTaskAsync<TrnRequestManualChecksNeededData>(
            new UpdateSupportTaskOptions<TrnRequestManualChecksNeededData>
            {
                SupportTaskReference = options.SupportTaskReference,
                UpdateData = data => data,
                Status = SupportTaskStatus.Closed
            },
            processContext);
}
