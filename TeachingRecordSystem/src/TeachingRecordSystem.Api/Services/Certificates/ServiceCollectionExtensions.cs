namespace TeachingRecordSystem.Api.Services.Certificates;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCertificateGeneration(this IServiceCollection services)
    {
        services.AddSingleton<ICertificateGenerator, CertificateGenerator>();

        return services;
    }
}
