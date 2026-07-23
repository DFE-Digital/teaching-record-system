using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins.OneLoginDetail.DisconnectPerson;

public class VerifiedTests(HostFixture hostFixture) : DisconnectPersonTestBase(hostFixture)
{
    [Fact]
    public async Task Post_WithoutOption_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            person.PersonId,
            new DisconnectPersonState()
            {
                DisconnectReason = DisconnectPersonReason.NewInformation
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}/verified?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["StayVerified"] = ""
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StayVerified", "Select yes if you want to keep the GOV.UK One Login verified");
    }

    [Theory]
    [InlineData(DisconnectPersonStayVerified.Yes)]
    [InlineData(DisconnectPersonStayVerified.No)]
    public async Task Post_ValdidOption_RedirectsToCheckAnswersPage(DisconnectPersonStayVerified stayVerified)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            person.PersonId,
            new DisconnectPersonState() { DisconnectReason = DisconnectPersonReason.NewInformation, StayVerified = DisconnectPersonStayVerified.No });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}/verified?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.Contains($"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}/check-answers", response.Headers.Location?.OriginalString);
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

        var pageUrl = $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}/verified?{journeyInstance.GetUniqueIdQueryParameter()}";

        // Act
        var response = await PostCancelAsync(pageUrl);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/one-logins/{oneLogin.Subject}", response.Headers.Location?.OriginalString);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }
}
