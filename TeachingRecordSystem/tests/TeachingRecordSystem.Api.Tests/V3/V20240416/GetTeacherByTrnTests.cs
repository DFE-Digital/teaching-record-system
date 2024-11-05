namespace TeachingRecordSystem.Api.Tests.V3.V20240416;

public class GetTeacherByTrnTests : TestBase
{
    public GetTeacherByTrnTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.GetPerson]);
    }

    [Fact]
    public async Task Get_DateOfBirthDoesNotMatchTeachingRecord_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePerson(p => p.WithTrn());
        var dateOfBirth = person.DateOfBirth.AddDays(1);

        var httpClient = GetHttpClientWithApiKey();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{person.Trn}?dateOfBirth={dateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_DateOfBirthMatchesTeachingRecord_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson(p => p.WithTrn());

        var httpClient = GetHttpClientWithApiKey();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{person.Trn}?dateOfBirth={person.DateOfBirth:yyyy-MM-dd}");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_DateOfBirthNotProvided_ReturnsOk()
    {
        // Arrange
        var person = await TestData.CreatePerson(p => p.WithTrn());

        var httpClient = GetHttpClientWithApiKey();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/teachers/{person.Trn}");

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }
}
