using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public class DegreeTypeTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    [Arguments("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, true)]
    [Arguments("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, false)]
    [Arguments("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false)]
    [Arguments("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, false)]
    public async Task Get_FieldsMarkedAsOptional_BasedOnRouteAndStatusFieldRequirements(string routeName, RouteToProfessionalStatusStatus status, bool expectFieldsToBeOptional)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == routeName);

        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, addRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/add/degree-type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var heading = doc.QuerySelector("label.govuk-label--l");
        Assert.NotNull(heading);
        if (expectFieldsToBeOptional)
        {
            Assert.Contains("(optional)", heading.TrimmedText());
        }
        else
        {
            Assert.DoesNotContain("(optional)", heading.TrimmedText());
        }
    }

    [Test]
    [Arguments("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, true)]
    [Arguments("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, false)]
    [Arguments("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false)]
    [Arguments("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, false)]
    public async Task Post_MissingValues_ValidOrInvalid_BasedOnRouteAndStatusFieldRequirements(string routeName, RouteToProfessionalStatusStatus status, bool expectFieldsToBeOptional)
    {
        // Arrange
        var routes = await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync();
        var route = routes.Single(r => r.Name == routeName);

        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, addRouteState);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/degree-type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        if (expectFieldsToBeOptional)
        {
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        }
        else
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, "DegreeTypeId", "Select a degree type");
        }
    }

    [Test]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.DegreeTypeRequired != FieldRequirement.NotApplicable)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.DegreeTypeRequired != FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/add/degree-type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
    public async Task Post_SelectDegreeType_PersistsSelection()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.DegreeTypeRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.DegreeTypeRequired == FieldRequirement.Optional)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/degree-type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "DegreeTypeId", degreeType.DegreeTypeId.ToString()}
            }
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(degreeType.DegreeTypeId, journeyInstance.State.DegreeTypeId);
    }

    [Test]
    public async Task Post_WhenDegreeTypeIsEntered_RedirectsToCountry()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.DegreeTypeRequired != FieldRequirement.NotApplicable && r.TrainingCountryRequired != FieldRequirement.NotApplicable)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.DegreeTypeRequired != FieldRequirement.NotApplicable && s.TrainingCountryRequired != FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/degree-type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "DegreeTypeId", degreeType.DegreeTypeId.ToString()}
            }
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/add/country?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.DegreeTypeRequired != FieldRequirement.NotApplicable)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.DegreeTypeRequired != FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(httpMethod, $"/routes/add/degree-type?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
