using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.DisconnectOneLogin;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Post_WithoutReason_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
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

    private Task<JourneyInstance<DisconnectOneLoginState>> CreateJourneyInstanceAsync(Guid personId, DisconnectOneLoginState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.DisconnectOneLogin,
            state ?? new DisconnectOneLoginState(),
            new KeyValuePair<string, object>("personId", personId));
}
