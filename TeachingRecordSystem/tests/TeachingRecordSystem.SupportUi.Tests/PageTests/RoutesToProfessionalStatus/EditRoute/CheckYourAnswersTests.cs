using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Pages.Routes.EditRoute;
using TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Routes.EditRoute;

public class CheckYourAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Cancel_RedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusesAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithProfessionalStatus(r => r
                .WithRoute(route)
                .WithStatus(ProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.Deferred)
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new Mock<IFileService>().Object,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        Assert.Equal($"/persons/{person.PersonId}/induction", location);
    }

    [Fact]
    public async Task Get_ShowsChangeReason_AsExpected()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusesAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithProfessionalStatus(r => r
                .WithRoute(route)
                .WithStatus(ProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.Deferred)
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new Mock<IFileService>().Object,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TextContent == "Reason for change");
        Assert.NotNull(label);
        var value = label.NextElementSibling;
        Assert.Equal(editRouteState.ChangeReasonDetail.ChangeReason!.GetDisplayName(), value!.TextContent);

        var labelDetails = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TextContent == "Additional information");
        Assert.NotNull(labelDetails);
        var valueDetails = labelDetails.NextElementSibling;
        Assert.Equal(editRouteState.ChangeReasonDetail.ChangeReasonDetail, valueDetails!.TextContent.Trim());

        var labelFileUpload = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TextContent == "Evidence");
        Assert.NotNull(labelFileUpload);
        var valueFileUpload = labelFileUpload.NextElementSibling;
        Assert.Equal("Not provided", valueFileUpload!.TextContent.Trim());
    }

    private Task<JourneyInstance<EditRouteState>> CreateJourneyInstanceAsync(Guid personId, IFileService fileService, EditRouteState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditRouteToProfessionalStatus,
            state ?? new EditRouteState(fileService),
            new KeyValuePair<string, object>("personId", personId));
}
