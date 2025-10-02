using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.Api.UnitTests;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices(IServiceProvider serviceProvider)
    {
        Clock = new();
        GetAnIdentityApiClient = new();
        EventObserver = new();
        FeatureProvider = ActivatorUtilities.CreateInstance<TestableFeatureProvider>(serviceProvider);
        TrnRequestOptions = new();
        BlobStorageFileService = new();
        BackgroundJobScheduler = new(serviceProvider);
    }

    public static void ConfigureServices(IServiceCollection services) =>
        services
            .AddTestScoped<IClock>(tss => tss.Clock)
            .AddTestScoped(tss => tss.GetAnIdentityApiClient.Object)
            .AddTestScoped(tss => tss.BlobStorageFileService.Object)
            .AddTestScoped<IFeatureProvider>(tss => tss.FeatureProvider)
            .AddTestScoped(tss => tss.EventObserver)
            .AddTestScoped(tss => Options.Create(tss.TrnRequestOptions))
            .AddTestScoped<IBackgroundJobScheduler>(tss => tss.BackgroundJobScheduler);

    public static TestScopedServices GetCurrent() =>
        TryGetCurrent(out var current) ? current : throw new InvalidOperationException("No current instance has been set.");

    public static TestScopedServices Reset(IServiceProvider serviceProvider)
    {
        if (_current.Value is not null)
        {
            throw new InvalidOperationException("Current instance has already been set.");
        }

        return _current.Value = new(serviceProvider);
    }

    public static bool TryGetCurrent([NotNullWhen(true)] out TestScopedServices? current)
    {
        if (_current.Value is TestScopedServices tss)
        {
            current = tss;
            return true;
        }

        current = default;
        return false;
    }

    public TestableClock Clock { get; }

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClient { get; }

    public CaptureEventObserver EventObserver { get; }

    public TestableFeatureProvider FeatureProvider { get; }

    public TrnRequestOptions TrnRequestOptions { get; }

    public Mock<IFileService> BlobStorageFileService { get; }

    public DeferredExecutionBackgroundJobScheduler BackgroundJobScheduler { get; }
}

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestScoped<T>(this IServiceCollection services, Func<TestScopedServices, T> resolveService)
        where T : class
    {
        return services.AddTransient(_ => resolveService(TestScopedServices.GetCurrent()));
    }
}
