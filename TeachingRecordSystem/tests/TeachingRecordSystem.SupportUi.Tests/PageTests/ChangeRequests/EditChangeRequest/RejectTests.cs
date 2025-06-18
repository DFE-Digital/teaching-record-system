namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ChangeRequests.EditChangeRequest;

public class RejectTests : TestBase
{
    public RejectTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.RecordManager));
    }

    [Fact]
    public async Task Get_WhenUserHasNoRoles_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));
        var createPersonResult = await TestData.CreatePersonAsync();
        var createIncidentResult = await TestData.CreateNameChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{createIncidentResult.TicketNumber}/reject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [RoleNamesData(except: [UserRoles.RecordManager, UserRoles.AccessManager, UserRoles.Administrator])]
    public async Task Get_WhenUserDoesNotHaveSupportOfficerOrAccessManagerOrAdministratorRole_ReturnsForbidden(string role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));
        var createPersonResult = await TestData.CreatePersonAsync();
        var createIncidentResult = await TestData.CreateNameChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{createIncidentResult.TicketNumber}/reject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithTicketNumberForNonExistentIncident_ReturnsNotFound()
    {
        // Arrange
        var nonExistentTicketNumber = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{nonExistentTicketNumber}/reject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithTicketNumberForInactiveIncident_ReturnsBadRequest()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var createIncidentResult = await TestData.CreateNameChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId).WithCanceledStatus());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{createIncidentResult.TicketNumber}/reject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenRejectionReasonChoiceHasNoSelection_ReturnsError()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var createIncidentResult = await TestData.CreateDateOfBirthChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/change-requests/{createIncidentResult.TicketNumber}/reject")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "RejectionReasonChoice", "Select the reason for rejecting this change");
    }

    [Theory]
    [RoleNamesData(except: [UserRoles.RecordManager, UserRoles.AccessManager, UserRoles.Administrator])]
    public async Task Post_WhenUserDoesNotHaveSupportOfficerOrAccessManagerOrAdministratorRole_ReturnsForbidden(string role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));
        var createPersonResult = await TestData.CreatePersonAsync();
        var createIncidentResult = await TestData.CreateDateOfBirthChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/change-requests/{createIncidentResult.TicketNumber}/reject")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "RejectionReasonChoice", "RequestAndProofDontMatch" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenRejectionReasonChoiceIsNotChangeNoLongerRequired_RedirectsWithFlashMessage()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var createIncidentResult = await TestData.CreateDateOfBirthChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/change-requests/{createIncidentResult.TicketNumber}/reject")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "RejectionReasonChoice", "RequestAndProofDontMatch" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "The request has been rejected");
    }

    [Fact]
    public async Task Post_WhenRejectionReasonChoiceIsChangeNoLongerRequired_RedirectsWithFlashMessage()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var createIncidentResult = await TestData.CreateDateOfBirthChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/change-requests/{createIncidentResult.TicketNumber}/reject")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "RejectionReasonChoice", "ChangeNoLongerRequired" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "The request has been cancelled");
    }
}
