using AngleSharp.Dom;
using AngleSharp.Html.Dom;


namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_NoSortByQueryParam_ShowsTasksSortedByDateRequested()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var dobChangeRequest = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth)));
        Clock.Advance();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        AssertResults(doc, dobChangeRequest.SupportTaskReference, connectOneLoginUser.SupportTaskReference);
    }

    [Test]
    public async Task Get_DateRequestedSortByQueryParam_ShowsTasksSortedByDateRequested()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var dobChangeRequest = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth)));
        Clock.Advance();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks?sortBy=DateRequested&category=OneLogin&category=ChangeRequests&_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        AssertResults(doc, dobChangeRequest.SupportTaskReference, connectOneLoginUser.SupportTaskReference);
    }

    [Test]
    public async Task Get_TypeSortByQueryParam_ShowsTasksSortedByType()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var dobChangeRequest = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth)));
        Clock.Advance();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks?sortBy=Type&category=OneLogin&category=ChangeRequests&_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        AssertResults(doc, dobChangeRequest.SupportTaskReference, connectOneLoginUser.SupportTaskReference);
    }

    [Test]
    public async Task Get_NoCategoriesSpecifiedAndNoFiltersApplied_ReturnsAllCategories()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var dobChangeRequest = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth)));
        Clock.Advance();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var references = GetTaskReferences(doc);
        Assert.Contains(dobChangeRequest.SupportTaskReference, references);
        Assert.Contains(connectOneLoginUser.SupportTaskReference, references);
    }

    [Test]
    public async Task Get_NoCategoriesSpecifiedAndFiltersApplied_ReturnsNoResults()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks?_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var references = GetTaskReferences(doc);
        Assert.Empty(references);
    }

    [Test]
    public async Task Get_CategoriesSpecified_ReturnsResultsMatchingCategoriesOnly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var dobChangeRequest = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth)));
        Clock.Advance();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks?category=ChangeRequests&_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var references = GetTaskReferences(doc);
        Assert.Contains(dobChangeRequest.SupportTaskReference, references);
        Assert.DoesNotContain(connectOneLoginUser.SupportTaskReference, references);
    }

    [Test]
    public async Task Get_ReferenceSpecified_ReturnsResultMatchingReferenceOnly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var dobChangeRequest = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
            person.PersonId,
            b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth)));
        Clock.Advance();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        var connectOneLoginUser = await TestData.CreateConnectOneLoginUserSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks?reference={connectOneLoginUser.SupportTaskReference}&category=OneLogin&category=ChangeRequests&_f=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var references = GetTaskReferences(doc);
        Assert.DoesNotContain(dobChangeRequest.SupportTaskReference, references);
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
