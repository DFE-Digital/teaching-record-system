using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace TeachingRecordSystem.Core.Services.DqtOutbox;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddDqtOutboxMessageSerializer(this IHostApplicationBuilder builder)
    {
        builder.Services.TryAddSingleton<MessageSerializer>();

        return builder;
    }

    public static IHostApplicationBuilder AddDqtOutboxMessageHandler(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<OutboxMessageHandler>();

        return builder;
    }

    public static IHostApplicationBuilder AddDqtOutboxMessageProcessorService(this IHostApplicationBuilder builder)
    {
        AddDqtOutboxMessageSerializer(builder);

        builder.Services.AddSingleton<IHostedService, DqtOutboxMessageProcessorService>();

        return builder;
    }
}
