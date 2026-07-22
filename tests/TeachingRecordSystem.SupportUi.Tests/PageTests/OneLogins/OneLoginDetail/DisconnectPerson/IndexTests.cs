using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins.OneLoginDetail.DisconnectPerson;

public class IndexTests(HostFixture hostFixture) : DisconnectPersonTestBase(hostFixture)
{
    [Fact]
    public async Task Post_WithoutReason_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            person.PersonId,
            new DisconnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Reason"] = ""
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Reason", "Select a reason");
    }

    [Fact]
    public async Task Post_WithAnotherReasonWithoutDetail_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            person.PersonId,
            new DisconnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Reason"] = DisconnectPersonReason.AnotherReason.ToString(),
                ["ReasonDetail"] = ""
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ReasonDetail", "Enter a reason");
    }

    [Fact]
    public async Task Post_WithReasonDetailExceedingMaxLength_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            person.PersonId,
            new DisconnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Reason"] = DisconnectPersonReason.AnotherReason.ToString(),
                ["ReasonDetail"] = new string('x', 10000)
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ReasonDetail", "Reason details must be 4000 characters or less");
    }

    [Theory]
    [InlineData(DisconnectPersonReason.AnotherReason, "Some reason detail")]
    [InlineData(DisconnectPersonReason.NewInformation, null)]
    [InlineData(DisconnectPersonReason.ConnectedIncorrectly, null)]
    public async Task Post_ValdidReason_RedirectsToVerifyPage(DisconnectPersonReason reason, string? detail)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            person.PersonId,
            new DisconnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                ["Reason"] = reason.ToString(),
                ["ReasonDetail"] = detail
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Contains($"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}/verified", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToOneLoginDetail()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            person.PersonId,
            new DisconnectPersonState
            {
                DisconnectReason = DisconnectPersonReason.NewInformation,
                StayVerified = DisconnectPersonStayVerified.Yes
            });

        var pageUrl = $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}?{journeyInstance.GetUniqueIdQueryParameter()}";

        // Act
        var response = await PostCancelAsync(pageUrl);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/one-logins/{oneLogin.Subject}", response.Headers.Location?.OriginalString);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }
}
