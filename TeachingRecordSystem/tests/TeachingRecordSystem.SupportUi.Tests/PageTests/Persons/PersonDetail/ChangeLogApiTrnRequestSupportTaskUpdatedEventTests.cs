using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogApiTrnRequestSupportTaskUpdatedEventTests : TestBase
{
    private string _oldFirstName;
    private string _oldMiddleName;
    private string _oldLastName;
    private DateOnly _oldDob;
    private string _oldEmail;
    private string _oldNino;
    private Gender _oldGender;

    private string _firstName;
    private string _middleName;
    private string _lastName;
    private DateOnly _dob;
    private string _email;
    private string _nino;
    private Gender _gender;

    public ChangeLogApiTrnRequestSupportTaskUpdatedEventTests(HostFixture hostFixture) : base(hostFixture)
    {
        // Toggle between GMT and BST to ensure we're testing rendering dates in local time
        var nows = new[]
        {
            new DateTime(2024, 1, 1, 12, 13, 14, DateTimeKind.Utc),  // GMT
            new DateTime(2024, 7, 5, 19, 20, 21, DateTimeKind.Utc)   // BST
        };
        Clock.UtcNow = nows.RandomOne();

        _oldFirstName = "Alfred";
        _oldMiddleName = "The";
        _oldLastName = "Great";
        _oldDob = Clock.Today.AddYears(-30);
        _oldEmail = "old@email-address.com";
        _oldNino = "AB 12 34 56 D";
        _oldGender = Gender.Male;

        _firstName = "Megan";
        _middleName = "Thee";
        _lastName = "Stallion";
        _dob = Clock.Today.AddYears(-20);
        _email = "new@email-address.com";
        _nino = "XY 98 76 54 A";
        _gender = Gender.Female;
    }

    [Theory]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonFirstName, false, false)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonMiddleName, false, false)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonLastName, false, false)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonDateOfBirth, false, false)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress, false, false)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress, true, false)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress, false, true)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber, false, false)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber, false, true)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber, true, false)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonGender, false, false)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonGender, false, true)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonGender, true, false)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.AllChanges, false, false)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.AllChanges, false, true)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.AllChanges, true, false)]
    public async Task Person_WithPersonDetailsUpdatedEvent_RendersExpectedContent(ApiTrnRequestSupportTaskUpdatedEventChanges changes, bool previousValueIsDefault, bool newValueIsDefault)
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync("Apply for QTS", "AfQTS");
        var person = await TestData.CreatePersonAsync();

        string? oldEmail = previousValueIsDefault ? null : _oldEmail;
        string? oldNino = previousValueIsDefault ? null : _oldNino;
        Gender? oldGender = previousValueIsDefault ? null : _oldGender;

        string? email = newValueIsDefault ? null : _email;
        string? nino = newValueIsDefault ? null : _nino;
        Gender? gender = newValueIsDefault ? null : _gender;

        var newFirstName = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonFirstName) ? _firstName : _oldFirstName;
        var newMiddleName = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonMiddleName) ? _middleName : _oldMiddleName;
        var newLastName = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonLastName) ? _lastName : _oldLastName;
        var newDob = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonDateOfBirth) ? _dob : _oldDob;
        var newEmail = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress) ? email : oldEmail;
        var newNino = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber) ? nino : oldNino;
        var newGender = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonGender) ? gender : oldGender;

        await CreateEvent(createdByUser.UserId, person.PersonId, applicationUser.UserId,
            newFirstName, _oldFirstName,
            newMiddleName, _oldMiddleName,
            newLastName, _oldLastName,
            newDob, _oldDob,
            newEmail, oldEmail,
            newNino, oldNino,
            newGender, oldGender,
            changes, "Some comments");

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-details-updated-event");
        Assert.NotNull(item);

        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);
        Assert.Equal("Record updated from Apply for QTS TRN request", title.TrimmedText());

        Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());

        if (changes.HasAnyFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNameChange))
        {
            item.AssertRow("details", "Name", v => Assert.Equal($"{newFirstName} {newMiddleName} {newLastName}", v.TrimmedText()));
            item.AssertRow("previous-details", "Name", v => Assert.Equal($"{_oldFirstName} {_oldMiddleName} {_oldLastName}", v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "Name");
            item.AssertRowDoesNotExist("previous-details", "Name");
        }

        if (changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonDateOfBirth))
        {
            item.AssertRow("details", "Date of birth", v => Assert.Equal(newDob.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
            item.AssertRow("previous-details", "Date of birth", v => Assert.Equal(_oldDob.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "Date of birth");
            item.AssertRowDoesNotExist("previous-details", "Date of birth");
        }

        if (changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress))
        {
            item.AssertRow("details", "Email address", v => Assert.Equal(newEmail ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
            item.AssertRow("previous-details", "Email address", v => Assert.Equal(oldEmail ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "Email address");
            item.AssertRowDoesNotExist("previous-details", "Email address");
        }

        if (changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber))
        {
            item.AssertRow("details", "National Insurance number", v => Assert.Equal(newNino ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
            item.AssertRow("previous-details", "National Insurance number", v => Assert.Equal(oldNino ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "National Insurance number");
            item.AssertRowDoesNotExist("previous-details", "National Insurance number");
        }

        if (changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonGender))
        {
            item.AssertRow("details", "Gender", v => Assert.Equal(newGender?.GetDisplayName() ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
            item.AssertRow("previous-details", "Gender", v => Assert.Equal(oldGender?.GetDisplayName() ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "Gender");
            item.AssertRowDoesNotExist("previous-details", "Gender");
        }

        item.AssertRow("change-reason", "Reason", v => Assert.Equal("Identified as same person during task resolution", v.TrimmedText()));
        item.AssertRow("change-reason", "Comments", v => Assert.Equal("Some comments", v.TrimmedText()));

        item.AssertRow("request-data", "Source", v => Assert.Equal("AfQTS", v.TrimmedText()));
        item.AssertRow("request-data", "Request ID", v => Assert.Equal("TEST-TRN-1", v.TrimmedText()));
        item.AssertRow("request-data", "Created on", v => Assert.Equal(Clock.UtcNow.ToString(UiDefaults.DateTimeDisplayFormat), v.TrimmedText()));
        item.AssertRow("request-data", "Name", v => Assert.Equal($"{newFirstName} {newMiddleName} {newLastName}", v.TrimmedText()));
        item.AssertRow("request-data", "Previous name", v => Assert.Equal("Jim Smith", v.TrimmedText()));
        item.AssertRow("request-data", "Date of birth", v => Assert.Equal(newDob.ToString(UiDefaults.DateOnlyDisplayFormat) ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        item.AssertRow("request-data", "Email address", v => Assert.Equal(newEmail ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        item.AssertRow("request-data", "National Insurance number", v => Assert.Equal(newNino ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        item.AssertRow("request-data", "Gender", v => Assert.Equal(newGender?.GetDisplayName() ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        item.AssertRow("request-data", "Address", v => Assert.Equal("<p class=\"govuk-body\">1 Test Place<br>Test Street<br>Testborough<br>Testington<br>TE57 1NG<br>Testland</p>", v.InnerHtml.Trim()));
    }

    [Fact]
    public async Task Person_WithUnknownApplicationSource_RendersExpectedContent()
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        await CreateEvent(createdByUser.UserId, person.PersonId, applicationUserId: Guid.NewGuid(),
            _firstName, _oldFirstName,
            _middleName, _oldMiddleName,
            _lastName, _oldLastName,
            ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNameChange, "Some comments");

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-details-updated-event");
        Assert.NotNull(item);

        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);
        Assert.Equal("Record updated from TRN request of unknown source", title.TrimmedText());
        item.AssertRow("request-data", "Source", v => Assert.Equal("Not provided", v.TrimmedText()));
    }

    private async Task CreateEvent(Guid createdByUserId, Guid personId, Guid applicationUserId,
        string firstName, string oldFirstName,
        string middleName, string oldMiddleName,
        string lastName, string oldLastName,
        ApiTrnRequestSupportTaskUpdatedEventChanges changes, string? comments)
    {
        await CreateEvent(createdByUserId, personId, applicationUserId,
            firstName, oldFirstName, middleName, oldMiddleName, lastName, oldLastName,
            null, null, null, null, null, null, null, null,
            changes, comments);
    }

    private async Task CreateEvent(Guid createdByUserId, Guid personId, Guid applicationUserId,
        string firstName, string oldFirstName,
        string middleName, string oldMiddleName,
        string lastName, string oldLastName,
        DateOnly? dob, DateOnly? oldDob,
        string? email, string? oldEmail,
        string? nino, string? oldNino,
        Gender? gender, Gender? oldGender,
        ApiTrnRequestSupportTaskUpdatedEventChanges changes, string? comments)
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
            SupportTaskTypeId = SupportTaskType.ApiTrnRequest.SupportTaskTypeId,
            OneLoginUserSubject = null,
            Status = SupportTaskStatus.Closed,
        };

        var oldSupportTask = new EventModels.SupportTask
        {
            PersonId = personId,
            SupportTaskReference = "TEST-ST-1",
            SupportTaskTypeId = SupportTaskType.ApiTrnRequest.SupportTaskTypeId,
            OneLoginUserSubject = null,
            Status = SupportTaskStatus.Open,
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

        var updatedEvent = new ApiTrnRequestSupportTaskUpdatedEvent
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
            Comments = comments
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });
    }
}
