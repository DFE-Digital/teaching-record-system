using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Services.ChangeHistory;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogMandatoryQualificationEventsTests : TestBase
{
    public ChangeLogMandatoryQualificationEventsTests(HostFixture hostFixture) : base(hostFixture)
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
    public async Task Person_WithMandatoryQualificationDqtReactivatedEvent_RendersExpectedContent()
    {
        // Arrange
        var (personId, mq) = await CreateMqWithLegacyProvider();
        TimeProvider.Advance(TimeSpan.FromDays(1));
        var dqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");

        await WithDbContextAsync(async dbContext =>
        {
            var now = TimeProvider.UtcNow;

            var qualification = await dbContext.MandatoryQualifications.IgnoreQueryFilters().SingleAsync(q => q.QualificationId == mq.QualificationId);

            var mqEstablishment = qualification.DqtMqEstablishmentValue is string mqEstablishmentValue ?
                LegacyDataCache.Instance.GetMqEstablishmentByValue(mqEstablishmentValue) :
                null;

            var reactivatedEvent = new LegacyEvents.MandatoryQualificationDqtReactivatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = dqtUser,
                PersonId = qualification.PersonId,
                MandatoryQualification = new()
                {
                    QualificationId = qualification.QualificationId,
                    Provider = qualification.ProviderId is not null || mqEstablishment is not null ?
                        new EventModels.MandatoryQualificationProvider
                        {
                            MandatoryQualificationProviderId = qualification.ProviderId,
                            Name = qualification.ProviderId is not null ?
                                qualification.Provider?.Name ?? throw new InvalidOperationException($"Missing {nameof(qualification.Provider)}.") :
                                null,
                            DqtMqEstablishmentName = mqEstablishment?.Name,
                            DqtMqEstablishmentValue = mqEstablishment?.Value
                        } :
                        null,
                    Specialism = qualification.Specialism,
                    Status = qualification.Status,
                    StartDate = qualification.StartDate,
                    EndDate = qualification.EndDate
                }
            };
            dbContext.AddEventWithoutBroadcast(reactivatedEvent);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-dqt-reactivated-event"),
            item =>
            {
                Assert.Equal($"By {dqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TrimmedText());
                Assert.Equal(TimeProvider.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
            });
    }

    private async Task<(Guid PersonId, MandatoryQualification MandatoryQualification)> CreateMqWithLegacyProvider()
    {
        var legacyProvider = LegacyDataCache.Instance.GetAllMqEstablishments().SingleRandom();

        var person = await TestData.CreatePersonAsync(b => b
            .WithMandatoryQualification(q => q
                .WithDqtMqEstablishment(legacyProvider.Value, mandatoryQualificationProviderId: null)));

        var mq = await WithDbContextAsync(dbContext => dbContext.MandatoryQualifications
            .SingleAsync(q => q.QualificationId == person.Qualifications!.OfType<MandatoryQualification>().Single().QualificationId));

        Debug.Assert(mq.DqtMqEstablishmentValue is not null);
        Debug.Assert(!mq.ProviderId.HasValue);

        return (person.PersonId, mq);
    }
}
