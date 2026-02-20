using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.DisconnectOneLogin;

public class VerifyTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Post_WithoutOption_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new DisconnectOneLoginState()
            {
                DisconnectReason = DisconnectOneLoginReason.NewInformation
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}/verified?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["StayVerified"] = ""
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StayVerified", "Select yes if you want to keep the person verified");
    }

    [Theory]
    [InlineData(DisconnectOneLoginStayVerified.Yes)]
    [InlineData(DisconnectOneLoginStayVerified.No)]
    public async Task Post_ValdidOption_RedirectsToCheckAnswersPage(DisconnectOneLoginStayVerified stayVerified)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new DisconnectOneLoginState() { DisconnectReason = DisconnectOneLoginReason.NewInformation, StayVerified = DisconnectOneLoginStayVerified.No });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}/verified?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                ["StayVerified"] = stayVerified.ToString()
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Contains($"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}/check-answers", response.Headers.Location?.OriginalString);
    }

    private Task<JourneyInstance<DisconnectOneLoginState>> CreateJourneyInstanceAsync(Guid personId, DisconnectOneLoginState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.DisconnectOneLogin,
            state ?? new DisconnectOneLoginState(),
            new KeyValuePair<string, object>("personId", personId));
}
