using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;

public class TeacherPensionsSupportTaskService(SupportTaskService supportTaskService)
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

    public Task ResolveWithMergeAsync(
        ResolveTeacherPensionsPotentialDuplicateWithMergeOptions options,
        ProcessContext processContext) =>
        supportTaskService.UpdateSupportTaskAsync<TeacherPensionsPotentialDuplicateData>(
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

    public Task ResolveWithoutMergeAsync(
        ResolveTeacherPensionsPotentialDuplicateWithoutMergeOptions options,
        ProcessContext processContext) =>
        supportTaskService.UpdateSupportTaskAsync<TeacherPensionsPotentialDuplicateData>(
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
