using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TeachingRecordSystem.WebCommon.FormFlow;

public interface IJourneyStateFactory<TState>
{
    Task<TState> CreateAsync(CreateJourneyStateContext context);
}

public record CreateJourneyStateContext(JourneyDescriptor Journey, JourneyInstanceId InstanceId, HttpContext HttpContext)
{
    public RouteData RouteData => HttpContext.GetRouteData();
}
