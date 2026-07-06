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
        var mq = person.MandatoryQualifications.Single();

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

        var reasonBlock = entry.GetElementByTestId("reason-for-change");
        Assert.NotNull(reasonBlock);
        Assert.Equal(changeReason.Reason, reasonBlock.GetElementByTestId("deletion-reason")?.TrimmedText());
        Assert.Equal(changeReason.Details, reasonBlock.GetElementByTestId("deletion-reason-detail")?.TrimmedText());
        Assert.Equal(changeReason.AdditionalInformation, reasonBlock.GetElementByTestId("additional-information")?.TrimmedText());

        var deletedData = entry.GetElementByTestId("deleted-data");
        Assert.NotNull(deletedData);
        Assert.Equal(provider.Name, deletedData.GetElementByTestId("provider")?.TrimmedText());
        Assert.Equal(specialism.GetTitle(), deletedData.GetElementByTestId("specialism")?.TrimmedText());
        Assert.Equal(startDate.ToString(WebConstants.DateDisplayFormat), deletedData.GetElementByTestId("start-date")?.TrimmedText());
        Assert.Equal(status.GetTitle(), deletedData.GetElementByTestId("status")?.TrimmedText());
        Assert.Equal(endDate.ToString(WebConstants.DateDisplayFormat), deletedData.GetElementByTestId("end-date")?.TrimmedText());
    }
}
