using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public partial class InductionExemptionTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithInvalidRoute_ReturnsBadRequest()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionRequired == FieldRequirement.NotApplicable)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/induction-exemption?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act, Assert
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPreviouslyStoredChoice_ShowsChoice()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.Name == "Northern Irish Recognition") // a route with mandatory induction exemption that isn't implicit (requires a yes/no answer)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithInductionExemption(isExempt: true)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/induction-exemption?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var inductionExemptionChoice = doc.GetElementByTestId("induction-exemption")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked).Value;
        Assert.Equal(true.ToString(), inductionExemptionChoice);
    }

    [Theory]
    [InlineData("Graduate Teacher Programme", "training-provider")]
    [InlineData("Apply for Qualified Teacher Status in England", "country")]
    public async Task Post_WhenExemptionEntered_SavesDataAndRedirectsToNextPage(string routeName, string page)
    {
        // Arrange
        var awardDate = Clock.Today;
        var endDate = awardDate.AddDays(-1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.Name == routeName)
            .First();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/induction-exemption?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "IsExemptFromInduction", true.ToString()}
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/add/{page}?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(true, journeyInstance.State.IsExemptFromInduction);
    }

    [Fact]
    public async Task Post_WhenNoChoiceSelected_ReturnsError()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.Name == "Northern Irish Recognition")
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;
        var editRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/induction-exemption?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "IsExemptFromInduction", "Select yes if this route provides an induction exemption");
    }

    [Fact]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.Name == "Northern Irish Recognition") // a route that requires the induction exemption question
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var personId = person.PersonId;
        var editRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/induction-exemption?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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

    [Theory]
    [MemberData(nameof(HttpMethods), TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var awardDate = Clock.Today;
        var endDate = awardDate.AddDays(-1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.Name == "Graduate Teacher Programme")
            .First();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var personId = person.PersonId;
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            personId,
            addRouteState
            );

        var request = new HttpRequestMessage(httpMethod, $"/route/add/induction-exemption?personId={personId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private Task<JourneyInstance<AddRouteState>> CreateJourneyInstanceAsync(Guid personId, AddRouteState? state = null) =>
        CreateJourneyInstance(
           JourneyNames.AddRouteToProfessionalStatus,
           state ?? new AddRouteState(),
           new KeyValuePair<string, object>("personId", personId));
}
