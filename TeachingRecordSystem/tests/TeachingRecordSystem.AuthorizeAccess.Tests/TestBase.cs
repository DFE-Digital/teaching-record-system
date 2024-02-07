using System.Reactive.Linq;
using System.Security.Claims;
using System.Text.Json;
using FakeXrmEasy.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.FormFlow;
using TeachingRecordSystem.FormFlow.State;
using TeachingRecordSystem.TestCommon;

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

    public AuthenticationTicket CreateOneLoginAuthenticationTicket(OneLoginUser user)
    {
        bool createCoreIdentityVc = false;
        string? firstName = null;
        string? lastName = null;
        DateOnly? dateOfBirth = null;

        if (user.CoreIdentityVc is JsonDocument vc)
        {
            var credentialSubject = vc.RootElement.GetProperty("credentialSubject");
            var nameParts = credentialSubject.GetProperty("name").EnumerateArray().Single().GetProperty("nameParts").EnumerateArray();
            firstName = nameParts.First().GetProperty("value").GetString();
            lastName = nameParts.Last().GetProperty("value").GetString();
            dateOfBirth = DateOnly.FromDateTime(credentialSubject.GetProperty("birthDate").EnumerateArray().Single().GetProperty("value").GetDateTime());
            createCoreIdentityVc = true;
        }

        return CreateOneLoginAuthenticationTicket(
            user.Subject,
            user.Email,
            firstName,
            lastName,
            dateOfBirth,
            createCoreIdentityVc);
    }

    public AuthenticationTicket CreateOneLoginAuthenticationTicket(
        string? sub = null,
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        DateOnly? dateOfBirth = null,
        bool createCoreIdentityVc = true)
    {
        sub ??= Faker.Internet.UserName();
        email ??= Faker.Internet.Email();

        var claims = new List<Claim>()
        {
            new("sub", sub),
            new("email", email)
        };

        if (createCoreIdentityVc)
        {
            firstName ??= Faker.Name.First();
            lastName ??= Faker.Name.Last();
            dateOfBirth ??= DateOnly.FromDateTime(Faker.Identification.DateOfBirth());

            var vc = TestData.CreateOneLoginCoreIdentityVc(firstName, lastName, dateOfBirth.Value);
            claims.Add(new Claim("vc", vc.RootElement.ToString(), "JSON"));
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "OneLogin", nameType: "sub", roleType: null);

        var principal = new ClaimsPrincipal(identity);

        return new AuthenticationTicket(principal, authenticationScheme: "OneLogin");
    }
}
