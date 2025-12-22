using System.Text.Encodings.Web;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
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
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithReasonChoice(reasonChoice)
                .WithReasonDetailsChoice(true, reasonDetail)
                .WithFileUploadChoice(false)
                .Build());

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
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.InProgress, InductionJourneyPage.Status)
                .Build());

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
        Assert.Equal("Do you want to add more information about why you’re changing the induction details?", additionalDetailLegend!.TrimmedText());
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
        var changeReason = PersonInductionChangeReason.NewInformation;
        var changeReasonDetails = "A description about why the change typed into the box";
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(changeReason)
                .WithChangeReasonDetailSelections(true, changeReasonDetails)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(changeReason.GetDisplayName(), journeyInstance.State.ChangeReason!.GetDisplayName());
        Assert.Equal(changeReasonDetails, journeyInstance.State.ChangeReasonDetail);
    }

    [Fact]
    public async Task Post_NoChoicesAreEntered_ReturnsErrors()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

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
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasAdditionalReasonDetail", "Select yes if you want to add more information about why you’re changing the induction details");
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
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(PersonInductionChangeReason.AnotherReason)
                .WithChangeReasonDetailSelections(true, null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ChangeReasonDetail", "Enter additional detail");
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
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(changeReason)
                .WithChangeReasonDetailSelections(false)
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
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(PersonInductionChangeReason.AnotherReason)
                .WithChangeReasonDetailSelections(true, "")
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
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());
        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(PersonInductionChangeReason.AnotherReason)
                .WithChangeReasonDetailSelections(true, "")
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
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());
        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(PersonInductionChangeReason.AnotherReason)
                .WithChangeReasonDetailSelections(true, "")
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
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());
        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(PersonInductionChangeReason.AnotherReason)
                .WithChangeReasonDetailSelections(true, "")
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
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(changeReason)
                .WithChangeReasonDetailSelections(false)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), evidenceFileName))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.State.Evidence.UploadEvidence);
        Assert.Equal(evidenceFileName, journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName);
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
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(changeReason)
                .WithChangeReasonDetailSelections(true, changeReasonDetails)
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
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());
        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithChangeReason(PersonInductionChangeReason.NewInformation)
                .WithChangeReasonDetailSelections(false, "A description about why the change typed into the box")
                .WithUploadEvidence(false, (CreateEvidenceFileBinaryContent(), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        FileServiceMock.AssertFileWasNotUploaded();

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(PersonInductionChangeReason.NewInformation, journeyInstance.State.ChangeReason);
        Assert.False(journeyInstance.State.HasAdditionalReasonDetail);
        Assert.Null(journeyInstance.State.ChangeReasonDetail);
        Assert.False(journeyInstance.State.Evidence.UploadEvidence);
        Assert.Null(journeyInstance.State.Evidence.UploadedEvidenceFile);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<EditInductionState> journeyInstance) =>
        $"/persons/{person.PersonId}/edit-induction/reason?{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}
