namespace TeachingRecordSystem.Api.IntegrationTests.V3.VNext;

public class GetPersonByTrnTests : TestBase
{
    public GetPersonByTrnTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient(roles: [ApiRoles.GetPerson]);
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
    }

    [Fact]
    public async Task Get_WithAllIncludes_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithEyts()
            .WithQtls()
            .WithMandatoryQualification()
            .WithAlert()
            .WithInductionStatus(InductionStatus.Exempt));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons/{person.Trn}?include=Induction,RoutesToProfessionalStatuses,MandatoryQualifications,PendingDetailChanges,Alerts,PreviousNames");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseAsync(response, StatusCodes.Status200OK);
    }
}
