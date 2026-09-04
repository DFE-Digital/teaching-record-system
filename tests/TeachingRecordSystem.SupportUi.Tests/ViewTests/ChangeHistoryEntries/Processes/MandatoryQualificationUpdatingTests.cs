using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class MandatoryQualificationUpdatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersChangedFieldsAsText()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithSpecialism(MandatoryQualificationSpecialism.Hearing)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var oldMandatoryQualification = EventModels.MandatoryQualification.FromModel(
            mq,
            providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null);
        var newMandatoryQualification = oldMandatoryQualification with { Specialism = MandatoryQualificationSpecialism.Visual };

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = "Correcting an error",
            Details = "Some reason details",
            AdditionalInformation = "Some additional information",
            EvidenceFile = new EventModels.File { FileId = Guid.NewGuid(), Name = "evidence.jpg" }
        };

        var @event = new MandatoryQualificationUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = newMandatoryQualification,
            OldMandatoryQualification = oldMandatoryQualification,
            Changes = MandatoryQualificationUpdatedEventChanges.Specialism
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationUpdating, changeReason: changeReason, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification changed");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Specialism changed from", bodyText);
        Assert.Contains(MandatoryQualificationSpecialism.Hearing.GetTitle(), bodyText);
        Assert.Contains(MandatoryQualificationSpecialism.Visual.GetTitle(), bodyText);

        var reasonBlock = entry.GetElementByTestId("reason-for-change");
        Assert.NotNull(reasonBlock);
        Assert.Equal(changeReason.Reason, reasonBlock.GetElementByTestId("change-reason")?.TrimmedText());

        Assert.Null(entry.GetElementByTestId("previous-data"));
    }

    [Theory]
    [InlineData("Another reason", "Some reason details", "Another reason: Some reason details")]
    [InlineData("Correcting an error", null, "Correcting an error")]
    [InlineData("Correcting an error", "", "Correcting an error")]
    public async Task WithChangeReason_RendersCorrectly(string reason, string? details, string expectedReasonDetails)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithSpecialism(MandatoryQualificationSpecialism.Hearing)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var oldMandatoryQualification = EventModels.MandatoryQualification.FromModel(
            mq,
            providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null);
        var newMandatoryQualification = oldMandatoryQualification with { Specialism = MandatoryQualificationSpecialism.Visual };

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = reason,
            Details = details,
            AdditionalInformation = null,
            EvidenceFile = null
        };

        var @event = new MandatoryQualificationUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = newMandatoryQualification,
            OldMandatoryQualification = oldMandatoryQualification,
            Changes = MandatoryQualificationUpdatedEventChanges.Specialism
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationUpdating, changeReason: changeReason, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification changed");

        var reasonBlock = entry.GetElementByTestId("reason-for-change");
        Assert.NotNull(reasonBlock);

        var reasonBlockSummary = reasonBlock.GetElementsByTagName("summary").SingleOrDefault();
        Assert.Equal("Reason for change", reasonBlockSummary?.TrimmedText());

        reasonBlock.AssertSummaryListRowValueContentMatches("Reason details", expectedReasonDetails);
    }

    [Fact]
    public async Task ProcessRendersProviderChangeAsText()
    {
        // Arrange
        var oldProvider = MandatoryQualificationProvider.All.First();
        var newProvider = MandatoryQualificationProvider.All.Last();

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithProvider(oldProvider.MandatoryQualificationProviderId)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var oldMandatoryQualification = EventModels.MandatoryQualification.FromModel(mq, providerNameHint: oldProvider.Name);
        var newMandatoryQualification = oldMandatoryQualification with
        {
            Provider = new EventModels.MandatoryQualificationProvider
            {
                MandatoryQualificationProviderId = newProvider.MandatoryQualificationProviderId,
                Name = newProvider.Name
            }
        };

        var @event = new MandatoryQualificationUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = newMandatoryQualification,
            OldMandatoryQualification = oldMandatoryQualification,
            Changes = MandatoryQualificationUpdatedEventChanges.Provider
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationUpdating, changeReason: null, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification changed");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Training provider changed from", bodyText);
        Assert.Contains(oldProvider.Name, bodyText);
        Assert.Contains(newProvider.Name, bodyText);
    }

    [Fact]
    public async Task ProcessRendersStartDateChangeAsText()
    {
        // Arrange
        var oldStartDate = new DateOnly(2020, 1, 1);
        var newStartDate = new DateOnly(2021, 2, 2);

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithStartDate(oldStartDate)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var oldMandatoryQualification = EventModels.MandatoryQualification.FromModel(
            mq,
            providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null);
        var newMandatoryQualification = oldMandatoryQualification with { StartDate = newStartDate };

        var @event = new MandatoryQualificationUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = newMandatoryQualification,
            OldMandatoryQualification = oldMandatoryQualification,
            Changes = MandatoryQualificationUpdatedEventChanges.StartDate
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationUpdating, changeReason: null, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification changed");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Start date changed from", bodyText);
        Assert.Contains(oldStartDate.ToString(WebConstants.DateDisplayFormat), bodyText);
        Assert.Contains(newStartDate.ToString(WebConstants.DateDisplayFormat), bodyText);
    }

    [Fact]
    public async Task ProcessRendersStatusChangeAsText()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithStatus(MandatoryQualificationStatus.InProgress, endDate: null)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var oldMandatoryQualification = EventModels.MandatoryQualification.FromModel(
            mq,
            providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null);
        var newMandatoryQualification = oldMandatoryQualification with { Status = MandatoryQualificationStatus.Passed };

        var @event = new MandatoryQualificationUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = newMandatoryQualification,
            OldMandatoryQualification = oldMandatoryQualification,
            Changes = MandatoryQualificationUpdatedEventChanges.Status
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationUpdating, changeReason: null, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification changed");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Status changed from", bodyText);
        Assert.Contains(MandatoryQualificationStatus.InProgress.GetTitle(), bodyText);
        Assert.Contains(MandatoryQualificationStatus.Passed.GetTitle(), bodyText);
    }

    [Fact]
    public async Task ProcessRendersEndDateChangeAsText()
    {
        // Arrange
        var oldEndDate = new DateOnly(2021, 1, 1);
        var newEndDate = new DateOnly(2022, 2, 2);

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithStatus(MandatoryQualificationStatus.Passed, oldEndDate)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var oldMandatoryQualification = EventModels.MandatoryQualification.FromModel(
            mq,
            providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null);
        var newMandatoryQualification = oldMandatoryQualification with { EndDate = newEndDate };

        var @event = new MandatoryQualificationUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = newMandatoryQualification,
            OldMandatoryQualification = oldMandatoryQualification,
            Changes = MandatoryQualificationUpdatedEventChanges.EndDate
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationUpdating, changeReason: null, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification changed");

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("End date changed from", bodyText);
        Assert.Contains(oldEndDate.ToString(WebConstants.DateDisplayFormat), bodyText);
        Assert.Contains(newEndDate.ToString(WebConstants.DateDisplayFormat), bodyText);
    }

    [Fact]
    public async Task WithoutChangeReason_DoesNotRenderReasonBlock()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithSpecialism(MandatoryQualificationSpecialism.Hearing)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var oldMandatoryQualification = EventModels.MandatoryQualification.FromModel(
            mq,
            providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null);
        var newMandatoryQualification = oldMandatoryQualification with { Specialism = MandatoryQualificationSpecialism.Visual };

        var @event = new MandatoryQualificationUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = newMandatoryQualification,
            OldMandatoryQualification = oldMandatoryQualification,
            Changes = MandatoryQualificationUpdatedEventChanges.Specialism
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationUpdating, changeReason: null, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        Assert.Null(entry.GetElementByTestId("reason-for-change"));

        var bodyText = entry.GetElementsByClassName("govuk-body").SingleOrDefault()?.TrimmedText();
        Assert.Contains("Specialism changed from", bodyText);
    }

    [Fact]
    public async Task WithEmptyChangeReason_DoesNotRenderReasonBlock()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithSpecialism(MandatoryQualificationSpecialism.Hearing)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var oldMandatoryQualification = EventModels.MandatoryQualification.FromModel(
            mq,
            providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null);
        var newMandatoryQualification = oldMandatoryQualification with { Specialism = MandatoryQualificationSpecialism.Visual };

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = null,
            Details = null,
            AdditionalInformation = null,
            EvidenceFile = null
        };

        var @event = new MandatoryQualificationUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = newMandatoryQualification,
            OldMandatoryQualification = oldMandatoryQualification,
            Changes = MandatoryQualificationUpdatedEventChanges.Specialism
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationUpdating, changeReason: changeReason, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification changed");
        Assert.Null(entry.GetElementByTestId("reason-for-change"));
    }

    [Fact]
    public async Task WithChangeReasonAndNoEvidence_DoesNotRenderEvidenceRow()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithSpecialism(MandatoryQualificationSpecialism.Hearing)));
        var mq = person.Qualifications!.OfType<MandatoryQualification>().Single();

        var oldMandatoryQualification = EventModels.MandatoryQualification.FromModel(
            mq,
            providerNameHint: mq.ProviderId is Guid providerId ? MandatoryQualificationProvider.GetById(providerId).Name : null);
        var newMandatoryQualification = oldMandatoryQualification with { Specialism = MandatoryQualificationSpecialism.Visual };

        var changeReason = new ChangeReasonWithDetailsAndEvidence
        {
            Reason = "Correcting an error",
            Details = "Some reason details",
            AdditionalInformation = "Some additional information",
            EvidenceFile = null
        };

        var @event = new MandatoryQualificationUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.PersonId,
            MandatoryQualification = newMandatoryQualification,
            OldMandatoryQualification = oldMandatoryQualification,
            Changes = MandatoryQualificationUpdatedEventChanges.Specialism
        };

        var process = await TestData.CreateProcessAsync(ProcessType.MandatoryQualificationUpdating, changeReason: changeReason, events: @event);

        // Act
        var entry = await GetEntryHtmlAsync(process.ProcessId);

        // Assert
        AssertTitle(entry, "Mandatory qualification changed");

        var reasonBlock = entry.GetElementByTestId("reason-for-change");
        Assert.NotNull(reasonBlock);
        Assert.Null(reasonBlock.GetElementByTestId("evidence"));
    }
}
