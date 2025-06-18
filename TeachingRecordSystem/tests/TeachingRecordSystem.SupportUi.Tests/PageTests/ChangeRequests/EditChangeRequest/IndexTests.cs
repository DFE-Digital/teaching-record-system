namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ChangeRequests.EditChangeRequest;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.RecordManager));
    }

    [Fact]
    public async Task Get_UserWithNoRoles_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));
        var createPersonResult = await TestData.CreatePersonAsync();
        var createIncidentResult = await TestData.CreateNameChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{createIncidentResult.TicketNumber}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [RoleNamesData(except: [UserRoles.RecordManager, UserRoles.AccessManager, UserRoles.Administrator])]
    public async Task Get_UserWithoutSupportOfficerOrAccessManagerOrAdministratorRole_ReturnsForbidden(string role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));
        var createPersonResult = await TestData.CreatePersonAsync();
        var createIncidentResult = await TestData.CreateNameChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{createIncidentResult.TicketNumber}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{nonExistentTicketNumber}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{createIncidentResult.TicketNumber}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public async Task Get_WithTicketNumberForActiveNameChangeIncident_RendersExpectedContent(bool hasNewFirstName, bool hasNewMiddleName, bool hasNewLastName)
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var createIncidentResult = await TestData.CreateNameChangeIncidentAsync(
            b => b.WithCustomerId(createPersonResult.ContactId)
                .WithNewFirstName(hasNewFirstName ? TestData.GenerateChangedFirstName(createPersonResult.FirstName) : createPersonResult.FirstName)
                .WithNewMiddleName(hasNewMiddleName ? TestData.GenerateChangedMiddleName(createPersonResult.MiddleName) : createPersonResult.MiddleName)
                .WithNewLastName(hasNewLastName ? TestData.GenerateChangedLastName(createPersonResult.LastName) : createPersonResult.LastName)
                .WithMultipleEvidenceFiles());

        var imageEvidence = createIncidentResult.Evidence.Single(e => e.MimeType == "image/jpeg");
        var pdfEvidence = createIncidentResult.Evidence.Single(e => e.MimeType == "application/pdf");

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{createIncidentResult.TicketNumber}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal($"{createIncidentResult.SubjectTitle} - {createPersonResult.FirstName} {createPersonResult.LastName}", doc.GetElementByTestId("heading-caption")!.TrimmedText());

        var firstNameRow = doc.GetElementByTestId("first-name");
        if (hasNewFirstName)
        {
            Assert.NotNull(firstNameRow);
            Assert.Equal(createPersonResult.FirstName, firstNameRow.GetElementByTestId("first-name-current")!.TrimmedText());
            Assert.Equal(createIncidentResult.NewFirstName, firstNameRow.GetElementByTestId("first-name-new")!.TrimmedText());
        }
        else
        {
            Assert.Null(firstNameRow);
        }

        var middleNameRow = doc.GetElementByTestId("middle-name");
        if (hasNewMiddleName)
        {
            Assert.NotNull(middleNameRow);
            Assert.Equal(createPersonResult.MiddleName, middleNameRow.GetElementByTestId("middle-name-current")!.TrimmedText());
            Assert.Equal(createIncidentResult.NewMiddleName, middleNameRow.GetElementByTestId("middle-name-new")!.TrimmedText());
        }
        else
        {
            Assert.Null(middleNameRow);
        }

        var lastNameRow = doc.GetElementByTestId("last-name");
        if (hasNewLastName)
        {
            Assert.NotNull(lastNameRow);
            Assert.Equal(createPersonResult.LastName, lastNameRow.GetElementByTestId("last-name-current")!.TrimmedText());
            Assert.Equal(createIncidentResult.NewLastName, lastNameRow.GetElementByTestId("last-name-new")!.TrimmedText());
        }
        else
        {
            Assert.Null(lastNameRow);
        }

        var imageDocument = doc.GetElementByTestId($"image-{imageEvidence.DocumentId}");
        Assert.NotNull(imageDocument);
        var pdfDocument = doc.GetElementByTestId($"pdf-{pdfEvidence.DocumentId}");
        Assert.NotNull(pdfDocument);
    }

    [Fact]
    public async Task Get_WithTicketNumberForActiveDateOfBirthChangeIncident_RendersExpectedContent()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var createIncidentResult = await TestData.CreateDateOfBirthChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/change-requests/{createIncidentResult.TicketNumber}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal($"{createIncidentResult.SubjectTitle} - {createPersonResult.FirstName} {createPersonResult.LastName}", doc.GetElementByTestId("heading-caption")!.TrimmedText());

        var dateOfBirthRow = doc.GetElementByTestId("date-of-birth");
        Assert.NotNull(dateOfBirthRow);
        Assert.Equal(createPersonResult.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), dateOfBirthRow.GetElementByTestId("date-of-birth-current")!.TrimmedText());
        Assert.Equal(createIncidentResult.NewDateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), dateOfBirthRow.GetElementByTestId("date-of-birth-new")!.TrimmedText());

        var imageDocument = doc.GetElementByTestId($"image-{createIncidentResult.Evidence.DocumentId}");
        Assert.NotNull(imageDocument);
    }
}
