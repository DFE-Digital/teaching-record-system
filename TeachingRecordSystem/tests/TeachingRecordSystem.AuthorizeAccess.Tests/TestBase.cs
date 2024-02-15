using System.Reactive.Linq;
using System.Security.Claims;
using FakeXrmEasy.Abstractions;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.FormFlow;
using TeachingRecordSystem.FormFlow.State;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public abstract class TestBase : IDisposable
{
    private readonly TestScopedServices _testServices;
    private readonly IDisposable _trsSyncSubscription;

    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        _testServices = TestScopedServices.Reset();

        HttpClient = hostFixture.CreateClient(new()
        {
            AllowAutoRedirect = false
        });

        _trsSyncSubscription = hostFixture.Services.GetRequiredService<TrsDataSyncHelper>().GetSyncedEntitiesObservable()
            .Subscribe(onNext: static (synced) =>
            {
                var events = synced.OfType<EventBase>();
                foreach (var e in events)
                {
                    TestScopedServices.GetCurrent().EventObserver.OnEventSaved(e);
                }
            });
    }

    public HostFixture HostFixture { get; }

    public TestableClock Clock => _testServices.Clock;

    public HttpClient HttpClient { get; }

    public TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    public IXrmFakedContext XrmFakedContext => HostFixture.Services.GetRequiredService<IXrmFakedContext>();

    public async Task<JourneyInstance<SignInJourneyState>> CreateJourneyInstance(SignInJourneyState state)
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var stateProvider = scope.ServiceProvider.GetRequiredService<IUserInstanceStateProvider>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<FormFlowOptions>>();

        var journeyDescriptor = SignInJourneyState.JourneyDescriptor;

        var keysDict = new Dictionary<string, StringValues>
        {
            { Constants.UniqueKeyQueryParameterName, new StringValues(Guid.NewGuid().ToString()) }
        };

        var instanceId = new JourneyInstanceId(journeyDescriptor.JourneyName, keysDict);

        var stateType = typeof(SignInJourneyState);

        var instance = await stateProvider.CreateInstanceAsync(instanceId, stateType, state, properties: null);
        return (JourneyInstance<SignInJourneyState>)instance;
    }

    public async Task<JourneyInstance<SignInJourneyState>> ReloadJourneyInstance(JourneyInstance<SignInJourneyState> journeyInstance)
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var stateProvider = scope.ServiceProvider.GetRequiredService<IUserInstanceStateProvider>();
        var reloadedInstance = await stateProvider.GetInstanceAsync(journeyInstance.InstanceId, typeof(SignInJourneyState));
        return (JourneyInstance<SignInJourneyState>)reloadedInstance!;
    }

    public virtual void Dispose()
    {
        _trsSyncSubscription.Dispose();
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

    public SignInJourneyHelper GetSignInJourneyHelper() => HostFixture.Services.GetRequiredService<SignInJourneyHelper>();

    public AuthenticationTicket CreateOneLoginAuthenticationTicket(
        string vtr,
        string? sub = null,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        DateOnly? dateOfBirth = null,
        bool? createCoreIdentityVc = null)
    {
        sub ??= TestData.CreateOneLoginUserSubject();
        email ??= Faker.Internet.Email();

        var claims = new List<Claim>()
        {
            new("sub", sub),
            new("email", email)
        };

        createCoreIdentityVc ??= vtr == SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr;

        if (createCoreIdentityVc == true)
        {
            if (vtr == SignInJourneyHelper.AuthenticationOnlyVtr)
            {
                throw new ArgumentException("Cannot assign core identity VC with authentication-only vtr.", nameof(vtr));
            }

            firstName ??= Faker.Name.First();
            lastName ??= Faker.Name.Last();
            dateOfBirth ??= DateOnly.FromDateTime(Faker.Identification.DateOfBirth());

            var vc = TestData.CreateOneLoginCoreIdentityVc(firstName, lastName, dateOfBirth.Value);
            claims.Add(new Claim("vc", vc.RootElement.ToString(), "JSON"));
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "OneLogin", nameType: "sub", roleType: null);

        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties();
        properties.SetVectorOfTrust(vtr);

        return new AuthenticationTicket(principal, properties, authenticationScheme: "OneLogin");
    }
}
