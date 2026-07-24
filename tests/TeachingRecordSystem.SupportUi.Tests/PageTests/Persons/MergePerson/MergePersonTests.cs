using System.Text.Encodings.Web;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.MergePerson;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.MergePerson;

public class MergePersonTests(HostFixture hostFixture) : MergePersonTestBase(hostFixture)
{
    [Fact]
    public async Task Get_ExpectedRadioButtonsExistOnPage()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithAllDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var uploadEvidenceChoicesLegend = doc.GetElementByTestId("upload-evidence-options-legend");
        Assert.Equal("Do you want to upload evidence?", uploadEvidenceChoicesLegend!.TrimmedText());
        var uploadEvidenceChoices = doc.GetElementByTestId("upload-evidence-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Where(i => i.IsChecked == false)
            .Select(i => i.Value);
        Assert.Equal(["True", "False"], uploadEvidenceChoices);
    }

    [Theory]
    [MemberData(nameof(GetAttributesAndFieldsData))]
    public async Task Get_AttributeIsNotDifferent_RendersDisabledAndUnselectedRadioButtons(
        PersonMatchedAttribute _,
        string fieldName,
        bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(useNullValues: useNullValues);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radios = doc.GetElementsByName(fieldName);

        foreach (var radio in radios)
        {
            Assert.True(radio.IsDisabled());
            Assert.False(radio.IsChecked());
        }
    }

    [Theory]
    [MemberData(nameof(GetAttributesAndFieldsData))]
    public async Task Get_AttributeIsDifferent_RendersRadioButtonsWithExistingValueHighlighted(
        PersonMatchedAttribute attribute,
        string fieldName,
        bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(attribute, useNullValues: useNullValues);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radios = doc.GetElementsByName(fieldName);

        Assert.Collection(
            radios,
            fromPrimaryPersonRadio =>
            {
                Assert.False(fromPrimaryPersonRadio.IsDisabled());
            },
            fromSecondaryPersonRadio =>
            {
                Assert.False(fromSecondaryPersonRadio.IsDisabled());
                Assert.NotEmpty(
                    fromSecondaryPersonRadio.GetAncestor<IHtmlDivElement>()!.GetElementsByClassName("hods-highlight"));
            });
    }

    [Theory]
    [MemberData(nameof(GetAttributesAndFieldsData))]
    public async Task Get_AttributeSourceSetToPrimaryPersonInState_RendersSelectedSourceRadioButton(
        PersonMatchedAttribute _,
        string fieldName,
        bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithAllDifferences(useNullValues: useNullValues);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.FirstNameSource = PersonAttributeSource.PrimaryPerson;
                s.MiddleNameSource = PersonAttributeSource.PrimaryPerson;
                s.LastNameSource = PersonAttributeSource.PrimaryPerson;
                s.DateOfBirthSource = PersonAttributeSource.PrimaryPerson;
                s.EmailAddressSource = PersonAttributeSource.PrimaryPerson;
                s.NationalInsuranceNumberSource = PersonAttributeSource.PrimaryPerson;
                s.GenderSource = PersonAttributeSource.PrimaryPerson;
                s.Comments = null;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radios = doc.GetElementsByName(fieldName);
        Assert.True(radios[0].IsChecked());
    }

    [Theory]
    [MemberData(nameof(GetAttributesAndFieldsData))]
    public async Task Get_AttributeSourceSetToSecondaryPersonInState_RendersSelectedSourceRadioButton(
        PersonMatchedAttribute _,
        string fieldName,
        bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithAllDifferences(useNullValues: useNullValues);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.FirstNameSource = PersonAttributeSource.SecondaryPerson;
                s.MiddleNameSource = PersonAttributeSource.SecondaryPerson;
                s.LastNameSource = PersonAttributeSource.SecondaryPerson;
                s.DateOfBirthSource = PersonAttributeSource.SecondaryPerson;
                s.EmailAddressSource = PersonAttributeSource.SecondaryPerson;
                s.NationalInsuranceNumberSource = PersonAttributeSource.SecondaryPerson;
                s.GenderSource = PersonAttributeSource.SecondaryPerson;
                s.Comments = null;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radios = doc.GetElementsByName(fieldName);
        Assert.True(radios[1].IsChecked());
    }

