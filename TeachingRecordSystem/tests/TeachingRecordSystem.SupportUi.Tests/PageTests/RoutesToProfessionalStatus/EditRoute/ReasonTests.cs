using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_WithPreviouslyStoredChoices_ShowsChoices()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );
        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoiceSelection = doc.GetElementByTestId("reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(editRouteState.ChangeReason.ToString(), reasonChoiceSelection);

        var additionalDetailChoices = doc.GetElementByTestId("has-additional-reason_detail-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(true.ToString(), additionalDetailChoices);

        var uploadEvidenceChoices = doc.GetElementByTestId("upload-evidence-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(false.ToString(), uploadEvidenceChoices);

        var additionalDetailTextArea = doc.GetElementByTestId("additional-detail")!.GetElementsByTagName("textarea").Single() as IHtmlTextAreaElement;
        Assert.Equal(editRouteState.ChangeReasonDetail.ChangeReasonDetail, additionalDetailTextArea!.Value);
    }

    [Test]
    public async Task Get_ExpectedRadioButtonsExistOnPage()
    {
        // Arrange
        var expectedChoices = Enum.GetValues<ChangeReasonOption>().Select(s => s.ToString());

        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoicesLegend = doc.GetElementByTestId("reason-options-legend");
        Assert.Equal("Why are you editing this route?", reasonChoicesLegend!.TrimmedText());
        var reasonChoices = doc.GetElementByTestId("reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
             .Select(i => i.Value);
        Assert.Equal(expectedChoices, reasonChoices);

        var additionalDetailChoices = doc.GetElementByTestId("has-additional-reason_detail-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Select(i => i.Value);
        Assert.Equal(["True", "False"], additionalDetailChoices);

        var uploadEvidenceChoices = doc.GetElementByTestId("upload-evidence-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Select(i => i.Value);
        Assert.Equal(["True", "False"], uploadEvidenceChoices);
    }

    [Test]
    public async Task Post_SetValidChangeReasonDetails_PersistsDetailsAndRedirects()
    {
        // Arrange
        var changeReason = ChangeReasonOption.NewInformation;
        var changeReasonDetails = "A description about why the change typed into the box";

        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(
                new EditRoutePostRequestBuilder()
                    .WithChangeReason(changeReason)
                    .WithChangeReasonDetailSelections(true, changeReasonDetails)
                    .WithNoFileUploadSelection()
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(changeReason.GetDisplayName(), journeyInstance.State.ChangeReason!.GetDisplayName());
        Assert.Equal(changeReasonDetails, journeyInstance.State.ChangeReasonDetail.ChangeReasonDetail);
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_NoChoicesAreEntered_ReturnsErrors()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "Evidence.UploadEvidence", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ChangeReason", "Select a reason");
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasAdditionalReasonDetail", "Select yes if you want to add more information about why you’re editing this route");
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Test]
    public async Task Post_AdditionalDetailYes_NoDetailAdded_ReturnsError()
    {
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(
                new EditRoutePostRequestBuilder()
                    .WithChangeReason(ChangeReasonOption.AnotherReason)
                    .WithChangeReasonDetailSelections(true, null)
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ChangeReasonDetail", "Enter additional detail");
    }

    [Test]
    public async Task Post_FileUploadYes_NoFileUploaded_ReturnsError()
    {
        // Arrange
        var changeReason = ChangeReasonOption.NewInformation;
        var changeReasonDetails = "A description about why the change typed into the box";

        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithChangeReasonOption(changeReason)
            .WithChangeReasonDetail(detail: changeReasonDetails, fileUpload: false)
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "ChangeReason", changeReason },
                { "HasAdditionalReasonDetail", false },
                { "Evidence.UploadEvidence", true }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Evidence.EvidenceFile", "Select a file");
    }

    [Test]
    public async Task Post_SetValidFileUpload_PersistsDetails()
    {
        // Arrange
        var changeReason = ChangeReasonOption.NewInformation;
        var changeReasonDetails = "A description about why the change typed into the box";
        var evidenceFileName = "evidence.pdf";
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithChangeReasonOption(changeReason)
            .WithChangeReasonDetail(detail: changeReasonDetails, fileUpload: false)
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder
            {
                { "ChangeReason", changeReason },
                { "HasAdditionalReasonDetail", false },
                { "Evidence.UploadEvidence", true },
                { "Evidence.EvidenceFile", (CreateEvidenceFileBinaryContent(), evidenceFileName) }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.State.ChangeReasonDetail.Evidence.UploadEvidence);
        Assert.Equal(evidenceFileName, journeyInstance.State.ChangeReasonDetail.Evidence.UploadedEvidenceFile!.FileName);
    }

    [Test]
    public async Task Cancel_deletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}/qualifications", location);
        Assert.Null(await ReloadJourneyInstance(journeyInstance));
    }

    [Test]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(httpMethod, $"/routes/{qualificationid}/edit/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private Task<JourneyInstance<EditRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, EditRouteState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditRouteToProfessionalStatus,
            state ?? new EditRouteState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
