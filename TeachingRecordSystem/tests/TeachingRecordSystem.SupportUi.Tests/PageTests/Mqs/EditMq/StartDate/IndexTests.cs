using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.StartDate;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithUninitializedJourneyState_PopulatesModelFromDatabase()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(databaseStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal($"{databaseStartDate:%d}", doc.GetElementById("StartDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{databaseStartDate:%M}", doc.GetElementById("StartDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{databaseStartDate:yyyy}", doc.GetElementById("StartDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var journeyStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(databaseStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStartDateState
            {
                Initialized = true,
                StartDate = journeyStartDate
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal($"{journeyStartDate:%d}", doc.GetElementById("StartDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{journeyStartDate:%M}", doc.GetElementById("StartDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{journeyStartDate:yyyy}", doc.GetElementById("StartDate.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var startDate = new DateOnly(2021, 10, 6);
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "StartDate", startDate }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoStartDateIsEntered_ReturnsError()
    {
        // Arrange
        var databaseStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(databaseStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "Enter a start date");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task Post_StartDateIsAfterOrEqualToEndDate_RendersError(int daysAfter)
    {
        // Arrange
        var endDate = new DateOnly(2022, 9, 1);
        var newStartDate = endDate.AddDays(daysAfter);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(MandatoryQualificationStatus.Passed, endDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "StartDate.Day", $"{newStartDate:%d}" },
                { "StartDate.Month", $"{newStartDate:%M}" },
                { "StartDate.Year", $"{newStartDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "StartDate", "Start date must be after end date");
    }

    [Fact]
    public async Task Post_WhenStartDateIsEntered_RedirectsToReasonPage()
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var newStartDate = new DateOnly(2021, 10, 6);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "StartDate.Day", $"{newStartDate:%d}" },
                { "StartDate.Month", $"{newStartDate:%M}" },
                { "StartDate.Year", $"{newStartDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/start-date/reason?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/start-date/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var oldStartDate = new DateOnly(2021, 10, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(oldStartDate)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(httpMethod, $"/mqs/{qualificationId}/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private async Task<JourneyInstance<EditMqStartDateState>> CreateJourneyInstanceAsync(Guid qualificationId, EditMqStartDateState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqStartDate,
            state ?? new EditMqStartDateState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
