using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogNpqTrnRequestProcessTests : TestBase
{
    private const string RecordCreatedReason = "Record created - no existing person identified during task resolution";
    private const string RecordMergedReason = "Records merged - identified as same person during task resolution";

    private readonly string _oldFirstName = "Alfred";
    private readonly string _oldMiddleName = "The";
    private readonly string _oldLastName = "Great";
    private readonly DateOnly _oldDob;
    private readonly string _oldEmail = "old@email-address.com";
    private readonly string _oldNino = "AB 12 34 56 D";
    private readonly Gender _oldGender = Gender.Male;

    private readonly string _firstName = "Megan";
    private readonly string _middleName = "Thee";
    private readonly string _lastName = "Stallion";
    private readonly DateOnly _dob;
    private readonly string _email = "new@email-address.com";
    private readonly string _nino = "XY 98 76 54 A";
    private readonly Gender _gender = Gender.Female;

    public ChangeLogNpqTrnRequestProcessTests(HostFixture hostFixture) : base(hostFixture)
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
    }

    public static TheoryData<PersonDetailsUpdatedEventChanges, bool, bool>
        Person_WithNpqTrnRequestApprovingProcess_ThatMergedRecords_RendersExpectedContentData =>
        new MatrixTheoryData<PersonDetailsUpdatedEventChanges, bool, bool>(
            [
                PersonDetailsUpdatedEventChanges.FirstName,
                PersonDetailsUpdatedEventChanges.MiddleName,
                PersonDetailsUpdatedEventChanges.LastName,
                PersonDetailsUpdatedEventChanges.DateOfBirth,
                PersonDetailsUpdatedEventChanges.EmailAddress,
                PersonDetailsUpdatedEventChanges.NationalInsuranceNumber,
                PersonDetailsUpdatedEventChanges.Gender,
                PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange
            ],
            [true, false],
            [true, false]);

    [Theory]
    [MemberData(nameof(Person_WithNpqTrnRequestApprovingProcess_ThatMergedRecords_RendersExpectedContentData))]
    public async Task Person_WithNpqTrnRequestApprovingProcess_ThatMergedRecords_RendersExpectedContent(
        PersonDetailsUpdatedEventChanges changes,
        bool previousValueIsDefault,
        bool newValueIsDefault)
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync("Apply for QTS");
        var person = await TestData.CreatePersonAsync();
        var comments = TestData.GenerateLoremIpsum();

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

        var personDetails = CreatePersonDetails(newFirstName, newMiddleName, newLastName, newDob, newEmail, newNino, newGender);
        var oldPersonDetails = CreatePersonDetails(_oldFirstName, _oldMiddleName, _oldLastName, _oldDob, oldEmail, oldNino, oldGender);

        var requestData = CreateRequestData(applicationUser.UserId, personDetails, person.PersonId);

        var process = await TestData.CreateProcessAsync(
            ProcessType.NpqTrnRequestApproving,
            createdByUser.UserId,
            changeReason: null,
            CreateSupportTaskUpdatedEvent(person.PersonId, comments),
            CreateTrnRequestUpdatedEvent(requestData),
            new PersonDetailsUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                PersonDetails = personDetails,
                OldPersonDetails = oldPersonDetails,
                Changes = changes
            });

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Record updated from Apply for QTS TRN request",
            createdByUser.Name,
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

        item.AssertSummaryListRowValue("change-reason", "Reason", v => Assert.Equal(RecordMergedReason, v.TrimmedText()));
        item.AssertSummaryListRowValue("change-reason", "Comments", v => Assert.Equal(comments, v.TrimmedText()));

        item.AssertSummaryListRowValue("request-data", "Source", v => Assert.Equal("Apply for QTS", v.TrimmedText()));
        item.AssertSummaryListRowValue("request-data", "Request ID", v => Assert.Equal("TEST-TRN-1", v.TrimmedText()));
        item.AssertSummaryListRowValue("request-data", "Created on", v => Assert.Equal(TimeProvider.UtcNow.ToString(WebConstants.DateAndTimeDisplayFormat), v.TrimmedText()));
        item.AssertSummaryListRowValue("request-data", "Name", v => Assert.Equal($"{newFirstName} {newMiddleName} {newLastName}", v.TrimmedText()));
        item.AssertSummaryListRowValue("request-data", "Date of birth", v => Assert.Equal(newDob.ToString(WebConstants.DateDisplayFormat), v.TrimmedText()));
        item.AssertSummaryListRowValue("request-data", "Email address", v => Assert.Equal(newEmail ?? WebConstants.EmptyFallbackContent, v.TrimmedText()));
        item.AssertSummaryListRowValue("request-data", "National Insurance number", v => Assert.Equal(newNino ?? WebConstants.EmptyFallbackContent, v.TrimmedText()));
        item.AssertSummaryListRowValue("request-data", "Gender", v => Assert.Equal(newGender?.GetDisplayName() ?? WebConstants.EmptyFallbackContent, v.TrimmedText()));
    }

    [Fact]
    public async Task Person_WithNpqTrnRequestApprovingProcess_ThatCreatedRecord_RendersExpectedContent()
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync("Apply for QTS");
        var person = await TestData.CreatePersonAsync();

        var personDetails = CreatePersonDetails(_firstName, _middleName, _lastName, _dob, _email, _nino, _gender);
        var requestData = CreateRequestData(applicationUser.UserId, personDetails, person.PersonId);

        var process = await TestData.CreateProcessAsync(
            ProcessType.NpqTrnRequestApproving,
            createdByUser.UserId,
            changeReason: null,
            CreateSupportTaskUpdatedEvent(person.PersonId, comments: null),
            CreateTrnRequestUpdatedEvent(requestData),
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = personDetails,
                TrnRequestMetadata = requestData
            });

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Record created from Apply for QTS TRN request",
            createdByUser.Name,
            process.CreatedOn);

        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(item);

        // Creating a record isn't a change to an existing one, so there's nothing to show as changed or previous.
        item.AssertSummaryListRowDoesNotExist("details", "Name");
        Assert.Null(item.GetElementByTestId("previous-details"));

        item.AssertSummaryListRowValue("change-reason", "Reason", v => Assert.Equal(RecordCreatedReason, v.TrimmedText()));
        item.AssertSummaryListRowDoesNotExist("change-reason", "Comments");

        item.AssertSummaryListRowValue("request-data", "Source", v => Assert.Equal("Apply for QTS", v.TrimmedText()));
        item.AssertSummaryListRowValue("request-data", "Name", v => Assert.Equal($"{_firstName} {_middleName} {_lastName}", v.TrimmedText()));
    }

    [Fact]
    public async Task Person_WithNpqTrnRequestApprovingProcess_ThatMergedRecordsWithoutChangingThem_RendersExpectedContent()
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync("Apply for QTS");
        var person = await TestData.CreatePersonAsync();

        var personDetails = CreatePersonDetails(_firstName, _middleName, _lastName, _dob, _email, _nino, _gender);
        var requestData = CreateRequestData(applicationUser.UserId, personDetails, person.PersonId);

        var process = await TestData.CreateProcessAsync(
            ProcessType.NpqTrnRequestApproving,
            createdByUser.UserId,
            changeReason: null,
            CreateSupportTaskUpdatedEvent(person.PersonId, comments: null),
            CreateTrnRequestUpdatedEvent(requestData));

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Record updated from Apply for QTS TRN request",
            createdByUser.Name,
            process.CreatedOn);

        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(item);

        item.AssertSummaryListRowDoesNotExist("details", "Name");
        Assert.Null(item.GetElementByTestId("previous-details"));

        item.AssertSummaryListRowValue("change-reason", "Reason", v => Assert.Equal(RecordMergedReason, v.TrimmedText()));
        item.AssertSummaryListRowValue("change-reason", "Comments", v => Assert.Equal(WebConstants.EmptyFallbackContent, v.TrimmedText()));
    }

    [Fact]
    public async Task Person_WithNpqTrnRequestApprovingProcess_WithUnknownApplicationSource_RendersExpectedContent()
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        var personDetails = CreatePersonDetails(_firstName, _middleName, _lastName, _dob, _email, _nino, _gender);
        var requestData = CreateRequestData(applicationUserId: Guid.NewGuid(), personDetails, person.PersonId);

        var process = await TestData.CreateProcessAsync(
            ProcessType.NpqTrnRequestApproving,
            createdByUser.UserId,
            changeReason: null,
            CreateSupportTaskUpdatedEvent(person.PersonId, comments: null),
            CreateTrnRequestUpdatedEvent(requestData),
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = personDetails,
                TrnRequestMetadata = requestData
            });

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertHasChangeHistoryEntry(
            process.ProcessId,
            "Record created from TRN request of unknown source",
            createdByUser.Name,
            process.CreatedOn);

        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(item);
        item.AssertSummaryListRowValue("request-data", "Source", v => Assert.Equal(WebConstants.EmptyFallbackContent, v.TrimmedText()));
    }

    [Fact]
    public async Task Person_WithNpqTrnRequestApprovingProcess_WithEmailSentEvent_RendersEmailSentMessage()
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync("Apply for QTS");
        var person = await TestData.CreatePersonAsync();

        var personDetails = CreatePersonDetails(_firstName, _middleName, _lastName, _dob, _email, _nino, _gender);
        var requestData = CreateRequestData(applicationUser.UserId, personDetails, person.PersonId);
        var email = new EventModels.Email
        {
            EmailId = Guid.NewGuid(),
            TemplateId = EmailTemplateIds.TrnGeneratedForNpq,
            EmailAddress = _email,
            Personalization = new Dictionary<string, string>(),
            Metadata = new Dictionary<string, object>(),
            SentOn = TimeProvider.UtcNow,
            EmailReplyToId = null
        };

        var process = await TestData.CreateProcessAsync(
            ProcessType.NpqTrnRequestApproving,
            createdByUser.UserId,
            changeReason: null,
            CreateSupportTaskUpdatedEvent(person.PersonId, comments: null),
            CreateTrnRequestUpdatedEvent(requestData),
            new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Details = personDetails,
                TrnRequestMetadata = requestData
            },
            new EmailSentEvent
            {
                EventId = Guid.NewGuid(),
                PersonId = person.PersonId,
                Email = email
            });

        // Act
        var response = await HttpClient.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history"));

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByDataAttribute("data-process-id", process.ProcessId.ToString());
        Assert.NotNull(item);
        Assert.Equal("We’ve sent them an email confirming their TRN.", item.GetElementByTestId("email-sent-message")?.TrimmedText());
    }

    private SupportTaskUpdatedEvent CreateSupportTaskUpdatedEvent(Guid personId, string? comments)
    {
        var oldSupportTask = new EventModels.SupportTask
        {
            PersonId = personId,
            SupportTaskReference = "TEST-ST-1",
            SupportTaskType = SupportTaskType.NpqTrnRequest,
            OneLoginUserSubject = null,
            Status = SupportTaskStatus.Open,
            Data = new NpqTrnRequestData(),
            SourceApplicationUserId = null,
            ResolveJourneySavedState = null,
            AssignedToUserId = null,
            Outcome = null,
            ZendeskTickets = []
        };

        return new SupportTaskUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            SupportTaskReference = oldSupportTask.SupportTaskReference,
            Changes = SupportTaskUpdatedEventChanges.Status | SupportTaskUpdatedEventChanges.Data,
            SupportTask = oldSupportTask with
            {
                Status = SupportTaskStatus.Closed,
                Outcome = SupportTaskOutcome.NpqTrnRequest_ResolvedWithNewPerson
            },
            OldSupportTask = oldSupportTask,
            Comments = comments,
            RejectionReason = null
        };
    }

    private static TrnRequestUpdatedEvent CreateTrnRequestUpdatedEvent(EventModels.TrnRequestMetadata requestData) =>
        new()
        {
            EventId = Guid.NewGuid(),
            SourceApplicationUserId = requestData.ApplicationUserId,
            RequestId = requestData.RequestId,
            Changes = TrnRequestUpdatedChanges.Status | TrnRequestUpdatedChanges.ResolvedPersonId,
            TrnRequest = requestData,
            OldTrnRequest = requestData with { Status = TrnRequestStatus.Pending, ResolvedPersonId = null },
            ReasonDetails = null
        };

    private EventModels.TrnRequestMetadata CreateRequestData(
        Guid applicationUserId,
        EventModels.PersonDetails personDetails,
        Guid resolvedPersonId) =>
        new()
        {
            ApplicationUserId = applicationUserId,
            RequestId = "TEST-TRN-1",
            CreatedOn = TimeProvider.UtcNow,
            IdentityVerified = null,
            EmailAddress = personDetails.EmailAddress,
            OneLoginUserSubject = null,
            FirstName = personDetails.FirstName,
            MiddleName = personDetails.MiddleName,
            LastName = personDetails.LastName,
            PreviousFirstName = "Jim",
            PreviousLastName = "Smith",
            Name = [personDetails.FirstName, personDetails.MiddleName, personDetails.LastName],
            DateOfBirth = personDetails.DateOfBirth!.Value,
            PotentialDuplicate = null,
            NationalInsuranceNumber = personDetails.NationalInsuranceNumber,
            Gender = personDetails.Gender,
            AddressLine1 = "1 Test Place",
            AddressLine2 = "Test Street",
            AddressLine3 = "Testborough",
            City = "Testington",
            Postcode = "TE57 1NG",
            Country = "Testland",
            TrnToken = null,
            ResolvedPersonId = resolvedPersonId,
            Matches = null,
            NpqApplicationId = null,
            NpqEvidenceFileId = null,
            NpqEvidenceFileName = null,
            NpqName = null,
            NpqTrainingProvider = null,
            NpqWorkingInEducationalSetting = null,
            Status = TrnRequestStatus.Completed
        };

    private static EventModels.PersonDetails CreatePersonDetails(
        string firstName,
        string middleName,
        string lastName,
        DateOnly dateOfBirth,
        string? emailAddress,
        string? nationalInsuranceNumber,
        Gender? gender) => new()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = gender
        };
}
