using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public partial class RedirectWithPersonIdFilter(TrsDbContext dbContext) : IAsyncResourceFilter
{
    [GeneratedRegex(@"^\/persons\/([0-9]{7})($|\/)")]
    private static partial Regex _personWithTrnPathRegex();

    public static int Order => FilterOrders.RedirectWithPersonIdFilterOrder;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var matched = _personWithTrnPathRegex().Match(context.HttpContext.Request.Path);

        if (matched.Success)
        {
            var trn = matched.Groups[1].Value;

            var person = await dbContext.Persons
                .Where(p => p.Trn == trn)
                .Select(p => new { p.PersonId })
                .SingleOrDefaultAsync();

            if (person is not null)
            {
                var newUrl = context.HttpContext.Request.Path.ToString().Replace($"/persons/{trn}", $"/persons/{person.PersonId}");
                context.Result = new RedirectResult(newUrl, permanent: false, preserveMethod: true);
                return;
            }
        }

        await next();
    }
}