    [Fact]
    public async Task Get_EvidenceAndCommentsSetInState_RendersChoices()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();
        var urlEncoder = UrlEncoder.Default;
        var expectedBlobStorageFileUrl = urlEncoder.Encode($"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}");
        var expectedFileUrl = $"http://localhost/files/evidence.jpg?fileUrl={expectedBlobStorageFileUrl}";
        var comments = "Some comments";

        var (personA, personB) = await CreatePersonsWithAllDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.Evidence = new()
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = evidenceFileId,
                        FileName = "evidence.jpg",
                        FileSizeDescription = "1.2 KB"
                    }
                };
                s.Comments = comments;
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var uploadEvidenceChoices = doc.GetChildElementsOfTestId<IHtmlInputElement>("upload-evidence-options", "input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(true.ToString(), uploadEvidenceChoices);

        var link = Assert.IsAssignableFrom<IHtmlAnchorElement>(doc.GetElementByTestId("uploaded-evidence-file-link"));
        Assert.Equal("evidence.jpg (1.2 KB)", link.TrimmedText());
        Assert.Equal(expectedFileUrl, link.Href);

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(MergeModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("evidence.jpg", doc.GetHiddenInputValue($"{nameof(MergeModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue($"{nameof(MergeModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));

        Assert.Equal(comments, doc.GetElementsByName("Comments").Single().TrimmedText());
    }

    [Theory]
    [InlineData(PersonMatchedAttribute.FirstName, "FirstNameSource", "Select a first name")]
    [InlineData(PersonMatchedAttribute.MiddleName, "MiddleNameSource", "Select a middle name")]
    [InlineData(PersonMatchedAttribute.LastName, "LastNameSource", "Select a last name")]
    [InlineData(PersonMatchedAttribute.DateOfBirth, "DateOfBirthSource", "Select a date of birth")]
    [InlineData(PersonMatchedAttribute.EmailAddress, "EmailAddressSource", "Select an email")]
    [InlineData(PersonMatchedAttribute.NationalInsuranceNumber, "NationalInsuranceNumberSource", "Select a National Insurance number")]
    [InlineData(PersonMatchedAttribute.Gender, "GenderSource", "Select a gender")]
    public async Task Post_AttributeSourceNotSelected_RendersError(
        PersonMatchedAttribute differentAttribute,
        string fieldName,
        string expectedErrorMessage)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(differentAttribute);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, fieldName, expectedErrorMessage);
    }

    [Fact]
    public async Task Post_FileUploadYes_NoFileUploaded_ReturnsError()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithUploadEvidence(true)
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, $"{nameof(MergeModel.Evidence)}.{nameof(EvidenceUploadModel.EvidenceFile)}", "Select a file");
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_ButEvidenceFileIsInvalidType_RendersError()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(), "invalidfile.cs"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, $"{nameof(MergeModel.Evidence)}.{nameof(EvidenceUploadModel.EvidenceFile)}", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFileIsSelected_ButOtherFieldsInvalid_ShowsUploadedFile()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(PersonMatchedAttribute.FirstName);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(MergeModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("validfile.png", doc.GetHiddenInputValue($"{nameof(MergeModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue($"{nameof(MergeModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_ButOtherFieldsInvalid_RemembersUploadedFile()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(PersonMatchedAttribute.FirstName);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue($"{nameof(MergeModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileId)}"));
        Assert.Equal("testfile.jpg", doc.GetHiddenInputValue($"{nameof(MergeModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileName)}"));
        Assert.Equal("3 KB", doc.GetHiddenInputValue($"{nameof(MergeModel.Evidence)}.{nameof(EvidenceUploadModel.UploadedEvidenceFile)}.{nameof(UploadedEvidenceFile.FileSizeDescription)}"));
    }

    [Fact]
    public async Task Post_UploadEvidenceSetToYes_AndEvidenceFilePreviouslyUploaded_AndNewFileUploaded_ButOtherFieldsInvalid_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(PersonMatchedAttribute.FirstName);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
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
        var evidenceFileId = Guid.NewGuid();

        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(PersonMatchedAttribute.FirstName);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
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
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithUploadEvidence(true, (CreateEvidenceFileBinaryContent(new byte[1230]), "evidence.pdf"))
                .BuildMultipartFormData()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        Assert.True(journeyInstance.State.Evidence.UploadEvidence);
        Assert.Equal("evidence.pdf", journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName);
        Assert.Equal("1.2 KB", journeyInstance.State.Evidence.UploadedEvidenceFile!.FileSizeDescription);
    }

    [Fact]
    public async Task Post_SetValidFileUpload_CallsFileServiceUpload()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
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
    public async Task Post_EmptyRequestWithNoDifferencesToSelect_Succeeds()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.True((int)response.StatusCode < 400);
    }

    [Fact]
    public async Task Post_ValidRequest_UpdatesStateAndRedirectsToCheckAnswers()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (personA, personB) = await CreatePersonsWithAllDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.FirstNameSource = PersonAttributeSource.SecondaryPerson;
                s.MiddleNameSource = PersonAttributeSource.SecondaryPerson;
                s.LastNameSource = PersonAttributeSource.SecondaryPerson;
                s.DateOfBirthSource = PersonAttributeSource.SecondaryPerson;
                s.EmailAddressSource = PersonAttributeSource.SecondaryPerson;
                s.NationalInsuranceNumberSource = PersonAttributeSource.SecondaryPerson;
                s.GenderSource = PersonAttributeSource.SecondaryPerson;
                s.Comments = null;
            }));

        var firstNameSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var middleNameSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var lastNameSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var dateOfBirthSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var emailAddressSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var nationalInsuranceNumberSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var genderSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithFirstNameSource(firstNameSelection)
                .WithMiddleNameSource(middleNameSelection)
                .WithLastNameSource(lastNameSelection)
                .WithDateOfBirthSource(dateOfBirthSelection)
                .WithEmailAddressSource(emailAddressSelection)
                .WithNationalInsuranceNumberSource(nationalInsuranceNumberSelection)
                .WithGenderSource(genderSelection)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AssertEx.ResponseIsRedirectTo(response,
            $"/persons/{personA.PersonId}/merge/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        Assert.Equal(firstNameSelection, journeyInstance.State.FirstNameSource);
        Assert.Equal(middleNameSelection, journeyInstance.State.MiddleNameSource);
        Assert.Equal(lastNameSelection, journeyInstance.State.LastNameSource);
        Assert.Equal(dateOfBirthSelection, journeyInstance.State.DateOfBirthSource);
        Assert.Equal(emailAddressSelection, journeyInstance.State.EmailAddressSource);
        Assert.Equal(nationalInsuranceNumberSelection, journeyInstance.State.NationalInsuranceNumberSource);
        Assert.Equal(genderSelection, journeyInstance.State.GenderSource);
    }

    public static (PersonMatchedAttribute Attribute, string FieldName, bool UseNullValues)[] GetAttributesAndFieldsData() =>
    [
        (PersonMatchedAttribute.FirstName, "FirstNameSource", false),
        (PersonMatchedAttribute.MiddleName, "MiddleNameSource", false),
        (PersonMatchedAttribute.LastName, "LastNameSource", false),
        (PersonMatchedAttribute.DateOfBirth, "DateOfBirthSource", false),
        (PersonMatchedAttribute.EmailAddress, "EmailAddressSource", false),
        (PersonMatchedAttribute.EmailAddress, "EmailAddressSource", true),
        (PersonMatchedAttribute.NationalInsuranceNumber, "NationalInsuranceNumberSource", false),
        (PersonMatchedAttribute.NationalInsuranceNumber, "NationalInsuranceNumberSource", true),
        (PersonMatchedAttribute.Gender, "GenderSource", false),
        (PersonMatchedAttribute.Gender, "GenderSource", true)
    ];

    private string GetRequestPath(CreatePersonResult person, MergePersonJourneyCoordinator? journeyInstance = null) =>
        $"/persons/{person.PersonId}/merge/merge?{journeyInstance?.GetUniqueIdQueryParameter()}";

    [Theory]
    [InlineData("matches")]
    public async Task Get_BacklinkLinksToExpected(string? expectedPage)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        var expectedBackLink = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedBackLink += "/merge/" + expectedPage;
        }
        Assert.Contains(expectedBackLink, backlink.Href);
    }

    [Theory]
    [InlineData("check-answers")]
    public async Task Get_WithReturnUrlToCheckAnswersPage_BacklinkLinksToExpected(string? expectedPage)
    {
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var checkAnswersUrl = $"/persons/{personA.PersonId}/merge/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"{GetRequestPath(personA, journeyInstance)}&returnUrl={Uri.EscapeDataString(checkAnswersUrl)}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.NotNull(backlink);
        var expectedBackLink = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedBackLink += "/merge/" + expectedPage;
        }
        Assert.Contains(expectedBackLink, backlink.Href);
    }

    [Theory]
    [InlineData("Continue", "Cancel and return to record")]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage(string continueButtonText, string cancelButtonText)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").OfType<IHtmlButtonElement>();
        Assert.Collection(buttons,
            b => Assert.Equal(continueButtonText, b.TrimmedText()),
            b => Assert.Equal(cancelButtonText, b.TrimmedText()));
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetailPage()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var pageUrl = GetRequestPath(personA, journeyInstance);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, pageUrl);
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
        AssertEx.ResponseIsRedirectTo(redirectResponse, $"/persons/{personA.PersonId}");

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_PersonAIsDeactivated_ReturnsBadRequest()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personA.Person);
            personA.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_PersonAHasOpenAlert_ReturnsBadRequest()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(p => p
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    public async Task Post_PersonAHasInvalidInductionStatus_ReturnsBadRequest(InductionStatus status)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(p => p
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_PersonBIsDeactivated_ReturnsBadRequest()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(personB.Person);
            personB.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_PersonBHasOpenAlert_ReturnsBadRequest()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(configurePersonB: p => p
            .WithAlert(a => a.WithEndDate(null)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    public async Task Post_PersonBHasInvalidInductionStatus_ReturnsBadRequest(InductionStatus status)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(configurePersonB: p => p
            .WithInductionStatus(i => i
                .WithStatus(status)
                .WithStartDate(new DateOnly(2024, 1, 1))
                .WithCompletedDate(new DateOnly(2024, 1, 1))));

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("check-answers")]
    public async Task Post_RedirectsToExpected(string? expectedPage)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var expectedRedirect = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedRedirect = $"{expectedRedirect}/merge/{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        }

        AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
    }

    [Theory]
    [InlineData("check-answers")]
    public async Task Post_WithReturnUrlToCheckAnswersPage_RedirectsToCheckAnswersPage(string? expectedPage)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            CreateState(personA, s =>
            {
                s.PersonBId = personB.PersonId;
                s.PersonBTrn = personB.Trn;
                s.PrimaryPersonId = personA.PersonId;
                s.PersonAttributeSourcesSet = true;
                s.Evidence = new()
                {
                    UploadEvidence = false
                };
            }));

        var checkAnswersUrl = $"/persons/{personA.PersonId}/merge/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(personA, journeyInstance)}&returnUrl={Uri.EscapeDataString(checkAnswersUrl)}")
        {
            Content = new MergePersonPostRequestContentBuilder()
                .WithOtherTrn(personB.Trn)
                .WithPrimaryPersonId(personB.PersonId)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var expectedRedirect = $"/persons/{personA.PersonId}";
        if (expectedPage is not null)
        {
            expectedRedirect = $"{expectedRedirect}/merge/{expectedPage}?{journeyInstance.GetUniqueIdQueryParameter()}";
        }

        AssertEx.ResponseIsRedirectTo(response, expectedRedirect);
    }
}
