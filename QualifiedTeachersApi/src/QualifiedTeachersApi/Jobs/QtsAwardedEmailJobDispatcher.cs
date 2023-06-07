﻿using Microsoft.EntityFrameworkCore;
using QualifiedTeachersApi.DataStore.Sql;
using QualifiedTeachersApi.Jobs.Scheduling;

namespace QualifiedTeachersApi.Jobs;

public class QtsAwardedEmailJobDispatcher
{
    private readonly DqtContext _dbContext;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;

    public QtsAwardedEmailJobDispatcher(
        DqtContext dbContext,
        IBackgroundJobScheduler backgroundJobScheduler)
    {
        _dbContext = dbContext;
        _backgroundJobScheduler = backgroundJobScheduler;
    }

    public async Task Execute(Guid qtsAwardedEmailsJobId)
    {
        var jobItems = await _dbContext.QtsAwardedEmailsJobItems
            .Where(i => i.QtsAwardedEmailsJobId == qtsAwardedEmailsJobId && i.EmailSent == false)
            .ToListAsync();

        foreach (var jobItem in jobItems)
        {
            await _backgroundJobScheduler.Enqueue<SendQtsAwardedEmailJob>(j => j.Execute(qtsAwardedEmailsJobId, jobItem.PersonId));
        }
    }
}
