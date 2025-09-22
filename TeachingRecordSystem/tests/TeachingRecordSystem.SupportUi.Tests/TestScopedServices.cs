using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;
using TeachingRecordSystem.TestCommon.Infrastructure;

namespace TeachingRecordSystem.SupportUi.Tests;

public class TestScopedServices
{
    public const string FakeBlobStorageFileUrlBase = "https://fake.blob.core.windows.net/";

    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices(IServiceProvider serviceProvider)
    {
        Clock = new();
        AzureActiveDirectoryUserServiceMock = new();
        EventObserver = new();
        FeatureProvider = ActivatorUtilities.CreateInstance<TestableFeatureProvider>(serviceProvider);
        BlobStorageFileServiceMock = new();
        BlobStorageFileServiceMock
            .Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string?>(), null))
            .ReturnsAsync(Guid.NewGuid());
        BlobStorageFileServiceMock
            .Setup(s => s.GetFileUrlAsync(It.IsAny<Guid>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync((Guid id, TimeSpan time) => $"{FakeBlobStorageFileUrlBase}{id}");
        GetAnIdentityApiClientMock = new();
        TrnRequestOptions = new TrnRequestOptions();
    }

    public static void ConfigureServices(IServiceCollection services) =>
        services
            .AddTestScoped<IClock>(tss => tss.Clock)
            .AddTestScoped(tss => tss.GetAnIdentityApiClientMock.Object)
            .AddTestScoped(tss => tss.AzureActiveDirectoryUserServiceMock.Object)
            .AddTestScoped<IFeatureProvider>(tss => tss.FeatureProvider)
            .AddTestScoped(tss => tss.BlobStorageFileServiceMock.Object)
            .AddTestScoped(tss => Options.Create(tss.TrnRequestOptions));

    public static TestScopedServices GetCurrent() =>
        _current.Value ?? throw new InvalidOperationException("No current instance has been set.");

    public static TestScopedServices Reset(IServiceProvider serviceProvider)
    {
        if (_current.Value is not null)
        {
            throw new InvalidOperationException("Current instance has already been set.");
        }

        return _current.Value = new(serviceProvider);
    }

    public TestableClock Clock { get; }

    public Mock<IAadUserService> AzureActiveDirectoryUserServiceMock { get; }

    public CaptureEventObserver EventObserver { get; }

    public TestableFeatureProvider FeatureProvider { get; }

    public Mock<IFileService> BlobStorageFileServiceMock { get; }

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClientMock { get; }

    public TrnRequestOptions TrnRequestOptions { get; }
}

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTestScoped<T>(this IServiceCollection services, Func<TestScopedServices, T> resolveService)
        where T : class
    {
        return services.AddTransient(_ => resolveService(TestScopedServices.GetCurrent()));
    }
}
