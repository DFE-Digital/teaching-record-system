using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Api.IntegrationTests;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices()
    {
        Clock = new();
        DataverseAdapterMock = new();
        GetAnIdentityApiClientMock = new();
        CrmQueryDispatcherSpy = new();
        BlobStorageFileServiceMock = new();

        AccessYourTeachingQualificationsOptions = Options.Create(new AccessYourTeachingQualificationsOptions()
        {
            BaseAddress = "https://aytq.com",
        });
    }

    public static TestScopedServices GetCurrent() =>
        TryGetCurrent(out var current) ? current : throw new InvalidOperationException("No current instance has been set.");

    public static TestScopedServices Reset()
    {
        if (_current.Value is not null)
        {
            throw new InvalidOperationException("Current instance has already been set.");
        }

        return _current.Value = new();
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

    public Mock<IDataverseAdapter> DataverseAdapterMock { get; }

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClientMock { get; }

    public IOptions<AccessYourTeachingQualificationsOptions> AccessYourTeachingQualificationsOptions { get; }

    public CrmQueryDispatcherSpy CrmQueryDispatcherSpy { get; }

    public Mock<IFileService> BlobStorageFileServiceMock { get; }
}
