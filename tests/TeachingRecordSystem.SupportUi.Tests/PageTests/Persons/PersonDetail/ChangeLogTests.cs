using TeachingRecordSystem.Core.DataStore.Postgres.Models;

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
        var person = await CreatePersonWithEventsAsync(1);

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
        var person = await CreatePersonWithEventsAsync(11);

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
        var person = await CreatePersonWithEventsAsync(11);

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
        var person = await CreatePersonWithEventsAsync(11);

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
        var person = await CreatePersonWithEventsAsync(11);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history?pageNumber=1");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.DoesNotContain(doc.GetElementsByClassName("govuk-pagination__link"), e => e.GetAttribute("rel") == "prev");
    }

    private async Task<Person> CreatePersonWithEventsAsync(int eventCount)
    {
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            for (int i = 0; i < eventCount; i++)
            {
                var @event = new LegacyEvents.MandatoryQualificationDqtReactivatedEvent
                {
                    EventId = Guid.NewGuid(),
                    CreatedUtc = TimeProvider.UtcNow.AddMinutes(-i),
                    RaisedBy = Core.DataStore.Postgres.Models.SystemUser.SystemUserId,
                    PersonId = person.PersonId,
                    Key = null,
                    MandatoryQualification = new EventModels.MandatoryQualification
                    {
                        QualificationId = Guid.NewGuid(),
                        Provider = new EventModels.MandatoryQualificationProvider
                        {
                            MandatoryQualificationProviderId = Guid.NewGuid(),
                            Name = $"Provider {i}"
                        },
                        Specialism = MandatoryQualificationSpecialism.Hearing,
                        Status = MandatoryQualificationStatus.Passed,
                        StartDate = new DateOnly(2020, 1, 1),
                        EndDate = new DateOnly(2021, 1, 1)
                    }
                };

                dbContext.AddEventWithoutBroadcast(@event);
            }

            await dbContext.SaveChangesAsync();
        });

        return person;
    }
}
