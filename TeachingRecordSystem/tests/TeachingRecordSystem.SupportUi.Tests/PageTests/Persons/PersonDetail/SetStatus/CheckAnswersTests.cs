using System.Text.Encodings.Web;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.SetStatus;

public class CheckAnswersTests(HostFixture hostFixture) : SetStatusTestBase(hostFixture)
{
    private const string ChangeReasonDetails = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";

    // TODO: extra BL from ticket:
    //   * The record must be read-only when deactivated, so we need to confirm if this needs anything further than simply removing and edit/change/add/delete link
    //   * The record would need to display that it is deactivated, and show the reason (person details page)
    //   * Check - identified duplicate - you'd need to check with devs if this is still relevant under how trs will deal with these
    // TODO: End-to-end tests (including manual merge)

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Get_WhenFieldChanged_ShowsReasonAndEvidenceFile_AsExpected(PersonStatus targetStatus)
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(true, evidenceFileId, "evidence.pdf", "1.2 MB");

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(PersonDeactivateReason.AnotherReason, ProvideMoreInformationOption.Yes, ChangeReasonDetails);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(PersonReactivateReason.AnotherReason, ProvideMoreInformationOption.Yes, ChangeReasonDetails);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("Reason", v => Assert.Equal("Another reason", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Additional information", v => Assert.Equal(ChangeReasonDetails, v.TrimmedText()));
        var urlEncoder = UrlEncoder.Default;
        var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
        var expectedFileUrl = $"http://localhost/files/evidence.pdf?fileUrl={expectedBlobStorageFileUrl}";
        doc.AssertSummaryListRowValue("Evidence", v =>
        {
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal("evidence.pdf (opens in new tab)", link.TrimmedText());
            Assert.Equal(expectedFileUrl, link.Href);
        });
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Get_RendersPersonInformationSummaryList(PersonStatus targetStatus)
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await CreatePersonToBecomeStatus(targetStatus, p => p
            .WithGender(Gender.Female)
            .WithNationalInsuranceNumber()
            .WithEmailAddress());

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(true, evidenceFileId, "evidence.pdf", "1.2 MB");

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(PersonDeactivateReason.AnotherReason, ProvideMoreInformationOption.Yes, ChangeReasonDetails);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(PersonReactivateReason.AnotherReason, ProvideMoreInformationOption.Yes, ChangeReasonDetails);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("Name", v => Assert.Equal($"{person.FirstName} {person.MiddleName} {person.LastName}", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Date of birth", v => Assert.Equal(person.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), v.TrimmedText()));
        doc.AssertSummaryListRowValue("Gender", v => Assert.Equal(person.Gender!.GetDisplayName(), v.TrimmedText()));
        doc.AssertSummaryListRowValue("Status", v => Assert.Equal(person.Person.Status.GetDisplayName(), v.TrimmedText()));
        doc.AssertSummaryListRowValue("National insurance number", v => Assert.Equal(person.NationalInsuranceNumber, v.TrimmedText()));
        doc.AssertSummaryListRowValue("Email address", v => Assert.Equal(person.EmailAddress, v.TrimmedText()));
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Get_WhenFieldChanged_ShowsMissingAdditionalDetailAndEvidenceFile_AsNotProvided(PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(false);

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(PersonDeactivateReason.RecordHolderDied, ProvideMoreInformationOption.No);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(PersonReactivateReason.DeactivatedByMistake, ProvideMoreInformationOption.No);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        if (targetStatus == PersonStatus.Deactivated)
        {
            doc.AssertSummaryListRowValue("Reason", v => Assert.Equal("The record holder died", v.TrimmedText()));
        }
        else
        {
            doc.AssertSummaryListRowValue("Reason", v => Assert.Equal("The record was deactivated by mistake", v.TrimmedText()));
        }
        doc.AssertSummaryListRowValue("Additional information", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertSummaryListRowValues("Evidence", v => Assert.Equal("Not provided", v.TrimmedText()));
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_Confirm_UpdatesPersonStatusCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(PersonStatus targetStatus)
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await CreatePersonToBecomeStatus(targetStatus, p => p
            .WithFirstName("Lily")
            .WithMiddleName("The")
            .WithLastName("Pink"));

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(true, evidenceFileId, "evidence.pdf", "1.2 MB");

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(PersonDeactivateReason.AnotherReason, ProvideMoreInformationOption.Yes, ChangeReasonDetails);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(PersonReactivateReason.AnotherReason, ProvideMoreInformationOption.Yes, ChangeReasonDetails);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            stateBuilder.Build());

        EventObserver.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        var expectedMessage = targetStatus == PersonStatus.Deactivated
            ? "Lily The Pink\u2019s record has been deactivated"
            : "Lily The Pink\u2019s record has been reactivated";
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, expectedMessage);

        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons
                .IgnoreQueryFilters()
                .SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(Clock.UtcNow, updatedPersonRecord.UpdatedOn);
            Assert.Equal(targetStatus, updatedPersonRecord.Status);
        });

        var raisedBy = GetCurrentUserId();

        EventObserver.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<PersonStatusUpdatedEvent>(e);

            Assert.Equal(Clock.UtcNow, actualEvent.CreatedUtc);
            Assert.Equal(person.PersonId, actualEvent.PersonId);
            Assert.Equal(targetStatus, actualEvent.Status);
            Assert.Equal(targetStatus == PersonStatus.Deactivated
                ? PersonStatus.Active
                : PersonStatus.Deactivated, actualEvent.OldStatus);
            Assert.Equal("Another reason", actualEvent.Reason);
            Assert.Equal(ChangeReasonDetails, actualEvent.ReasonDetail);
            Assert.Equal(evidenceFileId, actualEvent.EvidenceFile!.FileId);
            Assert.Equal("evidence.pdf", actualEvent.EvidenceFile.Name);
        });

        Events.AssertProcessesCreated(p =>
        {
            if (targetStatus == PersonStatus.Deactivated)
            {
                Assert.Equal(ProcessType.PersonDeactivating, p.ProcessContext.ProcessType);
                p.AssertProcessHasEvents<PersonDeactivatedEvent>();
            }
            else
            {
                Assert.Equal(ProcessType.PersonReactivating, p.ProcessContext.ProcessType);
                p.AssertProcessHasEvents<PersonReactivatedEvent>();
            }
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private string GetRequestPath(CreatePersonResult person, PersonStatus targetStatus, JourneyInstance<SetStatusState> journeyInstance) =>
        $"/persons/{person.PersonId}/set-status/{targetStatus}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<SetStatusState>> CreateJourneyInstanceAsync(Guid personId, SetStatusState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.SetStatus,
            state ?? new SetStatusState(),
            new KeyValuePair<string, object>("personId", personId));
}
