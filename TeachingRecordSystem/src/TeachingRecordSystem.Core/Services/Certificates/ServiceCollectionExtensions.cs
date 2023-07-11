using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.Certificates;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCertificateGeneration(this IServiceCollection services)
    {
        services.AddSingleton<ICertificateGenerator, CertificateGenerator>();

        return services;
    }
}
