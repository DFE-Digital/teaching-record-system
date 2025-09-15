using System.Transactions;
using Microsoft.AspNetCore.Http;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.WebCommon.Middleware;

public class TransactionScopeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, TrsDbContext dbContext)
    {
        using var txn = new TransactionScope(
            TransactionScopeOption.RequiresNew,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        await next(context);

        txn.Complete();
    }
}
