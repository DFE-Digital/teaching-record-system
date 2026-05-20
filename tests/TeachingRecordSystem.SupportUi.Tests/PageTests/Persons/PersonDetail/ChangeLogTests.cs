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

    private async Task<TestData.CreatePersonResult> CreatePersonWithEventsAsync(int eventCount)
    {
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            for (int i = 0; i < eventCount; i++)
            {
                var @event = new LegacyEvents.PersonDetailsUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    CreatedUtc = Clock.UtcNow.AddMinutes(-i),
                    RaisedBy = Core.DataStore.Postgres.Models.SystemUser.SystemUserId,
                    PersonId = person.PersonId,
                    PersonAttributes = new EventModels.PersonDetails
                    {
                        FirstName = person.FirstName,
                        MiddleName = person.MiddleName,
                        LastName = person.LastName,
                        DateOfBirth = person.DateOfBirth,
                        EmailAddress = person.EmailAddress,
                        NationalInsuranceNumber = person.NationalInsuranceNumber,
                        Gender = person.Gender
                    },
                    OldPersonAttributes = new EventModels.PersonDetails
                    {
                        FirstName = person.FirstName,
                        MiddleName = person.MiddleName,
                        LastName = person.LastName,
                        DateOfBirth = person.DateOfBirth,
                        EmailAddress = person.EmailAddress,
                        NationalInsuranceNumber = person.NationalInsuranceNumber,
                        Gender = person.Gender
                    },
                    Changes = LegacyEvents.PersonDetailsUpdatedEventChanges.EmailAddress,
                    NameChangeReason = null,
                    NameChangeEvidenceFile = null,
                    DetailsChangeReason = null,
                    DetailsChangeReasonDetail = null,
                    DetailsChangeEvidenceFile = null
                };

                dbContext.AddEventWithoutBroadcast(@event);
            }

            await dbContext.SaveChangesAsync();
        });

        return person;
    }
}
