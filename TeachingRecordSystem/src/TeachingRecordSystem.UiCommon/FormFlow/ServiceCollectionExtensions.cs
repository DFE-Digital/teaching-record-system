using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TeachingRecordSystem.UiCommon.FormFlow.Conventions;
using TeachingRecordSystem.UiCommon.FormFlow.Filters;
using TeachingRecordSystem.UiCommon.FormFlow.ModelBinding;
using TeachingRecordSystem.UiCommon.FormFlow.State;

namespace TeachingRecordSystem.UiCommon.FormFlow;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFormFlow(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<JourneyInstanceProvider>();
        services.TryAddSingleton<IStateSerializer, JsonStateSerializer>();
        services.TryAddSingleton<IUserInstanceStateProvider, DbWithHttpContextTransactionUserInstanceStateProvider>();
        services.AddOptions<State.JsonOptions>();
        services.AddScoped<MissingInstanceFilter>();
        services.AddScoped<ActivateInstanceFilter>();
        services.AddSingleton<IStartupFilter, CommitStateChangesStartupFilter>();

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
            options.Conventions.Add(new BindJourneyInstancePropertiesConvention());
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

    private class CommitStateChangesStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.UseMiddleware<CommitStateChangesMiddleware>();
            next(app);
        };
    }
}
