using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class CheckYourAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private const string _changeReasonDetails = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";

    [Theory]
    [InlineData(InductionJourneyPage.Status, InductionStatus.InProgress, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Passed, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Failed, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Exempt, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.FailedInWales, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.RequiredToComplete, true)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.InProgress, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Passed, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Failed, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.FailedInWales, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Passed, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Failed, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.FailedInWales, false)]
    [InlineData(InductionJourneyPage.ExemptionReason, InductionStatus.Exempt, false)]
    public async Task Get_ShowsInductionStatus_AsExpected(InductionJourneyPage startPage, InductionStatus inductionStatus, bool showStatus)
    {
        // Arrange
        var labelContent = "Induction status";
        var startDate = Clock.Today.AddYears(-2);
        var completedDate = Clock.Today;
        var exemptionReasonIds = (await TestData.ReferenceDataCache
            .GetInductionExemptionReasonsAsync(activeOnly: true))
            .RandomSelection(1)
            .Select(r => r.InductionExemptionReasonId)
            .ToArray();
        var editInductionState = new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, startPage)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .Create();

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        if (showStatus)
        {
            var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TextContent == labelContent);
            Assert.NotNull(label);
            var value = label.NextElementSibling;
            Assert.Equal(inductionStatus.GetDisplayName(), value!.TextContent);
        }
        else
        {
            Assert.Empty(doc.QuerySelectorAll(".govuk-summary-list__key").Where(e => e.TextContent == labelContent));
        }
    }

    [Theory]
    [InlineData(InductionJourneyPage.Status, InductionStatus.InProgress, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Passed, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Failed, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.FailedInWales, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Exempt, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.RequiredToComplete, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.InProgress, true)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Passed, true)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Failed, true)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.FailedInWales, true)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Passed, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Failed, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.FailedInWales, false)]
    [InlineData(InductionJourneyPage.ExemptionReason, InductionStatus.Exempt, false)]
    public async Task Get_ShowsStartDate_AsExpected(InductionJourneyPage startPage, InductionStatus inductionStatus, bool ShowsStartDate)
    {
        // Arrange
        var labelContent = "Induction start date";
        var startDate = Clock.Today.AddYears(-2);
        var completedDate = Clock.Today;
        var exemptionReasonIds = (await TestData.ReferenceDataCache
            .GetInductionExemptionReasonsAsync(activeOnly: true))
            .RandomSelection(1)
            .Select(r => r.InductionExemptionReasonId)
            .ToArray();
        var editInductionState = new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, startPage)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .Create();

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        if (ShowsStartDate)
        {
            var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TextContent == labelContent);
            Assert.NotNull(label);
            var value = label.NextElementSibling;
            Assert.Equal(startDate.ToString(UiDefaults.DateOnlyDisplayFormat), value!.TextContent);
        }
        else
        {
            Assert.Empty(doc.QuerySelectorAll(".govuk-summary-list__key").Where(e => e.TextContent == labelContent));
        }
    }

    [Theory]
    [InlineData(InductionJourneyPage.Status, InductionStatus.InProgress, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Passed, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Failed, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.FailedInWales, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Exempt, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.RequiredToComplete, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.InProgress, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Passed, true)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Failed, true)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.FailedInWales, true)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Passed, true)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Failed, true)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.FailedInWales, true)]
    [InlineData(InductionJourneyPage.ExemptionReason, InductionStatus.Exempt, false)]
    public async Task Get_ShowsCompletedDate_AsExpected(InductionJourneyPage startPage, InductionStatus inductionStatus, bool ShowsCompletedDate)
    {
        // Arrange
        var labelContent = "Induction completion date";
        var startDate = Clock.Today.AddYears(-2);
        var completedDate = Clock.Today;
        var exemptionReasonIds = (await TestData.ReferenceDataCache
            .GetInductionExemptionReasonsAsync(activeOnly: true))
            .RandomSelection(1)
            .Select(r => r.InductionExemptionReasonId)
            .ToArray();
        var editInductionState = new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, startPage)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .Create();

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        if (ShowsCompletedDate)
        {
            var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TextContent == labelContent);
            Assert.NotNull(label);
            var value = label.NextElementSibling;
            Assert.Equal(completedDate.ToString(UiDefaults.DateOnlyDisplayFormat), value!.TextContent);
        }
        else
        {
            Assert.Empty(doc.QuerySelectorAll(".govuk-summary-list__key").Where(e => e.TextContent == labelContent));
        }
    }

    [Theory]
    [InlineData(InductionJourneyPage.Status, InductionStatus.InProgress, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Passed, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Failed, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.FailedInWales, false)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.Exempt, true)]
    [InlineData(InductionJourneyPage.Status, InductionStatus.RequiredToComplete, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.InProgress, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Passed, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.Failed, false)]
    [InlineData(InductionJourneyPage.StartDate, InductionStatus.FailedInWales, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Passed, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.Failed, false)]
    [InlineData(InductionJourneyPage.CompletedDate, InductionStatus.FailedInWales, false)]
    [InlineData(InductionJourneyPage.ExemptionReason, InductionStatus.Exempt, true)]
    public async Task Get_ShowsExemptionReason_AsExpected(InductionJourneyPage startPage, InductionStatus inductionStatus, bool ShowsExemptionReason)
    {
        // CML TODO  - change - it's supposed to be shown as a list (no bullets)
        // Arrange
        var labelContent = "Exemption reason";
        var startDate = Clock.Today.AddYears(-2);
        var completedDate = Clock.Today;
        var exemptionReasonIds = new Guid[] { new Guid("a5faff9f-29ce-4a6b-a7b8-0c1f57f15920"), new Guid("a112e691-1694-46a7-8f33-5ec5b845c181") };
        var expectedExemptionReasonsDisplayString = "Exempt, Has, or is eligible for, full registration in Scotland";
        var editInductionState = new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, startPage)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .Create();

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        if (ShowsExemptionReason)
        {
            var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TextContent == labelContent);
            Assert.NotNull(label);
            var value = label.NextElementSibling;
            Assert.Equal(expectedExemptionReasonsDisplayString, value!.TextContent);
        }
        else
        {
            Assert.Empty(doc.QuerySelectorAll(".govuk-summary-list__key").Where(e => e.TextContent == labelContent));
        }
    }

    [Fact]
    public async Task Get_ShowsChangeReason_AsExpected()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var startDate = Clock.Today.AddYears(-2);
        var completedDate = Clock.Today;
        var exemptionReasonIds = (await TestData.ReferenceDataCache
            .GetInductionExemptionReasonsAsync(activeOnly: true))
            .RandomSelection(1)
            .Select(r => r.InductionExemptionReasonId)
            .ToArray();
        var editInductionState = new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, InductionJourneyPage.Status)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .WithReasonDetailsChoice(addDetails: true, _changeReasonDetails)
                .WithFileUploadChoice(uploadFile: false)
                .Create();

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TextContent == "Reason for changing induction details");
        Assert.NotNull(label);
        var value = label.NextElementSibling;
        Assert.Equal(InductionChangeReasonOption.AnotherReason.GetDisplayName(), value!.TextContent);

        var labelDetails = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TextContent == "Reason details");
        Assert.NotNull(labelDetails);
        var valueDetails = labelDetails.NextElementSibling;
        Assert.Equal(_changeReasonDetails, valueDetails!.TextContent.Trim());

        // CML TODO - add file upload test
        var labelFileUpload = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TextContent == "Do you have evidence to upload");
        Assert.NotNull(labelFileUpload);
        var valueFileUpload = labelFileUpload.NextElementSibling;
        Assert.Equal("Not provided", valueFileUpload!.TextContent.Trim());
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}
