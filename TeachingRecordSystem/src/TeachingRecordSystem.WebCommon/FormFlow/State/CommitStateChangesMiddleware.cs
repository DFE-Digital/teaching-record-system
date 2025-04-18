using Microsoft.AspNetCore.Http;

namespace TeachingRecordSystem.WebCommon.FormFlow.State;

public class CommitStateChangesMiddleware(IUserInstanceStateProvider userInstanceStateProvider, RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        finally
        {
            if (userInstanceStateProvider is DbWithHttpContextTransactionUserInstanceStateProvider typedProvider)
            {
                await typedProvider.CommitChangesAsync();
            }
        }
    }
}
