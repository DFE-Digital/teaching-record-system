using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Status;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidRequestWithUninitializedJourneyState_PopulatesModelFromDatabase()
    {
        // Arrange
        var databaseStatus = MandatoryQualificationStatus.Passed;
        var databaseEndDate = new DateOnly(2021, 11, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(databaseStatus, databaseEndDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var resultOptions = doc.GetElementByTestId("result-options");
        var radioButtons = resultOptions!.GetElementsByTagName("input");
        var selectedResult = radioButtons.SingleOrDefault(r => r.HasAttribute("checked"));
        Assert.NotNull(selectedResult);
        Assert.Equal(databaseStatus.ToString(), selectedResult.GetAttribute("value"));
        Assert.Equal($"{databaseEndDate:%d}", doc.GetElementById("EndDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{databaseEndDate:%M}", doc.GetElementById("EndDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{databaseEndDate:yyyy}", doc.GetElementById("EndDate.Year")?.GetAttribute("value"));
    }

    [Test]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var databaseStatus = MandatoryQualificationStatus.Failed;
        var journeyStatus = MandatoryQualificationStatus.Passed;
        var journeyEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(databaseStatus)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqStatusState
            {
                Initialized = true,
                Status = journeyStatus,
                EndDate = journeyEndDate
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var resultOptions = doc.GetElementByTestId("result-options");
        var radioButtons = resultOptions!.GetElementsByTagName("input");
        var selectedResult = radioButtons.SingleOrDefault(r => r.HasAttribute("checked"));
        Assert.NotNull(selectedResult);
        Assert.Equal(journeyStatus.ToString(), selectedResult.GetAttribute("value"));
        Assert.Equal($"{journeyEndDate:%d}", doc.GetElementById("EndDate.Day")?.GetAttribute("value"));
        Assert.Equal($"{journeyEndDate:%M}", doc.GetElementById("EndDate.Month")?.GetAttribute("value"));
        Assert.Equal($"{journeyEndDate:yyyy}", doc.GetElementById("EndDate.Year")?.GetAttribute("value"));
    }

    [Test]
    public async Task Post_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WhenResultIsNotSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Status", "Select a status");
    }

    [Test]
    public async Task Post_WhenResultIsSelectedAndEndDateHasNotBeenEntered_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);
        var status = MandatoryQualificationStatus.Passed;

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Status", status.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EndDate", "Enter an end date");
    }

    [Test]
    [Arguments(0)]
    [Arguments(1)]
    public async Task Post_EndDateIsBeforeOrEqualToStartDate_RendersError(int daysBefore)
    {
        // Arrange
        var startDate = new DateOnly(2021, 12, 5);
        var newStatus = dfeta_qualification_dfeta_MQ_Status.Passed;
        var newEndDate = startDate.AddDays(-daysBefore);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStartDate(startDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Status", newStatus.ToString() },
                { "EndDate.Day", $"{newEndDate:%d}" },
                { "EndDate.Month", $"{newEndDate:%M}" },
                { "EndDate.Year", $"{newEndDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EndDate", "End date must be after start date");
    }

    [Test]
    public async Task Post_ValidRequest_RedirectsToConfirmPage()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var newStatus = dfeta_qualification_dfeta_MQ_Status.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Status", newStatus.ToString() },
                { "EndDate.Day", $"{newEndDate:%d}" },
                { "EndDate.Month", $"{newEndDate:%M}" },
                { "EndDate.Year", $"{newEndDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/status/reason?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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

    [Test]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(httpMethod, $"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private async Task<JourneyInstance<EditMqStatusState>> CreateJourneyInstanceAsync(Guid qualificationId, EditMqStatusState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqStatus,
            state ?? new EditMqStatusState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
