using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<SupportTask> CreateTrnRequestManualChecksNeededSupportTaskAsync(
        Guid? applicationUserId = null,
        SupportTaskStatus status = SupportTaskStatus.Open,
        DateTime? createdOn = null,
        Action<CreateTrnRequestSupportTaskBuilder>? configureTrnRequest = null)
    {
        var matchedPerson = await CreatePersonAsync(p => p.WithEmailAddress(GenerateUniqueEmail()).WithAlert().WithQts().WithEyts());

        if (applicationUserId is null)
        {
            var applicationUser = await CreateApplicationUserAsync();
            applicationUserId = applicationUser.UserId;
        }

        (var apiSupportTask, _, _) = await CreateResolvedApiTrnRequestSupportTaskAsync(
            applicationUserId.Value,
            matchedPerson.Person,
            t =>
            {
                t.WithTrnRequestStatus(TrnRequestStatus.Pending);
                configureTrnRequest?.Invoke(t);
            });

        return await CreateTrnRequestManualChecksNeededSupportTaskAsync(
            applicationUserId.Value,
            apiSupportTask.TrnRequestMetadata!.RequestId,
            status,
            createdOn);
    }

    public Task<SupportTask> CreateTrnRequestManualChecksNeededSupportTaskAsync(
        Guid trnRequestApplicationUserId,
        string trnRequestId,
        SupportTaskStatus status = SupportTaskStatus.Open,
        DateTime? createdOn = null)
    {
        return WithDbContextAsync(async dbContext =>
        {
            var createdOnUtc = (createdOn ?? TimeProvider.UtcNow).ToUniversalTime();

            var trnRequestMetadata = await dbContext.TrnRequestMetadata.FindAsync(trnRequestApplicationUserId, trnRequestId) ??
                throw new InvalidOperationException("TRN request does not exist.");

            var person = await dbContext.Persons.FindAsync(trnRequestMetadata.ResolvedPersonId) ??
                throw new InvalidOperationException("Person does not exist.");

            var subject = SupportTask.Subject.FromPerson(person);

            var task = new SupportTask
            {
                CreatedOn = createdOnUtc,
                UpdatedOn = createdOnUtc,
                SupportTaskType = SupportTaskType.TrnRequestManualChecksNeeded,
                Status = status,
                Data = new TrnRequestManualChecksNeededData(),
                TrnRequestApplicationUserId = trnRequestApplicationUserId,
                TrnRequestId = trnRequestId,
                SubjectName = subject.Name,
                SubjectEmailAddress = subject.EmailAddress
            };

            dbContext.SupportTasks.Add(task);
            await dbContext.SaveChangesAsync();

            // Re-query what we've just added so we return a SupportTask with TrnRequestMetadata populated
            return await dbContext.SupportTasks
                .Include(t => t.TrnRequestMetadata)
                .ThenInclude(m => m!.ApplicationUser)
                .SingleAsync(t => t.SupportTaskReference == task.SupportTaskReference);
        });
    }
}
