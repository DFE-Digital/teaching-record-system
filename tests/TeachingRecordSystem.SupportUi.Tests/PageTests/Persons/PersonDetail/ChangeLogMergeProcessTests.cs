using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogMergeProcessTests(HostFixture hostFixture) : TestBase(hostFixture), IAsyncLifetime
{
    private readonly string _oldFirstName = "Alfred";
    private readonly string _oldMiddleName = "The";
    private readonly string _oldLastName = "Great";
    private DateOnly _oldDob;
    private readonly string _oldEmail = "old@email-address.com";
    private readonly string _oldNino = "AB 12 34 56 D";
    private readonly Gender _oldGender = Gender.Male;

    private readonly string _firstName = "Megan";
    private readonly string _middleName = "Thee";
    private readonly string _lastName = "Stallion";
    private DateOnly _dob;
    private readonly string _email = "new@email-address.com";
    private readonly string _nino = "XY 98 76 54 A";
    private readonly Gender _gender = Gender.Female;

    private Core.DataStore.Postgres.Models.User? _createdByUser;
    private Person? _person;
    private Person? _secondaryPerson;

    async ValueTask IAsyncLifetime.InitializeAsync()
    {
        // Toggle between GMT and BST to ensure we're testing rendering dates in local time
        var nows = new[]
        {
            new DateTime(2024, 1, 1, 12, 13, 14, DateTimeKind.Utc),  // GMT
            new DateTime(2024, 7, 5, 19, 20, 21, DateTimeKind.Utc)   // BST
        };
        TimeProvider.SetUtcNow(new DateTimeOffset(nows.SingleRandom(), TimeSpan.Zero));

        _oldDob = TimeProvider.Today.AddYears(-30);
        _dob = TimeProvider.Today.AddYears(-20);

        _createdByUser = await TestData.CreateUserAsync();
        _person = await TestData.CreatePersonAsync();
        _secondaryPerson = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(_secondaryPerson);
            _secondaryPerson.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
    }

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Theory]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.MiddleName, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.LastName, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.DateOfBirth, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.EmailAddress, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.EmailAddress, true, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.EmailAddress, false, true)]
    [InlineData(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, false, true)]
    [InlineData(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, true, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.Gender, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.Gender, false, true)]
    [InlineData(PersonDetailsUpdatedEventChanges.Gender, true, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, true)]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false)]
    public async Task Person_WithPersonMergingProcess_AsRetainedPerson_RendersExpectedContent(
        PersonDetailsUpdatedEventChanges changes,
        bool previousValueIsDefault,
        bool newValueIsDefault)
    {
        // Arrange
        string? oldEmail = previousValueIsDefault ? null : _oldEmail;
        string? oldNino = previousValueIsDefault ? null : _oldNino;
        Gender? oldGender = previousValueIsDefault ? null : _oldGender;

        string? email = newValueIsDefault ? null : _email;
        string? nino = newValueIsDefault ? null : _nino;
        Gender? gender = newValueIsDefault ? null : _gender;

        var newFirstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? _firstName : _oldFirstName;
        var newMiddleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? _middleName : _oldMiddleName;
        var newLastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? _lastName : _oldLastName;
        var newDob = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? _dob : _oldDob;
        var newEmail = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? email : oldEmail;
        var newNino = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? nino : oldNino;
        var newGender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? gender : oldGender;

        var process = await CreateProcessAsync(
            newFirstName, _oldFirstName,
            newMiddleName, _oldMiddleName,
            newLastName, _oldLastName,
            newDob, _oldDob,
            newEmail, oldEmail,
            newNino, oldNino,
            newGender, oldGender,
            changes, comments: null, evidenceFile: null);

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{_person!.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            $"Record merged with TRN {_secondaryPerson!.Trn}",
            _createdByUser!.Name,
            process.CreatedOn);

        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(item);

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            item.AssertSummaryListRowValue("details", "Name", v => Assert.Equal($"{newFirstName} {newMiddleName} {newLastName}", v.TrimmedText()));
            item.AssertSummaryListRowValue("previous-details", "Name", v => Assert.Equal($"{_oldFirstName} {_oldMiddleName} {_oldLastName}", v.TrimmedText()));
        }
        else
        {
            item.AssertSummaryListRowDoesNotExist("details", "Name");
            item.AssertSummaryListRowDoesNotExist("previous-details", "Name");
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth))
        {
            item.AssertSummaryListRowValue("details", "Date of birth", v => Assert.Equal(newDob.ToString(WebConstants.DateDisplayFormat), v.TrimmedText()));
            item.AssertSummaryListRowValue("previous-details", "Date of birth", v => Assert.Equal(_oldDob.ToString(WebConstants.DateDisplayFormat), v.TrimmedText()));
        }
        else
        {
            item.AssertSummaryListRowDoesNotExist("details", "Date of birth");
            item.AssertSummaryListRowDoesNotExist("previous-details", "Date of birth");
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress))
        {
            item.AssertSummaryListRowValue("details", "Email address", v => Assert.Equal(newEmail ?? WebConstants.EmptyFallbackContent, v.TrimmedText()));
            item.AssertSummaryListRowValue("previous-details", "Email address", v => Assert.Equal(oldEmail ?? WebConstants.EmptyFallbackContent, v.TrimmedText()));
        }
        else
        {
            item.AssertSummaryListRowDoesNotExist("details", "Email address");
            item.AssertSummaryListRowDoesNotExist("previous-details", "Email address");
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber))
        {
            item.AssertSummaryListRowValue("details", "National Insurance number", v => Assert.Equal(newNino ?? WebConstants.EmptyFallbackContent, v.TrimmedText()));
            item.AssertSummaryListRowValue("previous-details", "National Insurance number", v => Assert.Equal(oldNino ?? WebConstants.EmptyFallbackContent, v.TrimmedText()));
        }
        else
        {
            item.AssertSummaryListRowDoesNotExist("details", "National Insurance number");
            item.AssertSummaryListRowDoesNotExist("previous-details", "National Insurance number");
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender))
        {
            item.AssertSummaryListRowValue("details", "Gender", v => Assert.Equal(newGender?.GetDisplayName() ?? WebConstants.EmptyFallbackContent, v.TrimmedText()));
            item.AssertSummaryListRowValue("previous-details", "Gender", v => Assert.Equal(oldGender?.GetDisplayName() ?? WebConstants.EmptyFallbackContent, v.TrimmedText()));
        }
        else
        {
            item.AssertSummaryListRowDoesNotExist("details", "Gender");
            item.AssertSummaryListRowDoesNotExist("previous-details", "Gender");
        }
    }

    [Fact]
    public async Task Person_WithPersonMergingProcess_AsRetainedPerson_RendersChangeReason()
    {
        // Arrange
        var comments = "Some comments";
        var evidenceFile = new EventModels.File { FileId = Guid.NewGuid(), Name = "evidence.jpg" };

        var process = await CreateProcessAsync(
            PersonDetailsUpdatedEventChanges.NameChange, comments, evidenceFile);

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{_person!.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(item);
        item.AssertSummaryListRowValue("change-reason", "Comments", v => Assert.Equal(comments, v.TrimmedText()));
        item.AssertSummaryListRowValue("change-reason", "Evidence", v => Assert.Equal($"{evidenceFile.Name} (opens in new tab)", v.TrimmedText()));
    }

    [Fact]
    public async Task Person_WithPersonMergingProcess_AsDeactivatedPerson_RendersExpectedContent()
    {
        // Arrange
        var comments = "Some comments";
        var evidenceFile = new EventModels.File { FileId = Guid.NewGuid(), Name = "evidence.jpg" };

        var process = await CreateProcessAsync(
            PersonDetailsUpdatedEventChanges.NameChange, comments, evidenceFile);

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{_secondaryPerson!.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            $"Record merged into TRN {_person!.Trn} and deactivated",
            _createdByUser!.Name,
            process.CreatedOn);

        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(item);

        // The attribute changes belong to the retained record, so they aren't shown here.
        Assert.Null(item.GetElementByTestId("details"));
        Assert.Null(item.GetElementByTestId("previous-details"));

        item.AssertSummaryListRowValue("change-reason", "Comments", v => Assert.Equal(comments, v.TrimmedText()));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Person_WithPersonMergingProcess_WhenCommentsAndEvidenceEmpty_DoesNotRenderReasonForChangeSection(bool asRetainedPerson)
    {
        // Arrange
        var process = await CreateProcessAsync(
            PersonDetailsUpdatedEventChanges.NameChange, comments: null, evidenceFile: null);

        var personId = asRetainedPerson ? _person!.PersonId : _secondaryPerson!.PersonId;

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(item);
        Assert.Null(item.GetElementByTestId("change-reason"));
    }

    [Fact]
    public async Task Person_WithPersonMergingProcess_ThatChangedNothing_RendersNoDetails()
    {
        // Arrange
        var process = await TestData.CreateProcessAsync(
            ProcessType.PersonMerging,
            _createdByUser!.UserId,
            CreateChangeReason(comments: null, evidenceFile: null),
            CreatePersonDeactivatedEvent());

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{_person!.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            $"Record merged with TRN {_secondaryPerson!.Trn}",
            _createdByUser!.Name,
            process.CreatedOn);

        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(item);
        item.AssertSummaryListRowDoesNotExist("details", "Name");
        item.AssertSummaryListRowDoesNotExist("previous-details", "Name");
    }

    private Task<Process> CreateProcessAsync(
        PersonDetailsUpdatedEventChanges changes, string? comments, EventModels.File? evidenceFile) =>
        CreateProcessAsync(
            _firstName, _oldFirstName, _middleName, _oldMiddleName, _lastName, _oldLastName,
            null, null, null, null, null, null, null, null,
            changes, comments, evidenceFile);

    private Task<Process> CreateProcessAsync(
        string firstName, string oldFirstName,
        string middleName, string oldMiddleName,
        string lastName, string oldLastName,
        DateOnly? dob, DateOnly? oldDob,
        string? email, string? oldEmail,
        string? nino, string? oldNino,
        Gender? gender, Gender? oldGender,
        PersonDetailsUpdatedEventChanges changes, string? comments, EventModels.File? evidenceFile) =>
        TestData.CreateProcessAsync(
            ProcessType.PersonMerging,
            _createdByUser!.UserId,
            CreateChangeReason(comments, evidenceFile),
            CreatePersonDeactivatedEvent(),
            new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = _person!.PersonId,
                PersonDetails = new EventModels.PersonDetails
                {
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    DateOfBirth = dob,
                    EmailAddress = email,
                    NationalInsuranceNumber = nino,
                    Gender = gender
                },
                OldPersonDetails = new EventModels.PersonDetails
                {
                    FirstName = oldFirstName,
                    MiddleName = oldMiddleName,
                    LastName = oldLastName,
                    DateOfBirth = oldDob,
                    EmailAddress = oldEmail,
                    NationalInsuranceNumber = oldNino,
                    Gender = oldGender
                },
                Changes = changes
            });

    private PersonDeactivatedEvent CreatePersonDeactivatedEvent() => new()
    {
        EventId = Guid.NewGuid(),
        PersonId = _secondaryPerson!.PersonId,
        MergedWithPersonId = _person!.PersonId,
        Changes = PersonDeactivatedEventChanges.MergedWithPersonId,
        DateOfDeath = null
    };

    private static ChangeReasonWithDetailsAndEvidence CreateChangeReason(string? comments, EventModels.File? evidenceFile) => new()
    {
        Reason = null,
        Details = comments,
        EvidenceFile = evidenceFile,
        AdditionalInformation = null
    };
}
