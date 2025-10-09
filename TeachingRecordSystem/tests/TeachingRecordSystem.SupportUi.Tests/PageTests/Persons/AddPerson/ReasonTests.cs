using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.AddPerson;

[NotInParallel]
public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal("Continue", b.TrimmedText()),
            b => Assert.Equal("Cancel", b.TrimmedText()));
    }

    [Test]
    public async Task Get_WithPreviouslyStoredChoices_ShowsChoices()
    {
        // Arrange
        var reasonChoice = AddPersonReasonOption.AnotherReason;
        var reasonDetail = "A description about why the change typed into the box";
        var evidenceFileId = Guid.NewGuid();
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .WithAddPersonReasonChoice(reasonChoice, reasonDetail)
                .WithUploadEvidenceChoice(true, evidenceFileId, "evidence.jpg", "1.2 KB")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoiceSelection = doc.GetChildElementsOfTestId<IHtmlInputElement>("create-reason-options", "input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(reasonChoice.ToString(), reasonChoiceSelection);

        var additionalDetailTextArea = doc.GetElementByTestId("create-reason-detail")!.GetElementsByTagName("textarea").Single() as IHtmlTextAreaElement;
        Assert.Equal(reasonDetail, additionalDetailTextArea!.Value);

        var uploadEvidenceChoices = doc.GetChildElementsOfTestId<IHtmlInputElement>("upload-evidence-options", "input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(true.ToString(), uploadEvidenceChoices);

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("evidence.jpg (1.2 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("evidence.jpg", doc.GetHiddenInputValue($"{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue($"{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
    }

    [Test]
    public async Task Get_ExpectedRadioButtonsExistOnPage()
    {
        // Arrange
        var expectedChoices = Enum.GetValues<AddPersonReasonOption>().Select(s => s.ToString());

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoicesLegend = doc.GetElementByTestId("create-reason-options-legend");
        Assert.Equal("Why are you creating this record?", reasonChoicesLegend!.TrimmedText());
        var reasonChoices = doc.GetElementByTestId("create-reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Where(i => i.IsChecked == false)
            .Select(i => i.Value);
        Assert.Equal(expectedChoices, reasonChoices);

        var uploadEvidenceChoicesLegend = doc.GetElementByTestId("upload-evidence-options-legend");
        Assert.Equal("Do you want to upload evidence?", uploadEvidenceChoicesLegend!.TrimmedText());
        var uploadEvidenceChoices = doc.GetElementByTestId("upload-evidence-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Where(i => i.IsChecked == false)
            .Select(i => i.Value);
        Assert.Equal(["True", "False"], uploadEvidenceChoices);
    }

    [Test]
    public async Task Post_SetValidChangeReasonDetails_PersistsDetails()
    {
        // Arrange
        var changeReason = AddPersonReasonOption.AnotherReason;
        var changeReasonDetails = "A description about why the change typed into the box";

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(changeReason, changeReasonDetails)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(changeReason.GetDisplayName(), journeyInstance.State.Reason!.GetDisplayName());
        Assert.Equal(changeReasonDetails, journeyInstance.State.ReasonDetail);
    }

    [Test]
    public async Task Post_NoChoicesAreEntered_ReturnsErrors()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(ReasonModel.Reason), "Select a reason");
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EvidenceUploadModel.UploadEvidence), "Select yes if you want to upload evidence");
    }

    [Test]
    public async Task Post_AnotherReason_NoDetailAdded_ReturnsError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(AddPersonReasonOption.AnotherReason, null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(ReasonModel.ReasonDetail), "Enter a reason");
    }

    [Test]
    public async Task Post_FileUploadYes_NoFileUploaded_ReturnsError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(AddPersonReasonOption.MandatoryQualification)
                .WithUploadEvidence(true)
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EvidenceUploadModel.EvidenceFile), "Select a file");
    }

    [Test]
    public async Task Post_UploadEvidenceSetToYes_ButEvidenceFileIsInvalidType_RendersError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(AddPersonReasonOption.MandatoryQualification)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), "invalidfile.cs"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(EvidenceUploadModel.EvidenceFile), "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Test]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFileIsSelected_ButOtherFieldsInvalid_ShowsUploadedFile()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(AddPersonReasonOption.AnotherReason)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var doc = await AssertEx.HtmlResponseAsync(response, 400);

        var evidenceFileId = await FileServiceMock.AssertFileWasUploadedAsync();
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("validfile.png (1.2 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("validfile.png", doc.GetHiddenInputValue($"{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue($"{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
    }

    [Test]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_ButOtherFieldsInvalid_RemembersUploadedFile()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(AddPersonReasonOption.AnotherReason)
                .WithUploadEvidence(true, evidenceFileId, "testfile.jpg", "3 KB")
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var doc = await AssertEx.HtmlResponseAsync(response, 400);

        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("testfile.jpg (3 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("testfile.jpg", doc.GetHiddenInputValue($"{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("3 KB", doc.GetHiddenInputValue($"{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
    }

    [Test]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_AndNewFileUploaded_ButOtherFieldsInvalid_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(AddPersonReasonOption.AnotherReason)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png"))
                .WithUploadEvidence(true, evidenceFileId, "testfile.jpg", "3 KB")
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Test]
    public async Task Post_UploadEvidenceSetToNo_ButEvidenceFilePreviouslyUploaded_AndOtherFieldsInvalid_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(AddPersonReasonOption.AnotherReason)
                .WithUploadEvidence(false, evidenceFileId, "testfile.jpg", "3 KB")
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Test]
    public async Task Post_SetValidFileUpload_PersistsDetails()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(AddPersonReasonOption.MandatoryQualification)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.State.Evidence.UploadEvidence);
        Assert.Equal("evidence.pdf", journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName);
        Assert.Equal("1.2 KB", journeyInstance.State.Evidence.UploadedEvidenceFile.FileSizeDescription);
    }

    [Test]
    public async Task Post_SetValidFileUpload_CallsFileServiceUpload()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(AddPersonReasonOption.MandatoryQualification)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        await FileServiceMock.AssertFileWasUploadedAsync();
    }

    [Test]
    public async Task Post_ValidRequest_WithAdditionalInfo_ButAdditionalInfoRadioButtonsNotSetToYes_DiscardsAdditionalInfo()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(AddPersonReasonOption.MandatoryQualification, "A description about why the change typed into the box")
                .WithUploadEvidence(false, (CreateEvidenceFileBinaryContent(), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        FileServiceMock.AssertFileWasNotUploaded();

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(AddPersonReasonOption.MandatoryQualification, journeyInstance.State.Reason);
        Assert.Null(journeyInstance.State.ReasonDetail);
        Assert.False(journeyInstance.State.Evidence.UploadEvidence);
        Assert.Null(journeyInstance.State.Evidence.UploadedEvidenceFile);
    }

    private string GetRequestPath(JourneyInstance<AddPersonState> journeyInstance) =>
        $"/persons/add/reason?{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<AddPersonState>> CreateJourneyInstanceAsync(AddPersonState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.AddPerson,
            state ?? new AddPersonState());
}
