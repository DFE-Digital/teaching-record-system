using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250627;

public class SetRouteToProfessionalStatusTests : TestBase
{
    public SetRouteToProfessionalStatusTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.SetProfessionalStatus]);
    }

    private static object CreateRequestBody() => new
    {
        routeToProfessionalStatusTypeId = RouteToProfessionalStatusType.HeiProgrammeTypeId,
        status = "InTraining",
        trainingStartDate = new DateOnly(2024, 9, 1),
        trainingEndDate = new DateOnly(2025, 7, 1),
        trainingSubjectReferences = new[] { "100343" },
        trainingAgeSpecialism = new
        {
            type = "Range",
            from = 3,
            to = 7
        },
        trainingCountryReference = "GB",
        trainingProviderUkprn = "11111111"
    };

    [Theory, RoleNamesData(except: ApiRoles.SetProfessionalStatus)]
    public async Task Put_UserDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var person = await TestData.CreatePersonAsync();
        var reference = Guid.NewGuid().ToString();

        var response = await GetHttpClientWithApiKey().PutAsync(
            $"/v3/persons/{person.Trn}/routes-to-professional-statuses/{reference}",
            CreateJsonContent(CreateRequestBody()));

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_PersonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var reference = Guid.NewGuid().ToString();

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"/v3/persons/0000000/routes-to-professional-statuses/{reference}",
            CreateJsonContent(CreateRequestBody()));

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PersonNotFound, StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Put_ValidRequestForNewRoute_ReturnsNoContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var reference = Guid.NewGuid().ToString();

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync(
            $"/v3/persons/{person.Trn}/routes-to-professional-statuses/{reference}",
            CreateJsonContent(CreateRequestBody()));

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var route = await dbContext.RouteToProfessionalStatuses
                .SingleOrDefaultAsync(r => r.PersonId == person.PersonId);
            Assert.NotNull(route);
        });
    }
}
