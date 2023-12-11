using FormFlow;
using TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.DeleteMq;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithQualificationIdForValidQualification_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualification = person.MandatoryQualifications.Single();
        var journeyInstance = await CreateJourneyInstance(qualification.QualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualification.QualificationId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualification = person.MandatoryQualifications.Single();
        var deletionReason = MqDeletionReasonOption.ProviderRequest;
        var journeyInstance = await CreateJourneyInstance(
            qualification.QualificationId,
            new DeleteMqState()
            {
                DeletionReason = deletionReason,
                DeletionReasonDetail = "My deletion reason detail"
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualification.QualificationId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var deletionReasonOptions = doc.GetElementByTestId("deletion-reason-options");
        var radioButtons = deletionReasonOptions!.GetElementsByTagName("input");
        var selectedDeletionReason = radioButtons.SingleOrDefault(r => r.HasAttribute("checked"));
        Assert.NotNull(selectedDeletionReason);
        Assert.Equal(deletionReason.ToString(), selectedDeletionReason.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoDeletionReasonIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualification = person.MandatoryQualifications.Single();
        var journeyInstance = await CreateJourneyInstance(qualification.QualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualification.QualificationId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "DeletionReason", "Select a reason for deleting");
    }

    [Fact]
    public async Task Post_WhenDeletionReasonIsSelected_RedirectsToConfirmPage()
    {
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualification = person.MandatoryQualifications.Single();
        var journeyInstance = await CreateJourneyInstance(qualification.QualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualification.QualificationId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "DeletionReason", MqDeletionReasonOption.ProviderRequest.ToString() },
                { "DeletionReasonDetail", "My deletion reason detail" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualification.QualificationId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualification = person.MandatoryQualifications.Single();
        var journeyInstance = await CreateJourneyInstance(qualification.QualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualification.QualificationId}/delete/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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

    private async Task<JourneyInstance<DeleteMqState>> CreateJourneyInstance(Guid qualificationId, DeleteMqState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.DeleteMq,
            state ?? new DeleteMqState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
