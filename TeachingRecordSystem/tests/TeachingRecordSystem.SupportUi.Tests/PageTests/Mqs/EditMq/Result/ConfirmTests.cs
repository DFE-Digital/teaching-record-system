using FormFlow;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Result;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Result;

public class ConfirmTests : TestBase
{
    public ConfirmTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_Redirects()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqResultState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false)
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/result/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/result?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_DisplaysContentAsExpected()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(status: oldStatus));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqResultState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/result/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var changeDetails = doc.GetElementByTestId("change-details");
        Assert.NotNull(changeDetails);
        Assert.Equal(oldStatus.GetTitle(), changeDetails.GetElementByTestId("current-result")!.TextContent);
        Assert.Equal(newStatus.GetTitle(), changeDetails.GetElementByTestId("new-result")!.TextContent);
        Assert.Equal("None", changeDetails.GetElementByTestId("current-end-date")!.TextContent);
        Assert.Equal(newEndDate.ToString("d MMMM yyyy"), changeDetails.GetElementByTestId("new-end-date")!.TextContent);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_Redirects()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqResultState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false)
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/result/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/result?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Confirm_CompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(status: oldStatus));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqResultState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/result/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Mandatory qualification changed");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        TestData.CreatePersonResult person = await TestData.CreatePerson(b => b.WithMandatoryQualification(status: oldStatus));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqResultState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/result/confirm/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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

    private async Task<JourneyInstance<EditMqResultState>> CreateJourneyInstance(Guid qualificationId, EditMqResultState? state = null) =>
    await CreateJourneyInstance(
        JourneyNames.EditMqResult,
        state ?? new EditMqResultState(),
        new KeyValuePair<string, object>("qualificationId", qualificationId));
}
