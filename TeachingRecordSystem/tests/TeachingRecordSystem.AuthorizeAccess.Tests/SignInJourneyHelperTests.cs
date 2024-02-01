using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq.Protected;
using TeachingRecordSystem.AuthorizeAccess.Tests.Infrastructure.FormFlow;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public class SignInJourneyHelperTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task OnSignedInWithOneLogin_WithCoreIdentityVc_SetsOneLoginAuthenticationTicketAndVerifiedPropertiesOnState() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var linkGenerator = GetMockLinkGenerator();
            var options = Options.Create(new AuthorizeAccessOptions() { ShowDebugPages = false });
            var userInstanceStateProvider = new InMemoryInstanceStateProvider();
            var clock = new TestableClock();
            var helper = new SignInJourneyHelper(dbContext, linkGenerator, options, userInstanceStateProvider, clock);

            var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
            var journeyInstance = await CreateJourneyInstance(state);

            var firstName = Faker.Name.First();
            var lastName = Faker.Name.Last();
            var dateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
            var ticket = CreateOneLoginAuthenticationTicket(firstName: firstName, lastName: lastName, dateOfBirth: dateOfBirth);

            // Act
            await helper.OnSignedInWithOneLogin(journeyInstance, ticket);

            // Assert
            Assert.NotNull(state.OneLoginAuthenticationTicket);
            Assert.True(state.IdentityVerified);
            Assert.NotNull(state.VerifiedNames);
            Assert.Collection(state.VerifiedNames, nameParts => Assert.Collection(nameParts, name => Assert.Equal(firstName, name), name => Assert.Equal(lastName, name)));
            Assert.NotNull(state.VerifiedDatesOfBirth);
            Assert.Collection(state.VerifiedDatesOfBirth, dob => Assert.Equal(dateOfBirth, dob));
        });

    [Fact]
    public Task OnSignedInWithOneLogin_WithoutCoreIdentityVc_SetsOneLoginAuthenticationTicketOnState() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var linkGenerator = GetMockLinkGenerator();
            var options = Options.Create(new AuthorizeAccessOptions() { ShowDebugPages = false });
            var userInstanceStateProvider = new InMemoryInstanceStateProvider();
            var clock = new TestableClock();
            var helper = new SignInJourneyHelper(dbContext, linkGenerator, options, userInstanceStateProvider, clock);

            var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
            var journeyInstance = await CreateJourneyInstance(state);

            var ticket = CreateOneLoginAuthenticationTicket(createCoreIdentityVc: false);

            // Act
            await helper.OnSignedInWithOneLogin(journeyInstance, ticket);

            // Assert
            Assert.NotNull(state.OneLoginAuthenticationTicket);
            Assert.False(state.IdentityVerified);
            Assert.Null(state.VerifiedNames);
            Assert.Null(state.VerifiedDatesOfBirth);
        });

    [Fact]
    public Task OnSignedInWithOneLogin_OneLoginUserAlreadyExistsWithKnownPerson_UpdatesLastSignInAndSetsAuthenticationTicketOnState() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var linkGenerator = GetMockLinkGenerator();
            var options = Options.Create(new AuthorizeAccessOptions() { ShowDebugPages = false });
            var userInstanceStateProvider = new InMemoryInstanceStateProvider();
            var clock = new TestableClock();
            var helper = new SignInJourneyHelper(dbContext, linkGenerator, options, userInstanceStateProvider, clock);

            var person = await TestData.CreatePerson(b => b.WithTrn(true));
            var user = await TestData.CreateOneLoginUser(personId: person.PersonId);
            clock.Advance();

            var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
            var journeyInstance = await CreateJourneyInstance(state);

            var ticket = CreateOneLoginAuthenticationTicket(user);

            // Act
            await helper.OnSignedInWithOneLogin(journeyInstance, ticket);

            // Assert
            user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject);
            Assert.Equal(clock.UtcNow, user.LastOneLoginSignIn);
            Assert.Equal(clock.UtcNow, user.LastSignIn);
            Assert.NotNull(state.AuthenticationTicket);
            Assert.Equal(person.Trn, state.AuthenticationTicket.Principal.FindFirstValue(ClaimTypes.Trn));
            Assert.Equal(person.PersonId.ToString(), state.AuthenticationTicket.Principal.FindFirstValue(ClaimTypes.PersonId));
        });

    [Fact]
    public Task OnSignedInWithOneLogin_OneLoginUserAlreadyExistsButNotKnownPerson_UpdatesOneLoginLastSignInButDoesNotSetAuthenticationTicketOnState() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var linkGenerator = GetMockLinkGenerator();
            var options = Options.Create(new AuthorizeAccessOptions() { ShowDebugPages = false });
            var userInstanceStateProvider = new InMemoryInstanceStateProvider();
            var clock = new TestableClock();
            var helper = new SignInJourneyHelper(dbContext, linkGenerator, options, userInstanceStateProvider, clock);

            var user = await TestData.CreateOneLoginUser(personId: null);
            clock.Advance();

            var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
            var journeyInstance = await CreateJourneyInstance(state);

            var ticket = CreateOneLoginAuthenticationTicket(user);

            // Act
            await helper.OnSignedInWithOneLogin(journeyInstance, ticket);

            // Assert
            user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject);
            Assert.Equal(clock.UtcNow, user.LastOneLoginSignIn);
            Assert.Null(user.FirstSignIn);
            Assert.Null(user.LastSignIn);
            Assert.Null(state.AuthenticationTicket);
        });

    [Fact]
    public Task OnSignedInWithOneLogin_UserDoesNotExist_CreatesUserInDbButDoesNotAssignAuthenticationTicketOnState() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var linkGenerator = GetMockLinkGenerator();
            var options = Options.Create(new AuthorizeAccessOptions() { ShowDebugPages = false });
            var userInstanceStateProvider = new InMemoryInstanceStateProvider();
            var clock = new TestableClock();
            var helper = new SignInJourneyHelper(dbContext, linkGenerator, options, userInstanceStateProvider, clock);

            var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
            var journeyInstance = await CreateJourneyInstance(state);

            var subject = Faker.Internet.UserName();
            var email = Faker.Internet.Email();
            var firstName = Faker.Name.First();
            var lastName = Faker.Name.Last();
            var dateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
            var ticket = CreateOneLoginAuthenticationTicket(sub: subject, email, firstName, lastName, dateOfBirth);

            // Act
            await helper.OnSignedInWithOneLogin(journeyInstance, ticket);

            // Assert
            var user = await dbContext.OneLoginUsers.SingleOrDefaultAsync(u => u.Subject == subject);
            Assert.NotNull(user);
            Assert.Equal(email, user.Email);
            Assert.Equal(clock.UtcNow, user.FirstOneLoginSignIn);
            Assert.Equal(clock.UtcNow, user.LastOneLoginSignIn);
            Assert.Null(user.FirstSignIn);
            Assert.Null(user.LastSignIn);
            Assert.NotNull(user.CoreIdentityVc);

            Assert.Null(state.AuthenticationTicket);
        });

    private AuthenticationTicket CreateOneLoginAuthenticationTicket(OneLoginUser user)
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

    private AuthenticationTicket CreateOneLoginAuthenticationTicket(
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

    private AuthorizeAccessLinkGenerator GetMockLinkGenerator()
    {
        var mock = new Mock<AuthorizeAccessLinkGenerator>();

        mock.Protected()
            .Setup<string>("GetRequiredPathByPage", ItExpr.IsNull<string>(), ItExpr.IsNull<string>(), ItExpr.IsNull<object>())
            .Returns<string, string, object>((page, handler, routeValues) =>
            {
                if (handler is not null || routeValues is not null)
                {
                    throw new NotImplementedException();
                }

                return page;
            });

        return mock.Object;
    }
}
