namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Cases.EditCase;

public class RejectTests : TestBase
{
    public RejectTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WhenUserHasNoRoles_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/cases/{createIncidentResult.TicketNumber}/reject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WhenUserDoesNotHaveHelpdeskOrAdministratorRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.UnusedRole);
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/cases/{createIncidentResult.TicketNumber}/reject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithTicketNumberForNonExistentIncident_ReturnsNotFound()
    {
        // Arrange
        SetCurrentUser(TestUsers.Helpdesk);
        var nonExistentTicketNumber = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/cases/{nonExistentTicketNumber}/reject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithTicketNumberForInactiveIncident_ReturnsBadRequest()
    {
        // Arrange
        SetCurrentUser(TestUsers.Helpdesk);
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId).WithCanceledStatus());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/cases/{createIncidentResult.TicketNumber}/reject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenRejectionReasonChoiceHasNoSelection_ReturnsError()
    {
        // Arrange
        SetCurrentUser(TestUsers.Helpdesk);
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/cases/{createIncidentResult.TicketNumber}/reject")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "RejectionReasonChoice", "Select the reason for rejecting this change");
    }

    [Fact]
    public async Task Post_WhenUserDoesNotHaveHelpdeskOrAdministratorRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.UnusedRole);
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/cases/{createIncidentResult.TicketNumber}/reject")
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
        SetCurrentUser(TestUsers.Helpdesk);
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/cases/{createIncidentResult.TicketNumber}/reject")
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

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "The request has been rejected");
    }

    [Fact]
    public async Task Post_WhenRejectionReasonChoiceIsChangeNoLongerRequired_RedirectsWithFlashMessage()
    {
        // Arrange
        SetCurrentUser(TestUsers.Helpdesk);
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/cases/{createIncidentResult.TicketNumber}/reject")
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

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "The request has been cancelled");
    }
}
