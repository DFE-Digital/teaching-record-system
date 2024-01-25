using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Specialism;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithUninitializedJourneyState_PopulatesModelFromDatabase()
    {
        // Arrange
        var databaseSpecialism = MandatoryQualificationSpecialism.Hearing;
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithSpecialism(databaseSpecialism)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var specialismList = doc.GetElementByTestId("specialism-list");
        var radioButtons = specialismList!.GetElementsByTagName("input");
        var selectedSpecialism = radioButtons.SingleOrDefault(r => r.HasAttribute("checked"));
        Assert.NotNull(selectedSpecialism);
        Assert.Equal(databaseSpecialism.ToString(), selectedSpecialism.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var databaseSpecialism = MandatoryQualificationSpecialism.Hearing;
        var journeySpecialism = MandatoryQualificationSpecialism.Visual;
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithSpecialism(databaseSpecialism)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqSpecialismState()
            {
                Initialized = true,
                Specialism = journeySpecialism
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var specialismList = doc.GetElementByTestId("specialism-list");
        var radioButtons = specialismList!.GetElementsByTagName("input");
        var selectedSpecialism = radioButtons.SingleOrDefault(r => r.HasAttribute("checked"));
        Assert.NotNull(selectedSpecialism);
        Assert.Equal(journeySpecialism.ToString(), selectedSpecialism.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var specialismValue = "Hearing";
        var journeyInstance = await CreateJourneyInstance(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "SpecialismValue", specialismValue }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoSpecialismIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Specialism", "Select a specialism");
    }

    [Fact]
    public async Task Post_WhenSpecialismIsSelected_RedirectsToReasonPage()
    {
        // Arrange
        var oldSpecialism = MandatoryQualificationSpecialism.Hearing;
        var newSpecialism = MandatoryQualificationSpecialism.Visual;
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithSpecialism(oldSpecialism)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqSpecialismState()
            {
                Initialized = true,
                Specialism = oldSpecialism
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Specialism", newSpecialism }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/specialism/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithSpecialism(specialism)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqSpecialismState()
            {
                Initialized = true,
                Specialism = specialism
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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


    private async Task<JourneyInstance<EditMqSpecialismState>> CreateJourneyInstance(Guid qualificationId, EditMqSpecialismState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqSpecialism,
            state ?? new EditMqSpecialismState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
