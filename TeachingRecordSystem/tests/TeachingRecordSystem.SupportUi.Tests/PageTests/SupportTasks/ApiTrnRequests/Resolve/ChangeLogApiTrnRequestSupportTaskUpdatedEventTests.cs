using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ApiTrnRequests.Resolve;

public class ChangeLogApiTrnRequestSupportTaskUpdatedEventTests : TestBase
{
    public ChangeLogApiTrnRequestSupportTaskUpdatedEventTests(HostFixture hostFixture) : base(hostFixture)
    {
        // Toggle between GMT and BST to ensure we're testing rendering dates in local time
        var nows = new[]
        {
            new DateTime(2024, 1, 1, 12, 13, 14, DateTimeKind.Utc),  // GMT
            new DateTime(2024, 7, 5, 19, 20, 21, DateTimeKind.Utc)   // BST
        };
        Clock.UtcNow = nows.RandomOne();
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
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonFirstName | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonMiddleName | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonLastName | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonDateOfBirth | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber, false, false)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonFirstName | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonMiddleName | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonLastName | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonDateOfBirth | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber, false, true)]
    [InlineData(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonFirstName | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonMiddleName | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonLastName | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonDateOfBirth | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress | ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber, true, false)]
    public async Task Person_WithPersonDetailsUpdatedEvent_RendersExpectedContent(ApiTrnRequestSupportTaskUpdatedEventChanges changes, bool previousValueIsDefault, bool newValueIsDefault)
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var applicationUser = await TestData.CreateApplicationUserAsync("Apply for QTS", "AfQTS");
        var person = await TestData.CreatePersonAsync();

        string oldFirstName = "Alfred";
        string oldMiddleName = "The";
        string oldLastName = "Great";
        DateOnly oldDateOfBirth = Clock.Today.AddYears(-30);
        string oldEmailAddress = "old@email-address.com";
        string oldNationalInsuranceNumber = "AB 12 34 56 D";
        Gender oldGender = Gender.NotAvailable;

        string firstName = "Megan";
        string middleName = "Thee";
        string lastName = "Stallion";
        DateOnly dateOfBirth = Clock.Today.AddYears(-20);
        string emailAddress = "new@email-address.com";
        string nationalInsuranceNumber = "XY 98 76 54 A";
        Gender gender = Gender.Female;

        var updatedFirstName = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonFirstName) ? firstName : oldFirstName;
        var updatedMiddleName = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonMiddleName) ? middleName : oldMiddleName;
        var updatedLastName = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonLastName) ? lastName : oldLastName;

        var attributes = new EventModels.PersonAttributes()
        {
            FirstName = updatedFirstName,
            MiddleName = updatedMiddleName,
            LastName = updatedLastName,
            DateOfBirth = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonDateOfBirth) ? dateOfBirth : oldDateOfBirth,
            EmailAddress = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress) && !newValueIsDefault ? emailAddress : null,
            NationalInsuranceNumber = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber) && !newValueIsDefault ? nationalInsuranceNumber : null,
            Gender = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonGender) && !newValueIsDefault ? gender : oldGender
        };

        var oldAttributes = new EventModels.PersonAttributes
        {
            FirstName = oldFirstName,
            MiddleName = oldMiddleName,
            LastName = oldLastName,
            DateOfBirth = oldDateOfBirth,
            EmailAddress = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress) && !previousValueIsDefault ? oldEmailAddress : null,
            NationalInsuranceNumber = changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber) && !previousValueIsDefault ? oldNationalInsuranceNumber : null,
            Gender = oldGender
        };

        var supportTask = new EventModels.SupportTask
        {
            PersonId = person.PersonId,
            SupportTaskReference = "TEST-ST-1",
            SupportTaskType = SupportTaskType.ApiTrnRequest,
            OneLoginUserSubject = null,
            Status = SupportTaskStatus.Closed,
        };

        var oldSupportTask = new EventModels.SupportTask
        {
            PersonId = person.PersonId,
            SupportTaskReference = "TEST-ST-1",
            SupportTaskType = SupportTaskType.ApiTrnRequest,
            OneLoginUserSubject = null,
            Status = SupportTaskStatus.Open,
        };

        var requestData = new EventModels.TrnRequestMetadata
        {
            ApplicationUserId = applicationUser.UserId,
            RequestId = "TEST-TRN-1",
            CreatedOn = Clock.UtcNow,
            IdentityVerified = null,
            EmailAddress = emailAddress,
            OneLoginUserSubject = null,
            FirstName = updatedFirstName,
            MiddleName = updatedMiddleName,
            LastName = updatedLastName,
            PreviousFirstName = "Jim",
            PreviousLastName = "Smith",
            Name = [updatedFirstName, updatedMiddleName, updatedLastName],
            DateOfBirth = dateOfBirth,
            PotentialDuplicate = null,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = null,
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
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            PersonAttributes = attributes,
            OldPersonAttributes = oldAttributes,
            SupportTask = supportTask,
            OldSupportTask = oldSupportTask,
            RequestData = requestData,
            Changes = changes,
            Comments = "Some comments"
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-details-updated-event");
        Assert.NotNull(item);
        var title = item.QuerySelector(".moj-timeline__title");
        Assert.NotNull(title);
        Assert.Equal("TRN request from Apply for QTS - records merged", title.TrimmedText());
        Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());

        if (changes.HasAnyFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNameChange))
        {
            doc.AssertRow("details", "Name", v => Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", v.TrimmedText()));
            doc.AssertRow("previous-details", "Name", v => Assert.Equal($"{oldFirstName} {oldMiddleName} {oldLastName}", v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Name");
            doc.AssertRowDoesNotExist("previous-details", "Name");
        }

        if (changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonDateOfBirth))
        {
            doc.AssertRow("details", "Date of birth", v => Assert.Equal(dateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
            doc.AssertRow("previous-details", "Date of birth", v => Assert.Equal(oldDateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Date of birth");
            doc.AssertRowDoesNotExist("previous-details", "Date of birth");
        }

        if (changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonEmailAddress))
        {
            doc.AssertRow("details", "Email address", v => Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : emailAddress, v.TrimmedText()));
            doc.AssertRow("previous-details", "Email address", v => Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldEmailAddress, v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Email address");
            doc.AssertRowDoesNotExist("previous-details", "Email address");
        }

        if (changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.PersonNationalInsuranceNumber))
        {
            doc.AssertRow("details", "National Insurance number", v => Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : nationalInsuranceNumber, v.TrimmedText()));
            doc.AssertRow("previous-details", "National Insurance number", v => Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldNationalInsuranceNumber, v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "National Insurance number");
            doc.AssertRowDoesNotExist("previous-details", "National Insurance number");
        }

        doc.AssertRow("change-reason", "Reason", v => Assert.Equal("Identified as same person during task resolution", v.TrimmedText()));
        doc.AssertRow("change-reason", "Comments", v => Assert.Equal("Some comments", v.TrimmedText()));

        doc.AssertRow("request-data", "Source", v => Assert.Equal("AfQTS", v.TrimmedText()));
        doc.AssertRow("request-data", "Request ID", v => Assert.Equal("TEST-TRN-1", v.TrimmedText()));
        doc.AssertRow("request-data", "Created on", v => Assert.Equal(Clock.UtcNow.ToString(UiDefaults.DateTimeDisplayFormat), v.TrimmedText()));
        doc.AssertRow("request-data", "Name", v => Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", v.TrimmedText()));
        doc.AssertRow("request-data", "Previous name", v => Assert.Equal("Jim Smith", v.TrimmedText()));
        doc.AssertRow("request-data", "Date of birth", v => Assert.Equal(dateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        doc.AssertRow("request-data", "Email address", v => Assert.Equal(emailAddress, v.TrimmedText()));
        doc.AssertRow("request-data", "National Insurance number", v => Assert.Equal(nationalInsuranceNumber, v.TrimmedText()));
        doc.AssertRow("request-data", "Address", v => Assert.Equal("<p class=\"govuk-body\">1 Test Place<br>Test Street<br>Testborough<br>Testington<br>TE57 1NG<br>Testland</p>", v.InnerHtml.Trim()));
    }
}
