using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class MandatoryQualificationCreatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
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

        var @event = new MandatoryQualificationCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(mq, providerNameHint: provider.Name)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationCreating, changeReason: null, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification added");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Mandatory qualification added for", bodyText);
        Assert.Contains(specialism.GetTitle(), bodyText);
        Assert.Contains(startDate.ToString(WebConstants.DateDisplayFormat), bodyText);

        Assert.Null(entry.GetElementByTestId("change-reason"));
    }

    [Fact]
    public async Task ProcessWithoutSpecialismRendersCorrectly()
    {
        // Arrange
        var startDate = new DateOnly(2021, 10, 5);

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithSpecialism(null)
            .WithStartDate(startDate)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var @event = new MandatoryQualificationCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                mq,
                providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationCreating, changeReason: null, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Equal($"Mandatory qualification added with start date {startDate.ToString(WebConstants.DateDisplayFormat)}", bodyText);
    }

    [Fact]
    public async Task ProcessWithoutStartDateRendersCorrectly()
    {
        // Arrange
        var specialism = MandatoryQualificationSpecialism.Hearing;

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithSpecialism(specialism)
            .WithStartDate(null)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var @event = new MandatoryQualificationCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                mq,
                providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationCreating, changeReason: null, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Equal($"Mandatory qualification added for {specialism.GetTitle()}", bodyText);
    }

    [Fact]
    public async Task ProcessWithoutSpecialismOrStartDateRendersCorrectly()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithSpecialism(null)
            .WithStartDate(null)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var @event = new MandatoryQualificationCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                mq,
                providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationCreating, changeReason: null, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Equal("Mandatory qualification added", bodyText);
    }

    [Fact]
    public async Task WithChangeReason_RendersReason()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = "Change of provider",
            Details = "Some reason details",
            AdditionalInformation = "Some additional information",
            EvidenceFile = new EventModels.File { FileId = Guid.NewGuid(), Name = "evidence.jpg" }
        };

        var @event = new MandatoryQualificationCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                mq,
                providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationCreating, changeReason: changeReason, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification added");

        var reasonBlock = entry.GetElementByTestId("change-reason");
        Assert.NotNull(reasonBlock);

        var reasonBlockSummary = reasonBlock.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Reason for adding mandatory qualification", reasonBlockSummary?.TrimmedText());

        reasonBlock.AssertSummaryListRowValueContentMatches("Reason details", changeReason.Reason!);
        reasonBlock.AssertSummaryListRowValueContentMatches("Additional information", changeReason.AdditionalInformation);
        Assert.Equal($"{changeReason.EvidenceFile.Name} (opens in new tab)", reasonBlock.GetElementByTestId("evidence")?.TrimmedText());
    }

    [Fact]
    public async Task WithChangeReasonAndNoEvidence_DoesNotRenderEvidenceRow()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = "Change of provider",
            Details = "Some reason details",
            AdditionalInformation = "Some additional information",
            EvidenceFile = null
        };

        var @event = new MandatoryQualificationCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                mq,
                providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationCreating, changeReason: changeReason, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification added");

        var reasonBlock = entry.GetElementByTestId("change-reason");
        Assert.NotNull(reasonBlock);
        Assert.Null(reasonBlock.GetElementByTestId("evidence"));
    }

    [Theory]
    [InlineData("Another reason", "Some reason details", "Another reason: Some reason details")]
    [InlineData("Routine notification from stakeholder", null, "Routine notification from stakeholder")]
    [InlineData("Identified during data reconciliation with stakeholder", "", "Identified during data reconciliation with stakeholder")]
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

        var @event = new MandatoryQualificationCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                mq,
                providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationCreating, changeReason: changeReason, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification added");

        var reasonBlock = entry.GetElementByTestId("change-reason");
        Assert.NotNull(reasonBlock);

        var reasonBlockSummary = reasonBlock.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Reason for adding mandatory qualification", reasonBlockSummary?.TrimmedText());

        reasonBlock.AssertSummaryListRowValueContentMatches("Reason details", expectedReasonDetails);
    }

    [Fact]
    public async Task WithoutChangeReason_DoesNotRenderReason()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var @event = new MandatoryQualificationCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                mq,
                providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationCreating, changeReason: null, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification added");
        Assert.Null(entry.GetElementByTestId("change-reason"));
    }

    [Fact]
    public async Task WithEmptyChangeReason_DoesNotRenderReason()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = null,
            Details = null,
            AdditionalInformation = null,
            EvidenceFile = null
        };

        var @event = new MandatoryQualificationCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = EventModels.MandatoryQualification.FromModel(
                mq,
                providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null)
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationCreating, changeReason: changeReason, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification added");
        Assert.Null(entry.GetElementByTestId("change-reason"));
    }
}
