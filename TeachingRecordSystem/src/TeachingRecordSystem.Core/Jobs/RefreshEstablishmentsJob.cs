using System.Transactions;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Establishments.Gias;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Jobs;

public class RefreshEstablishmentsJob(IBackgroundJobScheduler backgroundJobScheduler)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var txn = new TransactionScope(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
        var refreshJobId = await backgroundJobScheduler.EnqueueAsync<EstablishmentRefresher>(j => j.RefreshEstablishmentsAsync(cancellationToken));
        txn.Complete();
        await backgroundJobScheduler.ContinueJobWithAsync<TpsCsvExtractProcessor>(refreshJobId, j => j.UpdateLatestEstablishmentVersionsAsync(cancellationToken));
    }
}
