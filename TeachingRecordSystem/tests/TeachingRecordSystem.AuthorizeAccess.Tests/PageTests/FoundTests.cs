using System.Diagnostics;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class FoundTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NotAuthenticated_RedirectsToStartOfMatchingJourney()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);
        Debug.Assert(state.AuthenticationTicket is null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/found?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/connect?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/found?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponse(response);
    }

    [Fact]
    public async Task Post_NotAuthenticated_RedirectsToStartOfMatchingJourney()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationAndIdentityVerificationVtr);
        await GetSignInJourneyHelper().OnSignedInWithOneLogin(journeyInstance, ticket);
        Debug.Assert(state.AuthenticationTicket is null);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/found?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/connect?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_ValidRequest_RedirectsToStateRedirectUri()
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/found?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }
}
