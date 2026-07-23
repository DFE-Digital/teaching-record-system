using System.Text.Encodings.Web;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditDetails;

public class OtherDetailsChangeReasonTests(HostFixture hostFixture) : EditDetailsTestBase(hostFixture)
{
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
            CreateState(person, s =>
            {
                s.FirstName = "A";
                s.MiddleName = "New";
                s.LastName = "Name";
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
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
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
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
            b => Assert.Equal("Continue", b.TrimmedText()),
            b => Assert.Equal("Cancel and return to record", b.TrimmedText()));
    }

    [Fact]
    public async Task Get_WithPreviouslyStoredChoices_ShowsChoices()
    {
        // Arrange
        var reasonChoice = PersonDetailsChangeReason.AnotherReason;
        var reasonDetail = "A description about why the change typed into the box";
        var evidenceFileId = Guid.NewGuid();
        var urlEncoder = UrlEncoder.Default;
        var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
        var expectedFileUrl = $"http://localhost/files/evidence.jpg?fileUrl={expectedBlobStorageFileUrl}";

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
                s.OtherDetailsChangeReason = reasonChoice;
                s.OtherDetailsChangeReasonDetail = reasonDetail;
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

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoiceSelection = doc.GetChildElementsOfTestId<IHtmlInputElement>("change-reason-options", "input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(reasonChoice.ToString(), reasonChoiceSelection);

        var additionalDetailTextArea = doc.GetElementByTestId("change-reason-detail")!.GetElementsByTagName("textarea").Single() as IHtmlTextAreaElement;
        Assert.Equal(reasonDetail, additionalDetailTextArea!.Value);

        var uploadEvidenceChoices = doc.GetChildElementsOfTestId<IHtmlInputElement>("upload-evidence-options", "input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(true.ToString(), uploadEvidenceChoices);

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("evidence.jpg (1.2 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(OtherDetailsChangeReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("evidence.jpg", doc.GetHiddenInputValue($"{nameof(OtherDetailsChangeReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue($"{nameof(OtherDetailsChangeReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
    }

    [Fact]
    public async Task Get_ExpectedRadioButtonsExistOnPage()
    {
        // Arrange
        var expectedChoices = Enum.GetValues<PersonDetailsChangeReason>().Select(s => s.ToString());

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

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
        Assert.Equal(["True", "False"], uploadEvidenceChoices);
    }

    [Fact]
    public async Task Get_WhenNameAlsoChanged_PageTitleChangesAccordingly()
    {
        // Arrange
        var expectedChoices = Enum.GetValues<PersonDetailsChangeReason>().Select(s => s.ToString());

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.FirstName = "Megan";
                s.MiddleName = "Thee";
                s.LastName = "Stallion";
                s.NameChangeReason = PersonNameChangeReason.MarriageOrCivilPartnership;
                s.NameChangeEvidence = new()
                {
                    UploadEvidence = false
                };
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

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
        var changeReason = PersonDetailsChangeReason.AnotherReason;
        var changeReasonDetails = "A description about why the change typed into the box";

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithReason(changeReason, changeReasonDetails)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
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
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(OtherDetailsChangeReasonModel.Reason), "Select a reason");
        await AssertEx.HtmlResponseHasErrorAsync(response, $"{nameof(OtherDetailsChangeReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadEvidence)}", "Select yes if you want to upload evidence");
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
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithReason(PersonDetailsChangeReason.AnotherReason, null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(OtherDetailsChangeReasonModel.ReasonDetail), "Enter a reason");
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
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithReason(PersonDetailsChangeReason.NewInformation)
                .WithUploadEvidence(true)
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, $"{nameof(OtherDetailsChangeReasonModel.Evidence)}.{nameof(EvidenceUploadModel.EvidenceFile)}", "Select a file");
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
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithReason(PersonDetailsChangeReason.NewInformation)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), "invalidfile.cs"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, $"{nameof(OtherDetailsChangeReasonModel.Evidence)}.{nameof(EvidenceUploadModel.EvidenceFile)}", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
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
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithReason(PersonDetailsChangeReason.AnotherReason)
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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(OtherDetailsChangeReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("validfile.png", doc.GetHiddenInputValue($"{nameof(OtherDetailsChangeReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue($"{nameof(OtherDetailsChangeReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
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
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithReason(PersonDetailsChangeReason.AnotherReason)
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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(OtherDetailsChangeReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("testfile.jpg", doc.GetHiddenInputValue($"{nameof(OtherDetailsChangeReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("3 KB", doc.GetHiddenInputValue($"{nameof(OtherDetailsChangeReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
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
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithReason(PersonDetailsChangeReason.AnotherReason)
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
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithReason(PersonDetailsChangeReason.AnotherReason)
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
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithReason(PersonDetailsChangeReason.NewInformation)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        Assert.True(journeyInstance.State.OtherDetailsChangeEvidence.UploadEvidence);
        Assert.Equal("evidence.pdf", journeyInstance.State.OtherDetailsChangeEvidence.UploadedEvidenceFile!.FileName);
        Assert.Equal("1.2 KB", journeyInstance.State.OtherDetailsChangeEvidence.UploadedEvidenceFile.FileSizeDescription);
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
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithReason(PersonDetailsChangeReason.NewInformation)
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

        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great")
            .WithDateOfBirth(DateOnly.Parse("1 Feb 1980")));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            CreateState(person, s =>
            {
                s.DateOfBirth = DateOnly.Parse("5 Jun 1999");
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, journeyInstance))
        {
            Content = new EditDetailsPostRequestContentBuilder()
                .WithReason(PersonDetailsChangeReason.NewInformation, "A description about why the change typed into the box")
                .WithUploadEvidence(false, (CreateEvidenceFileBinaryContent(), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        FileServiceMock.AssertFileWasNotUploaded();

        Assert.Equal(PersonDetailsChangeReason.NewInformation, journeyInstance.State.OtherDetailsChangeReason);
        Assert.Null(journeyInstance.State.OtherDetailsChangeReasonDetail);
        Assert.False(journeyInstance.State.OtherDetailsChangeEvidence.UploadEvidence);
        Assert.Null(journeyInstance.State.OtherDetailsChangeEvidence.UploadedEvidenceFile);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, EditDetailsJourneyCoordinator journeyInstance) =>
        $"/persons/{person.PersonId}/edit-details/other-details-change-reason?{journeyInstance.GetUniqueIdQueryParameter()}";

}
