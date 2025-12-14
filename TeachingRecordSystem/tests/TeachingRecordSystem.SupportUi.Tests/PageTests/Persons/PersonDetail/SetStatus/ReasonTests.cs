using System.Text.Encodings.Web;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.SetStatus;

public class ReasonTests(HostFixture hostFixture) : SetStatusTestBase(hostFixture)
{
    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Get_WithPreviouslyStoredChoices_ShowsChoices(PersonStatus targetStatus)
    {
        // Arrange
        var reasonDetail = "A description about why the change typed into the box";
        var evidenceFileId = Guid.NewGuid();
        var urlEncoder = UrlEncoder.Default;
        var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
        var expectedFileUrl = $"http://localhost/files/evidence.jpg?fileUrl={expectedBlobStorageFileUrl}";

        var person = await CreatePersonToBecomeStatus(targetStatus);

        var stateBuilder = new SetStatusStateBuilder()
            .WithInitializedState()
            .WithUploadEvidenceChoice(true, evidenceFileId, "evidence.jpg", "1.2 KB");

        if (targetStatus == PersonStatus.Deactivated)
        {
            stateBuilder.WithDeactivateReasonChoice(DeactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, reasonDetail);
        }
        else
        {
            stateBuilder.WithReactivateReasonChoice(ReactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, reasonDetail);
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
            var reasonChoiceSelection = doc.GetChildElementsOfTestId<IHtmlInputElement>("deactivate-reason-options", "input[type='radio']")
                .Single(i => i.IsChecked).Value;
            Assert.Equal(DeactivateReasonOption.AnotherReason.ToString(), reasonChoiceSelection);

            var additionalDetailTextArea = doc.GetElementByTestId("deactivate-reason-detail")!.GetElementsByTagName("textarea").Single() as IHtmlTextAreaElement;
            Assert.Equal(reasonDetail, additionalDetailTextArea!.Value);
        }
        else
        {
            var reasonChoiceSelection = doc.GetChildElementsOfTestId<IHtmlInputElement>("reactivate-reason-options", "input[type='radio']")
                .Single(i => i.IsChecked).Value;
            Assert.Equal(DeactivateReasonOption.AnotherReason.ToString(), reasonChoiceSelection);

            var additionalDetailTextArea = doc.GetElementByTestId("reactivate-reason-detail")!.GetElementsByTagName("textarea").Single() as IHtmlTextAreaElement;
            Assert.Equal(reasonDetail, additionalDetailTextArea!.Value);
        }

        var uploadEvidenceChoices = doc.GetChildElementsOfTestId<IHtmlInputElement>("upload-evidence-options", "input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(true.ToString(), uploadEvidenceChoices);

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("evidence.jpg (1.2 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("evidence.jpg", doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Get_ExpectedRadioButtonsExistOnPage(PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(person, targetStatus, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        if (targetStatus == PersonStatus.Deactivated)
        {
            var expectedChoices = Enum.GetValues<DeactivateReasonOption>().Select(s => s.ToString());
            var reasonChoicesLegend = doc.GetElementByTestId("deactivate-reason-options-legend");
            Assert.Equal("Why are you deactivating this record?", reasonChoicesLegend!.TrimmedText());
            var reasonChoices = doc.GetElementByTestId("deactivate-reason-options")!
                .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
                .Where(i => i.IsChecked == false)
                .Select(i => i.Value);
            Assert.Equal(expectedChoices, reasonChoices);
        }
        else
        {
            var expectedChoices = Enum.GetValues<ReactivateReasonOption>().Select(s => s.ToString());
            var reasonChoicesLegend = doc.GetElementByTestId("reactivate-reason-options-legend");
            Assert.Equal("Why are you reactivating this record?", reasonChoicesLegend!.TrimmedText());
            var reasonChoices = doc.GetElementByTestId("reactivate-reason-options")!
                .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
                .Where(i => i.IsChecked == false)
                .Select(i => i.Value);
            Assert.Equal(expectedChoices, reasonChoices);
        }

        var uploadEvidenceChoicesLegend = doc.GetElementByTestId("upload-evidence-options-legend");
        Assert.Equal("Do you want to upload evidence?", uploadEvidenceChoicesLegend!.TrimmedText());
        var uploadEvidenceChoices = doc.GetElementByTestId("upload-evidence-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Where(i => i.IsChecked == false)
            .Select(i => i.Value);
        Assert.Equal(["True", "False"], uploadEvidenceChoices);
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_SetValidReasonDetails_PersistsDetails(PersonStatus targetStatus)
    {
        // Arrange
        var changeReasonDetails = "A description about why the change typed into the box";

        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var contentBuilder = new SetStatusPostRequestContentBuilder()
            .WithUploadEvidence(false);

        if (targetStatus == PersonStatus.Deactivated)
        {
            contentBuilder.WithDeactivateReason(DeactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, changeReasonDetails);
        }
        else
        {
            contentBuilder.WithReactivateReason(ReactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, changeReasonDetails);
        }

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance))
        {
            Content = contentBuilder.BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);

        if (targetStatus == PersonStatus.Deactivated)
        {
            Assert.Equal(DeactivateReasonOption.AnotherReason.GetDisplayName(), journeyInstance.State.DeactivateReason!.GetDisplayName());
            Assert.Equal(changeReasonDetails, journeyInstance.State.DeactivateReasonDetail);
        }
        else
        {
            Assert.Equal(ReactivateReasonOption.AnotherReason.GetDisplayName(), journeyInstance.State.ReactivateReason!.GetDisplayName());
            Assert.Equal(changeReasonDetails, journeyInstance.State.ReactivateReasonDetail);
        }
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_NoChoicesAreEntered_ReturnsErrors(PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance))
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "Evidence.UploadEvidence", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        if (targetStatus == PersonStatus.Deactivated)
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(ReasonModel.DeactivateReason), "Select a reason");
        }
        else
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(ReasonModel.ReactivateReason), "Select a reason");
        }
        await AssertEx.HtmlResponseHasErrorAsync(response, $"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadEvidence)}", "Select yes if you want to upload evidence");
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_ProvideAdditionalInformation_NoDetailAdded_ReturnsError(PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var contentBuilder = new SetStatusPostRequestContentBuilder()
            .WithUploadEvidence(true);

        if (targetStatus == PersonStatus.Deactivated)
        {
            contentBuilder.WithDeactivateReason(DeactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, detail: null);
        }
        else
        {
            contentBuilder.WithReactivateReason(ReactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, detail: null);
        }

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance))
        {
            Content = contentBuilder.BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        if (targetStatus == PersonStatus.Deactivated)
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(ReasonModel.DeactivateReasonDetail), "Enter a reason");
        }
        else
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, nameof(ReasonModel.ReactivateReasonDetail), "Enter a reason");
        }
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_FileUploadYes_NoFileUploaded_ReturnsError(PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var contentBuilder = new SetStatusPostRequestContentBuilder()
            .WithUploadEvidence(true);

        if (targetStatus == PersonStatus.Deactivated)
        {
            contentBuilder.WithDeactivateReason(DeactivateReasonOption.ProblemWithTheRecord, ProvideMoreInformationOption.No);
        }
        else
        {
            contentBuilder.WithReactivateReason(ReactivateReasonOption.DeactivatedByMistake, ProvideMoreInformationOption.No);
        }

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance))
        {
            Content = contentBuilder.BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, $"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.EvidenceFile)}", "Select a file");
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_UploadEvidenceSetToYes_ButEvidenceFileIsInvalidType_RendersError(PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var contentBuilder = new SetStatusPostRequestContentBuilder()
            .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), "invalidfile.cs"));

        if (targetStatus == PersonStatus.Deactivated)
        {
            contentBuilder.WithDeactivateReason(DeactivateReasonOption.ProblemWithTheRecord, ProvideMoreInformationOption.No);
        }
        else
        {
            contentBuilder.WithReactivateReason(ReactivateReasonOption.DeactivatedByMistake, ProvideMoreInformationOption.No);
        }

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance))
        {
            Content = contentBuilder.BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, $"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.EvidenceFile)}", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFileIsSelected_ButOtherFieldsInvalid_ShowsUploadedFile(PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var contentBuilder = new SetStatusPostRequestContentBuilder()
            .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png"));

        if (targetStatus == PersonStatus.Deactivated)
        {
            contentBuilder.WithDeactivateReason(DeactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, detail: null);
        }
        else
        {
            contentBuilder.WithReactivateReason(ReactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, detail: null);
        }

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance))
        {
            Content = contentBuilder.BuildMultipartFormData()
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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("validfile.png", doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_ButOtherFieldsInvalid_RemembersUploadedFile(PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var contentBuilder = new SetStatusPostRequestContentBuilder()
            .WithUploadEvidence(true, evidenceFileId, "testfile.jpg", "3 KB");

        if (targetStatus == PersonStatus.Deactivated)
        {
            contentBuilder.WithDeactivateReason(DeactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, detail: null);
        }
        else
        {
            contentBuilder.WithReactivateReason(ReactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, detail: null);
        }

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance))
        {
            Content = contentBuilder.BuildMultipartFormData()
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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("testfile.jpg", doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("3 KB", doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_AndNewFileUploaded_ButOtherFieldsInvalid_DeletesPreviouslyUploadedFile(PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var contentBuilder = new SetStatusPostRequestContentBuilder()
            .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "validfile.png"))
            .WithUploadEvidence(true, evidenceFileId, "testfile.jpg", "3 KB");

        if (targetStatus == PersonStatus.Deactivated)
        {
            contentBuilder.WithDeactivateReason(DeactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, detail: null);
        }
        else
        {
            contentBuilder.WithReactivateReason(ReactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, detail: null);
        }

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance))
        {
            Content = contentBuilder.BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_UploadEvidenceSetToNo_ButEvidenceFilePreviouslyUploaded_AndOtherFieldsInvalid_DeletesPreviouslyUploadedFile(PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var evidenceFileId = Guid.NewGuid();

        var contentBuilder = new SetStatusPostRequestContentBuilder()
            .WithUploadEvidence(false, evidenceFileId, "testfile.jpg", "3 KB");

        if (targetStatus == PersonStatus.Deactivated)
        {
            contentBuilder.WithDeactivateReason(DeactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, detail: null);
        }
        else
        {
            contentBuilder.WithReactivateReason(ReactivateReasonOption.AnotherReason, ProvideMoreInformationOption.Yes, detail: null);
        }

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance))
        {
            Content = contentBuilder.BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_SetValidFileUpload_PersistsDetails(PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var contentBuilder = new SetStatusPostRequestContentBuilder()
            .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "evidence.pdf"));

        if (targetStatus == PersonStatus.Deactivated)
        {
            contentBuilder.WithDeactivateReason(DeactivateReasonOption.RecordHolderDied, ProvideMoreInformationOption.No);
        }
        else
        {
            contentBuilder.WithReactivateReason(ReactivateReasonOption.DeactivatedByMistake, ProvideMoreInformationOption.No);
        }

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance))
        {
            Content = contentBuilder.BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.State.Evidence.UploadEvidence);
        Assert.Equal("evidence.pdf", journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName);
        Assert.Equal("1.2 KB", journeyInstance.State.Evidence.UploadedEvidenceFile!.FileSizeDescription);
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_SetValidFileUpload_CallsFileServiceUpload(PersonStatus targetStatus)
    {
        // Arrange
        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var contentBuilder = new SetStatusPostRequestContentBuilder()
            .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), "evidence.pdf"));

        if (targetStatus == PersonStatus.Deactivated)
        {
            contentBuilder.WithDeactivateReason(DeactivateReasonOption.RecordHolderDied, ProvideMoreInformationOption.No);
        }
        else
        {
            contentBuilder.WithReactivateReason(ReactivateReasonOption.DeactivatedByMistake, ProvideMoreInformationOption.No);
        }

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance))
        {
            Content = contentBuilder.BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        await FileServiceMock.AssertFileWasUploadedAsync();
    }

    [Theory]
    [MemberData(nameof(GetAllStatuses))]
    public async Task Post_ValidRequest_WithAdditionalInfo_ButAdditionalInfoRadioButtonsNotSetToYes_DiscardsAdditionalInfo(PersonStatus targetStatus)
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await CreatePersonToBecomeStatus(targetStatus);

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new SetStatusStateBuilder()
                .WithInitializedState()
                .Build());

        var contentBuilder = new SetStatusPostRequestContentBuilder()
            .WithUploadEvidence(false, (CreateEvidenceFileBinaryContent(), "unneccessary-evidence.pdf"));

        if (targetStatus == PersonStatus.Deactivated)
        {
            contentBuilder.WithDeactivateReason(DeactivateReasonOption.RecordHolderDied, ProvideMoreInformationOption.No, "Unneccessary detail");
        }
        else
        {
            contentBuilder.WithReactivateReason(ReactivateReasonOption.DeactivatedByMistake, ProvideMoreInformationOption.No, "Unneccessary detail");
        }

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(person, targetStatus, journeyInstance))
        {
            Content = contentBuilder.BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        FileServiceMock.AssertFileWasNotUploaded();

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        if (targetStatus == PersonStatus.Deactivated)
        {
            Assert.Equal(DeactivateReasonOption.RecordHolderDied, journeyInstance.State.DeactivateReason);
            Assert.Null(journeyInstance.State.DeactivateReasonDetail);
        }
        else
        {
            Assert.Equal(ReactivateReasonOption.DeactivatedByMistake, journeyInstance.State.ReactivateReason);
            Assert.Null(journeyInstance.State.ReactivateReasonDetail);
        }
        Assert.False(journeyInstance.State.Evidence.UploadEvidence);
        Assert.Null(journeyInstance.State.Evidence.UploadedEvidenceFile);
    }

    private string GetRequestPath(TestData.CreatePersonResult person, PersonStatus targetStatus, JourneyInstance<SetStatusState> journeyInstance) =>
        $"/persons/{person.PersonId}/set-status/{targetStatus}/reason?{journeyInstance.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<SetStatusState>> CreateJourneyInstanceAsync(Guid personId, SetStatusState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.SetStatus,
            state ?? new SetStatusState(),
            new KeyValuePair<string, object>("personId", personId));
}
