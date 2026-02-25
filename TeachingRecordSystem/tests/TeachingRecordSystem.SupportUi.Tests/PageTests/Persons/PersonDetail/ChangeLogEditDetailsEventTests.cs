using AngleSharp.Dom;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;
using PersonDetailsUpdatedEvent = TeachingRecordSystem.Core.Events.Legacy.PersonDetailsUpdatedEvent;
using PersonDetailsUpdatedEventChanges = TeachingRecordSystem.Core.Events.Legacy.PersonDetailsUpdatedEventChanges;

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
        Clock.SetUtcNow(new DateTimeOffset(nows.SingleRandom(), TimeSpan.Zero));
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
            ? PersonNameChangeReason.DeedPollOrOtherLegalProcess.GetDisplayName()
            : null;
        var nameChangeEvidenceFile = changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange)
            ? new EventModels.File
            {
                FileId = Guid.NewGuid(),
                Name = "name-evidence.jpg"
            }
            : null;

        var detailsChangeReason = changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange)
            ? PersonDetailsChangeReason.AnotherReason.GetDisplayName()
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

        var details = new EventModels.PersonDetails
        {
            FirstName = updatedFirstName,
            MiddleName = updatedMiddleName,
            LastName = updatedLastName,
            DateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? dateOfBirth : oldDateOfBirth,
            EmailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) && !newValueIsDefault ? emailAddress : null,
            NationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) && !newValueIsDefault ? nationalInsuranceNumber : null,
            Gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) && !newValueIsDefault ? gender : null
        };

        var oldDetails = new EventModels.PersonDetails
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

        await WithDbContextAsync(async dbContext =>
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
            doc.AssertSummaryListRowValue("details", "Name", v => Assert.Equal($"{updatedFirstName} {updatedMiddleName} {updatedLastName}", v.TrimmedText()));
            doc.AssertSummaryListRowValue("previous-details", "Name", v => Assert.Equal($"{oldFirstName} {oldMiddleName} {oldLastName}", v.TrimmedText()));
        }
        else
        {
            doc.AssertSummaryListRowDoesNotExist("details", "Name");
            doc.AssertSummaryListRowDoesNotExist("previous-details", "Name");
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth))
        {
            doc.AssertSummaryListRowValue("details", "Date of birth", v => Assert.Equal(dateOfBirth?.ToString(WebConstants.DateOnlyDisplayFormat), v.TrimmedText()));
            doc.AssertSummaryListRowValue("previous-details", "Date of birth", v => Assert.Equal(oldDateOfBirth?.ToString(WebConstants.DateOnlyDisplayFormat), v.TrimmedText()));
        }
        else
        {
            doc.AssertSummaryListRowDoesNotExist("details", "Date of birth");
            doc.AssertSummaryListRowDoesNotExist("previous-details", "Date of birth");
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress))
        {
            doc.AssertSummaryListRowValue("details", "Email address", v => Assert.Equal(newValueIsDefault ? WebConstants.EmptyFallbackContent : emailAddress, v.TrimmedText()));
            doc.AssertSummaryListRowValue("previous-details", "Email address", v => Assert.Equal(previousValueIsDefault ? WebConstants.EmptyFallbackContent : oldEmailAddress, v.TrimmedText()));
        }
        else
        {
            doc.AssertSummaryListRowDoesNotExist("details", "Email address");
            doc.AssertSummaryListRowDoesNotExist("previous-details", "Email address");
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber))
        {
            doc.AssertSummaryListRowValue("details", "National Insurance number", v => Assert.Equal(newValueIsDefault ? WebConstants.EmptyFallbackContent : nationalInsuranceNumber, v.TrimmedText()));
            doc.AssertSummaryListRowValue("previous-details", "National Insurance number", v => Assert.Equal(previousValueIsDefault ? WebConstants.EmptyFallbackContent : oldNationalInsuranceNumber, v.TrimmedText()));
        }
        else
        {
            doc.AssertSummaryListRowDoesNotExist("details", "National Insurance number");
            doc.AssertSummaryListRowDoesNotExist("previous-details", "National Insurance number");
        }

        if (changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender))
        {
            doc.AssertSummaryListRowValue("details", "Gender", v => Assert.Equal(newValueIsDefault ? WebConstants.EmptyFallbackContent : gender.GetDisplayName(), v.TrimmedText()));
            doc.AssertSummaryListRowValue("previous-details", "Gender", v => Assert.Equal(previousValueIsDefault ? WebConstants.EmptyFallbackContent : oldGender.GetDisplayName(), v.TrimmedText()));
        }
        else
        {
            doc.AssertSummaryListRowDoesNotExist("details", "Gender");
            doc.AssertSummaryListRowDoesNotExist("previous-details", "Gender");
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            doc.AssertSummaryListRowValue("change-reason", "Name change", v => Assert.Equal(nameChangeReason, v.TrimmedText()));
        }
        else
        {
            doc.AssertSummaryListRowDoesNotExist("change-reason", "Name change");
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            var keyContent = changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange)
                ? "Other personal details change"
                : "Personal details change";

            doc.AssertSummaryListRowValue("change-reason", keyContent, v => Assert.Equal(detailsChangeReason, v.TrimmedText()));
            doc.AssertSummaryListRowValue("change-reason", "Reason details", v => Assert.Equal(detailsChangeReasonDetail, v.TrimmedText()));
        }
        else
        {
            doc.AssertSummaryListRowDoesNotExist("change-reason", "Personal details change");
            doc.AssertSummaryListRowDoesNotExist("change-reason", "Other personal details change");
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

        doc.AssertSummaryListRowValues("change-reason", "Evidence", assertions.ToArray());
    }
}
