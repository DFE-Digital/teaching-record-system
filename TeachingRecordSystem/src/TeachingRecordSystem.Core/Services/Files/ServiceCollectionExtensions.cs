using Microsoft.Extensions.DependencyInjection;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.DqtNoteAttachments;

namespace TeachingRecordSystem.Core.Services.Files;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileService(this IServiceCollection services)
    {
        services.AddSingleton<IFileService, BlobStorageFileService>();

        return services;
    }
}
