using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.DisconnectOneLogin;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.DisconnectOneLogin;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(null, null)]
    [InlineData(DisconnectOneLoginStayVerified.Yes, null)]
    [InlineData(null, DisconnectOneLoginReason.NewInformation)]
    public async Task Get_WithMissingOptions_ReturnsToIndex(DisconnectOneLoginStayVerified? stayVerified,
        DisconnectOneLoginReason? reason)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new DisconnectOneLoginState() { DisconnectReason = reason, StayVerified = stayVerified });

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Contains($"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(DisconnectOneLoginStayVerified.Yes, DisconnectOneLoginReason.NewInformation, null)]
    public async Task Get_RendersCorrectDetails(DisconnectOneLoginStayVerified? stayVerified,
        DisconnectOneLoginReason? reason, string? details)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new DisconnectOneLoginState() { DisconnectReason = reason, StayVerified = stayVerified, Detail = details });

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        //doc.AssertSummaryListRowValue("GOV.UK One Login email address", v => Assert.Equal(oneLogin.Subject, v.TrimmedText()));
        doc.AssertSummaryListRowValue("Why are you disconnecting GOV.UK One Login from this record?", v => Assert.Contains(reason!.GetDisplayName()!, v.TrimmedText()));
        doc.AssertSummaryListRowValue("Should this One Login User remain verified?", v => Assert.Contains(stayVerified!.GetDisplayName()!, v.TrimmedText()));
    }

    [Theory]
    [InlineData(DisconnectOneLoginStayVerified.Yes, DisconnectOneLoginReason.AnotherReason, "this is a test record")]
    public async Task Get_RendersOtherReasonDetails(DisconnectOneLoginStayVerified? stayVerified,
        DisconnectOneLoginReason? reason, string? details)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new DisconnectOneLoginState() { DisconnectReason = reason, StayVerified = stayVerified, Detail = details });

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/persons/{person.PersonId}/disconnect-one-login/{oneLogin.Subject}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        //doc.AssertSummaryListRowValue("GOV.UK One Login email address", v => Assert.Equal(oneLogin.Subject, v.TrimmedText()));
        doc.AssertSummaryListRowValue("Why are you disconnecting GOV.UK One Login from this record?", v => Assert.Contains(reason!.GetDisplayName()!, v.TrimmedText()));
        doc.AssertSummaryListRowValue("Should this One Login User remain verified?", v => Assert.Contains(stayVerified!.GetDisplayName()!, v.TrimmedText()));
        var otherReasonDetails = doc.GetElementByTestId("disconnect-reason-detail");
        Assert.NotNull(otherReasonDetails);
        Assert.Equal(details, otherReasonDetails!.TextContent!.Trim());
    }

    private Task<JourneyInstance<DisconnectOneLoginState>> CreateJourneyInstanceAsync(Guid personId,
        DisconnectOneLoginState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.DisconnectOneLogin,
            state ?? new DisconnectOneLoginState(),
            new KeyValuePair<string, object>("personId", personId));
}
