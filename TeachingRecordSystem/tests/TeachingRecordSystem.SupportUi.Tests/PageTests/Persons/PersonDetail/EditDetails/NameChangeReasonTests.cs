using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

[Collection(nameof(DisableParallelization))]
public class NameChangeReasonTests : TestBase
{
    public NameChangeReasonTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
        FileServiceMock.Invocations.Clear();
    }

    public override void Dispose()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);
        base.Dispose();
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
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var caption = doc.GetElementByTestId("edit-details-caption");
        Assert.Equal("Change name - Alfred The Great", caption!.TrimmedText());
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
                .WithName("Megan", "Thee", "Stallion")
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
        var reasonChoice = EditDetailsNameChangeReasonOption.MarriageOrCivilPartnership;
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
                .WithName("Megan", "Thee", "Stallion")
                .WithNameChangeReasonChoice(reasonChoice)
                .WithNameChangeUploadEvidenceChoice(true, evidenceFileId, "evidence.jpg", "1.2 KB")
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoiceSelection = GetChildElementsOfTestId<IHtmlInputElement>(doc, "change-reason-options", "input[type='radio']")
            .Single(i => i.IsChecked == true).Value;
        Assert.Equal(reasonChoice.ToString(), reasonChoiceSelection);

        var uploadEvidenceChoices = GetChildElementsOfTestId<IHtmlInputElement>(doc, "upload-evidence-options", "input[type='radio']")
            .Single(i => i.IsChecked == true).Value;
        Assert.Equal(true.ToString(), uploadEvidenceChoices);

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("evidence.jpg (1.2 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), GetHiddenInputValue(doc, nameof(NameChangeReasonModel.NameChangeEvidenceFileId)));
        Assert.Equal("evidence.jpg", GetHiddenInputValue(doc, nameof(NameChangeReasonModel.NameChangeEvidenceFileName)));
        Assert.Equal("1.2 KB", GetHiddenInputValue(doc, nameof(NameChangeReasonModel.NameChangeEvidenceFileSizeDescription)));
        Assert.Equal(expectedFileUrl, GetHiddenInputValue(doc, nameof(NameChangeReasonModel.NameChangeUploadedEvidenceFileUrl)));
    }

    [Fact]
    public async Task Get_ExpectedRadioButtonsExistOnPage()
    {
        // Arrange
        var expectedChoices = Enum.GetValues<EditDetailsNameChangeReasonOption>().Select(s => s.ToString());

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
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoicesLegend = doc.GetElementByTestId("change-reason-options-legend");
        Assert.Equal("Why are you changing the name on this record?", reasonChoicesLegend!.TrimmedText());
        var reasonChoices = doc.GetElementByTestId("change-reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Where(i => i.IsChecked == false)
            .Select(i => i.Value);
        Assert.Equal(expectedChoices, reasonChoices);

        var uploadEvidenceChoicesLegend = doc.GetElementByTestId("upload-evidence-options-legend");
        Assert.Equal("Do you want to upload evidence of this name change?", uploadEvidenceChoicesLegend!.TrimmedText());
        var uploadEvidenceChoices = doc.GetElementByTestId("upload-evidence-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Where(i => i.IsChecked == false)
            .Select(i => i.Value);
        Assert.Equal(new[] { "True", "False" }, uploadEvidenceChoices);
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
                .WithName("Megan", "Thee", "Stallion")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(NameChangeReasonModel.NameChangeReason), "Select a reason");
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(NameChangeReasonModel.NameChangeUploadEvidence), "Select yes if you want to upload evidence");
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
                .WithName("Megan", "Thee", "Stallion")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MultipartFormDataContentBuilder()
                .Add(nameof(NameChangeReasonModel.NameChangeReason), EditDetailsNameChangeReasonOption.CorrectingAnError)
                .Add(nameof(NameChangeReasonModel.NameChangeUploadEvidence), true)
                .Build()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(NameChangeReasonModel.NameChangeEvidenceFile), "Select a file");
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
                .WithName("Megan", "Thee", "Stallion")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MultipartFormDataContentBuilder()
                .Add(nameof(NameChangeReasonModel.NameChangeReason), EditDetailsNameChangeReasonOption.CorrectingAnError)
                .Add(nameof(NameChangeReasonModel.NameChangeUploadEvidence), true)
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFile), CreateEvidenceFileBinaryContent(), "invalidfile.cs")
                .Build()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(NameChangeReasonModel.NameChangeEvidenceFile), "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
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
                .WithName("Megan", "Thee", "Stallion")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MultipartFormDataContentBuilder()
                .Add(nameof(NameChangeReasonModel.NameChangeUploadEvidence), true)
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFile), CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png")
                .Build()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var html = await AssertEx.HtmlResponseAsync(response, 400);

        var evidenceFileId = await FileServiceMock.AssertFileWasUploadedAsync();
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(html.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("validfile.png (1.2 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), GetHiddenInputValue(html, nameof(NameChangeReasonModel.NameChangeEvidenceFileId)));
        Assert.Equal("validfile.png", GetHiddenInputValue(html, nameof(NameChangeReasonModel.NameChangeEvidenceFileName)));
        Assert.Equal("1.2 KB", GetHiddenInputValue(html, nameof(NameChangeReasonModel.NameChangeEvidenceFileSizeDescription)));
        Assert.Equal(expectedFileUrl, GetHiddenInputValue(html, nameof(NameChangeReasonModel.NameChangeUploadedEvidenceFileUrl)));
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
                .WithName("Megan", "Thee", "Stallion")
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MultipartFormDataContentBuilder()
                .Add(nameof(NameChangeReasonModel.NameChangeUploadEvidence), true)
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFileId), evidenceFileId)
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFileName), "testfile.jpg")
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFileSizeDescription), "3 KB")
                .Add(nameof(NameChangeReasonModel.NameChangeUploadedEvidenceFileUrl), "http://test.com/file")
                .Build()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        var html = await AssertEx.HtmlResponseAsync(response, 400);

        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(html.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("testfile.jpg (3 KB)", link.TrimmedText());
        Assert.Equal("http://test.com/file", link.Href);

        Assert.Equal(evidenceFileId.ToString(), GetHiddenInputValue(html, nameof(NameChangeReasonModel.NameChangeEvidenceFileId)));
        Assert.Equal("testfile.jpg", GetHiddenInputValue(html, nameof(NameChangeReasonModel.NameChangeEvidenceFileName)));
        Assert.Equal("3 KB", GetHiddenInputValue(html, nameof(NameChangeReasonModel.NameChangeEvidenceFileSizeDescription)));
        Assert.Equal("http://test.com/file", GetHiddenInputValue(html, nameof(NameChangeReasonModel.NameChangeUploadedEvidenceFileUrl)));
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
                .WithName("Megan", "Thee", "Stallion")
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MultipartFormDataContentBuilder()
                .Add(nameof(NameChangeReasonModel.NameChangeUploadEvidence), true)
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFileId), evidenceFileId)
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFileName), "testfile.jpg")
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFileSizeDescription), "3 KB")
                .Add(nameof(NameChangeReasonModel.NameChangeUploadedEvidenceFileUrl), "http://test.com/file")
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFile), CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png")
                .Build()
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
                .WithName("Megan", "Thee", "Stallion")
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MultipartFormDataContentBuilder()
                .Add(nameof(NameChangeReasonModel.NameChangeUploadEvidence), false)
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFileId), evidenceFileId)
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFileName), "testfile.jpg")
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFileSizeDescription), "3 KB")
                .Add(nameof(NameChangeReasonModel.NameChangeUploadedEvidenceFileUrl), "http://test.com/file")
                .Build()
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
                .WithName("Megan", "Thee", "Stallion")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MultipartFormDataContentBuilder()
                .Add(nameof(NameChangeReasonModel.NameChangeReason), EditDetailsNameChangeReasonOption.CorrectingAnError)
                .Add(nameof(NameChangeReasonModel.NameChangeUploadEvidence), true)
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFile), CreateEvidenceFileBinaryContent(new byte[1230]), "evidence.pdf")
                .Build()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.State.NameChangeUploadEvidence);
        Assert.Equal("evidence.pdf", journeyInstance.State.NameChangeEvidenceFileName);
        Assert.Equal("1.2 KB", journeyInstance.State.NameChangeEvidenceFileSizeDescription);
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
                .WithName("Megan", "Thee", "Stallion")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MultipartFormDataContentBuilder()
                .Add(nameof(NameChangeReasonModel.NameChangeReason), EditDetailsNameChangeReasonOption.CorrectingAnError)
                .Add(nameof(NameChangeReasonModel.NameChangeUploadEvidence), true)
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFile), CreateEvidenceFileBinaryContent(), "evidence.pdf")
                .Build()
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
                .WithName("Megan", "Thee", "Stallion")
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new MultipartFormDataContentBuilder()
                .Add(nameof(NameChangeReasonModel.NameChangeReason), EditDetailsNameChangeReasonOption.CorrectingAnError)
                .Add(nameof(NameChangeReasonModel.NameChangeUploadEvidence), false)
                .Add(nameof(NameChangeReasonModel.NameChangeEvidenceFile), CreateEvidenceFileBinaryContent(), "evidence.pdf")
                .Build()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        FileServiceMock.AssertFileWasNotUploaded();

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(EditDetailsNameChangeReasonOption.CorrectingAnError, journeyInstance.State.NameChangeReason);
        Assert.False(journeyInstance.State.NameChangeUploadEvidence);
        Assert.Null(journeyInstance.State.NameChangeEvidenceFileId);
        Assert.Null(journeyInstance.State.NameChangeEvidenceFileName);
        Assert.Null(journeyInstance.State.NameChangeEvidenceFileSizeDescription);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, JourneyInstance<EditDetailsState> journeyInstance) =>
        $"/persons/{person.PersonId}/edit-details/name-change-reason?{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<EditDetailsState>> CreateJourneyInstanceAsync(Guid personId, EditDetailsState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditDetails,
            state ?? new EditDetailsState(),
            new KeyValuePair<string, object>("personId", personId));
}
