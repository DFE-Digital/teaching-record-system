using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogAlertEventsTests : TestBase
{
    public ChangeLogAlertEventsTests(HostFixture hostFixture) : base(hostFixture)
    {
        // Toggle between GMT and BST to ensure we're testing rendering dates in local time
        var nows = new[]
        {
            new DateTime(2024, 1, 1, 12, 13, 14, DateTimeKind.Utc),  // GMT
            new DateTime(2024, 7, 5, 19, 20, 21, DateTimeKind.Utc)   // BST
        };
        Clock.UtcNow = nows.RandomOne();
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsReadWrite, UserRoles.DbsAlertsReadWrite));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithAlertCreatedEventGeneratedInTrs_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByUser = await TestData.CreateUser();
        var person = await TestData.CreatePerson();
        var alertCreatedEvent = await CreateAlertCreatedEventFromTrs(person.PersonId, createdByUser.UserId, populateOptional, isOpenAlert: true);
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(alertCreatedEvent.Alert.AlertTypeId!.Value);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-created-event"),
                item =>
                {
                    Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertType!.Name, item.GetElementByTestId("alert-type")?.TextContent.Trim());
                    Assert.Equal(alertCreatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertCreatedEvent.AddReason : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("reason")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertCreatedEvent.AddReasonDetail : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("reason-detail")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? $"{alertCreatedEvent.EvidenceFile!.Name} (opens in new tab)" : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("uploaded-evidence-link")?.TextContent);
                });
    }

    [Theory]
    [InlineData(false, null, true)]
    [InlineData(false, UserRoles.AlertsReadWrite, true)]
    [InlineData(true, null, false)]
    [InlineData(true, UserRoles.AlertsReadWrite, false)]
    [InlineData(true, UserRoles.DbsAlertsReadWrite, true)]
    [InlineData(true, UserRoles.DbsAlertsReadOnly, true)]
    [InlineData(true, UserRoles.Administrator, true)]
    public async Task Person_WithAlertCreatedEventGeneratedInTrs_DisplaysAsExpectedForUserRole(bool isDbsAlertType, string? role, bool shouldDisplay)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role is not null ? [role!] : []));

        var createdByUser = await TestData.CreateUser();
        var person = await TestData.CreatePerson();
        var alertCreatedEvent = await CreateAlertCreatedEventFromTrs(person.PersonId, createdByUser.UserId, populateOptional: true, isOpenAlert: true, isDbsAlertType: isDbsAlertType);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var items = doc.GetAllElementsByTestId("timeline-item-alert-created-event");

        if (shouldDisplay)
        {
            Assert.NotEmpty(items);
        }
        else
        {
            Assert.Empty(items);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithAlertCreatedEventGeneratedInDqt_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alertCreatedEvent = await CreateAlertCreatedEventFromDqt(person.PersonId, createdByDqtUser, populateOptional: populateOptional, isOpenAlert: true);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-created-event"),
                item =>
                {
                    Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertCreatedEvent!.Alert.DqtSanctionCode!.Value, item.GetElementByTestId("sanction-code")?.TextContent.Trim());
                    Assert.Equal(alertCreatedEvent!.Alert.DqtSanctionCode!.Name, item.GetElementByTestId("sanction-name")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertCreatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("start-date")?.TextContent.Trim());
                });
    }

    [Theory]
    [InlineData(false, null, true)]
    [InlineData(false, UserRoles.AlertsReadWrite, true)]
    [InlineData(true, null, false)]
    [InlineData(true, UserRoles.AlertsReadWrite, false)]
    [InlineData(true, UserRoles.DbsAlertsReadWrite, true)]
    [InlineData(true, UserRoles.DbsAlertsReadOnly, true)]
    [InlineData(true, UserRoles.Administrator, true)]
    public async Task Person_WithAlertCreatedEventGeneratedInDqt_DisplaysAsExpectedForUserRole(bool isDbsAlertType, string? role, bool shouldDisplay)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role is not null ? [role!] : []));

        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alertCreatedEvent = await CreateAlertCreatedEventFromDqt(person.PersonId, createdByDqtUser, populateOptional: true, isOpenAlert: true, isDbsAlertType: isDbsAlertType);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var items = doc.GetAllElementsByTestId("timeline-item-alert-created-event");

        if (shouldDisplay)
        {
            if (items.Count == 0)
            {
                Debug.Assert(false);
            }
            Assert.NotEmpty(items);
        }
        else
        {
            Assert.Empty(items);
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task Person_WithAlertDeletedEvent_RendersExpectedContent(bool populateOptional, bool isOpenAlert)
    {
        // Arrange
        var createdByUser = await TestData.CreateUser();
        var person = await TestData.CreatePerson();
        var alertDeletedEvent = await CreateAlertDeletedEvent(person.PersonId, createdByUser.UserId, populateOptional, isOpenAlert);
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(alertDeletedEvent.Alert.AlertTypeId!.Value);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-deleted-event"),
             item =>
             {
                 Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                 Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                 Assert.Equal(alertType!.Name, item.GetElementByTestId("alert-type")?.TextContent.Trim());
                 Assert.Equal(alertDeletedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                 Assert.Equal(populateOptional ? alertDeletedEvent.DeletionReasonDetail : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("reason-detail")?.TextContent.Trim());
                 Assert.Equal(populateOptional ? $"{alertDeletedEvent.EvidenceFile!.Name} (opens in new tab)" : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("uploaded-evidence-link")?.TextContent);
             });
    }

    [Theory]
    [InlineData(false, null, true)]
    [InlineData(false, UserRoles.AlertsReadWrite, true)]
    [InlineData(true, null, false)]
    [InlineData(true, UserRoles.AlertsReadWrite, false)]
    [InlineData(true, UserRoles.DbsAlertsReadWrite, true)]
    [InlineData(true, UserRoles.DbsAlertsReadOnly, true)]
    [InlineData(true, UserRoles.Administrator, true)]
    public async Task Person_WithAlertDeletedEvent_DisplaysAsExpectedForUserRole(bool isDbsAlertType, string? role, bool shouldDisplay)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role is not null ? [role!] : []));

        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alertCreatedEvent = await CreateAlertDeletedEvent(person.PersonId, createdByDqtUser, populateOptional: true, isOpenAlert: true, isDbsAlertType: isDbsAlertType);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var items = doc.GetAllElementsByTestId("timeline-item-alert-deleted-event");

        if (shouldDisplay)
        {
            if (items.Count == 0)
            {
                Debug.Assert(false);
            }
            Assert.NotEmpty(items);
        }
        else
        {
            Assert.Empty(items);
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public async Task Person_WithAlertDqtDeactivatedEvent_RendersExpectedContent(bool populateOptional, bool isOpenAlert)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alertDqtDeactivatedEvent = await CreateAlertDqtDeactivatedEvent(person.PersonId, createdByDqtUser, populateOptional, isOpenAlert);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var all = doc.GetAllElementsByTestId("timeline-item-alert-dqt-deactivated-event").ToArray();

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-dqt-deactivated-event"),
             item =>
             {
                 Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                 Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                 Assert.Equal(alertDqtDeactivatedEvent!.Alert.DqtSanctionCode!.Value, item.GetElementByTestId("sanction-code")?.TextContent.Trim());
                 Assert.Equal(alertDqtDeactivatedEvent!.Alert.DqtSanctionCode!.Name, item.GetElementByTestId("sanction-name")?.TextContent.Trim());
                 Assert.Equal(populateOptional ? alertDqtDeactivatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("start-date")?.TextContent.Trim());
                 Assert.Equal(populateOptional ? alertDqtDeactivatedEvent.Alert.Details : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("details")?.TextContent.Trim());
                 Assert.Equal(isOpenAlert ? UiDefaults.EmptyDisplayContent : alertDqtDeactivatedEvent.Alert.EndDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("end-date")?.TextContent.Trim());
             });
    }

    [Theory]
    [InlineData(false, null, true)]
    [InlineData(false, UserRoles.AlertsReadWrite, true)]
    [InlineData(true, null, false)]
    [InlineData(true, UserRoles.AlertsReadWrite, false)]
    [InlineData(true, UserRoles.DbsAlertsReadWrite, true)]
    [InlineData(true, UserRoles.DbsAlertsReadOnly, true)]
    [InlineData(true, UserRoles.Administrator, true)]
    public async Task Person_WithAlertDqtDeactivatedEvent_DisplaysAsExpectedForUserRole(bool isDbsAlertType, string? role, bool shouldDisplay)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role is not null ? [role!] : []));

        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alertCreatedEvent = await CreateAlertDqtDeactivatedEvent(person.PersonId, createdByDqtUser, populateOptional: true, isOpenAlert: true, isDbsAlertType: isDbsAlertType);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var items = doc.GetAllElementsByTestId("timeline-item-alert-dqt-deactivated-event");

        if (shouldDisplay)
        {
            if (items.Count == 0)
            {
                Debug.Assert(false);
            }
            Assert.NotEmpty(items);
        }
        else
        {
            Assert.Empty(items);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithAlertDqtImportedEvent_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alertDqtImportedEvent = await CreateAlertDqtImportedEvent(person.PersonId, createdByDqtUser, populateOptional);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-dqt-imported-event"),
                item =>
                {
                    Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertDqtImportedEvent!.Alert.DqtSanctionCode!.Value, item.GetElementByTestId("sanction-code")?.TextContent.Trim());
                    Assert.Equal(alertDqtImportedEvent!.Alert.DqtSanctionCode!.Name, item.GetElementByTestId("sanction-name")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertDqtImportedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("start-date")?.TextContent.Trim());
                });
    }

    [Theory]
    [InlineData(false, null, true)]
    [InlineData(false, UserRoles.AlertsReadWrite, true)]
    [InlineData(true, null, false)]
    [InlineData(true, UserRoles.AlertsReadWrite, false)]
    [InlineData(true, UserRoles.DbsAlertsReadWrite, true)]
    [InlineData(true, UserRoles.DbsAlertsReadOnly, true)]
    [InlineData(true, UserRoles.Administrator, true)]
    public async Task Person_WithAlertDqtImportedEvent_DisplaysAsExpectedForUserRole(bool isDbsAlertType, string? role, bool shouldDisplay)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role is not null ? [role!] : []));

        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alertCreatedEvent = await CreateAlertDqtImportedEvent(person.PersonId, createdByDqtUser, populateOptional: true, isDbsAlertType: isDbsAlertType);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var items = doc.GetAllElementsByTestId("timeline-item-alert-dqt-imported-event");

        if (shouldDisplay)
        {
            if (items.Count == 0)
            {
                Debug.Assert(false);
            }
            Assert.NotEmpty(items);
        }
        else
        {
            Assert.Empty(items);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithAlertDqtReactivatedEvent_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alertDqtImportedEvent = await CreateAlertDqtReactivatedEvent(person.PersonId, createdByDqtUser, populateOptional);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-dqt-reactivated-event"),
                item =>
                {
                    Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertDqtImportedEvent!.Alert.DqtSanctionCode!.Value, item.GetElementByTestId("sanction-code")?.TextContent.Trim());
                    Assert.Equal(alertDqtImportedEvent!.Alert.DqtSanctionCode!.Name, item.GetElementByTestId("sanction-name")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertDqtImportedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("start-date")?.TextContent.Trim());
                });
    }

    [Theory]
    [InlineData(false, null, true)]
    [InlineData(false, UserRoles.AlertsReadWrite, true)]
    [InlineData(true, null, false)]
    [InlineData(true, UserRoles.AlertsReadWrite, false)]
    [InlineData(true, UserRoles.DbsAlertsReadWrite, true)]
    [InlineData(true, UserRoles.DbsAlertsReadOnly, true)]
    [InlineData(true, UserRoles.Administrator, true)]
    public async Task Person_WithAlertDqtReactivatedEvent_DisplaysAsExpectedForUserRole(bool isDbsAlertType, string? role, bool shouldDisplay)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role is not null ? [role!] : []));

        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alertCreatedEvent = await CreateAlertDqtReactivatedEvent(person.PersonId, createdByDqtUser, populateOptional: true, isDbsAlertType: isDbsAlertType);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var items = doc.GetAllElementsByTestId("timeline-item-alert-dqt-reactivated-event");

        if (shouldDisplay)
        {
            if (items.Count == 0)
            {
                Debug.Assert(false);
            }
            Assert.NotEmpty(items);
        }
        else
        {
            Assert.Empty(items);
        }
    }

    [Fact]
    public async Task Person_WithAlertMigratedEvent_RendersExpectedContent()
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alertMigratedEvent = await CreateAlertMigratedEvent(person.PersonId, createdByDqtUser, populateOptional: true);
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(alertMigratedEvent.Alert.AlertTypeId!.Value);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-migrated-event"),
                item =>
                {
                    Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertType!.Name, item.GetElementByTestId("alert-type")?.TextContent.Trim());
                    Assert.Equal(alertMigratedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(alertMigratedEvent!.OldAlert.DqtSanctionCode!.Value, item.GetElementByTestId("sanction-code")?.TextContent.Trim());
                    Assert.Equal(alertMigratedEvent!.OldAlert.DqtSanctionCode!.Name, item.GetElementByTestId("sanction-name")?.TextContent.Trim());
                });
    }

    [Theory]
    [InlineData(false, null, true)]
    [InlineData(false, UserRoles.AlertsReadWrite, true)]
    [InlineData(true, null, false)]
    [InlineData(true, UserRoles.AlertsReadWrite, false)]
    [InlineData(true, UserRoles.DbsAlertsReadWrite, true)]
    [InlineData(true, UserRoles.DbsAlertsReadOnly, true)]
    [InlineData(true, UserRoles.Administrator, true)]
    public async Task Person_WithAlertMigratedEvent_DisplaysAsExpectedForUserRole(bool isDbsAlertType, string? role, bool shouldDisplay)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role is not null ? [role!] : []));

        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alertCreatedEvent = await CreateAlertMigratedEvent(person.PersonId, createdByDqtUser, populateOptional: true, isDbsAlertType: isDbsAlertType);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var items = doc.GetAllElementsByTestId("timeline-item-alert-migrated-event");

        if (shouldDisplay)
        {
            if (items.Count == 0)
            {
                Debug.Assert(false);
            }
            Assert.NotEmpty(items);
        }
        else
        {
            Assert.Empty(items);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithAlertUpdatedEventStartChangedFromDqt_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alert = await CreateEventAlertFromDqt(person.PersonId, populateOptional, isOpenAlert: true);
        var oldAlert = alert with { StartDate = populateOptional ? null : Clock.Today.AddDays(-30) };

        var alertUpdatedEvent = await CreateAlertUpdatedEvent(person.PersonId, alert, oldAlert, AlertUpdatedEventChanges.StartDate, createdByDqtUser, hasReason: false);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-updated-event"),
                item =>
                {
                    Assert.Equal("Alert start date changed", item.GetElementByTestId("heading")?.TextContent.Trim());
                    Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.Alert.DqtSanctionCode!.Value, item.GetElementByTestId("sanction-code")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.Alert.DqtSanctionCode!.Name, item.GetElementByTestId("sanction-name")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("start-date")?.TextContent.Trim());                    
                    Assert.Equal(populateOptional ? UiDefaults.EmptyDisplayContent : alertUpdatedEvent.OldAlert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("old-start-date")?.TextContent.Trim());
                });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithAlertUpdatedEventStartChangedFromTrs_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByUser = await TestData.CreateUser();
        var person = await TestData.CreatePerson();
        var alert = await CreateEventAlertFromTrs(person.PersonId, populateOptional, isOpenAlert: true);
        var oldAlert = alert with { StartDate = Clock.Today.AddDays(-31) };

        var alertUpdatedEvent = await CreateAlertUpdatedEvent(person.PersonId, alert, oldAlert, AlertUpdatedEventChanges.StartDate, createdByUser.UserId, hasReason: populateOptional);
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(alertUpdatedEvent.Alert.AlertTypeId!.Value);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-updated-event"),
                item =>
                {
                    Assert.Equal("Alert start date changed", item.GetElementByTestId("heading")?.TextContent.Trim());
                    Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertType!.Name, item.GetElementByTestId("alert-type")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent.OldAlert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("old-start-date")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.ChangeReason : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("reason")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.ChangeReasonDetail : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("reason-detail")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? $"{alertUpdatedEvent.EvidenceFile!.Name} (opens in new tab)" : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("uploaded-evidence-link")?.TextContent);
                });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithAlertUpdatedEventDetailsChangedFromDqt_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alert = await CreateEventAlertFromDqt(person.PersonId, populateOptional, isOpenAlert: true);
        var oldAlert = alert with { Details = populateOptional ? null : "Old details" };

        var alertUpdatedEvent = await CreateAlertUpdatedEvent(person.PersonId, alert, oldAlert, AlertUpdatedEventChanges.Details, createdByDqtUser, hasReason: false);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-updated-event"),
                item =>
                {
                    Assert.Equal("Alert details changed", item.GetElementByTestId("heading")?.TextContent.Trim());
                    Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.Alert.DqtSanctionCode!.Value, item.GetElementByTestId("sanction-code")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.Alert.DqtSanctionCode!.Name, item.GetElementByTestId("sanction-name")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.Alert.Details : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("details")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? UiDefaults.EmptyDisplayContent : alertUpdatedEvent.OldAlert.Details, item.GetElementByTestId("old-details")?.TextContent.Trim());
                });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithAlertUpdatedEventDetailsChangedFromTrs_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByUser = await TestData.CreateUser();
        var person = await TestData.CreatePerson();
        var alert = await CreateEventAlertFromTrs(person.PersonId, populateOptional, isOpenAlert: true);
        var oldAlert = alert with { StartDate = Clock.Today.AddDays(-31) };

        var alertUpdatedEvent = await CreateAlertUpdatedEvent(person.PersonId, alert, oldAlert, AlertUpdatedEventChanges.Details, createdByUser.UserId, hasReason: populateOptional);
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(alertUpdatedEvent.Alert.AlertTypeId!.Value);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-updated-event"),
                item =>
                {
                    Assert.Equal("Alert details changed", item.GetElementByTestId("heading")?.TextContent.Trim());
                    Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertType!.Name, item.GetElementByTestId("alert-type")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent.Alert.Details!, item.GetElementByTestId("details")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent.OldAlert.Details!, item.GetElementByTestId("old-details")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.ChangeReason : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("reason")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.ChangeReasonDetail : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("reason-detail")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? $"{alertUpdatedEvent.EvidenceFile!.Name} (opens in new tab)" : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("uploaded-evidence-link")?.TextContent);
                });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithAlertUpdatedEventExternalLinkChangedFromTrs_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByUser = await TestData.CreateUser();
        var person = await TestData.CreatePerson();
        var alert = await CreateEventAlertFromTrs(person.PersonId, populateOptional, isOpenAlert: true);
        var oldAlert = alert with { ExternalLink = populateOptional ? null : TestData.GenerateUrl() };

        var alertUpdatedEvent = await CreateAlertUpdatedEvent(person.PersonId, alert, oldAlert, AlertUpdatedEventChanges.ExternalLink, createdByUser.UserId, hasReason: populateOptional);
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(alertUpdatedEvent.Alert.AlertTypeId!.Value);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-updated-event"),
                item =>
                {
                    Assert.Equal("Alert link changed", item.GetElementByTestId("heading")?.TextContent.Trim());
                    Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertType!.Name, item.GetElementByTestId("alert-type")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? $"{alertUpdatedEvent.Alert!.ExternalLink} (opens in new tab)" : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("external-link")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? UiDefaults.EmptyDisplayContent : $"{alertUpdatedEvent.OldAlert!.ExternalLink} (opens in new tab)", item.GetElementByTestId("old-external-link")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.ChangeReason : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("reason")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.ChangeReasonDetail : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("reason-detail")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? $"{alertUpdatedEvent.EvidenceFile!.Name} (opens in new tab)" : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("uploaded-evidence-link")?.TextContent);
                });
    }

    [Theory]
    [InlineData(EndDateChangeType.Change)]
    [InlineData(EndDateChangeType.Close)]
    [InlineData(EndDateChangeType.Reopen)]
    public async Task Person_WithAlertUpdatedEventEndDateChangedFromDqt_RendersExpectedContent(EndDateChangeType changeType)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alert = await CreateEventAlertFromDqt(person.PersonId, populateOptional: true, isOpenAlert: changeType == EndDateChangeType.Reopen);
        var oldAlert = alert with { EndDate = changeType == EndDateChangeType.Close ? null : Clock.Today.AddDays(-6) };

        var alertUpdatedEvent = await CreateAlertUpdatedEvent(person.PersonId, alert, oldAlert, AlertUpdatedEventChanges.EndDate, createdByDqtUser, hasReason: false);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-updated-event"),
                item =>
                {
                    var expectedHeading = "Alert end date changed";
                    if (alertUpdatedEvent.Alert.EndDate is null)
                    {
                        expectedHeading = "Alert re-opened";
                    } 
                    else if (alertUpdatedEvent.OldAlert.EndDate is null)
                    {
                        expectedHeading = "Alert closed";
                    }

                    Assert.Equal(expectedHeading, item.GetElementByTestId("heading")?.TextContent.Trim());
                    Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.Alert.DqtSanctionCode!.Value, item.GetElementByTestId("sanction-code")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.Alert.DqtSanctionCode!.Name, item.GetElementByTestId("sanction-name")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(changeType != EndDateChangeType.Reopen ? alertUpdatedEvent.Alert.EndDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("end-date")?.TextContent.Trim());
                    Assert.Equal(changeType != EndDateChangeType.Close ? alertUpdatedEvent.OldAlert.EndDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : null, item.GetElementByTestId("old-end-date")?.TextContent.Trim());
                });
    }

    [Theory]
    [InlineData(EndDateChangeType.Change)]
    [InlineData(EndDateChangeType.Close)]
    [InlineData(EndDateChangeType.Reopen)]
    public async Task Person_WithAlertUpdatedEventEndDateChangedFromTrs_RendersExpectedContent(EndDateChangeType changeType)
    {
        // Arrange
        var createdByUser = await TestData.CreateUser();
        var person = await TestData.CreatePerson();
        var alert = await CreateEventAlertFromTrs(person.PersonId, populateOptional: true, isOpenAlert: changeType == EndDateChangeType.Reopen);
        var oldAlert = alert with { EndDate = changeType == EndDateChangeType.Close ? null : Clock.Today.AddDays(-6) };

        var alertUpdatedEvent = await CreateAlertUpdatedEvent(person.PersonId, alert, oldAlert, AlertUpdatedEventChanges.EndDate, createdByUser.UserId, hasReason: false);
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(alertUpdatedEvent.Alert.AlertTypeId!.Value);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-updated-event"),
                item =>
                {
                    var expectedHeading = "Alert end date changed";
                    if (alertUpdatedEvent.Alert.EndDate is null)
                    {
                        expectedHeading = "Alert re-opened";
                    }
                    else if (alertUpdatedEvent.OldAlert.EndDate is null)
                    {
                        expectedHeading = "Alert closed";
                    }

                    Assert.Equal(expectedHeading, item.GetElementByTestId("heading")?.TextContent.Trim());
                    Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertType!.Name, item.GetElementByTestId("alert-type")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(changeType != EndDateChangeType.Reopen ? alertUpdatedEvent.Alert.EndDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("end-date")?.TextContent.Trim());
                    Assert.Equal(changeType != EndDateChangeType.Close ? alertUpdatedEvent.OldAlert.EndDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : null, item.GetElementByTestId("old-end-date")?.TextContent.Trim());
                });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithAlertUpdatedEventDqtSpentChanged_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alert = await CreateEventAlertFromDqt(person.PersonId, populateOptional, isOpenAlert: true);
        var oldAlert = alert with { DqtSpent = populateOptional ? null : true };

        var alertUpdatedEvent = await CreateAlertUpdatedEvent(person.PersonId, alert, oldAlert, AlertUpdatedEventChanges.DqtSpent, createdByDqtUser, hasReason: false);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-updated-event"),
                item =>
                {
                    Assert.Equal("Alert DQT spent changed", item.GetElementByTestId("heading")?.TextContent.Trim());
                    Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.Alert.DqtSanctionCode!.Value, item.GetElementByTestId("sanction-code")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.Alert.DqtSanctionCode!.Name, item.GetElementByTestId("sanction-name")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? bool.TrueString : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("dqt-spent")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? UiDefaults.EmptyDisplayContent : bool.TrueString, item.GetElementByTestId("old-dqt-spent")?.TextContent.Trim());
                });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithAlertUpdatedEventDqtSanctionCodeChanged_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alert = await CreateEventAlertFromDqt(person.PersonId, populateOptional, isOpenAlert: true);
        var oldAlert = alert with { DqtSpent = populateOptional ? null : true };

        var alertUpdatedEvent = await CreateAlertUpdatedEvent(person.PersonId, alert, oldAlert, AlertUpdatedEventChanges.DqtSanctionCode, createdByDqtUser, hasReason: false);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-updated-event"),
                item =>
                {
                    Assert.Equal("Alert DQT sanction code changed", item.GetElementByTestId("heading")?.TextContent.Trim());
                    Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.Alert.DqtSanctionCode!.Value, item.GetElementByTestId("sanction-code")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.Alert.DqtSanctionCode!.Name, item.GetElementByTestId("sanction-name")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.OldAlert.DqtSanctionCode!.Value, item.GetElementByTestId("sanction-code")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.OldAlert.DqtSanctionCode!.Name, item.GetElementByTestId("sanction-name")?.TextContent.Trim());
                });
    }

    [Theory]
    [InlineData(false, null, true)]
    [InlineData(false, UserRoles.AlertsReadWrite, true)]
    [InlineData(true, null, false)]
    [InlineData(true, UserRoles.AlertsReadWrite, false)]
    [InlineData(true, UserRoles.DbsAlertsReadWrite, true)]
    [InlineData(true, UserRoles.DbsAlertsReadOnly, true)]
    [InlineData(true, UserRoles.Administrator, true)]
    public async Task Person_WithAlertUpdatedEventFromDqt_DisplaysAsExpectedForUserRole(bool isDbsAlertType, string? role, bool shouldDisplay)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role is not null ? [role!] : []));

        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alert = await CreateEventAlertFromDqt(person.PersonId, populateOptional: true, isOpenAlert: true, isDbsAlertType);
        var oldAlert = alert with { Details = "Old details" };

        var alertUpdatedEvent = await CreateAlertUpdatedEvent(person.PersonId, alert, oldAlert, AlertUpdatedEventChanges.Details, createdByDqtUser, hasReason: false);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var items = doc.GetAllElementsByTestId("timeline-item-alert-updated-event");

        if (shouldDisplay)
        {
            if (items.Count == 0)
            {
                Debug.Assert(false);
            }
            Assert.NotEmpty(items);
        }
        else
        {
            Assert.Empty(items);
        }
    }


    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithAlertUpdatedEventMultipleChanges_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var createdByDqtUser = EventModels.RaisedByUserInfo.FromDqtUser(dqtUserId: Guid.NewGuid(), dqtUserName: "DQT User");
        var person = await TestData.CreatePerson();
        var alert = await CreateEventAlertFromDqt(person.PersonId, populateOptional, isOpenAlert: true);
        var oldAlert = alert with { Details = populateOptional ? null : "Old details", DqtSpent = populateOptional ? null : true };

        var alertUpdatedEvent = await CreateAlertUpdatedEvent(person.PersonId, alert, oldAlert, AlertUpdatedEventChanges.Details | AlertUpdatedEventChanges.DqtSpent, createdByDqtUser, hasReason: false);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        Assert.Collection(
            doc.GetAllElementsByTestId("timeline-item-alert-updated-event"),
                item =>
                {
                    Assert.Equal("Alert changed", item.GetElementByTestId("heading")?.TextContent.Trim());
                    Assert.Equal($"By {createdByDqtUser.DqtUserName} on", item.GetElementByTestId("raised-by")?.TextContent.Trim());
                    Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.Alert.DqtSanctionCode!.Value, item.GetElementByTestId("sanction-code")?.TextContent.Trim());
                    Assert.Equal(alertUpdatedEvent!.Alert.DqtSanctionCode!.Name, item.GetElementByTestId("sanction-name")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.Alert.StartDate!.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("start-date")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? alertUpdatedEvent.Alert.Details : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("details")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? UiDefaults.EmptyDisplayContent : alertUpdatedEvent.OldAlert.Details, item.GetElementByTestId("old-details")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? bool.TrueString : UiDefaults.EmptyDisplayContent, item.GetElementByTestId("dqt-spent")?.TextContent.Trim());
                    Assert.Equal(populateOptional ? UiDefaults.EmptyDisplayContent : bool.TrueString, item.GetElementByTestId("old-dqt-spent")?.TextContent.Trim());
                });
    }

    private async Task<AlertCreatedEvent> CreateAlertCreatedEventFromDqt(Guid personId, EventModels.RaisedByUserInfo createdByUser, bool populateOptional, bool isOpenAlert, bool isDbsAlertType = false)
    {
        var alert = await CreateEventAlertFromDqt(personId, populateOptional, isOpenAlert, isDbsAlertType);

        var alertCreatedEvent = new AlertCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = personId,
            Alert = alert,
            AddReason = null,
            AddReasonDetail = null,
            EvidenceFile = null,
            RaisedBy = createdByUser,
            CreatedUtc = Clock.UtcNow
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEvent(alertCreatedEvent);
            await dbContext.SaveChangesAsync();
        });

        return alertCreatedEvent;
    }

    private async Task<AlertCreatedEvent> CreateAlertCreatedEventFromTrs(Guid personId, EventModels.RaisedByUserInfo createdByUser, bool populateOptional, bool isOpenAlert, bool isDbsAlertType = false)
    {
        var alert = await CreateEventAlertFromTrs(personId, populateOptional, isOpenAlert, isDbsAlertType);
        var reason = populateOptional ? AddAlertReasonOption.RoutineNotificationFromStakeholder.GetDisplayName() : null;
        var reasonDetail = populateOptional ? "Reason detail" : null;
        var evidenceFile = populateOptional ? new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        }
        : null;

        var alertCreatedEvent = new AlertCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = personId,
            Alert = alert,
            AddReason = reason,
            AddReasonDetail = reasonDetail,
            EvidenceFile = evidenceFile,
            RaisedBy = createdByUser,
            CreatedUtc = Clock.UtcNow
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEvent(alertCreatedEvent);
            await dbContext.SaveChangesAsync();
        });

        return alertCreatedEvent;
    }

    private async Task<AlertDeletedEvent> CreateAlertDeletedEvent(Guid personId, EventModels.RaisedByUserInfo createdByUser, bool populateOptional, bool isOpenAlert, bool isDbsAlertType = false)
    {
        var alert = await CreateEventAlertFromTrs(personId, populateOptional, isOpenAlert, isDbsAlertType);
        var reasonDetail = populateOptional ? "Reason detail" : null;
        var evidenceFile = populateOptional ? new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        }
        : null;

        var alertDeletedEvent = new AlertDeletedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = personId,
            Alert = alert,            
            DeletionReasonDetail = reasonDetail,
            EvidenceFile = evidenceFile,
            RaisedBy = createdByUser,
            CreatedUtc = Clock.UtcNow
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEvent(alertDeletedEvent);
            await dbContext.SaveChangesAsync();
        });

        return alertDeletedEvent;
    }

    private async Task<AlertDqtDeactivatedEvent> CreateAlertDqtDeactivatedEvent(Guid personId, EventModels.RaisedByUserInfo createdByUser, bool populateOptional, bool isOpenAlert, bool isDbsAlertType = false)
    {
        var alert = await CreateEventAlertFromDqt(personId, populateOptional, isOpenAlert, isDbsAlertType);        

        var alertDqtDeactivatedEvent = new AlertDqtDeactivatedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = personId,
            Alert = alert,
            RaisedBy = createdByUser,
            CreatedUtc = Clock.UtcNow
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEvent(alertDqtDeactivatedEvent);
            await dbContext.SaveChangesAsync();
        });

        return alertDqtDeactivatedEvent;
    }

    private async Task<AlertDqtImportedEvent> CreateAlertDqtImportedEvent(Guid personId, EventModels.RaisedByUserInfo createdByUser, bool populateOptional, bool isDbsAlertType = false)
    {
        var alert = await CreateEventAlertFromDqt(personId, populateOptional, isOpenAlert: true, isDbsAlertType);

        var alertDqtImportedEvent = new AlertDqtImportedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = personId,
            Alert = alert,
            RaisedBy = createdByUser,
            CreatedUtc = Clock.UtcNow,
            DqtState = 1
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEvent(alertDqtImportedEvent);
            await dbContext.SaveChangesAsync();
        });

        return alertDqtImportedEvent;
    }

    private async Task<AlertDqtReactivatedEvent> CreateAlertDqtReactivatedEvent(Guid personId, EventModels.RaisedByUserInfo createdByUser, bool populateOptional, bool isDbsAlertType = false)
    {
        var alert = await CreateEventAlertFromDqt(personId, populateOptional, isOpenAlert: true, isDbsAlertType);

        var alertDqtReactivatedEvent = new AlertDqtReactivatedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = personId,
            Alert = alert,
            RaisedBy = createdByUser,
            CreatedUtc = Clock.UtcNow
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEvent(alertDqtReactivatedEvent);
            await dbContext.SaveChangesAsync();
        });

        return alertDqtReactivatedEvent;
    }

    private async Task<AlertMigratedEvent> CreateAlertMigratedEvent(Guid personId, EventModels.RaisedByUserInfo createdByUser, bool populateOptional, bool isDbsAlertType = false)
    {
        var alert = await CreateEventAlertFromTrs(personId, populateOptional, isOpenAlert: true, isDbsAlertType);
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(alert.AlertTypeId!.Value);
        var dqtSanctionCode = await TestData.ReferenceDataCache.GetSanctionCodeByValue(alertType.DqtSanctionCode!);
        var oldAlert = alert with
        {
            AlertId = Guid.NewGuid(),
            DqtSanctionCode = populateOptional ? new AlertDqtSanctionCode
            {
                SanctionCodeId = dqtSanctionCode!.Id,
                Name = dqtSanctionCode.dfeta_name,
                Value = dqtSanctionCode.dfeta_Value
            }
            : null,
            DqtSpent = populateOptional ? true : (bool?)null,
        };

        var alertMigratedEvent = new AlertMigratedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = personId,
            Alert = alert,
            OldAlert = oldAlert,
            RaisedBy = createdByUser,
            CreatedUtc = Clock.UtcNow
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEvent(alertMigratedEvent);
            await dbContext.SaveChangesAsync();
        });

        return alertMigratedEvent;
    }

    private async Task<AlertUpdatedEvent> CreateAlertUpdatedEvent(Guid personId, EventModels.Alert alert, EventModels.Alert oldAlert, AlertUpdatedEventChanges changes, EventModels.RaisedByUserInfo createdByUser, bool hasReason)
    {
        var alertUpdatedEvent = new AlertUpdatedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = personId,
            Alert = alert,
            OldAlert = oldAlert,
            RaisedBy = createdByUser,
            CreatedUtc = Clock.UtcNow,
            Changes = changes,
            ChangeReason = hasReason ? "Change reason" : null,
            ChangeReasonDetail = hasReason ? "Change reason detail" : null,
            EvidenceFile = hasReason ? new EventModels.File
            {
                FileId = Guid.NewGuid(),
                Name = "evidence.jpg"
            }
        : null
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEvent(alertUpdatedEvent);
            await dbContext.SaveChangesAsync();
        });

        return alertUpdatedEvent;
    }

    private async Task<EventModels.Alert> CreateEventAlertFromTrs(Guid personId, bool populateOptional, bool isOpenAlert, bool isDbsAlertType = false)
    {
        var dbsAlertType = await TestData.ReferenceDataCache.GetAlertTypeById(AlertType.DbsAlertTypeId);
        var alertType = (await TestData.ReferenceDataCache.GetAlertTypes()).Where(t => t.IsDbsAlertType == isDbsAlertType).RandomOne();
        
        var alertDetails = "My alert details";
        var externalLink = populateOptional ? TestData.GenerateUrl() : null;
        var startDate = Clock.Today.AddDays(-30);
        var endDate = isOpenAlert ? (DateOnly?)null : Clock.Today.AddDays(-5);

        var alert = new EventModels.Alert
        {
            AlertId = Guid.NewGuid(),
            AlertTypeId = alertType?.AlertTypeId,
            Details = alertDetails,
            ExternalLink = externalLink,
            StartDate = startDate,
            EndDate = endDate,
            DqtSpent = null,
            DqtSanctionCode = null
        };

        return alert;
    }

    private async Task<EventModels.Alert> CreateEventAlertFromDqt(Guid personId, bool populateOptional, bool isOpenAlert, bool isDbsAlertType = false)
    {
        var dbsAlertType = await TestData.ReferenceDataCache.GetAlertTypeById(AlertType.DbsAlertTypeId);
        var dqtSanctionCode = (await TestData.ReferenceDataCache.GetSanctionCodes(activeOnly: false)).Where(s => isDbsAlertType ? s.dfeta_Value == dbsAlertType.DqtSanctionCode : s.dfeta_Value != dbsAlertType.DqtSanctionCode).RandomOne();
        var alertDqtSanctionCode = new AlertDqtSanctionCode
        {
            SanctionCodeId = dqtSanctionCode!.Id,
            Name = dqtSanctionCode.dfeta_name,
            Value = dqtSanctionCode.dfeta_Value
        };

        var dqtSpent = populateOptional ? true : (bool?)null;
        var alertDetails = populateOptional ? "My alert details" : null;
        var startDate = populateOptional ? Clock.Today.AddDays(-30) : (DateOnly?)null;
        var endDate = isOpenAlert ? (DateOnly?)null : Clock.Today.AddDays(-5);

        var alert = new EventModels.Alert
        {
            AlertId = Guid.NewGuid(),
            AlertTypeId = null,
            Details = alertDetails,
            ExternalLink = null,
            StartDate = startDate,
            EndDate = endDate,
            DqtSpent = dqtSpent,
            DqtSanctionCode = alertDqtSanctionCode
        };

        return alert;
    }

    public enum EndDateChangeType
    {
        Change,
        Close,
        Reopen
    }
}
