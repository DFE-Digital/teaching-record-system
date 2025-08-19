using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.WebCommon.Infrastructure;

namespace TeachingRecordSystem.WebCommon;

public static class HttpContextExtensions
{
    public static async Task EnsureDbTransactionAsync(this HttpContext context)
    {
        var dbContext = context.RequestServices.GetRequiredService<TrsDbContext>();

        if (dbContext.Database.CurrentTransaction is null && System.Transactions.Transaction.Current is null)
        {
            await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            context.Items.Add(typeof(DbTransactionCreatedMarker), DbTransactionCreatedMarker.Instance);
        }
    }
}
