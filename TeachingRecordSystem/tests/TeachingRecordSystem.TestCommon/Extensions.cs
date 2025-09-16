using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Events.EventHandlers;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.TestCommon;

public static class Extensions
{
    public static IServiceCollection AddEventObserver(this IServiceCollection services)
    {
        services.AddSingleton<IEventHandler, EventObserverEventHandler>();

        return services;
    }
}
