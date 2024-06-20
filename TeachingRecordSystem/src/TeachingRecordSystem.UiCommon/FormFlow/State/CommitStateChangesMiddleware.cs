using Microsoft.AspNetCore.Http;

namespace TeachingRecordSystem.UiCommon.FormFlow.State;

public class CommitStateChangesMiddleware(IUserInstanceStateProvider userInstanceStateProvider, RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        finally
        {
            if (userInstanceStateProvider is DbWithHttpContextTransactionUserInstanceStateProvider typedProvider)
            {
                await typedProvider.CommitChanges();
            }
        }
    }
}
