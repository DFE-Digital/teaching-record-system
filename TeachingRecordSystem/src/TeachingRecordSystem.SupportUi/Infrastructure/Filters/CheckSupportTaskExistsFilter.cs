using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class CheckSupportTaskExistsFilter(TrsDbContext dbContext, bool openOnly, SupportTaskType? supportTaskType) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        if (context.RouteData.Values["supportTaskReference"] is not string supportTaskReference)
        {
            context.Result = new BadRequestResult();
            return;
        }

        var currentSupportTask = await dbContext.SupportTasks.SingleOrDefaultAsync(t => t.SupportTaskReference == supportTaskReference);

        if (currentSupportTask is null ||
            (supportTaskType is SupportTaskType type && currentSupportTask.SupportTaskType != type) ||
            (openOnly && currentSupportTask.Status != SupportTaskStatus.Open))
        {
            context.Result = new NotFoundResult();
            return;
        }

        context.HttpContext.SetCurrentSupportTaskFeature(new(currentSupportTask));

        await next();
    }
}

public class CheckSupportTaskExistsFilterFactory(bool openOnly, SupportTaskType? supportTaskType = null) : IFilterFactory, IOrderedFilter
{
    public bool IsReusable => false;

    public int Order => -200;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<CheckSupportTaskExistsFilter>(serviceProvider, openOnly, supportTaskType!);
}
