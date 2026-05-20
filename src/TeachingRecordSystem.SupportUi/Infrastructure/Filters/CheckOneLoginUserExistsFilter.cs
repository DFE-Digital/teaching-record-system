using System.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

/// <summary>
/// Checks that a OneLoginUser exists with the Subject specified by the oneLoginUserSubject route value or query parameter.
/// </summary>
/// <remarks>
/// <para>Returns a <see cref="StatusCodes.Status400BadRequest"/> response if the request is missing the oneLoginUserSubject route value.</para>
/// <para>
/// Returns a <see cref="StatusCodes.Status404NotFound"/> response if no OneLoginUser with the specified Subject exists.
/// </para>
/// <para>Assigns the <see cref="CurrentOneLoginUserFeature"/> on success.</para>
/// </remarks>
public class CheckOneLoginUserExistsFilter(TrsDbContext dbContext) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var oneLoginUserSubjectParam = context.RouteData.Values["oneLoginUserSubject"] as string ?? context.HttpContext.Request.Query["oneLoginUserSubject"];
        if (string.IsNullOrEmpty(oneLoginUserSubjectParam))
        {
            context.Result = new BadRequestResult();
            return;
        }

        _ = Transaction.Current ?? throw new InvalidOperationException("A TransactionScope is required.");

        var oneLoginUser = await GetOneLoginUserAsync();

        if (oneLoginUser is not null)
        {
            context.HttpContext.SetCurrentOneLoginUserFeature(oneLoginUser);
        }
        else
        {
            context.Result = new NotFoundResult();
            return;
        }

        await next();

        Task<OneLoginUser?> GetOneLoginUserAsync() => dbContext.OneLoginUsers
            .FromSql($"select * from one_login_users where subject = {oneLoginUserSubjectParam} for update")
            .SingleOrDefaultAsync();
    }
}

public class CheckOneLoginUserExistsFilterFactory : IFilterFactory, IOrderedFilter
{
    public bool IsReusable => false;

    public int Order => -200;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<CheckOneLoginUserExistsFilter>(serviceProvider);
}
