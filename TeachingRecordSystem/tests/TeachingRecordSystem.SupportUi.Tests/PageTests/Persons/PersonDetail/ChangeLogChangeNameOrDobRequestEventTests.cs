using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogChangeNameOrDobRequestEventTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task ChangeNameRequestSupportTaskApprovedEvent_IsDisplayedInChangeLog()
    {
        // Arrange
        var user = TestData.CreateOneLoginUserSubject();
        var raisedByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var supportTaskRequestData = new Core.Models.SupportTasks.ChangeNameRequestData
        {
            ChangeRequestOutcome = null,
            EmailAddress = null,
            EvidenceFileId = Guid.NewGuid(),
            EvidenceFileName = "evidence.pdf",
            FirstName = TestData.GenerateFirstName(),
            MiddleName = TestData.GenerateMiddleName(),
            LastName = TestData.GenerateLastName()
        };

        var requestData = EventModels.ChangeNameRequestData.FromModel(supportTaskRequestData);

        var oldSupportTask = new SupportTask
        {
            Data = supportTaskRequestData,
            OneLoginUserSubject = user,
            PersonId = person.PersonId,
            Status = SupportTaskStatus.Open,
            SupportTaskReference = "REF1",
            SupportTaskType = SupportTaskType.ChangeNameRequest
        };
        var supportTask = new SupportTask
        {
            Data = supportTaskRequestData with { ChangeRequestOutcome = SupportRequestOutcome.Approved },
            OneLoginUserSubject = user,
            PersonId = person.PersonId,
            Status = SupportTaskStatus.Closed,
            SupportTaskReference = "REF1",
            SupportTaskType = SupportTaskType.ChangeNameRequest
        };

        var updatedEvent = new ChangeNameRequestSupportTaskApprovedEvent()
        {
            PersonId = person!.PersonId,
            RequestData = requestData!,
            Changes = ChangeNameRequestSupportTaskApprovedEventChanges.FirstName
                      | ChangeNameRequestSupportTaskApprovedEventChanges.MiddleName
                      | ChangeNameRequestSupportTaskApprovedEventChanges.LastName,
            OldPersonAttributes = new PersonDetails
            {
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                DateOfBirth = null,
                EmailAddress = null,
                Gender = null,
                NationalInsuranceNumber = null
            },
            PersonAttributes = new PersonDetails
            {
                FirstName = requestData.FirstName!,
                MiddleName = requestData.MiddleName!,
                LastName = requestData.LastName!,
                DateOfBirth = null,
                EmailAddress = null,
                Gender = null,
                NationalInsuranceNumber = null
            },
            SupportTask = supportTask,
            OldSupportTask = oldSupportTask,
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = raisedByUser.UserId
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var oldName = StringHelper.JoinNonEmpty(' ', [person.FirstName, person.MiddleName, person.LastName]);

        var newName = StringHelper.JoinNonEmpty(' ', [requestData!.FirstName, requestData.MiddleName, requestData.LastName]);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-name-request-approved-event");
        Assert.NotNull(item);

        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);

        item.AssertSummaryListRowValue("details", "Name", v => Assert.Equal(newName, v.TrimmedText()));
        item.AssertSummaryListRowValue("previous-details", "Name", v => Assert.Equal(oldName, v.TrimmedText()));
    }

    [Fact]
    public async Task ChangeNameRequestSupportTaskRejectedEvent_IsDisplayedInChangeLog()
    {
        // Arrange
        var user = TestData.CreateOneLoginUserSubject();
        var raisedByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var supportTaskRequestData = new Core.Models.SupportTasks.ChangeNameRequestData
        {
            ChangeRequestOutcome = null,
            EmailAddress = null,
            EvidenceFileId = Guid.NewGuid(),
            EvidenceFileName = "evidence.pdf",
            FirstName = "NewFirstName",
            MiddleName = "NewMiddleName",
            LastName = "NewLastName"
        };

        var requestData = EventModels.ChangeNameRequestData.FromModel(supportTaskRequestData);

        var oldSupportTask = new SupportTask
        {
            Data = supportTaskRequestData,
            OneLoginUserSubject = user,
            PersonId = person.PersonId,
            Status = SupportTaskStatus.Open,
            SupportTaskReference = "REF1",
            SupportTaskType = SupportTaskType.ChangeNameRequest
        };
        var supportTask = new SupportTask
        {
            Data = supportTaskRequestData with { ChangeRequestOutcome = SupportRequestOutcome.Rejected },
            OneLoginUserSubject = user,
            PersonId = person.PersonId,
            Status = SupportTaskStatus.Closed,
            SupportTaskReference = "REF1",
            SupportTaskType = SupportTaskType.ChangeNameRequest
        };

        var updatedEvent = new ChangeNameRequestSupportTaskRejectedEvent()
        {
            PersonId = person!.PersonId,
            RequestData = requestData!,
            SupportTask = supportTask,
            OldSupportTask = oldSupportTask,
            RejectionReason = "No evidence supplied",
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = raisedByUser.UserId
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var oldName = StringHelper.JoinNonEmpty(' ', [person.FirstName, person.MiddleName, person.LastName]);

        var newName = StringHelper.JoinNonEmpty(' ', [requestData!.FirstName, requestData.MiddleName, requestData.LastName]);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-name-request-rejected-event");
        Assert.NotNull(item);

        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);

        item.AssertSummaryListRowValue("details", "Date of request", v => Assert.Equal(updatedEvent.CreatedUtc.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        item.AssertSummaryListRowValue("reason", "Reason", v => Assert.Equal("No evidence supplied", v.TrimmedText()));
    }

    [Fact]
    public async Task ChangeDobRequestSupportTaskAcceptedEvent_IsDisplayedInChangeLog()
    {
        // Arrange
        var user = TestData.CreateOneLoginUserSubject();
        var raisedByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var supportTaskRequestData = new Core.Models.SupportTasks.ChangeDateOfBirthRequestData
        {
            ChangeRequestOutcome = null,
            EmailAddress = null,
            EvidenceFileId = Guid.NewGuid(),
            EvidenceFileName = "evidence.pdf",
            DateOfBirth = new DateOnly(1990, 1, 1)
        };
        var requestData = EventModels.ChangeDateOfBirthRequestData.FromModel(supportTaskRequestData);
        var oldSupportTask = new SupportTask
        {
            Data = supportTaskRequestData,
            OneLoginUserSubject = user,
            PersonId = person.PersonId,
            Status = SupportTaskStatus.Open,
            SupportTaskReference = "REF1",
            SupportTaskType = SupportTaskType.ChangeDateOfBirthRequest
        };
        var supportTask = new SupportTask
        {
            Data = supportTaskRequestData with { ChangeRequestOutcome = SupportRequestOutcome.Approved },
            OneLoginUserSubject = user,
            PersonId = person.PersonId,
            Status = SupportTaskStatus.Closed,
            SupportTaskReference = "REF1",
            SupportTaskType = SupportTaskType.ChangeDateOfBirthRequest
        };

        var updatedEvent = new ChangeDateOfBirthRequestSupportTaskApprovedEvent()
        {
            PersonId = person!.PersonId,
            RequestData = requestData!,
            SupportTask = supportTask,
            OldSupportTask = oldSupportTask,
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = raisedByUser.UserId,
            Changes = ChangeDateOfBirthRequestSupportTaskApprovedEventChanges.DateOfBirth,
            OldPersonAttributes = new PersonDetails
            {
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                DateOfBirth = person.DateOfBirth,
                EmailAddress = null,
                Gender = null,
                NationalInsuranceNumber = null
            },
            PersonAttributes = new PersonDetails
            {
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                DateOfBirth = requestData.DateOfBirth,
                EmailAddress = null,
                Gender = null,
                NationalInsuranceNumber = null
            },
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-dob-request-approved-event");
        Assert.NotNull(item);

        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);

        item.AssertSummaryListRowValue("details", "Date of birth", v => Assert.Equal(requestData.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        item.AssertSummaryListRowValue("previous-details", "Date of birth", v => Assert.Equal(person.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
    }

    [Fact]
    public async Task ChangeDobRequestSupportTaskRejectedEvent_IsDisplayedInChangeLog()
    {
        // Arrange
        var user = TestData.CreateOneLoginUserSubject();
        var raisedByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var supportTaskRequestData = new Core.Models.SupportTasks.ChangeDateOfBirthRequestData
        {
            ChangeRequestOutcome = null,
            EmailAddress = null,
            EvidenceFileId = Guid.NewGuid(),
            EvidenceFileName = "evidence.pdf",
            DateOfBirth = new DateOnly(1990, 1, 1)
        };
        var requestData = EventModels.ChangeDateOfBirthRequestData.FromModel(supportTaskRequestData);
        var oldSupportTask = new SupportTask
        {
            Data = supportTaskRequestData,
            OneLoginUserSubject = user,
            PersonId = person.PersonId,
            Status = SupportTaskStatus.Open,
            SupportTaskReference = "REF1",
            SupportTaskType = SupportTaskType.ChangeDateOfBirthRequest
        };
        var supportTask = new SupportTask
        {
            Data = supportTaskRequestData with { ChangeRequestOutcome = SupportRequestOutcome.Rejected },
            OneLoginUserSubject = user,
            PersonId = person.PersonId,
            Status = SupportTaskStatus.Closed,
            SupportTaskReference = "REF1",
            SupportTaskType = SupportTaskType.ChangeDateOfBirthRequest
        };

        var updatedEvent = new ChangeDateOfBirthRequestSupportTaskRejectedEvent()
        {
            PersonId = person!.PersonId,
            RequestData = requestData!,
            SupportTask = supportTask,
            OldSupportTask = oldSupportTask,
            RejectionReason = "No evidence supplied",
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = raisedByUser.UserId
        };

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-dob-request-rejected-event");
        Assert.NotNull(item);

        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);

        item.AssertSummaryListRowValue("details", "Date of request", v => Assert.Equal(updatedEvent.CreatedUtc.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        item.AssertSummaryListRowValue("reason", "Reason", v => Assert.Equal("No evidence supplied", v.TrimmedText()));
    }
}
