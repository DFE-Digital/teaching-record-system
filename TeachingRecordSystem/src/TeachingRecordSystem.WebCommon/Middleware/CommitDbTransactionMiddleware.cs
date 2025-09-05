using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.WebCommon.Infrastructure;

namespace TeachingRecordSystem.WebCommon.Middleware;

public class CommitDbTransactionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (context.Items.ContainsKey(typeof(DbTransactionCreatedMarker)))
        {
            var dbContext = context.RequestServices.GetRequiredService<TrsDbContext>();
            if (dbContext.Database.CurrentTransaction is not null)
            {
                await dbContext.Database.CurrentTransaction.CommitAsync();
            }
        }
    }
}
