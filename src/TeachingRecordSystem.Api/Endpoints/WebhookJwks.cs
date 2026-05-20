using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Api.Endpoints;

public static class WebhookJwks
{
    public static IEndpointConventionBuilder MapWebhookJwks(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapGet("/webhook-jwks", async ctx =>
        {
            var webhookOptions = ctx.RequestServices.GetRequiredService<IOptions<WebhookOptions>>();
            var jsonWebKeySet = webhookOptions.Value.GetJsonWebKeySet();
            await ctx.Response.WriteAsJsonAsync(jsonWebKeySet);
        });
    }
}
