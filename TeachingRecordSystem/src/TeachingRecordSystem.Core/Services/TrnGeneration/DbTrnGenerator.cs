using System.Diagnostics;
using System.Transactions;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.TrnGeneration;

public class DbTrnGenerator(TrsDbContext dbContext) : ITrnGenerator
{
    public async Task<string> GenerateTrnAsync()
    {
        Debug.Assert(Transaction.Current is not null);

        var nextTrn = await dbContext
            .Database
            .SqlQueryRaw<IntReturn>("SELECT \"fn_generate_trn\" as Value FROM fn_generate_trn()")
            .FirstOrDefaultAsync();

        if (nextTrn?.Value is null)
        {
            throw new InvalidOperationException("Failed to generate a TRN.");
        }

        return nextTrn.Value.Value.ToString("D7");
    }
}
