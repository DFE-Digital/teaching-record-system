using TeachingRecordSystem.Api.Services.GetAnIdentity.WebHooks;

namespace TeachingRecordSystem.Api.Services;

public static class WebHookEndpoints
{
    public static IEndpointConventionBuilder MapWebHookEndpoints(this IEndpointRouteBuilder builder)
    {
        return builder.MapGroup("/webhooks")
            .MapIdentityEndpoints();
    }
}
