using System.Text.Json;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Jobs.Scheduling;

namespace TeachingRecordSystem.Core.Jobs;

public class InductionStatusUpdatedSupportJob(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher, IClock clock, IOptions<InductionStatusUpdatedSupportJobOptions> inductionStatusUpdatedSupportJobOptions)
{
    public const string JobSchedule = "0 3 * * *";  //3AM every day
    public const string LastRunDateKey = "LastRunDate";

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var item = await dbContext.JobMetadata.SingleOrDefaultAsync(i => i.JobName == nameof(InductionStatusUpdatedSupportJob));
        var lastRunDate = inductionStatusUpdatedSupportJobOptions.Value.InitialLastRunDate;

        //if the job has ran before - the lastrundate from metadata is used.
        if (item != null)
        {
            if (item.Metadata.TryGetValue(LastRunDateKey, out var obj) &&
                obj is JsonElement jsonElement &&
                jsonElement.ValueKind == JsonValueKind.String)
            {
                lastRunDate = jsonElement.GetDateTime(); ;
            }
        }

        var inductionStatusChangedEvents = await dbContext.Events
           .Where(e => e.EventName == nameof(PersonInductionUpdatedEvent) && e.Created > lastRunDate!.Value.ToUniversalTime())
           .ToListAsync();

        var createTaskEvents = inductionStatusChangedEvents
            .Select(e => ((PersonInductionUpdatedEvent)EventBase.Deserialize(e.Payload, nameof(PersonInductionUpdatedEvent))))
            .Where(x =>
                 ((x.OldInduction.Status == InductionStatus.Exempt && x.Induction.Status == InductionStatus.None) ||
                 (x.OldInduction.Status == InductionStatus.Exempt && x.Induction.Status == InductionStatus.RequiredToComplete) ||
                 (x.OldInduction.Status == InductionStatus.InProgress && x.Induction.Status == InductionStatus.Exempt)));

        foreach (var changeEvent in createTaskEvents)
        {
            await crmQueryDispatcher.ExecuteQueryAsync(
                new CreateTaskQuery()
                {
                    ContactId = changeEvent.PersonId,
                    Category = "Induction Status Updated",
                    Description = $"Induction Status Updated from {changeEvent.OldInduction!.Status.ToString()} to {changeEvent!.Induction.Status.ToString()}"!,
                    Subject = "Induction Status Updated",
                    ScheduledEnd = clock.UtcNow
                });
        }

        //update last run
        if (item != null)
        {
            item.Metadata = new Dictionary<string, object>
            {
                { LastRunDateKey, clock.UtcNow }
            };
        }
        else
        {
            item = new JobMetadata()
            {
                JobName = nameof(InductionStatusUpdatedSupportJob),
                Metadata = new Dictionary<string, object>
                {
                    { LastRunDateKey, clock.UtcNow}
                }
            };
            dbContext.JobMetadata.Add(item);
        }

        await dbContext.SaveChangesAsync();
    }
}
