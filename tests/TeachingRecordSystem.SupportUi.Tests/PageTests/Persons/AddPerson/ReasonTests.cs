using System.Text.Encodings.Web;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;
using ReasonModel = TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson.ReasonModel;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.AddPerson;

public class ReasonTests(HostFixture hostFixture) : AddPersonTestBase(hostFixture)
{
    [Fact]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999")
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
            b => Assert.Equal("Continue", b.TrimmedText()),
            b => Assert.Equal("Cancel", b.TrimmedText()));
    }

    [Fact]
    public async Task Get_WithPreviouslyStoredChoices_ShowsChoices()
    {
        // Arrange
        var reasonChoice = PersonCreateReason.AnotherReason;
        var reasonDetail = "A description about why the change typed into the box";
        var evidenceFileId = Guid.NewGuid();
        var urlEncoder = UrlEncoder.Default;
        var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
        var expectedFileUrl = $"http://localhost/files/evidence.jpg?fileUrl={expectedBlobStorageFileUrl}";
        var additionalInfo = "this is additional info";

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999"),
                Reason = reasonChoice,
                ReasonDetail = reasonDetail,
                ProvideAdditionalInformation = ProvideMoreInformationOption.Yes,
                AdditionalInformation = additionalInfo,
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

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoiceSelection = doc.GetChildElementsOfTestId<IHtmlInputElement>("create-reason-options", "input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(reasonChoice.ToString(), reasonChoiceSelection);

        var reasonDetailTextbox =
            doc.GetElementById("ReasonDetail") as IHtmlInputElement;
        Assert.Equal(reasonDetail, reasonDetailTextbox!.Value);

        var additionalInformation =
            doc.GetElementById("AdditionalInformation") as IHtmlTextAreaElement;
        Assert.Equal(additionalInfo, additionalInformation!.Value);

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

    [Fact]
    public async Task Get_ExpectedRadioButtonsExistOnPage()
    {
        // Arrange
        var expectedChoices = Enum.GetValues<PersonCreateReason>().Select(s => s.ToString());

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999")
            });

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

        var provideAdditionalInformationLegnd = doc.GetElementByTestId("create-provide-additional-information-legend");
        Assert.Equal("Do you want to provide more information?", provideAdditionalInformationLegnd!.TrimmedText());
        var provideAdditionalInformationChoices = doc.GetElementByTestId("provide-more-information-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Where(i => i.IsChecked == false)
            .Select(i => i.Value);
        Assert.Equal(["Yes", "No"], provideAdditionalInformationChoices);

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
        var changeReason = PersonCreateReason.AnotherReason;
        var changeReasonDetails = "A description about why the change typed into the box";

        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                ProvideAdditionalInformation = ProvideMoreInformationOption.Yes,
                AdditionalInformation = "Some more information",
                DateOfBirth = DateOnly.Parse("5 Jun 1999")
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(changeReason, changeReasonDetails)
                .WithUploadEvidence(false)
                .WithAdditionalInformation(ProvideMoreInformationOption.Yes, "Some more information")
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(changeReason.GetDisplayName(), journeyInstance.State.Reason!.GetDisplayName());
        Assert.Equal(changeReasonDetails, journeyInstance.State.ReasonDetail);
    }

    [Fact]
    public async Task Post_NoChoicesAreEntered_ReturnsErrors()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999")
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder().BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(ReasonModel.Reason), "Select a reason");
        await AssertEx.HtmlResponseHasErrorAsync(response, $"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadEvidence)}", "Select yes if you want to upload evidence");
    }

    [Fact]
    public async Task Post_AnotherReason_NoDetailAdded_ReturnsError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999")
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(PersonCreateReason.AnotherReason, null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(ReasonModel.ReasonDetail), "Enter a reason");
    }

    [Fact]
    public async Task Post_ProvideAdditionalInformationNotAnswered_ReturnsError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999")
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(PersonCreateReason.AnotherReason, null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(ReasonModel.ProvideAdditionalInformation), "Select yes if you want to add more information");
    }


    [Fact]
    public async Task Post_ProvideAdditionalInformationYes_AdditionalInformationNotProvided_ReturnsError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999"),
                ProvideAdditionalInformation = ProvideMoreInformationOption.No,
                AdditionalInformation = ""
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(PersonCreateReason.AnotherReason, null)
                .WithAdditionalInformation(ProvideMoreInformationOption.Yes, null)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(ReasonModel.AdditionalInformation), "Enter details");
    }

    [Fact]
    public async Task Post_FileUploadYes_NoFileUploaded_ReturnsError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999")
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(PersonCreateReason.MandatoryQualification)
                .WithUploadEvidence(true)
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, $"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.EvidenceFile)}", "Select a file");
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_ButEvidenceFileIsInvalidType_RendersError()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999")
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(PersonCreateReason.MandatoryQualification)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), "invalidfile.cs"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, $"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.EvidenceFile)}", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFileIsSelected_ButOtherFieldsInvalid_ShowsUploadedFile()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999")
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(PersonCreateReason.AnotherReason)
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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("validfile.png", doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_ButOtherFieldsInvalid_RemembersUploadedFile()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999")
            });

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(PersonCreateReason.AnotherReason)
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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("testfile.jpg", doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("3 KB", doc.GetHiddenInputValue($"{nameof(ReasonModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_AndNewFileUploaded_ButOtherFieldsInvalid_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999")
            });

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(PersonCreateReason.AnotherReason)
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
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999")
            });

        var evidenceFileId = Guid.NewGuid();

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(PersonCreateReason.AnotherReason)
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
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999"),
                ProvideAdditionalInformation = ProvideMoreInformationOption.Yes,
                AdditionalInformation = "Some more information"
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(PersonCreateReason.MandatoryQualification)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "evidence.pdf"))
                .WithAdditionalInformation(ProvideMoreInformationOption.Yes, "Some more information")
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        Assert.True(journeyInstance.State.Evidence.UploadEvidence);
        Assert.Equal("evidence.pdf", journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName);
        Assert.Equal("1.2 KB", journeyInstance.State.Evidence.UploadedEvidenceFile.FileSizeDescription);
    }

    [Fact]
    public async Task Post_SetValidFileUpload_CallsFileServiceUpload()
    {
        // Arrange
        var journeyInstance = await CreateJourneyInstanceAsync(
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999"),
                ProvideAdditionalInformation = ProvideMoreInformationOption.Yes,
                AdditionalInformation = "Some more information"
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(PersonCreateReason.MandatoryQualification)
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), "evidence.pdf"))
                .WithAdditionalInformation(ProvideMoreInformationOption.Yes, "Some more information")
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
            new AddPersonState
            {
                FirstName = "Alfred",
                MiddleName = "The",
                LastName = "Great",
                DateOfBirth = DateOnly.Parse("5 Jun 1999"),
                ProvideAdditionalInformation = ProvideMoreInformationOption.Yes,
                AdditionalInformation = "this is additional info that should be discarded"
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content = new AddPersonPostRequestContentBuilder()
                .WithReason(PersonCreateReason.MandatoryQualification, "A description about why the change typed into the box")
                .WithUploadEvidence(false, (CreateEvidenceFileBinaryContent(), "evidence.pdf"))
                .WithAdditionalInformation(ProvideMoreInformationOption.No)
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        FileServiceMock.AssertFileWasNotUploaded();

        Assert.Equal(PersonCreateReason.MandatoryQualification, journeyInstance.State.Reason);
        Assert.Null(journeyInstance.State.ReasonDetail);
        Assert.False(journeyInstance.State.Evidence.UploadEvidence);
        Assert.Null(journeyInstance.State.Evidence.UploadedEvidenceFile);
        Assert.Null(journeyInstance.State.AdditionalInformation);
    }

    private string GetRequestPath(AddPersonJourneyCoordinator journeyInstance) =>
        $"/persons/add/reason?{journeyInstance.GetUniqueIdQueryParameter()}";

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
    public async Task Get_BacklinkLinksToPersonalDetailsPage()
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
        Assert.Contains("/persons/add/personal-details", backlink.Href);
    }


    [Fact]
    public async Task Post_ValidRequest_RedirectsToCheckAnswersPage()
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

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(journeyInstance))
        {
            Content =
            new AddPersonPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmailAddress("some@email-address.com")
                .WithNationalInsuranceNumber("AB123456D")
                .WithReason(PersonCreateReason.MandatoryQualification)
                .WithUploadEvidence(false)
                .WithAdditionalInformation(ProvideMoreInformationOption.Yes, "Some more information")
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/persons/add/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
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


    [Fact]
    public async Task Get_WithReturnUrlToCheckAnswersPage_BacklinkLinksToCheckAnswersPage()
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

        var checkAnswersUrl = $"/persons/add/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/persons/add/reason?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        Assert.Contains("/persons/add/check-answers", backlink.Href);
    }

    [Fact]
    public async Task Post_WithReturnUrlToCheckAnswersPage_RedirectsToCheckAnswersPage()
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

        var checkAnswersUrl = $"/persons/add/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/add/reason?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content =
            new AddPersonPostRequestContentBuilder()
                .WithFirstName("Alfred")
                .WithMiddleName("The")
                .WithLastName("Great")
                .WithDateOfBirth(DateOnly.Parse("1 Feb 1980"))
                .WithEmailAddress("some@email-address.com")
                .WithNationalInsuranceNumber("AB123456D")
                .WithReason(PersonCreateReason.MandatoryQualification)
                .WithUploadEvidence(false)
                .WithAdditionalInformation(ProvideMoreInformationOption.Yes, "Some more information")
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(checkAnswersUrl, response.Headers.Location?.OriginalString);
    }
}
