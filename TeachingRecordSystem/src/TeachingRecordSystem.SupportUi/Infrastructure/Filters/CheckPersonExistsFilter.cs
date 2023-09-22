using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class CheckPersonExistsFilter : IAsyncPageFilter
{
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personId = context.RouteData.Values["personId"] as string;
        if (personId is not null)
        {
            var crmQueryDispatcher = context.HttpContext.RequestServices.GetRequiredService<ICrmQueryDispatcher>();
            var person = await crmQueryDispatcher.ExecuteQuery(new GetContactDetailByIdQuery(Guid.Parse(personId), new ColumnSet(Contact.Fields.Id)));
            if (person is null)
            {
                context.Result = new NotFoundResult();
                return;
            }
        }

        await next();
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;
}
