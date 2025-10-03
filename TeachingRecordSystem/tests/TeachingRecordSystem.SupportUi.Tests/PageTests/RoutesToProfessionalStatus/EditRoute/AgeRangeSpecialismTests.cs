using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class AgeRangeSpecialismTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    [Arguments("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false, true)]
    [Arguments("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, true, true)]
    [Arguments("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, false, true)]
    [Arguments("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, true, true)]
    [Arguments("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, true)]
    [Arguments("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, true)]
    [Arguments("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, false)]
    [Arguments("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, false)]
    [Arguments("Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, false)]
    [Arguments("Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, false)]
    public async Task Get_FieldsMarkedAsOptional_BasedOnRouteAndStatusFieldRequirements(string routeName, RouteToProfessionalStatusStatus status, bool statusEditedDuringCurrentJourney, bool expectFieldsToBeOptional)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == routeName).Single();
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today.AddDays(-1);
        var holdsFrom = endDate.AddDays(1);
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1).Select(s => s.TrainingSubjectId).ToArray();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(statusEditedDuringCurrentJourney ? RouteToProfessionalStatusStatus.Deferred : status)
                .WithTrainingStartDate(startDate)
                .WithTrainingEndDate(endDate)
                .WithHoldsFrom(holdsFrom)
                .WithTrainingSubjectIds(subjects)
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithDegreeTypeId(degreeType.DegreeTypeId)
                .WithTrainingCountryId(country.CountryId)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var builder = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(statusEditedDuringCurrentJourney ? RouteToProfessionalStatusStatus.Deferred : status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithHoldsFrom(holdsFrom)
            .WithTrainingSubjectIds(subjects)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithDegreeTypeId(degreeType.DegreeTypeId)
            .WithTrainingCountryId(country.CountryId);

        if (statusEditedDuringCurrentJourney)
        {
            builder = builder.WithEditRouteStatusState(builder => builder
                .WithStatus(status)
                .WithEndDate(endDate));
        }

        var editRouteState = builder.Build();

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, editRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationId}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var heading = doc.QuerySelector("h1.govuk-fieldset__heading");
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
    [Arguments("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false, true)]
    [Arguments("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, true, true)]
    [Arguments("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, false, true)]
    [Arguments("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, true, true)]
    [Arguments("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, true)]
    [Arguments("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, true)]
    [Arguments("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, false)]
    [Arguments("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, false)]
    [Arguments("Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, false)]
    [Arguments("Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, false)]
    public async Task Post_MissingValues_ValidOrInvalid_BasedOnRouteAndStatusFieldRequirements(string routeName, RouteToProfessionalStatusStatus status, bool statusEditedDuringCurrentJourney, bool isValid)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == routeName).Single();
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today.AddDays(-1);
        var holdsFrom = endDate.AddDays(1);
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1).Select(s => s.TrainingSubjectId).ToArray();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(statusEditedDuringCurrentJourney ? RouteToProfessionalStatusStatus.Deferred : status)
                .WithTrainingStartDate(startDate)
                .WithTrainingEndDate(endDate)
                .WithHoldsFrom(holdsFrom)
                .WithTrainingSubjectIds(subjects)
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithDegreeTypeId(degreeType.DegreeTypeId)
                .WithTrainingCountryId(country.CountryId)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var builder = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(statusEditedDuringCurrentJourney ? RouteToProfessionalStatusStatus.Deferred : status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithHoldsFrom(holdsFrom)
            .WithTrainingSubjectIds(subjects)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithDegreeTypeId(degreeType.DegreeTypeId)
            .WithTrainingCountryId(country.CountryId);

        if (statusEditedDuringCurrentJourney)
        {
            builder = builder.WithEditRouteStatusState(builder => builder
                .WithStatus(status)
                .WithEndDate(endDate));
        }

        var editRouteState = builder.Build();

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, editRouteState);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        if (isValid)
        {
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        }
        else
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, "TrainingAgeSpecialism.AgeRangeType", "Enter an age range specialism");
        }
    }

    [Test]
    public async Task Post_WhenAgeRangeFromToIsEntered_PersistsDataAndRedirectsToDetail()
    {
        // Arrange
        var ageFrom = 4;
        var ageTo = 8;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, editRouteState);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeType), TrainingAgeSpecialismType.Range },
                { nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeFrom), ageFrom },
                { nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeTo), ageTo }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(ageFrom, journeyInstance.State.TrainingAgeSpecialismRangeFrom);
        Assert.Equal(ageTo, journeyInstance.State.TrainingAgeSpecialismRangeTo);
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/{qualificationId}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_WhenAgeSpecialismIsEntered_PersistsDataAndRedirectsToDetail()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, editRouteState);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeType), TrainingAgeSpecialismType.KeyStage4 }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(TrainingAgeSpecialismType.KeyStage4, journeyInstance.State.TrainingAgeSpecialismType);
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/{qualificationId}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Test]
    [Arguments(TrainingAgeSpecialismType.Range, null, null)]
    [Arguments(TrainingAgeSpecialismType.Range, 1, null)]
    [Arguments(TrainingAgeSpecialismType.Range, null, 5)]
    [Arguments(TrainingAgeSpecialismType.Range, 1, 33)]
    public async Task Post_WhenInputInvalid_ShowsError(TrainingAgeSpecialismType radioChoice, int? ageFrom, int? ageTo)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, editRouteState);

        var contentBuilder = new FormUrlEncodedContentBuilder()
            .Add(nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeType), radioChoice);
        if (ageFrom is not null)
        {
            contentBuilder.Add(nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeFrom), ageFrom);
        }
        if (ageTo is not null)
        {
            contentBuilder.Add(nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeTo), ageTo);
        }
        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = contentBuilder
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response, StatusCodes.Status400BadRequest);
    }

    [Test]
    [Arguments(TrainingAgeSpecialismType.KeyStage4, 1, null)]
    [Arguments(TrainingAgeSpecialismType.KeyStage4, 1, 5)]
    [Arguments(TrainingAgeSpecialismType.KeyStage4, 1, 33)]
    public async Task Post_WhenTrainingAgeSpecialismTypeEntered_AndAgeRangeTextEntered_ClearsAgeRangeTextAndPersistsTrainingAgeSpecialismType(TrainingAgeSpecialismType radioChoice, int? ageFrom, int? ageTo)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, editRouteState);

        var contentBuilder = new FormUrlEncodedContentBuilder()
            .Add(nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeType), radioChoice);
        if (ageFrom is not null)
        {
            contentBuilder.Add(nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeFrom), ageFrom);
        }
        if (ageTo is not null)
        {
            contentBuilder.Add(nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeTo), ageTo);
        }
        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = contentBuilder
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance.State.TrainingAgeSpecialismRangeFrom);
        Assert.Null(journeyInstance.State.TrainingAgeSpecialismRangeTo);
    }

    [Test]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, editRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationId}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, editRouteState);

        var request = new HttpRequestMessage(httpMethod, $"/route/{qualificationId}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}");

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
