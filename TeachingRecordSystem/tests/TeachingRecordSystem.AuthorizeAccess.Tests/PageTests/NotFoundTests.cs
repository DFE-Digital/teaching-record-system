using System.Diagnostics;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class NotFoundTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = CreateNewState();
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
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr, createCoreIdentityVc: false);
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
        var state = CreateNewState();
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/not-found?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        Debug.Assert(state.NationalInsuranceNumber is null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/not-found?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_TrnNotSpecified_RedirectsToTrnPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        Debug.Assert(state.NationalInsuranceNumber is null);
        await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber()));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/not-found?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/trn?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);

        await journeyInstance.UpdateStateAsync(async state =>
        {
            state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber());
            state.SetTrn(true, await TestData.GenerateTrn());
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/not-found?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
    }
}
