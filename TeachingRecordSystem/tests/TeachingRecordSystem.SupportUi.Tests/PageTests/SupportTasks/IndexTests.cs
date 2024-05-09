using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks;

[Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture), IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await WithDbContext(dbContext => dbContext.SupportTasks.ExecuteDeleteAsync());
        XrmFakedContext.DeleteAllEntities<Incident>();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Get_NoSortByQueryParam_ShowsTasksSortedByDateRequested()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var dobChangeRequest = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(person.ContactId));
        Clock.UtcNow = dobChangeRequest.CreatedOn.ToUniversalTime().AddMinutes(1);  // CRM entity creations don't use IClock
        var oneLoginUser = await TestData.CreateOneLoginUser();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTask(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        AssertResults(doc, dobChangeRequest.TicketNumber, connectOneLoginUser.SupportTaskReference);
    }

    [Fact]
    public async Task Get_DateRequestedSortByQueryParam_ShowsTasksSortedByDateRequested()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var dobChangeRequest = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(person.ContactId));
        Clock.UtcNow = dobChangeRequest.CreatedOn.ToUniversalTime().AddMinutes(1);  // CRM entity creations don't use IClock
        var oneLoginUser = await TestData.CreateOneLoginUser();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTask(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks?sortBy=DateRequested&category=OneLogin&category=ChangeRequests&_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        AssertResults(doc, dobChangeRequest.TicketNumber, connectOneLoginUser.SupportTaskReference);
    }

    [Fact]
    public async Task Get_TypeSortByQueryParam_ShowsTasksSortedByType()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var dobChangeRequest = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(person.ContactId));
        Clock.UtcNow = dobChangeRequest.CreatedOn.ToUniversalTime().AddMinutes(1);  // CRM entity creations don't use IClock
        var oneLoginUser = await TestData.CreateOneLoginUser();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTask(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks?sortBy=Type&category=OneLogin&category=ChangeRequests&_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        AssertResults(doc, dobChangeRequest.TicketNumber, connectOneLoginUser.SupportTaskReference);
    }

    [Fact]
    public async Task Get_NoCategoriesSpecifiedAndNoFiltersApplied_ReturnsAllCategories()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var dobChangeRequest = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(person.ContactId));
        var oneLoginUser = await TestData.CreateOneLoginUser();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTask(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var references = GetTaskReferences(doc);
        Assert.Contains(dobChangeRequest.TicketNumber, references);
        Assert.Contains(connectOneLoginUser.SupportTaskReference, references);
    }

    [Fact]
    public async Task Get_NoCategoriesSpecifiedAndFiltersApplied_ReturnsNoResults()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var dobChangeRequest = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(person.ContactId));
        var oneLoginUser = await TestData.CreateOneLoginUser();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTask(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks?_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var references = GetTaskReferences(doc);
        Assert.Empty(references);
    }

    [Fact]
    public async Task Get_CategoriesSpecified_ReturnsResultsMatchingCategoriesOnly()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var dobChangeRequest = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(person.ContactId));
        var oneLoginUser = await TestData.CreateOneLoginUser();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTask(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks?category=ChangeRequests&_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var references = GetTaskReferences(doc);
        Assert.Contains(dobChangeRequest.TicketNumber, references);
        Assert.DoesNotContain(connectOneLoginUser.SupportTaskReference, references);
    }

    [Fact]
    public async Task Get_ReferenceSpecified_ReturnsResultMatchingReferenceOnly()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var dobChangeRequest = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(person.ContactId));
        var oneLoginUser = await TestData.CreateOneLoginUser();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTask(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks?reference={connectOneLoginUser.SupportTaskReference}&category=OneLogin&category=ChangeRequests&_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var references = GetTaskReferences(doc);
        Assert.DoesNotContain(dobChangeRequest.TicketNumber, references);
        Assert.Contains(connectOneLoginUser.SupportTaskReference, references);
    }

    private static IEnumerable<string> GetTaskReferences(IHtmlDocument document) =>
        document.GetElementByTestId("results")?.QuerySelectorAll("tbody>tr").Attr("data-reference") ?? [];

    private static void AssertResults(IHtmlDocument document, params string[] expectedReferences)
    {
        var references = GetTaskReferences(document);
        Assert.Equal(expectedReferences, references);
    }
}
