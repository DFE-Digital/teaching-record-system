using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class NationalInsuranceNumberTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_NotVerifiedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(createCoreIdentityVc: false);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_AlreadyAuthenticated_RedirectsToStateRedirectUri()
    {
        // Arrange
        var redirectUri = "/";
        var state = new SignInJourneyState(redirectUri, authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(person.PersonId);

        var ticket = CreateOneLoginAuthenticationTicket(oneLoginUser);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"{redirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_ValidRequest_RendersExpectedContent(bool haveExistingValueInState)
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket();
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var existingNationalInsuranceNumber = haveExistingValueInState ? Faker.Identification.UkNationalInsuranceNumber() : null;
        if (existingNationalInsuranceNumber is not null)
        {
            await journeyInstance.UpdateStateAsync(state =>
            {
                state.NationalInsuranceNumber = existingNationalInsuranceNumber;
                state.NationalInsuranceNumberSpecified = true;
            });
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        Assert.Equal(existingNationalInsuranceNumber ?? "", doc.GetElementById("NationalInsuranceNumber")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_NotAuthenticatedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
        {
            { "NationalInsuranceNumber", nationalInsuranceNumber }
        }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_NotVerifiedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var ticket = CreateOneLoginAuthenticationTicket(createCoreIdentityVc: false);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "NationalInsuranceNumber", nationalInsuranceNumber }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_AlreadyAuthenticated_RedirectsToStateRedirectUri()
    {
        // Arrange
        var redirectUri = "/";
        var state = new SignInJourneyState(redirectUri, authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: person.PersonId, firstName: person.FirstName, lastName: person.LastName, dateOfBirth: person.DateOfBirth);

        var ticket = CreateOneLoginAuthenticationTicket(oneLoginUser);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "NationalInsuranceNumber", nationalInsuranceNumber }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"{redirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_EmptyNationalInsuranceNumber_RendersError()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
        var nationalInsuranceNumber = "";
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: null, firstName: person.FirstName, lastName: person.LastName, dateOfBirth: person.DateOfBirth);

        var ticket = CreateOneLoginAuthenticationTicket(oneLoginUser);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "NationalInsuranceNumber", nationalInsuranceNumber }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "NationalInsuranceNumber", "Enter a National Insurance number");
    }

    [Fact]
    public async Task Post_InvalidNationalInsuranceNumber_RendersError()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
        var nationalInsuranceNumber = "xxx";
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: null, firstName: person.FirstName, lastName: person.LastName, dateOfBirth: person.DateOfBirth);

        var ticket = CreateOneLoginAuthenticationTicket(oneLoginUser);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "NationalInsuranceNumber", nationalInsuranceNumber }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "NationalInsuranceNumber", "Enter a National Insurance number in the correct format");
    }

    [Fact]
    public async Task Post_ValidNationalInsuranceNumberButLookupFailed_UpdatesStateAndRedirectsToTrnPage()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: null, firstName: person.FirstName, lastName: person.LastName, dateOfBirth: person.DateOfBirth);

        var ticket = CreateOneLoginAuthenticationTicket(oneLoginUser);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "NationalInsuranceNumber", nationalInsuranceNumber }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/trn?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        Assert.Equal(nationalInsuranceNumber, state.NationalInsuranceNumber);
        Assert.True(state.NationalInsuranceNumberSpecified);
        Assert.Null(state.AuthenticationTicket);
    }

    [Fact]
    public async Task Post_ValidNationalInsuranceNumberAndLookupSucceeded_UpdatesStateUpdatesOneLoginUserCompletesAuthenticationAndRedirectsToStateRedirectUri()
    {
        // Arrange
        var redirectUri = "/";
        var state = new SignInJourneyState(redirectUri, authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
        var nationalInsuranceNumber = person.NationalInsuranceNumber!;
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: null, firstName: person.FirstName, lastName: person.LastName, dateOfBirth: person.DateOfBirth);

        var ticket = CreateOneLoginAuthenticationTicket(oneLoginUser);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "NationalInsuranceNumber", nationalInsuranceNumber }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"{redirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        Assert.Equal(nationalInsuranceNumber, state.NationalInsuranceNumber);
        Assert.True(state.NationalInsuranceNumberSpecified);
        Assert.NotNull(state.AuthenticationTicket);

        oneLoginUser = await WithDbContext(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
        Assert.Equal(Clock.UtcNow, oneLoginUser.FirstSignIn);
        Assert.Equal(Clock.UtcNow, oneLoginUser.LastSignIn);
        Assert.Equal(person.PersonId, oneLoginUser.PersonId);
    }

    [Fact]
    public async Task Post_ContinueWithout_UpdatesStateAndRedirectsToTrnPage()
    {
        // Arrange
        var redirectUri = "/";
        var state = new SignInJourneyState(redirectUri, authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
        var nationalInsuranceNumber = person.NationalInsuranceNumber!;
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: null, firstName: person.FirstName, lastName: person.LastName, dateOfBirth: person.DateOfBirth);

        var ticket = CreateOneLoginAuthenticationTicket(oneLoginUser);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number/ContinueWithout?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/trn?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        Assert.Null(state.NationalInsuranceNumber);
        Assert.True(state.NationalInsuranceNumberSpecified);
    }
}
