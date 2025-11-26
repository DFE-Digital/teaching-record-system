using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<SupportTask> CreateOneLoginUserIdVerificationDataSupportTaskAsync(string oneLoginUserSubject)
    {
        return await CreateOneLoginUserIdVerificationDataSupportTaskAsync(
            oneLoginUserSubject: oneLoginUserSubject,
            statedFirstName: GenerateFirstName(),
            statedLastName: GenerateLastName(),
            statedDateOfBirth: GenerateDateOfBirth(),
            statedTrn: (await GenerateTrnAsync()));
    }

    public Task<SupportTask> CreateOneLoginUserIdVerificationDataSupportTaskAsync(
        string oneLoginUserSubject,
        string statedFirstName,
        string statedLastName,
        DateOnly statedDateOfBirth,
        string statedTrn,
        Guid clientApplicationUserId = default,
        string? statedNationalInsuranceNumber = null,
        string? trnTokenTrn = null) =>
        WithDbContextAsync(async dbContext =>
        {
            var user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUserSubject);
            Debug.Assert(user.EmailAddress is not null);

            var data = new OneLoginUserIdVerificationData()
            {
                Verified = user.VerificationRoute is not null,
                OneLoginUserSubject = user.Subject,
                EvidenceFileId = Guid.Empty,
                EvidenceFileName = string.Empty,
                PersonId = user.PersonId,
                StatedDateOfBirth = statedDateOfBirth!,
                StatedFirstName = statedFirstName!,
                StatedLastName = statedLastName!,
                StatedNationalInsuranceNumber = statedNationalInsuranceNumber,
                StatedTrn = statedTrn,
                ClientApplicationUserId = clientApplicationUserId,
                TrnTokenTrn = trnTokenTrn
            };

            var reference = SupportTask.GenerateSupportTaskReference();

            var supportTask = new SupportTask()
            {
                CreatedOn = Clock.UtcNow,
                SupportTaskType = SupportTaskType.OneLoginUserIdVerification,
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
