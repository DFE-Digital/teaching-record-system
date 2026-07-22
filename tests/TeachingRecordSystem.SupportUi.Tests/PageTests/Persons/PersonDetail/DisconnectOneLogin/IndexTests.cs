using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.DisconnectOneLogin;

public class IndexTests(HostFixture hostFixture) : DisconnectOneLoginTestBase(hostFixture)
{
    [Fact]
    public async Task Post_WithoutReason_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            oneLogin.Subject,
            new DisconnectOneLoginState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            person.PersonId,
            oneLogin.Subject,
            new DisconnectOneLoginState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Reason"] = DisconnectOneLoginReason.AnotherReason.ToString(),
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
            person.PersonId,
            oneLogin.Subject,
            new DisconnectOneLoginState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Reason"] = DisconnectOneLoginReason.AnotherReason.ToString(),
                ["ReasonDetail"] = new string('x', 10000)
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ReasonDetail", "Reason details must be 4000 characters or less");
    }

    [Theory]
    [InlineData(DisconnectOneLoginReason.AnotherReason, "Some reason detail")]
    [InlineData(DisconnectOneLoginReason.NewInformation, null)]
    [InlineData(DisconnectOneLoginReason.ConnectedIncorrectly, null)]
    public async Task Post_ValdidReason_RedirectsToVerifyPage(DisconnectOneLoginReason reason, string? detail)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            oneLogin.Subject,
            new DisconnectOneLoginState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.Contains($"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}/verified", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetail()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            oneLogin.Subject,
            new DisconnectOneLoginState
            {
                DisconnectReason = DisconnectOneLoginReason.NewInformation,
                StayVerified = DisconnectOneLoginStayVerified.Yes
            });

        var pageUrl = $"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}?{journeyInstance.GetUniqueIdQueryParameter()}";

        // Act
        var response = await PostCancelAsync(pageUrl);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }
}
