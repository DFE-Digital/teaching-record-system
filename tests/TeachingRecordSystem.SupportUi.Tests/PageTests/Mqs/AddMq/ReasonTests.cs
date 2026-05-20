using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.AddMq;

public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(personId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/reason?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithStatusMissingFromState_RedirectsToStatusPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            state => state.Status = null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithPersonIdForValidPerson_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            state =>
            {
                state.HasAdditionalReasonDetail = true;
                state.AddReason = AddMqReasonOption.NewInformationReceived;
                state.AddReasonDetail = "Some additional details";
                state.Evidence = new EvidenceUploadModel
                {
                    UploadEvidence = true,
                    UploadedEvidenceFile = new()
                    {
                        FileId = Guid.NewGuid(),
                        FileName = "evidence.pdf",
                        FileSizeDescription = "5MB"
                    }
                };
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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

    [Fact]
    public async Task Post_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(personId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/reason?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithStatusMissingFromState_RedirectsToStatusPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            state => state.Status = null);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoReasonIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "HasAdditionalReasonDetail", "false" },
                { "Evidence.UploadEvidence", "false" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "AddReason", "Select a reason");
    }

    [Fact]
    public async Task Post_WhenNoHasAdditionalReasonDetailIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "AddReason", AddMqReasonOption.NewInformationReceived.ToString() },
                { "Evidence.UploadEvidence", "false" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasAdditionalReasonDetail", "Select yes if you want to add more information about why you’re adding this mandatory qualification");
    }

    [Fact]
    public async Task Post_WhenAdditionalDetailIsYesButAdditionalDetailsAreEmpty_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "AddReason", AddMqReasonOption.NewInformationReceived.ToString() },
                { "HasAdditionalReasonDetail", "true" },
                { "AddReasonDetail", "" },
                { "Evidence.UploadEvidence", "false" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "AddReasonDetail", "Enter additional detail");
    }

    [Fact]
    public async Task Post_WhenNoUploadEvidenceOptionIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "AddReason", AddMqReasonOption.NewInformationReceived.ToString() },
                { "HasAdditionalReasonDetail", "false" },
                { "Evidence.UploadEvidence", null }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Fact]
    public async Task Post_WhenUploadEvidenceOptionIsYesAndNoFileIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "AddReason", AddMqReasonOption.NewInformationReceived.ToString() },
                { "HasAdditionalReasonDetail", "false" },
                { "Evidence.UploadEvidence", "true" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "Select a file");
    }

    [Fact]
    public async Task Post_WhenEvidenceFileIsInvalidType_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "AddReason", AddMqReasonOption.NewInformationReceived.ToString() },
                { "HasAdditionalReasonDetail", "false" },
                { "Evidence.UploadEvidence", "true" },
                { "Evidence.EvidenceFile", (CreateEvidenceFileBinaryContent(), "badevidence.exe") }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "The selected file must be a BMP, CSV, DOC, DOCX, EML, JPEG, JPG, MBOX, MSG, ODS, ODT, PDF, PNG, TIF, TXT, XLS or XLSX");
    }

    [Fact]
    public async Task Post_AdditionalReasonIsTooLong_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "AddReason", AddMqReasonOption.NewInformationReceived.ToString() },
                { "HasAdditionalReasonDetail", "true" },
                { "AddReasonDetail", new string('a', 4001) },
                { "Evidence.UploadEvidence", "false" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "AddReasonDetail", "Additional detail must be 4000 characters or less");
    }

    [Fact]
    public async Task Post_ValidInputWithoutEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var addReason = AddMqReasonOption.NewInformationReceived;
        var hasAdditionalReasonDetail = true;
        var addReasonDetail = "More details";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "AddReason", addReason.ToString() },
                { "HasAdditionalReasonDetail", hasAdditionalReasonDetail.ToString() },
                { "AddReasonDetail", addReasonDetail },
                { "Evidence.UploadEvidence", "false" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(addReason, journeyInstance.State.AddReason);
        Assert.Equal(hasAdditionalReasonDetail, journeyInstance.State.HasAdditionalReasonDetail);
        Assert.Equal(addReasonDetail, journeyInstance.State.AddReasonDetail);
        Assert.Null(journeyInstance.State.Evidence.UploadedEvidenceFile);
    }

    [Fact]
    public async Task Post_ValidInputWithEvidenceFile_UpdatesStateAndRedirectsToCheckAnswersPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);
        var addReason = AddMqReasonOption.NewInformationReceived;
        var hasAdditionalReasonDetail = false;
        var evidenceFileName = "evidence.pdf";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "AddReason", addReason.ToString() },
                { "HasAdditionalReasonDetail", hasAdditionalReasonDetail.ToString() },
                { "Evidence.UploadEvidence", "true" },
                { "Evidence.EvidenceFile", (CreateEvidenceFileBinaryContent(), evidenceFileName) }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(addReason, journeyInstance.State.AddReason);
        Assert.False(journeyInstance.State.HasAdditionalReasonDetail);
        Assert.Null(journeyInstance.State.AddReasonDetail);
        Assert.NotNull(journeyInstance.State.Evidence.UploadedEvidenceFile);
        Assert.Equal(evidenceFileName, journeyInstance.State.Evidence.UploadedEvidenceFile!.FileName);
        Assert.NotNull(journeyInstance.State.Evidence.UploadedEvidenceFile!.FileSizeDescription);
    }

    [Theory]
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

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(httpMethod, $"/mqs/add/reason?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/add/reason/cancel?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/qualifications", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private Task<JourneyInstance<AddMqState>> CreateJourneyInstanceAsync(Guid personId, Action<AddMqState>? configureState = null)
    {
        var state = new AddMqState
        {
            ProviderId = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham").MandatoryQualificationProviderId,
            Specialism = MandatoryQualificationSpecialism.Visual,
            StartDate = new(2020, 9, 1),
            Status = MandatoryQualificationStatus.Passed,
            EndDate = new(2021, 4, 30)
        };
        configureState?.Invoke(state);

        return CreateJourneyInstance(
            JourneyNames.AddMq,
            state,
            new KeyValuePair<string, object>("personId", personId));
    }
}
