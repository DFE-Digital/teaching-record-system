using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.Create;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Create;

[Collection(nameof(DisableParallelization))]
public class CreateReasonTests : TestBase
{
    public CreateReasonTests(HostFixture hostFixture) : base(hostFixture)
    {
        FileServiceMock.Invocations.Clear();
    }

    [Fact]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
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

    [Fact]
    public async Task Get_WithPreviouslyStoredChoices_ShowsChoices()
    {
        // Arrange
        var reasonChoice = CreateReasonOption.AnotherReason;
        var reasonDetail = "A description about why the change typed into the box";
        var evidenceFileId = Guid.NewGuid();
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";

        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .WithCreateReasonChoice(reasonChoice, reasonDetail)
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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileId)));
        Assert.Equal("evidence.jpg", doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileName)));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileSizeDescription)));
        Assert.Equal(expectedFileUrl, doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileUrl)));
    }

    [Fact]
    public async Task Get_ExpectedRadioButtonsExistOnPage()
    {
        // Arrange
        var expectedChoices = Enum.GetValues<CreateReasonOption>().Select(s => s.ToString());

        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
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

    [Fact]
    public async Task Post_SetValidChangeReasonDetails_PersistsDetails()
    {
        // Arrange
        var changeReason = CreateReasonOption.AnotherReason;
        var changeReasonDetails = "A description about why the change typed into the box";

        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithCreateReason(changeReason, changeReasonDetails)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(changeReason.GetDisplayName(), journeyInstance.State.CreateReason!.GetDisplayName());
        Assert.Equal(changeReasonDetails, journeyInstance.State.CreateReasonDetail);
    }

    [Fact]
    public async Task Post_NoChoicesAreEntered_ReturnsErrors()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(CreateReasonModel.CreateReason), "Select a reason");
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(CreateReasonModel.UploadEvidence), "Select yes if you want to upload evidence");
    }

    [Fact]
    public async Task Post_AnotherReason_NoDetailAdded_ReturnsError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithCreateReason(CreateReasonOption.AnotherReason, null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(CreateReasonModel.CreateReasonDetail), "Enter a reason");
    }

    [Fact]
    public async Task Post_FileUploadYes_NoFileUploaded_ReturnsError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithCreateReason(CreateReasonOption.MandatoryQualification)
                .WithUploadEvidence(true)
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(CreateReasonModel.EvidenceFile), "Select a file");
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_ButEvidenceFileIsInvalidType_RendersError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithCreateReason(CreateReasonOption.MandatoryQualification)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), "invalidfile.cs"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(CreateReasonModel.EvidenceFile), "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFileIsSelected_ButOtherFieldsInvalid_ShowsUploadedFile()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithCreateReason(CreateReasonOption.AnotherReason)
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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileId)));
        Assert.Equal("validfile.png", doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileName)));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileSizeDescription)));
        Assert.Equal(expectedFileUrl, doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileUrl)));
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_ButOtherFieldsInvalid_RemembersUploadedFile()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithCreateReason(CreateReasonOption.AnotherReason)
                .WithUploadEvidence(true, evidenceFileId, "testfile.jpg", "3 KB", "http://test.com/file")
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
        Assert.Equal("http://test.com/file", link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileId)));
        Assert.Equal("testfile.jpg", doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileName)));
        Assert.Equal("3 KB", doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileSizeDescription)));
        Assert.Equal("http://test.com/file", doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileUrl)));
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_AndNewFileUploaded_ButOtherFieldsInvalid_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithCreateReason(CreateReasonOption.AnotherReason)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png"))
                .WithUploadEvidence(true, evidenceFileId, "testfile.jpg", "3 KB", "http://test.com/file")
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
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithCreateReason(CreateReasonOption.AnotherReason)
                .WithUploadEvidence(false, evidenceFileId, "testfile.jpg", "3 KB", "http://test.com/file")
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
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithCreateReason(CreateReasonOption.MandatoryQualification)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.State.UploadEvidence);
        Assert.Equal("evidence.pdf", journeyInstance.State.EvidenceFileName);
        Assert.Equal("1.2 KB", journeyInstance.State.EvidenceFileSizeDescription);
    }

    [Fact]
    public async Task Post_SetValidFileUpload_CallsFileServiceUpload()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithCreateReason(CreateReasonOption.MandatoryQualification)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        await FileServiceMock.AssertFileWasUploadedAsync();
    }

    [Fact]
    public async Task Post_ValidRequest_WithAdditionalInfo_ButAdditionalInfoRadioButtonsNotSetToYes_DiscardsAdditionalInfo()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var journeyInstance = await CreateJourneyInstanceAsync(
            new CreateStateBuilder()
                .WithInitializedState()
                .WithName("Alfred", "The", "Great")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new CreatePostRequestContentBuilder()
                .WithCreateReason(CreateReasonOption.MandatoryQualification, "A description about why the change typed into the box")
                .WithUploadEvidence(false, (CreateEvidenceFileBinaryContent(), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        FileServiceMock.AssertFileWasNotUploaded();

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(CreateReasonOption.MandatoryQualification, journeyInstance.State.CreateReason);
        Assert.Null(journeyInstance.State.CreateReasonDetail);
        Assert.False(journeyInstance.State.UploadEvidence);
        Assert.Null(journeyInstance.State.EvidenceFileId);
        Assert.Null(journeyInstance.State.EvidenceFileName);
        Assert.Null(journeyInstance.State.EvidenceFileSizeDescription);
    }

    private string GetRequestPath(JourneyInstance<CreateState> journeyInstance) =>
        $"/persons/create/create-reason?{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<CreateState>> CreateJourneyInstanceAsync(CreateState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.CreatePerson,
            state ?? new CreateState());
}
