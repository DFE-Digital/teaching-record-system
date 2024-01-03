using FormFlow;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Status;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Status;

public class ConfirmTests(HostFixture hostFixture) : TestBase(hostFixture)
{
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
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/status/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_DisplaysContentAsExpected()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqResultState()
            {
                Initialized = true,
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/status/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var changeDetails = doc.GetElementByTestId("change-details");
        Assert.NotNull(changeDetails);
        Assert.Equal(oldStatus.GetTitle(), changeDetails.GetElementByTestId("current-status")!.TextContent);
        Assert.Equal(newStatus.GetTitle(), changeDetails.GetElementByTestId("new-status")!.TextContent);
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
                Initialized = true
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/status?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Confirm_CompletesJourneyRedirectsWithFlashMessageAndUpdatesMq()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqResultState()
            {
                Initialized = true,
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
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

        await WithDbContext(async dbContext =>
        {
            var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.PersonId == person.PersonId);
            Assert.Equal(newStatus, qualification.Status);
            Assert.Equal(newEndDate, qualification.EndDate);
        });
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyRedirectsAndDoesNotUpdateMq()
    {
        // Arrange
        var oldStatus = MandatoryQualificationStatus.Failed;
        var oldEndDate = (DateOnly?)null;
        var newStatus = MandatoryQualificationStatus.Passed;
        var newEndDate = new DateOnly(2021, 12, 5);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithStatus(oldStatus)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqResultState()
            {
                Initialized = true,
                Status = newStatus,
                EndDate = newEndDate,
                CurrentStatus = oldStatus,
                CurrentEndDate = oldEndDate
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/status/confirm/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);

        await WithDbContext(async dbContext =>
        {
            var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.PersonId == person.PersonId);
            Assert.Equal(oldStatus, qualification.Status);
            Assert.Equal(oldEndDate, qualification.EndDate);
        });
    }

    private async Task<JourneyInstance<EditMqResultState>> CreateJourneyInstance(Guid qualificationId, EditMqResultState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqResult,
            state ?? new EditMqResultState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
