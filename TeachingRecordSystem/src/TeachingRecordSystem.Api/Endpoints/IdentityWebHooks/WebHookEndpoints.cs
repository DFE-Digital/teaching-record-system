namespace TeachingRecordSystem.Api.Endpoints.IdentityWebHooks;

public static class WebHookEndpoints
{
    public static IEndpointConventionBuilder MapWebHookEndpoints(this IEndpointRouteBuilder builder)
    {
        return builder.MapGroup("/webhooks")
            .MapIdentityEndpoints();
    }
}
