using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.AddPerson;

public class CheckAnswersTests(HostFixture hostFixture) : AddPersonTestBase(hostFixture)
{
    private const string ChangeReasonDetails = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";

    [Fact]
    public async Task Get_ConfirmAndCancelButtons_ExistOnPage()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("1 Feb 1980"),
                Reason = PersonCreateReason.MandatoryQualification,
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal("Confirm and create record", b.TrimmedText()),
            b => Assert.Equal("Cancel", b.TrimmedText()));
    }

    [Fact]
    public async Task Get_ShowsPersonalDetails_AsExpected()
    {
        // Arrange

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("1 Feb 1980"),
                EmailAddress = AddPersonFieldState<EmailAddress>.FromRawValue("test@test.com"),
                NationalInsuranceNumber = AddPersonFieldState<NationalInsuranceNumber>.FromRawValue("AB 12 34 56 C"),
                Gender = Gender.Other,
                Reason = PersonCreateReason.MandatoryQualification,
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("Full name", v => Assert.Equal("Alfred The Great", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Date of birth", v => Assert.Equal("1 February 1980", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Email address", v => Assert.Equal("test@test.com", v.TrimmedText()));
        doc.AssertSummaryListRowValue("National Insurance number", v => Assert.Equal("AB 12 34 56 C", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Gender", v => Assert.Equal("Other", v.TrimmedText()));
    }

    [Fact]
    public async Task Get_ShowsMissingOptionalPersonalDetails_AsNotProvided()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("1 Feb 1980"),
                Reason = PersonCreateReason.MandatoryQualification,
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("Full name", v => Assert.Equal("Alfred Great", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Email address", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertSummaryListRowValue("National Insurance number", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Gender", v => Assert.Equal("Not provided", v.TrimmedText()));
    }

    [Fact]
    public async Task Get_ShowsCreateReasonAndEvidenceFile_AsExpected()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                DateOfBirth = DateOnly.Parse("1 Feb 1980"),
                Reason = PersonCreateReason.AnotherReason,
                ReasonDetail = ChangeReasonDetails,
                Evidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = "evidence.pdf",
                        FileSizeDescription = "1.2 MB"
                    }
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("Reason", v => Assert.Equal("Another reason", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Reason details", v => Assert.Equal(ChangeReasonDetails, v.TrimmedText()));
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

    [Fact]
    public async Task Get_WhenMissingAdditionalDetailAndEvidenceFile_ShowsAsNotProvided()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                DateOfBirth = DateOnly.Parse("1 Feb 1980"),
                Reason = PersonCreateReason.MandatoryQualification,
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("Reason", v => Assert.Equal("They were awarded a mandatory qualification", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Reason details", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertSummaryListRowValues("Evidence", v => Assert.Equal("Not provided", v.TrimmedText()));
    }

    [Fact]
    public async Task Post_Confirm_UpdatesPersonEditDetailsCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var firstName = "Alfred";
        var middleName = "The";
        var lastName = "Great";
        var dateOfBirth = DateOnly.Parse("1 Feb 1980");
        var emailAddress = "test@test.com";
        var nationalInsuranceNumber = "AB123456C";
        var gender = Gender.Female;

        var otherEvidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                EmailAddress = AddPersonFieldState<EmailAddress>.FromRawValue(emailAddress),
                NationalInsuranceNumber = AddPersonFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber),
                Gender = gender,
                Reason = PersonCreateReason.AnotherReason,
                ReasonDetail = ChangeReasonDetails,
                Evidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = otherEvidenceFileId,
                        FileName = "other-evidence.png",
                        FileSizeDescription = "5MB"
                    }
                }
            });

        EventObserver.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var path = new Regex(@"/persons/([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})");
        var match = path.Match(response.Headers.Location?.OriginalString ?? "");
        Assert.True(match.Success);
        var personId = Guid.Parse(match.Groups[1].ToString());

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, expectedHeading: "Record created for Alfred The Great");

        await WithDbContextAsync(async dbContext =>
        {
            var createdPersonRecord = await dbContext.Persons.SingleAsync(p => p.PersonId == personId);
            Assert.Equal(TimeProvider.UtcNow, createdPersonRecord.CreatedOn);
            Assert.Equal(TimeProvider.UtcNow, createdPersonRecord.UpdatedOn);
            Assert.Equal(firstName, createdPersonRecord.FirstName);
            Assert.Equal(middleName, createdPersonRecord.MiddleName);
            Assert.Equal(lastName, createdPersonRecord.LastName);
            Assert.Equal(dateOfBirth, createdPersonRecord.DateOfBirth);
            Assert.Equal(emailAddress, createdPersonRecord.EmailAddress);
            Assert.Equal(nationalInsuranceNumber, createdPersonRecord.NationalInsuranceNumber);
            Assert.Equal(gender, createdPersonRecord.Gender);
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<LegacyEvents.PersonCreatedEvent>(e);

            Assert.Equal(TimeProvider.UtcNow, actualEvent.CreatedUtc);
            Assert.Equal(personId, actualEvent.PersonId);
            Assert.Equal(firstName, actualEvent.PersonAttributes.FirstName);
            Assert.Equal(middleName, actualEvent.PersonAttributes.MiddleName);
            Assert.Equal(lastName, actualEvent.PersonAttributes.LastName);
            Assert.Equal(dateOfBirth, actualEvent.PersonAttributes.DateOfBirth);
            Assert.Equal(emailAddress, actualEvent.PersonAttributes.EmailAddress);
            Assert.Equal(nationalInsuranceNumber, actualEvent.PersonAttributes.NationalInsuranceNumber);
            Assert.Equal(gender, actualEvent.PersonAttributes.Gender);
            Assert.Equal("Another reason", actualEvent.CreateReason);
            Assert.Equal(ChangeReasonDetails, actualEvent.CreateReasonDetail);
            Assert.Equal(otherEvidenceFileId, actualEvent.EvidenceFile!.FileId);
            Assert.Equal("other-evidence.png", actualEvent.EvidenceFile.Name);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.PersonCreating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<PersonCreatedEvent>();

            var changeReasonInfo = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(p.ProcessContext.Process.ChangeReason);
            Assert.Equal("Another reason", changeReasonInfo.Reason);
            Assert.Equal(ChangeReasonDetails, changeReasonInfo.Details);
            Assert.Equal(otherEvidenceFileId, changeReasonInfo.EvidenceFile?.FileId);
            Assert.Equal("other-evidence.png", changeReasonInfo?.EvidenceFile?.Name);
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    private string GetRequestPath(AddPersonJourneyCoordinator journeyInstance) =>
        $"/persons/add/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

    [Theory]
    [InlineData(UserRoles.Viewer)]
    [InlineData(null)]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("1 Feb 1980"),
                EmailAddress = AddPersonFieldState<EmailAddress>.FromRawValue("some@email-address.com"),
                NationalInsuranceNumber = AddPersonFieldState<NationalInsuranceNumber>.FromRawValue("AB123456D"),
                Reason = PersonCreateReason.MandatoryQualification,
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }


    [Fact]
    public async Task Get_BacklinkLinksToReasonPage()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("1 Feb 1980"),
                EmailAddress = AddPersonFieldState<EmailAddress>.FromRawValue("some@email-address.com"),
                NationalInsuranceNumber = AddPersonFieldState<NationalInsuranceNumber>.FromRawValue("AB123456D"),
                Reason = PersonCreateReason.MandatoryQualification,
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        Assert.Contains("/persons/add/reason", backlink.Href);
    }


    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToAddPersonIndexPage()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("1 Feb 1980"),
                EmailAddress = AddPersonFieldState<EmailAddress>.FromRawValue("some@email-address.com"),
                NationalInsuranceNumber = AddPersonFieldState<NationalInsuranceNumber>.FromRawValue("AB123456D"),
                Reason = PersonCreateReason.MandatoryQualification,
                Evidence = new()
                {
                    UploadEvidence = false
                }
            });

        var pageUrl = GetRequestPath(journeyInstance);
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;
        Assert.NotNull(cancelButton);
        Assert.Equal("Cancel", cancelButton.Name);

        // Act
        var cancelRequest = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
        var cancelResponse = await HttpClient.SendAsync(cancelRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)cancelResponse.StatusCode);
        Assert.Equal("/persons/add", cancelResponse.Headers.Location?.OriginalString);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_Cancel_EvidenceFilePreviouslyUploaded_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("1 Feb 1980"),
                Reason = PersonCreateReason.MandatoryQualification,
                Evidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = "evidence.jpg",
                        FileSizeDescription = "1.2 KB"
                    }
                }
            });

        var pageUrl = GetRequestPath(journeyInstance);
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        // Act
        var cancelRequest = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
        var cancelResponse = await HttpClient.SendAsync(cancelRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)cancelResponse.StatusCode);
        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }
}
