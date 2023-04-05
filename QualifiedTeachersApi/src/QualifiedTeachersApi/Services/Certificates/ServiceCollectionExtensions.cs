#nullable disable
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace QualifiedTeachersApi.Services.Certificates;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCertificateGeneration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddSingleton<ICertificateGenerator, CertificateGenerator>()
            .AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddBlobServiceClient(configuration["StorageConnectionString"]);
            });

        return services;
    }
}
