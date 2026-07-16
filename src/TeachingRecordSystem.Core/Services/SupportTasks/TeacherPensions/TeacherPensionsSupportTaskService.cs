using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.TeacherPensions;

public class TeacherPensionsSupportTaskService(SupportTaskService supportTaskService)
{
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
