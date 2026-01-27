using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public partial class OneLoginUserMatchingSupportTaskService
{
    public async Task<SupportTask> CreateOneLoginUserRecordMatchingSupportTaskAsync(
        CreateOneLoginUserRecordMatchingSupportTaskOptions options,
        ProcessContext processContext)
    {
        var supportTask = await supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.OneLoginUserRecordMatching,
                Data = new OneLoginUserRecordMatchingData
                {
                    Verified = options.Verified,
                    OneLoginUserSubject = options.OneLoginUserSubject,
                    OneLoginUserEmail = options.OneLoginUserEmail,
                    VerifiedNames = options.VerifiedNames,
                    VerifiedDatesOfBirth = options.VerifiedDatesOfBirth,
                    StatedNationalInsuranceNumber = options.StatedNationalInsuranceNumber,
                    StatedTrn = options.StatedTrn,
                    ClientApplicationUserId = options.ClientApplicationUserId,
                    TrnTokenTrn = options.TrnTokenTrn
                },
                PersonId = null,
                OneLoginUserSubject = options.OneLoginUserSubject,
                TrnRequest = null
            },
            processContext);

        return supportTask;
    }
}
