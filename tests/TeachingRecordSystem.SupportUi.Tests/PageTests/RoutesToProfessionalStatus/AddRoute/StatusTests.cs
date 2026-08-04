using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public class StatusTests(HostFixture hostFixture) : AddRouteTestBase(hostFixture)
{
    [Fact]
    public async Task Get_ShowsExistingStatus()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .SingleRandom();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var statusChoice = doc.GetElementByTestId("status")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(status.ToString(), statusChoice);
    }

    [Theory]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, "start-and-end-date")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Failed, "country")]
    public async Task Post_Status_PersistsDataAndRedirectsToExpected(string routeName, RouteToProfessionalStatusStatus status, string expected)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Single(r => r.Name == routeName);
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { nameof(StatusModel.Status), status }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(status, GetJourneyInstanceState(journeyInstance)!.Status);
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/add/{expected}?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_NoStatusSelected_ShowsError()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Single(r => r.Name == "Postgraduate Teaching Apprenticeship");
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState { RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Status", "Select a route status");
    }

    [Fact]
    public async Task Post_FromCheckAnswers_WhenNewStatusChangesTheQuestionsAsked_ResumesTheJourneyAtTheNextQuestion()
    {
        // Arrange
        // Failed asks only for the country; In training asks for the start and end dates first.
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Single(r => r.Name == "Postgraduate Teaching Apprenticeship");
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = RouteToProfessionalStatusStatus.Failed,
                TrainingCountryId = country.CountryId,
                ChangeReason = ChangeReasonOption.AnotherReason,
                ChangeReasonDetail = CreateChangeReasonDetail()
            });

        var checkAnswersUrl = GetCheckAnswersReturnUrl(journeyInstance, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/status?personId={person.PersonId}&returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { nameof(StatusModel.Status), RouteToProfessionalStatusStatus.InTraining }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/routes/add/start-and-end-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        // The questions after this one are no longer part of the journey, so the user can't skip back
        // to check answers without working through them.
        var checkAnswersRequest = new HttpRequestMessage(HttpMethod.Get, checkAnswersUrl);
        var checkAnswersResponse = await HttpClient.SendAsync(checkAnswersRequest);

        Assert.Equal(StatusCodes.Status302Found, (int)checkAnswersResponse.StatusCode);
        Assert.Equal(
            $"/routes/add/start-and-end-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}",
            checkAnswersResponse.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToQualifications()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .SingleRandom();
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState { RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/qualifications", response.Headers.Location?.OriginalString);
        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .SingleRandom();
        var person = await TestData.CreatePersonAsync();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState { RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId });

        var request = new HttpRequestMessage(httpMethod, $"/routes/add/status?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}
