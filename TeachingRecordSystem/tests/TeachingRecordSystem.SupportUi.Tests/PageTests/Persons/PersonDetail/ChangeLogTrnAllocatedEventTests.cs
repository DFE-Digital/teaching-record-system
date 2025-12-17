using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogTrnAllocatedEventTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task ChangeLogDisplaysTrnAllocatedEvent()
    {
        // arrange
        var person = await TestData.CreatePersonAsync();
        var trn = person.Person.Trn;
        var trnAllocatedEvent = new TrnAllocatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = SystemUser.SystemUserId,
            PersonId = person.PersonId,
            Trn = trn
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(trnAllocatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-trn-allocated-event");
        Assert.NotNull(item);
        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);
        Assert.Equal("TRN allocated", title.TrimmedText());
        Assert.Equal($"By {SystemUser.SystemUserName} on", item.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());

        doc.AssertSummaryListRowValue("Trn", v => Assert.Equal(trn, v.TrimmedText()));
    }
}
