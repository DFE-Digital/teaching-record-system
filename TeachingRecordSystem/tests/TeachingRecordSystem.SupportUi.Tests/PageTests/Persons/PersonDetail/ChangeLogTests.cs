namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_PersonDoesNotExist_ReturnsNotFound()
    {
        // Arrange        
        var nonExistentPersonId = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{nonExistentPersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_NoChanges_DisplaysNoChangesMessage()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var noChanges = doc.GetElementByTestId("no-changes");
        Assert.NotNull(noChanges);
    }

    [Fact]
    public async Task Get_PersonWithNote_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        await TestData.CreateNote(b => b.WithPersonId(person.ContactId).WithSubject("Note 1 Subject").WithDescription("Note 1 Description"));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item"),
            item =>
            {
                Assert.Equal("Note modified", item.GetElementByTestId("timeline-item-title")?.TextContent.Trim());
                Assert.Equal("by Test User", item.GetElementByTestId("timeline-item-user")?.TextContent.Trim());
                Assert.Null(item.GetElementByTestId("timeline-item-status"));
                Assert.NotNull(item.GetElementByTestId("timeline-item-time"));
                Assert.Equal("Note 1 Subject", item.GetElementByTestId("timeline-item-summary")?.TextContent.Trim());
                Assert.Equal("Note 1 Description", item.GetElementByTestId("timeline-item-description")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Get_PersonWithTask_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        await TestData.CreateCrmTask(b => b.WithPersonId(person.ContactId).WithSubject("Task 1 Subject").WithDescription("Task 1 Description").WithOpenStatus());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item"),
            item =>
            {
                Assert.Equal("Task modified", item.GetElementByTestId("timeline-item-title")?.TextContent.Trim());
                Assert.Equal("by Test User", item.GetElementByTestId("timeline-item-user")?.TextContent.Trim());
                Assert.NotNull(item.GetElementByTestId("timeline-item-time"));
                Assert.Equal("Task 1 Subject", item.GetElementByTestId("timeline-item-summary")?.TextContent.Trim());
                Assert.Equal("Task 1 Description", item.GetElementByTestId("timeline-item-description")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Get_PersonWithTaskWithPastDueDate_RendersOverdueStatus()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        await TestData.CreateCrmTask(b => b.WithPersonId(person.ContactId).WithDueDate(Clock.UtcNow.AddDays(-2)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item"),
            item => Assert.Equal("Overdue", item.GetElementByTestId("timeline-item-status")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Get_PersonWithTaskWithCompletedStatus_RendersClosedStatus()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        await TestData.CreateCrmTask(b => b.WithPersonId(person.ContactId).WithCompletedStatus());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item"),
            item =>
            {
                Assert.Equal("Task completed", item.GetElementByTestId("timeline-item-title")?.TextContent.Trim());
                Assert.Equal("Closed", item.GetElementByTestId("timeline-item-status")?.TextContent.Trim());
            });
    }

    [Theory]
    [InlineData(TestData.CreateNameChangeIncidentBuilder.IncidentStatusType.Rejected, "Rejected")]
    [InlineData(TestData.CreateNameChangeIncidentBuilder.IncidentStatusType.Approved, "Approved")]
    public async Task Get_PersonWithNameChangeCase_RendersExpectedItem(
        TestData.CreateNameChangeIncidentBuilder.IncidentStatusType status,
        string expectedSummaryText)
    {
        // Arrange
        var person = await TestData.CreatePerson();
        await TestData.CreateNameChangeIncident(b => b.WithCustomerId(person.ContactId).WithStatus(status));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item"),
            item =>
            {
                Assert.Equal("Request to change name case resolved", item.GetElementByTestId("timeline-item-title")?.TextContent.Trim());
                Assert.Equal("by Test User", item.GetElementByTestId("timeline-item-user")?.TextContent.Trim());
                Assert.Null(item.GetElementByTestId("timeline-item-status"));
                Assert.NotNull(item.GetElementByTestId("timeline-item-time"));
                Assert.Equal(expectedSummaryText, item.GetElementByTestId("timeline-item-summary")?.TextContent.Trim());
            });
    }

    [Theory]
    [InlineData(TestData.CreateDateOfBirthChangeIncidentBuilder.IncidentStatusType.Rejected, "Rejected")]
    [InlineData(TestData.CreateDateOfBirthChangeIncidentBuilder.IncidentStatusType.Approved, "Approved")]
    public async Task Get_PersonWithDateOfBirthChangeCase_RendersExpectedItem(
        TestData.CreateDateOfBirthChangeIncidentBuilder.IncidentStatusType status,
        string expectedSummaryText)
    {
        // Arrange
        var person = await TestData.CreatePerson();
        await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(person.ContactId).WithStatus(status));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item"),
            item =>
            {
                Assert.Equal("Request to change date of birth case resolved", item.GetElementByTestId("timeline-item-title")?.TextContent.Trim());
                Assert.Equal("by Test User", item.GetElementByTestId("timeline-item-user")?.TextContent.Trim());
                Assert.Null(item.GetElementByTestId("timeline-item-status"));
                Assert.NotNull(item.GetElementByTestId("timeline-item-time"));
                Assert.Equal(expectedSummaryText, item.GetElementByTestId("timeline-item-summary")?.TextContent.Trim());
            });
    }
}
