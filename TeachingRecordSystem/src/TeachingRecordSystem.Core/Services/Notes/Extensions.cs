using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.Notes;

public static class Extensions
{
    public static IServiceCollection AddNoteService(this IServiceCollection services)
    {
        services.AddTransient<NoteService>();

        return services;
    }
}
