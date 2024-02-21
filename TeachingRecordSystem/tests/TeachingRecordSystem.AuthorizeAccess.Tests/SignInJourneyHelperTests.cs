using System.Security.Claims;
using Microsoft.Extensions.Options;
using Moq.Protected;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Services.PersonSearch;

namespace TeachingRecordSystem.AuthorizeAccess.Tests;

public class SignInJourneyHelperTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task OnSignedInWithOneLogin_WithCoreIdentityVc_SetsOneLoginAuthenticationTicketAndVerifiedPropertiesOnState() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var clock = new TestableClock();
            var helper = CreateHelper(dbContext, clock);

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
            var clock = new TestableClock();
            var helper = CreateHelper(dbContext, clock);

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
            var clock = new TestableClock();
            var helper = CreateHelper(dbContext, clock);

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
            var clock = new TestableClock();
            var helper = CreateHelper(dbContext, clock);

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
            var clock = new TestableClock();
            var helper = CreateHelper(dbContext, clock);

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

    [Fact]
    public Task TryMatchToTeachingRecord_MatchesZeroResults_DoesNotSetAuthenticationTicketAndReturnsFalse() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var clock = new TestableClock();
            var personSearchServiceMock = new Mock<IPersonSearchService>();
            var helper = CreateHelper(dbContext, clock, personSearchServiceMock.Object);

            var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
            var journeyInstance = await CreateJourneyInstance(state);

            var subject = Faker.Internet.UserName();
            var ticket = CreateOneLoginAuthenticationTicket(sub: subject);
            await helper.OnSignedInWithOneLogin(journeyInstance, ticket);

            await journeyInstance.UpdateStateAsync(state =>
            {
                state.NationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
                state.NationalInsuranceNumberSpecified = true;
            });

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

            var user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == subject);
            Assert.Null(user.PersonId);

            Assert.Null(state.AuthenticationTicket);
        });

    [Fact]
    public Task TryMatchToTeachingRecord_MatchesMultipleResults_DoesNotSetAuthenticationTicketAndReturnsFalse() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var clock = new TestableClock();
            var personSearchServiceMock = new Mock<IPersonSearchService>();
            var helper = CreateHelper(dbContext, clock, personSearchServiceMock.Object);

            var person1 = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
            var person2 = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());

            var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
            var journeyInstance = await CreateJourneyInstance(state);

            var subject = Faker.Internet.UserName();
            var ticket = CreateOneLoginAuthenticationTicket(sub: subject);
            await helper.OnSignedInWithOneLogin(journeyInstance, ticket);

            await journeyInstance.UpdateStateAsync(state =>
            {
                state.NationalInsuranceNumber = person1.NationalInsuranceNumber;
                state.NationalInsuranceNumberSpecified = true;
            });

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

            var user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == subject);
            Assert.Null(user.PersonId);

            Assert.Null(state.AuthenticationTicket);
        });

    [Fact]
    public Task TryMatchToTeachingRecord_MatchesSingleResultWithoutTrn_DoesNotSetAuthenticationTicketAndReturnsFalse() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var clock = new TestableClock();
            var personSearchServiceMock = new Mock<IPersonSearchService>();
            var helper = CreateHelper(dbContext, clock, personSearchServiceMock.Object);

            var person = await TestData.CreatePerson(b => b.WithTrn(false).WithNationalInsuranceNumber());

            var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
            var journeyInstance = await CreateJourneyInstance(state);

            var subject = Faker.Internet.UserName();
            var ticket = CreateOneLoginAuthenticationTicket(sub: subject);
            await helper.OnSignedInWithOneLogin(journeyInstance, ticket);

            await journeyInstance.UpdateStateAsync(state =>
            {
                state.NationalInsuranceNumber = person.NationalInsuranceNumber;
                state.NationalInsuranceNumberSpecified = true;
            });

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

            var user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == subject);
            Assert.Null(user.PersonId);

            Assert.Null(state.AuthenticationTicket);
        });

    [Fact]
    public Task TryMatchToTeachingRecord_MatchesSingleResult_UpdatesOneLoginUserAssignsAuthenticationTicketAndReturnsTrue() =>
        WithDbContext(async dbContext =>
        {
            // Arrange
            var clock = new TestableClock();
            var personSearchServiceMock = new Mock<IPersonSearchService>();
            var helper = CreateHelper(dbContext, clock, personSearchServiceMock.Object);

            var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());

            var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
            var journeyInstance = await CreateJourneyInstance(state);

            var subject = Faker.Internet.UserName();
            var ticket = CreateOneLoginAuthenticationTicket(sub: subject);
            await helper.OnSignedInWithOneLogin(journeyInstance, ticket);

            await journeyInstance.UpdateStateAsync(state =>
            {
                state.NationalInsuranceNumber = person.NationalInsuranceNumber;
                state.NationalInsuranceNumberSpecified = true;
            });

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

            var user = await dbContext.OneLoginUsers.SingleAsync(u => u.Subject == subject);
            Assert.Equal(clock.UtcNow, user.FirstSignIn);
            Assert.Equal(clock.UtcNow, user.LastSignIn);
            Assert.Equal(person.PersonId, user.PersonId);

            Assert.NotNull(state.AuthenticationTicket);
            Assert.Equal(person.Trn, state.AuthenticationTicket.Principal.FindFirstValue(ClaimTypes.Trn));
            Assert.Equal(person.PersonId.ToString(), state.AuthenticationTicket.Principal.FindFirstValue(ClaimTypes.PersonId));
        });

    private SignInJourneyHelper CreateHelper(TrsDbContext dbContext, IClock clock, IPersonSearchService? personSearchService = null)
    {
        var linkGenerator = GetMockLinkGenerator();
        var options = Options.Create(new AuthorizeAccessOptions() { ShowDebugPages = false });
        personSearchService ??= Mock.Of<IPersonSearchService>();

        return ActivatorUtilities.CreateInstance<SignInJourneyHelper>(HostFixture.Services, dbContext, personSearchService, linkGenerator, options, clock);
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
