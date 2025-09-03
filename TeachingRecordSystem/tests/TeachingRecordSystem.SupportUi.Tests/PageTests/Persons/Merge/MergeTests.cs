using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.Create;
using TeachingRecordSystem.SupportUi.Pages.Persons.Merge;
using static TeachingRecordSystem.TestCommon.TestData;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.Merge;

[Collection(nameof(DisableParallelization))]
public class MergeTests : MergeTestBase
{
    public MergeTests(HostFixture hostFixture) : base(hostFixture)
    {
        FileServiceMock.Invocations.Clear();
    }

    [Fact]
    public async Task Get_ExpectedRadioButtonsExistOnPage()
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithAllDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

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
    [MemberData(nameof(AttributesAndFieldsData))]
    public async Task Get_AttributeIsNotDifferent_RendersDisabledAndUnselectedRadioButtons(
        PersonMatchedAttribute _,
        string fieldName,
        bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithNoDifferences(useNullValues: useNullValues);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

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
    [MemberData(nameof(AttributesAndFieldsData))]
    public async Task Get_AttributeIsDifferent_RendersRadioButtonsWithExistingValueHighlighted(
        PersonMatchedAttribute attribute,
        string fieldName,
        bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(attribute, useNullValues: useNullValues);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

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
    [MemberData(nameof(AttributesAndFieldsData))]
    public async Task Get_AttributeSourceSetToPrimaryPersonInState_RendersSelectedSourceRadioButton(
        PersonMatchedAttribute _,
        string fieldName,
        bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithAllDifferences(useNullValues: useNullValues);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithFirstNameSource(PersonAttributeSource.PrimaryPerson)
                .WithMiddleNameSource(PersonAttributeSource.PrimaryPerson)
                .WithLastNameSource(PersonAttributeSource.PrimaryPerson)
                .WithDateOfBirthSource(PersonAttributeSource.PrimaryPerson)
                .WithEmailAddressSource(PersonAttributeSource.PrimaryPerson)
                .WithNationalInsuranceNumberSource(PersonAttributeSource.PrimaryPerson)
                .WithGenderSource(PersonAttributeSource.PrimaryPerson)
                .WithComments(null)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(personA, journeyInstance));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();
        var radios = doc.GetElementsByName(fieldName);
        Assert.True(radios[0].IsChecked());
    }

    [Theory]
    [MemberData(nameof(AttributesAndFieldsData))]
    public async Task Get_AttributeSourceSetToSecondaryPersonInState_RendersSelectedSourceRadioButton(
        PersonMatchedAttribute _,
        string fieldName,
        bool useNullValues)
    {
        // Arrange
        var (personA, personB) = await CreatePersonsWithAllDifferences(useNullValues: useNullValues);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithFirstNameSource(PersonAttributeSource.SecondaryPerson)
                .WithMiddleNameSource(PersonAttributeSource.SecondaryPerson)
                .WithLastNameSource(PersonAttributeSource.SecondaryPerson)
                .WithDateOfBirthSource(PersonAttributeSource.SecondaryPerson)
                .WithEmailAddressSource(PersonAttributeSource.SecondaryPerson)
                .WithNationalInsuranceNumberSource(PersonAttributeSource.SecondaryPerson)
                .WithGenderSource(PersonAttributeSource.SecondaryPerson)
                .WithComments(null)
                .Build());

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
        var expectedFileUrl = $"{TestScopedServices.FakeBlobStorageFileUrlBase}{evidenceFileId}";
        var comments = "Some comments";

        var (personA, personB) = await CreatePersonsWithAllDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithUploadEvidenceChoice(true, evidenceFileId, "evidence.jpg", "1.2 KB")
                .WithComments(comments)
                .Build());

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

