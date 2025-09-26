using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;
using TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Resolve;
using Xunit.Sdk;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Reject;

public class CheckAnswersTests(HostFixture hostFixture) : NpqTrnRequestTestBase(hostFixture)
{
    [Fact]
    public async Task Get_HasBackLinkExpected()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync("NPQ");

        var supportTask = await new CreateNpqTrnRequestSupportTaskBuilder(applicationUser.UserId)
            .WithMatches(false)
            .ExecuteAsync(TestData);

        var state = new RejectNpqTrnRequestState
        {
            RejectionReason = RejectionReasonOption.EvidenceDoesNotMatch
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.RejectNpqTrnRequest,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var expectedBackLink = $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/reject/reason?{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/reject/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedBackLink, doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
    }

    [Fact]
    public async Task Get_ShowsSelectedReason()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync("NPQ");

        var supportTask = await new CreateNpqTrnRequestSupportTaskBuilder(applicationUser.UserId)
            .WithMatches(false)
            .ExecuteAsync(TestData);

        var state = new RejectNpqTrnRequestState
        {
            RejectionReason = RejectionReasonOption.EvidenceDoesNotMatch
        };
        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.RejectNpqTrnRequest,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/reject/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(state.RejectionReason.GetDisplayName(), doc.GetSummaryListValueForKey("Reason"));
    }

    [Fact]
    public async Task Post_UpdatesSupportTaskPublishesEventAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync("NPQ");

        var supportTask = await new CreateNpqTrnRequestSupportTaskBuilder(applicationUser.UserId)
            .WithMatches(false)
            .ExecuteAsync(TestData);
        var state = new RejectNpqTrnRequestState
        {
            RejectionReason = RejectionReasonOption.EvidenceDoesNotMatch
        };
        var journeyInstance = await CreateJourneyInstance(
                JourneyNames.RejectNpqTrnRequest,
                state,
                new KeyValuePair<string, object>("supportTaskReference", supportTask.SupportTaskReference));

        var requestMetadata = supportTask.TrnRequestMetadata;
        Assert.NotNull(requestMetadata);

        EventPublisher.Clear();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/reject/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert

        // support task is updated
        await WithDbContext(async dbContext =>
        {
            var updatedSupportTask = await dbContext
                .SupportTasks
                .Include(st => st.TrnRequestMetadata)
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
            var supportTaskData = updatedSupportTask.GetData<NpqTrnRequestData>();
            Assert.Equal(SupportRequestOutcome.Rejected, supportTaskData.SupportRequestOutcome);
            Assert.Null(supportTaskData.ResolvedAttributes);
            Assert.Null(supportTaskData.SelectedPersonAttributes);
        });

        // event is published
        var expectedMetadata = EventModels.TrnRequestMetadata.FromModel(requestMetadata) with
        {
            ResolvedPersonId = null
        };
        EventPublisher.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<NpqTrnRequestSupportTaskRejectedEvent>(e);
            Assert.Equal(state.RejectionReason.GetDisplayName(), actualEvent.RejectionReason);
            AssertSupportTaskEventIsExpected(actualEvent);
            AssertTrnRequestMetadataMatches(expectedMetadata, actualEvent.RequestData);
            Assert.Equal(requestMetadata.NpqEvidenceFileId, actualEvent.RequestData?.NpqEvidenceFileId);
            Assert.Equal(requestMetadata.NpqEvidenceFileName, actualEvent.RequestData?.NpqEvidenceFileName);
        });

        // redirect
        Assert.Equal("/support-tasks/npq-trn-requests", response.Headers.Location?.OriginalString);
        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();

        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            $"TRN request for {StringHelper.JoinNonEmpty(' ', requestMetadata.FirstName, requestMetadata.MiddleName, requestMetadata.LastName)} rejected");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    public string? GetLinkToPersonFromBanner(IHtmlDocument doc, string? expectedHeading = null, string? expectedMessage = null)
    {
        var banner = doc.GetElementsByClassName("govuk-notification-banner--success").SingleOrDefault();

        if (banner is null)
        {
            throw new XunitException("No notification banner found.");
        }
        var link = banner.QuerySelector(".govuk-link");

        var href = link?.GetAttribute("href");
        return href;
    }

    private void AssertSupportTaskEventIsExpected(NpqTrnRequestSupportTaskRejectedEvent @event)
    {
        Assert.Equal(Clock.UtcNow, @event.CreatedUtc);
        Assert.Equal(SupportTaskStatus.Open, @event.OldSupportTask.Status);
        Assert.Equal(SupportTaskStatus.Closed, @event.SupportTask.Status);
    }

    private void AssertTrnRequestMetadataMatches(EventModels.TrnRequestMetadata expected, EventModels.TrnRequestMetadata actual)
    {
        Assert.Equal(expected.FirstName, actual.FirstName);
        Assert.Equal(expected.MiddleName, actual.MiddleName);
        Assert.Equal(expected.LastName, actual.LastName);
        Assert.Equal(expected.EmailAddress, actual.EmailAddress);
        Assert.Equal(expected.NationalInsuranceNumber, actual.NationalInsuranceNumber);
        Assert.Equal(expected.Gender, actual.Gender);
        Assert.Equal(expected.DateOfBirth, actual.DateOfBirth);
        Assert.Equal(expected.ResolvedPersonId, actual.ResolvedPersonId);
        Assert.Equivalent(expected.Matches, actual.Matches);
    }
}
