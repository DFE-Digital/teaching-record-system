using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.Core.Services.SupportTaskNotes;

public static class Extensions
{
    public static IServiceCollection AddSupportTaskNoteService(this IServiceCollection services)
    {
        services.AddTransient<SupportTaskNoteService>();

        return services;
    }
}
