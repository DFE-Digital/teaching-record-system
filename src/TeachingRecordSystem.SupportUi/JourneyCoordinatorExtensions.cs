using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;

namespace TeachingRecordSystem.SupportUi;

public static class JourneyCoordinatorExtensions
{
    /// <summary>
    /// Gets the <c>returnUrl</c> from the current request's query string, falling back to
    /// <paramref name="defaultReturnUrl"/> when it's missing or not a local URL.
    /// </summary>
    public static string GetReturnUrlOrDefault(this JourneyCoordinator journeyCoordinator, string defaultReturnUrl)
    {
        var returnUrl = journeyCoordinator.HttpContext.Request.Query["returnUrl"].ToString();

        if (!string.IsNullOrEmpty(returnUrl) && journeyCoordinator.IsLocalUrl(returnUrl))
        {
            return returnUrl;
        }

        return defaultReturnUrl;
    }

    public static bool IsLocalUrl(this JourneyCoordinator journeyCoordinator, string url)
    {
        var httpContext = journeyCoordinator.HttpContext;
        var actionDescriptor = httpContext.GetEndpoint()!.Metadata.GetMetadata<ActionDescriptor>()!;
        var urlHelperFactory = httpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();

        var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), actionDescriptor);
        var urlHelper = urlHelperFactory.GetUrlHelper(actionContext);

        return urlHelper.IsLocalUrl(url);
    }
}
