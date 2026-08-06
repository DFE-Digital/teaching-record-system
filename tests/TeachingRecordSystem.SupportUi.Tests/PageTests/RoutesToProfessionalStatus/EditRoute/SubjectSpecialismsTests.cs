using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public partial class SubjectSpecialismsTests(HostFixture hostFixture) : EditRouteTestBase(hostFixture)
{
    [Theory]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false, true)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, true, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, false, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, true, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, true)]
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/subjects?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var doc = await ReadContentAsync(response);

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
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false, true)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, true, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, false, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, true, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, true)]
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
        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/subjects?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        if (expectFieldsToBeOptional)
        {
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        }
        else
        {
            var doc = await ReadContentAsync(response);
            AssertEx.HtmlDocumentHasError(doc, "SubjectId1", "Enter a subject");
        }
    }

    [Fact]
    public async Task Get_WithPreviouslyStoredChoices_ShowsChoices()
    {
        // Arrange
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).TakeRandom(3).ToArray();
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingSubjectsRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationid = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status,
            CurrentStatus = status,
            TrainingSubjectIds = subjects.Select(s => s.TrainingSubjectId).ToArray()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/subjects?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await ReadContentAsync(response);
        Assert.Equal(subjects[0].TrainingSubjectId.ToString(), ((IHtmlSelectElement)doc.GetElementById("SubjectId1")!).Value);
        Assert.Equal(subjects[1].TrainingSubjectId.ToString(), ((IHtmlSelectElement)doc.GetElementById("SubjectId2")!).Value);
        Assert.Equal(subjects[2].TrainingSubjectId.ToString(), ((IHtmlSelectElement)doc.GetElementById("SubjectId3")!).Value);
    }

    [Fact]
    public async Task Post_WhenSubjectsAreEntered_SavesDataAndRedirectsToDetail()
    {
        // Arrange
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).TakeRandom(3).ToArray();
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingSubjectsRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationid = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/subjects?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "SubjectId1", subjects[0].TrainingSubjectId},
                { "SubjectId2", subjects[1].TrainingSubjectId },
                { "SubjectId3", subjects[2].TrainingSubjectId }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        Assert.Equal(subjects.Select(s => s.TrainingSubjectId), GetJourneyInstanceState(journeyInstance)!.TrainingSubjectIds);
    }

    [Fact]
    public async Task Post_SubjectIsOptional_WhenNoSubjectsAreEntered_RedirectsToDetail()
    {
        // Arrange
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).TakeRandom(3).Select(s => s.TrainingSubjectId).ToArray();
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingSubjectsRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingSubjectIds(subjects)));
        var qualificationid = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/subjects?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        Assert.Empty(GetJourneyInstanceState(journeyInstance)!.TrainingSubjectIds);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToQualifications()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingSubjectsRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationid = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/subjects?{journeyInstance.GetUniqueIdQueryParameter()}")
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

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingSubjectsRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingSubjectsRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
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
        var qualificationid = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(httpMethod, $"/routes/{qualificationid}/edit/subjects?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    // Using local helper methods instead of the test extension methods which enforce smart quotes
    // because the subject list contains names like "men's studies" without smart quotes
    private async Task<IHtmlDocument> ReadContentAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        var parser = new HtmlParser();
        return await parser.ParseDocumentAsync(content);
    }

    private string GetErrorMessage(IHtmlDocument doc, string errorElementId)
    {
        var errorElement = doc.GetElementById(errorElementId);
        var vht = errorElement!.GetElementsByTagName("span")[0];
        return errorElement.InnerHtml[vht.OuterHtml.Length..];
    }
}
