namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class TrnTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", oneLoginAuthenticationScheme: "dummy", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_NotVerifiedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", oneLoginAuthenticationScheme: "dummy", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr, createCoreIdentityVc: false);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var state = new SignInJourneyState(redirectUri, oneLoginAuthenticationScheme: "dummy", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(person.PersonId);

        var ticket = CreateOneLoginAuthenticationTicket(
            vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
            sub: oneLoginUser.Subject,
            email: oneLoginUser.Email,
            firstName: person.FirstName,
            lastName: person.LastName,
            dateOfBirth: person.DateOfBirth);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var state = new SignInJourneyState(redirectUri: "/", oneLoginAuthenticationScheme: "dummy", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var existingTrn = haveExistingValueInState ? await TestData.GenerateTrn() : null;

        await journeyInstance.UpdateStateAsync(state =>
        {
            state.NationalInsuranceNumberSpecified = true;

            if (existingTrn is not null)
            {
                state.Trn = existingTrn;
                state.TrnSpecified = true;
            }
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(existingTrn ?? "", doc.GetElementById("Trn")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_NotAuthenticatedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", oneLoginAuthenticationScheme: "dummy", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        await journeyInstance.UpdateStateAsync(state => state.NationalInsuranceNumberSpecified = true);

        var trn = await TestData.GenerateTrn();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", trn }
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
        var state = new SignInJourneyState(redirectUri: "/", oneLoginAuthenticationScheme: "dummy", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var trn = await TestData.GenerateTrn();

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr, createCoreIdentityVc: false);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", trn }
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
        var state = new SignInJourneyState(redirectUri, oneLoginAuthenticationScheme: "dummy", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
        var trn = person.Trn!;
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: person.PersonId);

        var ticket = CreateOneLoginAuthenticationTicket(
            vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
            sub: oneLoginUser.Subject,
            email: oneLoginUser.Email,
            firstName: person.FirstName,
            lastName: person.LastName,
            dateOfBirth: person.DateOfBirth);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"{redirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_EmptyTrn_RendersError()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", oneLoginAuthenticationScheme: "dummy", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
        var trn = "";
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: null);

        var ticket = CreateOneLoginAuthenticationTicket(
            vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
            sub: oneLoginUser.Subject,
            email: oneLoginUser.Email,
            firstName: person.FirstName,
            lastName: person.LastName,
            dateOfBirth: person.DateOfBirth);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        await journeyInstance.UpdateStateAsync(state => state.NationalInsuranceNumberSpecified = true);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Trn", "Enter your TRN");
    }

    [Fact]
    public async Task Post_InvalidTrn_RendersError()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", oneLoginAuthenticationScheme: "dummy", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
        var trn = "xxx";
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: null);

        var ticket = CreateOneLoginAuthenticationTicket(
            vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
            sub: oneLoginUser.Subject,
            email: oneLoginUser.Email,
            firstName: person.FirstName,
            lastName: person.LastName,
            dateOfBirth: person.DateOfBirth);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        await journeyInstance.UpdateStateAsync(state => state.NationalInsuranceNumberSpecified = true);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Trn", "Your TRN should contain 7 digits");
    }

    [Fact]
    public async Task Post_ValidTrnButLookupFailed_UpdatesStateAndRedirectsToNotFoundPage()
    {
        // Arrange
        var state = new SignInJourneyState(redirectUri: "/", oneLoginAuthenticationScheme: "dummy", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
        var trn = await TestData.GenerateTrn();
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: null);

        var ticket = CreateOneLoginAuthenticationTicket(
            vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
            sub: oneLoginUser.Subject,
            email: oneLoginUser.Email,
            firstName: person.FirstName,
            lastName: person.LastName,
            dateOfBirth: person.DateOfBirth);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        await journeyInstance.UpdateStateAsync(state => state.NationalInsuranceNumberSpecified = true);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/not-found?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        state = journeyInstance.State;
        Assert.Equal(trn, state.Trn);
        Assert.True(state.TrnSpecified);
        Assert.Null(state.AuthenticationTicket);
    }

    [Fact]
    public async Task Post_ValidTrnAndLookupSucceeded_UpdatesStateUpdatesOneLoginUserCompletesAuthenticationAndRedirectsToStateRedirectUri()
    {
        // Arrange
        var redirectUri = "/";
        var state = new SignInJourneyState(redirectUri, oneLoginAuthenticationScheme: "dummy", authenticationProperties: null);
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
        var trn = person.Trn!;
        var oneLoginUser = await TestData.CreateOneLoginUser(personId: null);

        var ticket = CreateOneLoginAuthenticationTicket(
            vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr,
            sub: oneLoginUser.Subject,
            email: oneLoginUser.Email,
            firstName: person.FirstName,
            lastName: person.LastName,
            dateOfBirth: person.DateOfBirth);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        await journeyInstance.UpdateStateAsync(state => state.NationalInsuranceNumberSpecified = true);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"{redirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        state = journeyInstance.State;
        Assert.Equal(trn, state.Trn);
        Assert.True(state.TrnSpecified);
        Assert.NotNull(state.AuthenticationTicket);

        oneLoginUser = await WithDbContext(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
        Assert.Equal(Clock.UtcNow, oneLoginUser.FirstSignIn);
        Assert.Equal(Clock.UtcNow, oneLoginUser.LastSignIn);
        Assert.Equal(person.PersonId, oneLoginUser.PersonId);
    }
}
