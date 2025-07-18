using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.DeleteRoute;

public class ChangeReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithPreviouslyStoredChoices_ShowsChoices()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState()
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            deleteRouteState
            );
        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationId}/delete/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoiceSelection = doc.GetElementByTestId("reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked == true).Value;
        Assert.Equal(deleteRouteState.ChangeReason.ToString(), reasonChoiceSelection);

        var additionalDetailChoices = doc.GetElementByTestId("has-additional-reason_detail-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked == true).Value;
        Assert.Equal(true.ToString(), additionalDetailChoices);

        var uploadEvidenceChoices = doc.GetElementByTestId("upload-evidence-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked == true).Value;
        Assert.Equal(false.ToString(), uploadEvidenceChoices);

        var additionalDetailTextArea = doc.GetElementByTestId("additional-detail")!.GetElementsByTagName("textarea").Single() as IHtmlTextAreaElement;
        Assert.Equal(deleteRouteState.ChangeReasonDetail.ChangeReasonDetail, additionalDetailTextArea!.Value);
    }

    [Fact]
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
        var deleteRouteState = new DeleteRouteState()
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            deleteRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationId}/delete/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var reasonChoicesLegend = doc.GetElementByTestId("reason-options-legend");
        Assert.Equal("Why are you deleting this route?", reasonChoicesLegend!.TrimmedText());
        var reasonChoices = doc.GetElementByTestId("reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
             .Select(i => i.Value);
        Assert.Equal(expectedChoices, reasonChoices);

        var additionalDetailChoices = doc.GetElementByTestId("has-additional-reason_detail-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Select(i => i.Value);
        Assert.Equal(new[] { "True", "False" }, additionalDetailChoices);

        var uploadEvidenceChoices = doc.GetElementByTestId("upload-evidence-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Select(i => i.Value);
        Assert.Equal(new[] { "True", "False" }, uploadEvidenceChoices);
    }

    [Fact]
    public async Task Post_SetValidChangeReasonDetails_PersistsDetailsAndRedirects()
    {
        // Arrange
        var changeReason = ChangeReasonOption.CreatedInError;
        var changeReasonDetails = "A description about why the deletion typed into the box";

        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            deleteRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/delete/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
                .Add("ChangeReason", changeReason)
                .Add("HasAdditionalReasonDetail", true)
                .Add("ChangeReasonDetail", changeReasonDetails)
                .Add("UploadEvidence", false)
                .Build()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(changeReason.GetDisplayName(), journeyInstance.State.ChangeReason!.GetDisplayName());
        Assert.Equal(changeReasonDetails, journeyInstance.State.ChangeReasonDetail.ChangeReasonDetail);
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_NoChoicesAreEntered_ReturnsErrors()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            deleteRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/delete/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ChangeReason", "Select a reason");
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasAdditionalReasonDetail", "Select yes if you want to add more information about why you’re deleting this route");
        await AssertEx.HtmlResponseHasErrorAsync(response, "UploadEvidence", "Select yes if you want to upload evidence");
    }

    [Fact]
    public async Task Post_AdditionalDetailYes_NoDetailAdded_ReturnsError()
    {
        var changeReason = ChangeReasonOption.CreatedInError;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            deleteRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/delete/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
                .Add("ChangeReason", changeReason)
                .Add("HasAdditionalReasonDetail", true)
                .Add("UploadEvidence", false)
                .Build()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ChangeReasonDetail", "Enter additional detail");
    }

    [Fact]
    public async Task Post_FileUploadYes_NoFileUploaded_ReturnsError()
    {
        // Arrange
        var changeReason = ChangeReasonOption.RemovedQtlsStatus;

        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            deleteRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/delete/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
                .Add("ChangeReason", changeReason)
                .Add("HasAdditionalReasonDetail", false)
                .Add("UploadEvidence", true)
                .Build()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EvidenceFile", "Select a file");
    }

    [Fact]
    public async Task Post_SetValidFileUpload_PersistsDetails()
    {
        // Arrange
        var changeReason = ChangeReasonOption.RemovedQtlsStatus;
        var evidenceFileName = "evidence.pdf";
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            deleteRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/delete/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new MultipartFormDataContentBuilder()
                .Add("ChangeReason", changeReason)
                .Add("HasAdditionalReasonDetail", false)
                .Add("UploadEvidence", true)
                .Add("EvidenceFile", CreateEvidenceFileBinaryContent(), evidenceFileName)
                .Build()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.State.ChangeReasonDetail.UploadEvidence);
        Assert.Equal(evidenceFileName, journeyInstance.State.ChangeReasonDetail.EvidenceFileName);
    }

    [Fact]
    public async Task Cancel_deletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            deleteRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/delete/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}");

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

    private Task<JourneyInstance<DeleteRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, DeleteRouteState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.DeleteRouteToProfessionalStatus,
            state ?? new DeleteRouteState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
