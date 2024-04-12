using System.Diagnostics;
using System.Security.Claims;
using GovUk.OneLogin.AspNetCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.PersonSearch;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public class SignInJourneyHelperTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task OnSignedInWithOneLogin_AuthenticationOnly_UserAlreadyExistsAndTeachingRecordKnown_CompletesJourney() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var person = await TestData.CreatePerson(b => b.WithTrn(true));
            var user = await TestData.CreateOneLoginUser(person);
            Clock.Advance();

            var state = new SignInJourneyState(redirectUri: "/", serviceName: "Test Service", serviceUrl: "https://service", oneLoginAuthenticationScheme: "dummy");
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: user.Subject);

            // Act
            var result = await helper.OnSignedInWithOneLogin(journeyInstance, authenticationTicket);

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
    public Task OnSignedInWithOneLogin_AuthenticationOnly_UserAlreadyExistsButTeachingNotRecordKnown_RequestsIdentityVerification() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var user = await TestData.CreateOneLoginUser(personId: null);
            Clock.Advance();

            var state = new SignInJourneyState(redirectUri: "/", serviceName: "Test Service", serviceUrl: "https://service", oneLoginAuthenticationScheme: "dummy");
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: user.Subject);

            // Act
            var result = await helper.OnSignedInWithOneLogin(journeyInstance, authenticationTicket);

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
    public Task OnSignedInWithOneLogin_AuthenticationOnly_UserDoesNotExist_RequestsIdentityVerification() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var state = new SignInJourneyState(redirectUri: "/", serviceName: "Test Service", serviceUrl: "https://service", oneLoginAuthenticationScheme: "dummy");
            var journeyInstance = await CreateJourneyInstance(state);

            var subject = TestData.CreateOneLoginUserSubject();
            var authenticationTicket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, sub: subject);

            // Act
            var result = await helper.OnSignedInWithOneLogin(journeyInstance, authenticationTicket);

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
    public Task OnSignedInWithOneLogin_AuthenticationAndVerification_VerificationFailed_RedirectsToErrorPage() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var user = await TestData.CreateOneLoginUser(personId: null);
            Clock.Advance();

            var state = new SignInJourneyState(redirectUri: "/", serviceName: "Test Service", serviceUrl: "https://service", oneLoginAuthenticationScheme: "dummy");
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                createCoreIdentityVc: false);
            var onAuthenticatedResult = await helper.OnSignedInWithOneLogin(journeyInstance, authenticationTicket);
            Debug.Assert(onAuthenticatedResult is ChallengeHttpResult);

            // Act
            var result = await helper.OnUserVerificationWithOneLoginFailed(journeyInstance);

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
    public Task OnSignedInWithOneLogin_AuthenticationAndVerification_VerificationSucceeded_RedirectsToStartOfMatchingJourney() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var helper = CreateHelper(dbContext);

            var user = await TestData.CreateOneLoginUser(personId: null);
            Clock.Advance();

            var firstName = Faker.Name.First();
            var lastName = Faker.Name.Last();
            var dateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());

            var state = new SignInJourneyState(redirectUri: "/", serviceName: "Test Service", serviceUrl: "https://service", oneLoginAuthenticationScheme: "dummy");
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                firstName: firstName,
                lastName: lastName,
                dateOfBirth: dateOfBirth,
                createCoreIdentityVc: false);
            var onAuthenticatedResult = await helper.OnSignedInWithOneLogin(journeyInstance, authenticationTicket);
            Debug.Assert(onAuthenticatedResult is ChallengeHttpResult);

            var verifiedTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
                sub: user.Subject,
                firstName: firstName,
                lastName: lastName,
                dateOfBirth: dateOfBirth,
                createCoreIdentityVc: true);

            // Act
            var result = await helper.OnSignedInWithOneLogin(journeyInstance, verifiedTicket);

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
    public Task TryMatchToTeachingRecord_MatchesZeroResults_ReturnsFalseAndDoesNotSetAuthenticationTicket() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var personSearchServiceMock = new Mock<IPersonSearchService>();
            var helper = CreateHelper(dbContext, personSearchServiceMock.Object);

            var verifiedFirstName = Faker.Name.Last();
            var verifiedLastName = Faker.Name.Last();
            var verifiedDateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
            var user = await TestData.CreateOneLoginUser(personId: null, verifiedInfo: ([verifiedFirstName, verifiedLastName], verifiedDateOfBirth));
            Clock.Advance();

            var state = new SignInJourneyState(redirectUri: "/", serviceName: "Test Service", serviceUrl: "https://service", oneLoginAuthenticationScheme: "dummy");
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                createCoreIdentityVc: false);
            await helper.OnSignedInWithOneLogin(journeyInstance, authenticationTicket);

            await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, Faker.Identification.UkNationalInsuranceNumber()));

            personSearchServiceMock
                .Setup(mock => mock.Search(
                    It.Is<IEnumerable<string[]>>(names => names.SequenceEqual(state.VerifiedNames!)),
                    It.Is<IEnumerable<DateOnly>>(dobs => dobs.SequenceEqual(state.VerifiedDatesOfBirth!)),
                    state.NationalInsuranceNumber,
                    state.Trn))
                .ReturnsAsync(Array.Empty<PersonSearchResult>());

            // Act
            var result = await helper.TryMatchToTeachingRecord(journeyInstance);

            // Assert
            Assert.False(result);

            user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject);
            Assert.Null(user.PersonId);

            Assert.Null(state.AuthenticationTicket);
        });

    [Fact]
    public Task TryMatchToTeachingRecord_MatchesMultipleResults_ReturnsFalseAndDoesNotSetAuthenticationTicket() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var personSearchServiceMock = new Mock<IPersonSearchService>();
            var helper = CreateHelper(dbContext, personSearchServiceMock.Object);

            var firstName = Faker.Name.Last();
            var lastName = Faker.Name.Last();
            var dateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
            var person1 = await TestData.CreatePerson(b => b.WithTrn().WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithNationalInsuranceNumber());
            var person2 = await TestData.CreatePerson(b => b.WithTrn().WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithNationalInsuranceNumber());

            var user = await TestData.CreateOneLoginUser(personId: null, verifiedInfo: ([firstName, lastName], dateOfBirth));
            Clock.Advance();

            var state = new SignInJourneyState(redirectUri: "/", serviceName: "Test Service", serviceUrl: "https://service", oneLoginAuthenticationScheme: "dummy");
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                createCoreIdentityVc: false);
            await helper.OnSignedInWithOneLogin(journeyInstance, authenticationTicket);

            await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, person1.NationalInsuranceNumber));

            personSearchServiceMock
                .Setup(mock => mock.Search(
                    It.Is<IEnumerable<string[]>>(names => names.SequenceEqual(state.VerifiedNames!)),
                    It.Is<IEnumerable<DateOnly>>(dobs => dobs.SequenceEqual(state.VerifiedDatesOfBirth!)),
                    state.NationalInsuranceNumber,
                    state.Trn))
                .ReturnsAsync(new[]
                {
                    new PersonSearchResult()
                    {
                        PersonId = person1.PersonId,
                        FirstName = person1.FirstName,
                        MiddleName = person1.MiddleName,
                        LastName = person1.LastName,
                        DateOfBirth = person1.DateOfBirth,
                        Trn = person1.Trn,
                        NationalInsuranceNumber = person1.NationalInsuranceNumber
                    },
                    new PersonSearchResult()
                    {
                        PersonId = person2.PersonId,
                        FirstName = person2.FirstName,
                        MiddleName = person2.MiddleName,
                        LastName = person2.LastName,
                        DateOfBirth = person2.DateOfBirth,
                        Trn = person2.Trn,
                        NationalInsuranceNumber = person2.NationalInsuranceNumber
                    }
                });

            // Act
            var result = await helper.TryMatchToTeachingRecord(journeyInstance);

            // Assert
            Assert.False(result);

            user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject);
            Assert.Null(user.PersonId);

            Assert.Null(state.AuthenticationTicket);
        });

    [Fact]
    public Task TryMatchToTeachingRecord_MatchesSingleResultWithoutTrn_ReturnsFalseAndDoesNotSetAuthenticationTicket() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var personSearchServiceMock = new Mock<IPersonSearchService>();
            var helper = CreateHelper(dbContext, personSearchServiceMock.Object);

            var firstName = Faker.Name.Last();
            var lastName = Faker.Name.Last();
            var dateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
            var person = await TestData.CreatePerson(b => b.WithTrn(false).WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithNationalInsuranceNumber());

            var user = await TestData.CreateOneLoginUser(personId: null, verifiedInfo: ([firstName, lastName], dateOfBirth));
            Clock.Advance();

            var state = new SignInJourneyState(redirectUri: "/", serviceName: "Test Service", serviceUrl: "https://service", oneLoginAuthenticationScheme: "dummy");
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                createCoreIdentityVc: false);
            await helper.OnSignedInWithOneLogin(journeyInstance, authenticationTicket);

            await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, person.NationalInsuranceNumber));

            personSearchServiceMock
                .Setup(mock => mock.Search(
                    It.Is<IEnumerable<string[]>>(names => names.SequenceEqual(state.VerifiedNames!)),
                    It.Is<IEnumerable<DateOnly>>(dobs => dobs.SequenceEqual(state.VerifiedDatesOfBirth!)),
                    state.NationalInsuranceNumber,
                    state.Trn))
                .ReturnsAsync(new[]
                {
                    new PersonSearchResult()
                    {
                        PersonId = person.PersonId,
                        FirstName = person.FirstName,
                        MiddleName = person.MiddleName,
                        LastName = person.LastName,
                        DateOfBirth = person.DateOfBirth,
                        Trn = person.Trn,
                        NationalInsuranceNumber = person.NationalInsuranceNumber
                    }
                });

            // Act
            var result = await helper.TryMatchToTeachingRecord(journeyInstance);

            // Assert
            Assert.False(result);

            user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject);
            Assert.Null(user.PersonId);

            Assert.Null(state.AuthenticationTicket);
        });

    [Fact]
    public Task TryMatchToTeachingRecord_MatchesSingleResult_ReturnsTrueAndUpdatesOneLoginUserAssignsAuthenticationTicket() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var personSearchServiceMock = new Mock<IPersonSearchService>();
            var helper = CreateHelper(dbContext, personSearchServiceMock.Object);

            var firstName = Faker.Name.Last();
            var lastName = Faker.Name.Last();
            var dateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
            var person = await TestData.CreatePerson(b => b.WithTrn().WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithNationalInsuranceNumber());

            var user = await TestData.CreateOneLoginUser(personId: null, verifiedInfo: ([firstName, lastName], dateOfBirth));
            Clock.Advance();

            var state = new SignInJourneyState(redirectUri: "/", serviceName: "Test Service", serviceUrl: "https://service", oneLoginAuthenticationScheme: "dummy");
            var journeyInstance = await CreateJourneyInstance(state);

            var authenticationTicket = CreateOneLoginAuthenticationTicket(
                vtr: SignInJourneyHelper.AuthenticationOnlyVtr,
                sub: user.Subject,
                createCoreIdentityVc: false);
            await helper.OnSignedInWithOneLogin(journeyInstance, authenticationTicket);

            await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, person.NationalInsuranceNumber));

            personSearchServiceMock
                .Setup(mock => mock.Search(
                    It.Is<IEnumerable<string[]>>(names => names.SequenceEqual(state.VerifiedNames!)),
                    It.Is<IEnumerable<DateOnly>>(dobs => dobs.SequenceEqual(state.VerifiedDatesOfBirth!)),
                    state.NationalInsuranceNumber,
                    state.Trn))
                .ReturnsAsync(new[]
                {
                    new PersonSearchResult()
                    {
                        PersonId = person.PersonId,
                        FirstName = person.FirstName,
                        MiddleName = person.MiddleName,
                        LastName = person.LastName,
                        DateOfBirth = person.DateOfBirth,
                        Trn = person.Trn,
                        NationalInsuranceNumber = person.NationalInsuranceNumber
                    }
                });

            // Act
            var result = await helper.TryMatchToTeachingRecord(journeyInstance);

            // Assert
            Assert.True(result);

            user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == user.Subject);
            Assert.Equal(Clock.UtcNow, user.FirstSignIn);
            Assert.Equal(Clock.UtcNow, user.LastSignIn);
            Assert.Equal(person.PersonId, user.PersonId);

            Assert.NotNull(state.AuthenticationTicket);
            Assert.Equal(person.Trn, state.AuthenticationTicket.Principal.FindFirstValue(ClaimTypes.Trn));
        });

    private SignInJourneyHelper CreateHelper(TrsDbContext dbContext, IPersonSearchService? personSearchService = null)
    {
        var linkGenerator = new FakeLinkGenerator();
        var options = Options.Create(new AuthorizeAccessOptions() { ShowDebugPages = false });
        personSearchService ??= Mock.Of<IPersonSearchService>();

        return ActivatorUtilities.CreateInstance<SignInJourneyHelper>(HostFixture.Services, dbContext, personSearchService, linkGenerator, options, Clock);
    }

    private class FakeLinkGenerator : AuthorizeAccessLinkGenerator
    {
        protected override string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null) => page;
    }
}
