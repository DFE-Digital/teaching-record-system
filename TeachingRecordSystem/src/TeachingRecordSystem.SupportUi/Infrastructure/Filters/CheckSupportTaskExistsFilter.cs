using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class CheckSupportTaskExistsFilter(TrsDbContext dbContext, bool openOnly, params SupportTaskType[] supportTaskTypes) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (context.RouteData.Values["supportTaskReference"] is not string supportTaskReference)
        {
            context.Result = new BadRequestResult();
            return;
        }

        _ = Transaction.Current ?? throw new InvalidOperationException("A TransactionScope is required when enqueueing a background job.");

        var currentSupportTaskQuery = dbContext.SupportTasks
            .FromSql($"select * from support_tasks where support_task_reference = {supportTaskReference} for update");  // https://github.com/dotnet/efcore/issues/26042

        if (supportTaskTypes.Contains(SupportTaskType.ApiTrnRequest) || supportTaskTypes.Contains(SupportTaskType.TrnRequestManualChecksNeeded) || supportTaskTypes.Contains(SupportTaskType.NpqTrnRequest))
        {
            currentSupportTaskQuery = currentSupportTaskQuery
                .Include(t => t.TrnRequestMetadata)
                .ThenInclude(m => m!.ApplicationUser);
        }

        var currentSupportTask = await currentSupportTaskQuery.SingleOrDefaultAsync();

        if (currentSupportTask is null ||
            !supportTaskTypes.Contains(currentSupportTask.SupportTaskType) ||
            (openOnly && currentSupportTask.Status != SupportTaskStatus.Open))
        {
            context.Result = new NotFoundResult();
            return;
        }

        context.HttpContext.SetCurrentSupportTaskFeature(new(currentSupportTask));

        await next();
    }
}

public class CheckSupportTaskExistsFilterFactory(bool openOnly, params SupportTaskType[] supportTaskTypes) : IFilterFactory, IOrderedFilter
{
    public bool IsReusable => false;

    public int Order => -200;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<CheckSupportTaskExistsFilter>(serviceProvider, openOnly, supportTaskTypes);
}
