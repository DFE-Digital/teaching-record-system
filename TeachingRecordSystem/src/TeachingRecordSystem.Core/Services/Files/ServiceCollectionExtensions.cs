using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.Files;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileService(this IServiceCollection services)
    {
        services.AddSingleton<IFileService, BlobStorageFileService>();

        return services;
    }
}
