using FormFlow;
using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class ConfirmTests : TestBase
{
    public ConfirmTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange        
        var personId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstance(personId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("G1", null, null)]
    [InlineData(null, "These are some test details", null)]
    [InlineData(null, null, "2021-01-01")]
    public async Task Get_StateHasMissingRequiredData_RedirectsToIndex(string? sanctionCode, string? details, string? startDateString)
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var addAlertState = new AddAlertState();
        if (sanctionCode is not null)
        {
            var sanctionCodeId = (await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCode)).dfeta_sanctioncodeId;
            addAlertState.AlertTypeId = sanctionCodeId;
        }

        if (details is not null)
        {
            addAlertState.Details = details;
        }

        if (startDateString is not null)
        {
            addAlertState.StartDate = DateOnly.Parse(startDateString);
        }

        var journeyInstance = await CreateJourneyInstance(
            person.ContactId,
            addAlertState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/confirm?personId={person.ContactId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/alerts/add?personId={person.ContactId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var sanctionCodeValue = "G1";
        var sanctionCode = await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCodeValue);
        var sanctionCodeId = sanctionCode.dfeta_sanctioncodeId;
        var sanctionCodeName = sanctionCode.dfeta_name;
        var details = "These are some test details";
        var link = "http://www.gov.uk";
        var startDate = new DateOnly(2021, 01, 01);
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(
            person.ContactId,
            new AddAlertState()
            {
                AlertTypeId = sanctionCodeId,
                Details = details,
                Link = link,
                StartDate = startDate,
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/confirm?personId={person.ContactId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        Assert.Equal(sanctionCodeName, doc.GetElementByTestId("alert-type")!.TextContent);
        var detailsElement = doc.GetElementByTestId("details");
        Assert.NotNull(detailsElement);
        Assert.Equal(details, doc.GetElementByTestId("details")!.TextContent);
        Assert.Equal(link, doc.GetElementByTestId("link")!.TextContent);
        Assert.Equal(startDate.ToString("dd/MM/yyyy"), doc.GetElementByTestId("start-date")!.TextContent);
    }

    [Fact]
    public async Task Post_ValidRequest_AddsAlertAndCompletesJourney()
    {
        // Arrange
        var sanctionCodeValue = "G1";
        var sanctionCode = await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCodeValue);
        var sanctionCodeId = sanctionCode.dfeta_sanctioncodeId;
        var sanctionCodeName = sanctionCode.dfeta_name;
        var details = "These are some test details";
        var link = "http://www.gov.uk";
        var startDate = new DateOnly(2021, 01, 01);
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(
            person.ContactId,
            new AddAlertState()
            {
                AlertTypeId = sanctionCodeId,
                Details = details,
                Link = link,
                StartDate = startDate,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/confirm?personId={person.ContactId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Alert added");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private async Task<JourneyInstance<AddAlertState>> CreateJourneyInstance(Guid personId, AddAlertState? state = null) =>
    await CreateJourneyInstance(
        JourneyNames.AddAlert,
        state ?? new AddAlertState(),
        new KeyValuePair<string, object>("personId", personId));
}
