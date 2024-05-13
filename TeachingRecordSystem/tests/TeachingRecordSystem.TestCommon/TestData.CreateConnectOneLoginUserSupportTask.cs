using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<SupportTask> CreateConnectOneLoginUserSupportTask(
        string oneLoginUserSubject,
        Guid clientApplicationUserId = default,
        string? statedNationalInsuranceNumber = null,
        string? statedTrn = null,
        string? trnTokenTrn = null) =>
        WithDbContext(async dbContext =>
        {
            var user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUserSubject);

            var data = new ConnectOneLoginUserData()
            {
                Verified = user.VerificationRoute is not null,
                OneLoginUserSubject = user.Subject,
                OneLoginUserEmail = user.Email,
                VerifiedNames = user.VerifiedNames,
                VerifiedDatesOfBirth = user.VerifiedDatesOfBirth,
                StatedNationalInsuranceNumber = statedNationalInsuranceNumber,
                StatedTrn = statedTrn,
                ClientApplicationUserId = clientApplicationUserId,
                TrnTokenTrn = trnTokenTrn
            };

            var reference = SupportTask.GenerateSupportTaskReference();

            var supportTask = new SupportTask()
            {
                CreatedOn = Clock.UtcNow,
                SupportTaskType = SupportTaskType.ConnectOneLoginUser,
                Data = data,
                OneLoginUserSubject = oneLoginUserSubject,
                Status = SupportTaskStatus.Open,
                SupportTaskReference = reference,
                UpdatedOn = Clock.UtcNow
            };

            dbContext.SupportTasks.Add(supportTask);
            await dbContext.SaveChangesAsync();

            return supportTask;
        });
}
