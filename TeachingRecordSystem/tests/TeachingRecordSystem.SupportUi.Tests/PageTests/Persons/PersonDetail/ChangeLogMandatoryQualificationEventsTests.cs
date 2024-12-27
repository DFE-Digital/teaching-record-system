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
        var createdByUser = await TestData.CreateUserAsync();
        var (personId, mq) = await CreateFullyPopulatedMq(createdByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

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
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-created-event"),
            item => Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationDeletedEvent_RendersExpectedContent()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        var deletedByUser = await TestData.CreateUserAsync();
        var deletionReason = "Created in error";
        var deletionReasonDetail = "More information";
        var evidenceFile = (FileId: Guid.NewGuid(), Name: "A file.jpeg");
        await TestData.DeleteMandatoryQualificationAsync(mq.QualificationId, deletedByUser.UserId, deletionReason, deletionReasonDetail, evidenceFile);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

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
                Assert.Equal(mq.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                Assert.Equal(mq.Status!.Value.GetTitle(), item.GetElementByTestId("status")?.TextContent.Trim());
                Assert.Equal(mq.EndDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("end-date")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationDeletedEventWithEmptyData_RendersExpectedContent()
    {
        // Arrange
        var (personId, mq) = await CreateEmptyMq();
        var deletedByUser = await TestData.CreateUserAsync();
        var deletionReason = "Created in error";
        await TestData.DeleteMandatoryQualificationAsync(mq.QualificationId, deletedByUser.UserId, deletionReason);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

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
        var deletedByUser = await TestData.CreateUserAsync();
        await TestData.DeleteMandatoryQualificationAsync(mq.QualificationId, deletedByUser.UserId, evidenceFile: null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-deleted-event"),
            item => Assert.Null(item.GetElementByTestId("evidence")));
    }

    [Fact]
    public async Task Person_WithDeletedMandatoryQualificationEventWithLegacyProvider_RendersExpectedProviderName()
    {
        // Arrange
        var (personId, mq, legacyProvider) = await CreateMqWithLegacyProvider();
        var deletedByUser = await TestData.CreateUserAsync();
        await TestData.DeleteMandatoryQualificationAsync(mq.QualificationId, deletedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-deleted-event"),
            item => Assert.Equal(legacyProvider.dfeta_name, item.GetElementByTestId("provider")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithDeletedMandatoryQualificationEventWithNoProvider_RendersNoneForProviderName()
    {
        // Arrange
        var (personId, mq) = await CreateMqWithoutProvider();
        var deletedByUser = await TestData.CreateUserAsync();
        await TestData.DeleteMandatoryQualificationAsync(mq.QualificationId, deletedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-deleted-event"),
            item => Assert.Equal("None", item.GetElementByTestId("provider")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualficationDqtDeactivatedEvent_RendersExpectedContent()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        Clock.Advance();
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
                Assert.Equal($"By {deactivatedByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                Assert.Equal(mq.Provider!.Name, item.GetElementByTestId("provider")?.TextContent.Trim());
                Assert.Equal(mq.Specialism!.Value.GetTitle(), item.GetElementByTestId("specialism")?.TextContent.Trim());
                Assert.Equal(mq.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                Assert.Equal(mq.Status!.Value.GetTitle(), item.GetElementByTestId("status")?.TextContent.Trim());
                Assert.Equal(mq.EndDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("end-date")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualficationDqtDeactivatedEventWithLegacyProvider_RendersExpectedProviderName()
    {
        // Arrange
        var (personId, mq, legacyProvider) = await CreateMqWithLegacyProvider();
        Clock.Advance();
        var deactivatedByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        await DeactivateMq(mq.QualificationId, deactivatedByDqtUser);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-dqt-deactivated-event"),
            item => Assert.Equal(legacyProvider.dfeta_name, item.GetElementByTestId("provider")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualficationDqtDeactivatedEventWithNoProvider_RendersExpectedContent()
    {
        // Arrange
        var (personId, mq) = await CreateMqWithoutProvider();
        Clock.Advance();
        var deactivatedByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        await DeactivateMq(mq.QualificationId, deactivatedByDqtUser);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-dqt-deactivated-event"),
            item => Assert.Equal("None", item.GetElementByTestId("provider")?.TextContent.Trim()));
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
                Assert.Equal($"By {importedByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationDqtReactivatedEvent_RendersExpectedContent()
    {
        // Arrange
        var (personId, mq, legacyProvider) = await CreateMqWithLegacyProvider();
        Clock.Advance();
        var dqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        await DeactivateMq(mq.QualificationId, dqtUser);
        Clock.Advance();

        await WithDbContext(async dbContext =>
        {
            var now = Clock.UtcNow;

            var qualification = await dbContext.MandatoryQualifications.IgnoreQueryFilters().SingleAsync(q => q.QualificationId == mq.QualificationId);
            qualification.DeletedOn = null;

            var mqEstablishment = qualification.DqtMqEstablishmentId is Guid mqEstablishmentId ?
                await TestData.ReferenceDataCache.GetMqEstablishmentByIdAsync(mqEstablishmentId) :
                null;

            var reactivatedEvent = new MandatoryQualificationDqtReactivatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = dqtUser,
                PersonId = qualification.PersonId,
                MandatoryQualification = new()
                {
                    QualificationId = qualification.QualificationId,
                    Provider = qualification.ProviderId is not null || mqEstablishment is not null ?
                        new EventModels.MandatoryQualificationProvider()
                        {
                            MandatoryQualificationProviderId = qualification.ProviderId,
                            Name = qualification.ProviderId is not null ?
                                qualification.Provider?.Name ?? throw new InvalidOperationException($"Missing {nameof(qualification.Provider)}.") :
                                null,
                            DqtMqEstablishmentId = mqEstablishment?.Id,
                            DqtMqEstablishmentName = mqEstablishment?.dfeta_name
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
                Assert.Equal($"By {dqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEvent_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
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
                Assert.Equal($"By {SystemUser.SystemUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEventWithNoChanges_DoesNotRenderPreviousDataSummaryList()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
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
        var establishmentWithProviderMapping = await TestData.ReferenceDataCache.GetMqEstablishmentByValueAsync("150");  // Postgraduate Diploma in Deaf Education, University of Manchester, School of Psychological Sciences
        var person = await TestData.CreatePersonAsync(b => b
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
            item => Assert.Equal(migratedProvider?.Name, item.GetElementByTestId("provider")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEventWithChangedSpecialism_RendersSpecialismRowInPreviousDataSummaryList()
    {
        // Arrange
        var specialism = MandatoryQualificationSpecialism.DeafEducation;
        var establishmentWithSpecialismMapping = await TestData.ReferenceDataCache.GetMqEstablishmentByValueAsync("961");  // University of Manchester
        var person = await TestData.CreatePersonAsync(b => b
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
            item => Assert.Equal(migratedSpecialism?.GetTitle(), item.GetElementByTestId("specialism")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationUpdatedEvent_RendersExpectedContent()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        Clock.Advance();
        var updatedByUser = await TestData.CreateUserAsync();
        var changeReason = "Update from provider";
        var changeReasonDetail = "More information";
        var evidenceFile = (FileId: Guid.NewGuid(), Name: "A file.jpeg");

        await UpdateMq(
            mq.QualificationId,
            q => q.Specialism = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: false).RandomOneExcept(s => s.Value == mq.Specialism).Value,
            MandatoryQualificationUpdatedEventChanges.Specialism,
            updatedByUser.UserId,
            changeReason,
            changeReasonDetail,
            evidenceFile);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item =>
            {
                Assert.Equal($"By {updatedByUser.Name} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                Assert.Equal(changeReason, item.GetElementByTestId("change-reason")?.TextContent.Trim());
                Assert.Equal(changeReasonDetail, item.GetElementByTestId("change-reason-detail")?.TextContent.Trim());
                Assert.Equal($"{evidenceFile.Name} (opens in new tab)", item.GetElementByTestId("evidence")?.TextContent);
                //Assert.Equal(mq.Provider!.Name, item.GetElementByTestId("provider")?.TextContent.Trim());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationUpdatedEventWithoutChangeReason_DoesNotRenderReasonForChangeBlock()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        Clock.Advance();
        var updatedByUser = await TestData.CreateUserAsync();

        await UpdateMq(
            mq.QualificationId,
            q => q.Specialism = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: false).RandomOneExcept(s => s.Value == mq.Specialism).Value,
            MandatoryQualificationUpdatedEventChanges.Specialism,
            updatedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item =>
            {
                Assert.Null(item.GetElementByTestId("change-reason")?.TextContent.Trim());
                Assert.Null(item.GetElementByTestId("change-reason-detail")?.TextContent.Trim());
                Assert.Null(item.GetElementByTestId("evidence")?.TextContent);
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationUpdatedEventWithChangedProvider_RendersTrainingProviderRowWithinPreviousData()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        Clock.Advance();
        var updatedByUser = await TestData.CreateUserAsync();
        var oldProviderId = mq.ProviderId!.Value;
        var oldProvider = MandatoryQualificationProvider.GetById(oldProviderId);

        await UpdateMq(
            mq.QualificationId,
            q => q.ProviderId = MandatoryQualificationProvider.All.RandomOneExcept(p => p.MandatoryQualificationProviderId == oldProviderId).MandatoryQualificationProviderId,
            MandatoryQualificationUpdatedEventChanges.Provider,
            updatedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item => Assert.Equal(oldProvider.Name, item.GetElementByTestId("provider")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationUpdatedEventWithoutChangedProvider_DoesNotRenderTrainingProviderRowWithinPreviousData()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        Clock.Advance();
        var updatedByUser = await TestData.CreateUserAsync();

        await UpdateMq(
            mq.QualificationId,
            q => q.StartDate = mq.StartDate!.Value.AddDays(1),
            MandatoryQualificationUpdatedEventChanges.StartDate,
            updatedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item => Assert.Null(item.GetElementByTestId("provider")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationUpdatedEventWithChangedSpecialism_RendersSpecialismRowWithinPreviousData()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        Clock.Advance();
        var updatedByUser = await TestData.CreateUserAsync();
        var oldSpecialism = mq.Specialism!.Value;

        await UpdateMq(
            mq.QualificationId,
            q => q.Specialism = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: false).RandomOneExcept(s => s.Value == oldSpecialism).Value,
            MandatoryQualificationUpdatedEventChanges.Specialism,
            updatedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item => Assert.Equal(oldSpecialism.GetTitle(), item.GetElementByTestId("specialism")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationUpdatedEventWithoutChangedSpecialism_DoesNotRenderSpecialismRowWithinPreviousData()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        Clock.Advance();
        var updatedByUser = await TestData.CreateUserAsync();

        await UpdateMq(
            mq.QualificationId,
            q => q.StartDate = mq.StartDate!.Value.AddDays(1),
            MandatoryQualificationUpdatedEventChanges.StartDate,
            updatedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item => Assert.Null(item.GetElementByTestId("specialism")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationUpdatedEventWithChangedStartDate_RendersStartDateRowWithinPreviousData()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        Clock.Advance();
        var updatedByUser = await TestData.CreateUserAsync();
        var oldStartDate = mq.StartDate!.Value;

        await UpdateMq(
            mq.QualificationId,
            q => q.StartDate = oldStartDate.AddDays(1),
            MandatoryQualificationUpdatedEventChanges.StartDate,
            updatedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item => Assert.Equal(oldStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationUpdatedEventWithoutChangedStartDate_DoesNotRenderStartDateRowWithinPreviousData()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        Clock.Advance();
        var updatedByUser = await TestData.CreateUserAsync();

        await UpdateMq(
            mq.QualificationId,
            q => q.EndDate = mq.EndDate!.Value.AddDays(1),
            MandatoryQualificationUpdatedEventChanges.EndDate,
            updatedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item => Assert.Null(item.GetElementByTestId("specialism")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationUpdatedEventWithChangedStatus_RendersStatusRowWithinPreviousData()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        Clock.Advance();
        var updatedByUser = await TestData.CreateUserAsync();
        var oldStatus = mq.Status!.Value;

        await UpdateMq(
            mq.QualificationId,
            q =>
            {
                q.Status = MandatoryQualificationStatus.InProgress;
                q.EndDate = null;
            },
            MandatoryQualificationUpdatedEventChanges.Status | MandatoryQualificationUpdatedEventChanges.EndDate,
            updatedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item => Assert.Equal(oldStatus.GetTitle(), item.GetElementByTestId("status")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationUpdatedEventWithoutChangedStatus_DoesNotRenderStatusRowWithinPreviousData()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        Clock.Advance();
        var updatedByUser = await TestData.CreateUserAsync();

        await UpdateMq(
            mq.QualificationId,
            q => q.StartDate = mq.StartDate!.Value.AddDays(1),
            MandatoryQualificationUpdatedEventChanges.StartDate,
            updatedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item => Assert.Null(item.GetElementByTestId("status")?.TextContent.Trim()));
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationUpdatedEventWithChangedEndDate_RendersEndDateRowWithinPreviousData()
    {
        // Arrange
        var (personId, mq) = await CreateFullyPopulatedMq();
        Clock.Advance();
        var updatedByUser = await TestData.CreateUserAsync();
        var oldEndDate = mq.EndDate!.Value;

        await UpdateMq(
            mq.QualificationId,
            q => q.EndDate = oldEndDate.AddDays(1),
            MandatoryQualificationUpdatedEventChanges.EndDate,
            updatedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item => Assert.Equal(oldEndDate.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("end-date")?.TextContent.Trim()));
    }

    //public async Task Person_WithMandatoryQualificationUpdatedEventWithoutChangedEndDate_DoesNotRenderEndDateRowWithinPreviousData()

    private async Task<(Guid PersonId, MandatoryQualification MandatoryQualification)> CreateFullyPopulatedMq(EventModels.RaisedByUserInfo? createdByUser = null)
    {
        var person = await TestData.CreatePersonAsync(b => b
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
        var person = await TestData.CreatePersonAsync(b => b
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
        var person = await TestData.CreatePersonAsync(b => b
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
        var legacyProvider = (await TestData.ReferenceDataCache.GetMqEstablishmentsAsync()).RandomOne();

        var person = await TestData.CreatePersonAsync(b => b
            .WithMandatoryQualification(q => q
                .WithDqtMqEstablishment(legacyProvider, mandatoryQualificationProviderId: null)));

        var mq = await WithDbContext(dbContext => dbContext.MandatoryQualifications
            .Include(q => q.Provider)
            .SingleAsync(q => q.QualificationId == person.MandatoryQualifications.Single().QualificationId));

        Debug.Assert(mq.DqtMqEstablishmentId.HasValue);
        Debug.Assert(!mq.ProviderId.HasValue);

        return (person.PersonId, mq, legacyProvider);
    }

    private Task DeactivateMq(Guid qualificationId, EventModels.RaisedByUserInfo deactivatedBy) => WithDbContext(async dbContext =>
    {
        if (!deactivatedBy.IsDqtUser)
        {
            throw new ArgumentException($"{nameof(deactivatedBy)} should be a DQT user.", nameof(deactivatedBy));
        }

        var now = Clock.UtcNow;

        var qualification = await dbContext.MandatoryQualifications
            .Include(q => q.Provider)
            .SingleAsync(q => q.QualificationId == qualificationId);

        qualification.DeletedOn = now;

        var mqEstablishment = qualification.DqtMqEstablishmentId is Guid mqEstablishmentId ?
            await TestData.ReferenceDataCache.GetMqEstablishmentByIdAsync(mqEstablishmentId) :
            null;

        var deletedEvent = new MandatoryQualificationDqtDeactivatedEvent()
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = deactivatedBy,
            PersonId = qualification.PersonId,
            MandatoryQualification = new()
            {
                QualificationId = qualification.QualificationId,
                Provider = qualification.ProviderId is not null || mqEstablishment is not null ?
                    new EventModels.MandatoryQualificationProvider()
                    {
                        MandatoryQualificationProviderId = qualification.ProviderId,
                        Name = qualification.ProviderId is not null ?
                            qualification.Provider?.Name ?? throw new InvalidOperationException($"Missing {nameof(qualification.Provider)}.") :
                            null,
                        DqtMqEstablishmentId = mqEstablishment?.Id,
                        DqtMqEstablishmentName = mqEstablishment?.dfeta_name
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

    private Task UpdateMq(
        Guid qualificationId,
        Action<MandatoryQualification> update,
        MandatoryQualificationUpdatedEventChanges changes,
        EventModels.RaisedByUserInfo updatedBy,
        string? changeReason = null,
        string? changeReasonDetail = null,
        (Guid FileId, string Name)? evidenceFile = null)
    {
        return WithDbContext(async dbContext =>
        {
            var now = Clock.UtcNow;

            var qualification = await dbContext.MandatoryQualifications
                .Include(q => q.Provider)
                .SingleAsync(q => q.QualificationId == qualificationId);

            var oldMqEstablishment = qualification.DqtMqEstablishmentId is Guid oldMqEstablishmentId ?
                await TestData.ReferenceDataCache.GetMqEstablishmentByIdAsync(oldMqEstablishmentId) :
                null;

            var oldMq = new EventModels.MandatoryQualification()
            {
                QualificationId = qualification.QualificationId,
                Provider = qualification.ProviderId is not null || oldMqEstablishment is not null ?
                    new EventModels.MandatoryQualificationProvider()
                    {
                        MandatoryQualificationProviderId = qualification.ProviderId,
                        Name = qualification.ProviderId is not null ?
                            qualification.Provider?.Name ?? throw new InvalidOperationException($"Missing {nameof(qualification.Provider)}.") :
                            null,
                        DqtMqEstablishmentId = oldMqEstablishment?.Id,
                        DqtMqEstablishmentName = oldMqEstablishment?.dfeta_name
                    } :
                    null,
                Specialism = qualification.Specialism,
                Status = qualification.Status,
                StartDate = qualification.StartDate,
                EndDate = qualification.EndDate
            };

            update(qualification);
            qualification.UpdatedOn = now;

            var mqEstablishment = qualification.DqtMqEstablishmentId is Guid mqEstablishmentId ?
                await TestData.ReferenceDataCache.GetMqEstablishmentByIdAsync(mqEstablishmentId) :
                null;

            var updatedEvent = new MandatoryQualificationUpdatedEvent()
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = updatedBy,
                PersonId = qualification.PersonId,
                MandatoryQualification = new()
                {
                    QualificationId = qualification.QualificationId,
                    Provider = qualification.ProviderId is not null || mqEstablishment is not null ?
                        new EventModels.MandatoryQualificationProvider()
                        {
                            MandatoryQualificationProviderId = qualification.ProviderId,
                            Name = qualification.ProviderId is not null ?
                                qualification.Provider?.Name ?? throw new InvalidOperationException($"Missing {nameof(qualification.Provider)}.") :
                                null,
                            DqtMqEstablishmentId = mqEstablishment?.Id,
                            DqtMqEstablishmentName = mqEstablishment?.dfeta_name
                        } :
                        null,
                    Specialism = qualification.Specialism,
                    Status = qualification.Status,
                    StartDate = qualification.StartDate,
                    EndDate = qualification.EndDate
                },
                OldMandatoryQualification = oldMq,
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                EvidenceFile = evidenceFile is not null ?
                    new EventModels.File()
                    {
                        FileId = evidenceFile!.Value.FileId,
                        Name = evidenceFile!.Value.Name
                    } :
                    null,
                Changes = changes
            };
            dbContext.AddEventWithoutBroadcast(updatedEvent);

            await dbContext.SaveChangesAsync();
        });
    }
}
