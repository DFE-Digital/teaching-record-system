using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Services.ChangeHistory;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

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
    public async Task Person_WithMandatoryQualficationDqtDeactivatedEvent_RendersExpectedContent()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        TimeProvider.Advance(TimeSpan.FromDays(1));
        var deactivatedByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        await DeactivateMq(mq.QualificationId, deactivatedByDqtUser);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-dqt-deactivated-event"),
            item =>
            {
                Assert.Equal($"By {deactivatedByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TrimmedText());
                Assert.Equal(TimeProvider.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
                Assert.Equal(mq.Provider!.Name, item.GetElementByTestId("provider")?.TrimmedText());
                Assert.Equal(mq.Specialism!.Value.GetTitle(), item.GetElementByTestId("specialism")?.TrimmedText());
                Assert.Equal(mq.StartDate!.Value.ToString(WebConstants.DateDisplayFormat), item.GetElementByTestId("start-date")?.TrimmedText());
                Assert.Equal(mq.Status!.Value.GetTitle(), item.GetElementByTestId("status")?.TrimmedText());
                Assert.Equal(mq.EndDate!.Value.ToString(WebConstants.DateDisplayFormat), item.GetElementByTestId("end-date")?.TrimmedText());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualficationDqtDeactivatedEventWithLegacyProvider_RendersExpectedProviderName()
    {
        // Arrange
        var (personId, mq, legacyProvider) = await CreateMqWithLegacyProvider();
        TimeProvider.Advance(TimeSpan.FromDays(1));
        var deactivatedByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        await DeactivateMq(mq.QualificationId, deactivatedByDqtUser);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-dqt-deactivated-event"),
            item => Assert.Equal(legacyProvider.Name, item.GetElementByTestId("provider")?.TrimmedText()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualficationDqtDeactivatedEventWithNoProvider_RendersExpectedContent()
    {
        // Arrange
        var (personId, mq) = await CreateMqWithoutProvider();
        TimeProvider.Advance(TimeSpan.FromDays(1));
        var deactivatedByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        await DeactivateMq(mq.QualificationId, deactivatedByDqtUser);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-dqt-deactivated-event"),
            item => Assert.Equal("None", item.GetElementByTestId("provider")?.TrimmedText()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationImportedEvent_RendersExpectedContent()
    {
        // Arrange
        var importedByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");

        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithImportedByDqtUser(importedByDqtUser)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-dqt-imported-event"),
            item =>
            {
                Assert.Equal($"By {importedByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TrimmedText());
                Assert.Equal(TimeProvider.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationDqtReactivatedEvent_RendersExpectedContent()
    {
        // Arrange
        var (personId, mq, legacyProvider) = await CreateMqWithLegacyProvider();
        TimeProvider.Advance(TimeSpan.FromDays(1));
        var dqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        await DeactivateMq(mq.QualificationId, dqtUser);
        TimeProvider.Advance(TimeSpan.FromDays(1));

        await WithDbContextAsync(async dbContext =>
        {
            var now = TimeProvider.UtcNow;

            var qualification = await dbContext.MandatoryQualifications.IgnoreQueryFilters().SingleAsync(q => q.QualificationId == mq.QualificationId);
            qualification.DeletedOn = null;

            var mqEstablishment = qualification.DqtMqEstablishmentValue is string mqEstablishmentValue ?
                LegacyDataCache.Instance.GetMqEstablishmentByValue(mqEstablishmentValue) :
                null;

            var reactivatedEvent = new MandatoryQualificationDqtReactivatedEvent
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

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEvent_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        TimeProvider.Advance(TimeSpan.FromDays(1));

        await WithDbContextAsync(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications
                .Where(q => q.PersonId == person.PersonId)
                .SingleAsync();

            var migratedEvent = new MandatoryQualificationMigratedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = TimeProvider.UtcNow,
                Key = $"{mq.QualificationId}-migrated",
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                MandatoryQualification = EventModels.MandatoryQualification.FromModel(mq),
                Changes = MandatoryQualificationMigratedEventChanges.None
            };
            dbContext.AddEventWithoutBroadcast(migratedEvent);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-migrated-event"),
            item =>
            {
                Assert.Equal($"By {SystemUser.SystemUserName} on", item.GetElementByTestId("raised-by")?.TrimmedText());
                Assert.Equal(TimeProvider.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEventWithNoChanges_DoesNotRenderPreviousDataSummaryList()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        TimeProvider.Advance(TimeSpan.FromDays(1));

        await WithDbContextAsync(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications
                .Where(q => q.PersonId == person.PersonId)
                .SingleAsync();

            var migratedEvent = new MandatoryQualificationMigratedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = TimeProvider.UtcNow,
                Key = $"{mq.QualificationId}-migrated",
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                MandatoryQualification = EventModels.MandatoryQualification.FromModel(mq),
                Changes = MandatoryQualificationMigratedEventChanges.None
            };
            dbContext.AddEventWithoutBroadcast(migratedEvent);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-migrated-event"),
            item => Assert.Null(item.GetElementByTestId("previous-data")));
    }
    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEventWithChangedProvider_RendersProviderRowInPreviousDataSummaryList()
    {
        // Arrange
        var establishmentWithProviderMapping = LegacyDataCache.Instance.GetMqEstablishmentByValue("150");
        var person = await TestData.CreatePersonAsync(b => b
            .WithMandatoryQualification(q => q
                .WithDqtMqEstablishment(establishmentWithProviderMapping.Value)));
        TimeProvider.Advance(TimeSpan.FromDays(1));

        var migratedProvider = await WithDbContextAsync(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications
                .Where(q => q.PersonId == person.PersonId)
                .SingleAsync();

            MandatoryQualificationProvider.TryMapFromDqtMqEstablishmentValue(establishmentWithProviderMapping.Value, out var migratedProvider);
            Debug.Assert(migratedProvider is not null);
            mq.ProviderId = migratedProvider.MandatoryQualificationProviderId;
            await dbContext.SaveChangesAsync();

            var migratedEvent = new MandatoryQualificationMigratedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = TimeProvider.UtcNow,
                Key = $"{mq.QualificationId}-migrated",
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                MandatoryQualification = EventModels.MandatoryQualification.FromModel(mq),
                Changes = MandatoryQualificationMigratedEventChanges.Provider
            };
            dbContext.AddEventWithoutBroadcast(migratedEvent);

            await dbContext.SaveChangesAsync();

            return migratedProvider;
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-migrated-event"),
            item => Assert.Equal(migratedProvider?.Name, item.GetElementByTestId("provider")?.TrimmedText()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEventWithChangedSpecialism_RendersSpecialismRowInPreviousDataSummaryList()
    {
        // Arrange
        var specialism = MandatoryQualificationSpecialism.DeafEducation;
        var establishmentWithSpecialismMapping = LegacyDataCache.Instance.GetMqEstablishmentByValue("961");  // University of Manchester
        var person = await TestData.CreatePersonAsync(b => b
            .WithMandatoryQualification(q => q
                .WithSpecialism(specialism)
                .WithDqtMqEstablishment(establishmentWithSpecialismMapping.Value)));
        TimeProvider.Advance(TimeSpan.FromDays(1));

        var migratedSpecialism = await WithDbContextAsync(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications
                .Where(q => q.PersonId == person.PersonId)
                .SingleAsync();

            MandatoryQualificationSpecialismRegistry.TryMapFromDqtSpecialism(
                establishmentWithSpecialismMapping.Value,
                specialism.GetDqtValue(),
                out var migratedSpecialism);
            Debug.Assert(migratedSpecialism is not null);
            mq.Specialism = migratedSpecialism;

            var migratedEvent = new MandatoryQualificationMigratedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = TimeProvider.UtcNow,
                Key = $"{mq.QualificationId}-migrated",
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                MandatoryQualification = EventModels.MandatoryQualification.FromModel(mq),
                Changes = MandatoryQualificationMigratedEventChanges.Specialism
            };
            dbContext.AddEventWithoutBroadcast(migratedEvent);

            await dbContext.SaveChangesAsync();

            return migratedSpecialism;
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-migrated-event"),
            item => Assert.Equal(migratedSpecialism?.GetTitle(), item.GetElementByTestId("specialism")?.TrimmedText()));
    }

    private async Task<(Guid PersonId, MandatoryQualification MandatoryQualification)> CreateFullyPopulatedMq(
        MandatoryQualificationStatus? status = null)
    {
        var person = await TestData.CreatePersonAsync(b => b
            .WithMandatoryQualification(q => q.WithStatus(status ?? MandatoryQualificationStatus.Passed)));

        var mq = await WithDbContextAsync(dbContext => dbContext.MandatoryQualifications
            .SingleAsync(q => q.QualificationId == person.MandatoryQualifications.Single().QualificationId));

        Debug.Assert(mq.ProviderId.HasValue);
        Debug.Assert(mq.Specialism.HasValue);
        Debug.Assert(mq.StartDate.HasValue);
        if (mq.Status == MandatoryQualificationStatus.Passed || mq.Status == MandatoryQualificationStatus.Failed)
        {
            Debug.Assert(mq.EndDate.HasValue);
        }
        else
        {
            Debug.Assert(!mq.EndDate.HasValue);
        }
        Debug.Assert(mq.Status.HasValue);

        return (person.PersonId, mq);
    }

    private async Task<(Guid PersonId, MandatoryQualification MandatoryQualification)> CreateMqWithoutProvider()
    {
        var person = await TestData.CreatePersonAsync(b => b
            .WithMandatoryQualification(q => q
                .WithProvider(null)));

        var mq = await WithDbContextAsync(dbContext => dbContext.MandatoryQualifications
            .SingleAsync(q => q.QualificationId == person.MandatoryQualifications.Single().QualificationId));

        Debug.Assert(!mq.DqtMqEstablishmentId.HasValue);
        Debug.Assert(!mq.ProviderId.HasValue);

        return (person.PersonId, mq);
    }

    private async Task<(Guid PersonId, MandatoryQualification MandatoryQualification, LegacyDataCache.MqEstablishment MqEstablishment)> CreateMqWithLegacyProvider()
    {
        var legacyProvider = LegacyDataCache.Instance.GetAllMqEstablishments().SingleRandom();

        var person = await TestData.CreatePersonAsync(b => b
            .WithMandatoryQualification(q => q
                .WithDqtMqEstablishment(legacyProvider.Value, mandatoryQualificationProviderId: null)));

        var mq = await WithDbContextAsync(dbContext => dbContext.MandatoryQualifications
            .SingleAsync(q => q.QualificationId == person.MandatoryQualifications.Single().QualificationId));

        Debug.Assert(mq.DqtMqEstablishmentValue is not null);
        Debug.Assert(!mq.ProviderId.HasValue);

        return (person.PersonId, mq, legacyProvider);
    }

    private Task DeactivateMq(Guid qualificationId, EventModels.RaisedByUserInfo deactivatedBy) => WithDbContextAsync(async dbContext =>
    {
        if (!deactivatedBy.IsDqtUser)
        {
            throw new ArgumentException($"{nameof(deactivatedBy)} should be a DQT user.", nameof(deactivatedBy));
        }

        var now = TimeProvider.UtcNow;

        var qualification = await dbContext.MandatoryQualifications
            .SingleAsync(q => q.QualificationId == qualificationId);

        qualification.DeletedOn = now;

        var mqEstablishment = qualification.DqtMqEstablishmentValue is string mqEstablishmentValue ?
            LegacyDataCache.Instance.GetMqEstablishmentByValue(mqEstablishmentValue) :
            null;

        var deletedEvent = new MandatoryQualificationDqtDeactivatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = deactivatedBy,
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
        dbContext.AddEventWithoutBroadcast(deletedEvent);

        await dbContext.SaveChangesAsync();
    });
}
