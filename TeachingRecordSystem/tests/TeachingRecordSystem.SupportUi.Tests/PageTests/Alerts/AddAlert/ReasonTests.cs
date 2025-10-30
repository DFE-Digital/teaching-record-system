using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class ReasonTests(HostFixture hostFixture) : AddAlertTestBase(hostFixture)
{
    private const string PreviousStep = JourneySteps.StartDate;
    private const string ThisStep = JourneySteps.Reason;

    [Before(Test)]
    public async Task SetUser() => SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.AlertsManagerTraDbs));

    [Test]
    [RolesWithoutAlertWritePermissionData]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, personId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/reason?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_MissingDataInJourneyState_RedirectsToStartDatePage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/start-date?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(ThisStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        AssertCheckedRadioOption("AddReason", journeyInstance.State.AddReason!.ToString()!);
        AssertCheckedRadioOption("HasAdditionalReasonDetail", bool.TrueString);
        Assert.Equal(journeyInstance.State.AddReasonDetail, doc.GetElementsByName("AddReasonDetail")[0].TrimmedText());
        AssertCheckedRadioOption("Evidence.UploadEvidence", bool.TrueString);

        var uploadedEvidenceLink = doc.GetElementByTestId("uploaded-evidence-file-link");
        Assert.NotNull(uploadedEvidenceLink);
        Assert.Equal($"{journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName} ({journeyInstance.State.Evidence.UploadedEvidenceFile!.FileSizeDescription})", uploadedEvidenceLink!.TrimmedText());

        void AssertCheckedRadioOption(string name, string expectedCheckedValue)
        {
            var selectedOption = doc.GetElementsByName(name).SingleOrDefault(r => r.HasAttribute("checked"));
            Assert.Equal(expectedCheckedValue, selectedOption?.GetAttribute("value"));
        }
    }

    [Test]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_WithMissingDataInJourneyState_RedirectsToStartDatePage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/start-date?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_WhenNoReasonIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(addReason: null, hasAdditionalReasonDetail: false, uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "AddReason", "Select a reason");
    }

    [Test]
    public async Task Post_WhenNoHasAdditionalReasonDetailIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                addReason: AddAlertReasonOption.AnotherReason,
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasAdditionalReasonDetail", "Select yes if you want to add more information");
    }

    [Test]
    public async Task Post_WhenAdditionalDetailIsYesButAdditionalDetailsAreEmpty_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                addReason: AddAlertReasonOption.AnotherReason,
                hasAdditionalReasonDetail: true,
                addReasonDetail: null,
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "AddReasonDetail", "Enter additional detail");
    }

    [Test]
    public async Task Post_WhenNoUploadEvidenceOptionIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                addReason: AddAlertReasonOption.AnotherReason,
                hasAdditionalReasonDetail: false,
                addReasonDetail: null)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Test]
    public async Task Post_WhenUploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                addReason: AddAlertReasonOption.AnotherReason,
                hasAdditionalReasonDetail: false,
                uploadEvidence: true,
                evidenceFile: null)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "Select a file");
    }

    [Test]
    public async Task Post_WhenEvidenceFileIsInvalidType_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                addReason: AddAlertReasonOption.AnotherReason,
                hasAdditionalReasonDetail: false,
                uploadEvidence: true,
                evidenceFile: (CreateEvidenceFileBinaryContent(), "badevidence.exe"))
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Test]
    public async Task Post_AdditionalReasonIsTooLong_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                addReason: AddAlertReasonOption.AnotherReason,
                hasAdditionalReasonDetail: true,
                addReasonDetail: new string('a', 4001),
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "AddReasonDetail", "Additional detail must be 4000 characters or less");
    }

    [Test]
    public async Task Post_ValidInputWithoutEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var reason = AddAlertReasonOption.AnotherReason;
        var hasAdditionalReasonDetail = true;
        var reasonDetail = "More details";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                addReason: reason,
                hasAdditionalReasonDetail: hasAdditionalReasonDetail,
                addReasonDetail: reasonDetail,
                uploadEvidence: false)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/check-answers?personId={person.PersonId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(reason, journeyInstance.State.AddReason);
        Assert.Equal(hasAdditionalReasonDetail, journeyInstance.State.HasAdditionalReasonDetail);
        Assert.Equal(reasonDetail, journeyInstance.State.AddReasonDetail);
        Assert.Null(journeyInstance.State.Evidence.UploadedEvidenceFile);
    }

    [Test]
    public async Task Post_ValidInputWithEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var reason = AddAlertReasonOption.AnotherReason;
        var hasAdditionalReasonDetail = false;
        var evidenceFileName = "evidence.pdf";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(
                addReason: reason,
                hasAdditionalReasonDetail: hasAdditionalReasonDetail,
                uploadEvidence: true,
                evidenceFile: (CreateEvidenceFileBinaryContent(), evidenceFileName))
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/check-answers?personId={person.PersonId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(reason, journeyInstance.State.AddReason);
        Assert.False(journeyInstance.State.HasAdditionalReasonDetail);
        Assert.Null(journeyInstance.State.AddReasonDetail);
        Assert.Equal(evidenceFileName, journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName);
        Assert.NotNull(journeyInstance.State.Evidence.UploadedEvidenceFile!.FileId);
        Assert.NotNull(journeyInstance.State.Evidence.UploadedEvidenceFile!.FileSizeDescription);
    }

    [Test]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/add/reason/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Test]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, person.PersonId);

        var request = new HttpRequestMessage(httpMethod, $"/alerts/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private static MultipartFormDataContentBuilder CreateMinimumValidPostContent() =>
        CreatePostContent(
            addReason: AddAlertReasonOption.AnotherReason,
            hasAdditionalReasonDetail: false,
            uploadEvidence: false);

    private static MultipartFormDataContentBuilder CreatePostContent(
        AddAlertReasonOption? addReason = null,
        bool? hasAdditionalReasonDetail = null,
        string? addReasonDetail = null,
        bool? uploadEvidence = null,
        (HttpContent Content, string FileName)? evidenceFile = null)
    {
        return new MultipartFormDataContentBuilder
        {
            { "AddReason", addReason },
            { "HasAdditionalReasonDetail", hasAdditionalReasonDetail },
            { "AddReasonDetail", addReasonDetail },
            { "Evidence.UploadEvidence", uploadEvidence },
            { "Evidence.EvidenceFile", evidenceFile }
        };
    }
}
