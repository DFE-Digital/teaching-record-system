using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class StatusTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_ShowsExistingStatus()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .RandomOne();
        var status = ProfessionalStatusStatus.InTraining;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(status)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var statusChoice = doc.GetElementByTestId("status")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked == true).Value;
        Assert.Equal(status.ToString(), statusChoice);
    }

    [Fact]
    public async Task Post_StatusIsNotAwardedOrApprovedStatus_PersistsDataAndRedirectsToDetail()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .RandomOne();
        var status = ProfessionalStatusStatus.InTraining;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.UnderAssessment)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { nameof(StatusModel.Status), status }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(status, journeyInstance.State.Status);
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_StatusMovesToAwarded_PersistsDataAndRedirectsToEndDate()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatus.Awarded;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.UnderAssessment)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .WithCurrentStatus(ProfessionalStatusStatus.UnderAssessment)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { nameof(StatusModel.Status), status }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(status, journeyInstance.State.EditStatusState!.Status);
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/{qualificationid}/edit/end-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_StatusMovesFromAwardedToAnotherStatus_RemovesAwardedDateAndExemptionFlag()
    {
        // Arrange
        var awardedDate = Clock.Today;
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .RandomOne();
        var newStatus = ProfessionalStatusStatus.UnderAssessment;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Awarded)
                .WithAwardedDate(awardedDate)
                .WithExemptFromInduction(true)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.Awarded)
            .WithAwardedDate(awardedDate)
            .WithInductionExemption(true)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { nameof(StatusModel.Status), newStatus }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(newStatus, journeyInstance.State.Status);
        Assert.Null(journeyInstance.State.AwardedDate);
        Assert.Null(journeyInstance.State.IsExemptFromInduction);
    }

    [Fact]
    public async Task Post_StatusMovesToAwarded_RouteHasImplictExemption_ExemptionSetToTrue()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.InductionExemptionReasonId.HasValue)
            .Join(
                (await ReferenceDataCache.GetInductionExemptionReasonsAsync()).Where(e => e.RouteImplicitExemption),
                r => r.InductionExemptionReasonId,
                e => e.InductionExemptionReasonId,
                (r, e) => r
            )
        .RandomOne();

        var exemptionReasons = await ReferenceDataCache.GetInductionExemptionReasonsAsync();

        var status = ProfessionalStatusStatus.Awarded;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.UnderAssessment)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .WithCurrentStatus(ProfessionalStatusStatus.UnderAssessment)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { nameof(StatusModel.Status), status }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(status, journeyInstance.State.EditStatusState!.Status);
        Assert.True(journeyInstance.State.EditStatusState!.RouteImplicitExemption);
        Assert.Equal(true, journeyInstance.State.EditStatusState!.InductionExemption);
    }

    [Fact]
    public async Task Post_StatusStaysAwarded_PersistsDataAndRedirectsToDetail()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .RandomOne();
        var status = ProfessionalStatusStatus.Awarded;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(status)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { nameof(StatusModel.Status), ProfessionalStatusStatus.Approved }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(ProfessionalStatusStatus.Approved, journeyInstance.State.Status);
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .RandomOne();
        var status = ProfessionalStatusStatus.InTraining;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(status)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .WithCurrentStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}");

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

    private Task<JourneyInstance<EditRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, EditRouteState? state = null) =>
        CreateJourneyInstance(
           JourneyNames.EditRouteToProfessionalStatus,
           state ?? new EditRouteState(),
           new KeyValuePair<string, object>("qualificationId", qualificationId));

}
