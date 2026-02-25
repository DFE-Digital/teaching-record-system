using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Time.Testing;
using TeachingRecordSystem.AuthorizeAccess.Pages.RequestTrn;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.WebCommon.FormFlow;
using TeachingRecordSystem.WebCommon.FormFlow.State;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public abstract class TestBase : IDisposable
{
    private readonly TestScopedServices _testServices;

    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        _testServices = TestScopedServices.Reset(hostFixture.Services);

        HttpClient = hostFixture.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
    }

    public HostFixture HostFixture { get; }

    public EventCapture Events => _testServices.Events;

    public CaptureEventObserver LegacyEventObserver => _testServices.LegacyEventObserver;

    public FakeTimeProvider Clock => _testServices.Clock;

    public HttpClient HttpClient { get; }

    public TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    public async Task<JourneyInstance<RequestTrnJourneyState>> CreateJourneyInstance(RequestTrnJourneyState state)
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var stateProvider = scope.ServiceProvider.GetRequiredService<IUserInstanceStateProvider>();

        var journeyDescriptor = RequestTrnJourneyState.JourneyDescriptor;

        var keysDict = new Dictionary<string, StringValues>
        {
            { Constants.UniqueKeyQueryParameterName, new StringValues(Guid.NewGuid().ToString()) }
        };

        var instanceId = new JourneyInstanceId(journeyDescriptor.JourneyName, keysDict);

        var stateType = typeof(RequestTrnJourneyState);

        var instance = await stateProvider.CreateInstanceAsync(instanceId, stateType, state);
        return (JourneyInstance<RequestTrnJourneyState>)instance;
    }

    public async Task<JourneyInstance<RequestTrnJourneyState>> ReloadJourneyInstance(JourneyInstance<RequestTrnJourneyState> journeyInstance)
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var stateProvider = scope.ServiceProvider.GetRequiredService<IUserInstanceStateProvider>();
        var reloadedInstance = await stateProvider.GetInstanceAsync(journeyInstance.InstanceId, typeof(RequestTrnJourneyState));
        return (JourneyInstance<RequestTrnJourneyState>)reloadedInstance!;
    }

    public RequestTrnJourneyState CreateNewState() => new RequestTrnJourneyState()
    {
        IsTakingNpq = true,
        HaveRegisteredForAnNpq = true,
        NpqApplicationId = "SOMEID",
        WorkingInSchoolOrEducationalSetting = true,
        WorkEmail = Faker.Internet.Email(),
        PersonalEmail = Faker.Internet.Email(),
        FirstName = TestData.GenerateFirstName(),
        MiddleName = TestData.GenerateMiddleName(),
        LastName = TestData.GenerateLastName(),
        HasPreviousName = false,
        PreviousFirstName = null,
        PreviousMiddleName = null,
        PreviousLastName = null,
        DateOfBirth = new DateOnly(1999, 01, 01),
        EvidenceFileId = Guid.NewGuid(),
        EvidenceFileName = "evidence-file-name.jpg",
        EvidenceFileSizeDescription = "1.2 MB",
        HasNationalInsuranceNumber = true,
        NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber()
    };

    public virtual void Dispose()
    {
    }

    public virtual async Task<T> WithDbContext<T>(Func<TrsDbContext, Task<T>> action)
    {
        var dbContextFactory = HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        return await action(dbContext);
    }

    public virtual Task WithDbContext(Func<TrsDbContext, Task> action) =>
        WithDbContext(async dbContext =>
        {
            await action(dbContext);
            return 0;
        });
}
