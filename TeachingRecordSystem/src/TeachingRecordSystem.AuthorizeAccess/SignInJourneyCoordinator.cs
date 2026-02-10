using System.Diagnostics;
using System.Security.Claims;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.Security;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.OneLogin;

namespace TeachingRecordSystem.AuthorizeAccess;

[JourneyCoordinator(JourneyName, routeValueKeys: [])]
[UsedImplicitly]
public class SignInJourneyCoordinator(
    OneLoginService oneLoginService,
    TrsDbContext dbContext,
    AuthorizeAccessLinkGenerator linkGenerator,
    IOptions<AuthorizeAccessOptions> optionsAccessor,
    IClock clock) :
    JourneyCoordinator<SignInJourneyState>
{
    public const string JourneyName = "SignInJourney";

    public static class Vtrs
    {
        public const string AuthenticationOnly = "Cl.Cm";
        public const string AuthenticationAndIdentityVerification = "Cl.Cm.P2";
    }

    public LinkHelper Links => field ??= new LinkHelper(linkGenerator, InstanceId);

    public bool ShowDebugPages => optionsAccessor.Value.ShowDebugPages;

    public AdvanceToResult AdvanceTo(Func<LinkHelper, string> getUrl, PushStepOptions pushStepOptions = default) => AdvanceTo(getUrl(Links), pushStepOptions);

    public IResult SignInWithOneLogin() =>
        OneLoginChallenge(Vtrs.AuthenticationOnly);

    public IResult VerifyIdentityWithOneLogin() =>
        OneLoginChallenge(Vtrs.AuthenticationAndIdentityVerification);

    public async Task<IResult> OnOneLoginCallbackAsync(AuthenticationTicket ticket)
    {
        if (!ticket.Properties.TryGetVectorsOfTrust(out var vtr))
        {
            throw new InvalidOperationException("No vtr.");
        }

        if (vtr.SequenceEqual([Vtrs.AuthenticationOnly]))
        {
            await OnUserAuthenticatedAsync(ticket);
        }
        else
        {
            Debug.Assert(vtr.SequenceEqual([Vtrs.AuthenticationAndIdentityVerification]));
            Debug.Assert(State.OneLoginAuthenticationTicket is not null);
            await OnUserVerifiedAsync(ticket);
        }

        if (ShowDebugPages)
        {
            return SquashPathAndAdvanceTo(Links.DebugIdentity());
        }

        return GetNextPage();
    }

    public async Task OnUserAuthenticatedAsync(AuthenticationTicket ticket)
    {
        var sub = ticket.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");
        var email = ticket.Principal.FindFirstValue("email") ?? throw new InvalidOperationException("No email claim.");

        var processContext = await ProcessContext.FromDbAsync(dbContext, State.SigningInProcessId, clock.UtcNow);

        var oneLoginUser = await oneLoginService.OnSignInAsync(sub, email, processContext);
        var trn = oneLoginUser.Person?.Trn;

        var pendingSupportTaskReference = await oneLoginService.GetPendingSupportTaskReferenceByUserAsync(oneLoginUser.Subject);

        UpdateState(state =>
        {
            state.Reset();
            state.OneLoginAuthenticationTicket = ticket;
            state.PendingSupportTaskReference = pendingSupportTaskReference;

            if (oneLoginUser.VerificationRoute is not null)
            {
                Debug.Assert(oneLoginUser.VerifiedNames is not null);
                Debug.Assert(oneLoginUser.VerifiedDatesOfBirth is not null);
                state.SetVerified(oneLoginUser.VerifiedNames!, oneLoginUser.VerifiedDatesOfBirth!);
            }

            if (trn is not null && !ShowDebugPages)
            {
                Complete(state, trn);
            }
        });
    }

    public Task OnUserVerifiedAsync(AuthenticationTicket ticket)
    {
        var verifiedNames = ticket.Principal.GetCoreIdentityNames().Select(n => n.NameParts.Select(part => part.Value).ToArray()).ToArray();
        var verifiedDatesOfBirth = ticket.Principal.GetCoreIdentityBirthDates().Select(d => d.Value).ToArray();
        var coreIdentityClaimVc = ticket.Principal.FindFirstValue("vc") ?? throw new InvalidOperationException("No vc claim present.");

        return OnUserVerifiedCoreAsync(
            verifiedNames,
            verifiedDatesOfBirth,
            coreIdentityClaimVc,
            state => state.OneLoginAuthenticationTicket = ticket);
    }

    public async Task OnUserVerifiedCoreAsync(
        string[][] verifiedNames,
        DateOnly[] verifiedDatesOfBirth,
        string? coreIdentityClaimVc,
        Action<SignInJourneyState>? updateState = null)
    {
        var sub = State.OneLoginAuthenticationTicket!.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");
        var email = State.OneLoginAuthenticationTicket.Principal.FindFirstValue("email") ?? throw new InvalidOperationException("No email claim.");

        var processContext = await ProcessContext.FromDbAsync(dbContext, State.SigningInProcessId, clock.UtcNow);

        await oneLoginService.SetUserVerifiedAsync(
            new SetUserVerifiedOptions
            {
                OneLoginUserSubject = sub,
                VerificationRoute = OneLoginUserVerificationRoute.OneLogin,
                VerifiedDatesOfBirth = verifiedDatesOfBirth,
                VerifiedNames = verifiedNames,
                CoreIdentityClaimVc = coreIdentityClaimVc
            },
            processContext);

        string? trn = null;
        string? trnTokenTrn = null;

        if (await oneLoginService.FindTeacherIdentityUserAsync(verifiedNames, verifiedDatesOfBirth, State.TrnToken, email) is { } result)
        {
            if (result.MatchRoute is OneLoginUserMatchRoute.TrnToken)
            {
                trnTokenTrn = result.Trn;
            }

            if (result.MatchRoute is not null)
            {
                await oneLoginService.SetUserMatchedAsync(
                    new SetUserMatchedOptions
                    {
                        OneLoginUserSubject = sub,
                        MatchedPersonId = result.PersonId,
                        MatchRoute = result.MatchRoute.Value,
                        MatchedAttributes = result.MatchedAttributes!
                    },
                    processContext);

                trn = result.Trn;
            }
        }

        UpdateState(state =>
        {
            updateState?.Invoke(state);
            state.AttemptedIdentityVerification = true;

            state.SetVerified(verifiedNames, verifiedDatesOfBirth);

            state.TrnTokenTrn = trnTokenTrn;

            if (trn is not null)
            {
                Complete(state, trn);
            }
        });
    }

    public IResult OnVerificationFailed()
    {
        UpdateState(state =>
        {
            state.AttemptedIdentityVerification = true;
        });

        return GetNextPage();
    }

    public void OnSignOut() => UpdateState(s => s.Reset());

    public IResult GetNextPage() => State switch
    {
        // Authentication is complete
        { AuthenticationTicket: not null } =>
            SquashPathAndAdvanceTo(GetRedirectUri(), includeRedirectUri: false),

        // Authenticated with OneLogin, pending support tasks
        { OneLoginAuthenticationTicket: not null, PendingSupportTaskReference: not null } =>
            SquashPathAndAdvanceTo(Links.PendingSupportRequest(), includeRedirectUri: false),

        // Authenticated with OneLogin, identity verification succeeded, not yet matched to teaching record
        { OneLoginAuthenticationTicket: not null, IdentityVerified: true, AuthenticationTicket: null } =>
            SquashPathAndAdvanceTo(Links.Connect()),

        // Authenticated with OneLogin, not yet verified
        { OneLoginAuthenticationTicket: not null, IdentityVerified: false, AttemptedIdentityVerification: false } =>
            VerifyIdentityWithOneLogin(),

        // Authenticated with OneLogin, identity verification failed
        { OneLoginAuthenticationTicket: not null, IdentityVerified: false } =>
            SquashPathAndAdvanceTo(Links.NotVerified()),

        _ => throw new InvalidOperationException("Cannot determine next page.")
    };

    public override string? GetBackLink()
    {
        var backLink = base.GetBackLink();
        return backLink != GetRedirectUri() ? backLink : null;
    }

    public string GetRedirectUri() => State.RedirectUri;

    public override JourneyPathStep? GetCurrentStep()
    {
        // Ensure the sign out URL can always be hit, even though it'll never be in the journey's Path
        if (HttpContext.Request.GetEncodedPathAndQuery() == Links.SignOut())
        {
            return Path.Steps.Last();
        }

        return base.GetCurrentStep();
    }

    public override bool StepIsValid(JourneyPathStep step)
    {
        return base.StepIsValid(step) || step.NormalizedUrl.StartsWith("/oauth2/authorize");
    }

    public async Task<IActionResult?> TryMatchToTeachingRecordAsync()
    {
        if (State.OneLoginAuthenticationTicket is null)
        {
            throw new InvalidOperationException("Cannot match to a teaching record without a verified One Login user.");
        }

        if (!State.IdentityVerified)
        {
            return null;
        }

        var email = State.OneLoginAuthenticationTicket.Principal.FindFirstValue("email") ?? throw new InvalidOperationException("No email claim.");

        var names = State.VerifiedNames;
        var datesOfBirth = State.VerifiedDatesOfBirth;
        var nationalInsuranceNumber = State.NationalInsuranceNumber;
        var trn = State.Trn;
        var trnTokenTrn = State.TrnTokenTrn;

        var matchResult = await oneLoginService.MatchPersonAsync(
            new(names!, datesOfBirth!, email, nationalInsuranceNumber, trn, trnTokenTrn));

        if (matchResult is var (matchedPersonId, matchedTrn, matchedAttributes))
        {
            var subject = State.OneLoginAuthenticationTicket.Principal.FindFirstValue("sub") ?? throw new InvalidOperationException("No sub claim.");

            var processContext = await ProcessContext.FromDbAsync(dbContext, State.SigningInProcessId, clock.UtcNow);

            await oneLoginService.SetUserMatchedAsync(
                new SetUserMatchedOptions
                {
                    OneLoginUserSubject = subject,
                    MatchedPersonId = matchedPersonId,
                    MatchRoute = OneLoginUserMatchRoute.Interactive,
                    MatchedAttributes = matchedAttributes
                },
                processContext);

            UpdateState(state => Complete(state, matchedTrn));

            var nextUrl = SquashPathAndAdvanceTo(Links.Found()).Url;
            return new RedirectResult(nextUrl);
        }

        return null;
    }

    private RedirectHttpResult SquashPathAndAdvanceTo(string url, bool includeRedirectUri = true)
    {
        var urls = new List<string>(2) { url };

        // Ensure the RedirectUri is included in the path
        if (includeRedirectUri)
        {
            urls.Insert(0, GetRedirectUri());
        }

        var steps = urls.Select(CreateStepFromUrl);
        var path = new JourneyPath(steps);
        UnsafeSetPath(path);

        return (RedirectHttpResult)Results.Redirect(urls.Last());
    }

    public void Complete(SignInJourneyState state, string trn)
    {
        if (state.OneLoginAuthenticationTicket is null)
        {
            throw new InvalidOperationException("User is not authenticated with One Login.");
        }

        var oneLoginPrincipal = state.OneLoginAuthenticationTicket.Principal;
        var oneLoginIdToken = state.OneLoginAuthenticationTicket.Properties.GetTokenValue(OpenIdConnectParameterNames.IdToken)!;

        var teachingRecordIdentity = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Subject, oneLoginPrincipal.FindFirstValue("sub")!),
                new Claim(ClaimTypes.Trn, trn),
                new Claim(ClaimTypes.Email, oneLoginPrincipal.FindFirstValue("email")!),
                new Claim(ClaimTypes.OneLoginIdToken, oneLoginIdToken),
                new Claim(ClaimTypes.TrsUserId, state.ClientApplicationUserId.ToString())
            ],
            authenticationType: "Authorize access to a teaching record",
            nameType: ClaimTypes.Subject,
            roleType: null);

        var principal = new ClaimsPrincipal(teachingRecordIdentity);

        state.AuthenticationTicket = new AuthenticationTicket(principal, properties: null, AuthenticationSchemes.MatchToTeachingRecord);
    }

    private IResult OneLoginChallenge(string vtr)
    {
        var delegatedProperties = new AuthenticationProperties();
        delegatedProperties.SetVectorsOfTrust([vtr]);
        delegatedProperties.Items.Add(JourneySignInHandler.PropertyKeys.JourneyInstanceId, InstanceId.ToString());
        return Results.Challenge(delegatedProperties, authenticationSchemes: [State.OneLoginAuthenticationScheme]);
    }

    public sealed class LinkHelper(AuthorizeAccessLinkGenerator linkGenerator, JourneyInstanceId instanceId)
    {
        public string DebugIdentity() => linkGenerator.DebugIdentity(instanceId);

        public string Connect() => linkGenerator.Connect(instanceId);

        public string NationalInsuranceNumber(string? returnUrl = null) =>
            linkGenerator.NationalInsuranceNumber(instanceId, returnUrl);

        public string Trn(string? returnUrl = null) => linkGenerator.Trn(instanceId, returnUrl);

        public string CheckAnswers() => linkGenerator.CheckAnswers(instanceId);

        public string SupportRequestSubmitted() => linkGenerator.SupportRequestSubmitted(instanceId);

        public string Found() => linkGenerator.Found(instanceId);

        public string NotFound() => linkGenerator.NotFound(instanceId);

        public string NotVerified() => linkGenerator.NotVerified(instanceId);

        public string Name(string? returnUrl = null) => linkGenerator.Name(instanceId, returnUrl);

        public string DateOfBirth(string? returnUrl = null) => linkGenerator.DateOfBirth(instanceId, returnUrl);

        public string NoTrn() => linkGenerator.NoTrn(instanceId);

        public string PendingSupportRequest() => linkGenerator.PendingSupportRequest(instanceId);

        public string ProofOfIdentity(string? returnUrl = null) => linkGenerator.ProofOfIdentity(instanceId, returnUrl);

        public string SignOut() => linkGenerator.SignOut(instanceId);
    }
}
