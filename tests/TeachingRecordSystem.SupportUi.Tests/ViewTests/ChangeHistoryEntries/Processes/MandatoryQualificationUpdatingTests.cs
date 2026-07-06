using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries.Processes;

public class MandatoryQualificationUpdatingTests(HostFixture hostFixture) : ChangeHistoryEntryTestBase(hostFixture)
{
    [Fact]
    public async Task ProcessRendersReasonAndChangedFieldInPreviousData()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithSpecialism(MandatoryQualificationSpecialism.Hearing)));
        var mq = person.MandatoryQualifications.Single();

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

        var reasonBlock = entry.GetElementByTestId("reason-for-change");
        Assert.NotNull(reasonBlock);
        Assert.Equal(changeReason.Reason, reasonBlock.GetElementByTestId("change-reason")?.TrimmedText());
        Assert.Equal(changeReason.Details, reasonBlock.GetElementByTestId("change-reason-detail")?.TrimmedText());

        var previousData = entry.GetElementByTestId("previous-data");
        Assert.NotNull(previousData);
        Assert.Equal(MandatoryQualificationSpecialism.Hearing.GetTitle(), previousData.GetElementByTestId("specialism")?.TrimmedText());
        // Only the changed field is shown in the previous data.
        Assert.Null(previousData.GetElementByTestId("provider"));
        Assert.Null(previousData.GetElementByTestId("start-date"));
    }

    [Fact]
    public async Task WithoutChangeReason_DoesNotRenderReasonBlock()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q
            .WithSpecialism(MandatoryQualificationSpecialism.Hearing)));
        var mq = person.MandatoryQualifications.Single();

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
        Assert.NotNull(entry.GetElementByTestId("previous-data"));
    }
}
