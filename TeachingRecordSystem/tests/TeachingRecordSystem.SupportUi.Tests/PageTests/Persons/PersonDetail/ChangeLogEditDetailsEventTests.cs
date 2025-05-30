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
        Clock.UtcNow = nows.RandomOne();
    }

    [Theory]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.MiddleName, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.LastName, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.DateOfBirth, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.EmailAddress, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.EmailAddress, true, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.EmailAddress, false, true)]
    [InlineData(PersonDetailsUpdatedEventChanges.MobileNumber, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.MobileNumber, true, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.MobileNumber, false, true)]
    [InlineData(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, false, true)]
    [InlineData(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, true, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName | PersonDetailsUpdatedEventChanges.MiddleName | PersonDetailsUpdatedEventChanges.LastName | PersonDetailsUpdatedEventChanges.DateOfBirth | PersonDetailsUpdatedEventChanges.EmailAddress | PersonDetailsUpdatedEventChanges.MobileNumber | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, false, false)]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName | PersonDetailsUpdatedEventChanges.MiddleName | PersonDetailsUpdatedEventChanges.LastName | PersonDetailsUpdatedEventChanges.DateOfBirth | PersonDetailsUpdatedEventChanges.EmailAddress | PersonDetailsUpdatedEventChanges.MobileNumber | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, false, true)]
    [InlineData(PersonDetailsUpdatedEventChanges.FirstName | PersonDetailsUpdatedEventChanges.MiddleName | PersonDetailsUpdatedEventChanges.LastName | PersonDetailsUpdatedEventChanges.DateOfBirth | PersonDetailsUpdatedEventChanges.EmailAddress | PersonDetailsUpdatedEventChanges.MobileNumber | PersonDetailsUpdatedEventChanges.NationalInsuranceNumber, true, false)]
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
        string? oldMobileNumber = "07654321098";
        string? oldNationalInsuranceNumber = "AB 12 34 56 D";

        string firstName = "Megan";
        string middleName = "Thee";
        string lastName = "Stallion";
        DateOnly? dateOfBirth = Clock.Today.AddYears(-20);
        string emailAddress = "new@email-address.com";
        string? mobileNumber = "07890123456";
        string? nationalInsuranceNumber = "XY 98 76 54 A";

        var changeReason = EditDetailsChangeReasonOption.AnotherReason.GetDisplayName();
        var changeReasonDetail = "Reason detail";
        var evidenceFile = new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "evidence.jpg"
        };

        var updatedFirstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? firstName : oldFirstName;
        var updatedMiddleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? middleName : oldMiddleName;
        var updatedLastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? lastName : oldLastName;

        var details = new EventModels.PersonDetails
        {
            FirstName = updatedFirstName,
            MiddleName = updatedMiddleName,
            LastName = updatedLastName,
            DateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? dateOfBirth : oldDateOfBirth,
            EmailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) && !newValueIsDefault ? emailAddress : null,
            MobileNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.MobileNumber) && !newValueIsDefault ? mobileNumber : null,
            NationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) && !newValueIsDefault ? nationalInsuranceNumber : null,
        };

        var oldDetails = new EventModels.PersonDetails
        {
            FirstName = oldFirstName,
            MiddleName = oldMiddleName,
            LastName = oldLastName,
            DateOfBirth = oldDateOfBirth,
            EmailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) && !previousValueIsDefault ? oldEmailAddress : null,
            MobileNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.MobileNumber) && !previousValueIsDefault ? oldMobileNumber : null,
            NationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) && !previousValueIsDefault ? oldNationalInsuranceNumber : null,
        };

        var updatedEvent = new PersonDetailsUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            Details = details,
            OldDetails = oldDetails,
            Changes = changes,
            ChangeReason = changeReason,
            ChangeReasonDetail = changeReasonDetail,
            EvidenceFile = evidenceFile
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

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.AnyNameChange))
        {
            Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", item.GetElementByTestId("details-name")?.TrimmedText());
            Assert.Equal($"{oldFirstName} {oldMiddleName} {oldLastName}", item.GetElementByTestId("old-details-name")?.TrimmedText());
        }
        else
        {
            Assert.Null(item.GetElementByTestId("details-name"));
            Assert.Null(item.GetElementByTestId("old-details-name"));
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth))
        {
            Assert.Equal(dateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("details-dob")?.TrimmedText());
            Assert.Equal(oldDateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat), item.GetElementByTestId("old-details-dob")?.TrimmedText());
        }
        else
        {
            Assert.Null(item.GetElementByTestId("details-dob"));
            Assert.Null(item.GetElementByTestId("old-details-dob"));
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress))
        {
            Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : emailAddress, item.GetElementByTestId("details-email")?.TrimmedText());
            Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldEmailAddress, item.GetElementByTestId("old-details-email")?.TrimmedText());
        }
        else
        {
            Assert.Null(item.GetElementByTestId("details-email"));
            Assert.Null(item.GetElementByTestId("old-details-email"));
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.MobileNumber))
        {
            Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : mobileNumber, item.GetElementByTestId("details-mobile")?.TrimmedText());
            Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldMobileNumber, item.GetElementByTestId("old-details-mobile")?.TrimmedText());
        }
        else
        {
            Assert.Null(item.GetElementByTestId("details-mobile"));
            Assert.Null(item.GetElementByTestId("old-details-mobile"));
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber))
        {
            Assert.Equal(newValueIsDefault ? UiDefaults.EmptyDisplayContent : nationalInsuranceNumber, item.GetElementByTestId("details-nino")?.TrimmedText());
            Assert.Equal(previousValueIsDefault ? UiDefaults.EmptyDisplayContent : oldNationalInsuranceNumber, item.GetElementByTestId("old-details-nino")?.TrimmedText());
        }
        else
        {
            Assert.Null(item.GetElementByTestId("details-nino"));
            Assert.Null(item.GetElementByTestId("old-details-nino"));
        }

        Assert.Equal(changeReason, item.GetElementByTestId("reason")?.TrimmedText());
        Assert.Equal(changeReasonDetail, item.GetElementByTestId("reason-detail")?.TrimmedText());
        Assert.Equal($"{evidenceFile.Name} (opens in new tab)", item.GetElementByTestId("uploaded-evidence-link")?.TrimmedText());
    }
}
