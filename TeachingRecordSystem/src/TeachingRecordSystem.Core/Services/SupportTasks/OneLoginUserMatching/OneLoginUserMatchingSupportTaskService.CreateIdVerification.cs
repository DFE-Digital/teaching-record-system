using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public partial class OneLoginUserMatchingSupportTaskService
{
    public async Task<SupportTask> CreateOneLoginUserIdVerificationSupportTaskAsync(
        CreateOneLoginUserIdVerificationSupportTaskOptions options,
        ProcessContext processContext)
    {
        var supportTask = await supportTaskService.CreateSupportTaskAsync(
            new CreateSupportTaskOptions
            {
                SupportTaskType = SupportTaskType.OneLoginUserIdVerification,
                Data = new OneLoginUserIdVerificationData
                {
                    OneLoginUserSubject = options.OneLoginUserSubject,
                    StatedNationalInsuranceNumber = options.StatedNationalInsuranceNumber,
                    StatedTrn = options.StatedTrn,
                    ClientApplicationUserId = options.ClientApplicationUserId,
                    TrnTokenTrn = options.TrnTokenTrn,
                    StatedFirstName = options.StatedFirstName,
                    StatedLastName = options.StatedLastName,
                    StatedDateOfBirth = options.StatedDateOfBirth,
                    EvidenceFileId = options.EvidenceFileId,
                    EvidenceFileName = options.EvidenceFileName
                },
                PersonId = null,
                OneLoginUserSubject = options.OneLoginUserSubject,
                TrnRequest = null
            },
            processContext);

        return supportTask;
    }
}
