using FormFlow.Filters;
using FormFlow.ModelBinding;
using FormFlow.State;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FormFlow;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFormFlow(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<JourneyInstanceProvider>();
        services.TryAddSingleton<IStateSerializer, JsonStateSerializer>();
        //services.TryAddSingleton<IUserInstanceStateProvider, SessionUserInstanceStateProvider>();
        services.AddOptions<State.JsonOptions>();
        services.AddScoped<MissingInstanceFilter>();
        services.AddScoped<ActivateInstanceFilter>();

        var conventions = new FormFlowConventions();

        services.Configure<MvcOptions>(options =>
        {
            options.Conventions.Add((IControllerModelConvention)conventions);
            options.Conventions.Add((IActionModelConvention)conventions);

            options.Filters.Add(new ServiceFilterAttribute(typeof(MissingInstanceFilter)) { Order = MissingInstanceFilter.Order });
            options.Filters.Add(new ServiceFilterAttribute(typeof(ActivateInstanceFilter)) { Order = ActivateInstanceFilter.Order });

            options.ModelBinderProviders.Insert(0, new JourneyInstanceModelBinderProvider());
        });

        services.Configure<RazorPagesOptions>(options =>
        {
            options.Conventions.Add(conventions);
        });

        return services;
    }

    public static IServiceCollection AddFormFlow(
        this IServiceCollection services,
        Action<FormFlowOptions> configure)
    {
        services.Configure(configure);
        services.AddFormFlow();

        return services;
    }
}
