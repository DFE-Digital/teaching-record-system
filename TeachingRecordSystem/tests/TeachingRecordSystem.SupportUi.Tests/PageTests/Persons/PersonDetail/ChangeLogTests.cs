namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogTests : TestBase
{
    public ChangeLogTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange        
        var nonExistentPersonId = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{nonExistentPersonId}/changelog");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithNoNotes_DisplaysNoChanges()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/changelog");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var noChanges = doc.GetElementByTestId("no-changes");
        Assert.NotNull(noChanges);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithNotes_DisplaysChangesInDescendingOrder()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        await TestData.CreateNote(b => b.WithPersonId(person.ContactId).WithSubject("Note 1 Subject").WithDescription("Note 1 Description"));
        await TestData.CreateNote(b => b.WithPersonId(person.ContactId).WithSubject("Note 2 Subject").WithDescription("Note 2 Description"));
        await TestData.CreateCrmTask(b => b.WithPersonId(person.ContactId).WithSubject("Task 1 Subject").WithDescription("Task 1 Description"));
        await TestData.CreateCrmTask(b => b.WithPersonId(person.ContactId).WithSubject("Task 2 Subject").WithDescription("Task 2 Description").WithDueDate(Clock.UtcNow.AddDays(-2)));
        await TestData.CreateCrmTask(b => b.WithPersonId(person.ContactId).WithSubject("Task 3 Subject").WithDescription("Task 3 Description").WithCompletedStatus());
        await TestData.CreateNameChangeIncident(b => b.WithCustomerId(person.ContactId).WithRejectedStatus());
        await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(person.ContactId).WithApprovedStatus());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/changelog");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var changes = doc.GetAllElementsByTestId("timeline-item");
        Assert.NotEmpty(changes);
        Assert.Equal(7, changes.Count);
        Assert.Equal("Request to change date of birth case resolved", changes[0].GetElementByTestId("timeline-item-title")!.TextContent);
        Assert.Equal("by Test User", changes[0].GetElementByTestId("timeline-item-user")!.TextContent);
        Assert.Null(changes[0].GetElementByTestId("timeline-item-status"));
        Assert.NotNull(changes[0].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Approved", changes[0].GetElementByTestId("timeline-item-summary")!.TextContent);
        Assert.Equal("Request to change name case resolved", changes[1].GetElementByTestId("timeline-item-title")!.TextContent);
        Assert.Equal("by Test User", changes[1].GetElementByTestId("timeline-item-user")!.TextContent);
        Assert.Null(changes[1].GetElementByTestId("timeline-item-status"));
        Assert.NotNull(changes[1].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Rejected", changes[1].GetElementByTestId("timeline-item-summary")!.TextContent);
        Assert.Equal("Task completed", changes[2].GetElementByTestId("timeline-item-title")!.TextContent);
        Assert.Equal("by Test User", changes[2].GetElementByTestId("timeline-item-user")!.TextContent);
        Assert.Equal("Closed", changes[2].GetElementByTestId("timeline-item-status")!.TextContent);
        Assert.NotNull(changes[2].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Task 3 Subject", changes[2].GetElementByTestId("timeline-item-summary")!.TextContent);
        Assert.Equal("Task 3 Description", changes[2].GetElementByTestId("timeline-item-description")!.TextContent);
        Assert.Equal("Task modified", changes[3].GetElementByTestId("timeline-item-title")!.TextContent);
        Assert.Equal("by Test User", changes[3].GetElementByTestId("timeline-item-user")!.TextContent);
        Assert.Equal("Overdue", changes[3].GetElementByTestId("timeline-item-status")!.TextContent);
        Assert.NotNull(changes[3].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Task 2 Subject", changes[3].GetElementByTestId("timeline-item-summary")!.TextContent);
        Assert.Equal("Task 2 Description", changes[3].GetElementByTestId("timeline-item-description")!.TextContent);
        Assert.Equal("Task modified", changes[4].GetElementByTestId("timeline-item-title")!.TextContent);
        Assert.Equal("by Test User", changes[4].GetElementByTestId("timeline-item-user")!.TextContent);
        Assert.Equal("Active", changes[4].GetElementByTestId("timeline-item-status")!.TextContent);
        Assert.NotNull(changes[4].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Task 1 Subject", changes[4].GetElementByTestId("timeline-item-summary")!.TextContent);
        Assert.Equal("Task 1 Description", changes[4].GetElementByTestId("timeline-item-description")!.TextContent);
        Assert.Equal("Note modified", changes[5].GetElementByTestId("timeline-item-title")!.TextContent);
        Assert.Equal("by Test User", changes[5].GetElementByTestId("timeline-item-user")!.TextContent);
        Assert.Null(changes[5].GetElementByTestId("timeline-item-status"));
        Assert.NotNull(changes[5].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Note 2 Subject", changes[5].GetElementByTestId("timeline-item-summary")!.TextContent);
        Assert.Equal("Note 2 Description", changes[5].GetElementByTestId("timeline-item-description")!.TextContent);
        Assert.Equal("Note modified", changes[6].GetElementByTestId("timeline-item-title")!.TextContent);
        Assert.Equal("by Test User", changes[6].GetElementByTestId("timeline-item-user")!.TextContent);
        Assert.Null(changes[6].GetElementByTestId("timeline-item-status"));
        Assert.NotNull(changes[6].GetElementByTestId("timeline-item-time"));
        Assert.Equal("Note 1 Subject", changes[6].GetElementByTestId("timeline-item-summary")!.TextContent);
        Assert.Equal("Note 1 Description", changes[6].GetElementByTestId("timeline-item-description")!.TextContent);
    }
}
