using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogMergeEventTests : TestBase
{
    public ChangeLogMergeEventTests(HostFixture hostFixture) : base(hostFixture)
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
    public async Task Person_WithPersonsMergedEvent_WhenPrimaryPerson_RendersExpectedContent(PersonsMergedEventChanges changes, bool previousValueIsDefault, bool newValueIsDefault)
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());
        var secondaryPerson = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(secondaryPerson.Person);
            secondaryPerson.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        string oldFirstName = "Alfred";
        string oldMiddleName = "The";
        string oldLastName = "Great";
        DateOnly? oldDateOfBirth = Clock.Today.AddYears(-30);
        string? oldEmailAddress = "old@email-address.com";
        string? oldNationalInsuranceNumber = "AB 12 34 56 D";
        Gender? oldGender = Gender.Male;

        string firstName = "Megan";
        string middleName = "Thee";
        string lastName = "Stallion";
        DateOnly? dateOfBirth = Clock.Today.AddYears(-20);
        string emailAddress = "new@email-address.com";
        string? nationalInsuranceNumber = "XY 98 76 54 A";
        Gender? gender = Gender.Female;

        var updatedFirstName = changes.HasFlag(PersonsMergedEventChanges.FirstName) ? firstName : oldFirstName;
        var updatedMiddleName = changes.HasFlag(PersonsMergedEventChanges.MiddleName) ? middleName : oldMiddleName;
        var updatedLastName = changes.HasFlag(PersonsMergedEventChanges.LastName) ? lastName : oldLastName;

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
            DateOfBirth = changes.HasFlag(PersonsMergedEventChanges.DateOfBirth) ? dateOfBirth : oldDateOfBirth,
            EmailAddress = changes.HasFlag(PersonsMergedEventChanges.EmailAddress) && !newValueIsDefault ? emailAddress : null,
            NationalInsuranceNumber = changes.HasFlag(PersonsMergedEventChanges.NationalInsuranceNumber) && !newValueIsDefault ? nationalInsuranceNumber : null,
            Gender = changes.HasFlag(PersonsMergedEventChanges.Gender) && !newValueIsDefault ? gender : null,
        };

        var oldDetails = new EventModels.PersonAttributes
        {
            FirstName = oldFirstName,
            MiddleName = oldMiddleName,
            LastName = oldLastName,
            DateOfBirth = oldDateOfBirth,
            EmailAddress = changes.HasFlag(PersonsMergedEventChanges.EmailAddress) && !previousValueIsDefault ? oldEmailAddress : null,
            NationalInsuranceNumber = changes.HasFlag(PersonsMergedEventChanges.NationalInsuranceNumber) && !previousValueIsDefault ? oldNationalInsuranceNumber : null,
            Gender = changes.HasFlag(PersonsMergedEventChanges.Gender) && !previousValueIsDefault ? oldGender : null,
        };

        var mergedEvent = new PersonsMergedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            PersonTrn = person.Trn!,
            SecondaryPersonId = secondaryPerson.PersonId,
            SecondaryPersonTrn = secondaryPerson.Trn!,
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-persons-merged-event");
        Assert.NotNull(item);
        Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());

        doc.AssertRow("details", "TRN of other record", v => Assert.Equal(secondaryPerson.Trn, v.TrimmedText()));

        if (changes.HasAnyFlag(PersonsMergedEventChanges.NameChange))
        {
            doc.AssertRow("details", "Name", v => Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", v.TrimmedText()));
            doc.AssertRow("previous-details", "Name", v => Assert.Equal($"{oldFirstName} {oldMiddleName} {oldLastName}", v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Name");
            doc.AssertRowDoesNotExist("previous-details", "Name");
        }

        if (changes.HasFlag(PersonsMergedEventChanges.DateOfBirth))
        {
            doc.AssertRow("details", "Date of birth", v => Assert.Equal(dateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
            doc.AssertRow("previous-details", "Date of birth", v => Assert.Equal(oldDateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Date of birth");
            doc.AssertRowDoesNotExist("previous-details", "Date of birth");
        }

        if (changes.HasFlag(PersonsMergedEventChanges.EmailAddress))
        {
            doc.AssertRow("details", "Email address", v => Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : emailAddress, v.TrimmedText()));
            doc.AssertRow("previous-details", "Email address", v => Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldEmailAddress, v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Email address");
            doc.AssertRowDoesNotExist("previous-details", "Email address");
        }

        if (changes.HasFlag(PersonsMergedEventChanges.NationalInsuranceNumber))
        {
            doc.AssertRow("details", "National Insurance number", v => Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : nationalInsuranceNumber, v.TrimmedText()));
            doc.AssertRow("previous-details", "National Insurance number", v => Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldNationalInsuranceNumber, v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "National Insurance number");
            doc.AssertRowDoesNotExist("previous-details", "National Insurance number");
        }

        if (changes.HasFlag(PersonsMergedEventChanges.Gender))
        {
            doc.AssertRow("details", "Gender", v => Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : gender.GetDisplayName(), v.TrimmedText()));
            doc.AssertRow("previous-details", "Gender", v => Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldGender.GetDisplayName(), v.TrimmedText()));
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
    public async Task Person_WithPersonsMergedEvent_WhenSecondaryPerson_RendersExpectedContent()
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());
        var secondaryPerson = await TestData.CreatePersonAsync(p => p
            .WithPersonDataSource(TestDataPersonDataSource.Trs)
            .WithTrn());

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(secondaryPerson.Person);
            secondaryPerson.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        string oldFirstName = "Alfred";
        string oldMiddleName = "The";
        string oldLastName = "Great";
        DateOnly? oldDateOfBirth = Clock.Today.AddYears(-30);
        string? oldEmailAddress = "old@email-address.com";
        string? oldNationalInsuranceNumber = "AB 12 34 56 D";
        Gender? oldGender = Gender.Male;

        string firstName = "Megan";
        string middleName = "Thee";
        string lastName = "Stallion";
        DateOnly? dateOfBirth = Clock.Today.AddYears(-20);
        string emailAddress = "new@email-address.com";
        string? nationalInsuranceNumber = "XY 98 76 54 A";
        Gender? gender = Gender.Female;

        var changes = PersonsMergedEventChanges.FirstName | PersonsMergedEventChanges.MiddleName | PersonsMergedEventChanges.LastName | PersonsMergedEventChanges.DateOfBirth | PersonsMergedEventChanges.EmailAddress | PersonsMergedEventChanges.NationalInsuranceNumber | PersonsMergedEventChanges.Gender;

        var comments = "Some comments";
        var evidenceFile = new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        };

        var details = new EventModels.PersonAttributes
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = gender,
        };

        var oldDetails = new EventModels.PersonAttributes
        {
            FirstName = oldFirstName,
            MiddleName = oldMiddleName,
            LastName = oldLastName,
            DateOfBirth = oldDateOfBirth,
            EmailAddress = oldEmailAddress,
            NationalInsuranceNumber = oldNationalInsuranceNumber,
            Gender = oldGender
        };

        var updatedEvent = new PersonsMergedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            PersonTrn = person.Trn!,
            SecondaryPersonId = secondaryPerson.PersonId,
            SecondaryPersonTrn = secondaryPerson.Trn!,
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{secondaryPerson.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-persons-merged-event");
        Assert.NotNull(item);
        Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());

        doc.AssertRow("details", "Status", v => Assert.Equal("Deactivated", v.TrimmedText()));
        doc.AssertRow("previous-details", "Status", v => Assert.Equal("Active", v.TrimmedText()));
        doc.AssertRow("change-reason", "Reason", v => Assert.Equal("This record was identified as a duplicate and deactivated after a merge", v.TrimmedText()));
        doc.AssertRow("change-reason", "TRN of other record", v => Assert.Equal(person.Trn, v.TrimmedText()));
        doc.AssertRow("change-reason", "Comments", v => Assert.Equal(comments, v.TrimmedText()));
        doc.AssertRow("change-reason", "Evidence", v => Assert.Equal($"{evidenceFile!.Name} (opens in new tab)", v.TrimmedText()));
    }
}
