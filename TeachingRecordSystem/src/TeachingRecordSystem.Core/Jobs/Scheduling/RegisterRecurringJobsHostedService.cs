using Hangfire;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.Core.Jobs.Scheduling;

public class RegisterRecurringJobsHostedService : IHostedService
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly RecurringJobsOptions _recurringJobsOptions;

    public RegisterRecurringJobsHostedService(
        IRecurringJobManager recurringJobManager,
        IOptions<RecurringJobsOptions> recurringJobsOptions)
    {
        _recurringJobManager = recurringJobManager;
        _recurringJobsOptions = recurringJobsOptions.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        RegisterJobs();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void RegisterJobs()
    {
        _recurringJobManager.AddOrUpdate<BatchSendQtsAwardedEmailsJob>(nameof(BatchSendQtsAwardedEmailsJob), job => job.Execute(CancellationToken.None), _recurringJobsOptions.BatchSendQtsAwardedEmails.JobSchedule);
        _recurringJobManager.AddOrUpdate<BatchSendInternationalQtsAwardedEmailsJob>(nameof(BatchSendInternationalQtsAwardedEmailsJob), job => job.Execute(CancellationToken.None), _recurringJobsOptions.BatchSendInternationalQtsAwardedEmails.JobSchedule);
        _recurringJobManager.AddOrUpdate<BatchSendEytsAwardedEmailsJob>(nameof(BatchSendEytsAwardedEmailsJob), job => job.Execute(CancellationToken.None), _recurringJobsOptions.BatchSendEytsAwardedEmails.JobSchedule);
        _recurringJobManager.AddOrUpdate<BatchSendInductionCompletedEmailsJob>(nameof(BatchSendInductionCompletedEmailsJob), job => job.Execute(CancellationToken.None), _recurringJobsOptions.BatchSendInductionCompletedEmails.JobSchedule);
        _recurringJobManager.AddOrUpdate<MopUpQtsAwardeesJob>(nameof(MopUpQtsAwardeesJob), job => job.Execute(CancellationToken.None), Cron.Never);
    }
}
