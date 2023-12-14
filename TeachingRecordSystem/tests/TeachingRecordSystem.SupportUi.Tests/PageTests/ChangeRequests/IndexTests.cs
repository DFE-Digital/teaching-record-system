using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ChangeRequests;

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

        var request = new HttpRequestMessage(HttpMethod.Get, "/change-requests");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var createPerson1Result = await TestData.CreatePerson();
        var createIncident1Result = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPerson1Result.ContactId));
        var createPerson2Result = await TestData.CreatePerson();
        var createIncident2Result = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPerson2Result.ContactId));

        var request = new HttpRequestMessage(HttpMethod.Get, "/change-requests");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var tableRow1 = doc.GetElementByTestId($"change-request-{createIncident1Result.TicketNumber}");
        Assert.NotNull(tableRow1);
        Assert.Equal(createIncident1Result.TicketNumber, tableRow1.GetElementByTestId($"request-reference-{createIncident1Result.TicketNumber}")!.TextContent);
        Assert.Equal($"{createPerson1Result.FirstName} {createPerson1Result.LastName}", tableRow1.GetElementByTestId($"name-{createIncident1Result.TicketNumber}")!.TextContent);
        Assert.Equal(createIncident1Result.SubjectTitle, tableRow1.GetElementByTestId($"change-type-{createIncident1Result.TicketNumber}")!.TextContent);
        Assert.Equal(createIncident1Result.CreatedOn.ToString("dd/MM/yyyy"), tableRow1.GetElementByTestId($"created-on-{createIncident1Result.TicketNumber}")!.TextContent);
        var tableRow2 = doc.GetElementByTestId($"change-request-{createIncident2Result.TicketNumber}");
        Assert.NotNull(tableRow2);
        Assert.Equal(createIncident2Result.TicketNumber, tableRow2.GetElementByTestId($"request-reference-{createIncident2Result.TicketNumber}")!.TextContent);
        Assert.Equal($"{createPerson2Result.FirstName} {createPerson2Result.LastName}", tableRow2.GetElementByTestId($"name-{createIncident2Result.TicketNumber}")!.TextContent);
        Assert.Equal(createIncident2Result.SubjectTitle, tableRow2.GetElementByTestId($"change-type-{createIncident2Result.TicketNumber}")!.TextContent);
        Assert.Equal(createIncident2Result.CreatedOn.ToString("dd/MM/yyyy"), tableRow2.GetElementByTestId($"created-on-{createIncident2Result.TicketNumber}")!.TextContent);
    }

    [Fact]
    public async Task Get_ValidRequestNoActiveChangeRequests_RendersExpectedContent()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/change-requests");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var doc = await response.GetDocument();
        Assert.NotNull(doc.GetElementByTestId("no-change-requests"));
    }
}
