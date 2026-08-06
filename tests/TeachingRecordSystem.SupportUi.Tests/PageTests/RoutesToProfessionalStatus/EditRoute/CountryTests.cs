using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class CountryTests(HostFixture hostFixture) : EditRouteTestBase(hostFixture)
{
    [Theory]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false, false)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, true, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, false, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, true, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, false)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, false)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, false)]
    public async Task Get_FieldsMarkedAsOptional_BasedOnRouteAndStatusFieldRequirements(string routeName, RouteToProfessionalStatusStatus status, bool statusEditedDuringCurrentJourney, bool expectFieldsToBeOptional)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == routeName);
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = endDate.AddDays(1);
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1).Select(s => s.TrainingSubjectId).ToArray();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
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

        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = statusEditedDuringCurrentJourney ? RouteToProfessionalStatusStatus.Deferred : status,
            CurrentStatus = statusEditedDuringCurrentJourney ? RouteToProfessionalStatusStatus.Deferred : status,
            TrainingStartDate = startDate,
            TrainingEndDate = endDate,
            HoldsFrom = holdsFrom,
            TrainingSubjectIds = subjects,
            TrainingProviderId = trainingProvider.TrainingProviderId,
            DegreeTypeId = degreeType.DegreeTypeId,
            TrainingCountryId = country.CountryId,
            EditStatusState = statusEditedDuringCurrentJourney ? new() { Status = status } : null
        };

        var qualificationid = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationid, editRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/country?{journeyInstance.GetUniqueIdQueryParameter()}");

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

    [Theory]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false, false)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, true, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, false, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, true, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, false)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, false)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, false)]
    public async Task Post_MissingValues_ValidOrInvalid_BasedOnRouteAndStatusFieldRequirements(string routeName, RouteToProfessionalStatusStatus status, bool statusEditedDuringCurrentJourney, bool expectFieldsToBeOptional)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == routeName);
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = endDate.AddDays(1);
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1).Select(s => s.TrainingSubjectId).ToArray();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
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

        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = statusEditedDuringCurrentJourney ? RouteToProfessionalStatusStatus.Deferred : status,
            CurrentStatus = statusEditedDuringCurrentJourney ? RouteToProfessionalStatusStatus.Deferred : status,
            TrainingStartDate = startDate,
            TrainingEndDate = endDate,
            HoldsFrom = holdsFrom,
            TrainingSubjectIds = subjects,
            TrainingProviderId = trainingProvider.TrainingProviderId,
            DegreeTypeId = degreeType.DegreeTypeId,
            TrainingCountryId = country.CountryId,
            EditStatusState = statusEditedDuringCurrentJourney ? new() { Status = status } : null
        };

        var qualificationid = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationid, editRouteState);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/country?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        if (expectFieldsToBeOptional)
        {
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        }
        else
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, "TrainingCountryId", "Enter a country");
        }
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToQualifications()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationid = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = RouteToProfessionalStatusStatus.Deferred
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/country?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}/qualifications", location);
        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_SelectCountry_PersistsSelection()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingCountryRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingCountryRequired == FieldRequirement.Mandatory && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status
        };
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/country?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TrainingCountryId", country.CountryId.ToString()}
            }
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(country.CountryId, GetJourneyInstanceState(journeyInstance)!.TrainingCountryId);
    }

    [Fact]
    public async Task Post_WhenCountryIsEntered_RedirectsToDetail()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.DegreeTypeRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.DegreeTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status
        };
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/country?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TrainingCountryId", country.CountryId.ToString()}
            }
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/{qualificationId}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_QuestionIsNotAskedForRouteAndStatus_IsNotReachable()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == "Test Route With NotApplicable Country");
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(new DateOnly(2024, 01, 01))));
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/country?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingCountryRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingCountryRequired == FieldRequirement.Mandatory && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status
        };
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(httpMethod, $"/routes/{qualificationId}/edit/country?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}
