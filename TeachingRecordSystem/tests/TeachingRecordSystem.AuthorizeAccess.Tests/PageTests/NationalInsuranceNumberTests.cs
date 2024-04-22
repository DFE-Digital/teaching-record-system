namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class NationalInsuranceNumberTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = CreateNewState();
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
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, createCoreIdentityVc: false);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

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
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(person);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_ValidRequest_RendersExpectedContent(bool haveExistingValueInState)
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var existingNationalInsuranceNumber = haveExistingValueInState ? Faker.Identification.UkNationalInsuranceNumber() : null;
        if (existingNationalInsuranceNumber is not null)
        {
            await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, existingNationalInsuranceNumber));
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(existingNationalInsuranceNumber ?? "", doc.GetElementById("NationalInsuranceNumber")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_NotAuthenticatedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = CreateNewState();
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
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, createCoreIdentityVc: false);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

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
    public async Task Post_AlreadyAuthenticated_RedirectsToStateRedirectUri()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(person);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var nationalInsuranceNumber = person.NationalInsuranceNumber!;

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
        Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HaveNationalInsuranceNumberNotAnswered_RendersError()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "HaveNationalInsuranceNumber", "Select yes if you have a National Insurance number");
    }

    [Fact]
    public async Task Post_EmptyNationalInsuranceNumber_RendersError()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var nationalInsuranceNumber = "";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveNationalInsuranceNumber", bool.TrueString },
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
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var nationalInsuranceNumber = "xxx";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveNationalInsuranceNumber", bool.TrueString },
                { "NationalInsuranceNumber", nationalInsuranceNumber }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "NationalInsuranceNumber", "Enter a National Insurance number in the correct format");
    }

    [Fact]
    public async Task Post_HaveNationalInsuranceNumberNotSpecified_UpdatesStateAndRedirectsToTrnPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveNationalInsuranceNumber", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/trn?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        state = journeyInstance.State;
        Assert.False(state.HaveNationalInsuranceNumber);
        Assert.Null(state.AuthenticationTicket);
    }

    [Fact]
    public async Task Post_ValidNationalInsuranceNumberButLookupFailed_UpdatesStateAndRedirectsToTrnPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveNationalInsuranceNumber", bool.TrueString },
                { "NationalInsuranceNumber", nationalInsuranceNumber }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/trn?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        state = journeyInstance.State;
        Assert.True(state.HaveNationalInsuranceNumber);
        Assert.Equal(nationalInsuranceNumber, state.NationalInsuranceNumber);
        Assert.Null(state.AuthenticationTicket);
    }

    [Fact]
    public async Task Post_ValidNationalInsuranceNumberAndLookupSucceeded_UpdatesStateUpdatesOneLoginUserCompletesAuthenticationAndRedirectsToFoundPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn().WithNationalInsuranceNumber());
        var oneLoginUser = await TestData.CreateOneLoginUser(
            personId: null,
            verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var nationalInsuranceNumber = person.NationalInsuranceNumber!;

        var request = new HttpRequestMessage(HttpMethod.Post, $"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveNationalInsuranceNumber", bool.TrueString },
                { "NationalInsuranceNumber", nationalInsuranceNumber }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/found?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        state = journeyInstance.State;
        Assert.True(state.HaveNationalInsuranceNumber);
        Assert.Equal(nationalInsuranceNumber, state.NationalInsuranceNumber);
        Assert.NotNull(state.AuthenticationTicket);

        oneLoginUser = await WithDbContext(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
        Assert.Equal(Clock.UtcNow, oneLoginUser.FirstSignIn);
        Assert.Equal(Clock.UtcNow, oneLoginUser.LastSignIn);
        Assert.Equal(person.PersonId, oneLoginUser.PersonId);
    }
}
