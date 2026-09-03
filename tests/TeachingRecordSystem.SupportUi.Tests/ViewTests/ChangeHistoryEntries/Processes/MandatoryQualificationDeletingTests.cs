using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class MandatoryQualificationDeletingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersCorrectly()
    {
        // Arrange
        var provider = MandatoryQualificationProvider.All.First();
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var status = MandatoryQualificationStatus.Passed;
        var startDate = new DateOnly(2021, 10, 5);
        var endDate = new DateOnly(2021, 11, 5);

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithProvider(provider.MandatoryQualificationProviderId)
            .WithSpecialism(specialism)
            .WithStartDate(startDate)
            .WithStatus(status, endDate)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = "Added in error",
            Details = "Some reason details",
            AdditionalInformation = "Some additional information",
            EvidenceFile = new EventModels.File { FileId = Guid.NewGuid(), Name = "evidence.jpg" }
        };

        var @event = new MandatoryQualificationDeletedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(mq, providerNameHint: provider.Name)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationDeleting, changeReason: changeReason, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification deleted");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Mandatory qualification deleted for", bodyText);
        Assert.Contains(specialism.GetTitle(), bodyText);
        Assert.Contains(startDate.ToString(WebConstants.DateDisplayFormat), bodyText);

        var reasonBlock = entry.GetElementByTestId("reason-for-change");
        Assert.NotNull(reasonBlock);

        var reasonBlockSummary = reasonBlock.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Reason for deletion", reasonBlockSummary?.TrimmedText());

        reasonBlock.AssertSummaryListRowValueContentMatches("Reason details", changeReason.Reason!);
        reasonBlock.AssertSummaryListRowValueContentMatches("Additional information", changeReason.AdditionalInformation);
        Assert.Equal($"{changeReason.EvidenceFile.Name} (opens in new tab)", reasonBlock.GetElementByTestId("evidence")?.TrimmedText());
    }

    [Theory]
    [InlineData("Another reason", "Some reason details", "Another reason: Some reason details")]
    [InlineData("Added in error", null, "Added in error")]
    [InlineData("Added in error", "", "Added in error")]
    public async Task WithChangeReason_RendersCorrectly(string reason, string? details, string expectedReasonDetails)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = reason,
            Details = details,
            AdditionalInformation = null,
            EvidenceFile = null
        };

        var @event = new MandatoryQualificationDeletedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                mq,
                providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationDeleting, changeReason: changeReason, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification deleted");

        var reasonBlock = entry.GetElementByTestId("reason-for-change");
        Assert.NotNull(reasonBlock);

        var reasonBlockSummary = reasonBlock.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Reason for deletion", reasonBlockSummary?.TrimmedText());

        reasonBlock.AssertSummaryListRowValueContentMatches("Reason details", expectedReasonDetails);
    }

    [Fact]
    public async Task WithoutChangeReason_RendersNoneForReason()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var @event = new MandatoryQualificationDeletedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                mq,
                providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationDeleting, changeReason: null, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification deleted");

        var reasonBlock = entry.GetElementByTestId("reason-for-change");
        Assert.NotNull(reasonBlock);
        reasonBlock.AssertSummaryListRowValueContentMatches("Reason details", "Not provided");
        reasonBlock.AssertSummaryListRowValueContentMatches("Additional information", "Not provided");
        Assert.Null(reasonBlock.GetElementByTestId("evidence"));
    }

    [Fact]
    public async Task WithChangeReasonAndNoEvidence_DoesNotRenderEvidenceRow()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = "Added in error",
            Details = "Some reason details",
            AdditionalInformation = "Some additional information",
            EvidenceFile = null
        };

        var @event = new MandatoryQualificationDeletedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                mq,
                providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationDeleting, changeReason: changeReason, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification deleted");

        var reasonBlock = entry.GetElementByTestId("reason-for-change");
        Assert.NotNull(reasonBlock);
        Assert.Null(reasonBlock.GetElementByTestId("evidence"));
    }
}
