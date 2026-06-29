using System.Diagnostics.CodeAnalysis;
using System.Net;
using JustEat.HttpClientInterception;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.Api.IntegrationTests;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices(IServiceProvider serviceProvider)
    {
        TimeProvider = new FakeTimeProvider(new DateTimeOffset(2021, 1, 4, 0, 0, 0, TimeSpan.Zero));
        BlobStorageFileServiceMock = new();
        FeatureProvider = ActivatorUtilities.CreateInstance<TestableFeatureProvider>(serviceProvider);
        BackgroundJobScheduler = new(serviceProvider);
        EvidenceFilesHttpClientInterceptorOptions = new() { OnMissingRegistration = _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)) };

        AccessYourTeachingQualificationsOptions = Options.Create(new AccessYourTeachingQualificationsOptions()
        {
            BaseAddress = "https://aytq.com"
        });
    }

    public static void ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton<TimeProvider>(new ForwardToTestScopedTimeProvider())
            .AddTestScoped(tss => tss.AccessYourTeachingQualificationsOptions)
            .AddTestScoped(tss => tss.BlobStorageFileServiceMock.Object)
            .AddTestScoped<IFeatureProvider>(tss => tss.FeatureProvider)
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

    public FakeTimeProvider TimeProvider { get; }

    public IOptions<AccessYourTeachingQualificationsOptions> AccessYourTeachingQualificationsOptions { get; }

    public Mock<IFileService> BlobStorageFileServiceMock { get; }

    public TestableFeatureProvider FeatureProvider { get; }

    public DeferredExecutionBackgroundJobScheduler BackgroundJobScheduler { get; }

    public HttpClientInterceptorOptions EvidenceFilesHttpClientInterceptorOptions { get; }

    private class ForwardToTestScopedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => GetCurrent().TimeProvider.GetUtcNow();
    }
}

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestScoped<T>(this IServiceCollection services, Func<TestScopedServices, T> resolveService)
        where T : class
    {
        return services.AddTransient(_ => resolveService(TestScopedServices.GetCurrent()));
    }
}
