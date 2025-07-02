using TeachingRecordSystem.SupportUi.Pages.Persons.Create;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogCreateEventTests : TestBase
{
    public ChangeLogCreateEventTests(HostFixture hostFixture) : base(hostFixture)
    {
        // Toggle between GMT and BST to ensure we're testing rendering dates in local time
        var nows = new[]
        {
            new DateTime(2024, 1, 1, 12, 13, 14, DateTimeKind.Utc),  // GMT
            new DateTime(2024, 7, 5, 19, 20, 21, DateTimeKind.Utc)   // BST
        };
        Clock.UtcNow = nows.RandomOne();
    }

    [Fact]
    public async Task Person_WithPersonCreatedEvent_RendersExpectedContent()
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        string firstName = "Alfred";
        string middleName = "The";
        string lastName = "Great";
        DateOnly? dateOfBirth = Clock.Today.AddYears(-30);
        string? emailAddress = "old@email-address.com";
        string? mobileNumber = "07654321098";
        string? nationalInsuranceNumber = "AB 12 34 56 D";
        Gender? gender = Gender.Female;

        var createReason = CreateReasonOption.AnotherReason.GetDisplayName();
        var createReasonDetail = "Reason detail";
        var evidenceFile = new EventModels.File
        {
            FileId = Guid.NewGuid(),
            Name = "other-evidence.jpg"
        };

        var details = new EventModels.PersonDetails
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = emailAddress,
            MobileNumber = mobileNumber,
            NationalInsuranceNumber = nationalInsuranceNumber,
            Gender = gender
        };

        var createdEvent = new PersonCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            Details = details,
            CreateReason = createReason,
            CreateReasonDetail = createReasonDetail,
            EvidenceFile = evidenceFile
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(createdEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-created-event");
        Assert.NotNull(item);
        Assert.Equal($"By {createdByUser.Name} on", item.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), item.GetElementByTestId("timeline-item-time")?.TrimmedText());

        doc.AssertSummaryListValue("details", "Name", v => Assert.Equal($"{firstName} {middleName} {lastName}", v.TrimmedText()));
        doc.AssertSummaryListValue("details", "Date of birth", v => Assert.Equal(dateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        doc.AssertSummaryListValue("details", "Email address", v => Assert.Equal(emailAddress, v.TrimmedText()));
        doc.AssertSummaryListValue("details", "Mobile number", v => Assert.Equal(mobileNumber, v.TrimmedText()));
        doc.AssertSummaryListValue("details", "National Insurance number", v => Assert.Equal(nationalInsuranceNumber, v.TrimmedText()));
        doc.AssertSummaryListValue("details", "Gender", v => Assert.Equal(gender.GetDisplayName(), v.TrimmedText()));

        doc.AssertSummaryListValue("create-reason", "Reason", v => Assert.Equal(createReason, v.TrimmedText()));
        doc.AssertSummaryListValue("create-reason", "Reason details", v => Assert.Equal(createReasonDetail, v.TrimmedText()));
        doc.AssertSummaryListValue("create-reason", "Evidence", v => Assert.Equal($"{evidenceFile!.Name} (opens in new tab)", v.TrimmedText()));
    }

    [Fact]
    public async Task Person_WithPersonCreatedEvent_DoesNotRenderNullDetails_AndRendersNullReasonsAsNotProvided()
    {
        // Arrange
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync();

        string firstName = "Alfred";
        string middleName = "The";
        string lastName = "Great";
        DateOnly? dateOfBirth = Clock.Today.AddYears(-30);

        var createReason = CreateReasonOption.AnotherReason.GetDisplayName();

        var details = new EventModels.PersonDetails
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            EmailAddress = null,
            MobileNumber = null,
            NationalInsuranceNumber = null,
            Gender = null
        };

        var createdEvent = new PersonCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            Details = details,
            CreateReason = createReason,
            CreateReasonDetail = null,
            EvidenceFile = null
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(createdEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var item = doc.GetElementByTestId("timeline-item-created-event");
        Assert.NotNull(item);

        doc.AssertSummaryListRowDoesNotExist("details", "Email address");
        doc.AssertSummaryListRowDoesNotExist("details", "Mobile number");
        doc.AssertSummaryListRowDoesNotExist("details", "National Insurance number");
        doc.AssertSummaryListRowDoesNotExist("details", "Gender");

        doc.AssertSummaryListValue("create-reason", "Reason details", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertSummaryListValue("create-reason", "Evidence", v => Assert.Equal("Not provided", v.TrimmedText()));
    }
}
