using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogNpqTrnRequestSupportTaskResolvedEventTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private string _oldFirstName = "Alfred";
    private string _oldMiddleName = "The";
    private string _oldLastName = "Great";
    private DateOnly _oldDob;
    private string _oldEmail = "old@email-address.com";
    private string _oldNino = "AB 12 34 56 D";
    private Gender _oldGender = Gender.Male;

    private string _firstName = "Megan";
    private string _middleName = "Thee";
    private string _lastName = "Stallion";
    private DateOnly _dob;
    private string _email = "new@email-address.com";
    private string _nino = "XY 98 76 54 A";
    private Gender _gender = Gender.Female;

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

        _oldDob = Clock.Today.AddYears(-30);
        _dob = Clock.Today.AddYears(-20);
    }

    [Test]
    [MatrixDataSource]
    public async Task Person_WithNpqTrnRequestSupportTaskResolvedEvent_RendersExpectedContent(
        [Matrix(NpqTrnRequestSupportTaskResolvedEventChanges.PersonFirstName,
            NpqTrnRequestSupportTaskResolvedEventChanges.PersonMiddleName,
            NpqTrnRequestSupportTaskResolvedEventChanges.PersonLastName,
            NpqTrnRequestSupportTaskResolvedEventChanges.PersonDateOfBirth,
            NpqTrnRequestSupportTaskResolvedEventChanges.PersonEmailAddress,
            NpqTrnRequestSupportTaskResolvedEventChanges.PersonNationalInsuranceNumber,
            NpqTrnRequestSupportTaskResolvedEventChanges.PersonGender,
            NpqTrnRequestSupportTaskResolvedEventChanges.AllChanges)]
        NpqTrnRequestSupportTaskResolvedEventChanges changes,
        [Matrix(false, true)] bool previousValueIsDefault,
        [Matrix(false, true)] bool newValueIsDefault,
        [Matrix(NpqTrnRequestResolvedReason.RecordCreated, NpqTrnRequestResolvedReason.RecordMerged)] NpqTrnRequestResolvedReason reason)
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync("Apply for QTS", "AfQTS");
        var person = await TestData.CreatePersonAsync();
        var updateComments = TestData.GenerateLoremIpsum();

        string? oldEmail = previousValueIsDefault ? null : _oldEmail;
        string? oldNino = previousValueIsDefault ? null : _oldNino;
        Gender? oldGender = previousValueIsDefault ? null : _oldGender;

        string? email = newValueIsDefault ? null : _email;
        string? nino = newValueIsDefault ? null : _nino;
        Gender? gender = newValueIsDefault ? null : _gender;

        var newFirstName = changes.HasFlag(NpqTrnRequestSupportTaskResolvedEventChanges.PersonFirstName) ? _firstName : _oldFirstName;
        var newMiddleName = changes.HasFlag(NpqTrnRequestSupportTaskResolvedEventChanges.PersonMiddleName) ? _middleName : _oldMiddleName;
        var newLastName = changes.HasFlag(NpqTrnRequestSupportTaskResolvedEventChanges.PersonLastName) ? _lastName : _oldLastName;
        var newDob = changes.HasFlag(NpqTrnRequestSupportTaskResolvedEventChanges.PersonDateOfBirth) ? _dob : _oldDob;
        var newEmail = changes.HasFlag(NpqTrnRequestSupportTaskResolvedEventChanges.PersonEmailAddress) ? email : oldEmail;
        var newNino = changes.HasFlag(NpqTrnRequestSupportTaskResolvedEventChanges.PersonNationalInsuranceNumber) ? nino : oldNino;
        var newGender = changes.HasFlag(NpqTrnRequestSupportTaskResolvedEventChanges.PersonGender) ? gender : oldGender;

        await CreateEvent(createdByUser.UserId, person.PersonId, applicationUser.UserId,
            newFirstName, _oldFirstName,
            newMiddleName, _oldMiddleName,
            newLastName, _oldLastName,
            newDob, _oldDob,
            newEmail, oldEmail,
            newNino, oldNino,
            newGender, oldGender,
            changes, reason, reason == NpqTrnRequestResolvedReason.RecordMerged ? updateComments : null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-details-updated-event");
        Assert.NotNull(item);

        var title = item.QuerySelector(".moj-timeline__title");

        Assert.NotNull(title);

        if (reason == NpqTrnRequestResolvedReason.RecordCreated)
        {
            Assert.Equal("Record created from Apply for QTS TRN request", title.TrimmedText());
        }
        else
        {
            Assert.Equal("Record updated from Apply for QTS TRN request", title.TrimmedText());
        }

        Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());

        if (changes.HasAnyFlag(NpqTrnRequestSupportTaskResolvedEventChanges.PersonNameChange))
        {
            item.AssertRow("details", "Name", v => Assert.Equal($"{newFirstName} {newMiddleName} {newLastName}", v.TrimmedText()));
            item.AssertRow("previous-details", "Name", v => Assert.Equal($"{_oldFirstName} {_oldMiddleName} {_oldLastName}", v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "Name");
            item.AssertRowDoesNotExist("previous-details", "Name");
        }

        if (changes.HasFlag(NpqTrnRequestSupportTaskResolvedEventChanges.PersonDateOfBirth))
        {
            item.AssertRow("details", "Date of birth", v => Assert.Equal(newDob.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
            item.AssertRow("previous-details", "Date of birth", v => Assert.Equal(_oldDob.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "Date of birth");
            item.AssertRowDoesNotExist("previous-details", "Date of birth");
        }

        if (changes.HasFlag(NpqTrnRequestSupportTaskResolvedEventChanges.PersonEmailAddress))
        {
            item.AssertRow("details", "Email address", v => Assert.Equal(newEmail ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
            item.AssertRow("previous-details", "Email address", v => Assert.Equal(oldEmail ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "Email address");
            item.AssertRowDoesNotExist("previous-details", "Email address");
        }

        if (changes.HasFlag(NpqTrnRequestSupportTaskResolvedEventChanges.PersonNationalInsuranceNumber))
        {
            item.AssertRow("details", "National Insurance number", v => Assert.Equal(newNino ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
            item.AssertRow("previous-details", "National Insurance number", v => Assert.Equal(oldNino ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "National Insurance number");
            item.AssertRowDoesNotExist("previous-details", "National Insurance number");
        }

        if (changes.HasFlag(NpqTrnRequestSupportTaskResolvedEventChanges.PersonGender))
        {
            item.AssertRow("details", "Gender", v => Assert.Equal(newGender?.GetDisplayName() ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
            item.AssertRow("previous-details", "Gender", v => Assert.Equal(oldGender?.GetDisplayName() ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "Gender");
            item.AssertRowDoesNotExist("previous-details", "Gender");
        }

        if (reason == NpqTrnRequestResolvedReason.RecordCreated)
        {
            item.AssertRowDoesNotExist("change-reason", "Comments");
        }
        else
        {
            item.AssertRow("change-reason", "Comments", v => Assert.Equal(updateComments, v.TrimmedText()));
        }

        item.AssertRow("request-data", "Source", v => Assert.Equal("AfQTS", v.TrimmedText()));
        item.AssertRow("request-data", "Request ID", v => Assert.Equal("TEST-TRN-1", v.TrimmedText()));
        item.AssertRow("request-data", "Created on", v => Assert.Equal(Clock.UtcNow.ToString(UiDefaults.DateTimeDisplayFormat), v.TrimmedText()));
        item.AssertRow("request-data", "Name", v => Assert.Equal($"{newFirstName} {newMiddleName} {newLastName}", v.TrimmedText()));
        item.AssertRow("request-data", "Date of birth", v => Assert.Equal(newDob.ToString(UiDefaults.DateOnlyDisplayFormat) ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        item.AssertRow("request-data", "Email address", v => Assert.Equal(newEmail ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        item.AssertRow("request-data", "National Insurance number", v => Assert.Equal(newNino ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        item.AssertRow("request-data", "Gender", v => Assert.Equal(newGender?.GetDisplayName() ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
    }

    [Test]
    [Arguments(NpqTrnRequestResolvedReason.RecordCreated)]
    [Arguments(NpqTrnRequestResolvedReason.RecordMerged)]
    public async Task Person_WithNpqTrnRequestSupportTaskResolvedEvent_WithUnknownApplicationSource_RendersExpectedContent(NpqTrnRequestResolvedReason reason)
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        await CreateEvent(createdByUser.UserId, person.PersonId, applicationUserId: Guid.NewGuid(),
            _firstName, _oldFirstName,
            _middleName, _oldMiddleName,
            _lastName, _oldLastName,
            NpqTrnRequestSupportTaskResolvedEventChanges.PersonNameChange, reason, "Some comments");

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-details-updated-event");
        Assert.NotNull(item);

        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);

        if (reason == NpqTrnRequestResolvedReason.RecordCreated)
        {
            Assert.Equal("Record created from TRN request of unknown source", title.TrimmedText());
        }
        else
        {
            Assert.Equal("Record updated from TRN request of unknown source", title.TrimmedText());
        }

        item.AssertRow("request-data", "Source", v => Assert.Equal("Not provided", v.TrimmedText()));
    }

    [Test]
    [Arguments(NpqTrnRequestResolvedReason.RecordCreated)]
    [Arguments(NpqTrnRequestResolvedReason.RecordMerged)]
    public async Task Person_WithNpqTrnRequestSupportTaskResolvedEvent_ChangeReason_RendersExpectedContent(NpqTrnRequestResolvedReason reason)
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        await CreateEvent(createdByUser.UserId, person.PersonId, applicationUserId: Guid.NewGuid(),
            _firstName, _oldFirstName,
            _middleName, _oldMiddleName,
            _lastName, _oldLastName,
            NpqTrnRequestSupportTaskResolvedEventChanges.PersonNameChange, reason, "Some comments");

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-details-updated-event");
        Assert.NotNull(item);

        item.AssertRow("change-reason", "Reason", v => Assert.Equal(reason.GetDisplayName(), v.TrimmedText()));
    }

    private Task CreateEvent(Guid createdByUserId, Guid personId, Guid applicationUserId,
        string firstName, string oldFirstName,
        string middleName, string oldMiddleName,
        string lastName, string oldLastName,
        NpqTrnRequestSupportTaskResolvedEventChanges changes, NpqTrnRequestResolvedReason changeReason, string? comments)
    {
        return CreateEvent(createdByUserId, personId, applicationUserId,
            firstName, oldFirstName, middleName, oldMiddleName, lastName, oldLastName,
            null, null, null, null, null, null, null, null,
            changes, changeReason, comments);
    }

    private Task CreateEvent(Guid createdByUserId, Guid personId, Guid applicationUserId,
        string firstName, string oldFirstName,
        string middleName, string oldMiddleName,
        string lastName, string oldLastName,
        DateOnly? dob, DateOnly? oldDob,
        string? email, string? oldEmail,
        string? nino, string? oldNino,
        Gender? gender, Gender? oldGender,
        NpqTrnRequestSupportTaskResolvedEventChanges changes, NpqTrnRequestResolvedReason changeReason, string? comments)
    {
        var attributes = new EventModels.PersonAttributes
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dob,
            EmailAddress = email,
            NationalInsuranceNumber = nino,
            Gender = gender
        };

        var oldAttributes = new EventModels.PersonAttributes
        {
            FirstName = oldFirstName,
            MiddleName = oldMiddleName,
            LastName = oldLastName,
            DateOfBirth = oldDob,
            EmailAddress = oldEmail,
            NationalInsuranceNumber = oldNino,
            Gender = oldGender
        };

        var supportTask = new EventModels.SupportTask
        {
            PersonId = personId,
            SupportTaskReference = "TEST-ST-1",
            SupportTaskType = SupportTaskType.ApiTrnRequest,
            OneLoginUserSubject = null,
            Status = SupportTaskStatus.Closed,
            Data = new NpqTrnRequestData()
        };

        var oldSupportTask = new EventModels.SupportTask
        {
            PersonId = personId,
            SupportTaskReference = "TEST-ST-1",
            SupportTaskType = SupportTaskType.ApiTrnRequest,
            OneLoginUserSubject = null,
            Status = SupportTaskStatus.Open,
            Data = new NpqTrnRequestData()
        };

        var requestData = new EventModels.TrnRequestMetadata
        {
            ApplicationUserId = applicationUserId,
            RequestId = "TEST-TRN-1",
            CreatedOn = Clock.UtcNow,
            IdentityVerified = null,
            EmailAddress = email,
            OneLoginUserSubject = null,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            PreviousFirstName = "Jim",
            PreviousLastName = "Smith",
            Name = [firstName, middleName, lastName],
            DateOfBirth = dob ?? DateOnly.Parse("1 Jan 2000"),
            PotentialDuplicate = null,
            NationalInsuranceNumber = nino,
            Gender = gender,
            AddressLine1 = "1 Test Place",
            AddressLine2 = "Test Street",
            AddressLine3 = "Testborough",
            City = "Testington",
            Postcode = "TE57 1NG",
            Country = "Testland",
            TrnToken = null,
            ResolvedPersonId = null,
            Matches = null,
            NpqApplicationId = null,
            NpqEvidenceFileId = null,
            NpqEvidenceFileName = null,
            NpqName = null,
            NpqTrainingProvider = null,
            NpqWorkingInEducationalSetting = null
        };

        var updatedEvent = new NpqTrnRequestSupportTaskResolvedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUserId,
            PersonId = personId,
            PersonAttributes = attributes,
            OldPersonAttributes = oldAttributes,
            SupportTask = supportTask,
            OldSupportTask = oldSupportTask,
            RequestData = requestData,
            Changes = changes,
            ChangeReason = changeReason,
            Comments = comments
        };

        return WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });
    }
}
