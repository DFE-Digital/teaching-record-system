using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins.OneLoginDetail;

public class ChangeHistoryTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WhenFeatureFlagDisabled_ReturnsNotFound()
    {
        // Arrange
        FeatureProvider.Features.Remove(FeatureNames.SupportTaskChangeHistory);

        var user = await TestData.CreateOneLoginUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithNonExistentOneLoginUserSubject_ReturnsNotFound()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.SupportTaskChangeHistory);

        var nonExistentSubject = "non-existent-subject";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{nonExistentSubject}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_NoChanges_DisplaysNoChangesMessage()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.SupportTaskChangeHistory);

        var user = await TestData.CreateOneLoginUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var noChanges = doc.GetElementByTestId("no-changes");
        Assert.NotNull(noChanges);
    }

    [Fact]
    public async Task Get_WithChanges_DisplaysChangeHistory()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.SupportTaskChangeHistory);

        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateOneLoginUserAsync(person);

        // Create a process that links to this OneLogin user
        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonOneLoginUserConnecting,
            events: new OneLoginUserUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                OneLoginUser = EventModels.OneLoginUser.FromModel(user),
                OldOneLoginUser = EventModels.OneLoginUser.FromModel(user) with { PersonId = null },
                Changes = OneLoginUserUpdatedEventChanges.PersonId
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeHistoryItem = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(changeHistoryItem);
    }

    [Fact]
    public async Task Get_OutOfBoundsPageNumber_RedirectsToPage1()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.SupportTaskChangeHistory);

        var user = await TestData.CreateOneLoginUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}/change-history?pageNumber=2");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var expectedUrl = $"/one-logins/{user.Subject}/change-history?pageNumber=1";
        Assert.Equal(expectedUrl, Uri.UnescapeDataString(response.Headers.Location!.OriginalString));
    }

    [Fact]
    public async Task Get_SinglePage_DoesNotShowPagination()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.SupportTaskChangeHistory);

        var user = await CreateOneLoginUserWithProcessesAsync(1);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}/change-history");

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
        FeatureProvider.Features.Add(FeatureNames.SupportTaskChangeHistory);

        var user = await CreateOneLoginUserWithProcessesAsync(11);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}/change-history?pageNumber=1");

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
        FeatureProvider.Features.Add(FeatureNames.SupportTaskChangeHistory);

        var user = await CreateOneLoginUserWithProcessesAsync(11);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}/change-history?pageNumber=2");

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
        FeatureProvider.Features.Add(FeatureNames.SupportTaskChangeHistory);

        var user = await CreateOneLoginUserWithProcessesAsync(11);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}/change-history?pageNumber=2");

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
        FeatureProvider.Features.Add(FeatureNames.SupportTaskChangeHistory);

        var user = await CreateOneLoginUserWithProcessesAsync(11);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}/change-history?pageNumber=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.DoesNotContain(doc.GetElementsByClassName("govuk-pagination__link"), e => e.GetAttribute("rel") == "prev");
    }

    [Fact]
    public async Task Get_MultiplePagesOfChanges_DisplaysCorrectPage()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.SupportTaskChangeHistory);

        var user = await CreateOneLoginUserWithProcessesAsync(15);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{user.Subject}/change-history?pageNumber=2");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var changeHistoryItems = doc.QuerySelectorAll("[data-process-id]");
        Assert.Equal(5, changeHistoryItems.Length);
    }

    private async Task<OneLoginUser> CreateOneLoginUserWithProcessesAsync(int processCount)
    {
        var person = await TestData.CreatePersonAsync();
        var user = await TestData.CreateOneLoginUserAsync(person);

        for (int i = 0; i < processCount; i++)
        {
            await TestData.CreateProcessAsync(
                ProcessType.PersonOneLoginUserConnecting,
                events: new OneLoginUserUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    OneLoginUser = EventModels.OneLoginUser.FromModel(user),
                    OldOneLoginUser = EventModels.OneLoginUser.FromModel(user) with { PersonId = null },
                    Changes = OneLoginUserUpdatedEventChanges.PersonId
                });
        }

        return user;
    }
}