        Assert.Equal(evidenceFileId.ToString(), doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileId)));
        Assert.Equal("evidence.jpg", doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileName)));
        Assert.Equal("1.2 KB", doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileSizeDescription)));
        Assert.Equal(expectedFileUrl, doc.GetHiddenInputValue(nameof(CreateReasonModel.EvidenceFileUrl)));

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
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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
        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(PersonMatchedAttribute.FirstName);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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
        var evidenceFileId = Guid.NewGuid();

        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(PersonMatchedAttribute.FirstName);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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
        var evidenceFileId = Guid.NewGuid();

        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(PersonMatchedAttribute.FirstName);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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
        var evidenceFileId = Guid.NewGuid();

        var (personA, personB) = await CreatePersonsWithSingleDifferenceToMatch(PersonMatchedAttribute.FirstName);

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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
        var (personA, personB) = await CreatePersonsWithNoDifferences();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personA.PersonId,
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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
            new MergeStateBuilder()
                .WithInitializedState(personA)
                .WithPersonB(personB)
                .WithPrimaryPerson(personA)
                .WithFirstNameSource(PersonAttributeSource.SecondaryPerson)
                .WithMiddleNameSource(PersonAttributeSource.SecondaryPerson)
                .WithLastNameSource(PersonAttributeSource.SecondaryPerson)
                .WithDateOfBirthSource(PersonAttributeSource.SecondaryPerson)
                .WithEmailAddressSource(PersonAttributeSource.SecondaryPerson)
                .WithNationalInsuranceNumberSource(PersonAttributeSource.SecondaryPerson)
                .WithGenderSource(PersonAttributeSource.SecondaryPerson)
                .WithComments(null)
                .Build());

        var firstNameSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var middleNameSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var lastNameSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var dateOfBirthSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var emailAddressSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var nationalInsuranceNumberSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();
        var genderSelection = Enum.GetValues<PersonAttributeSource>().SingleRandom();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(personA, journeyInstance))
        {
            Content = new MergePostRequestContentBuilder()
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

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(firstNameSelection, journeyInstance.State.FirstNameSource);
        Assert.Equal(middleNameSelection, journeyInstance.State.MiddleNameSource);
        Assert.Equal(lastNameSelection, journeyInstance.State.LastNameSource);
        Assert.Equal(dateOfBirthSelection, journeyInstance.State.DateOfBirthSource);
        Assert.Equal(emailAddressSelection, journeyInstance.State.EmailAddressSource);
        Assert.Equal(nationalInsuranceNumberSelection, journeyInstance.State.NationalInsuranceNumberSource);
        Assert.Equal(genderSelection, journeyInstance.State.GenderSource);
    }

    public static TheoryData<PersonMatchedAttribute, string, bool> AttributesAndFieldsData { get; } = new()
    {
        { PersonMatchedAttribute.FirstName, "FirstNameSource", false },
        { PersonMatchedAttribute.MiddleName, "MiddleNameSource", false },
        { PersonMatchedAttribute.LastName, "LastNameSource", false },
        { PersonMatchedAttribute.DateOfBirth, "DateOfBirthSource", false },
        { PersonMatchedAttribute.EmailAddress, "EmailAddressSource", false },
        { PersonMatchedAttribute.EmailAddress, "EmailAddressSource", true },
        { PersonMatchedAttribute.NationalInsuranceNumber, "NationalInsuranceNumberSource", false },
        { PersonMatchedAttribute.NationalInsuranceNumber, "NationalInsuranceNumberSource", true },
        { PersonMatchedAttribute.Gender, "GenderSource", false },
        { PersonMatchedAttribute.Gender, "GenderSource", true }
    };

    private string GetRequestPath(CreatePersonResult person, JourneyInstance<MergeState>? journeyInstance = null) =>
        $"/persons/{person.PersonId}/merge/merge?{journeyInstance?.GetUniqueIdQueryParameter()}";

    private Task<JourneyInstance<MergeState>> CreateJourneyInstanceAsync(Guid personId, MergeState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.MergePerson,
            state ?? new MergeState(),
            new KeyValuePair<string, object>("personId", personId));
}
