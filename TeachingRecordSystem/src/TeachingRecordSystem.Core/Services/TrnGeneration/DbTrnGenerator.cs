using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.TrnGeneration;

public class DbTrnGenerator(TrsDbContext dbContext) : ITrnGenerator
{
    public async Task<string> GenerateTrnAsync()
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A database transaction is required to generate a TRN.");
        }

        var nextTrn = await dbContext
            .Set<IntReturn>()
            .FromSqlRaw("SELECT \"fn_generate_trn\" as Value FROM fn_generate_trn()")
            .FirstOrDefaultAsync();

        if (nextTrn?.Value is null)
        {
            throw new InvalidOperationException("Failed to generate a TRN.");
        }

        return nextTrn.Value.Value.ToString("D7");
    }
}
