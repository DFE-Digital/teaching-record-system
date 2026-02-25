using System.Security.Claims;
using GovUk.OneLogin.AspNetCore;
using GovUk.Questions.AspNetCore;
using GovUk.Questions.AspNetCore.Testing;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.Core.Services.OneLogin;
using JourneyInstanceId = GovUk.Questions.AspNetCore.JourneyInstanceId;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public abstract class TestBase
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

    protected HostFixture HostFixture { get; }

    protected IDbContextFactory<TrsDbContext> DbContextFactory => HostFixture.Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    protected EventCapture Events => _testServices.Events;

    protected CaptureEventObserver LegacyEventPublisher => _testServices.LegacyEventObserver;

    protected FakeTimeProvider Clock => _testServices.Clock;

    protected HttpClient HttpClient { get; }

    protected TestData TestData => HostFixture.Services.GetRequiredService<TestData>();

    protected Mock<ISafeFileService> SafeFileService => _testServices.SafeFileService;

    protected void AddUrlToPath(SignInJourneyCoordinator coordinator, string url)
    {
        var newStep = coordinator.CreateStepFromUrl(url);
        var newPath = new JourneyPath(coordinator.Path.Steps.Append(newStep));
        coordinator.UnsafeSetPath(newPath);
    }

    protected async Task WithJourneyCoordinatorAsync(
        Func<JourneyInstanceId, Guid, SignInJourneyState> getState,
        Func<SignInJourneyCoordinator, Task> action)
    {
        var journeyHelper = HostFixture.Services.GetRequiredService<JourneyHelper>();

        using var scope = HostFixture.Services.CreateScope();

        SignInJourneyState state;
        List<string> pathUrls = new();

        var signInJourneyCoordinator = await journeyHelper.CreateInstanceAsync(
            new RouteValueDictionary(),
            async instanceId =>
            {
                var process = await TestData.CreateProcessAsync(
                    ProcessType.TeacherSigningIn,
                    events: [
                        new AuthorizeAccessRequestStartedEvent
                        {
                            EventId = Guid.NewGuid(),
                            JourneyInstanceId = instanceId.ToString(),
                            ApplicationUserId = Guid.Empty,
                            ClientId = string.Empty
                        }
                    ]);

                state = getState(instanceId, process.ProcessId);
                pathUrls.Add(state.RedirectUri);
                return state;
            },
            pathUrls,
            () => ActivatorUtilities.CreateInstance<SignInJourneyCoordinator>(scope.ServiceProvider));

        await action(signInJourneyCoordinator);
    }

    protected Task<T> WithDbContextAsync<T>(Func<TrsDbContext, Task<T>> action) =>
        DbContextFactory.WithDbContextAsync(action);

    protected Task WithDbContextAsync(Func<TrsDbContext, Task> action) =>
        DbContextFactory.WithDbContextAsync(action);

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

        createCoreIdentityVc ??= vtr == SignInJourneyCoordinator.Vtrs.AuthenticationAndIdentityVerification;

        if (createCoreIdentityVc == true)
        {
            if (vtr == SignInJourneyCoordinator.Vtrs.AuthenticationOnly)
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

    protected SignInJourneyState CreateSignInJourneyState(
        JourneyInstanceId journeyInstanceId,
        Guid processId,
        IdTrnToken? trnToken,
        string redirectUri = "/",
        Guid clientApplicationUserId = default) =>
        CreateSignInJourneyState(journeyInstanceId, processId, redirectUri, clientApplicationUserId, trnToken?.TrnToken, trnToken?.Trn);

    protected SignInJourneyState CreateSignInJourneyState(JourneyInstanceId journeyInstanceId, Guid processId) =>
        CreateSignInJourneyState(journeyInstanceId, processId, "/");

    protected SignInJourneyState CreateSignInJourneyState(
        JourneyInstanceId journeyInstanceId,
        Guid processId,
        string redirectUriBase,
        Guid clientApplicationUserId = default,
        string? trnToken = null,
        string? trnTokenTrn = null)
    {
        var redirectUri = journeyInstanceId.EnsureUrlHasKey(redirectUriBase);

        return new(
            processId,
            redirectUri,
            serviceName: "Test Service",
            serviceUrl: "https://service",
            oneLoginAuthenticationScheme: "dummy",
            clientApplicationUserId,
            trnToken)
        {
            TrnTokenTrn = trnTokenTrn
        };
    }

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

    protected static class StepUrls
    {
        public const string NotVerified = "/not-verified";
        public const string Connect = "/connect";
        public const string NationalInsuranceNumber = "/national-insurance-number";
        public const string Trn = "/trn";
        public const string Found = "/found";
        public const string NotFound = "/not-found";
        public const string CheckAnswers = "/check-answers";
        public const string RequestSubmitted = "/request-submitted";
        public const string Name = "/name";
        public const string DateOfBirth = "/date-of-birth";
        public const string NoTrn = "/no-trn";
        public const string PendingSupportRequest = "/pending-support-request";
        public const string ProofOfIdentity = "/proof-of-identity";
    }

    protected static class JourneyUrls
    {
        public static string NotVerified(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.NotVerified);

        public static string Connect(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.Connect);

        public static string NationalInsuranceNumber(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.NationalInsuranceNumber);

        public static string Trn(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.Trn);

        public static string Found(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.Found);

        public static string NotFound(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.NotFound);

        public static string CheckAnswers(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.CheckAnswers);

        public static string RequestSubmitted(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.RequestSubmitted);

        public static string Name(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.Name);

        public static string DateOfBirth(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.DateOfBirth);

        public static string NoTrn(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.NoTrn);

        public static string PendingSupportRequest(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.PendingSupportRequest);

        public static string ProofOfIdentity(JourneyInstanceId instanceId) =>
            instanceId.EnsureUrlHasKey(StepUrls.ProofOfIdentity);
    }
}
