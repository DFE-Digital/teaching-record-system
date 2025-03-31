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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.Equal($"/route/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_StatusMovesToAwarded_PersistsDataAndRedirectsToEndDate()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.Equal($"/route/{qualificationid}/edit/end-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}")
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.Equal(true, journeyInstance.State.IsExemptFromInduction);
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
            .WithCurrentStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.Equal($"/route/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Cancel_RedirectsToExpectedPage()
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelLink = doc.QuerySelector("a.govuk-link:contains('Cancel')") as IHtmlAnchorElement;
        Assert.Contains($"/route/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", cancelLink!.Href);
    }

    private Task<JourneyInstance<EditRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, EditRouteState? state = null) =>
        CreateJourneyInstance(
           JourneyNames.EditRouteToProfessionalStatus,
           state ?? new EditRouteState(),
           new KeyValuePair<string, object>("qualificationId", qualificationId));

}
