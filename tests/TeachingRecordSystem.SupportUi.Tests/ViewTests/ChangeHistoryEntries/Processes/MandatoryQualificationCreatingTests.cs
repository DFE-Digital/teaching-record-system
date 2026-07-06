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
        var mq = person.MandatoryQualifications.Single();

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
        Assert.Equal(provider.Name, entry.GetElementByTestId("provider")?.TrimmedText());
        Assert.Equal(specialism.GetTitle(), entry.GetElementByTestId("specialism")?.TrimmedText());
        Assert.Equal(startDate.ToString(WebConstants.DateDisplayFormat), entry.GetElementByTestId("start-date")?.TrimmedText());
        Assert.Equal(status.GetTitle(), entry.GetElementByTestId("status")?.TrimmedText());
        Assert.Equal(endDate.ToString(WebConstants.DateDisplayFormat), entry.GetElementByTestId("end-date")?.TrimmedText());
        Assert.Null(entry.GetElementByTestId("reason-for-change"));
    }

    [Fact]
    public async Task WithChangeReason_RendersReason()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var mq = person.MandatoryQualifications.Single();

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
        var reasonBlock = entry.GetElementByTestId("reason-for-change");
        Assert.NotNull(reasonBlock);
        Assert.Equal(changeReason.Reason, reasonBlock.GetElementByTestId("reason")?.TrimmedText());
        Assert.Equal(changeReason.Details, reasonBlock.GetElementByTestId("reason-detail")?.TrimmedText());
        Assert.Equal(changeReason.AdditionalInformation, reasonBlock.GetElementByTestId("additional-information")?.TrimmedText());
        Assert.Equal($"{changeReason.EvidenceFile.Name} (opens in new tab)", reasonBlock.GetElementByTestId("evidence")?.TrimmedText());
    }
}
