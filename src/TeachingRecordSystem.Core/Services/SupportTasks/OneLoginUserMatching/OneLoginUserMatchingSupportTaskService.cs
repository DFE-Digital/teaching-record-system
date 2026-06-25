using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.Core.Services.TrnRequests;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public partial class OneLoginUserMatchingSupportTaskService(
    SupportTaskService supportTaskService,
    OneLoginService oneLoginService,
    TrnRequestService trnRequestService,
    TrsDbContext dbContext)
{
    public Task<ApplicationUser> GetApplicationUserAsync(SupportTask supportTask)
    {
        var applicationUserId = supportTask.Data switch
        {
            OneLoginUserIdVerificationData verificationData => verificationData.ClientApplicationUserId,
            OneLoginUserRecordMatchingData matchingData => matchingData.ClientApplicationUserId,
            _ => throw new ArgumentException($"Unknown task type: '{supportTask.SupportTaskType}'.")
        };

        return dbContext.ApplicationUsers.SingleAsync(u => u.UserId == applicationUserId);
    }

    public Task<AppContent?> GetAppContentAsync(SupportTask supportTask)
    {
        var applicationUserId = supportTask.Data switch
        {
            OneLoginUserIdVerificationData verificationData => verificationData.ClientApplicationUserId,
            OneLoginUserRecordMatchingData matchingData => matchingData.ClientApplicationUserId,
            _ => throw new ArgumentException($"Unknown task type: '{supportTask.SupportTaskType}'.")
        };

        return dbContext.ApplicationUsers
            .Where(u => u.UserId == applicationUserId)
            .Select(u => u.AppContent)
            .SingleAsync();
    }

    public Task<AppContent?> GetAppContentAsync(Guid applicationUserId) =>
        dbContext.ApplicationUsers
            .Where(u => u.UserId == applicationUserId)
            .Select(u => u.AppContent)
            .SingleAsync();
}
