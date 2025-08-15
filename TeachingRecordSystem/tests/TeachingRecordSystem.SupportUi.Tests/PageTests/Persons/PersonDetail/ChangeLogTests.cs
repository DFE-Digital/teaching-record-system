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
        var person = await TestData.CreatePersonAsync(b => b.WithEvents(1));

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
        var person = await TestData.CreatePersonAsync(b => b.WithEvents(11));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history?pageNumber=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Contains(doc.GetElementsByClassName("govuk-pagination__link"), e => e.GetAttribute("rel") == "next");
    }

    [Fact]
    public async Task Get_PageIsLastPage_DoesNotShowNextPageLink()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithEvents(11));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history?pageNumber=2");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.DoesNotContain(doc.GetElementsByClassName("govuk-pagination__link"), e => e.GetAttribute("rel") == "next");
    }

    [Fact]
    public async Task Get_PageIsNotFirstPage_ShowsPreviousPageLink()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithEvents(11));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history?pageNumber=2");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Contains(doc.GetElementsByClassName("govuk-pagination__link"), e => e.GetAttribute("rel") == "prev");
    }

    [Fact]
    public async Task Get_PageIsFirstPage_DoesNotShowPreviousPageLink()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithEvents(11));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history?pageNumber=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.DoesNotContain(doc.GetElementsByClassName("govuk-pagination__link"), e => e.GetAttribute("rel") == "prev");
    }
}

file static class Extensions
{
    public static TestData.CreatePersonBuilder WithEvents(this TestData.CreatePersonBuilder builder, int eventCount)
    {
        for (int i = 0; i < eventCount; i++)
        {
            builder.WithAlert();
        }

        return builder;
    }
}
