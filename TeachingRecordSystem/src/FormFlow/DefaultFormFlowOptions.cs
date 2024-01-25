using Microsoft.AspNetCore.Mvc;

namespace FormFlow;

public static class DefaultFormFlowOptions
{
    public static MissingInstanceHandler MissingInstanceHandler { get; } =
        (journeyDescriptor, httpContext, statusCode) => new StatusCodeResult(statusCode ?? 404);
}
