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
                .WithSingletonLifetime());

        services.AddSingleton<ICrmQueryDispatcher, CrmQueryDispatcher>();

        return services;
    }
}
