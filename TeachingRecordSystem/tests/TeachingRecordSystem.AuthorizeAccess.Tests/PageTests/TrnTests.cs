using System.Diagnostics;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class TrnTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = CreateNewState();
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
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, createCoreIdentityVc: false);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

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
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(person);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_NationalInsuranceNumberNotSpecified_RedirectsToNationalInsuranceNumberPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        Debug.Assert(state.NationalInsuranceNumber is null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
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

        var existingTrn = haveExistingValueInState ? await TestData.GenerateTrn() : null;

        await journeyInstance.UpdateStateAsync(state =>
        {
            state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber());

            if (existingTrn is not null)
            {
                state.SetTrn(true, existingTrn);
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
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber()));

        var trn = await TestData.GenerateTrn();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveTrn", bool.TrueString },
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
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var trn = await TestData.GenerateTrn();

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, createCoreIdentityVc: false);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveTrn", bool.TrueString },
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
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(person);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var trn = person.Trn!;

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveTrn", bool.TrueString },
                { "Trn", trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_NationalInsuranceNumberNotSpecified_RedirectsToNationalInsuranceNumberPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var trn = await TestData.GenerateTrn();

        Debug.Assert(state.NationalInsuranceNumber is null);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveTrn", bool.TrueString },
                { "Trn", trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_HaveTrnNotAnswered_RendersError()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber()));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "HaveTrn", "Select yes if you have a teacher reference number");
    }

    [Fact]
    public async Task Post_EmptyTrn_RendersError()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var trn = "";

        await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber()));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveTrn", bool.TrueString },
                { "Trn", trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Trn", "Enter your teacher reference number");
    }

    [Fact]
    public async Task Post_InvalidTrn_RendersError()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var trn = "xxx";

        await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber()));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveTrn", bool.TrueString },
                { "Trn", trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Trn", "Your teacher reference number should contain 7 digits");
    }

    [Fact]
    public async Task Post_NoTrnSpecified_UpdatesStateAndRedirectsToNotFoundPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var trn = await TestData.GenerateTrn();

        await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber()));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveTrn", bool.FalseString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/not-found?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        state = journeyInstance.State;
        Assert.False(state.HaveTrn);
        Assert.Null(state.Trn);
        Assert.Null(state.AuthenticationTicket);
    }

    [Fact]
    public async Task Post_ValidTrnButLookupFailed_UpdatesStateAndRedirectsToNotFoundPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var trn = await TestData.GenerateTrn();

        await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber()));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveTrn", bool.TrueString },
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
        Assert.True(state.HaveTrn);
        Assert.Equal(trn, state.Trn);
        Assert.Null(state.AuthenticationTicket);
    }

    [Fact]
    public async Task Post_ValidTrnAndLookupSucceeded_UpdatesStateUpdatesOneLoginUserCompletesAuthenticationAndRedirectsToFoundPage()
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

        var trn = person.Trn!;

        await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber()));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/trn?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HaveTrn", bool.TrueString },
                { "Trn", trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/found?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        state = journeyInstance.State;
        Assert.True(state.HaveTrn);
        Assert.Equal(trn, state.Trn);
        Assert.NotNull(state.AuthenticationTicket);

        oneLoginUser = await WithDbContext(dbContext => dbContext.OneLoginUsers.SingleAsync(u => u.Subject == oneLoginUser.Subject));
        Assert.Equal(Clock.UtcNow, oneLoginUser.FirstSignIn);
        Assert.Equal(Clock.UtcNow, oneLoginUser.LastSignIn);
        Assert.Equal(person.PersonId, oneLoginUser.PersonId);
    }
}
