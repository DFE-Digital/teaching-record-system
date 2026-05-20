using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.Persons;

public static class Extensions
{
    public static IServiceCollection AddPersonService(this IServiceCollection services)
    {
        services.AddTransient<PersonService>();

        return services;
    }
}
