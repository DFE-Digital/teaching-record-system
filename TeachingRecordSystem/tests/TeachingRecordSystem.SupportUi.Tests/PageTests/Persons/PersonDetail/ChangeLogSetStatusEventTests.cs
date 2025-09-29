using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogSetStatusEventTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Before(Test)]
    public void Initialize()
    {
        // Toggle between GMT and BST to ensure we're testing rendering dates in local time
        var nows = new[]
        {
            new DateTime(2024, 1, 1, 12, 13, 14, DateTimeKind.Utc),  // GMT
            new DateTime(2024, 7, 5, 19, 20, 21, DateTimeKind.Utc)   // BST
        };
        Clock.UtcNow = nows.SingleRandom();
    }

    [Test]
    public async Task Person_WithPersonStatusUpdatedEvent_Deactivated_RendersExpectedContent()
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var reason = DeactivateReasonOption.AnotherReason.GetDisplayName();
        var reasonDetail = "Reason detail";
        var evidenceFile = new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        };

        var statusUpdatedEvent = new PersonStatusUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            Status = PersonStatus.Deactivated,
            OldStatus = PersonStatus.Active,
            Reason = reason,
            ReasonDetail = reasonDetail,
            EvidenceFile = evidenceFile,
            DateOfDeath = null
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(statusUpdatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-status-updated-event");
        Assert.NotNull(item);

        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);
        Assert.Equal($"Record deactivated", title.TrimmedText());

        Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());

        doc.AssertRow("change-reason", "Reason", v => Assert.Equal(reason, v.TrimmedText()));
        doc.AssertRow("change-reason", "Reason details", v => Assert.Equal(reasonDetail, v.TrimmedText()));
        doc.AssertRow("change-reason", "Evidence", v => Assert.Equal($"{evidenceFile!.Name} (opens in new tab)", v.TrimmedText()));
    }

    [Test]
    public async Task Person_WithPersonStatusUpdatedEvent_Reactivated_RendersExpectedContent()
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var reason = ReactivateReasonOption.AnotherReason.GetDisplayName();
        var reasonDetail = "Reason detail";
        var evidenceFile = new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        };

        var statusUpdatedEvent = new PersonStatusUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            Status = PersonStatus.Active,
            OldStatus = PersonStatus.Deactivated,
            Reason = reason,
            ReasonDetail = reasonDetail,
            EvidenceFile = evidenceFile,
            DateOfDeath = null
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(statusUpdatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-status-updated-event");
        Assert.NotNull(item);

        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);
        Assert.Equal($"Record reactivated", title.TrimmedText());

        Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());

        doc.AssertRow("change-reason", "Reason", v => Assert.Equal(reason, v.TrimmedText()));
        doc.AssertRow("change-reason", "Reason details", v => Assert.Equal(reasonDetail, v.TrimmedText()));
        doc.AssertRow("change-reason", "Evidence", v => Assert.Equal($"{evidenceFile!.Name} (opens in new tab)", v.TrimmedText()));
    }
}
