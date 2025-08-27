using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

[Collection(nameof(DisableParallelization))]
public class OtherDetailsChangeReasonTests : TestBase
{
    public OtherDetailsChangeReasonTests(HostFixture hostFixture) : base(hostFixture)
    {
        FileServiceMock.Invocations.Clear();
    }

    [Fact]
    public async Task Get_PageLegend_PopulatedFromOriginalPersonName()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("A", "New", "Name")
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.CorrectingAnError)
                .WithNameChangeUploadEvidenceChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var caption = doc.GetElementByTestId("change-reason-caption");
        Assert.Equal("Change personal details - Alfred The Great", caption!.TrimmedText());
    }

    [Fact]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal("Continue", b.TrimmedText()),
            b => Assert.Equal("Cancel and return to record", b.TrimmedText()));
    }

    [Fact]
    public async Task Get_WithPreviouslyStoredChoices_ShowsChoices()
    {
        // Arrange
        var reasonChoice = EditDetailsOtherDetailsChangeReasonOption.AnotherReason;
        var reasonDetail = "A description about why the change typed into the box";
        var evidenceFileId = Guid.NewGuid();
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .WithOtherDetailsChangeReasonChoice(reasonChoice, reasonDetail)
                .WithOtherDetailsChangeUploadEvidenceChoice(true, evidenceFileId, "evidence.jpg", "1.2 KB")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoiceSelection = doc.GetChildElementsOfTestId<IHtmlInputElement>("change-reason-options", "input[type='radio']")
            .Single(i => i.IsChecked == true).Value;
        Assert.Equal(reasonChoice.ToString(), reasonChoiceSelection);

        var additionalDetailTextArea = doc.GetElementByTestId("change-reason-detail")!.GetElementsByTagName("textarea").Single() as IHtmlTextAreaElement;
        Assert.Equal(reasonDetail, additionalDetailTextArea!.Value);

        var uploadEvidenceChoices = doc.GetChildElementsOfTestId<IHtmlInputElement>("upload-evidence-options", "input[type='radio']")
            .Single(i => i.IsChecked == true).Value;
        Assert.Equal(true.ToString(), uploadEvidenceChoices);

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("evidence.jpg (1.2 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue(nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeEvidenceFileId)));
        Assert.Equal("evidence.jpg", doc.GetHiddenInputValue(nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeEvidenceFileName)));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue(nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeEvidenceFileSizeDescription)));
        Assert.Equal(expectedFileUrl, doc.GetHiddenInputValue(nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeUploadedEvidenceFileUrl)));
    }

    [Fact]
    public async Task Get_ExpectedRadioButtonsExistOnPage()
    {
        // Arrange
        var expectedChoices = Enum.GetValues<EditDetailsOtherDetailsChangeReasonOption>().Select(s => s.ToString());

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoicesLegend = doc.GetElementByTestId("change-reason-options-legend");
        Assert.Equal("Why are you changing this record?", reasonChoicesLegend!.TrimmedText());
        var reasonChoices = doc.GetElementByTestId("change-reason-options")!
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
        Assert.Equal(new[] { "True", "False" }, uploadEvidenceChoices);
    }

    [Fact]
    public async Task Get_WhenNameAlsoChanged_PageTitleChangesAccordingly()
    {
        // Arrange
        var expectedChoices = Enum.GetValues<EditDetailsOtherDetailsChangeReasonOption>().Select(s => s.ToString());

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithName("Megan", "Thee", "Stallion")
                .WithNameChangeReasonChoice(EditDetailsNameChangeReasonOption.MarriageOrCivilPartnership)
                .WithNameChangeUploadEvidenceChoice(false)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoicesLegend = doc.GetElementByTestId("change-reason-options-legend");
        Assert.Equal("Why are you changing the other personal details on this record?", reasonChoicesLegend!.TrimmedText());
    }

    [Fact]
    public async Task Post_SetValidChangeReasonDetails_PersistsDetails()
    {
        // Arrange
        var changeReason = EditDetailsOtherDetailsChangeReasonOption.AnotherReason;
        var changeReasonDetails = "A description about why the change typed into the box";

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithOtherDetailsChangeReason(changeReason, changeReasonDetails)
                .WithOtherDetailsChangeEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(changeReason.GetDisplayName(), journeyInstance.State.OtherDetailsChangeReason!.GetDisplayName());
        Assert.Equal(changeReasonDetails, journeyInstance.State.OtherDetailsChangeReasonDetail);
    }

    [Fact]
    public async Task Post_NoChoicesAreEntered_ReturnsErrors()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeReason), "Select a reason");
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeUploadEvidence), "Select yes if you want to upload evidence");
    }

    [Fact]
    public async Task Post_AnotherReason_NoDetailAdded_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption.AnotherReason, null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeReasonDetail), "Enter a reason");
    }

    [Fact]
    public async Task Post_FileUploadYes_NoFileUploaded_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption.NewInformation)
                .WithOtherDetailsChangeEvidence(true)
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeEvidenceFile), "Select a file");
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_ButEvidenceFileIsInvalidType_RendersError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption.NewInformation)
                .WithOtherDetailsChangeEvidence(true, (CreateEvidenceFileBinaryContent(), "invalidfile.cs"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeEvidenceFile), "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFileIsSelected_ButOtherFieldsInvalid_ShowsUploadedFile()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption.AnotherReason)
                .WithOtherDetailsChangeEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png"))
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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue(nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeEvidenceFileId)));
        Assert.Equal("validfile.png", doc.GetHiddenInputValue(nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeEvidenceFileName)));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue(nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeEvidenceFileSizeDescription)));
        Assert.Equal(expectedFileUrl, doc.GetHiddenInputValue(nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeUploadedEvidenceFileUrl)));
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_ButOtherFieldsInvalid_RemembersUploadedFile()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption.AnotherReason)
                .WithOtherDetailsChangeEvidence(true, evidenceFileId, "testfile.jpg", "3 KB", "http://test.com/file")
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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue(nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeEvidenceFileId)));
        Assert.Equal("testfile.jpg", doc.GetHiddenInputValue(nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeEvidenceFileName)));
        Assert.Equal("3 KB", doc.GetHiddenInputValue(nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeEvidenceFileSizeDescription)));
        Assert.Equal("http://test.com/file", doc.GetHiddenInputValue(nameof(OtherDetailsChangeReasonModel.OtherDetailsChangeUploadedEvidenceFileUrl)));
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_AndNewFileUploaded_ButOtherFieldsInvalid_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption.AnotherReason)
                .WithOtherDetailsChangeEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png"))
                .WithOtherDetailsChangeEvidence(true, evidenceFileId, "testfile.jpg", "3 KB", "http://test.com/file")
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
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption.AnotherReason)
                .WithOtherDetailsChangeEvidence(false, evidenceFileId, "testfile.jpg", "3 KB", "http://test.com/file")
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
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption.NewInformation)
                .WithOtherDetailsChangeEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.State.OtherDetailsChangeUploadEvidence);
        Assert.Equal("evidence.pdf", journeyInstance.State.OtherDetailsChangeEvidenceFileName);
        Assert.Equal("1.2 KB", journeyInstance.State.OtherDetailsChangeEvidenceFileSizeDescription);
    }

    [Fact]
    public async Task Post_SetValidFileUpload_CallsFileServiceUpload()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption.NewInformation)
                .WithOtherDetailsChangeEvidence(true, (CreateEvidenceFileBinaryContent(), "evidence.pdf"))
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

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditDetailsStateBuilder()
                .WithInitializedState(person)
                .WithDateOfBirth(DateOnly.Parse("5 Jun 1999"))
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithOtherDetailsChangeReason(EditDetailsOtherDetailsChangeReasonOption.NewInformation, "A description about why the change typed into the box")
                .WithOtherDetailsChangeEvidence(false, (CreateEvidenceFileBinaryContent(), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        FileServiceMock.AssertFileWasNotUploaded();

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(EditDetailsOtherDetailsChangeReasonOption.NewInformation, journeyInstance.State.OtherDetailsChangeReason);
        Assert.Null(journeyInstance.State.OtherDetailsChangeReasonDetail);
        Assert.False(journeyInstance.State.OtherDetailsChangeUploadEvidence);
        Assert.Null(journeyInstance.State.OtherDetailsChangeEvidenceFileId);
        Assert.Null(journeyInstance.State.OtherDetailsChangeEvidenceFileName);
        Assert.Null(journeyInstance.State.OtherDetailsChangeEvidenceFileSizeDescription);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<EditDetailsState> journeyInstance) =>
        $"/persons/{person.PersonId}/edit-details/other-details-change-reason?{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<EditDetailsState>> CreateJourneyInstanceAsync(Guid personId, EditDetailsState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditDetails,
            state ?? new EditDetailsState(),
            new KeyValuePair<string, object>("personId", personId));
}
