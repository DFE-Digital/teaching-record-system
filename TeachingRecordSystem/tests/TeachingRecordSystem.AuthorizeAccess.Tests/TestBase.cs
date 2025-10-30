using System.Security.Claims;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.WebCommon.FormFlow;
using TeachingRecordSystem.WebCommon.FormFlow.State;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public abstract class TestBase
{
    private readonly TestScopedServices _testServices;

    protected TestBase(HostFixture hostFixture)
    {
        HostFixture = hostFixture;

        _testServices = TestScopedServices.Reset();

        HttpClient = hostFixture.CreateClient(new()
        {
            AllowAutoRedirect = false
        });
    }

    protected HostFixture HostFixture { get; }

    protected IDbContextFactory<TrsDbContext> DbContextFactory => HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected CaptureEventObserver EventPublisher => _testServices.EventObserver;

    protected TestableClock Clock => _testServices.Clock;

    protected HttpClient HttpClient { get; }

    protected TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    protected async Task<JourneyInstance<SignInJourneyState>> CreateJourneyInstanceAsync(SignInJourneyState state)
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var stateProvider = scope.ServiceProvider.GetRequiredService<IUserInstanceStateProvider>();

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

    protected async Task<JourneyInstance<SignInJourneyState>> ReloadJourneyInstanceAsync(JourneyInstance<SignInJourneyState> journeyInstance)
    {
        await using var scope = HostFixture.Services.CreateAsyncScope();
        var stateProvider = scope.ServiceProvider.GetRequiredService<IUserInstanceStateProvider>();
        var reloadedInstance = await stateProvider.GetInstanceAsync(journeyInstance.InstanceId, typeof(SignInJourneyState));
        return (JourneyInstance<SignInJourneyState>)reloadedInstance!;
    }

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected async Task WithSignInJourneyHelper(Func<SignInJourneyHelper, Task> action)
    {
        using var scope = HostFixture.Services.CreateScope();
        var signInJourneyHelper = scope.ServiceProvider.GetRequiredService<SignInJourneyHelper>();
        await action(signInJourneyHelper);
    }

    protected AuthenticationTicket CreateOneLoginAuthenticationTicket(
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
        properties.SetVectorsOfTrust([vtr]);
        properties.StoreTokens([new AuthenticationToken() { Name = OpenIdConnectParameterNames.IdToken, Value = "dummy" }]);

        return new AuthenticationTicket(principal, properties, authenticationScheme: "OneLogin");
    }

    protected AuthenticationTicket CreateOneLoginAuthenticationTicket(string vtr, OneLoginUser user) =>
        CreateOneLoginAuthenticationTicket(
            vtr,
            user.Subject,
            user.EmailAddress,
            user.VerifiedNames?.First().First(),
            user.VerifiedNames?.First().Last(),
            user.VerifiedDatesOfBirth?.First());

    protected SignInJourneyState CreateNewState(IdTrnToken trnToken, string redirectUri = "/", Guid clientApplicationUserId = default) =>
        CreateNewState(redirectUri, clientApplicationUserId, trnToken?.TrnToken, trnToken?.Trn);

    protected SignInJourneyState CreateNewState(string redirectUri = "/", Guid clientApplicationUserId = default, string? trnToken = null, string? trnTokenTrn = null) =>
        new(
            redirectUri,
            serviceName: "Test Service",
            serviceUrl: "https://service",
            oneLoginAuthenticationScheme: "dummy",
            clientApplicationUserId,
            trnToken)
        {
            TrnTokenTrn = trnTokenTrn
        };

    private static int _lastTrnToken;

    protected async Task<IdTrnToken> CreateTrnTokenAsync(string trn, string? email = null)
    {
        var trnTokenStr = Interlocked.Increment(ref _lastTrnToken).ToString("D12");

        email ??= Faker.Internet.Email();

        using var scope = HostFixture.Services.CreateScope();
        using var idDbContext = scope.ServiceProvider.GetRequiredService<IdDbContext>();

        var trnToken = new IdTrnToken()
        {
            CreatedUtc = Clock.UtcNow,
            Email = email,
            ExpiresUtc = Clock.UtcNow.AddDays(30),
            Trn = trn,
            TrnToken = trnTokenStr,
            UserId = null
        };

        idDbContext.TrnTokens.Add(trnToken);
        await idDbContext.SaveChangesAsync();

        return trnToken;
    }
}
