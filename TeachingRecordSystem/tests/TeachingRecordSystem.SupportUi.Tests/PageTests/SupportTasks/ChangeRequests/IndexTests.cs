using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ChangeRequests;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoOpenChangeRequests_ShowsNoChangeRequestsMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(person.PersonId, b => b
            .WithLastName(TestData.GenerateChangedLastName(person.LastName))
            .WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Empty(doc.GetElementsByTagName("table"));
        Assert.NotNull(doc.GetElementByTestId("no-tasks-message"));
        Assert.Null(doc.GetElementByTestId("no-results-message"));
    }

    [Theory]
    [InlineData(SupportTaskType.ChangeNameRequest)]
    [InlineData(SupportTaskType.ChangeDateOfBirthRequest)]
    public async Task Get_WithChangeRequest_ButNotMatchingSearchCriteria_ShowsNoResultsMessage(SupportTaskType supportTaskType)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        SupportTask supportTask;

        if (supportTaskType == SupportTaskType.ChangeNameRequest)
        {
            supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(person.PersonId,
                b => b.WithLastName(TestData.GenerateChangedLastName(person.LastName)));
        }
        else
        {
            supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(person.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth)));
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests?Search=XXX");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Empty(doc.GetElementsByTagName("table"));
        Assert.Null(doc.GetElementByTestId("no-tasks-message"));
        Assert.NotNull(doc.GetElementByTestId("no-results-message"));
    }

    [Theory]
    [InlineData(SupportTaskType.ChangeNameRequest)]
    [InlineData(SupportTaskType.ChangeDateOfBirthRequest)]
    public async Task Get_WithChangeRequest_ShowsExpectedDataInResultsTable(SupportTaskType supportTaskType)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        SupportTask supportTask;

        if (supportTaskType == SupportTaskType.ChangeNameRequest)
        {
            supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(person.PersonId,
                b => b.WithLastName(TestData.GenerateChangedLastName(person.LastName)));
        }
        else
        {
            supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(person.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person.DateOfBirth)));
        }

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByTestId("no-tasks-message"));
        Assert.Null(doc.GetElementByTestId("no-results-message"));

        var resultRow = doc.GetElementByTestId("results")
            ?.GetElementsByTagName("tbody")
            .FirstOrDefault()
            ?.GetElementsByTagName("tr")
            .FirstOrDefault();

        Assert.NotNull(resultRow);
        AssertRowHasContent("name", $"{person.FirstName} {person.MiddleName} {person.LastName}");
        AssertRowHasContent("requested-on", supportTask.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent("change-type", supportTaskType == SupportTaskType.ChangeNameRequest ? "Name" : "Date of birth");

        void AssertRowHasContent(string testId, string expectedText)
        {
            var column = resultRow.GetElementByTestId(testId);
            Assert.NotNull(column);
            Assert.Equal(expectedText, column.TrimmedText());
        }
    }

    [Fact]
    public async Task Get_NoChangeRequestTypesSpecifiedAndNoFiltersApplied_ReturnsAllChangeRequestTypes()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var nameChangeRequest = await TestData.CreateChangeNameRequestSupportTaskAsync(person1.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(person1.LastName)));

        var person2 = await TestData.CreatePersonAsync();
        var dobChangeRequest = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(person2.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person2.DateOfBirth)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var references = GetTaskReferences(doc);
        Assert.Contains(nameChangeRequest.SupportTaskReference, references);
        Assert.Contains(dobChangeRequest.SupportTaskReference, references);
    }

    [Fact]
    public async Task Get_NoChangeRequestTypesSpecifiedAndFiltersApplied_ReturnsNoResults()
    {
        // Arrange
        var createPersonResult = await TestData.CreatePersonAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            createPersonResult.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests?_f=true");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Empty(doc.GetElementsByTagName("table"));
        Assert.Null(doc.GetElementByTestId("no-tasks-message"));
        Assert.NotNull(doc.GetElementByTestId("no-results-message"));
    }

    [Theory]
    [InlineData(new[] { SupportTaskType.ChangeNameRequest })]
    [InlineData(new[] { SupportTaskType.ChangeDateOfBirthRequest })]
    [InlineData(new[] { SupportTaskType.ChangeNameRequest, SupportTaskType.ChangeDateOfBirthRequest })]
    public async Task Get_ChangeRequestTypesSpecifiedAndFiltersApplied_ReturnsResultsMatchingChangeRequestTypesOnly(SupportTaskType[] supportTaskTypes)
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var nameChangeRequest = await TestData.CreateChangeNameRequestSupportTaskAsync(person1.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(person1.LastName)));

        var person2 = await TestData.CreatePersonAsync();
        var dobChangeRequest = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(person2.PersonId,
            b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(person2.DateOfBirth)));

        var changeRequestTypesQueryParam = string.Join("&", supportTaskTypes.Select(t => $"changeRequestTypes={t}"));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests?_f=true&{changeRequestTypesQueryParam}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var references = GetTaskReferences(doc);

        if (supportTaskTypes.Contains(SupportTaskType.ChangeNameRequest))
        {
            Assert.Contains(nameChangeRequest.SupportTaskReference, references);
        }
        else
        {
            Assert.DoesNotContain(nameChangeRequest.SupportTaskReference, references);
        }

        if (supportTaskTypes.Contains(SupportTaskType.ChangeDateOfBirthRequest))
        {
            Assert.Contains(dobChangeRequest.SupportTaskReference, references);
        }
        else
        {
            Assert.DoesNotContain(dobChangeRequest.SupportTaskReference, references);
        }
    }

    [Fact]
    public async Task Get_SearchByName_ShowsMatchingResult()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var request1 = await TestData.CreateChangeNameRequestSupportTaskAsync(person1.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(person1.LastName)));

        var person2 = await TestData.CreatePersonAsync();
        var request2 = await TestData.CreateChangeNameRequestSupportTaskAsync(person2.PersonId,
            b => b.WithLastName(TestData.GenerateChangedLastName(person2.LastName)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/change-requests?search={Uri.EscapeDataString(person1.LastName)}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var references = GetTaskReferences(doc);
        Assert.Contains(request1.SupportTaskReference, references);
        Assert.DoesNotContain(request2.SupportTaskReference, references);
    }

    private static IEnumerable<string> GetTaskReferences(IHtmlDocument document) =>
        document.GetElementByTestId("results")?.QuerySelectorAll("tbody>tr").Attr("data-reference") ?? [];
}
