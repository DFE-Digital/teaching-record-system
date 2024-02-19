using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
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

    [Fact]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var sanctionCode = "G1";
        var sanctionCodeId = (await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCode)).dfeta_sanctioncodeId;
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var alertTypeIdElement = doc.GetElementById("AlertTypeId") as IHtmlSelectElement;
        Assert.NotNull(alertTypeIdElement);
        Assert.Equal(sanctionCodeId!.Value.ToString(), alertTypeIdElement.Value);
        Assert.Equal(details, doc.GetElementById("Details")!.TextContent);
        Assert.Equal(link, doc.GetElementById("Link")!.GetAttribute("value"));
        Assert.Equal($"{startDate:%d}", doc.GetElementById("StartDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{startDate:%M}", doc.GetElementById("StartDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{startDate:yyyy}", doc.GetElementById("StartDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_EmptyRequiredFields_ReturnsError()
    {
        // Arrange        
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.ContactId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "AlertTypeId", "Add an alert type");
        await AssertEx.HtmlResponseHasError(response, "Details", "Add details");
        await AssertEx.HtmlResponseHasError(response, "StartDate", "Add a start date");
    }

    [Fact]
    public async Task Post_InvalidLinkUrl_ReturnsError()
    {
        // Arrange
        var sanctionCode = "G1";
        var sanctionCodeId = (await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCode)).dfeta_sanctioncodeId;
        var details = "These are some test details";
        var link = "invalid-url";
        var startDate = new DateOnly(2021, 01, 01);
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.ContactId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "AlertTypeId", sanctionCodeId!.Value.ToString() },
                { "Details", details },
                { "Link", link },
                { "StartDate.Day", startDate.Day.ToString() },
                { "StartDate.Month", startDate.Month.ToString() },
                { "StartDate.Year", startDate.Year.ToString() },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Link", "Enter a valid URL");
    }

    [Fact]
    public async Task Post_ValidData_RedirectsToConfirmPage()
    {
        // Arrange
        var sanctionCode = "G1";
        var sanctionCodeId = (await TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCode)).dfeta_sanctioncodeId;
        var details = "These are some test details";
        var link = "http://www.gov.uk";
        var startDate = new DateOnly(2021, 01, 01);
        var person = await TestData.CreatePerson();

        var journeyInstance = await CreateJourneyInstance(person.ContactId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "AlertTypeId", sanctionCodeId!.Value.ToString() },
                { "Details", details },
                { "Link", link },
                { "StartDate.Day", startDate.Day.ToString() },
                { "StartDate.Month", startDate.Month.ToString() },
                { "StartDate.Year", startDate.Year.ToString() },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/alerts/add/confirm?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    private async Task<JourneyInstance<AddAlertState>> CreateJourneyInstance(Guid personId, AddAlertState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.AddAlert,
            state ?? new AddAlertState(),
            new KeyValuePair<string, object>("personId", personId));
}
