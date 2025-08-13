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
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var noChanges = doc.GetElementByTestId("no-changes");
        Assert.NotNull(noChanges);
    }

    [Fact]
    public async Task Get_PersonWithNote_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await TestData.CreateNoteAsync(b => b.WithPersonId(person.ContactId).WithSubject("Note 1 Subject").WithDescription("Note 1 Description"));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item"),
            item =>
            {
                Assert.Equal("Note modified", item.GetElementByTestId("timeline-item-title")?.TrimmedText());
                Assert.Equal("by Test User", item.GetElementByTestId("timeline-item-user")?.TrimmedText());
                Assert.Null(item.GetElementByTestId("timeline-item-status"));
                Assert.NotNull(item.GetElementByTestId("timeline-item-time"));
                Assert.Equal("Note 1 Subject", item.GetElementByTestId("timeline-item-summary")?.TrimmedText());
                Assert.Equal("Note 1 Description", item.GetElementByTestId("timeline-item-description")?.TrimmedText());
            });
    }

    [Fact]
    public async Task Get_PersonWithTask_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await TestData.CreateCrmTaskAsync(b => b.WithPersonId(person.ContactId).WithSubject("Task 1 Subject").WithDescription("Task 1 Description").WithOpenStatus());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item"),
            item =>
            {
                Assert.Equal("Task modified", item.GetElementByTestId("timeline-item-title")?.TrimmedText());
                Assert.Equal("by Test User", item.GetElementByTestId("timeline-item-user")?.TrimmedText());
                Assert.NotNull(item.GetElementByTestId("timeline-item-time"));
                Assert.Equal("Task 1 Subject", item.GetElementByTestId("timeline-item-summary")?.TrimmedText());
                Assert.Equal("Task 1 Description", item.GetElementByTestId("timeline-item-description")?.TrimmedText());
            });
    }

    [Fact]
    public async Task Get_PersonWithTaskWithPastDueDate_RendersOverdueStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await TestData.CreateCrmTaskAsync(b => b.WithPersonId(person.ContactId).WithDueDate(Clock.UtcNow.AddDays(-2)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item"),
            item => Assert.Equal("Overdue", item.GetElementByTestId("timeline-item-status")?.TrimmedText()));
    }

    [Fact]
    public async Task Get_PersonWithTaskWithCompletedStatus_RendersClosedStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await TestData.CreateCrmTaskAsync(b => b.WithPersonId(person.ContactId).WithCompletedStatus());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item"),
            item =>
            {
                Assert.Equal("Task completed", item.GetElementByTestId("timeline-item-title")?.TrimmedText());
                Assert.Equal("Closed", item.GetElementByTestId("timeline-item-status")?.TrimmedText());
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
        var person = await TestData.CreatePersonAsync();
        await TestData.CreateNameChangeIncidentAsync(b => b.WithCustomerId(person.ContactId).WithStatus(status));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item"),
            item =>
            {
                Assert.Equal("Request to change name case resolved", item.GetElementByTestId("timeline-item-title")?.TrimmedText());
                Assert.Equal("by Test User", item.GetElementByTestId("timeline-item-user")?.TrimmedText());
                Assert.Null(item.GetElementByTestId("timeline-item-status"));
                Assert.NotNull(item.GetElementByTestId("timeline-item-time"));
                Assert.Equal(expectedSummaryText, item.GetElementByTestId("timeline-item-summary")?.TrimmedText());
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
        var person = await TestData.CreatePersonAsync();
        await TestData.CreateDateOfBirthChangeIncidentAsync(b => b.WithCustomerId(person.ContactId).WithStatus(status));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item"),
            item =>
            {
                Assert.Equal("Request to change date of birth case resolved", item.GetElementByTestId("timeline-item-title")?.TrimmedText());
                Assert.Equal("by Test User", item.GetElementByTestId("timeline-item-user")?.TrimmedText());
                Assert.Null(item.GetElementByTestId("timeline-item-status"));
                Assert.NotNull(item.GetElementByTestId("timeline-item-time"));
                Assert.Equal(expectedSummaryText, item.GetElementByTestId("timeline-item-summary")?.TrimmedText());
            });
    }

    [Fact]
    public async Task Get_OutOfBoundsPageNumber_RedirectsToPage1()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await TestData.CreateCrmTaskAsync(b => b.WithPersonId(person.ContactId).WithCompletedStatus());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history?pageNumber=2");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/change-history?pageNumber=1", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_SinglePage_DoesNotShowPagination()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await TestData.CreateCrmTaskAsync(b => b.WithPersonId(person.ContactId).WithCompletedStatus());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Empty(doc.GetElementsByClassName("govuk-pagination"));
    }

    [Fact]
    public async Task Get_PageIsNotLastPage_ShowsNextPageLink()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await CreateTasksAsync(person.PersonId, 11);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history?pageNumber=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotEmpty(doc.GetElementsByClassName("govuk-pagination__link").Where(e => e.GetAttribute("rel") == "next"));
    }

    [Fact]
    public async Task Get_PageIsLastPage_DoesNotShowNextPageLink()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await CreateTasksAsync(person.PersonId, 11);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history?pageNumber=2");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Empty(doc.GetElementsByClassName("govuk-pagination__link").Where(e => e.GetAttribute("rel") == "next"));
    }

    // Random comment

    [Fact]
    public async Task Get_PageIsNotFirstPage_ShowsPreviousPageLink()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await CreateTasksAsync(person.PersonId, 11);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history?pageNumber=2");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotEmpty(doc.GetElementsByClassName("govuk-pagination__link").Where(e => e.GetAttribute("rel") == "prev"));
    }

    [Fact]
    public async Task Get_PageIsFirstPage_DoesNotShowPreviousPageLink()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await CreateTasksAsync(person.PersonId, 11);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history?pageNumber=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Empty(doc.GetElementsByClassName("govuk-pagination__link").Where(e => e.GetAttribute("rel") == "prev"));
    }

    private async Task CreateTasksAsync(Guid personId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            await TestData.CreateCrmTaskAsync(b => b.WithPersonId(personId).WithCompletedStatus());
        }
    }
}
