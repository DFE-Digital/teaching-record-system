using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class CheckPersonExistsFilter : IAsyncResourceFilter, IOrderedFilter
{
    private readonly bool _requireQts;

    public int Order => -200;

    public CheckPersonExistsFilter(bool requireQts = false)
    {
        _requireQts = requireQts;
    }

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var personIdParam = context.RouteData.Values["personId"] as string ?? context.HttpContext.Request.Query["personId"];
        if (personIdParam is null || !Guid.TryParse(personIdParam, out Guid personId))
        {
            context.Result = new BadRequestResult();
            return;
        }

        var crmQueryDispatcher = context.HttpContext.RequestServices.GetRequiredService<ICrmQueryDispatcher>();
        var person = await crmQueryDispatcher.ExecuteQuery(
            new GetContactDetailByIdQuery(
                personId,
                new ColumnSet(
                    Contact.Fields.Id,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedLastName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_QTSDate)));
        if (person is null)
        {
            context.Result = new NotFoundResult();
            return;
        }
        else
        {
            if (_requireQts && person.Contact.dfeta_QTSDate is null)
            {
                context.Result = new BadRequestResult();
                return;
            }
        }

        context.HttpContext.Items["CurrentPersonDetail"] = person;

        await next();
    }
}
