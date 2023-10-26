using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class CheckPersonExistsFilter : IAsyncResourceFilter, IOrderedFilter
{
    public int Order => -200;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
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
}
