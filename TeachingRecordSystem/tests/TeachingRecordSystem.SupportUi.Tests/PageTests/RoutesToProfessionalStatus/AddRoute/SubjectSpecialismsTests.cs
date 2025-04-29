using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public partial class SubjectSpecialismsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithPreviouslyStoredChoices_ShowsChoices()
    {
        // Arrange
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).RandomSelection(3).ToArray();
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingSubjectsRequired == FieldRequirement.Optional)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/subjects?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await ReadContentAsync(response);
        Assert.Equal(subjects[0].TrainingSubjectId.ToString(), ((IHtmlSelectElement)doc.GetElementById("SubjectId1")!).Value);
        Assert.Equal(subjects[1].TrainingSubjectId.ToString(), ((IHtmlSelectElement)doc.GetElementById("SubjectId2")!).Value);
        Assert.Equal(subjects[2].TrainingSubjectId.ToString(), ((IHtmlSelectElement)doc.GetElementById("SubjectId3")!).Value);
    }

    [Fact]
    public async Task Post_WhenSubjectsAreEntered_SavesDataAndRedirectsToCyaPage()
    {
        // Arrange
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).RandomSelection(3).ToArray();
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingSubjectsRequired == FieldRequirement.Optional)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/subjects?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "SubjectId1", subjects[0].TrainingSubjectId},
                { "SubjectId2", subjects[1].TrainingSubjectId },
                { "SubjectId3", subjects[2].TrainingSubjectId },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(subjects.Select(s => s.TrainingSubjectId), journeyInstance.State.TrainingSubjectIds);
    }

    [Fact]
    public async Task Post_SubjectIsOptional_WhenNoSubjectIsEntered_RedirectsToNextPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingSubjectsRequired == FieldRequirement.Optional)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/subjects?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingSubjectsRequired == FieldRequirement.Optional)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/subjects?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await ReadContentAsync(response);
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

    // Using local helper methods instead of the test extension methods which enforce smart quotes
    // because the subject list contains names like "men's studies" without smart quotes
    private async Task<IHtmlDocument> ReadContentAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var parser = new HtmlParser();
        return await parser.ParseDocumentAsync(content);
    }

    private Task<JourneyInstance<AddRouteState>> CreateJourneyInstanceAsync(Guid personId, AddRouteState? state = null) =>
        CreateJourneyInstance(
           JourneyNames.AddRouteToProfessionalStatus,
           state ?? new AddRouteState(),
           new KeyValuePair<string, object>("personId", personId));
}
