using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogMergeEventTests : TestBase, IAsyncLifetime
{
    private string _oldFirstName;
    private string _oldMiddleName;
    private string _oldLastName;
    private DateOnly? _oldDateOfBirth;
    private string? _oldEmailAddress;
    private string? _oldNationalInsuranceNumber;
    private Gender? _oldGender;

    private string _firstName;
    private string _middleName;
    private string _lastName;
    private DateOnly? _dateOfBirth;
    private string _emailAddress;
    private string? _nationalInsuranceNumber;
    private Gender? _gender;

    private Core.DataStore.Postgres.Models.User? _createdByUser;
    private TestData.CreatePersonResult? _person;
    private TestData.CreatePersonResult? _secondaryPerson;

    public ChangeLogMergeEventTests(HostFixture hostFixture) : base(hostFixture)
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
        _oldDateOfBirth = Clock.Today.AddYears(-30);
        _oldEmailAddress = "old@email-address.com";
        _oldNationalInsuranceNumber = "AB 12 34 56 D";
        _oldGender = Gender.Male;

        _firstName = "Megan";
        _middleName = "Thee";
        _lastName = "Stallion";
        _dateOfBirth = Clock.Today.AddYears(-20);
        _emailAddress = "new@email-address.com";
        _nationalInsuranceNumber = "XY 98 76 54 A";
        _gender = Gender.Female;
    }

    public async Task InitializeAsync()
    {
        _createdByUser = await TestData.CreateUserAsync();
        _person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());
        _secondaryPerson = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(_secondaryPerson.Person);
            _secondaryPerson.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
    }

    public Task DisposeAsync()
        => Task.CompletedTask;

    [Theory]
    [InlineData(PersonsMergedEventChanges.FirstName, false, false)]
    [InlineData(PersonsMergedEventChanges.MiddleName, false, false)]
    [InlineData(PersonsMergedEventChanges.LastName, false, false)]
    [InlineData(PersonsMergedEventChanges.DateOfBirth, false, false)]
    [InlineData(PersonsMergedEventChanges.EmailAddress, false, false)]
    [InlineData(PersonsMergedEventChanges.EmailAddress, true, false)]
    [InlineData(PersonsMergedEventChanges.EmailAddress, false, true)]
    [InlineData(PersonsMergedEventChanges.NationalInsuranceNumber, false, false)]
    [InlineData(PersonsMergedEventChanges.NationalInsuranceNumber, false, true)]
    [InlineData(PersonsMergedEventChanges.NationalInsuranceNumber, true, false)]
    [InlineData(PersonsMergedEventChanges.Gender, false, false)]
    [InlineData(PersonsMergedEventChanges.Gender, false, true)]
    [InlineData(PersonsMergedEventChanges.Gender, true, false)]
    [InlineData(PersonsMergedEventChanges.FirstName | PersonsMergedEventChanges.MiddleName | PersonsMergedEventChanges.LastName | PersonsMergedEventChanges.DateOfBirth | PersonsMergedEventChanges.EmailAddress | PersonsMergedEventChanges.NationalInsuranceNumber | PersonsMergedEventChanges.Gender, false, false)]
    [InlineData(PersonsMergedEventChanges.FirstName | PersonsMergedEventChanges.MiddleName | PersonsMergedEventChanges.LastName | PersonsMergedEventChanges.DateOfBirth | PersonsMergedEventChanges.EmailAddress | PersonsMergedEventChanges.NationalInsuranceNumber | PersonsMergedEventChanges.Gender, false, true)]
    [InlineData(PersonsMergedEventChanges.FirstName | PersonsMergedEventChanges.MiddleName | PersonsMergedEventChanges.LastName | PersonsMergedEventChanges.DateOfBirth | PersonsMergedEventChanges.EmailAddress | PersonsMergedEventChanges.NationalInsuranceNumber | PersonsMergedEventChanges.Gender, true, false)]
    public async Task Person_WithPersonsMergedEvent_AsPrimaryPerson_RendersExpectedContent(PersonsMergedEventChanges changes, bool previousValueIsDefault, bool newValueIsDefault)
    {
        // Arrange
        var updatedFirstName = changes.HasFlag(PersonsMergedEventChanges.FirstName) ? _firstName : _oldFirstName;
        var updatedMiddleName = changes.HasFlag(PersonsMergedEventChanges.MiddleName) ? _middleName : _oldMiddleName;
        var updatedLastName = changes.HasFlag(PersonsMergedEventChanges.LastName) ? _lastName : _oldLastName;

        var comments = "Some comments";
        var evidenceFile = new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        };

        var details = new EventModels.PersonAttributes
        {
            FirstName = updatedFirstName,
            MiddleName = updatedMiddleName,
            LastName = updatedLastName,
            DateOfBirth = changes.HasFlag(PersonsMergedEventChanges.DateOfBirth) ? _dateOfBirth : _oldDateOfBirth,
            EmailAddress = changes.HasFlag(PersonsMergedEventChanges.EmailAddress) && !newValueIsDefault ? _emailAddress : null,
            NationalInsuranceNumber = changes.HasFlag(PersonsMergedEventChanges.NationalInsuranceNumber) && !newValueIsDefault ? _nationalInsuranceNumber : null,
            Gender = changes.HasFlag(PersonsMergedEventChanges.Gender) && !newValueIsDefault ? _gender : null,
        };

        var oldDetails = new EventModels.PersonAttributes
        {
            FirstName = _oldFirstName,
            MiddleName = _oldMiddleName,
            LastName = _oldLastName,
            DateOfBirth = _oldDateOfBirth,
            EmailAddress = changes.HasFlag(PersonsMergedEventChanges.EmailAddress) && !previousValueIsDefault ? _oldEmailAddress : null,
            NationalInsuranceNumber = changes.HasFlag(PersonsMergedEventChanges.NationalInsuranceNumber) && !previousValueIsDefault ? _oldNationalInsuranceNumber : null,
            Gender = changes.HasFlag(PersonsMergedEventChanges.Gender) && !previousValueIsDefault ? _oldGender : null,
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
            doc.AssertRow("details", "Name", v => Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", v.TrimmedText()));
            doc.AssertRow("previous-details", "Name", v => Assert.Equal($"{_oldFirstName} {_oldMiddleName} {_oldLastName}", v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Name");
            doc.AssertRowDoesNotExist("previous-details", "Name");
        }

        if (changes.HasFlag(PersonsMergedEventChanges.DateOfBirth))
        {
            doc.AssertRow("details", "Date of birth", v => Assert.Equal(_dateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
            doc.AssertRow("previous-details", "Date of birth", v => Assert.Equal(_oldDateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Date of birth");
            doc.AssertRowDoesNotExist("previous-details", "Date of birth");
        }

        if (changes.HasFlag(PersonsMergedEventChanges.EmailAddress))
        {
            doc.AssertRow("details", "Email address", v => Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : _emailAddress, v.TrimmedText()));
            doc.AssertRow("previous-details", "Email address", v => Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : _oldEmailAddress, v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Email address");
            doc.AssertRowDoesNotExist("previous-details", "Email address");
        }

        if (changes.HasFlag(PersonsMergedEventChanges.NationalInsuranceNumber))
        {
            doc.AssertRow("details", "National Insurance number", v => Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : _nationalInsuranceNumber, v.TrimmedText()));
            doc.AssertRow("previous-details", "National Insurance number", v => Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : _oldNationalInsuranceNumber, v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "National Insurance number");
            doc.AssertRowDoesNotExist("previous-details", "National Insurance number");
        }

        if (changes.HasFlag(PersonsMergedEventChanges.Gender))
        {
            doc.AssertRow("details", "Gender", v => Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : _gender.GetDisplayName(), v.TrimmedText()));
            doc.AssertRow("previous-details", "Gender", v => Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : _oldGender.GetDisplayName(), v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Gender");
            doc.AssertRowDoesNotExist("previous-details", "Gender");
        }

        doc.AssertRow("change-reason", "Comments", v => Assert.Equal(comments, v.TrimmedText()));
        doc.AssertRow("change-reason", "Evidence", v => Assert.Equal($"{evidenceFile!.Name} (opens in new tab)", v.TrimmedText()));
    }

    [Fact]
    public async Task Person_WithPersonsMergedEvent_AsSecondaryPerson_RendersExpectedContent()
    {
        // Arrange
        var changes = PersonsMergedEventChanges.NameChange;

        var comments = "Some comments";
        var evidenceFile = new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        };

        var details = new EventModels.PersonAttributes
        {
            FirstName = _firstName,
            MiddleName = _middleName,
            LastName = _lastName,
            DateOfBirth = null,
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null,
        };

        var oldDetails = new EventModels.PersonAttributes
        {
            FirstName = _oldFirstName,
            MiddleName = _oldMiddleName,
            LastName = _oldLastName,
            DateOfBirth = null,
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null
        };

        var updatedEvent = new PersonsMergedEvent
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
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });

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

    [Fact]
    public async Task Person_WithPersonsMergedEvent_AsPrimaryPerson_WhenCommentsAndEvidenceEmpty_DoesNotRenderReasonForChangeSection()
    {
        // Arrange
        var details = new EventModels.PersonAttributes
        {
            FirstName = _firstName,
            MiddleName = _middleName,
            LastName = _lastName,
            DateOfBirth = null,
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null,
        };

        var oldDetails = new EventModels.PersonAttributes
        {
            FirstName = _oldFirstName,
            MiddleName = _oldMiddleName,
            LastName = _oldLastName,
            DateOfBirth = null,
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null
        };

        var updatedEvent = new PersonsMergedEvent
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
            Changes = PersonsMergedEventChanges.NameChange,
            Comments = null,
            EvidenceFile = null
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });

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

    [Fact]
    public async Task Person_WithPersonsMergedEvent_AsSecondaryPerson_WhenCommentsAndEvidenceEmpty_DoesNotRenderReasonForChangeSection()
    {
        // Arrange
        var details = new EventModels.PersonAttributes
        {
            FirstName = _firstName,
            MiddleName = _middleName,
            LastName = _lastName,
            DateOfBirth = null,
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null,
        };

        var oldDetails = new EventModels.PersonAttributes
        {
            FirstName = _oldFirstName,
            MiddleName = _oldMiddleName,
            LastName = _oldLastName,
            DateOfBirth = null,
            EmailAddress = null,
            NationalInsuranceNumber = null,
            Gender = null
        };

        var updatedEvent = new PersonsMergedEvent
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
            Changes = PersonsMergedEventChanges.NameChange,
            Comments = null,
            EvidenceFile = null
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });

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
}
