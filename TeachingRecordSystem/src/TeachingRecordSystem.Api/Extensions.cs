namespace TeachingRecordSystem.Api;

public static class Extensions
{
    public static IServiceCollection AddApiCommands(this IServiceCollection services)
    {
        services.Scan(scan =>
            scan.FromAssemblyOf<Program>()
                .AddClasses(filter => filter.AssignableTo(typeof(ICommandHandler<,>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());

        services.AddTransient<ICommandDispatcher, CommandDispatcher>();

        return services;
    }
}
