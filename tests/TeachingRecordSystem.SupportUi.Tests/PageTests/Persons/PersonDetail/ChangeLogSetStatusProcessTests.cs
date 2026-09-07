using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogSetStatusProcessTests : TestBase
{
    public ChangeLogSetStatusProcessTests(HostFixture hostFixture) : base(hostFixture)
    {
        // Toggle between GMT and BST to ensure we're testing rendering dates in local time
        var nows = new[]
        {
            new DateTime(2024, 1, 1, 12, 13, 14, DateTimeKind.Utc),  // GMT
            new DateTime(2024, 7, 5, 19, 20, 21, DateTimeKind.Utc)   // BST
        };
        TimeProvider.SetUtcNow(new DateTimeOffset(nows.SingleRandom(), TimeSpan.Zero));
    }

    [Fact]
    public async Task Person_WithPersonDeactivatingProcess_RendersExpectedContent()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var reason = PersonDeactivateReason.AnotherReason.GetDisplayName();
        var reasonDetail = "Reason detail";
        var additionalInformation = "this is additional information";
        var evidenceFile = new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonDeactivating,
            user.UserId,
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = reason,
                Details = reasonDetail,
                EvidenceFile = evidenceFile,
                AdditionalInformation = additionalInformation
            },
            new PersonDeactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Changes = PersonDeactivatedEventChanges.PersonStatus,
                MergedWithPersonId = null,
                DateOfDeath = null
            });

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Record deactivated",
            user.Name,
            process.CreatedOn);

        doc.AssertSummaryListRowValue("change-reason", "Reason", v => Assert.Equal(reason, v.TrimmedText()));
        doc.AssertSummaryListRowValue("change-reason", "Reason details", v => Assert.Equal(reasonDetail, v.TrimmedText()));
        doc.AssertSummaryListRowValue("change-reason", "Additional information", v => Assert.Equal(additionalInformation, v.TrimmedText()));
        doc.AssertSummaryListRowValue("change-reason", "Evidence", v => Assert.Equal($"{evidenceFile.Name} (opens in new tab)", v.TrimmedText()));
    }

    [Fact]
    public async Task Person_WithPersonReactivatingProcess_RendersExpectedContent()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var reason = PersonReactivateReason.AnotherReason.GetDisplayName();
        var reasonDetail = "Reason detail";
        var additionalInformation = "this is additional information";
        var evidenceFile = new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonReactivating,
            user.UserId,
            new ChangeReasonWithDetailsAndEvidence
            {
                Reason = reason,
                Details = reasonDetail,
                EvidenceFile = evidenceFile,
                AdditionalInformation = additionalInformation
            },
            new PersonReactivatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Changes = PersonReactivatedEventChanges.PersonStatus
            });

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Record reactivated",
            user.Name,
            process.CreatedOn);

        doc.AssertSummaryListRowValue("change-reason", "Reason", v => Assert.Equal(reason, v.TrimmedText()));
        doc.AssertSummaryListRowValue("change-reason", "Reason details", v => Assert.Equal(reasonDetail, v.TrimmedText()));
        doc.AssertSummaryListRowValue("change-reason", "Additional information", v => Assert.Equal(additionalInformation, v.TrimmedText()));
        doc.AssertSummaryListRowValue("change-reason", "Evidence", v => Assert.Equal($"{evidenceFile.Name} (opens in new tab)", v.TrimmedText()));
    }
}
