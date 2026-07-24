using System.Text.Encodings.Web;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;
using PersonDetailsUpdatedEventChanges = TeachingRecordSystem.Core.Events.Legacy.PersonDetailsUpdatedEventChanges;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class CheckAnswersTests(HostFixture hostFixture) : EditDetailsTestBase(hostFixture)
{
    private const string ChangeReasonDetails = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";

    [Fact]
    public async Task Get_PageLegend_PopulatedFromOriginalPersonName()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great"));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "A";
                s.MiddleName = "New";
                s.LastName = "Name";
                s.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var caption = doc.GetElementByTestId("edit-details-caption");
        Assert.Equal("Change personal details - Alfred The Great", caption!.TrimmedText());
    }

    [Fact]
    public async Task Get_ConfirmAndCancelButtons_ExistOnPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "A";
                s.MiddleName = "New";
                s.LastName = "Name";
                s.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal("Confirm changes", b.TrimmedText()),
            b => Assert.Equal("Cancel and return to record", b.TrimmedText()));
    }

    [Fact]
    public async Task Get_ShowsPersonalDetails_AsExpected()
    {
        // Arrange

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue("test@test.com");
                s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue("AB 12 34 56 C");
                s.Gender = Gender.Male;
                s.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("Full name", v => Assert.Equal("Alfred The Great", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Date of birth", v => Assert.Equal("1 February 1980", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Email address", v => Assert.Equal("test@test.com", v.TrimmedText()));
        doc.AssertSummaryListRowValue("National Insurance number", v => Assert.Equal("AB 12 34 56 C", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Gender", v => Assert.Equal("Male", v.TrimmedText()));
    }

    [Fact]
    public async Task Get_ShowsMissingOptionalPersonalDetails_AsNotProvided()
    {
        // Arrange

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

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
    public async Task Get_WhenNameFieldChanged_ShowsNameChangeReasonAndEvidenceFile_AsExpected()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.NameChangeReason = PersonNameChangeReason.DeedPollOrOtherLegalProcess;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = "evidence.pdf",
                        FileSizeDescription = "1.2 MB"
                    }
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("Reason for name change", v => Assert.Equal("Name has changed by deed poll or another legal process", v.TrimmedText()));
        var urlEncoder = UrlEncoder.Default;
        var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
        var expectedFileUrl = $"http://localhost/files/evidence.pdf?fileUrl={expectedBlobStorageFileUrl}";
        doc.AssertSummaryListRowValues("Evidence uploaded", v =>
        {
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal("evidence.pdf (opens in new tab)", link.TrimmedText());
            Assert.Equal(expectedFileUrl, link.Href);
        });

        doc.AssertSummaryListRowDoesNotExist("Personal details change");
        doc.AssertSummaryListRowDoesNotExist("Other personal details change");
        doc.AssertSummaryListRowDoesNotExist("Reason details");
        doc.AssertSummaryListRowDoesNotExist("Evidence");
    }

    [Fact]
    public async Task Get_WhenOtherDetailsFieldChanged_ShowsDetailsChangeReasonAndEvidenceFile_AsExpected()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.AnotherReason;
                s.OtherDetailsChangeReasonDetail = ChangeReasonDetails;
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = "evidence.pdf",
                        FileSizeDescription = "1.2 MB"
                    }
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("Reason for personal details change", v => Assert.Equal("Another reason", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Reason details", v => Assert.Equal(ChangeReasonDetails, v.TrimmedText()));
        var urlEncoder = UrlEncoder.Default;
        var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
        var expectedFileUrl = $"http://localhost/files/evidence.pdf?fileUrl={expectedBlobStorageFileUrl}";
        doc.AssertSummaryListRowValue("Evidence uploaded", v =>
        {
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal("evidence.pdf (opens in new tab)", link.TrimmedText());
            Assert.Equal(expectedFileUrl, link.Href);
        });

        doc.AssertSummaryListRowDoesNotExist("Reason for name change");
        doc.AssertSummaryListRowDoesNotExist("Other personal details change");
        doc.AssertSummaryListRowDoesNotExist("Evidence");
    }

    [Fact]
    public async Task Get_WhenNameAndOtherFieldsChanged_ShowsNameChangeReasonAndOtherChangeReasonAndEvidenceFiles_AsExpected()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.NameChangeReason = PersonNameChangeReason.DeedPollOrOtherLegalProcess;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = "name-evidence.pdf",
                        FileSizeDescription = "2.4 MB"
                    }
                };
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.AnotherReason;
                s.OtherDetailsChangeReasonDetail = ChangeReasonDetails;
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = "other-evidence.pdf",
                        FileSizeDescription = "1.2 MB"
                    }
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("Reason for name change", v => Assert.Equal("Name has changed by deed poll or another legal process", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Other personal details change", v => Assert.Equal("Another reason", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Reason details", v => Assert.Equal(ChangeReasonDetails, v.TrimmedText()));
        var urlEncoder = UrlEncoder.Default;
        var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
        doc.AssertSummaryListRowValues("Evidence", v =>
        {
            var expectedFileUrl = $"http://localhost/files/name-evidence.pdf?fileUrl={expectedBlobStorageFileUrl}";
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal("name-evidence.pdf (opens in new tab)", link.TrimmedText());
            Assert.Equal(expectedFileUrl, link.Href);
        }, v =>
        {
            var expectedFileUrl = $"http://localhost/files/other-evidence.pdf?fileUrl={expectedBlobStorageFileUrl}";
            var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(v.QuerySelector("a"));
            Assert.Equal("other-evidence.pdf (opens in new tab)", link.TrimmedText());
            Assert.Equal(expectedFileUrl, link.Href);
        });

        doc.AssertSummaryListRowDoesNotExist("Reason for personal details change");
    }

    [Fact]
    public async Task Get_WhenNameFieldChanged_ShowsMissingAdditionalDetailAndEvidenceFile_AsNotProvided()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.NameChangeReason = PersonNameChangeReason.DeedPollOrOtherLegalProcess;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("Reason for name change", v => Assert.Equal("Name has changed by deed poll or another legal process", v.TrimmedText()));
        doc.AssertSummaryListRowValues("Evidence uploaded", v => Assert.Equal("Not provided", v.TrimmedText()));

        doc.AssertSummaryListRowDoesNotExist("Other personal details change");
        doc.AssertSummaryListRowDoesNotExist("Reason for personal details change");
        doc.AssertSummaryListRowDoesNotExist("Reason details");
        doc.AssertSummaryListRowDoesNotExist("Evidence");
    }

    [Fact]
    public async Task Get_WhenOtherDetailsFieldChanged_ShowsMissingAdditionalDetailAndEvidenceFile_AsNotProvided()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValue("Reason for personal details change", v => Assert.Equal("Data loss or incomplete information", v.TrimmedText()));
        doc.AssertSummaryListRowValue("Reason details", v => Assert.Equal("Not provided", v.TrimmedText()));
        doc.AssertSummaryListRowValues("Evidence uploaded", v => Assert.Equal("Not provided", v.TrimmedText()));

        doc.AssertSummaryListRowDoesNotExist("Other personal details change");
        doc.AssertSummaryListRowDoesNotExist("Reason for name change");
        doc.AssertSummaryListRowDoesNotExist("Evidence");
    }

    [Theory]
    [InlineData(LegacyEvents.PersonDetailsUpdatedEventChanges.FirstName)]
    [InlineData(LegacyEvents.PersonDetailsUpdatedEventChanges.MiddleName)]
    [InlineData(LegacyEvents.PersonDetailsUpdatedEventChanges.LastName)]
    [InlineData(LegacyEvents.PersonDetailsUpdatedEventChanges.DateOfBirth)]
    [InlineData(LegacyEvents.PersonDetailsUpdatedEventChanges.EmailAddress)]
    [InlineData(LegacyEvents.PersonDetailsUpdatedEventChanges.NationalInsuranceNumber)]
    [InlineData(LegacyEvents.PersonDetailsUpdatedEventChanges.Gender)]
    [InlineData(LegacyEvents.PersonDetailsUpdatedEventChanges.FirstName | LegacyEvents.PersonDetailsUpdatedEventChanges.MiddleName | LegacyEvents.PersonDetailsUpdatedEventChanges.LastName | LegacyEvents.PersonDetailsUpdatedEventChanges.DateOfBirth | LegacyEvents.PersonDetailsUpdatedEventChanges.EmailAddress | LegacyEvents.PersonDetailsUpdatedEventChanges.NationalInsuranceNumber | LegacyEvents.PersonDetailsUpdatedEventChanges.Gender)]
    public async Task Post_Confirm_UpdatesPersonEditDetailsCreatesEventCompletesJourneyAndRedirectsWithFlashMessage(LegacyEvents.PersonDetailsUpdatedEventChanges changes)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("test@test.com")
            .WithNationalInsuranceNumber("AB123456C")
            .WithGender(Gender.Other));

        var firstName = changes.HasFlag(LegacyEvents.PersonDetailsUpdatedEventChanges.FirstName) ? "Jim" : person.FirstName;
        var middleName = changes.HasFlag(LegacyEvents.PersonDetailsUpdatedEventChanges.MiddleName) ? "A" : person.MiddleName;
        var lastName = changes.HasFlag(LegacyEvents.PersonDetailsUpdatedEventChanges.LastName) ? "Person" : person.LastName;
        var dateOfBirth = changes.HasFlag(LegacyEvents.PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("3 July 1990") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(LegacyEvents.PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email.com" : person.EmailAddress;
        var nationalInsuranceNumber = changes.HasFlag(LegacyEvents.PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "JK987654D" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(LegacyEvents.PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var nameEvidenceFileId = Guid.NewGuid();
        var otherEvidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = firstName;
                s.MiddleName = middleName;
                s.LastName = lastName;
                s.DateOfBirth = dateOfBirth;
                s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(emailAddress);
                s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber);
                s.Gender = gender;
                s.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false,
                    UploadedEvidenceFile = new()
                    {
                        FileId = nameEvidenceFileId,
                        FileName = "name-evidence.pdf",
                        FileSizeDescription = "2.4 MB"
                    }
                };
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.AnotherReason;
                s.OtherDetailsChangeReasonDetail = ChangeReasonDetails;
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = otherEvidenceFileId,
                        FileName = "other-evidence.png",
                        FileSizeDescription = "5MB"
                    }
                };
            }));

        EventObserver.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, expectedHeading: "Personal details have been updated");

        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(TimeProvider.UtcNow, updatedPersonRecord.UpdatedOn);
            Assert.Equal(firstName, updatedPersonRecord.FirstName);
            Assert.Equal(middleName, updatedPersonRecord.MiddleName);
            Assert.Equal(lastName, updatedPersonRecord.LastName);
            Assert.Equal(dateOfBirth, updatedPersonRecord.DateOfBirth);
            Assert.Equal(emailAddress, updatedPersonRecord.EmailAddress);
            Assert.Equal(nationalInsuranceNumber, updatedPersonRecord.NationalInsuranceNumber);
            Assert.Equal(gender, updatedPersonRecord.Gender);
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<LegacyEvents.PersonDetailsUpdatedEvent>(e);

            Assert.Equal(TimeProvider.UtcNow, actualEvent.CreatedUtc);
            Assert.Equal(person.PersonId, actualEvent.PersonId);
            Assert.Equal(firstName, actualEvent.PersonAttributes.FirstName);
            Assert.Equal(middleName, actualEvent.PersonAttributes.MiddleName);
            Assert.Equal(lastName, actualEvent.PersonAttributes.LastName);
            Assert.Equal(dateOfBirth, actualEvent.PersonAttributes.DateOfBirth);
            Assert.Equal(emailAddress, actualEvent.PersonAttributes.EmailAddress);
            Assert.Equal(nationalInsuranceNumber, actualEvent.PersonAttributes.NationalInsuranceNumber);
            Assert.Equal(gender, actualEvent.PersonAttributes.Gender);
            Assert.Equal("Correcting an error", actualEvent.NameChangeReason);
            Assert.Equal(nameEvidenceFileId, actualEvent.NameChangeEvidenceFile!.FileId);
            Assert.Equal("name-evidence.pdf", actualEvent.NameChangeEvidenceFile.Name);
            Assert.Equal("Another reason", actualEvent.DetailsChangeReason);
            Assert.Equal(ChangeReasonDetails, actualEvent.DetailsChangeReasonDetail);
            Assert.Equal(otherEvidenceFileId, actualEvent.DetailsChangeEvidenceFile!.FileId);
            Assert.Equal("other-evidence.png", actualEvent.DetailsChangeEvidenceFile.Name);
            Assert.Equal(changes, actualEvent.Changes);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.PersonDetailsUpdating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<PersonDetailsUpdatedEvent>();

            var changeReasonInfo = Assert.IsType<PersonDetailsChangeReasonInfo>(p.ProcessContext.Process.ChangeReason);
            Assert.Equal("Another reason", changeReasonInfo.Reason);
            Assert.Equal(ChangeReasonDetails, changeReasonInfo.Details);
            Assert.Equal(otherEvidenceFileId, changeReasonInfo.EvidenceFile?.FileId);
            Assert.Equal("other-evidence.png", changeReasonInfo.EvidenceFile?.Name);
            Assert.Equal("Correcting an error", changeReasonInfo.NameChangeReason);
            Assert.Equal(nameEvidenceFileId, changeReasonInfo.NameChangeEvidenceFile?.FileId);
            Assert.Equal("name-evidence.pdf", changeReasonInfo.NameChangeEvidenceFile?.Name);
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_Confirm_WhenAnyNameFieldChanged_AndNameChangeReasonIsCorrectingAnError_DoesNotUpdatePersonPreviousNames()
    {
        // Arrange
        var ethelredDate = DateTime.Parse("1 Jan 1990").ToUniversalTime();
        var conanDate = DateTime.Parse("1 Jan 1991").ToUniversalTime();
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithPreviousNames(("Ethelred", "The", "Unready", ethelredDate), ("Conan", "The", "Barbarian", conanDate))
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfrede";
                s.MiddleName = "Thee";
                s.LastName = "Greate";
                s.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, expectedHeading: "Personal details have been updated");

        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons
                .Include(p => p.PreviousNames)
                .SingleAsync(p => p.PersonId == person.PersonId);

            Assert.Equal(TimeProvider.UtcNow, updatedPersonRecord.UpdatedOn);
            Assert.Equal("Alfrede", updatedPersonRecord.FirstName);
            Assert.Equal("Thee", updatedPersonRecord.MiddleName);
            Assert.Equal("Greate", updatedPersonRecord.LastName);

            Assert.Collection(updatedPersonRecord.PreviousNames!.OrderByDescending(pn => pn.CreatedOn),
                pn => Assert.Equal(("Conan", "The", "Barbarian", conanDate, conanDate), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)),
                pn => Assert.Equal(("Ethelred", "The", "Unready", ethelredDate, ethelredDate), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)));
        });
    }

    [Theory]
    [InlineData(PersonNameChangeReason.DeedPollOrOtherLegalProcess)]
    [InlineData(PersonNameChangeReason.MarriageOrCivilPartnership)]
    public async Task Post_Confirm_WhenAnyNameFieldChanged_AndNameChangeReasonIsFormalNameChange_UpdatesPersonPreviousNames(PersonNameChangeReason reason)
    {
        // Arrange
        var ethelredDate = DateTime.Parse("1 Jan 1990").ToUniversalTime();
        var conanDate = DateTime.Parse("1 Jan 1991").ToUniversalTime();
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithPreviousNames(("Ethelred", "The", "Unready", ethelredDate), ("Conan", "The", "Barbarian", conanDate))
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Megan";
                s.MiddleName = "Thee";
                s.LastName = "Stallion";
                s.NameChangeReason = reason;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, expectedHeading: "Personal details have been updated");

        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons
                .Include(p => p.PreviousNames)
                .SingleAsync(p => p.PersonId == person.PersonId);

            Assert.Equal(TimeProvider.UtcNow, updatedPersonRecord.UpdatedOn);
            Assert.Equal("Megan", updatedPersonRecord.FirstName);
            Assert.Equal("Thee", updatedPersonRecord.MiddleName);
            Assert.Equal("Stallion", updatedPersonRecord.LastName);

            Assert.Collection(updatedPersonRecord.PreviousNames!.OrderByDescending(pn => pn.CreatedOn),
                pn => Assert.Equal(("Alfred", "The", "Great", TimeProvider.UtcNow, TimeProvider.UtcNow), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)),
                pn => Assert.Equal(("Conan", "The", "Barbarian", conanDate, conanDate), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)),
                pn => Assert.Equal(("Ethelred", "The", "Unready", ethelredDate, ethelredDate), (pn.FirstName, pn.MiddleName, pn.LastName, pn.CreatedOn, pn.UpdatedOn)));
        });
    }

    private string GetRequestPath(TestData.CreatePersonResult person, EditDetailsJourneyCoordinator journeyInstance) =>
        $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

    public static TheoryData<string?, HttpMethod> UserDoesNotHavePermission_ReturnsForbiddenData =>
        new MatrixTheoryData<string?, HttpMethod>(
            [UserRoles.Viewer, null],
            TestHttpMethods.GetAndPost.SplitTestMethods().ToArray());

    [Theory]
    [MemberData(nameof(UserDoesNotHavePermission_ReturnsForbiddenData))]
    public async Task UserDoesNotHavePermission_ReturnsForbidden(string? role, HttpMethod httpMethod)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
            }));

        var request = new HttpRequestMessage(httpMethod,
            $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
            }));

        var request = new HttpRequestMessage(httpMethod,
            $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange, false, false, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange, false, true, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, true, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/other-details-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    public async Task Get_CheckAnswersWithUnansweredReason_RedirectsToReasonPage(PersonDetailsUpdatedEventChanges changes, bool hasNameChangeReason, bool hasOtherDetailsChangeReason, string expectedPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Other));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var state = CreateState(person, s =>
                    {
                        s.FirstName = firstName;
                        s.MiddleName = middleName;
                        s.LastName = lastName;
                        s.DateOfBirth = dateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(emailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber);
                        s.Gender = gender;
                    });

        if (hasNameChangeReason)
        {
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };
        }

        if (hasOtherDetailsChangeReason)
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Theory]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, "/edit-details/other-details-change-reason")]
    public async Task Get_BacklinkContainsExpected(PersonDetailsUpdatedEventChanges changes, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Male));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var state = CreateState(person, s =>
                    {
                        s.FirstName = firstName;
                        s.MiddleName = middleName;
                        s.LastName = lastName;
                        s.DateOfBirth = dateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(emailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber);
                    });

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.NameChange))
        {
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };
        }

        if (changes.HasAnyFlag(PersonDetailsUpdatedEventChanges.OtherThanNameChange))
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        Assert.Contains($"/persons/{person.PersonId}{expectedBackPage}", backlink.Href);
    }

    [Theory]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange, false, false, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange, false, true, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, true, "/edit-details/name-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.OtherThanNameChange, false, false, "/edit-details/other-details-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    [InlineData(PersonDetailsUpdatedEventChanges.NameChange | PersonDetailsUpdatedEventChanges.OtherThanNameChange, true, false, "/edit-details/other-details-change-reason")]
    public async Task Post_CheckAnswersWithUnansweredReason_RedirectsToReasonPage(PersonDetailsUpdatedEventChanges changes, bool hasNameChangeReason, bool hasOtherDetailsChangeReason, string expectedPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
            .WithEmailAddress("some@email-address.com")
            .WithNationalInsuranceNumber("AB123456D")
            .WithGender(Gender.Other));

        var firstName = changes.HasFlag(PersonDetailsUpdatedEventChanges.FirstName) ? "Megan" : person.FirstName;
        var middleName = changes.HasFlag(PersonDetailsUpdatedEventChanges.MiddleName) ? "Thee" : person.MiddleName;
        var lastName = changes.HasFlag(PersonDetailsUpdatedEventChanges.LastName) ? "Stallion" : person.LastName;
        var dateOfBirth = changes.HasFlag(PersonDetailsUpdatedEventChanges.DateOfBirth) ? DateOnly.Parse("2 Mar 1981") : person.DateOfBirth;
        var emailAddress = changes.HasFlag(PersonDetailsUpdatedEventChanges.EmailAddress) ? "new@email-address.com" : person.EmailAddress;
        var nationalInsuranceNumber = changes.HasFlag(PersonDetailsUpdatedEventChanges.NationalInsuranceNumber) ? "XY987654A" : person.NationalInsuranceNumber;
        var gender = changes.HasFlag(PersonDetailsUpdatedEventChanges.Gender) ? Gender.Female : person.Gender;

        var state = CreateState(person, s =>
                    {
                        s.FirstName = firstName;
                        s.MiddleName = middleName;
                        s.LastName = lastName;
                        s.DateOfBirth = dateOfBirth;
                        s.EmailAddress = EditDetailsFieldState<EmailAddress>.FromRawValue(emailAddress);
                        s.NationalInsuranceNumber = EditDetailsFieldState<NationalInsuranceNumber>.FromRawValue(nationalInsuranceNumber);
                        s.Gender = gender;
                    });

        var content = new EditDetailsPostRequestContentBuilder()
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmailAddress(emailAddress)
            .WithNationalInsuranceNumber(nationalInsuranceNumber)
            .WithGender(gender);

        if (hasNameChangeReason)
        {
            state.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
            state.NameChangeEvidence = new() { UploadEvidence = false };

            content = content
                .WithReason(PersonNameChangeReason.CorrectingAnError)
                .WithUploadEvidence(false);
        }

        if (hasOtherDetailsChangeReason)
        {
            state.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
            state.OtherDetailsChangeEvidence = new() { UploadEvidence = false };

            content = content
                .WithReason(PersonDetailsChangeReason.IncompleteDetails)
                .WithUploadEvidence(false);
        }

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = content.BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetailsPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var pageUrl = $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;
        Assert.NotNull(cancelButton);
        Assert.Equal("Cancel", cancelButton.Name);

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}", location);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_Cancel_EvidenceFilePreviouslyUploaded_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Alfred";
                s.MiddleName = "The";
                s.LastName = "Great";
                s.DateOfBirth = DateOnly.Parse("1 Feb 1980");
                s.NameChangeReason = PersonNameChangeReason.CorrectingAnError;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
                s.OtherDetailsChangeReason = PersonDetailsChangeReason.IncompleteDetails;
                s.OtherDetailsChangeEvidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = "evidence.jpg",
                        FileSizeDescription = "1.2 KB"
                    }
                };
            }));

        var pageUrl = $"/persons/{person.PersonId}/edit-details/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, pageUrl)
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }
}
