using System.Diagnostics;
using System.Security.Claims;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.PersonMatching;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public class SignInJourneyHelperTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task OnOneLoginCallback_AuthenticationOnly_UserAlreadyExistsAndTeachingRecordKnown_CompletesJourney() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var person = await TestData.CreatePerson(b => b.WithTrn(true));
            var user = await TestData.CreateOneLoginUser(person);
            Clock.Advance();

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: user.Subject);

            // Act
            var result = await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);

            // Assert
            Assert.NotNull(state.AuthenticationTicket);

            user = await WithDbContext(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
            Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
            Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
            Assert.NotEqual(Clock.UtcNow, user.FirstSignIn);
            Assert.Equal(Clock.UtcNow, user.LastSignIn);

            var redirectResult = Assert.IsType<RedirectHttpResult>(result);
            Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
        });

    [Fact]
    public Task OnOneLoginCallback_AuthenticationOnly_UserAlreadyExistsButTeachingNotRecordKnown_RequestsIdentityVerification() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var user = await TestData.CreateOneLoginUser(personId: null);
            Clock.Advance();

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: user.Subject);

            // Act
            var result = await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);

            // Assert
            Assert.NotNull(state.OneLoginAuthenticationTicket);
            Assert.Null(state.AuthenticationTicket);

            user = await WithDbContext(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
            Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
            Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
            Assert.NotEqual(Clock.UtcNow, user.FirstSignIn);
            Assert.NotEqual(Clock.UtcNow, user.LastSignIn);

            var challengeResult = Assert.IsType<ChallengeHttpResult>(result);
            Assert.Equal(SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr, challengeResult.Properties?.GetVectorOfTrust());
        });

    [Fact]
    public Task OnOneLoginCallback_AuthenticationOnly_UserDoesNotExist_RequestsIdentityVerification() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default);
            var journeyInstance = await CreateJourneyInstance(state);

            var subject = TestData.CreateOneLoginUserSubject();
            var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: subject);

            // Act
            var result = await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);

            // Assert
            Assert.NotNull(state.OneLoginAuthenticationTicket);
            Assert.Null(state.AuthenticationTicket);

            var user = await WithDbContext(dbContext => dbContext.OneLoginUsers.SingleOrDefaultAsync(u => u.Subject == subject));
            Assert.NotNull(user);
            Assert.Equal(Clock.UtcNow, user.FirstOneLoginSignIn);
            Assert.Null(user.FirstSignIn);
            Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
            Assert.NotEqual(Clock.UtcNow, user.LastSignIn);

            var challengeResult = Assert.IsType<ChallengeHttpResult>(result);
            Assert.Equal(SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr, challengeResult.Properties?.GetVectorOfTrust());
        });

    [Fact]
    public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationFailed_RedirectsToErrorPage() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var user = await TestData.CreateOneLoginUser(personId: null);
            Clock.Advance();

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                email: user.Subject,
                createCoreIdentityVc: false);
            var callbackResult = await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);
            Debug.Assert(callbackResult is ChallengeHttpResult);

            // Act
            var result = await helper.OnVerificationFailed(journeyInstance);

            // Assert
            Assert.NotNull(state.OneLoginAuthenticationTicket);
            Assert.Null(state.AuthenticationTicket);

            user = await WithDbContext(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
            Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
            Assert.Null(user.FirstSignIn);
            Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
            Assert.NotEqual(Clock.UtcNow, user.LastSignIn);

            var redirectResult = Assert.IsType<RedirectHttpResult>(result);
            Assert.Equal($"/NotVerified?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
        });

    [Fact]
    public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceeded_RedirectsToStartOfMatchingJourney() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var user = await TestData.CreateOneLoginUser(personId: null);
            Clock.Advance();

            var firstName = Faker.Name.First();
            var lastName = Faker.Name.Last();
            var dateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                email: user.Subject,
                createCoreIdentityVc: false);
            var callbackResult = await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);
            Debug.Assert(callbackResult is ChallengeHttpResult);

            var verifiedTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
                sub: user.Subject,
                firstName: firstName,
                lastName: lastName,
                dateOfBirth: dateOfBirth,
                createCoreIdentityVc: true);

            // Act
            var result = await helper.OnOneLoginCallback(journeyInstance, verifiedTicket);

            // Assert
            Assert.NotNull(state.OneLoginAuthenticationTicket);
            Assert.Null(state.AuthenticationTicket);

            user = await WithDbContext(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
            Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
            Assert.Null(user.FirstSignIn);
            Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
            Assert.NotEqual(Clock.UtcNow, user.LastSignIn);
            Assert.Equal(Clock.UtcNow, user.VerifiedOn);
            Assert.Equal(OneLoginUserVerificationRoute.OneLogin, user.VerificationRoute);
            Assert.NotNull(user.VerifiedNames);
            Assert.Collection(user.VerifiedNames, names => Assert.Collection(names, n => Assert.Equal(firstName, n), n => Assert.Equal(lastName, n)));
            Assert.NotNull(user.VerifiedDatesOfBirth);
            Assert.Collection(user.VerifiedDatesOfBirth, dob => Assert.Equal(dateOfBirth, dob));

            var redirectResult = Assert.IsType<RedirectHttpResult>(result);
            Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
        });

    [Fact]
    public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededAndTrnTokenMatchesVerifiedLastNameAndDateOfBirth_CompletesJourney() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var person = await TestData.CreatePerson(b => b.WithTrn(true));
            var user = await TestData.CreateOneLoginUser(personId: null);
            Clock.Advance();

            var trnToken = await CreateTrnToken(person.Trn!, user.Email);

            var firstName = person.FirstName;
            var lastName = person.LastName;
            var dateOfBirth = person.DateOfBirth;

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default,
                trnToken);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                email: user.Subject,
                createCoreIdentityVc: false);
            var callbackResult = await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);
            Debug.Assert(callbackResult is ChallengeHttpResult);

            var verifiedTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
                sub: user.Subject,
                firstName: firstName,
                lastName: lastName,
                dateOfBirth: dateOfBirth,
                createCoreIdentityVc: true);

            // Act
            var result = await helper.OnOneLoginCallback(journeyInstance, verifiedTicket);

            // Assert
            Assert.NotNull(state.AuthenticationTicket);

            user = await WithDbContext(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject));
            Assert.NotEqual(Clock.UtcNow, user.FirstOneLoginSignIn);
            Assert.Equal(Clock.UtcNow, user.LastOneLoginSignIn);
            Assert.Equal(Clock.UtcNow, user.FirstSignIn);
            Assert.Equal(Clock.UtcNow, user.LastSignIn);
            Assert.Equal(OneLoginUserMatchRoute.TrnToken, user.MatchRoute);

            var redirectResult = Assert.IsType<RedirectHttpResult>(result);
            Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
        });

    [Fact]
    public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButTrnTokenDoesNotExist_RedirectsToStartOfMatchingJourney() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var person = await TestData.CreatePerson(b => b.WithTrn(true));
            var user = await TestData.CreateOneLoginUser(personId: null);
            Clock.Advance();

            var trnToken = Guid.NewGuid().ToString();

            var firstName = person.FirstName;
            var lastName = person.LastName;
            var dateOfBirth = person.DateOfBirth;

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default,
                trnToken);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                email: user.Subject,
                createCoreIdentityVc: false);
            var callbackResult = await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);
            Debug.Assert(callbackResult is ChallengeHttpResult);

            var verifiedTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
                sub: user.Subject,
                firstName: firstName,
                lastName: lastName,
                dateOfBirth: dateOfBirth,
                createCoreIdentityVc: true);

            // Act
            var result = await helper.OnOneLoginCallback(journeyInstance, verifiedTicket);

            // Assert
            var redirectResult = Assert.IsType<RedirectHttpResult>(result);
            Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
        });

    [Fact]
    public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButTrnTokenHasExpired_RedirectsToStartOfMatchingJourney() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var person = await TestData.CreatePerson(b => b.WithTrn(true));
            var user = await TestData.CreateOneLoginUser(personId: null);
            Clock.Advance();

            var trnToken = await CreateTrnToken(person.Trn!, user.Email, expires: TimeSpan.FromSeconds(-1));

            var firstName = person.FirstName;
            var lastName = person.LastName;
            var dateOfBirth = person.DateOfBirth;

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default,
                trnToken);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                email: user.Subject,
                createCoreIdentityVc: false);
            var callbackResult = await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);
            Debug.Assert(callbackResult is ChallengeHttpResult);

            var verifiedTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
                sub: user.Subject,
                firstName: firstName,
                lastName: lastName,
                dateOfBirth: dateOfBirth,
                createCoreIdentityVc: true);

            // Act
            var result = await helper.OnOneLoginCallback(journeyInstance, verifiedTicket);

            // Assert
            var redirectResult = Assert.IsType<RedirectHttpResult>(result);
            Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
        });

    [Fact]
    public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButTrnTokenHasAlreadyBeenUsed_RedirectsToStartOfMatchingJourney() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var person = await TestData.CreatePerson(b => b.WithTrn(true));
            var user = await TestData.CreateOneLoginUser(personId: null);
            Clock.Advance();

            var trnToken = await CreateTrnToken(person.Trn!, user.Email, userId: Guid.NewGuid());

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default,
                trnToken);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                email: user.Subject,
                createCoreIdentityVc: false);
            var callbackResult = await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);
            Debug.Assert(callbackResult is ChallengeHttpResult);

            var verifiedTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
                sub: user.Subject,
                firstName: person.FirstName,
                lastName: person.LastName,
                dateOfBirth: person.DateOfBirth,
                createCoreIdentityVc: true);

            // Act
            var result = await helper.OnOneLoginCallback(journeyInstance, verifiedTicket);

            // Assert
            var redirectResult = Assert.IsType<RedirectHttpResult>(result);
            Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
        });

    [Fact]
    public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButRecordFromTrnTokenDoesNotMatchLastName_RedirectsToStartOfMatchingJourney() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var person = await TestData.CreatePerson(b => b.WithTrn(true));
            var user = await TestData.CreateOneLoginUser(personId: null);
            Clock.Advance();

            var trnToken = await CreateTrnToken(person.Trn!, user.Email);

            var firstName = person.FirstName;
            var lastName = TestData.GenerateChangedLastName(person.LastName);
            var dateOfBirth = person.DateOfBirth;

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default,
                trnToken);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                email: user.Subject,
                createCoreIdentityVc: false);
            var callbackResult = await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);
            Debug.Assert(callbackResult is ChallengeHttpResult);

            var verifiedTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
                sub: user.Subject,
                firstName: firstName,
                lastName: lastName,
                dateOfBirth: dateOfBirth,
                createCoreIdentityVc: true);

            // Act
            var result = await helper.OnOneLoginCallback(journeyInstance, verifiedTicket);

            // Assert
            var redirectResult = Assert.IsType<RedirectHttpResult>(result);
            Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
        });

    [Fact]
    public Task OnOneLoginCallback_AuthenticationAndVerification_VerificationSucceededButRecordFromTrnTokenDoesNotMatchDateOfBirth_RedirectsToStartOfMatchingJourney() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var person = await TestData.CreatePerson(b => b.WithTrn(true));
            var user = await TestData.CreateOneLoginUser(personId: null);
            Clock.Advance();

            var trnToken = await CreateTrnToken(person.Trn!, user.Email);

            var firstName = person.FirstName;
            var lastName = person.LastName;
            var dateOfBirth = person.DateOfBirth.AddDays(1);

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default,
                trnToken);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                email: user.Subject,
                createCoreIdentityVc: false);
            var callbackResult = await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);
            Debug.Assert(callbackResult is ChallengeHttpResult);

            var verifiedTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
                sub: user.Subject,
                firstName: firstName,
                lastName: lastName,
                dateOfBirth: dateOfBirth,
                createCoreIdentityVc: true);

            // Act
            var result = await helper.OnOneLoginCallback(journeyInstance, verifiedTicket);

            // Assert
            var redirectResult = Assert.IsType<RedirectHttpResult>(result);
            Assert.Equal($"/Connect?{journeyInstance.GetUniqueIdQueryParameter()}", redirectResult.Url);
        });

    [Fact]
    public Task TryMatchToTeachingRecord_MatchesZeroResults_ReturnsFalseAndDoesNotSetAuthenticationTicket() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var personMatchingServiceMock = new Mock<IPersonMatchingService>();
            var helper = CreateHelper(dbContext, personMatchingServiceMock.Object);

            var verifiedFirstName = Faker.Name.Last();
            var verifiedLastName = Faker.Name.Last();
            var verifiedDateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
            var user = await TestData.CreateOneLoginUser(personId: null, verifiedInfo: ([verifiedFirstName, verifiedLastName], verifiedDateOfBirth));
            Clock.Advance();

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                createCoreIdentityVc: false);
            await helper.OnOneLoginCallback(journeyInstance,
                authenticationTicket);

            await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true,
                Faker.Identification.UkNationalInsuranceNumber()));

            personMatchingServiceMock
                .Setup(mock => mock.Match(It.Is<MatchRequest>(r =>
                    r.Names.SequenceEqual(state.VerifiedNames!) &&
                    r.DatesOfBirth.SequenceEqual(state.VerifiedDatesOfBirth!) &&
                    r.NationalInsuranceNumber == state.NationalInsuranceNumber &&
                    r.Trn == state.Trn)))
                .ReturnsAsync(value: null);

            // Act
            var result = await helper.TryMatchToTeachingRecord(journeyInstance);

            // Assert
            Assert.False(result);

            user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject);
            Assert.Null(user.PersonId);

            Assert.Null(state.AuthenticationTicket);
        });

    [Fact]
    public Task TryMatchToTeachingRecord_MatchesWithoutTrn_ReturnsFalseAndDoesNotSetAuthenticationTicket() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var personMatchingServiceMock = new Mock<IPersonMatchingService>();
            var helper = CreateHelper(dbContext, personMatchingServiceMock.Object);

            var firstName = Faker.Name.Last();
            var lastName = Faker.Name.Last();
            var dateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
            var person = await TestData.CreatePerson(b => b.WithTrn(false).WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithNationalInsuranceNumber());

            var user = await TestData.CreateOneLoginUser(personId: null, verifiedInfo: ([firstName, lastName], dateOfBirth));
            Clock.Advance();

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                createCoreIdentityVc: false);
            await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);

            await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, person.NationalInsuranceNumber));

            personMatchingServiceMock
                .Setup(mock => mock.Match(It.Is<MatchRequest>(r =>
                    r.Names.SequenceEqual(state.VerifiedNames!) &&
                    r.DatesOfBirth.SequenceEqual(state.VerifiedDatesOfBirth!) &&
                    r.NationalInsuranceNumber == state.NationalInsuranceNumber &&
                    r.Trn == state.Trn)))
                .ReturnsAsync(new MatchResult(
                    person.PersonId,
                    person.Trn!,
                    new Dictionary<OneLoginUserMatchedAttribute, string>()
                    {
                        { OneLoginUserMatchedAttribute.FullName, $"{person.FirstName} {person.LastName}" },
                        { OneLoginUserMatchedAttribute.NationalInsuranceNumber, person.NationalInsuranceNumber! },
                    }));

            // Act
            var result = await helper.TryMatchToTeachingRecord(journeyInstance);

            // Assert
            Assert.False(result);

            user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject);
            Assert.Null(user.PersonId);

            Assert.Null(state.AuthenticationTicket);
        });

    [Fact]
    public Task TryMatchToTeachingRecord_Matches_ReturnsTrueAndUpdatesOneLoginUserAssignsAuthenticationTicket() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var personMatchingServiceMock = new Mock<IPersonMatchingService>();
            var helper = CreateHelper(dbContext, personMatchingServiceMock.Object);

            var firstName = Faker.Name.Last();
            var lastName = Faker.Name.Last();
            var dateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
            var person = await TestData.CreatePerson(b => b.WithTrn().WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithNationalInsuranceNumber());

            var user = await TestData.CreateOneLoginUser(personId: null, verifiedInfo: ([firstName, lastName], dateOfBirth));
            Clock.Advance();

            var state = new SignInJourneyState(
                redirectUri: "/",
                serviceName: "Test Service",
                serviceUrl: "https://service",
                oneLoginAuthenticationScheme: "dummy",
                clientApplicationUserId: default);
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                createCoreIdentityVc: false);
            await helper.OnOneLoginCallback(journeyInstance, authenticationTicket);

            await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, person.NationalInsuranceNumber));

            personMatchingServiceMock
                .Setup(mock => mock.Match(It.Is<MatchRequest>(r =>
                    r.Names.SequenceEqual(state.VerifiedNames!) &&
                    r.DatesOfBirth.SequenceEqual(state.VerifiedDatesOfBirth!) &&
                    r.NationalInsuranceNumber == state.NationalInsuranceNumber &&
                    r.Trn == state.Trn)))
                .ReturnsAsync(new MatchResult(
                    person.PersonId,
                    person.Trn!,
                    new Dictionary<OneLoginUserMatchedAttribute, string>()
                    {
                        { OneLoginUserMatchedAttribute.FullName, $"{person.FirstName} {person.LastName}" },
                        { OneLoginUserMatchedAttribute.NationalInsuranceNumber, person.NationalInsuranceNumber! },
                    }));

            // Act
            var result = await helper.TryMatchToTeachingRecord(journeyInstance);

            // Assert
            Assert.True(result);

            user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject);
            Assert.Equal(Clock.UtcNow, user.FirstSignIn);
            Assert.Equal(Clock.UtcNow, user.LastSignIn);
            Assert.Equal(person.PersonId, user.PersonId);
            Assert.Equal(OneLoginUserMatchRoute.Interactive, user.MatchRoute);
            Assert.NotNull(user.MatchedAttributes);
            Assert.NotEmpty(user.MatchedAttributes);

            Assert.NotNull(state.AuthenticationTicket);
            Assert.Equal(person.Trn, state.AuthenticationTicket.Principal.FindFirstValue(ClaimTypes.Trn));
        });

    private SignInJourneyHelper CreateHelper(TrsDbContext dbContext, IPersonMatchingService? personMatchingService = null)
    {
        var linkGenerator = new FakeLinkGenerator();
        var options = Options.Create(new AuthorizeAccessOptions() { ShowDebugPages = false });
        personMatchingService ??= Mock.Of<IPersonMatchingService>();

        return ActivatorUtilities.CreateInstance<SignInJourneyHelper>(HostFixture.Services, dbContext, personMatchingService, linkGenerator, options, Clock);
    }

    private async Task<string> CreateTrnToken(string trn, string email, TimeSpan? expires = null, Guid? userId = null)
    {
        var trnToken = Guid.NewGuid().ToString();
        var expiresUtc = Clock.UtcNow.Add(expires ?? TimeSpan.FromHours(1));

        using (var idDbContext = HostFixture.Services.GetRequiredService<IdDbContext>())
        {
            idDbContext.TrnTokens.Add(new IdTrnToken()
            {
                TrnToken = trnToken,
                Trn = trn,
                CreatedUtc = Clock.UtcNow,
                ExpiresUtc = expiresUtc,
                Email = email,
                UserId = userId
            });

            await idDbContext.SaveChangesAsync();
        }

        return trnToken;
    }

    private class FakeLinkGenerator : AuthorizeAccessLinkGenerator
    {
        protected override string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null) => page;
    }
}
