﻿using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Api.Jobs.Scheduling;

namespace TeachingRecordSystem.Api.Jobs;

public class InductionCompletedEmailJobDispatcher
{
    private readonly TrsContext _dbContext;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;

    public InductionCompletedEmailJobDispatcher(
        TrsContext dbContext,
        IBackgroundJobScheduler backgroundJobScheduler)
    {
        _dbContext = dbContext;
        _backgroundJobScheduler = backgroundJobScheduler;
    }

    public async Task Execute(Guid inductionCompletedEmailsJobId)
    {
        var jobItems = await _dbContext.InductionCompletedEmailsJobItems
            .Where(i => i.InductionCompletedEmailsJobId == inductionCompletedEmailsJobId && i.EmailSent == false)
            .ToListAsync();

        foreach (var jobItem in jobItems)
        {
            await _backgroundJobScheduler.Enqueue<SendInductionCompletedEmailJob>(j => j.Execute(inductionCompletedEmailsJobId, jobItem.PersonId));
        }
    }
}
