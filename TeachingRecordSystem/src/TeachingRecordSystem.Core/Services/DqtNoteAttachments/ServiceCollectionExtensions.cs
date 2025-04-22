using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.DqtNoteAttachments;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBlobStorageDqtNoteAttachments(this IServiceCollection services)
    {
        services.AddSingleton<IDqtNoteAttachmentStorage, BlobStorageDqtNoteAttachmentStorage>();

        return services;
    }
}
