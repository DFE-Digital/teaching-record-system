namespace TeachingRecordSystem.Api.Tests.V3.V20250327;

public class GetPersonByTrnTests : TestBase
{
    public GetPersonByTrnTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient(roles: [ApiRoles.GetPerson]);
    }

    [Fact]
    public async Task Get_PersonWithQtlsAndQtsViaAnotherRoute_ReturnsExpectedAwardedOrApprovedCount()
    {
        // Arrange


        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithQtlsDate(Clock.Today));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var responseJson = await AssertEx.JsonResponseAsync(response);
        var awardedOrApprovedCount = responseJson.RootElement.GetProperty("qts").GetProperty("awardedOrApprovedCount").GetInt32();
        Assert.Equal(2, awardedOrApprovedCount);
    }
}
