using System.Text.Encodings.Web;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class ReasonTests(HostFixture hostFixture) : EditInductionTestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithPreviouslyStoredChoices_ShowsChoices()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var reasonChoice = PersonInductionChangeReason.NewInformation;
        var reasonDetail = "A description about why the change typed into the box";
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                ChangeReason = reasonChoice,
                ProvideAdditionalInformation = true,
                AdditionalInformation = reasonDetail,
                Evidence = CreateEvidence(false)
            });

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoiceSelection = doc.GetElementByTestId("reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(reasonChoice.ToString(), reasonChoiceSelection);

        var additionalDetailChoices = doc.GetElementByTestId("has-additional-reason_detail-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(true.ToString(), additionalDetailChoices);

        var uploadEvidenceChoices = doc.GetElementByTestId("upload-evidence-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(false.ToString(), uploadEvidenceChoices);

        var additionalDetailTextArea = doc.GetElementByTestId("additional-detail")!.GetElementsByTagName("textarea").Single() as IHtmlTextAreaElement;
        Assert.Equal(reasonDetail, additionalDetailTextArea!.Value);
    }

    [Fact]
    public async Task Get_ExpectedRadioButtonsExistOnPage()
    {
        // Arrange
        var expectedChoices = Enum.GetValues<PersonInductionChangeReason>().Select(s => s.ToString());

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = InductionStatus.InProgress,
                CurrentInductionStatus = InductionStatus.InProgress
            });

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoicesLegend = doc.GetElementByTestId("reason-options-legend");
        Assert.Equal("Why are you changing the induction details?", reasonChoicesLegend!.TrimmedText());
        var reasonChoices = doc.GetElementByTestId("reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Where(i => i.IsChecked == false)
            .Select(i => i.Value);
        Assert.Equal(expectedChoices, reasonChoices);

        var additionalDetailLegend = doc.GetElementByTestId("has-additional-reason_detail-options-legend");
        Assert.Equal("Do you want to provide more information?", additionalDetailLegend!.TrimmedText());
        var additionalDetailChoices = doc.GetElementByTestId("has-additional-reason_detail-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Where(i => i.IsChecked == false)
            .Select(i => i.Value);
        Assert.Equal(["True", "False"], additionalDetailChoices);

        var uploadEvidenceChoicesLegend = doc.GetElementByTestId("upload-evidence-options-legend");
        Assert.Equal("Do you want to upload evidence?", uploadEvidenceChoicesLegend!.TrimmedText());
        var uploadEvidenceChoices = doc.GetElementByTestId("upload-evidence-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Where(i => i.IsChecked == false)
            .Select(i => i.Value);
        Assert.Equal(["True", "False"], uploadEvidenceChoices);
    }

    [Fact]
    public async Task Post_SetValidChangeReasonDetails_PersistsDetails()
    {
        // Arrange
        var changeReason = PersonInductionChangeReason.AnotherReason;
        var changeReasonDetails = "A description about why the change typed into the box";
        var additionalInformation = "additional information on top of the box";
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(changeReason, changeReasonDetails)
                .WithProvideAdditionalInformation(true, additionalInformation)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        var state = GetJourneyInstanceState(journeyInstance)!;
        Assert.Equal(changeReason.GetDisplayName(), state.ChangeReason!.GetDisplayName());
        Assert.Equal(changeReasonDetails, state.ChangeReasonDetail);
        Assert.Equal(additionalInformation, state.AdditionalInformation);
    }

    [Fact]
    public async Task Post_NoChoicesAreEntered_ReturnsErrors()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "Evidence.UploadEvidence", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ChangeReason", "Select a reason");
        await AssertEx.HtmlResponseHasErrorAsync(response, "ProvideAdditionalInformation", "Select yes if you want to add more information about why you’re changing the induction details");
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Fact]
    public async Task Post_AdditionalDetailYes_NoDetailAdded_ReturnsError()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(PersonInductionChangeReason.AnotherReason, "some reason")
                .WithProvideAdditionalInformation(true, null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "AdditionalInformation", "Enter details");
    }

    [Fact]
    public async Task Post_FileUploadYes_NoFileUploaded_ReturnsError()
    {
        // Arrange
        var changeReason = PersonInductionChangeReason.NoLongerExempt;
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(changeReason)
                .WithProvideAdditionalInformation(false)
                .WithUploadEvidence(true)
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "Select a file");
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFileIsSelected_ButOtherFieldsInvalid_ShowsUploadedFile()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var inductionStatus = InductionStatus.InProgress;
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(PersonInductionChangeReason.AnotherReason, "some reason")
                .WithProvideAdditionalInformation(true, "")
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var doc = await AssertEx.HtmlResponseAsync(response, 400);

        var evidenceFileId = await FileServiceMock.AssertFileWasUploadedAsync();
        var urlEncoder = UrlEncoder.Default;
        var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
        var expectedFileUrl = $"http://localhost/files/validfile.png?fileUrl={expectedBlobStorageFileUrl}";

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("validfile.png (1.2 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue("Evidence.UploadedEvidenceFile.FileId"));
        Assert.Equal("validfile.png", doc.GetHiddenInputValue("Evidence.UploadedEvidenceFile.FileName"));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue("Evidence.UploadedEvidenceFile.FileSizeDescription"));
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_ButOtherFieldsInvalid_RemembersUploadedFile()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var inductionStatus = InductionStatus.InProgress;
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });
        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(PersonInductionChangeReason.AnotherReason, null)
                .WithProvideAdditionalInformation(true, null)
                .WithUploadEvidence(true, evidenceFileId, "testfile.jpg", "3 KB")
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var doc = await AssertEx.HtmlResponseAsync(response, 400);

        var urlEncoder = UrlEncoder.Default;
        var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
        var expectedFileUrl = $"http://localhost/files/testfile.jpg?fileUrl={expectedBlobStorageFileUrl}";

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("testfile.jpg (3 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue("Evidence.UploadedEvidenceFile.FileId"));
        Assert.Equal("testfile.jpg", doc.GetHiddenInputValue("Evidence.UploadedEvidenceFile.FileName"));
        Assert.Equal("3 KB", doc.GetHiddenInputValue("Evidence.UploadedEvidenceFile.FileSizeDescription"));
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_AndNewFileUploaded_ButOtherFieldsInvalid_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var inductionStatus = InductionStatus.InProgress;
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });
        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(PersonInductionChangeReason.AnotherReason)
                .WithProvideAdditionalInformation(true, "")
                .WithUploadEvidence(true, evidenceFileId, "testfile.jpg", "3 KB")
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToNo_ButEvidenceFilePreviouslyUploaded_AndOtherFieldsInvalid_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var inductionStatus = InductionStatus.InProgress;
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });
        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(PersonInductionChangeReason.AnotherReason, "reason details")
                .WithProvideAdditionalInformation(true, "")
                .WithUploadEvidence(false, evidenceFileId, "testfile.jpg", "3 KB")
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Fact]
    public async Task Post_SetValidFileUpload_PersistsDetails()
    {
        // Arrange
        var changeReason = PersonInductionChangeReason.NewInformation;
        var evidenceFileName = "evidence.pdf";
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(changeReason)
                .WithProvideAdditionalInformation(false)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), evidenceFileName))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        var state = GetJourneyInstanceState(journeyInstance)!;
        Assert.True(state.Evidence.UploadEvidence);
        Assert.Equal(evidenceFileName, state.Evidence.UploadedEvidenceFile!.FileName);
    }

    [Fact]
    public async Task Post_SetValidFileUpload_CallsFileServiceUpload()
    {
        // Arrange
        var changeReason = PersonInductionChangeReason.NewInformation;
        var changeReasonDetails = "A description about why the change typed into the box";
        var evidenceFileName = "evidence.pdf";
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(changeReason)
                .WithProvideAdditionalInformation(true, changeReasonDetails)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), evidenceFileName))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await FileServiceMock.AssertFileWasUploadedAsync();
    }

    [Fact]
    public async Task Post_ValidRequest_WithAdditionalInfo_ButAdditionalInfoRadioButtonsNotSetToYes_DiscardsAdditionalInfo()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var inductionStatus = InductionStatus.InProgress;
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });
        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(PersonInductionChangeReason.NewInformation)
                .WithProvideAdditionalInformation(false, "A description about why the change typed into the box")
                .WithUploadEvidence(false, (CreateEvidenceFileBinaryContent(), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        FileServiceMock.AssertFileWasNotUploaded();

        var state = GetJourneyInstanceState(journeyInstance)!;
        Assert.Equal(PersonInductionChangeReason.NewInformation, state.ChangeReason);
        Assert.False(state.ProvideAdditionalInformation);
        Assert.Null(state.ChangeReasonDetail);
        Assert.False(state.Evidence.UploadEvidence);
        Assert.Null(state.Evidence.UploadedEvidenceFile);
    }

    private string GetRequestPath(Person person, EditInductionJourneyCoordinator journeyInstance) =>
        $"/persons/{person.PersonId}/edit-induction/reason?{journeyInstance.GetUniqueIdQueryParameter()}";

    [Theory]
    [InlineData(InductionStatus.Exempt, "edit-induction/check-answers")]
    [InlineData(InductionStatus.InProgress, "edit-induction/check-answers")]
    [InlineData(InductionStatus.Failed, "edit-induction/check-answers")]
    [InlineData(InductionStatus.FailedInWales, "edit-induction/check-answers")]
    [InlineData(InductionStatus.Passed, "edit-induction/check-answers")]
    [InlineData(InductionStatus.RequiredToComplete, "edit-induction/check-answers")]
    public async Task Post_RedirectsToExpectedPage(InductionStatus inductionStatus, string expectedNextPageUrl)
    {
        // Arrange
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(i => i.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2)
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(inductionStatus)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(TimeProvider.Today.AddDays(-1))
                .WithCompletedDate(TimeProvider.Today)
                .WithChangeReason(PersonInductionChangeReason.IncompleteDetails)
                .WithProvideAdditionalInformation(false)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/persons/{person.PersonId}/{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToInduction()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2),
                CompletedDate = TimeProvider.Today,
                ChangeReason = PersonInductionChangeReason.AnotherReason,
                ProvideAdditionalInformation = true,
                AdditionalInformation = "Details",
                Evidence = CreateEvidence(false)
            });

        // Act
        // Cancelling is a field on the form rather than a handler of its own: a distinct URL would be
        // an invalid step for the journey.
        var response = await HttpClient.SendAsync(new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/{person.PersonId}/edit-induction/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        });

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/induction", response.Headers.Location?.OriginalString);
        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_Cancel_EvidenceFilePreviouslyUploaded_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var evidenceFileId = Guid.NewGuid();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2),
                CompletedDate = TimeProvider.Today,
                ChangeReason = PersonInductionChangeReason.AnotherReason,
                ProvideAdditionalInformation = true,
                AdditionalInformation = "Details",
                Evidence = CreateEvidence(true, evidenceFileId)
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/{person.PersonId}/edit-induction/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        });

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    [InlineData(StartPage.Status, InductionStatus.Exempt, "edit-induction/exemption-reasons")]
    [InlineData(StartPage.StartDate, InductionStatus.InProgress, "edit-induction/start-date")]
    [InlineData(StartPage.StartDate, InductionStatus.Failed, "edit-induction/date-completed")]
    [InlineData(StartPage.StartDate, InductionStatus.FailedInWales, "edit-induction/date-completed")]
    [InlineData(StartPage.StartDate, InductionStatus.Passed, "edit-induction/date-completed")]
    [InlineData(StartPage.Status, InductionStatus.RequiredToComplete, "edit-induction/status")]
    public async Task Get_BackLinkContainsExpected(StartPage startPage, InductionStatus inductionStatus, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2),
                CompletedDate = TimeProvider.Today
            },
            startPage);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/{expectedBackPage}", backlink!.Href);
    }

    [Fact]
    public async Task Get_FromCheckAnswers_BackLinkReturnsToCheckAnswers()
    {
        // Arrange
        var inductionStatus = InductionStatus.Passed;
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                ExemptionReasonIds = exemptionReasonIds,
                StartDate = new DateOnly(2000, 2, 2),
                CompletedDate = new DateOnly(2002, 2, 2),
                ChangeReason = PersonInductionChangeReason.AnotherReason
            });

        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/persons/{person.PersonId}/edit-induction/reason?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/edit-induction/check-answers", backlink!.Href);
    }

    [Theory]
    [InlineData(InductionStatus.Passed, "edit-induction/check-answers")]
    public async Task Post_FromCheckAnswers_RedirectsToExpectedPage(InductionStatus inductionStatus, string expectedNextPageUrl)
    {
        // Arrange
        var startDate = new DateOnly(2000, 2, 1);
        var completedDate = new DateOnly(2002, 2, 2);
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };

        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(i => i.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                ExemptionReasonIds = exemptionReasonIds,
                StartDate = startDate,
                CompletedDate = completedDate,
                ChangeReason = PersonInductionChangeReason.AnotherReason
            });

        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/{person.PersonId}/edit-induction/reason?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(inductionStatus)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithChangeReason(PersonInductionChangeReason.IncompleteDetails)
                .WithProvideAdditionalInformation(false)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/persons/{person.PersonId}/{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }
}
