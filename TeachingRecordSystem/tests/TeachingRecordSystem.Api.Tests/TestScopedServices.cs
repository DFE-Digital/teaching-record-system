using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Certificates;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Api.Tests;

public class TestScopedServices
{
    private static readonly AsyncLocal<TestScopedServices> _current = new();

    public TestScopedServices()
    {
        CertificateGeneratorMock = new();
        Clock = new();
        DataverseAdapterMock = new();
        GetAnIdentityApiClientMock = new();

        AccessYourTeachingQualificationsOptions = Options.Create(new AccessYourTeachingQualificationsOptions()
        {
            BaseAddress = "https://aytq.com",
        });
    }

    public static TestScopedServices GetCurrent() =>
        _current.Value ?? throw new InvalidOperationException("No current instance has been set.");

    public static TestScopedServices Reset()
    {
        if (_current.Value is not null)
        {
            throw new InvalidOperationException("Current instance has already been set.");
        }

        return _current.Value = new();
    }

    public Mock<ICertificateGenerator> CertificateGeneratorMock { get; }

    public TestableClock Clock { get; }

    public Mock<IDataverseAdapter> DataverseAdapterMock { get; }

    public Mock<IGetAnIdentityApiClient> GetAnIdentityApiClientMock { get; }

    public IOptions<AccessYourTeachingQualificationsOptions> AccessYourTeachingQualificationsOptions { get; }

}
