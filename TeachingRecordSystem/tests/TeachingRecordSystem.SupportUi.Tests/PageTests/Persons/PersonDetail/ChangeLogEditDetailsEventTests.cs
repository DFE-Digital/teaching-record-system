using AngleSharp.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogEditDetailsEventTests : TestBase
{
    public ChangeLogEditDetailsEventTests(HostFixture hostFixture) : base(hostFixture)
    {
        // Toggle between GMT and BST to ensure we're testing rendering dates in local time
        var nows = new[]
        {
            new DateTime(2024, 1, 1, 12, 13, 14, DateTimeKind.Utc),  // GMT
            new DateTime(2024, 7, 5, 19, 20, 21, DateTimeKind.Utc)   // BST
        };
        Clock.UtcNow = nows.SingleRandom();
    }

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
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName | PersonDetailsUpdatedEventChanges.MiddleName | PersonDetailsUpdatedEventChanges.LastName | PersonDetailsUpdatedEventChanges.DateOfBirth | PersonDetailsUpdatedEventChanges.EmailAddress | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber | PersonDetailsUpdatedEventChanges.Gender, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName | PersonDetailsUpdatedEventChanges.MiddleName | PersonDetailsUpdatedEventChanges.LastName | PersonDetailsUpdatedEventChanges.DateOfBirth | PersonDetailsUpdatedEventChanges.EmailAddress | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber | PersonDetailsUpdatedEventChanges.Gender, false, true)]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName | PersonDetailsUpdatedEventChanges.MiddleName | PersonDetailsUpdatedEventChanges.LastName | PersonDetailsUpdatedEventChanges.DateOfBirth | PersonDetailsUpdatedEventChanges.EmailAddress | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber | PersonDetailsUpdatedEventChanges.Gender, true, false)]
    public async Task Person_WithPersonDetailsUpdatedEvent_RendersExpectedContent(PersonDetailsUpdatedEventChanges changes, bool previousValueIsDefault, bool newValueIsDefault)
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

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

        var updatedFirstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? firstName : oldFirstName;
        var updatedMiddleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? middleName : oldMiddleName;
        var updatedLastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? lastName : oldLastName;

        var nameChangeReason = changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange)
            ? EditDetailsNameChangeReasonOption.DeedPollOrOtherLegalProcess.GetDisplayName()
            : null;
        var nameChangeEvidenceFile = changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange)
            ? new EventModels.File
            {
                FileId = Guid.NewGuid(),
                Name = "name-evidence.jpg"
            }
            : null;

        var detailsChangeReason = changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange)
            ? EditDetailsOtherDetailsChangeReasonOption.AnotherReason.GetDisplayName()
            : null;
        var detailsChangeReasonDetail = changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange)
            ? "Reason detail"
            : null;
        var detailsChangeEvidenceFile = changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange)
            ? new EventModels.File
            {
                FileId = Guid.NewGuid(),
                Name = "other-evidence.jpg"
            }
            : null;

        var details = new EventModels.PersonAttributes
        {
            FirstName = updatedFirstName,
            MiddleName = updatedMiddleName,
            LastName = updatedLastName,
            DateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? dateOfBirth : oldDateOfBirth,
            EmailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) && !newValueIsDefault ? emailAddress : null,
            NationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) && !newValueIsDefault ? nationalInsuranceNumber : null,
            Gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) && !newValueIsDefault ? gender : null
        };

        var oldDetails = new EventModels.PersonAttributes
        {
            FirstName = oldFirstName,
            MiddleName = oldMiddleName,
            LastName = oldLastName,
            DateOfBirth = oldDateOfBirth,
            EmailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) && !previousValueIsDefault ? oldEmailAddress : null,
            NationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) && !previousValueIsDefault ? oldNationalInsuranceNumber : null,
            Gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) && !previousValueIsDefault ? oldGender : null
        };

        var updatedEvent = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            PersonAttributes = details,
            OldPersonAttributes = oldDetails,
            Changes = changes,
            NameChangeReason = nameChangeReason,
            NameChangeEvidenceFile = nameChangeEvidenceFile,
            DetailsChangeReason = detailsChangeReason,
            DetailsChangeReasonDetail = detailsChangeReasonDetail,
            DetailsChangeEvidenceFile = detailsChangeEvidenceFile
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
        Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            doc.AssertRow("details", "Name", v => Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", v.TrimmedText()));
            doc.AssertRow("previous-details", "Name", v => Assert.Equal($"{oldFirstName} {oldMiddleName} {oldLastName}", v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Name");
            doc.AssertRowDoesNotExist("previous-details", "Name");
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth))
        {
            doc.AssertRow("details", "Date of birth", v => Assert.Equal(dateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
            doc.AssertRow("previous-details", "Date of birth", v => Assert.Equal(oldDateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Date of birth");
            doc.AssertRowDoesNotExist("previous-details", "Date of birth");
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress))
        {
            doc.AssertRow("details", "Email address", v => Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : emailAddress, v.TrimmedText()));
            doc.AssertRow("previous-details", "Email address", v => Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldEmailAddress, v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Email address");
            doc.AssertRowDoesNotExist("previous-details", "Email address");
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber))
        {
            doc.AssertRow("details", "National Insurance number", v => Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : nationalInsuranceNumber, v.TrimmedText()));
            doc.AssertRow("previous-details", "National Insurance number", v => Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldNationalInsuranceNumber, v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "National Insurance number");
            doc.AssertRowDoesNotExist("previous-details", "National Insurance number");
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender))
        {
            doc.AssertRow("details", "Gender", v => Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : gender.GetDisplayName(), v.TrimmedText()));
            doc.AssertRow("previous-details", "Gender", v => Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldGender.GetDisplayName(), v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("details", "Gender");
            doc.AssertRowDoesNotExist("previous-details", "Gender");
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            doc.AssertRow("change-reason", "Name change", v => Assert.Equal(nameChangeReason, v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("change-reason", "Name change");
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            var keyContent = changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange)
                ? "Other personal details change"
                : "Personal details change";

            doc.AssertRow("change-reason", keyContent, v => Assert.Equal(detailsChangeReason, v.TrimmedText()));
            doc.AssertRow("change-reason", "Reason details", v => Assert.Equal(detailsChangeReasonDetail, v.TrimmedText()));
        }
        else
        {
            doc.AssertRowDoesNotExist("change-reason", "Personal details change");
            doc.AssertRowDoesNotExist("change-reason", "Other personal details change");
        }

        var assertions = new List<Action<IElement>>();
        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            assertions.Add(v => Assert.Equal($"{nameChangeEvidenceFile!.Name} (opens in new tab)", v.TrimmedText()));
        }
        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            assertions.Add(v => Assert.Equal($"{detailsChangeEvidenceFile!.Name} (opens in new tab)", v.TrimmedText()));
        }

        doc.AssertRows("change-reason", "Evidence", assertions.ToArray());
    }
}
