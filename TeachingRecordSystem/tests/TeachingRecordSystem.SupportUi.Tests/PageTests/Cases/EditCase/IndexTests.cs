namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Cases.EditCase;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
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
    public async Task Get_WithTicketNumberForActiveNameChangeIncident_RendersExpectedContent()
    {
        // Arrange
        SetCurrentUser(TestUsers.Helpdesk);
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId).WithMultipleEvidenceFiles());

        var imageEvidence = createIncidentResult.Evidence.Single(e => e.MimeType == "image/jpeg");
        var pdfEvidence = createIncidentResult.Evidence.Single(e => e.MimeType == "application/pdf");

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

        var firstNameRow = doc.GetElementByTestId("first-name");
        Assert.NotNull(firstNameRow);
        Assert.Equal(createPersonResult.FirstName, firstNameRow.GetElementByTestId("first-name-current")!.TextContent);
        Assert.Equal(createIncidentResult.NewFirstName, firstNameRow.GetElementByTestId("first-name-new")!.TextContent);
        var middleNameRow = doc.GetElementByTestId("middle-name");
        Assert.NotNull(middleNameRow);
        Assert.Equal(createPersonResult.MiddleName, middleNameRow.GetElementByTestId("middle-name-current")!.TextContent);
        Assert.Equal(createIncidentResult.NewMiddleName, middleNameRow.GetElementByTestId("middle-name-new")!.TextContent);
        var lastNameRow = doc.GetElementByTestId("last-name");
        Assert.NotNull(lastNameRow);
        Assert.Equal(createPersonResult.LastName, lastNameRow.GetElementByTestId("last-name-current")!.TextContent);
        Assert.Equal(createIncidentResult.NewLastName, lastNameRow.GetElementByTestId("last-name-new")!.TextContent);

        var imageDocument = doc.GetElementByTestId($"image-{imageEvidence.DocumentId}");
        Assert.NotNull(imageDocument);
        var pdfDocument = doc.GetElementByTestId($"pdf-{pdfEvidence.DocumentId}");
        Assert.NotNull(pdfDocument);
    }

    [Fact]
    public async Task Get_WithTicketNumberForActiveDateOfBirthChangeIncident_RendersExpectedContent()
    {
        // Arrange
        SetCurrentUser(TestUsers.Helpdesk);
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));

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

        var dateOfBirthRow = doc.GetElementByTestId("date-of-birth");
        Assert.NotNull(dateOfBirthRow);
        Assert.Equal(createPersonResult.DateOfBirth.ToString("dd/MM/yyyy"), dateOfBirthRow.GetElementByTestId("date-of-birth-current")!.TextContent);
        Assert.Equal(createIncidentResult.NewDateOfBirth.ToString("dd/MM/yyyy"), dateOfBirthRow.GetElementByTestId("date-of-birth-new")!.TextContent);

        var imageDocument = doc.GetElementByTestId($"image-{createIncidentResult.Evidence.DocumentId}");
        Assert.NotNull(imageDocument);
    }
}
