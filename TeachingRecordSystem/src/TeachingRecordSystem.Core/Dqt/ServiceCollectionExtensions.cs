using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Dqt;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddCrmQueries(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssembliesOf(typeof(ICrmQuery<>))
            .AddClasses(classes => classes.AssignableTo(typeof(ICrmQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

        services.AddTransient<ICrmQueryDispatcher, CrmQueryDispatcher>();

        return services;
    }
}
