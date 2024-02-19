namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class NotFoundTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/not-found?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/not-found?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/not-found?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"{redirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_NationalInsuranceNumberNotSpecified_RedirectsToStartOfMatchingQuestions()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket();
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        await journeyInstance.UpdateStateAsync(state =>
        {
            state.NationalInsuranceNumberSpecified = false;
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/not-found?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_TrnNotSpecified_RedirectsToStartOfMatchingQuestions()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket();
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        await journeyInstance.UpdateStateAsync(state =>
        {
            state.NationalInsuranceNumberSpecified = true;
            state.TrnSpecified = false;
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/not-found?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket();
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        await journeyInstance.UpdateStateAsync(async state =>
        {
            state.NationalInsuranceNumberSpecified = true;
            state.Trn = await TestData.GenerateTrn();
            state.TrnSpecified = true;
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/not-found?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
    }
}
