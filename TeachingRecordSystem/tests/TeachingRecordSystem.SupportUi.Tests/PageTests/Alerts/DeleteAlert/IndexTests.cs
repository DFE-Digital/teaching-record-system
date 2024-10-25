using TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.DeleteAlert;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite, UserRoles.DbsAlertsReadWrite));
    }

    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alert = person.Alerts.Single();

        var journeyInstance = await CreateJourneyInstance(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateJourneyInstance(
            alert.AlertId,
            new DeleteAlertState
            {
                ConfirmDelete = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var radioButtons = doc.GetElementsByName("ConfirmDelete");
        var selectedRadioButton = radioButtons.Single(r => r.HasAttribute("checked"));
        Assert.Equal("True", selectedRadioButton.GetAttribute("value"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_ValidRequest_RendersPageAsExpected(bool populateOptional)
    {
        // Arrange
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct
        var startDate = TestData.Clock.Today.AddDays(-50);
        var details = "Some details";
        var link = populateOptional ? TestData.GenerateUrl() : null;
        var endDate = populateOptional ? TestData.Clock.Today.AddDays(-5) : (DateOnly?)null;
        var person = await TestData.CreatePerson(
            b => b.WithAlert(
                a => a.WithAlertTypeId(alertType.AlertTypeId)
                    .WithDetails(details)
                    .WithExternalLink(link)
                    .WithStartDate(startDate)
                    .WithEndDate(endDate)));
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateJourneyInstance(
            alert.AlertId,
            new DeleteAlertState
            {
                ConfirmDelete = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(alertType.Name, doc.GetElementByTestId("alert-type")!.TextContent);
        Assert.Equal(details, doc.GetElementByTestId("details")!.TextContent);
        Assert.Equal(populateOptional ? $"{link} (opens in new tab)" : "-", doc.GetElementByTestId("link")!.TextContent);
        Assert.Equal(startDate.ToString("d MMMM yyyy"), doc.GetElementByTestId("start-date")!.TextContent);
        Assert.Equal(populateOptional ? endDate?.ToString("d MMMM yyyy") : "-", doc.GetElementByTestId("end-date")!.TextContent);
    }

    [Fact]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var person = await TestData.CreatePerson(b => b.WithAlert());
        var alert = person.Alerts.Single();

        var journeyInstance = await CreateJourneyInstance(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "ConfirmDelete", bool.TrueString }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoConfirmDeleteOptionIsSelected_ReturnsError()
    {
        // Arrange
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct
        var startDate = TestData.Clock.Today.AddDays(-50);
        var details = "Some details";
        var link = TestData.GenerateUrl();
        var endDate = TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(
            b => b.WithAlert(
                a => a.WithAlertTypeId(alertType.AlertTypeId)
                    .WithDetails(details)
                    .WithExternalLink(link)
                    .WithStartDate(startDate)
                    .WithEndDate(endDate)));
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateJourneyInstance(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "ConfirmDelete", "Confirm you want to delete this alert");
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task Post_ValidInput_RedirectsToAppropriatePage(bool confirmDelete, bool isActive)
    {
        // Arrange
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct
        var startDate = TestData.Clock.Today.AddDays(-50);
        var details = "Some details";
        var link = TestData.GenerateUrl();
        var endDate = isActive ? (DateOnly?)null : TestData.Clock.Today.AddDays(-5);
        var person = await TestData.CreatePerson(
            b => b.WithAlert(
                a => a.WithAlertTypeId(alertType.AlertTypeId)
                    .WithDetails(details)
                    .WithExternalLink(link)
                    .WithStartDate(startDate)
                    .WithEndDate(endDate)));
        var alert = person.Alerts.Single();
        var journeyInstance = await CreateJourneyInstance(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["ConfirmDelete"] = confirmDelete ? "True" : "False"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        if (confirmDelete)
        {
            Assert.StartsWith($"/alerts/{alert.AlertId}/delete/confirm", response.Headers.Location!.OriginalString);
        }
        else
        {
            if (isActive)
            {
                Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location!.OriginalString);
            }
            else
            {
                Assert.StartsWith($"/alerts/{alert.AlertId}", response.Headers.Location!.OriginalString);
            }
        }
    }

    private async Task<JourneyInstance<DeleteAlertState>> CreateJourneyInstance(Guid alertId, DeleteAlertState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.DeleteAlert,
            state ?? new DeleteAlertState(),
            new KeyValuePair<string, object>("alertId", alertId));
}
