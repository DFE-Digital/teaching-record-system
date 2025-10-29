using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillCapitaImportWarningStatusesJob(TrsDbContext dbContext)
{
    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var integrationTransactionsToBackfill = await dbContext.IntegrationTransactions
            .Include(it => it.IntegrationTransactionRecords)
            .Where(it => it.InterfaceType == IntegrationTransactionInterfaceType.CapitaImport)
            .ToListAsync(cancellationToken);

        foreach (var integrationTransaction in integrationTransactionsToBackfill)
        {
            int warningCount = integrationTransaction.WarningCount;
            int successCount = integrationTransaction.SuccessCount;

            foreach (var record in integrationTransaction.IntegrationTransactionRecords!.Where(r => r.Status == IntegrationTransactionRecordStatus.Success && !string.IsNullOrEmpty(r.FailureMessage)))
            {
                record.Status = IntegrationTransactionRecordStatus.Warning;
                warningCount++;
                successCount--;
            }

            integrationTransaction.WarningCount = warningCount;
            integrationTransaction.SuccessCount = successCount;

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (dryRun)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        else
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }
}
