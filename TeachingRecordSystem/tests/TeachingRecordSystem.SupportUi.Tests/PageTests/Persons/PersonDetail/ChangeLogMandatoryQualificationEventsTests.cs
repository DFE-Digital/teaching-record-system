using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;
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
        Clock.UtcNow = nows.SingleRandom();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithMandatoryQualificationCreatedEvent_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var status = populateOptional ? MandatoryQualificationStatus.Passed : MandatoryQualificationStatus.InProgress;
        AddMqReasonOption? addReason = populateOptional ? AddMqReasonOption.NewInformationReceived : null;
        var addReasonDetail = populateOptional ? "More information" : null;
        (Guid FileId, string Name)? evidenceFile = populateOptional ? (FileId: Guid.NewGuid(), Name: "evidence.jpeg") : null;
        var (personId, mq) = await CreateFullyPopulatedMq(createdByUser.UserId, status, addReason, addReasonDetail, evidenceFile);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-created-event"),
            item =>
            {
                Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
                Assert.Equal(mq.Provider!.Name, item.GetElementByTestId("provider")?.TrimmedText());
                Assert.Equal(mq.Specialism!.Value.GetTitle(), item.GetElementByTestId("specialism")?.TrimmedText());
                Assert.Equal(mq.StartDate!.Value.ToString(WebConstants.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TrimmedText());
                Assert.Equal(mq.Status!.Value.GetTitle(), item.GetElementByTestId("status")?.TrimmedText());
                if (populateOptional)
                {
                    Assert.Equal(mq.EndDate!.Value.ToString(WebConstants.DateOnlyDisplayFormat), item.GetElementByTestId("end-date")?.TrimmedText());
                    Assert.Equal(addReason?.GetDisplayName(), item.GetElementByTestId("reason")?.TrimmedText());
                    Assert.Equal(addReasonDetail, item.GetElementByTestId("reason-detail")?.TrimmedText());
                    Assert.Equal($"{evidenceFile!.Value.Name} (opens in new tab)", item.GetElementByTestId("evidence")?.TrimmedText());
                }
                else
                {
                    Assert.Equal("None", item.GetElementByTestId("end-date")?.TrimmedText());
                    Assert.Equal(WebConstants.EmptyFallbackContent, item.GetElementByTestId("reason")?.TrimmedText());
                    Assert.Equal(WebConstants.EmptyFallbackContent, item.GetElementByTestId("reason-detail")?.TrimmedText());
                    Assert.Equal(WebConstants.EmptyFallbackContent, item.GetElementByTestId("evidence")?.TrimmedText());
                }
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
            item => Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TrimmedText()));
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
                Assert.Equal($"By {deletedByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
                Assert.Equal(deletionReason, item.GetElementByTestId("deletion-reason")?.TrimmedText());
                Assert.Equal(deletionReasonDetail, item.GetElementByTestId("deletion-reason-detail")?.TrimmedText());
                Assert.Equal($"{evidenceFile.Name} (opens in new tab)", item.GetElementByTestId("evidence")?.TrimmedText());
                Assert.Equal(mq.Provider!.Name, item.GetElementByTestId("provider")?.TrimmedText());
                Assert.Equal(mq.Specialism!.Value.GetTitle(), item.GetElementByTestId("specialism")?.TrimmedText());
                Assert.Equal(mq.StartDate!.Value.ToString(WebConstants.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TrimmedText());
                Assert.Equal(mq.Status!.Value.GetTitle(), item.GetElementByTestId("status")?.TrimmedText());
                Assert.Equal(mq.EndDate!.Value.ToString(WebConstants.DateOnlyDisplayFormat), item.GetElementByTestId("end-date")?.TrimmedText());
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
                Assert.Equal($"By {deletedByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
                Assert.Equal(deletionReason, item.GetElementByTestId("deletion-reason")?.TrimmedText());
                Assert.Equal("None", item.GetElementByTestId("deletion-reason-detail")?.TrimmedText());
                Assert.Equal("None", item.GetElementByTestId("provider")?.TrimmedText());
                Assert.Equal("None", item.GetElementByTestId("specialism")?.TrimmedText());
                Assert.Equal("None", item.GetElementByTestId("start-date")?.TrimmedText());
                Assert.Equal("None", item.GetElementByTestId("status")?.TrimmedText());
                Assert.Equal("None", item.GetElementByTestId("end-date")?.TrimmedText());
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
            item => Assert.Equal(legacyProvider.Name, item.GetElementByTestId("provider")?.TrimmedText()));
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
            item => Assert.Equal("None", item.GetElementByTestId("provider")?.TrimmedText()));
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
                Assert.Equal($"By {deactivatedByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TrimmedText());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
                Assert.Equal(mq.Provider!.Name, item.GetElementByTestId("provider")?.TrimmedText());
                Assert.Equal(mq.Specialism!.Value.GetTitle(), item.GetElementByTestId("specialism")?.TrimmedText());
                Assert.Equal(mq.StartDate!.Value.ToString(WebConstants.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TrimmedText());
                Assert.Equal(mq.Status!.Value.GetTitle(), item.GetElementByTestId("status")?.TrimmedText());
                Assert.Equal(mq.EndDate!.Value.ToString(WebConstants.DateOnlyDisplayFormat), item.GetElementByTestId("end-date")?.TrimmedText());
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
            item => Assert.Equal(legacyProvider.Name, item.GetElementByTestId("provider")?.TrimmedText()));
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
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
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

        await WithDbContextAsync(async dbContext =>
        {
            var now = Clock.UtcNow;

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
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEvent_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        Clock.Advance();

        await WithDbContextAsync(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications
                .Where(q => q.PersonId == person.PersonId)
                .SingleAsync();

            var migratedEvent = new MandatoryQualificationMigratedEvent
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
                Assert.Equal($"By {SystemUser.SystemUserName} on", item.GetElementByTestId("raised-by")?.TrimmedText());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
            });
    }

    [Fact]
    public async Task Person_WithMandatoryQualificationMigratedEventWithNoChanges_DoesNotRenderPreviousDataSummaryList()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        Clock.Advance();

        await WithDbContextAsync(async dbContext =>
        {
            var mq = await dbContext.MandatoryQualifications
                .Where(q => q.PersonId == person.PersonId)
                .SingleAsync();

            var migratedEvent = new MandatoryQualificationMigratedEvent
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
        var establishmentWithProviderMapping = LegacyDataCache.Instance.GetMqEstablishmentByValue("150");
        var person = await TestData.CreatePersonAsync(b => b
            .WithMandatoryQualification(q => q
                .WithDqtMqEstablishment(establishmentWithProviderMapping.Value)));
        Clock.Advance();

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
        Clock.Advance();

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
            item => Assert.Equal(migratedSpecialism?.GetTitle(), item.GetElementByTestId("specialism")?.TrimmedText()));
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
            q => q.Specialism = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: false).SingleRandom(s => s.Value != mq.Specialism).Value,
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
                Assert.Equal($"By {updatedByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
                Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());
                Assert.Equal(changeReason, item.GetElementByTestId("change-reason")?.TrimmedText());
                Assert.Equal(changeReasonDetail, item.GetElementByTestId("change-reason-detail")?.TrimmedText());
                Assert.Equal($"{evidenceFile.Name} (opens in new tab)", item.GetElementByTestId("evidence")?.TrimmedText());
                //Assert.Equal(mq.Provider!.Name, item.GetElementByTestId("provider")?.TrimmedTextContent());
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
            q => q.Specialism = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: false).SingleRandom(s => s.Value != mq.Specialism).Value,
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
                Assert.Null(item.GetElementByTestId("change-reason")?.TrimmedText());
                Assert.Null(item.GetElementByTestId("change-reason-detail")?.TrimmedText());
                Assert.Null(item.GetElementByTestId("evidence")?.TrimmedText());
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
            q => q.ProviderId = MandatoryQualificationProvider.All.SingleRandom(p => p.MandatoryQualificationProviderId != oldProviderId).MandatoryQualificationProviderId,
            MandatoryQualificationUpdatedEventChanges.Provider,
            updatedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item => Assert.Equal(oldProvider.Name, item.GetElementByTestId("provider")?.TrimmedText()));
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
            item => Assert.Null(item.GetElementByTestId("provider")?.TrimmedText()));
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
            q => q.Specialism = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: false).SingleRandom(s => s.Value != oldSpecialism).Value,
            MandatoryQualificationUpdatedEventChanges.Specialism,
            updatedByUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-mq-updated-event"),
            item => Assert.Equal(oldSpecialism.GetTitle(), item.GetElementByTestId("specialism")?.TrimmedText()));
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
            item => Assert.Null(item.GetElementByTestId("specialism")?.TrimmedText()));
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
            item => Assert.Equal(oldStartDate.ToString(WebConstants.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TrimmedText()));
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
            item => Assert.Null(item.GetElementByTestId("specialism")?.TrimmedText()));
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
            item => Assert.Equal(oldStatus.GetTitle(), item.GetElementByTestId("status")?.TrimmedText()));
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
            item => Assert.Null(item.GetElementByTestId("status")?.TrimmedText()));
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
            item => Assert.Equal(oldEndDate.ToString(WebConstants.DateOnlyDisplayFormat), item.GetElementByTestId("end-date")?.TrimmedText()));
    }

    //public async Task Person_WithMandatoryQualificationUpdatedEventWithoutChangedEndDate_DoesNotRenderEndDateRowWithinPreviousData()

    private async Task<(Guid PersonId, MandatoryQualification MandatoryQualification)> CreateFullyPopulatedMq(
        EventModels.RaisedByUserInfo? createdByUser = null,
        MandatoryQualificationStatus? status = null,
        AddMqReasonOption? reason = null,
        string? reasonDetail = null,
        (Guid FileId, string Name)? evidenceFile = null)
    {
        var person = await TestData.CreatePersonAsync(b => b
            .WithMandatoryQualification(q =>
            {
                q.WithStatus(status ?? MandatoryQualificationStatus.Passed);
                q.WithAddReason(reason?.GetDisplayName(), reasonDetail, evidenceFile);

                if (createdByUser is not null)
                {
                    q.WithCreatedByUser(createdByUser);
                }
            }));

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

    private async Task<(Guid PersonId, MandatoryQualification MandatoryQualification)> CreateEmptyMq()
    {
        var person = await TestData.CreatePersonAsync(b => b
            .WithMandatoryQualification(q => q
                .WithStartDate(null)
                .WithSpecialism(null)
                .WithStatus(null)
                .WithProvider(null)));

        var mq = await WithDbContextAsync(dbContext => dbContext.MandatoryQualifications
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

        var now = Clock.UtcNow;

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

    private Task UpdateMq(
        Guid qualificationId,
        Action<MandatoryQualification> update,
        MandatoryQualificationUpdatedEventChanges changes,
        EventModels.RaisedByUserInfo updatedBy,
        string? changeReason = null,
        string? changeReasonDetail = null,
        (Guid FileId, string Name)? evidenceFile = null)
    {
        return WithDbContextAsync(async dbContext =>
        {
            var now = Clock.UtcNow;

            var qualification = await dbContext.MandatoryQualifications
                .SingleAsync(q => q.QualificationId == qualificationId);

            var oldMqEstablishment = qualification.DqtMqEstablishmentValue is string oldMqEstablishmentValue ?
                LegacyDataCache.Instance.GetMqSpecialismByValue(oldMqEstablishmentValue) :
                null;

            var oldMq = new EventModels.MandatoryQualification
            {
                QualificationId = qualification.QualificationId,
                Provider = qualification.ProviderId is not null || oldMqEstablishment is not null ?
                    new EventModels.MandatoryQualificationProvider
                    {
                        MandatoryQualificationProviderId = qualification.ProviderId,
                        Name = qualification.ProviderId is not null ?
                            qualification.Provider?.Name ?? throw new InvalidOperationException($"Missing {nameof(qualification.Provider)}.") :
                            null,
                        DqtMqEstablishmentName = oldMqEstablishment?.Name,
                        DqtMqEstablishmentValue = oldMqEstablishment?.Value
                    } :
                    null,
                Specialism = qualification.Specialism,
                Status = qualification.Status,
                StartDate = qualification.StartDate,
                EndDate = qualification.EndDate
            };

            update(qualification);
            qualification.UpdatedOn = now;

            var mqEstablishment = qualification.DqtMqEstablishmentValue is string mqEstablishmentValue ?
                LegacyDataCache.Instance.GetMqSpecialismByValue(mqEstablishmentValue) :
                null;

            var updatedEvent = new MandatoryQualificationUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = updatedBy,
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
                },
                OldMandatoryQualification = oldMq,
                ChangeReason = changeReason,
                ChangeReasonDetail = changeReasonDetail,
                EvidenceFile = evidenceFile is not null ?
                    new EventModels.File
                    {
                        FileId = evidenceFile.Value.FileId,
                        Name = evidenceFile.Value.Name
                    } :
                    null,
                Changes = changes
            };
            dbContext.AddEventWithoutBroadcast(updatedEvent);

            await dbContext.SaveChangesAsync();
        });
    }
}
