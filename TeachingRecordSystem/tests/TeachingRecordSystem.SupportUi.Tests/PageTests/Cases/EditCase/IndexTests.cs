using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Cases.EditCase;

[Collection(nameof(DisableParallelization))]
public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        XrmFakedContext.DeleteAllEntities<Incident>();
    }

    [Fact]
    public async Task Get_UserWithNoRoles_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/cases/{createIncidentResult.TicketNumber}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_UserWithoutHelpdeskOrAdministratorRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.UnusedRole);
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/cases/{createIncidentResult.TicketNumber}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/cases/{nonExistentTicketNumber}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/cases/{createIncidentResult.TicketNumber}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithTicketNumberForActiveIncident_RendersExpectedContent()
    {
        // Arrange
        SetCurrentUser(TestUsers.Helpdesk);
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/cases/{createIncidentResult.TicketNumber}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        Assert.Equal(createIncidentResult.SubjectTitle, doc.GetElementByTestId("page-title")!.TextContent);
        var headerRow = doc.GetElementByTestId("case-header");
        Assert.NotNull(headerRow);
        Assert.Equal(createIncidentResult.TicketNumber, headerRow.GetElementByTestId("case-header-case-reference")!.TextContent);
        Assert.Equal($"{createPersonResult.FirstName} {createPersonResult.LastName}", headerRow.GetElementByTestId("case-header-name")!.TextContent);
        Assert.Equal(createIncidentResult.CreatedOn.ToString("dd/MM/yyyy"), headerRow.GetElementByTestId("case-header-created-on")!.TextContent);
    }
}
