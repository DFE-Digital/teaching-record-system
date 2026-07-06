using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.MandatoryQualifications;

public static class Extensions
{
    public static IServiceCollection AddMandatoryQualificationService(this IServiceCollection services)
    {
        services.AddTransient<MandatoryQualificationService>();

        return services;
    }
}
