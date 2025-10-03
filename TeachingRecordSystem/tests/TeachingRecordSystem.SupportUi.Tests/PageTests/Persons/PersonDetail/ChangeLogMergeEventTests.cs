using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogMergeEventTests(HostFixture hostFixture) : TestBase(hostFixture)
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

    private Core.DataStore.Postgres.Models.User? _createdByUser;
    private TestData.CreatePersonResult? _person;
    private TestData.CreatePersonResult? _secondaryPerson;

    [Before(Test)]
    public async Task InitializeAsync()
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

        _createdByUser = await TestData.CreateUserAsync();
        _person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs));
        _secondaryPerson = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs));

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(_secondaryPerson.Person);
            _secondaryPerson.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
    }

    [Test]
    [Arguments(PersonsMergedEventChanges.FirstName, false, false)]
    [Arguments(PersonsMergedEventChanges.MiddleName, false, false)]
    [Arguments(PersonsMergedEventChanges.LastName, false, false)]
    [Arguments(PersonsMergedEventChanges.DateOfBirth, false, false)]
    [Arguments(PersonsMergedEventChanges.EmailAddress, false, false)]
    [Arguments(PersonsMergedEventChanges.EmailAddress, true, false)]
    [Arguments(PersonsMergedEventChanges.EmailAddress, false, true)]
    [Arguments(PersonsMergedEventChanges.NationalInsuranceNumber, false, false)]
    [Arguments(PersonsMergedEventChanges.NationalInsuranceNumber, false, true)]
    [Arguments(PersonsMergedEventChanges.NationalInsuranceNumber, true, false)]
    [Arguments(PersonsMergedEventChanges.Gender, false, false)]
    [Arguments(PersonsMergedEventChanges.Gender, false, true)]
    [Arguments(PersonsMergedEventChanges.Gender, true, false)]
    [Arguments(PersonsMergedEventChanges.FirstName | PersonsMergedEventChanges.MiddleName | PersonsMergedEventChanges.LastName | PersonsMergedEventChanges.DateOfBirth | PersonsMergedEventChanges.EmailAddress | PersonsMergedEventChanges.NationalInsuranceNumber | PersonsMergedEventChanges.Gender, false, false)]
    [Arguments(PersonsMergedEventChanges.FirstName | PersonsMergedEventChanges.MiddleName | PersonsMergedEventChanges.LastName | PersonsMergedEventChanges.DateOfBirth | PersonsMergedEventChanges.EmailAddress | PersonsMergedEventChanges.NationalInsuranceNumber | PersonsMergedEventChanges.Gender, false, true)]
    [Arguments(PersonsMergedEventChanges.FirstName | PersonsMergedEventChanges.MiddleName | PersonsMergedEventChanges.LastName | PersonsMergedEventChanges.DateOfBirth | PersonsMergedEventChanges.EmailAddress | PersonsMergedEventChanges.NationalInsuranceNumber | PersonsMergedEventChanges.Gender, true, false)]
    public async Task Person_WithPersonsMergedEvent_AsPrimaryPerson_RendersExpectedContent(PersonsMergedEventChanges changes, bool previousValueIsDefault, bool newValueIsDefault)
    {
        // Arrange
        string? oldEmail = previousValueIsDefault ? null : _oldEmail;
        string? oldNino = previousValueIsDefault ? null : _oldNino;
        Gender? oldGender = previousValueIsDefault ? null : _oldGender;

        string? email = newValueIsDefault ? null : _email;
        string? nino = newValueIsDefault ? null : _nino;
        Gender? gender = newValueIsDefault ? null : _gender;

        var newFirstName = changes.HasFlag(PersonsMergedEventChanges.FirstName) ? _firstName : _oldFirstName;
        var newMiddleName = changes.HasFlag(PersonsMergedEventChanges.MiddleName) ? _middleName : _oldMiddleName;
        var newLastName = changes.HasFlag(PersonsMergedEventChanges.LastName) ? _lastName : _oldLastName;
        var newDob = changes.HasFlag(PersonsMergedEventChanges.DateOfBirth) ? _dob : _oldDob;
        var newEmail = changes.HasFlag(PersonsMergedEventChanges.EmailAddress) ? email : oldEmail;
        var newNino = changes.HasFlag(PersonsMergedEventChanges.NationalInsuranceNumber) ? nino : oldNino;
        var newGender = changes.HasFlag(PersonsMergedEventChanges.Gender) ? gender : oldGender;

        var comments = "Some comments";
        var evidenceFile = new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        };

        await CreateEvent(
            newFirstName, _oldFirstName,
            newMiddleName, _oldMiddleName,
            newLastName, _oldLastName,
            newDob, _oldDob,
            newEmail, oldEmail,
            newNino, oldNino,
            newGender, oldGender,
            changes, comments, evidenceFile);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{_person!.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-persons-merged-event");
        Assert.NotNull(item);
        Assert.Equal($"By {_createdByUser!.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());

        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);
        Assert.Equal($"Record merged with TRN {_secondaryPerson!.Trn}", title.TrimmedText());

        if (changes.HasAnyFlag(PersonsMergedEventChanges.NameChange))
        {
            doc.AssertRow("details", "Name", v => Assert.Equal($"{newFirstName} {newMiddleName} {newLastName}", v.TrimmedText()));
            doc.AssertRow("previous-details", "Name", v => Assert.Equal($"{_oldFirstName} {_oldMiddleName} {_oldLastName}", v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Name");
            doc.AssertRowDoesNotExist("previous-details", "Name");
        }

        if (changes.HasFlag(PersonsMergedEventChanges.DateOfBirth))
        {
            doc.AssertRow("details", "Date of birth", v => Assert.Equal(newDob.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
            doc.AssertRow("previous-details", "Date of birth", v => Assert.Equal(_oldDob.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Date of birth");
            doc.AssertRowDoesNotExist("previous-details", "Date of birth");
        }

        if (changes.HasFlag(PersonsMergedEventChanges.EmailAddress))
        {
            item.AssertRow("details", "Email address", v => Assert.Equal(newEmail ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
            item.AssertRow("previous-details", "Email address", v => Assert.Equal(oldEmail ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "Email address");
            item.AssertRowDoesNotExist("previous-details", "Email address");
        }

        if (changes.HasFlag(PersonsMergedEventChanges.NationalInsuranceNumber))
        {
            item.AssertRow("details", "National Insurance number", v => Assert.Equal(newNino ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
            item.AssertRow("previous-details", "National Insurance number", v => Assert.Equal(oldNino ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "National Insurance number");
            item.AssertRowDoesNotExist("previous-details", "National Insurance number");
        }

        if (changes.HasFlag(PersonsMergedEventChanges.Gender))
        {
            item.AssertRow("details", "Gender", v => Assert.Equal(newGender?.GetDisplayName() ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
            item.AssertRow("previous-details", "Gender", v => Assert.Equal(oldGender?.GetDisplayName() ?? UiDefaults.EmptyDisplayContent, v.TrimmedText()));
        }
        else
        {
            item.AssertRowDoesNotExist("details", "Gender");
            item.AssertRowDoesNotExist("previous-details", "Gender");
        }

        doc.AssertRow("change-reason", "Comments", v => Assert.Equal(comments, v.TrimmedText()));
        doc.AssertRow("change-reason", "Evidence", v => Assert.Equal($"{evidenceFile!.Name} (opens in new tab)", v.TrimmedText()));
    }

    [Test]
    public async Task Person_WithPersonsMergedEvent_AsSecondaryPerson_RendersExpectedContent()
    {
        // Arrange
        var comments = "Some comments";
        var evidenceFile = new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        };

        await CreateEvent(
            _firstName, _oldFirstName,
            _middleName, _oldMiddleName,
            _lastName, _oldLastName,
            PersonsMergedEventChanges.NameChange, comments, evidenceFile);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{_secondaryPerson!.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-persons-merged-event");
        Assert.NotNull(item);

        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);
        Assert.Equal($"Record merged into TRN {_person!.Trn} and deactivated", title.TrimmedText());

        Assert.Equal($"By {_createdByUser!.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());

        doc.AssertRow("change-reason", "Comments", v => Assert.Equal(comments, v.TrimmedText()));
        doc.AssertRow("change-reason", "Evidence", v => Assert.Equal($"{evidenceFile!.Name} (opens in new tab)", v.TrimmedText()));
    }

    [Test]
    public async Task Person_WithPersonsMergedEvent_AsPrimaryPerson_WhenCommentsAndEvidenceEmpty_DoesNotRenderReasonForChangeSection()
    {
        // Arrange
        await CreateEvent(
            _firstName, _oldFirstName,
            _middleName, _oldMiddleName,
            _lastName, _oldLastName,
            PersonsMergedEventChanges.NameChange, comments: null, evidenceFile: null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{_person!.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-persons-merged-event");
        Assert.NotNull(item);

        var changeReasonSection = item.GetElementByTestId("change-reason");
        Assert.Null(changeReasonSection);
    }

    [Test]
    public async Task Person_WithPersonsMergedEvent_AsSecondaryPerson_WhenCommentsAndEvidenceEmpty_DoesNotRenderReasonForChangeSection()
    {
        // Arrange
        await CreateEvent(
            _firstName, _oldFirstName,
            _middleName, _oldMiddleName,
            _lastName, _oldLastName,
            PersonsMergedEventChanges.NameChange, comments: null, evidenceFile: null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{_secondaryPerson!.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-persons-merged-event");
        Assert.NotNull(item);

        var changeReasonSection = item.GetElementByTestId("change-reason");
        Assert.Null(changeReasonSection);
    }

    private async Task CreateEvent(
        string firstName, string oldFirstName,
        string middleName, string oldMiddleName,
        string lastName, string oldLastName,
        PersonsMergedEventChanges changes, string? comments, EventModels.File? evidenceFile)
    {
        await CreateEvent(
            firstName, oldFirstName, middleName, oldMiddleName, lastName, oldLastName,
            null, null, null, null, null, null, null, null,
            changes, comments, evidenceFile);
    }

    private async Task CreateEvent(
        string firstName, string oldFirstName,
        string middleName, string oldMiddleName,
        string lastName, string oldLastName,
        DateOnly? dob, DateOnly? oldDob,
        string? email, string? oldEmail,
        string? nino, string? oldNino,
        Gender? gender, Gender? oldGender,
        PersonsMergedEventChanges changes, string? comments, EventModels.File? evidenceFile)
    {
        var details = new EventModels.PersonAttributes
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dob,
            EmailAddress = email,
            NationalInsuranceNumber = nino,
            Gender = gender
        };

        var oldDetails = new EventModels.PersonAttributes
        {
            FirstName = oldFirstName,
            MiddleName = oldMiddleName,
            LastName = oldLastName,
            DateOfBirth = oldDob,
            EmailAddress = oldEmail,
            NationalInsuranceNumber = oldNino,
            Gender = oldGender
        };

        var mergedEvent = new PersonsMergedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = _createdByUser!.UserId,
            PersonId = _person!.PersonId,
            PersonTrn = _person!.Trn!,
            SecondaryPersonId = _secondaryPerson!.PersonId,
            SecondaryPersonTrn = _secondaryPerson!.Trn!,
            SecondaryPersonStatus = PersonStatus.Deactivated,
            PersonAttributes = details,
            OldPersonAttributes = oldDetails,
            Changes = changes,
            Comments = comments,
            EvidenceFile = evidenceFile
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(mergedEvent);
            await dbContext.SaveChangesAsync();
        });
    }
}
