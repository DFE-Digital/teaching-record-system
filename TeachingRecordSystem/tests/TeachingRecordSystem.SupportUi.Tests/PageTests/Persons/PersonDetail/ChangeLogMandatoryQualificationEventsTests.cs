using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;
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
        Clock.UtcNow = nows.RandomOne();
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationCreatedEvent_RendersExpectedContent()
    {
        // Arrange
        var createdByUser = await TestData.CreateUser();
        var (personId, mq) = await CreateFullyPopulatedMq(createdByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-created-event"),
            item =>
            {
                Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationCreatedEventCreatedByDqtUser_RendersExpectedRaisedBy()
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var (personId, mq) = await CreateFullyPopulatedMq(createdByDqtUser);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-created-event"),
            item => Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationDeletedEvent_RendersExpectedContent()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        var deletedByUser = await TestData.CreateUser();
        var deletionReason = "Created in error";
        var deletionReasonDetail = "More information";
        var evidenceFile = (FileId: Guid.NewGuid(), Name: "A file.jpeg");
        await TestData.DeleteMandatoryQualification(mq.QualificationId, deletedByUser.UserId, deletionReason, deletionReasonDetail, evidenceFile);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-deleted-event"),
            item =>
            {
                Assert.Equal($"By {deletedByUser.Name} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                Assert.Equal(deletionReason, item.GetElementByTestId("deletion-reason")?.TextContent.Trim());
                Assert.Equal(deletionReasonDetail, item.GetElementByTestId("deletion-reason-detail")?.TextContent.Trim());
                Assert.Equal($"{evidenceFile.Name} (opens in new tab)", item.GetElementByTestId("evidence")?.TextContent);
                Assert.Equal(mq.Provider!.Name, item.GetElementByTestId("provider")?.TextContent.Trim());
                Assert.Equal(mq.Specialism!.Value.GetTitle(), item.GetElementByTestId("specialism")?.TextContent.Trim());
                Assert.Equal(mq.StartDate!.Value.ToString("d MMMM yyyy"), item.GetElementByTestId("start-date")?.TextContent.Trim());
                Assert.Equal(mq.Status!.Value.GetTitle(), item.GetElementByTestId("status")?.TextContent.Trim());
                Assert.Equal(mq.EndDate!.Value.ToString("d MMMM yyyy"), item.GetElementByTestId("end-date")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationDeletedEventWithEmptyData_RendersExpectedContent()
    {
        // Arrange
        var (personId, mq) = await CreateEmptyMq();
        var deletedByUser = await TestData.CreateUser();
        var deletionReason = "Created in error";
        await TestData.DeleteMandatoryQualification(mq.QualificationId, deletedByUser.UserId, deletionReason);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-deleted-event"),
            item =>
            {
                Assert.Equal($"By {deletedByUser.Name} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                Assert.Equal(deletionReason, item.GetElementByTestId("deletion-reason")?.TextContent.Trim());
                Assert.Equal("None", item.GetElementByTestId("deletion-reason-detail")?.TextContent.Trim());
                Assert.Equal("None", item.GetElementByTestId("provider")?.TextContent.Trim());
                Assert.Equal("None", item.GetElementByTestId("specialism")?.TextContent.Trim());
                Assert.Equal("None", item.GetElementByTestId("start-date")?.TextContent.Trim());
                Assert.Equal("None", item.GetElementByTestId("status")?.TextContent.Trim());
                Assert.Equal("None", item.GetElementByTestId("end-date")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationDeletedEventWithNoEvidenceFile_DoesNotRenderEvidenceRow()
    {
        // Arrange
        var (personId, mq) = await CreateEmptyMq();
        var deletedByUser = await TestData.CreateUser();
        await TestData.DeleteMandatoryQualification(mq.QualificationId, deletedByUser.UserId, evidenceFile: null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-deleted-event"),
            item => Assert.Null(item.GetElementByTestId("evidence")));
    }

    [Fact]
    public async Task Person_WithDeletedMandatoryQualificationEventWithLegacyProvider_RendersExpectedProviderName()
    {
        // Arrange
        var (personId, mq, legacyProvider) = await CreateMqWithLegacyProvider();
        var deletedByUser = await TestData.CreateUser();
        await TestData.DeleteMandatoryQualification(mq.QualificationId, deletedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-deleted-event"),
            item => Assert.Equal(legacyProvider.dfeta_name, item.GetElementByTestId("provider")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithDeletedMandatoryQualificationEventWithNoProvider_RendersNoneForProviderName()
    {
        // Arrange
        var (personId, mq) = await CreateMqWithoutProvider();
        var deletedByUser = await TestData.CreateUser();
        await TestData.DeleteMandatoryQualification(mq.QualificationId, deletedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-deleted-event"),
            item => Assert.Equal("None", item.GetElementByTestId("provider")?.TextContent.Trim()));
    }

    //TODO DqtDeactivated

    [Fact]
    public async Task Person_WithMandatoryQualificationImportedEvent_RendersExpectedContent()
    {
        // Arrange
        var importedByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");

        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithImportedByDqtUser(importedByDqtUser)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-dqt-imported-event"),
            item =>
            {
                Assert.Equal($"By {importedByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
            });
    }

    //TODO DqtReactivated

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEvent_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        Clock.Advance();

        await WithDbContext(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications
                .Include(q => q.Provider)
                .Where(q => q.PersonId == person.PersonId)
                .SingleAsync();

            var migratedEvent = new MandatoryQualificationMigratedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow,
                Key = $"{mq.QualificationId}-migrated",
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                MandatoryQualification = EventModels.MandatoryQualification.FromModel(mq),
                Changes = MandatoryQualificationMigratedEventChanges.None
            };
            dbContext.AddEvent(migratedEvent);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-migrated-event"),
            item =>
            {
                Assert.Equal($"By {SystemUser.SystemUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEventWithNoChanges_DoesNotRenderPreviousDataSummaryList()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        Clock.Advance();

        await WithDbContext(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications
                .Include(q => q.Provider)
                .Where(q => q.PersonId == person.PersonId)
                .SingleAsync();

            var migratedEvent = new MandatoryQualificationMigratedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow,
                Key = $"{mq.QualificationId}-migrated",
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                MandatoryQualification = EventModels.MandatoryQualification.FromModel(mq),
                Changes = MandatoryQualificationMigratedEventChanges.None
            };
            dbContext.AddEvent(migratedEvent);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-migrated-event"),
            item => Assert.Null(item.GetElementByTestId("previous-data")));
    }
    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEventWithChangedProvider_RendersProviderRowInPreviousDataSummaryList()
    {
        // Arrange
        var establishmentWithProviderMapping = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("150");  // Postgraduate Diploma in Deaf Education, University of Manchester, School of Psychological Sciences
        var person = await TestData.CreatePerson(b => b
            .WithMandatoryQualification(q => q
                .WithDqtMqEstablishment(establishmentWithProviderMapping)));
        Clock.Advance();

        var migratedProvider = await WithDbContext(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications
                .Include(q => q.Provider)
                .Where(q => q.PersonId == person.PersonId)
                .SingleAsync();

            MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(establishmentWithProviderMapping, out var migratedProvider);
            Debug.Assert(migratedProvider is not null);
            mq.ProviderId = migratedProvider.MandatoryQualificationProviderId;
            await dbContext.SaveChangesAsync();

            var migratedEvent = new MandatoryQualificationMigratedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow,
                Key = $"{mq.QualificationId}-migrated",
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                MandatoryQualification = EventModels.MandatoryQualification.FromModel(mq),
                Changes = MandatoryQualificationMigratedEventChanges.Provider
            };
            dbContext.AddEvent(migratedEvent);

            await dbContext.SaveChangesAsync();

            return migratedProvider;
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-migrated-event"),
            item => Assert.Equal(migratedProvider?.Name, item.GetElementByTestId("provider")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEventWithChangedSpecialism_RendersSpecialismRowInPreviousDataSummaryList()
    {
        // Arrange
        var specialism = MandatoryQualificationSpecialism.DeafEducation;
        var establishmentWithSpecialismMapping = await TestData.ReferenceDataCache.GetMqEstablishmentByValue("961");  // University of Manchester
        var person = await TestData.CreatePerson(b => b
            .WithMandatoryQualification(q => q
                .WithSpecialism(specialism)
                .WithDqtMqEstablishment(establishmentWithSpecialismMapping)));
        Clock.Advance();

        var migratedSpecialism = await WithDbContext(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications
                .Include(q => q.Provider)
                .Where(q => q.PersonId == person.PersonId)
                .SingleAsync();

            MandatoryQualificationSpecialismRegistry.TryMapFromDqtMqEstablishment(
                establishmentWithSpecialismMapping.dfeta_Value,
                specialism.GetDqtValue(),
                out var migratedSpecialism);
            Debug.Assert(migratedSpecialism is not null);
            mq.Specialism = migratedSpecialism;

            var migratedEvent = new MandatoryQualificationMigratedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = Clock.UtcNow,
                Key = $"{mq.QualificationId}-migrated",
                RaisedBy = SystemUser.SystemUserId,
                PersonId = person.PersonId,
                MandatoryQualification = EventModels.MandatoryQualification.FromModel(mq),
                Changes = MandatoryQualificationMigratedEventChanges.Specialism
            };
            dbContext.AddEvent(migratedEvent);

            await dbContext.SaveChangesAsync();

            return migratedSpecialism;
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-migrated-event"),
            item => Assert.Equal(migratedSpecialism?.GetTitle(), item.GetElementByTestId("specialism")?.TextContent.Trim()));
    }


    //TODO Updated

    private async Task<(Guid PersonId, MandatoryQualification MandatoryQualification)> CreateFullyPopulatedMq(EventModels.RaisedByUserInfo? createdByUser = null)
    {
        var person = await TestData.CreatePerson(b => b
            .WithMandatoryQualification(q =>
            {
                q.WithStatus(MandatoryQualificationStatus.Passed);

                if (createdByUser is not null)
                {
                    q.WithCreatedByUser(createdByUser);
                }
            }));

        var mq = await WithDbContext(dbContext => dbContext.MandatoryQualifications
            .Include(q => q.Provider)
            .SingleAsync(q => q.QualificationId == person.MandatoryQualifications.Single().QualificationId));

        Debug.Assert(mq.ProviderId.HasValue);
        Debug.Assert(mq.Specialism.HasValue);
        Debug.Assert(mq.StartDate.HasValue);
        Debug.Assert(mq.Status.HasValue);
        Debug.Assert(mq.EndDate.HasValue);

        return (person.PersonId, mq);
    }

    private async Task<(Guid PersonId, MandatoryQualification MandatoryQualification)> CreateEmptyMq()
    {
        var person = await TestData.CreatePerson(b => b
            .WithMandatoryQualification(q => q
                .WithStartDate(null)
                .WithSpecialism(null)
                .WithStatus(null)
                .WithProvider(null)));

        var mq = await WithDbContext(dbContext => dbContext.MandatoryQualifications
            .Include(q => q.Provider)
            .SingleAsync(q => q.QualificationId == person.MandatoryQualifications.Single().QualificationId));

        Debug.Assert(!mq.DqtMqEstablishmentId.HasValue);
        Debug.Assert(!mq.ProviderId.HasValue);
        Debug.Assert(!mq.Specialism.HasValue);
        Debug.Assert(!mq.StartDate.HasValue);
        Debug.Assert(!mq.Status.HasValue);
        Debug.Assert(!mq.EndDate.HasValue);

        return (person.PersonId, mq);
    }

    private async Task<(Guid PersonId, MandatoryQualification MandatoryQualification)> CreateMqWithoutProvider()
    {
        var person = await TestData.CreatePerson(b => b
            .WithMandatoryQualification(q => q
                .WithProvider(null)));

        var mq = await WithDbContext(dbContext => dbContext.MandatoryQualifications
            .Include(q => q.Provider)
            .SingleAsync(q => q.QualificationId == person.MandatoryQualifications.Single().QualificationId));

        Debug.Assert(!mq.DqtMqEstablishmentId.HasValue);
        Debug.Assert(!mq.ProviderId.HasValue);

        return (person.PersonId, mq);
    }

    private async Task<(Guid PersonId, MandatoryQualification MandatoryQualification, dfeta_mqestablishment MqEstablishment)> CreateMqWithLegacyProvider()
    {
        var legacyProvider = (await TestData.ReferenceDataCache.GetMqEstablishments()).RandomOne();

        var person = await TestData.CreatePerson(b => b
            .WithMandatoryQualification(q => q
                .WithDqtMqEstablishment(legacyProvider, mandatoryQualificationProviderId: null)));

        var mq = await WithDbContext(dbContext => dbContext.MandatoryQualifications
            .Include(q => q.Provider)
            .SingleAsync(q => q.QualificationId == person.MandatoryQualifications.Single().QualificationId));

        Debug.Assert(mq.DqtMqEstablishmentId.HasValue);
        Debug.Assert(!mq.ProviderId.HasValue);

        return (person.PersonId, mq, legacyProvider);
    }
}
