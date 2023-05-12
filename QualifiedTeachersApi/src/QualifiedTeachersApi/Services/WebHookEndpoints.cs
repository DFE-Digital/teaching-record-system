using QualifiedTeachersApi.Services.GetAnIdentity.WebHooks;

namespace QualifiedTeachersApi.Services;

public static class WebHookEndpoints
{
    public static IEndpointConventionBuilder MapWebHookEndpoints(this IEndpointRouteBuilder builder)
    {
        return builder.MapGroup("/webhooks")
            .MapIdentityEndpoints();
    }
}
